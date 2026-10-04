using System.Windows;

namespace rans0m
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show(args.Exception.ToString(), "Unhandled Error");
                args.Handled = true;
            };
            base.OnStartup(e);
        }
    }

}
