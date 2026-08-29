using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WandEnhancer.Core.Services;

namespace WandEnhancer.View
{
    public partial class LicenseWindow : Window
    {
        public bool IsAuthenticated { get; private set; }
        private bool _keyVisible = false;

        public LicenseWindow()
        {
            InitializeComponent();
            TxtUsername.Focus();
            TxtLicenseKey.KeyDown += Input_KeyDown;
            TxtLicenseKeyVisible.KeyDown += Input_KeyDown;
            TxtUsername.KeyDown += Input_KeyDown;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BtnActivate_Click(sender, new RoutedEventArgs());
        }

        private async void BtnActivate_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string licenseKey = _keyVisible
                ? TxtLicenseKeyVisible.Text.Trim()
                : TxtLicenseKey.Password.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(licenseKey))
            {
                ShowStatus("Please fill in both fields.", true);
                return;
            }

            BtnActivate.IsEnabled = false;
            BtnActivateText.Text = "Verifying...";
            HideStatus();

            var result = await System.Threading.Tasks.Task.Run(() =>
                LicenseService.LoginAsync(username, licenseKey));

            BtnActivate.IsEnabled = true;
            BtnActivateText.Text = "ACTIVATE";

            if (result.Success)
            {
                IsAuthenticated = true;
                DialogResult = true;
                Close();
            }
            else
            {
                ShowStatus(result.Message, true);
                ShakeWindow();
            }
        }

        private void ShowStatus(string message, bool isError)
        {
            TxtStatus.Text = message;
            TxtStatus.Foreground = new SolidColorBrush(
                isError ? Color.FromRgb(0xFF, 0x3B, 0x30)
                        : Color.FromRgb(0x1A, 0x6B, 0x3A));
            TxtStatus.Visibility = Visibility.Visible;
        }

        private void HideStatus()
        {
            TxtStatus.Visibility = Visibility.Collapsed;
        }

        private void ShakeWindow()
        {
            double origin = Left;
            var anim = new DoubleAnimationUsingKeyFrames();
            anim.KeyFrames.Add(new LinearDoubleKeyFrame(origin - 8, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(50))));
            anim.KeyFrames.Add(new LinearDoubleKeyFrame(origin + 8, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
            anim.KeyFrames.Add(new LinearDoubleKeyFrame(origin - 6, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150))));
            anim.KeyFrames.Add(new LinearDoubleKeyFrame(origin + 6, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
            anim.KeyFrames.Add(new LinearDoubleKeyFrame(origin, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(250))));
            BeginAnimation(LeftProperty, anim);
        }
    }
}
