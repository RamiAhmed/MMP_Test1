using System;
using System.Linq;
using System.Windows;
using Awesomium.Core;
using System.Collections.Generic;
using Awesomium.Windows.Controls;

namespace MMP_Test1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Shutdown the Core.
            WebCore.Shutdown();
        }
    }
}
