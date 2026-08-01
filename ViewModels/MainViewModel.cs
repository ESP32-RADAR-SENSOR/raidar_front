using System;
using RAIDAR_FRONT.Models;
using RAIDAR_FRONT.Services;

namespace RAIDAR_FRONT.ViewModels
{
    /// <summary>
    /// 메인 대시보드 ViewModel
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public INetworkService NetworkService { get; }
        public RadarViewModel RadarVM { get; }
        public DeviceControlViewModel DeviceControlVM { get; }
        public LogViewerViewModel LogViewerVM { get; }

        public MainViewModel()
        {
            NetworkService = new TcpClientService();
            
            RadarVM = new RadarViewModel();
            DeviceControlVM = new DeviceControlViewModel(NetworkService);
            LogViewerVM = new LogViewerViewModel(NetworkService);

            // 레이더 타겟 수신시 업데이트
            NetworkService.TargetDataReceived += onTargetReceived;

            // 레이더 상의 선택 타겟 변경시 제어 패널 연동
            RadarVM.TargetSelectionChanged += target =>
            {
                DeviceControlVM.SelectedTarget = target;
            };

            // 초기 안내 로그 추가
            LogViewerVM.AddLog(new LogEntry
            {
                Level = "INFO",
                Message = "RAIDAR 프론트엔드 대시보드 시스템이 성공적으로 구동되었습니다.",
                Source = "SYSTEM"
            });
        }

        private void onTargetReceived(RadarTargetDto dto)
        {
            RadarVM.UpdateTarget(dto);
        }
    }
}
