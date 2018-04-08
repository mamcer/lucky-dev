using System.Windows;

namespace LuckyDev
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                this.Properties["ArgName"] = e.Args[0];
            }

            base.OnStartup(e);
        }
    }
}
