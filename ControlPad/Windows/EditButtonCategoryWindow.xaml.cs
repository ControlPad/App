using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Wpf.Ui.Controls;
using Microsoft.Win32;
using System.Reflection.Metadata;
using System.Collections.ObjectModel;
using NAudio.CoreAudioApi;

namespace ControlPad
{
    public partial class EditButtonCategoryWindow : FluentWindow
    {
        private int indexOfCategory;
        MainWindow _mainWindow;
        List<ButtonAction> _buttonActionsTemp;
        public EditButtonCategoryWindow(int indexOfCategory, MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            this.indexOfCategory = indexOfCategory;

            tb_CategoryName.Text = DataHandler.ButtonCategories[this.indexOfCategory].Name;

            _buttonActionsTemp = GetCopy(DataHandler.ButtonCategories[this.indexOfCategory].ButtonActions);

            ComboBox_Type.DisplayMemberPath = "Description";
            ComboBox_Type.ItemsSource = DataHandler.ActionTypes;

            LoadButtonActions();
        }

        private List<ButtonAction> GetCopy(List<ButtonAction> original)
        {
            return original.Select(a => new ButtonAction(a.ActionType)
            {
                ActionProperty = a.ActionProperty,
                ActionPropertyDisplay = a.ActionPropertyDisplay
            }).ToList();
        }

        private void LoadButtonActions()
        {
            ActionsContainer.Children.Clear();
            foreach (var buttonAction in _buttonActionsTemp)
            {
                var ctrl = CreateAction(buttonAction);
                ActionsContainer.Children.Add(ctrl);
                ctrl.TextBlock.Text = $"{buttonAction.ActionType.Description}: {buttonAction.ActionPropertyDisplay}";
            }
        }
        private void btn_AddAction_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBox_Type.SelectedItem == null || string.IsNullOrEmpty(ComboBox_Type.SelectedItem.ToString())) return;

