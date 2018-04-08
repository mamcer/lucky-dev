using System.Windows;

namespace LuckyDev
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                Properties["ArgName"] = e.Args[0];
            }

            base.OnStartup(e);
        }
    }
}