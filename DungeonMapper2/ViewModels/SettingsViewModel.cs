using DungeonMapper2.DataAccess;
using DungeonMapper2.Models;
using DungeonMapper2.Utilities;
using System;
using System.Windows;

namespace DungeonMapper2.ViewModels
{
    public class SettingsViewModel : DependencyObject
    {
        private Action _closeWindow;
        public static readonly DependencyProperty AutoSaveEnabledProperty = DependencyProperty.Register("AutoSaveEnabled", typeof(bool), typeof(SettingsViewModel));

        public bool AutoSaveEnabled
        {
            get => (bool)GetValue(AutoSaveEnabledProperty);
            set => SetValue(AutoSaveEnabledProperty, value);
        }

        private RelayCommand _saveCommand;
        private RelayCommand _CancelCommand;

        public RelayCommand SaveCommand => _saveCommand ??= new RelayCommand(eventArgs => Save(), o => true);

        public RelayCommand CancelCommand => _CancelCommand ??= new RelayCommand(eventArgs => _closeWindow(), o => true);

        public SettingsViewModel(Action closeWindow)
        {
            _closeWindow = closeWindow;
            AutoSaveEnabled = SettingDataAccess.GetSetting<bool?>(Setting.AutoSaveEnabled) ?? false;
        }

        private void Save()
        {
            SettingDataAccess.SaveSetting(Setting.AutoSaveEnabled, AutoSaveEnabled);
            _closeWindow();
        }
    }
}
