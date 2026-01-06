using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Repositories;
using System.Security.Cryptography;
using System.Text;
using DnsClient;

namespace MechanicBuddy.Management.Api.Services;

public class DomainService
{
    private readonly IDomainVerificationRepository _domainVerificationRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IKubernetesClientService _kubernetesClient;
    private readonly ILogger<DomainService> _logger;
    private readonly ILookupClient _dnsClient;
    private readonly string _namespacePrefix;

    public DomainService(
        IDomainVerificationRepository domainVerificationRepository,
        ITenantRepository tenantRepository,
        IKubernetesClientService kubernetesClient,
        ILogger<DomainService> logger,
        IConfiguration configuration)
    {
        _domainVerificationRepository = domainVerificationRepository;
        _tenantRepository = tenantRepository;
        _kubernetesClient = kubernetesClient;
        _logger = logger;
        _dnsClient = new LookupClient();
        _namespacePrefix = configuration.GetValue<string>("Provisioning:NamespacePrefix") ?? "mechanicbuddy-";
    }

    public async Task<DomainVerification> InitiateDomainVerificationAsync(int tenantId, string domain, string method = "dns")
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException("Tenant not found");
        }

        // Check if domain is already in use
        var existingTenant = await _tenantRepository.GetByCustomDomainAsync(domain);
        if (existingTenant != null && existingTenant.Id != tenantId)
        {
            throw new InvalidOperationException("Domain is already in use by another tenant");
        }

        // Generate verification token
        var token = GenerateVerificationToken();

        var verification = new DomainVerification
        {
            TenantId = tenantId,
            Domain = domain,
            VerificationToken = token,
            VerificationMethod = method,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var id = await _domainVerificationRepository.CreateAsync(verification);
        verification.Id = id;

        _logger.LogInformation("Initiated domain verification for {Domain} (tenant {TenantId})", domain, tenantId);

        return verification;
    }

    public async Task<bool> VerifyDomainAsync(string domain)
    {
        var verification = await _domainVerificationRepository.GetByDomainAsync(domain);
        if (verification == null || verification.IsVerified)
        {
            return false;
        }

        if (verification.ExpiresAt.HasValue && verification.ExpiresAt.Value < DateTime.UtcNow)
        {
            _logger.LogWarning("Domain verification for {Domain} has expired", domain);
            return false;
        }

        bool isVerified = verification.VerificationMethod switch
        {
            "dns" => await VerifyDnsTxtRecordAsync(domain, verification.VerificationToken),
            "file" => await VerifyFileAsync(domain, verification.VerificationToken),
            _ => false
        };

        if (isVerified)
        {
            verification.IsVerified = true;
            verification.VerifiedAt = DateTime.UtcNow;
            await _domainVerificationRepository.UpdateAsync(verification);

            // Update tenant
            var tenant = await _tenantRepository.GetByIdAsync(verification.TenantId);
            if (tenant != null)
            {
                tenant.CustomDomain = domain;
                tenant.DomainVerified = true;
                await _tenantRepository.UpdateAsync(tenant);

                // Update Kubernetes Ingress with custom domain
                await UpdateTenantIngressAsync(tenant.TenantId, domain);
            }

            _logger.LogInformation("Successfully verified domain {Domain} for tenant {TenantId}", domain, verification.TenantId);
        }

        return isVerified;
    }

    public async Task<DomainVerification?> GetVerificationStatusAsync(string domain)
    {
        return await _domainVerificationRepository.GetByDomainAsync(domain);
    }

    public async Task<bool> RemoveDomainAsync(int tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
        {
            return false;
        }

        tenant.CustomDomain = null;
        tenant.DomainVerified = false;
        return await _tenantRepository.UpdateAsync(tenant);
    }

    private static string GenerateVerificationToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(randomBytes).Replace("+", "").Replace("/", "").Replace("=", "")[..32];
    }

    private async Task<bool> VerifyDnsTxtRecordAsync(string domain, string expectedToken)
    {
        try
        {
            _logger.LogInformation("Checking DNS TXT record for {Domain}", domain);

            // Query for TXT record at _mechanicbuddy-verify.{domain}
            var verificationHost = $"_mechanicbuddy-verify.{domain}";

            var result = await _dnsClient.QueryAsync(verificationHost, DnsClient.QueryType.TXT);

            if (result.HasError)
            {
                _logger.LogWarning("DNS query failed for {Host}: {Error}", verificationHost, result.ErrorMessage);
                return false;
            }

            // Check if any TXT record matches our expected token
            var txtRecords = result.Answers.TxtRecords();
            foreach (var txtRecord in txtRecords)
            {
                foreach (var txtValue in txtRecord.Text)
                {
                    _logger.LogDebug("Found TXT record: {Value}", txtValue);

                    // Check if the value matches the expected token
                    if (txtValue == expectedToken || txtValue == $"mechanicbuddy-verification={expectedToken}")
                    {
                        _logger.LogInformation("DNS verification successful for {Domain}", domain);
                        return true;
                    }
                }
            }

            _logger.LogWarning("No matching TXT record found for {Domain}. Expected token: {Token}", domain, expectedToken);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify DNS record for {Domain}", domain);
            return false;
        }
    }

    private async Task<bool> UpdateTenantIngressAsync(string tenantId, string customDomain)
    {
        try
        {
            _logger.LogInformation("Updating Ingress for tenant {TenantId} with custom domain {Domain}", tenantId, customDomain);

            var namespace_ = $"{_namespacePrefix}{tenantId}";

            // Get existing ingresses in the namespace
            var ingresses = await _kubernetesClient.GetIngressesAsync(namespace_);

            if (ingresses == null || !ingresses.Any())
            {
                _logger.LogWarning("No Ingress found for tenant {TenantId} in namespace {Namespace}", tenantId, namespace_);
                return false;
            }

            // Find the main tenant ingress (usually named "mechanicbuddy" or similar)
            var ingress = ingresses.FirstOrDefault(i =>
                i.Metadata.Name.Contains("mechanicbuddy") ||
                i.Metadata.Name.Contains("tenant") ||
                i.Metadata.Name == tenantId);

            if (ingress == null)
            {
                _logger.LogWarning("Could not identify main Ingress for tenant {TenantId}", tenantId);
                ingress = ingresses.First(); // Fallback to first ingress
            }

            var ingressName = ingress.Metadata.Name;

            // Get all domains (default + custom)
            var domains = new List<string>();

            // Add existing default domain
            var existingHosts = ingress.Spec.Rules?.Select(r => r.Host).Where(h => !string.IsNullOrEmpty(h)).ToList();
            if (existingHosts != null && existingHosts.Any())
            {
                domains.AddRange(existingHosts);
            }

            // Add custom domain if not already present
            if (!domains.Contains(customDomain))
            {
                domains.Add(customDomain);
            }

            // Update the Ingress with all domains
            var success = await _kubernetesClient.UpdateIngressDomainsAsync(
                namespace_,
                ingressName,
                domains,
                clusterIssuer: "letsencrypt-prod");

            if (success)
            {
                _logger.LogInformation("Successfully updated Ingress for tenant {TenantId} with custom domain {Domain}",
                    tenantId, customDomain);
            }
            else
            {
                _logger.LogError("Failed to update Ingress for tenant {TenantId}", tenantId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Ingress for tenant {TenantId}", tenantId);
            return false;
        }
    }

    private async Task<bool> VerifyFileAsync(string domain, string expectedToken)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var url = $"https://{domain}/.well-known/mechanicbuddy-verification.txt";

            var response = await httpClient.GetStringAsync(url);
            var actualToken = response.Trim();

            return actualToken == expectedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify file for {Domain}", domain);
            return false;
        }
    }
}
