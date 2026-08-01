using RAIDAR_FRONT.Services;

namespace RAIDAR_FRONT.ViewModels
{
    /// <summary>
    /// 메인 대시보드 ViewModel
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public INetworkService NetworkService { get; }
        public RadarViewModel RadarVM { get; } = new();

        public MainViewModel()
        {
            NetworkService = new TcpClientService();
        }
    }
}
