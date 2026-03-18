using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Wpf.Ui.Controls;

namespace ControlPad
{
    public partial class SelectProcessPopup : FluentWindow
    {
        private readonly ObservableCollection<ProcessOption> _processOptions = new();

        public string SelectedProcessName { get; private set; } = string.Empty;
        public string SelectedProcessDisplayName { get; private set; } = string.Empty;

        public SelectProcessPopup()
        {
            InitializeComponent();

            LoadProcesses();
        }

        public void SetInitialSelection(string? processIdentifier)
        {
            if (string.IsNullOrWhiteSpace(processIdentifier))
                return;

            var existing = _processOptions.FirstOrDefault(x =>
                string.Equals(x.Identifier, processIdentifier, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                existing = new ProcessOption
                {
                    Identifier = processIdentifier,
                    DisplayName = processIdentifier.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                                  processIdentifier.Contains('\\') || processIdentifier.Contains('/')
                        ? Path.GetFileName(processIdentifier)
                        : processIdentifier
                };
                _processOptions.Add(existing);
            }

            cb_Processes.SelectedItem = existing;
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (cb_Processes.SelectedItem is not ProcessOption option) return;

            SelectedProcessName = option.Identifier;
            SelectedProcessDisplayName = option.DisplayName;
            DialogResult = true;
        }

        private void btn_AddManual_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Title = "Select executable",
                Filter = "Executable (*.exe)|*.exe|All Files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false,
                DefaultExt = ".exe"
            };

            if (fileDialog.ShowDialog() != true)
                return;

            var existing = _processOptions.FirstOrDefault(x =>
                string.Equals(x.Identifier, fileDialog.FileName, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                existing = new ProcessOption
                {
                    DisplayName = fileDialog.SafeFileName,
                    Identifier = fileDialog.FileName
                };
                _processOptions.Add(existing);
            }

            cb_Processes.SelectedItem = existing;
        }

        private void LoadProcesses()
        {
            var processNames = Process.GetProcesses()
                .Select(p =>
                {
                    try
                    {
                        return p.ProcessName;
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase);

            foreach (var processName in processNames)
            {
                _processOptions.Add(new ProcessOption
                {
                    DisplayName = processName!,
                    Identifier = processName!
                });
            }

            cb_Processes.ItemsSource = _processOptions;
        }

        private sealed class ProcessOption
        {
            public string DisplayName { get; set; } = string.Empty;
            public string Identifier { get; set; } = string.Empty;
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
