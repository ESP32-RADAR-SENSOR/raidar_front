using System.Windows;
using RAIDAR_FRONT.ViewModels;

namespace RAIDAR_FRONT
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                bool connected =
                    await vm.NetworkService.ConnectAsync(
                        "127.0.0.1",
                        9001
                    );

                if (!connected)
                {
                    MessageBox.Show(
                        "C++ 서버에 연결하지 못했습니다."
                    );
                }
            }
        }
    }
}