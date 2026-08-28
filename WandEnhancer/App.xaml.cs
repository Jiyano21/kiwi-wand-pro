// Licensed under the Apache License, Version 2.0
// Original author: k1tbyte (https://github.com/k1tbyte/Wand-Enhancer)
// Modified by: charlotte-zee

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WandEnhancer.Core;
using WandEnhancer.Core.Services;
using WandEnhancer.View;
using WandEnhancer.View.MainWindow;

namespace WandEnhancer
{
    public partial class App : Application
    {
        private static Mutex _mutex;

        protected override async void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, "KiwiWandPro-SingleInstance", out bool isNewInstance);
            if (!isNewInstance)
            {
                MessageBox.Show("Kiwi Wand Pro is already running.", "Kiwi Wand Pro", MessageBoxButton.OK, MessageBoxImage.Information);
                Current.Shutdown();
                return;
            }

            LocalizationManager.Initialize();

            bool hasCachedSession = LicenseService.TryLoadCacheInstant();

            if (!hasCachedSession)
            {
                var licenseWindow = new LicenseWindow();
                bool? result = licenseWindow.ShowDialog();

                if (result != true || !licenseWindow.IsAuthenticated)
                {
                    Current.Shutdown();
                    return;
                }
            }

            if (hasCachedSession)
            {
                var validation = await LicenseService.ValidateOnStartupAsync();

                if (!validation.Valid)
                {
                    string msg = validation.Message;
                    if (string.IsNullOrEmpty(msg))
                        msg = "Your license is no longer valid.";

                    MessageBox.Show(msg + "\n\nYou will need to re-enter your license key.",
                        "Kiwi Wand Pro", MessageBoxButton.OK, MessageBoxImage.Warning);

                    LicenseService.ClearCache();
                    var licenseWindow = new LicenseWindow();
                    bool? result = licenseWindow.ShowDialog();

                    if (result != true || !licenseWindow.IsAuthenticated)
                    {
                        Current.Shutdown();
                        return;
                    }
                }
            }

            if (!DisclaimerWindow.IsAccepted())
            {
                var disclaimerWindow = new DisclaimerWindow();
                bool? result = disclaimerWindow.ShowDialog();

                if (result != true)
                {
                    Current.Shutdown();
                    return;
                }
            }

            this.MainWindow.Show();

            if (hasCachedSession)
            {
                RunBackgroundValidation();
            }
        }

        private async void RunBackgroundValidation()
        {
            await Task.Delay(4000);
            var result = await LicenseService.BackgroundValidateAsync();

            if (!result.Valid)
            {
                MessageBox.Show("Your license has been revoked or expired. The app will now close.",
                    "Kiwi Wand Pro", MessageBoxButton.OK, MessageBoxImage.Warning);
                LicenseService.ClearCache();
                Current.Shutdown();
            }
        }

        public new static void Shutdown()
        {
            Current.Dispatcher.Invoke(() => Current.Shutdown());
        }
    }
}
