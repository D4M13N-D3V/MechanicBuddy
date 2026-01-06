using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MechanicBuddy.Management.Api.Infrastructure;

public class CloudflareClient : ICloudflareClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CloudflareClient> _logger;
    private readonly string _baseDomain;
    private readonly string _ingressTarget;
    private readonly bool _proxied;
    private string? _zoneId;

    public CloudflareClient(IConfiguration configuration, ILogger<CloudflareClient> logger, HttpClient? httpClient = null)
    {
        _logger = logger;

        var apiToken = configuration["Cloudflare:ApiToken"]
            ?? throw new InvalidOperationException("Cloudflare:ApiToken is required");

        // Zone ID is optional - we can look it up from the domain name
        _zoneId = configuration["Cloudflare:ZoneId"];

        _baseDomain = configuration["Cloudflare:BaseDomain"] ?? "mechanicbuddy.app";
        _ingressTarget = configuration["Cloudflare:IngressTarget"] ?? "ingress.mechanicbuddy.app";
        _proxied = configuration.GetValue("Cloudflare:Proxied", true);

        _httpClient = httpClient ?? new HttpClient();
        _httpClient.BaseAddress = new Uri("https://api.cloudflare.com/client/v4/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private async Task<string> GetZoneIdAsync()
    {
        if (!string.IsNullOrEmpty(_zoneId))
        {
            return _zoneId;
        }

        // Look up zone ID from domain name
        try
        {
            var response = await _httpClient.GetAsync($"zones?name={Uri.EscapeDataString(_baseDomain)}");
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to look up Cloudflare zone for {_baseDomain}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CloudflareResponse<List<CloudflareZone>>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var zone = result?.Result?.FirstOrDefault();
            if (zone == null)
            {
                throw new InvalidOperationException($"Cloudflare zone not found for {_baseDomain}");
            }

            _zoneId = zone.Id;
            _logger.LogInformation("Resolved Cloudflare zone ID for {Domain}: {ZoneId}", _baseDomain, _zoneId);
            return _zoneId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to look up Cloudflare zone ID for {Domain}", _baseDomain);
            throw;
        }
    }

    public async Task<bool> CreateTenantDnsRecordAsync(string subdomain, string? target = null)
    {
        var recordName = $"{subdomain}.{_baseDomain}";
        var recordTarget = target ?? _ingressTarget;

        try
        {
            var zoneId = await GetZoneIdAsync();

            // Check if record already exists
            var existingRecord = await GetDnsRecordAsync(subdomain);
            if (existingRecord != null)
            {
                _logger.LogInformation("DNS record for {RecordName} already exists, updating", recordName);
                return await UpdateDnsRecordAsync(existingRecord.Id, subdomain, recordTarget);
            }

            // Create new CNAME record
            var request = new CloudflareDnsCreateRequest
            {
                Type = "CNAME",
                Name = subdomain,
                Content = recordTarget,
                Ttl = 1, // Auto TTL when proxied
                Proxied = _proxied
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var response = await _httpClient.PostAsync(
                $"zones/{zoneId}/dns_records",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create DNS record for {RecordName}: {Response}", recordName, responseContent);
                return false;
            }

            var result = JsonSerializer.Deserialize<CloudflareResponse<CloudflareDnsRecord>>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (result?.Success == true)
            {
                _logger.LogInformation("Created DNS CNAME record: {RecordName} -> {Target}", recordName, recordTarget);
                return true;
            }

            _logger.LogError("Cloudflare API returned success=false for {RecordName}: {Errors}",
                recordName, string.Join(", ", result?.Errors?.Select(e => e.Message) ?? Array.Empty<string>()));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating DNS record for {RecordName}", recordName);
            return false;
        }
    }

    public async Task<bool> DeleteTenantDnsRecordAsync(string subdomain)
    {
        var recordName = $"{subdomain}.{_baseDomain}";

        try
        {
            var zoneId = await GetZoneIdAsync();

            var existingRecord = await GetDnsRecordAsync(subdomain);
            if (existingRecord == null)
            {
                _logger.LogWarning("DNS record for {RecordName} not found, nothing to delete", recordName);
                return true;
            }

            var response = await _httpClient.DeleteAsync($"zones/{zoneId}/dns_records/{existingRecord.Id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Deleted DNS record for {RecordName}", recordName);
                return true;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to delete DNS record for {RecordName}: {Response}", recordName, responseContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting DNS record for {RecordName}", recordName);
            return false;
        }
    }

    public async Task<bool> DnsRecordExistsAsync(string subdomain)
    {
        var record = await GetDnsRecordAsync(subdomain);
        return record != null;
    }

    private async Task<CloudflareDnsRecord?> GetDnsRecordAsync(string subdomain)
    {
        var recordName = $"{subdomain}.{_baseDomain}";

        try
        {
            var zoneId = await GetZoneIdAsync();

            var response = await _httpClient.GetAsync(
                $"zones/{zoneId}/dns_records?name={Uri.EscapeDataString(recordName)}&type=CNAME");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CloudflareResponse<List<CloudflareDnsRecord>>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return result?.Result?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking DNS record for {RecordName}", recordName);
            return null;
        }
    }

    private async Task<bool> UpdateDnsRecordAsync(string recordId, string subdomain, string target)
    {
        var recordName = $"{subdomain}.{_baseDomain}";

        var request = new CloudflareDnsCreateRequest
        {
            Type = "CNAME",
            Name = subdomain,
            Content = target,
            Ttl = 1,
            Proxied = _proxied
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var response = await _httpClient.PutAsync(
            $"zones/{_zoneId}/dns_records/{recordId}",
            new StringContent(json, Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Updated DNS record: {RecordName} -> {Target}", recordName, target);
            return true;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogError("Failed to update DNS record for {RecordName}: {Response}", recordName, responseContent);
        return false;
    }
}

// Cloudflare API DTOs
public class CloudflareResponse<T>
{
    public bool Success { get; set; }
    public T? Result { get; set; }
    public List<CloudflareError>? Errors { get; set; }
}

public class CloudflareError
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CloudflareDnsRecord
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool Proxied { get; set; }
    public int Ttl { get; set; }
}

public class CloudflareDnsCreateRequest
{
    public string Type { get; set; } = "CNAME";
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Ttl { get; set; } = 1;
    public bool Proxied { get; set; } = true;
}
