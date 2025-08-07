using Microsoft.Extensions.Configuration;
using PlatformCoreLib.Models;
using SIUtil;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace PlatformCoreLib.Configuration
{
    public class ConfigurationManager
    {
        private static ConfigurationManager _instance;
        private static readonly object _lock = new object();

        private readonly Dictionary<string, string> _configCache = new();
        private readonly Dictionary<string, string> _localConfigValues = new();
        private readonly HttpClient _httpClient;
        private IConfiguration _localConfig;
        private bool _initialized = false;

        private ConfigurationManager()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(5) };
        }

        public static ConfigurationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigurationManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public void Initialize()
        {
            if (_initialized) return;

            lock (_lock)
            {
                if (_initialized) return;

                Logger.LogMessage("Initializing configuration system...");
                LoadLocalConfiguration();
                LoadEnvironmentVariables();
                FetchConfigServerData();
                FetchVaultSecrets();
                FetchCyberArkCredentials();
                ApplyLocalConfigOverrides();
                ApplyCyberArkOverrides();
                _initialized = true;
                Logger.LogMessage("Configuration initialization complete");
            }
        }

        private void LoadLocalConfiguration()
        {
            if (!OperatingSystem.IsLinux())
            {
                try
                {
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

                    _localConfig = builder.Build();

                    foreach (var section in _localConfig.GetChildren())
                    {
                        LoadConfigSection(section, section.Key, true);
                    }

                    Logger.LogMessage("Loaded local configuration from appsettings.json");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to load local configuration");
                }
            }            
        }

        private void LoadConfigSection(IConfigurationSection section, string prefix, bool isLocalConfig = false)
        {
            if (section.Value != null)
            {
                if (isLocalConfig)
                {
                    _localConfigValues[prefix] = section.Value;
                }
                _configCache[prefix] = section.Value;
                return;
            }

            foreach (var child in section.GetChildren())
            {
                var key = string.IsNullOrEmpty(prefix) ? child.Key : $"{prefix}.{child.Key}";
                LoadConfigSection(child, key, isLocalConfig);
            }
        }

        private void LoadEnvironmentVariables()
        {
            if (!OperatingSystem.IsLinux())
            {
                SetEnvironmentVariable("SPRING_CLOUD_CONFIG_URI",GetValue("SPRING_CLOUD_CONFIG_URI"));
                SetEnvironmentVariable("SPRING_PROFILES_ACTIVE",GetValue("SPRING_PROFILES_ACTIVE"));
                SetEnvironmentVariable("SPRING_CLOUD_VAULT_HOST",GetValue("SPRING_CLOUD_VAULT_HOST"));
                SetEnvironmentVariable("ARTIFACT_ID", GetValue("ARTIFACT_ID"));
            }

            foreach (var env in Environment.GetEnvironmentVariables())
            {
                var entry = (System.Collections.DictionaryEntry)env;
                var key = entry.Key.ToString();
                var value = entry.Value.ToString();
                if (!_localConfigValues.ContainsKey(key))
                {
                    _configCache[key] = value;
                }
            }
        }

        private void SetEnvironmentVariable(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Environment.SetEnvironmentVariable(key, value);
                Logger.LogMessage($"Set environment variable: {key} = {value}");
            }            
        }

        private void FetchConfigServerData()
        {
            try
            {
                var baseUrl = GetValue("SPRING_CLOUD_CONFIG_URI");
                var artifactId = GetValue("ARTIFACT_ID");
                var profiles = GetValue("SPRING_PROFILES_ACTIVE");

                if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(artifactId))
                {
                    Logger.LogWarning("Config server environment variables not set");
                    return;
                }

                var endpoint = $"{baseUrl}/{artifactId}/{profiles}";
                Logger.LogDebug($"Fetching configuration from {endpoint}");
                string response = _httpClient.GetStringAsync(endpoint).GetAwaiter().GetResult();
                var configResponse = JsonSerializer.Deserialize<ConfigResponse>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (configResponse?.PropertySources != null)
                {
                    foreach (var source in configResponse.PropertySources)
                    {
                        if (source?.Source != null)
                        {
                            foreach (var kvp in source.Source)
                            {
                                _configCache[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
                            }
                        }
                    }
                }

                Logger.LogInformation("Loaded configuration from Config Server");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to fetch configuration from Config Server");
            }
        }

        private void FetchVaultSecrets()
        {
            try
            {
                var tokenUrl = GetValue("PlatformCore.Vault.TokenUrl");
                var platformUrl = GetValue("PlatformCore.Vault.PlatformUrl");
                var vaultId = GetValue("PlatformCore.Vault.VaultID");

                if (string.IsNullOrEmpty(tokenUrl) || string.IsNullOrEmpty(platformUrl))
                {
                    Logger.LogWarning("Vault configuration not found");
                    return;
                }
                string token = _httpClient.GetStringAsync(tokenUrl).GetAwaiter().GetResult();
                if (string.IsNullOrEmpty(token))
                {
                    Logger.LogWarning("Failed to get Vault token");
                    return;
                }
                var secretEndpoint = $"{platformUrl}/{vaultId}";
                var request = new HttpRequestMessage(HttpMethod.Get, secretEndpoint);
                request.Headers.Add("X-Vault-Token", token);

                HttpResponseMessage response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                var content = response.Content.ReadAsStringAsync().Result;                

                string naMail1Cert = null;
                string tarwebsCert = null;
                try
                {
                    using (var doc = JsonDocument.Parse(content))
                    {
                        var root = doc.RootElement;
                        JsonElement dataElement;
                        if (root.TryGetProperty("data", out dataElement))
                        {
                            root = dataElement;
                        }
                        foreach(var property in root.EnumerateObject())
                        {
                            string value = null;
                            if (property.Value.ValueKind == JsonValueKind.String)
                            {
                                value = property.Value.GetString();
                            }
                            else if (property.Value.ValueKind == JsonValueKind.Number)
                            {
                                value = property.Value.GetRawText();
                            }
                            else if (property.Value.ValueKind == JsonValueKind.True || property.Value.ValueKind == JsonValueKind.False)
                            {
                                value = property.Value.GetBoolean().ToString();
                            }
                            else if (property.Value.ValueKind == JsonValueKind.Object || property.Value.ValueKind == JsonValueKind.Array)
                            {
                                value = property.Value.GetRawText();
                            }
                            if (!string.IsNullOrEmpty(value))
                            {
                                _configCache[$"vault:{property.Name}"] = value;
                                if (property.Name == "na-mail1.crt")
                                    naMail1Cert = value;
                                if (property.Name == "tarwebs.crt")
                                    tarwebsCert = value;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex,"Failed to parse Vault response as JSON");
                    Logger.LogDebug($"Vault response content: {content}");
                    return;
                }

                if (!string.IsNullOrEmpty(naMail1Cert) || !string.IsNullOrEmpty(tarwebsCert))
                {
                    InstallCertificates(naMail1Cert, tarwebsCert);
                }

                Logger.LogInformation("Loaded secrets from Vault");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to fetch secrets from Vault");
            }
        }

        private void FetchCyberArkCredentials()
        {
            try
            {
                var baseUrl = GetVaultValue("PlatformCore.CyberArk.BaseUrl");
                var appId = GetVaultValue("PlatformCore.CyberArk.AppId");
                var safe = GetVaultValue("PlatformCore.CyberArk.Safe");
                var objectName = GetVaultValue("PlatformCore.CyberArk.ObjectName");

                if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(appId))
                {
                    Logger.LogWarning("CyberArk configuration not found");
                    return;
                }

                var uri = $"{baseUrl}?AppID={Uri.EscapeDataString(appId)}&Safe={Uri.EscapeDataString(safe)}&Object={Uri.EscapeDataString(objectName)}";

                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add("Accept", "application/json");

                HttpResponseMessage response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                var content = response.Content.ReadAsStringAsync().Result;
                var cyberArkData = JsonSerializer.Deserialize<CyberArkResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (cyberArkData != null)
                {
                    if (!string.IsNullOrEmpty(cyberArkData.Content))
                        _configCache["_cyberark_api_password"] = cyberArkData.Content;
                    if (!string.IsNullOrEmpty(cyberArkData.UserName))
                        _configCache["_cyberark_api_username"] = cyberArkData.UserName;
                    if (!string.IsNullOrEmpty(cyberArkData.Address))
                        _configCache["CyberArkDomain"] = cyberArkData.Address;
                }

                Logger.LogInformation("Loaded credentials from CyberArk");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to fetch credentials from CyberArk");
            }
        }
        private void ApplyLocalConfigOverrides()
        {
            if (_localConfigValues.Count > 0)
            {
                foreach (var kvp in _localConfigValues)
                {
                    _configCache[kvp.Key] = kvp.Value;
                    Logger.LogDebug($"Local config override applied for key: {kvp.Key}");
                }

                Logger.LogInformation("Applied local configuration overrides");
            }
        }
        private void ApplyCyberArkOverrides()
        {
            if (_configCache.ContainsKey("CyberArkUserName") && !_localConfigValues.ContainsKey("CyberArkUserName"))
            {
                Logger.LogInformation("Using CyberArkUserName from config server");
            }
            else if (_configCache.ContainsKey("_cyberark_api_username"))
            {
                _configCache["CyberArkUserName"] = _configCache["_cyberark_api_username"];
                Logger.LogInformation("Using CyberArkUserName from CyberArk API");
            }
            if (_configCache.ContainsKey("CyberArkPassword") && !_localConfigValues.ContainsKey("CyberArkPassword"))
            {
                Logger.LogInformation("Using CyberArkPassword from config server");
            }
            else if (_configCache.ContainsKey("_cyberark_api_password"))
            {
                _configCache["CyberArkPassword"] = _configCache["_cyberark_api_password"];
                Logger.LogInformation("Using CyberArkPassword from CyberArk API");
            }
            _configCache.Remove("_cyberark_api_username");
            _configCache.Remove("_cyberark_api_password");
        }
        private void InstallCertificates(string naMail1Cert, string tarwebsCert)
        {
            try
            {
                var certDirectory = "/etc/pki/ca-trust/source/anchors/";
                if (!Directory.Exists(certDirectory))
                {
                    Logger.LogInformation("Creating certificate directory");
                    Directory.CreateDirectory(certDirectory);
                }

                if (!string.IsNullOrEmpty(naMail1Cert))
                {
                    var certPath = Path.Combine(certDirectory, "na-mail1.crt");
                    if (File.Exists(certPath))
                    {
                        Logger.LogInformation("Certificate already installed");
                        return;
                    }
                    File.WriteAllText(certPath, naMail1Cert, Encoding.UTF8);
                    SetFilePermissions(certPath);
                }

                if (!string.IsNullOrEmpty(tarwebsCert))
                {
                    var certPath = Path.Combine(certDirectory, "tarwebs.crt");
                    if (File.Exists(certPath))
                    {
                        Logger.LogInformation("Certificate already installed");
                        return;
                    }
                    File.WriteAllText(certPath, tarwebsCert, Encoding.UTF8);
                    SetFilePermissions(certPath);
                }

                UpdateCaTrust();
                Logger.LogInformation("Certificates installed successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error installing certificates");
            }
        }

        private void SetFilePermissions(string filePath)
        {
            if (OperatingSystem.IsLinux())
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "chmod",
                        Arguments = $"644 {filePath}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                Logger.LogDebug("Setting file permissions");
                process.Start();
                process.WaitForExit();
            }
        }

        private void UpdateCaTrust()
        {
            if (OperatingSystem.IsLinux())
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "update-ca-trust",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                Logger.LogDebug("Updating CA trust");
                process.Start();
                process.WaitForExit();
            }
        }

        public string GetValue(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            lock (_lock)
            {
                if (_configCache.TryGetValue(key, out var value))
                    return value;

                if (!key.Contains("."))
                {
                    var appSettingsKey = $"AppSettings.{key}";
                    if (_configCache.TryGetValue(appSettingsKey, out value))
                        return value;
                }

                return null;
            }
        }

        public string GetConnectionString(string name)
        {
            return GetValue($"ConnectionStrings.{name}");
        }

        public string GetVaultValue(string key)
        {
            return GetValue($"vault:{key}") ?? GetValue(key);
        }

        public string GetPassword(string key)
        {
            if (key == "CyberArkPassword")
                return GetValue("CyberArkPassword");

            return GetValue(key);
        }
        public List<string> GetAllKeys()
        {
            lock (_lock)
            {
                return _configCache.Keys.ToList();
            }
        }
    }
}
