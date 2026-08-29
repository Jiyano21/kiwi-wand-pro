using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WandEnhancer.Core.Security;

namespace WandEnhancer.Core.Services
{
    public static class LicenseService
    {
        private static readonly byte[] _obf_ProjectUrl = new byte[] { 0x23, 0x03, 0x1D, 0x07, 0x1A, 0x6A, 0x5D, 0x40, 0x06, 0x02, 0x0F, 0x1D, 0x5C, 0x00, 0x5C, 0x3D, 0x11, 0x19, 0x06, 0x1F, 0x32, 0x0A, 0x04, 0x17, 0x08, 0x10, 0x17, 0x5D, 0x58, 0x41, 0x3E, 0x07, 0x08, 0x15, 0x08, 0x23, 0x17, 0x41, 0x17, 0x0A };
        private static readonly byte[] _obf_AnonKey = new byte[] { 0x2E, 0x0E, 0x23, 0x1F, 0x0B, 0x17, 0x11, 0x06, 0x3B, 0x0C, 0x29, 0x3D, 0x78, 0x0C, 0x7B, 0x7A, 0x39, 0x00, 0x3E, 0x1A, 0x19, 0x1C, 0x3D, 0x41, 0x06, 0x20, 0x3D, 0x1B, 0x3F, 0x59, 0x3B, 0x2F, 0x3F, 0x34, 0x23, 0x69, 0x5C, 0x0A, 0x0D, 0x2F, 0x13, 0x17, 0x1E, 0x3B, 0x5B, 0x04, 0x1E, 0x23, 0x0D, 0x0D, 0x08, 0x30, 0x07, 0x2D, 0x08, 0x25, 0x0E, 0x77, 0x25, 0x7B, 0x38, 0x3E, 0x07, 0x3D, 0x05, 0x0A, 0x1B, 0x26, 0x42, 0x2C, 0x0D, 0x3E, 0x43, 0x14, 0x75, 0x27, 0x0F, 0x0D, 0x1A, 0x5C, 0x62, 0x28, 0x01, 0x36, 0x1D, 0x07, 0x19, 0x67, 0x42, 0x53, 0x79, 0x39, 0x1D, 0x14, 0x5B, 0x1E, 0x05, 0x26, 0x1D, 0x12, 0x0A, 0x17, 0x40, 0x4F, 0x41, 0x11, 0x24, 0x20, 0x41, 0x20, 0x3D, 0x34, 0x1A, 0x16, 0x57, 0x57, 0x1D, 0x61, 0x35, 0x78, 0x3B, 0x2E, 0x31, 0x26, 0x00, 0x1F, 0x18, 0x2A, 0x47, 0x2A, 0x27, 0x3D, 0x1E, 0x38, 0x66, 0x1E, 0x00, 0x27, 0x23, 0x0E, 0x23, 0x3B, 0x02, 0x22, 0x51, 0x00, 0x37, 0x64, 0x40, 0x7F, 0x21, 0x36, 0x5C, 0x38, 0x2D, 0x1D, 0x08, 0x22, 0x20, 0x24, 0x52, 0x3B, 0x65, 0x46, 0x1C, 0x1C, 0x05, 0x5A, 0x24, 0x06, 0x06, 0x36, 0x37, 0x1E, 0x10, 0x50, 0x2D, 0x4B, 0x31, 0x5F, 0x01, 0x1F, 0x5C, 0x02, 0x0D, 0x1C, 0x34, 0x56, 0x02, 0x30, 0x01, 0x44, 0x62, 0x31, 0x47, 0x04, 0x03, 0x5E, 0x5A, 0x04, 0x37, 0x0B, 0x5B, 0x07, 0x0C, 0x04, 0x01, 0x5A };
        private static readonly byte[] _obf_CacheRegKey = new byte[] { 0x18, 0x38, 0x2F, 0x23, 0x3E, 0x11, 0x20, 0x2A, 0x28, 0x2E, 0x0A, 0x03, 0x44, 0x21, 0x53, 0x25, 0x13, 0x39, 0x05, 0x06, 0x0C, 0x3E, 0x06, 0x17, 0x00, 0x0D, 0x07, 0x48 };
        private static readonly byte[] _obf_EntropyLegacy = new byte[] { 0x00, 0x1E, 0x1E, 0x1E, 0x3E, 0x31, 0x1C, 0x0B, 0x24, 0x17, 0x0C, 0x59, 0x68, 0x17, 0x41, 0x32, 0x36, 0x1C, 0x03, 0x01, 0x7D, 0x04, 0x5E };

