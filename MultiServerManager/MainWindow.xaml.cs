using MultiServerManager.ViewModel;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using MultiServerManager.Service;

namespace MultiServerManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();

            InitializeComponent();
            
            MainSnackbar.MessageQueue = Ioc.Default.GetRequiredService<SnackbarService>().MessageQueue;
        }

        private void NavigationListBox_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) => MenuToggleButton.IsChecked = false;
    }
}
