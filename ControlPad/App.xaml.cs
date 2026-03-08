using System.Configuration;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Wpf.Ui.Tray;

namespace ControlPad
{
    public partial class App : Application
    {
        private static Mutex _mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "Slidr";
            bool createdNew;

            _mutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("Slidr is already open.");
                _mutex?.ReleaseMutex();
                _mutex?.Dispose();
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);

            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            bool startHidden = e.Args.Any(a => string.Equals(a, "--hidden", StringComparison.OrdinalIgnoreCase));

            var main = new MainWindow(_mutex);
            Current.MainWindow = main;

            var helper = new WindowInteropHelper(main);
            _ = helper.EnsureHandle();

            main.Show();

            if (startHidden)
                main.Hide();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _mutex?.ReleaseMutex();
                _mutex?.Dispose();
            }
            catch { }

            base.OnExit(e);
        }
    }
}