        private static string ProjectUrl => StringProtector.Decode(_obf_ProjectUrl, _obf_ProjectUrl.Length);
        private static string AnonKey => StringProtector.Decode(_obf_AnonKey, _obf_AnonKey.Length);
        private const string AppVersion = "1.0";
        private static string CacheRegKey => StringProtector.Decode(_obf_CacheRegKey, _obf_CacheRegKey.Length);
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly string[] ProductIds = {
            "00000000-0000-0000-0000-000000000000"
        };

        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

        public static string AuthenticatedUsername { get; private set; }
        public static string LicenseType { get; private set; }
        public static DateTime? ExpiresAt { get; private set; }
        public static bool IsTrial { get; private set; }
        public static int TrialDaysRemaining { get; private set; } = -1;

        private static byte[] GetEntropy()
        {
            string salt = StringProtector.Decode(_obf_EntropyLegacy, _obf_EntropyLegacy.Length);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            byte[] result = DeriveKey(GetHardwareId(), saltBytes, 100000);
            Array.Clear(saltBytes, 0, saltBytes.Length);
            return result;
        }

        private static byte[] GetLegacyEntropy()
        {
            string decoded = StringProtector.Decode(_obf_EntropyLegacy, _obf_EntropyLegacy.Length);
            return Encoding.UTF8.GetBytes(decoded);
        }

        private static byte[] DeriveKey(string password, byte[] salt, int iterations)
        {
            using (var derive = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                return derive.GetBytes(32);
            }
        }

        public static string GetHardwareId()
        {
            try
            {
                string sid = WindowsIdentity.GetCurrent().User.Value;
                using (var sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(sid));
                    var hex = new StringBuilder(hash.Length * 2);
                    for (int i = 0; i < Math.Min(hash.Length, 16); i++)
                        hex.Append(hash[i].ToString("x2"));
                    return hex.ToString();
                }
            }
            catch { }
            try
            {
                string fallbackKey = @"SOFTWARE\KiwiWandPro\HWID";
                using (var key = Registry.CurrentUser.CreateSubKey(fallbackKey))
                {
                    string existing = key.GetValue("id") as string;
                    if (!string.IsNullOrEmpty(existing)) return existing;
                    string newId = Guid.NewGuid().ToString("N");
                    key.SetValue("id", newId);
                    return newId;
                }
            }
            catch { return Guid.NewGuid().ToString("N"); }
        }

        public static string GetWindowsVersion()
        {
            try
            {
                return Environment.OSVersion.VersionString;
            }
            catch { return "Unknown"; }
        }

        public static async System.Threading.Tasks.Task<(bool Success, string Message)> LoginAsync(string username, string licenseKey)
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
                return (false, "License key cannot be empty.");

