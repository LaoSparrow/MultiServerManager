using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using MultiServerManager.ViewModel;

namespace MultiServerManager.View
{
    /// <summary>
    /// ServerListView.xaml 的交互逻辑
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class ServerListView : UserControl
    {
        public ServerListView()
        {
            InitializeComponent();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnRemoveServerDialogClosed(object sender, DialogClosedEventArgs e)
        {
            var cmd = ((ServerListViewModel)DataContext).RemoveServerCommand;
            if ((bool)e.Parameter! && cmd.CanExecute(null))
                cmd.Execute(null);
        }
    }
}
