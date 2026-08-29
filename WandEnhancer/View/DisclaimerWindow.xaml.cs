using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Win32;
using WandEnhancer.Core.Security;

namespace WandEnhancer.View
{
    public partial class DisclaimerWindow : Window
    {
        private static readonly string CacheKey = "DisclaimerAccepted";
        private static readonly byte[] _obf_DisclaimerSalt = new byte[] { 0x00, 0x1E, 0x1E, 0x1E, 0x3E, 0x31, 0x1C, 0x0B, 0x24, 0x17, 0x0C, 0x59, 0x69, 0x13, 0x40, 0x22, 0x01, 0x0C, 0x13, 0x44, 0x26, 0x43 };

        public DisclaimerWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (AgreeCheckBox.IsChecked != true)
            {
                ShakeButton();
                return;
            }

            SaveAccepted();
            DialogResult = true;
            Close();
        }

        private void ShakeButton()
        {
            var translateTransform = new System.Windows.Media.TranslateTransform();
            ContinueButton.RenderTransform = translateTransform;

            var animation = new DoubleAnimationUsingKeyFrames
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(400))
            };
            animation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0)));
            animation.KeyFrames.Add(new LinearDoubleKeyFrame(-10, KeyTime.FromPercent(0.1)));
            animation.KeyFrames.Add(new LinearDoubleKeyFrame(10, KeyTime.FromPercent(0.2)));
            animation.KeyFrames.Add(new LinearDoubleKeyFrame(-8, KeyTime.FromPercent(0.3)));
            animation.KeyFrames.Add(new LinearDoubleKeyFrame(8, KeyTime.FromPercent(0.4)));
            animation.KeyFrames.Add(new LinearDoubleKeyFrame(-4, KeyTime.FromPercent(0.5)));
            animation.KeyFrames.Add(new LinearDoubleKeyFrame(4, KeyTime.FromPercent(0.6)));
            animation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1)));

            translateTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, animation);

            AcceptWarningText.Visibility = Visibility.Visible;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
            timer.Tick += (s, args) =>
            {
                AcceptWarningText.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri)
                {
                    UseShellExecute = true
                });
            }
            catch { }
            e.Handled = true;
        }

        public static bool IsAccepted()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\KiwiWandPro\Settings"))
                {
                    if (key == null) return false;
                    string encrypted = key.GetValue(CacheKey) as string;
                    if (string.IsNullOrEmpty(encrypted)) return false;

                    byte[] data = Convert.FromBase64String(encrypted);
                    string machineId = GetMachineId();

                    if (data.Length > 16)
                    {
                        try
                        {
                            byte[] iv = new byte[16];
                            byte[] ciphertext = new byte[data.Length - 16];
                            Buffer.BlockCopy(data, 0, iv, 0, 16);
                            Buffer.BlockCopy(data, 16, ciphertext, 0, ciphertext.Length);

                            byte[] keyBytes = DeriveKey(machineId);
                            using (var aes = Aes.Create())
                            {
                                aes.Key = keyBytes;
                                aes.IV = iv;
                                aes.Mode = CipherMode.CBC;
                                aes.Padding = PaddingMode.PKCS7;
                                using (var decryptor = aes.CreateDecryptor())
                                {
                                    byte[] decrypted = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
                                    bool accepted = Encoding.UTF8.GetString(decrypted) == "accepted";
                                    Array.Clear(decrypted, 0, decrypted.Length);
                                    if (accepted)
                                    {
                                        SaveAccepted();
                                        return true;
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        byte[] legacyKeyBytes = Encoding.UTF8.GetBytes(machineId);
                        using (var aes = Aes.Create())
                        {
                            aes.Key = legacyKeyBytes;
                            aes.IV = new byte[16];
                            aes.Mode = CipherMode.CBC;
                            aes.Padding = PaddingMode.PKCS7;
                            using (var decryptor = aes.CreateDecryptor())
                            {
                                byte[] decrypted = decryptor.TransformFinalBlock(data, 0, data.Length);
                                bool accepted = Encoding.UTF8.GetString(decrypted) == "accepted";
                                Array.Clear(decrypted, 0, decrypted.Length);
                                if (accepted)
                                {
                                    SaveAccepted();
                                    return true;
                                }
                            }
                        }
                    }
                    catch { }

                    return false;
                }
            }
            catch { return false; }
        }

        private static void SaveAccepted()
        {
            try
            {
                byte[] plain = Encoding.UTF8.GetBytes("accepted");
                byte[] keyBytes = DeriveKey(GetMachineId());
                using (var aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.GenerateIV();
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    using (var encryptor = aes.CreateEncryptor())
                    {
                        byte[] encrypted = encryptor.TransformFinalBlock(plain, 0, plain.Length);
                        byte[] result = new byte[16 + encrypted.Length];
                        Buffer.BlockCopy(aes.IV, 0, result, 0, 16);
                        Buffer.BlockCopy(encrypted, 0, result, 16, encrypted.Length);
                        using (var regKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\KiwiWandPro\Settings"))
                        {
                            regKey?.SetValue(CacheKey, Convert.ToBase64String(result));
                        }
                    }
                }
            }
            catch { }
        }

        public static void ClearAccepted()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\KiwiWandPro\Settings", true))
                {
                    key?.DeleteValue(CacheKey, false);
                }
            }
            catch { }
        }

        private static string GetMachineId()
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
            catch { return "default-machine-id"; }
        }

        private static byte[] DeriveKey(string machineId)
        {
            string salt = StringProtector.Decode(_obf_DisclaimerSalt, _obf_DisclaimerSalt.Length);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            using (var derive = new Rfc2898DeriveBytes(machineId, saltBytes, 100000))
            {
                byte[] result = derive.GetBytes(32);
                Array.Clear(saltBytes, 0, saltBytes.Length);
                return result;
            }
        }
    }
}