            try
            {
                string hwid = GetHardwareId();
                string computerName = Environment.MachineName;
                string windowsVersion = GetWindowsVersion();

                var activateResult = await ActivateLicenseAsync(licenseKey, hwid, AppVersion, computerName, windowsVersion);

                if (activateResult == null)
                    return (false, "Can't reach the activation server. Check your internet connection.");

                if (!activateResult.success)
                {
                    string msg = activateResult.message ?? "";
                    string friendly;
                    if (msg == "invalidkey")
                        friendly = "That license key doesn't exist. Double-check and try again.";
                    else if (msg == "bannedkey")
                        friendly = "This license key has been banned.";
                    else if (msg == "suspended")
                        friendly = "This license key has been suspended.";
                    else if (msg == "keyused")
                        friendly = "This key is already activated on another device.";
                    else if (msg == "expired")
                        friendly = "This license key has expired.";
                    else if (msg == "hwid_mismatch")
                        friendly = "This key is already activated on another device. Contact support for a reset.";
                    else if (msg == "device_banned")
                        friendly = "This device has been permanently banned.";
                    else if (msg.IndexOf("hwid", StringComparison.OrdinalIgnoreCase) >= 0)
                        friendly = "This key is already activated on another device. Contact support for a reset.";
                    else if (string.IsNullOrWhiteSpace(msg))
                        friendly = "Activation failed. Check your license key and try again.";
                    else
                        friendly = msg;
                    return (false, friendly);
                }

                long expiryUnix = -1;
                IsTrial = false;
                TrialDaysRemaining = -1;
                LicenseType = activateResult.license_type ?? "";
                ExpiresAt = null;

                if (!string.IsNullOrWhiteSpace(activateResult.expires_at))
                {
                    if (DateTime.TryParse(activateResult.expires_at, System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                        out var expDate))
                    {
                        if ((expDate - DateTime.UtcNow).TotalDays < 365 * 50)
                        {
                            expiryUnix = (long)(expDate.ToUniversalTime() - UnixEpoch).TotalSeconds;
                            ExpiresAt = expDate.ToUniversalTime();
                            int daysLeft = (int)(expDate.ToUniversalTime() - DateTime.UtcNow).TotalDays;
                            IsTrial = LicenseType == "trial";
                            TrialDaysRemaining = Math.Max(daysLeft, 0);
                        }
                    }
                }

                AuthenticatedUsername = !string.IsNullOrWhiteSpace(username) ? username : "User";

                CacheCredentials(AuthenticatedUsername, licenseKey, expiryUnix, LicenseType);

                return (true, activateResult.message ?? "Authenticated.");
            }
            catch (CryptographicException)
            {
                return (false, "License system error. Please try again.");
            }
            catch (HttpRequestException)
            {
                return (false, "Can't reach the activation server. Check your internet connection.");
            }
            catch (TimeoutException)
            {
                return (false, "Connection timed out. Check your internet connection.");
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

        private static async System.Threading.Tasks.Task<ActivateLicenseResponse> ActivateLicenseAsync(
            string licenseKey, string hwid, string appVersion, string computerName, string windowsVersion)
        {
            ActivateLicenseResponse lastResult = null;
            foreach (var productId in ProductIds)
            {
                var body = new
                {
                    p_license_key = licenseKey,
                    p_product_id = productId,
                    p_hwid = hwid,
                    p_app_version = appVersion,
                    p_computer_name = computerName,
                    p_windows_version = windowsVersion
                };

                var result = await RpcAsync<ActivateLicenseResponse>("activate_license", body);
                if (result == null) continue;
                if (result.success) return result;
                if (result.message != null && result.message.IndexOf("not found", StringComparison.OrdinalIgnoreCase) < 0)
                    return result;
                lastResult = result;
            }
            return lastResult;
        }

        private static async System.Threading.Tasks.Task<T> RpcAsync<T>(string functionName, object body)
        {
            try
            {
                string url = $"{ProjectUrl}/rest/v1/rpc/{functionName}";
                string json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
                req.Headers.Add("apikey", AnonKey);
                req.Headers.Add("Authorization", $"Bearer {AnonKey}");

                var resp = await Http.SendAsync(req);
                string bodyStr = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    return default(T);

                return JsonConvert.DeserializeObject<T>(bodyStr);
            }
            catch
            {
                return default(T);
            }
        }

        public static bool TryLoadCacheInstant()
        {
            try
            {
                var cached = LoadCachedCredentials();
                if (string.IsNullOrEmpty(cached.Key))
                    return false;

                LicenseType = cached.LicenseType;
                AuthenticatedUsername = cached.Username;

                if (cached.ExpiryUnix >= 0)
                {
                    var expiryDate = UnixEpoch.AddSeconds(cached.ExpiryUnix).ToUniversalTime();
                    if (DateTime.UtcNow > expiryDate)
                        return false;

                    ExpiresAt = expiryDate;
                    int daysLeft = (int)(expiryDate - DateTime.UtcNow).TotalDays;
                    IsTrial = LicenseType == "trial";
                    TrialDaysRemaining = Math.Max(daysLeft, 0);
                }
                else
                {
                    ExpiresAt = null;
                }

                return true;
            }
            catch { return false; }
        }

        public static async System.Threading.Tasks.Task<(bool Valid, string Message)> BackgroundValidateAsync()
        {
            var cached = LoadCachedCredentials();
            if (string.IsNullOrEmpty(cached.Key)) return (true, "");

            try
            {
                var result = await LoginAsync(cached.Username ?? "", cached.Key);
                if (!result.Success)
                {
                    if (IsNetworkError(result.Message))
                        return (true, "network_error");

                    ClearCache();
                    return (false, result.Message);
                }
                return (true, "");
            }
            catch { return (true, "network_error"); }
        }

        public static async System.Threading.Tasks.Task<(bool Valid, string Message)> ValidateOnStartupAsync()
        {
            var cached = LoadCachedCredentials();
            if (string.IsNullOrEmpty(cached.Key))
                return (false, "");

            try
            {
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    var result = await System.Threading.Tasks.Task.Run(async () =>
                    {
                        return await LoginAsync(cached.Username ?? "", cached.Key);
                    }, cts.Token);

                    if (!result.Success)
                    {
                        if (IsNetworkError(result.Message))
                            return (true, "network_error");

                        ClearCache();
                        return (false, result.Message);
                    }
                    return (true, "");
                }
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                return (true, "network_error");
            }
            catch (AggregateException)
            {
                return (true, "network_error");
            }
            catch { return (true, "network_error"); }
        }

