using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ControlPad
{
    public static class ArduinoController
    {
        private static SerialPort? _serialPort;
        private static ManagementEventWatcher? _insertWatcher;
        private static ManagementEventWatcher? _removeWatcher;
        private static MainWindow _mainWindow;
        private static EventHandler _eventHandler;
        public static bool IsConnected = false;
        private static CancellationTokenSource? _readCts;
        private static readonly StringBuilder _lineBuf = new();
        private static BoardType _lastBoardType = BoardType.None;

        public static void Initialize(MainWindow mainWindow, EventHandler eventHandler)
        {
            _mainWindow = mainWindow;
            _eventHandler = eventHandler;

            _insertWatcher = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2")
            );
            _insertWatcher.EventArrived += async (s, e) => await TryOpenAsync();
            _insertWatcher.Start();

            _removeWatcher = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3")
            );
            _removeWatcher.EventArrived += (s, e) =>
            {
                if (_serialPort != null && !_serialPort.IsOpen)
                {
                    _mainWindow.Dispatcher.BeginInvoke(() =>
                    {
                        _mainWindow.BoardDisconnectedInfoBar.IsOpen = true;
                        if (_mainWindow.NVI_Home.IsActive) _mainWindow.MainContentFrame.Navigate(_mainWindow.progressRing);
                        _mainWindow.NVI_EditMode.Visibility = Visibility.Collapsed;
                        IsConnected = false;
                    });
                }
            };
            _removeWatcher.Start();

            _ = TryOpenAsync();
        }

        private static async Task TryOpenAsync()
        {
            string? port = await Task.Run(() => ArduinoPortFinder.FindFirstArduinoPort());
            if (port == null)
            {
                await _mainWindow.Dispatcher.InvokeAsync(() =>
                {
                    _mainWindow.BoardDisconnectedInfoBar.IsOpen = true;
                    if (_mainWindow.NVI_Home.IsActive) _mainWindow.MainContentFrame.Navigate(_mainWindow.progressRing);
                    _mainWindow.NVI_EditMode.Visibility = Visibility.Collapsed;
                    IsConnected = false;
                });
                return;
            }

            try
            {
                var sp = new SerialPort(port, 115200)
                {
                    NewLine = "\n",
                    ReadTimeout = 50,
                    WriteTimeout = 50,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    DtrEnable = true,
                    RtsEnable = false
                };

                sp.Open();
                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();
                _serialPort = sp;

                _readCts = new CancellationTokenSource();
                _ = Task.Run(() => ReadLoopAsync(sp, _readCts.Token));

                await _mainWindow.Dispatcher.InvokeAsync(() =>
                {
                    _mainWindow.BoardDisconnectedInfoBar.IsOpen = false;
                    if (_mainWindow.NVI_Home.IsActive) _mainWindow.MainContentFrame.Navigate(_mainWindow._homeUserControl);
                    _mainWindow.NVI_EditMode.Visibility = Visibility.Visible;
                    IsConnected = true;
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"Access denied to port: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EX: {ex}");
            }
        }

        private static async Task ReadLoopAsync(SerialPort sp, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && sp.IsOpen)
            {
                try
                {
                    string chunk = sp.ReadExisting();
                    if (!string.IsNullOrEmpty(chunk))
                    {
                        _lineBuf.Append(chunk);

                        int nl;
                        while ((nl = _lineBuf.ToString().IndexOf('\n')) >= 0)
                        {
                            string line = _lineBuf.ToString(0, nl).TrimEnd('\r');
                            _lineBuf.Remove(0, nl + 1);

                            ProcessLine(line);
                        }
                    }
                }
                catch (TimeoutException)
                {
                }
                catch (IOException ioEx)
                {
                    Debug.WriteLine($"IO: {ioEx.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"EX: {ex}");
                }

                await Task.Delay(1, ct);
            }

            await _mainWindow.Dispatcher.InvokeAsync(() =>
            {
                _mainWindow.BoardDisconnectedInfoBar.IsOpen = true;
                if (_mainWindow.NVI_Home.IsActive) _mainWindow.MainContentFrame.Navigate(_mainWindow.progressRing);
                _mainWindow.NVI_EditMode.Visibility = Visibility.Collapsed;
                IsConnected = false;
            });
        }

        private static void ProcessLine(string line)
        {
            try
            {
                var inputs = Regex.Split(line, ",");
                if (inputs.Length < 17) return;

                if (int.TryParse(inputs[0], out int boardType))
                {
                    if (_lastBoardType != (BoardType)boardType)
                    {
                        _mainWindow.Dispatcher.BeginInvoke(() =>
                        {
                            _mainWindow.UpdateBoardType(_lastBoardType, (BoardType)boardType);
                        });
                        _lastBoardType = (BoardType)boardType;
                    }
                }

                UpdateValues(inputs);

                // UI-Updates ggf. drosseln
                _mainWindow._homeUserControl.Dispatcher.BeginInvoke(() =>
                    _eventHandler.Update(DataHandler.SliderValues, DataHandler.ButtonValues)
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Parse EX: {ex}");
            }
        }

        public static void Dispose()
        {
            try { _insertWatcher?.Stop(); } catch { }
            try { _removeWatcher?.Stop(); } catch { }
            try
            {
                _readCts?.Cancel();
                _serialPort?.Close();
                _serialPort?.Dispose();
            }
            catch { }
        }

        /*private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort == null || !_serialPort.IsOpen) return;

            try
            {
                string line = _serialPort.ReadLine().Replace("\r", "");
                string[] inputs = Regex.Split(line, ",");

                if (inputs.Length < 17) return;

                UpdateValues(inputs);

                _mainWindow._homeUserControl.Dispatcher.BeginInvoke(() => _eventHandler.Update(DataHandler.SliderValues, DataHandler.ButtonValues));
            }
            catch (IOException)
            {
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EX: {ex}");
            }
        }*/

        private static void UpdateValues(string[] inputs)
        {
            for (int i = 0; i < DataHandler.SliderValues.Count; i++)
                DataHandler.SliderValues[i] = (DataHandler.SliderValues[i].slider, int.Parse(inputs[i]));
            for (int i = 0; i < DataHandler.ButtonValues.Count; i++)
                DataHandler.ButtonValues[i] = (DataHandler.ButtonValues[i].button, int.Parse(inputs[i + DataHandler.SliderValues.Count]));
        }
    }
}