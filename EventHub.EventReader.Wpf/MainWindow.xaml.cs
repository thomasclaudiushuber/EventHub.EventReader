using System.Windows;
using ThomasClaudiusHuber.EventHub.Receiver.DataAccess;
using ThomasClaudiusHuber.EventHub.Receiver.ViewModel;

namespace ThomasClaudiusHuber.EventHub.Receiver
{
  public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel(new EventHubSubscriber());
        }
    }
}
