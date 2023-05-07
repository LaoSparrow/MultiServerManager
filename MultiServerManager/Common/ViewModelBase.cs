using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Controls;

namespace MultiServerManager.Common
{
    [INotifyPropertyChanged]
    public abstract partial class ViewModelBase
    {
        public string ViewModelName { get; set; } = "Foo Name";

        public string ViewModelDescription { get; set; } = "Bar Description";

        public string Icon { get; set; } = "QuestionMarkBox";

        public ScrollBarVisibility VerticalScrollBarVisibility { get; set; } = ScrollBarVisibility.Auto;

        public ScrollBarVisibility HorizontalScrollBarVisibility { get; set; } = ScrollBarVisibility.Auto;

        //[ObservableProperty]
        //private object? view;
    }
}
