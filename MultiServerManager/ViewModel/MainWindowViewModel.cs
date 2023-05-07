using CommunityToolkit.Mvvm.DependencyInjection;
using MultiServerManager.Common;
using MultiServerManager.Service;

namespace MultiServerManager.ViewModel
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public NavigationService NavigationService { get; }

        public MainWindowViewModel()
        {
            NavigationService = Ioc.Default.GetRequiredService<NavigationService>();

            base.ViewModelName = "MainWindow";
            base.ViewModelDescription = "No description";
        }
    }
}
