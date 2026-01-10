using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MechanicBuddy.Management.Api.Infrastructure;

public class NpmClient : INpmClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NpmClient> _logger;
    private readonly string _baseUrl;
    private readonly string _email;
    private readonly string _password;
    private readonly string _forwardHost;
    private readonly int _forwardPort;
    private readonly string _forwardScheme;
    private readonly int _wildcardCertificateId;
    private readonly string _baseDomain;
    private string? _token;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public NpmClient(IConfiguration configuration, ILogger<NpmClient> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();

        _baseUrl = configuration["Npm:BaseUrl"] ?? throw new InvalidOperationException("Npm:BaseUrl is required");
        _email = configuration["Npm:Email"] ?? throw new InvalidOperationException("Npm:Email is required");
        _password = configuration["Npm:Password"] ?? throw new InvalidOperationException("Npm:Password is required");
        _forwardHost = configuration["Npm:ForwardHost"] ?? "192.168.1.101";
        _forwardPort = int.TryParse(configuration["Npm:ForwardPort"], out var port) ? port : 443;
        _forwardScheme = configuration["Npm:ForwardScheme"] ?? "https";
        _wildcardCertificateId = int.TryParse(configuration["Npm:WildcardCertificateId"], out var certId) ? certId : 0;
        _baseDomain = configuration["Cloudflare:BaseDomain"] ?? "mechanicbuddy.app";
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (_token != null && DateTime.UtcNow < _tokenExpiry)
            return;

        var loginPayload = new { identity = _email, secret = _password };
        var content = new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/tokens", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("NPM authentication failed: {Error}", error);
            throw new Exception($"NPM authentication failed: {error}");
        }

        var result = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<NpmTokenResponse>(result);

        _token = tokenResponse?.Token;
        _tokenExpiry = DateTime.UtcNow.AddHours(1); // NPM tokens typically last longer, but refresh hourly

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        _logger.LogInformation("Successfully authenticated with NPM");
    }

    public async Task<bool> CreateProxyHostAsync(string tenantId)
    {
        // Use default forward host/port from configuration
        return await CreateProxyHostAsync(tenantId, _forwardHost, _forwardPort);
    }

    public async Task<bool> CreateProxyHostAsync(string tenantId, string forwardHost, int forwardPort)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            var domain = $"{tenantId}.{_baseDomain}";

            // Check if proxy host already exists
            var existingHost = await GetProxyHostByDomainAsync(domain);
            if (existingHost != null)
            {
                _logger.LogInformation("Proxy host for {Domain} already exists with ID {Id}", domain, existingHost.Id);
                return true;
            }

            var proxyHost = new NpmProxyHostCreate
            {
                DomainNames = new[] { domain },
                ForwardScheme = _forwardScheme,
                ForwardHost = forwardHost,
                ForwardPort = forwardPort,
                CertificateId = _wildcardCertificateId > 0 ? _wildcardCertificateId : null,
                SslForced = _wildcardCertificateId > 0,
                Http2Support = true,
                BlockExploits = true,
                AllowWebsocketUpgrade = true,
                AccessListId = 0,
                Meta = new NpmProxyHostMeta { LetsencryptAgree = false, DnsChallenge = false },
                Locations = Array.Empty<object>()
            };

            var content = new StringContent(
                JsonSerializer.Serialize(proxyHost, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/nginx/proxy-hosts", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create NPM proxy host for {Domain}: {Error}", domain, error);
                return false;
            }

            _logger.LogInformation("Created NPM proxy host for {Domain} forwarding to {ForwardHost}:{ForwardPort}", domain, forwardHost, forwardPort);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating NPM proxy host for tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> DeleteProxyHostAsync(string tenantId)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            var domain = $"{tenantId}.{_baseDomain}";
            var existingHost = await GetProxyHostByDomainAsync(domain);

            if (existingHost == null)
            {
                _logger.LogWarning("Proxy host for {Domain} not found, nothing to delete", domain);
                return true;
            }

            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/nginx/proxy-hosts/{existingHost.Id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete NPM proxy host for {Domain}: {Error}", domain, error);
                return false;
            }

            _logger.LogInformation("Deleted NPM proxy host for {Domain}", domain);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting NPM proxy host for tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> CreateCustomDomainProxyHostAsync(string tenantId, string customDomain)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            // Check if proxy host already exists
            var existingHost = await GetProxyHostByDomainAsync(customDomain);
            if (existingHost != null)
            {
                _logger.LogInformation("Proxy host for custom domain {Domain} already exists with ID {Id}, checking SSL", customDomain, existingHost.Id);

                // If existing host doesn't have SSL, try to add it
                if (existingHost.CertificateId == null || existingHost.CertificateId == 0)
                {
                    return await UpdateProxyHostWithSslAsync(existingHost.Id, customDomain);
                }
                return true;
            }

            // First, request a Let's Encrypt certificate for the custom domain
            var certificateId = await RequestLetsEncryptCertificateAsync(customDomain);
            if (certificateId == null)
            {
                _logger.LogWarning("Could not obtain Let's Encrypt certificate for {Domain}, creating proxy without SSL", customDomain);
            }

            var proxyHost = new NpmProxyHostCreate
            {
                DomainNames = new[] { customDomain },
                ForwardScheme = "http",
                ForwardHost = _forwardHost,
                ForwardPort = _forwardPort,
                CertificateId = certificateId,
                SslForced = certificateId.HasValue,
                Http2Support = true,
                BlockExploits = true,
                AllowWebsocketUpgrade = true,
                AccessListId = 0,
                Meta = new NpmProxyHostMeta { LetsencryptAgree = false, DnsChallenge = false },
                Locations = Array.Empty<object>()
            };

            var content = new StringContent(
                JsonSerializer.Serialize(proxyHost, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/nginx/proxy-hosts", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create NPM proxy host for custom domain {Domain}: {Error}", customDomain, error);
                return false;
            }

            _logger.LogInformation("Created NPM proxy host for custom domain {Domain} (tenant {TenantId})", customDomain, tenantId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating NPM proxy host for custom domain {Domain}", customDomain);
            return false;
        }
    }

    private async Task<bool> UpdateProxyHostWithSslAsync(int proxyHostId, string domain)
    {
        try
        {
            _logger.LogInformation("Updating proxy host {Id} for {Domain} with SSL certificate", proxyHostId, domain);

            // Request Let's Encrypt certificate
            var certificateId = await RequestLetsEncryptCertificateAsync(domain);
            if (certificateId == null)
            {
                _logger.LogWarning("Could not obtain Let's Encrypt certificate for {Domain}", domain);
                return false;
            }

            // Update the proxy host with the certificate
            var updatePayload = new
            {
                certificate_id = certificateId,
                ssl_forced = true,
                http2_support = true
            };

            var content = new StringContent(
                JsonSerializer.Serialize(updatePayload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/nginx/proxy-hosts/{proxyHostId}", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update proxy host {Id} with SSL: {Error}", proxyHostId, error);
                return false;
            }

            _logger.LogInformation("Updated proxy host {Id} for {Domain} with SSL certificate {CertId}", proxyHostId, domain, certificateId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating proxy host {Id} with SSL", proxyHostId);
            return false;
        }
    }

    public async Task<bool> DeleteCustomDomainProxyHostAsync(string customDomain)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            var existingHost = await GetProxyHostByDomainAsync(customDomain);

            if (existingHost == null)
            {
                _logger.LogWarning("Proxy host for custom domain {Domain} not found, nothing to delete", customDomain);
                return true;
            }

            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/nginx/proxy-hosts/{existingHost.Id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete NPM proxy host for custom domain {Domain}: {Error}", customDomain, error);
                return false;
            }

            _logger.LogInformation("Deleted NPM proxy host for custom domain {Domain}", customDomain);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting NPM proxy host for custom domain {Domain}", customDomain);
            return false;
        }
    }

    private async Task<int?> RequestLetsEncryptCertificateAsync(string domain)
    {
        try
        {
            // NPM's Let's Encrypt certificate request requires empty meta object for HTTP challenge
            var certRequest = new
            {
                domain_names = new[] { domain },
                meta = new { },
                provider = "letsencrypt"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(certRequest),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("Requesting Let's Encrypt certificate for {Domain}", domain);

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/nginx/certificates", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to request Let's Encrypt certificate for {Domain}: {Error}", domain, error);
                return null;
            }

            var result = await response.Content.ReadAsStringAsync();
            var certResponse = JsonSerializer.Deserialize<NpmCertificateResponse>(result);

            _logger.LogInformation("Obtained Let's Encrypt certificate for {Domain} with ID {Id}", domain, certResponse?.Id);
            return certResponse?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting Let's Encrypt certificate for {Domain}", domain);
            return null;
        }
    }

    private async Task<NpmProxyHost?> GetProxyHostByDomainAsync(string domain)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/nginx/proxy-hosts");

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var hosts = JsonSerializer.Deserialize<NpmProxyHost[]>(content);

        return hosts?.FirstOrDefault(h => h.DomainNames?.Contains(domain) == true);
    }

    private class NpmTokenResponse
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }

    private class NpmProxyHost
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("domain_names")]
        public string[]? DomainNames { get; set; }

        [JsonPropertyName("certificate_id")]
        public int? CertificateId { get; set; }
    }

    private class NpmProxyHostCreate
    {
        [JsonPropertyName("domain_names")]
        public string[] DomainNames { get; set; } = Array.Empty<string>();

        [JsonPropertyName("forward_scheme")]
        public string ForwardScheme { get; set; } = "http";

        [JsonPropertyName("forward_host")]
        public string ForwardHost { get; set; } = "";

        [JsonPropertyName("forward_port")]
        public int ForwardPort { get; set; }

        [JsonPropertyName("certificate_id")]
        public int? CertificateId { get; set; }

        [JsonPropertyName("ssl_forced")]
        public bool SslForced { get; set; }

        [JsonPropertyName("http2_support")]
        public bool Http2Support { get; set; }

        [JsonPropertyName("block_exploits")]
        public bool BlockExploits { get; set; }

        [JsonPropertyName("allow_websocket_upgrade")]
        public bool AllowWebsocketUpgrade { get; set; }

        [JsonPropertyName("access_list_id")]
        public int AccessListId { get; set; }

        [JsonPropertyName("meta")]
        public NpmProxyHostMeta Meta { get; set; } = new();

        [JsonPropertyName("locations")]
        public object[] Locations { get; set; } = Array.Empty<object>();
    }

    private class NpmProxyHostMeta
    {
        [JsonPropertyName("letsencrypt_agree")]
        public bool LetsencryptAgree { get; set; }

        [JsonPropertyName("dns_challenge")]
        public bool DnsChallenge { get; set; }
    }

    private class NpmCertificateResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}