        private static void CacheCredentials(string username, string key, long expiryUnix, string licenseType)
        {
            byte[] entropy = null;
            byte[] uPlain = null;
            byte[] kPlain = null;
            byte[] ePlain = null;
            byte[] tPlain = null;
            byte[] uBytes = null;
            byte[] kBytes = null;
            byte[] eBytes = null;
            byte[] tBytes = null;
            try
            {
                entropy = GetEntropy();
                uPlain = Encoding.UTF8.GetBytes(username);
                kPlain = Encoding.UTF8.GetBytes(key);
                ePlain = Encoding.UTF8.GetBytes(expiryUnix.ToString());
                tPlain = Encoding.UTF8.GetBytes(licenseType);

                uBytes = ProtectedData.Protect(uPlain, entropy, DataProtectionScope.CurrentUser);
                kBytes = ProtectedData.Protect(kPlain, entropy, DataProtectionScope.CurrentUser);
                eBytes = ProtectedData.Protect(ePlain, entropy, DataProtectionScope.CurrentUser);
                tBytes = ProtectedData.Protect(tPlain, entropy, DataProtectionScope.CurrentUser);

                using (var reg = Registry.CurrentUser.CreateSubKey(CacheRegKey))
                {
                    if (reg != null)
                    {
                        reg.SetValue("u", Convert.ToBase64String(uBytes));
                        reg.SetValue("k", Convert.ToBase64String(kBytes));
                        reg.SetValue("e", Convert.ToBase64String(eBytes));
                        reg.SetValue("t", Convert.ToBase64String(tBytes));
                    }
                }
            }
            catch { }
            finally
            {
                if (entropy != null) Array.Clear(entropy, 0, entropy.Length);
                if (uPlain != null) Array.Clear(uPlain, 0, uPlain.Length);
                if (kPlain != null) Array.Clear(kPlain, 0, kPlain.Length);
                if (ePlain != null) Array.Clear(ePlain, 0, ePlain.Length);
                if (tPlain != null) Array.Clear(tPlain, 0, tPlain.Length);
                if (uBytes != null) Array.Clear(uBytes, 0, uBytes.Length);
                if (kBytes != null) Array.Clear(kBytes, 0, kBytes.Length);
                if (eBytes != null) Array.Clear(eBytes, 0, eBytes.Length);
                if (tBytes != null) Array.Clear(tBytes, 0, tBytes.Length);
            }
        }

