using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using RAIDAR_FRONT.Models;
using RAIDAR_FRONT.Services;

namespace RAIDAR_FRONT.ViewModels
{
    /// <summary>
    /// 실시간 이벤트 및 에러 로그 뷰어 (Region 3) ViewModel
    /// </summary>
    public class LogViewerViewModel : ViewModelBase
    {
        private const int MaxLogEntries = 200;
        private string _selectedFilter = "ALL";
        private bool _autoScroll = true;

        public ObservableCollection<LogEntry> Logs { get; } = new();

        public string SelectedFilter
        {
            get => _selectedFilter;
            set => SetProperty(ref _selectedFilter, value);
        }

        public bool AutoScroll
        {
            get => _autoScroll;
            set => SetProperty(ref _autoScroll, value);
        }

        public ICommand ClearLogsCommand { get; }

        public LogViewerViewModel(INetworkService networkService)
        {
            ClearLogsCommand = new RelayCommand(() => Logs.Clear());

            if (networkService != null)
            {
                networkService.LogReceived += OnLogReceived;
            }
        }

        public void AddLog(LogEntry log)
        {
            OnLogReceived(log);
        }

        private void OnLogReceived(LogEntry log)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Logs.Add(log);
                if (Logs.Count > MaxLogEntries)
                {
                    Logs.RemoveAt(0);
                }
            });
        }
    }
}
