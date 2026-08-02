using RAIDAR_FRONT.Models;
using RAIDAR_FRONT.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RAIDAR_FRONT.ViewModels
{

    /// 실시간 로그 뷰어 뷰모델 골격 (직접 작성할 영역)

    public class LogViewerViewModel : ViewModelBase
    {
        private const int MaxLogEntries = 1000; // 최대 로그 항목 수
        private string _selectedFilter = "ALL"; // 선택된 필터
        private bool _autoScroll = true; // 자동 스크롤 여부

        public ObservableCollection<LogEntry> Logs { get; } = new();

        public string SelectedFilter { get { return _selectedFilter; } set => SetProperty(ref _selectedFilter, value); }
        public bool AutoScroll { get { return _autoScroll; } set => SetProperty(ref _autoScroll, value); }

        public ICommand ClearLogsCommand { get; }

        public LogViewerViewModel(INetworkService networkService)
        {
            ClearLogsCommand = new RelayCommand(() => Logs.Clear());
            networkService.LogReceived += OnLogReceived;
        }

        public void AddLog(LogEntry logEntry)
        {
            OnLogReceived(logEntry);
        }

        private void OnLogReceived(LogEntry logEntry)
        {
            if (SelectedFilter != "ALL" && logEntry.Level != SelectedFilter)
                return;
            Logs.Insert(0, logEntry);
            // 최대 로그 항목 수 유지
            if (Logs.Count > MaxLogEntries)
                Logs.RemoveAt(0);
        }
    }
}