        private static CachedCredentials LoadCachedCredentials()
        {
            try
            {
                using (var reg = Registry.CurrentUser.OpenSubKey(CacheRegKey))
                {
                    if (reg == null) return new CachedCredentials();

                    string uEnc = reg.GetValue("u") as string;
                    string kEnc = reg.GetValue("k") as string;
                    string eEnc = reg.GetValue("e") as string;
                    string tEnc = reg.GetValue("t") as string;

                    if (string.IsNullOrEmpty(uEnc) || string.IsNullOrEmpty(kEnc))
                        return new CachedCredentials();

                    byte[] newEntropy = null;
                    byte[] legacyEntropy = null;
                    byte[] uDec = null;
                    byte[] kDec = null;
                    byte[] eDec = null;
                    byte[] tDec = null;
                    bool migrated = false;
                    try
                    {
                        newEntropy = GetEntropy();
                        legacyEntropy = GetLegacyEntropy();

                        byte[] entropy = newEntropy;
                        uDec = TryDecrypt(uEnc, entropy);
                        if (uDec == null)
                        {
                            entropy = legacyEntropy;
                            uDec = TryDecrypt(uEnc, entropy);
                            migrated = true;
                        }
                        kDec = TryDecrypt(kEnc, entropy);
                        if (kDec == null) return new CachedCredentials();

                        string user = Encoding.UTF8.GetString(uDec);
                        string key = Encoding.UTF8.GetString(kDec);

                        long expiry = -1;
                        if (!string.IsNullOrEmpty(eEnc))
                        {
                            try
                            {
                                eDec = TryDecrypt(eEnc, entropy);
                                if (eDec != null) long.TryParse(Encoding.UTF8.GetString(eDec), out expiry);
                            }
                            catch { }
                        }

                        string licType = "";
                        if (!string.IsNullOrEmpty(tEnc))
                        {
                            tDec = TryDecrypt(tEnc, entropy);
                            if (tDec != null) licType = Encoding.UTF8.GetString(tDec);
                        }

                        if (migrated)
                        {
                            CacheCredentials(user, key, expiry, licType);
                        }

                        return new CachedCredentials { Username = user, Key = key, ExpiryUnix = expiry, LicenseType = licType };
                    }
                    finally
                    {
                        if (newEntropy != null) Array.Clear(newEntropy, 0, newEntropy.Length);
                        if (legacyEntropy != null) Array.Clear(legacyEntropy, 0, legacyEntropy.Length);
                        if (uDec != null) Array.Clear(uDec, 0, uDec.Length);
                        if (kDec != null) Array.Clear(kDec, 0, kDec.Length);
                        if (eDec != null) Array.Clear(eDec, 0, eDec.Length);
                        if (tDec != null) Array.Clear(tDec, 0, tDec.Length);
                    }
                }
            }
            catch { return new CachedCredentials(); }
        }

        private static byte[] TryDecrypt(string encryptedBase64, byte[] entropy)
        {
            try
            {
                return ProtectedData.Unprotect(Convert.FromBase64String(encryptedBase64), entropy, DataProtectionScope.CurrentUser);
            }
            catch { return null; }
        }

        public static void ClearCache()
        {
            AuthenticatedUsername = null;
            LicenseType = "";
            IsTrial = false;
            TrialDaysRemaining = -1;
            ExpiresAt = null;
            try { Registry.CurrentUser.DeleteSubKey(CacheRegKey, false); } catch { }
        }

        private static bool IsNetworkError(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return false;
            string lower = msg.ToLowerInvariant();
            return lower.Contains("network") || lower.Contains("timeout") || lower.Contains("connection")
                || lower.Contains("unreachable") || lower.Contains("internet");
        }

        private class CachedCredentials
        {
            public string Username;
            public string Key;
            public long ExpiryUnix = -1;
            public string LicenseType = "";
        }
    }

    public class ActivateLicenseResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public string license_type { get; set; }
        public string expires_at { get; set; }
    }
}
