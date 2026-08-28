// Licensed under the Apache License, Version 2.0
// Original author: k1tbyte (https://github.com/k1tbyte/Wand-Enhancer)
// Modified by: charlotte-zee

using System.Windows;
using WandEnhancer.Core.Services;
using WandEnhancer.View;

namespace WandEnhancer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LocalizationManager.Initialize();
            this.MainWindow.Show();
        }
    }
}