            ButtonAction buttonAction = new ButtonAction((ActionType)ComboBox_Type.SelectedItem);
            ActionsContainer.Children.Add(CreateAction(buttonAction));
            _buttonActionsTemp.Add(buttonAction);
        }

        private void btn_Remove_Click(object sender, EventArgs e)
        {
            var btn = (System.Windows.Controls.Button)sender;

            if (btn.Parent is Grid grid && grid.Parent is ButtonActionUserControl wrapper)
            {
                RemoveHandlers(wrapper);
                ActionsContainer.Children.Remove(wrapper);
                _buttonActionsTemp.Remove(wrapper.ButtonAction);
            }
        }

        private ButtonActionUserControl CreateAction(ButtonAction buttonAction)
        {
            var buttonActionControl = new ButtonActionUserControl(buttonAction);
            if(buttonAction.ActionType.Type == EActionType.MuteMainAudio) buttonActionControl.btn_Settings.IsEnabled = false;
            buttonActionControl.TextBlock.Text = buttonAction.ActionType.Description;
            AddHandlers(buttonActionControl);
            return buttonActionControl;
        }

        private void AddHandlers(ButtonActionUserControl control)
        {
            control.btn_Settings.Click += new RoutedEventHandler(btn_settings_Click);
            control.btn_Remove.Click += new RoutedEventHandler(btn_Remove_Click);
        }
            
        private void RemoveHandlers(ButtonActionUserControl control)
        {
            control.btn_Settings.Click -= btn_settings_Click;
            control.btn_Remove.Click -= btn_Remove_Click;
        }

        private void btn_settings_Click(object sender, EventArgs e)
        {
            var btn = (System.Windows.Controls.Button)sender;
            var control = (ButtonActionUserControl)((Grid)btn.Parent).Parent;

            switch (control.ButtonAction.ActionType.Type)
            {
                case EActionType.MuteProcess:
                    {
                        var processDialog = new SelectProcessPopup { Owner = this };
                        processDialog.cb_Processes.Text = control.ButtonAction.ActionProperty as string ?? processDialog.cb_Processes.Text;

                        if (processDialog.ShowDialog() == true)
                        {
                            control.ButtonAction.ActionProperty = processDialog.SelectedProcessName;
                            control.ButtonAction.ActionPropertyDisplay = processDialog.SelectedProcessName;
                            control.TextBlock.Text = $"{control.ButtonAction.ActionType.Description}: {processDialog.SelectedProcessName}";
                        }
                        break;
                    }
                case EActionType.MuteMainAudio: break;
                case EActionType.MuteMic:
                    {
                        var micDialog = new SelectMicPopup { Owner = this };
                        micDialog.cb_Mics.Text = control.ButtonAction.ActionProperty as string ?? micDialog.cb_Mics.Text;

                        if (micDialog.ShowDialog() == true)
                        {
                            control.ButtonAction.ActionProperty = micDialog.SelectedMicName;
                            control.ButtonAction.ActionPropertyDisplay = micDialog.SelectedMicName;
                            control.TextBlock.Text = $"{control.ButtonAction.ActionType.Description}: {micDialog.SelectedMicName}";
                        }
                        break;
                    }                   
                case EActionType.OpenProcess:
                    {
                        var fileDialog = new Microsoft.Win32.OpenFileDialog
                        {
                            Title = "Select an EXE",
                            Filter = "Executable (*.exe)|*.exe|All Files (*.*)|*.*",
                            CheckFileExists = true,
                            Multiselect = false,
                            DefaultExt = ".exe"
                        };

                        if (fileDialog.ShowDialog() == true)
                        {
                            control.ButtonAction.ActionProperty = fileDialog.FileName;
                            control.ButtonAction.ActionPropertyDisplay = fileDialog.SafeFileName;
                            control.TextBlock.Text = $"{control.ButtonAction.ActionType.Description}: {fileDialog.SafeFileName}";
                        }
                        break;
                    }                   
                case EActionType.OpenWebsite: 
                    {
                        var websiteDialog = new EnterWebsitePopup { Owner = this };
                        websiteDialog.tb_WebsiteURL.Text = control.ButtonAction.ActionProperty as string ?? websiteDialog.tb_WebsiteURL.Text;

                        if (websiteDialog.ShowDialog() == true)
                        {
                            control.ButtonAction.ActionProperty = websiteDialog.URL;
                            control.ButtonAction.ActionPropertyDisplay = websiteDialog.DisplayURL;
                            control.TextBlock.Text = $"{control.ButtonAction.ActionType.Description}: {websiteDialog.DisplayURL}";
                        }
                        break;
                    }
                case EActionType.KeyPress:
                    {
                        var keyDialog = new SelectKeyPopup(_mainWindow) { Owner = this };

                        if (keyDialog.ShowDialog() == true)
                        {
                            control.ButtonAction.ActionProperty = keyDialog.SelectedKey.Code.ToString();
                            control.ButtonAction.ActionPropertyDisplay = keyDialog.SelectedKey.Label;
                            control.TextBlock.Text = $"{control.ButtonAction.ActionType.Description}: {keyDialog.SelectedKey.Label}";
                        }
                        break;
                    }
                default: break;
            }
        }

        private void RemoveEmptyActionsFromTemp()
        {
            var buttonActions = new List<ButtonAction>();
            var toRemove = new List<ButtonActionUserControl>();

            foreach (ButtonActionUserControl control in ActionsContainer.Children)
            {
                if (control.ButtonAction.ActionProperty == null && control.ButtonAction.ActionType.Type != EActionType.MuteMainAudio)
                {
                    toRemove.Add(control);
                }
                else
                {
                    buttonActions.Add(control.ButtonAction);
                }
            }

            foreach (var control in toRemove)
            {
                RemoveHandlers(control);
                ActionsContainer.Children.Remove(control);
            }

            _buttonActionsTemp = buttonActions;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb_CategoryName.Text)) return;

            RemoveEmptyActionsFromTemp();
            DataHandler.ButtonCategories[this.indexOfCategory].Name = tb_CategoryName.Text;
            DataHandler.ButtonCategories[indexOfCategory].ButtonActions = GetCopy(_buttonActionsTemp);
            DataHandler.SaveDataToFile(DataHandler.GetButtonCategoriesPath(), DataHandler.ButtonCategories.ToList());
            KeyController.StopHoldingAllKeys();
            DialogResult = true;
        }
    }
}
