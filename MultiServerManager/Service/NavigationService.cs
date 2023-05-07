using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MultiServerManager.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MultiServerManager.Service
{
    public static class NavigationServiceMessage
    {
        public static readonly Guid NavigatedToViewModel = Guid.NewGuid();

        public class NavigatedToViewModelMessage : ValueChangedMessage<ViewModelBase>
        {
            public NavigatedToViewModelMessage(ViewModelBase value, object[] parameters) : base(value)
                => Parameters = parameters;
            public object[] Parameters { get; }
        }
    }

    public partial class NavigationService : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ViewModelBase> viewModels;

        [ObservableProperty]
        private ViewModelBase currentViewModel;
        public int CurrentIndex => ViewModels.IndexOf(CurrentViewModel);

        public NavigationService(IEnumerable<ViewModelBase> viewModels, ViewModelBase? currentViewModel = null)
        {
            var viewModelsArray = viewModels.ToArray();
            if (viewModels == null || viewModelsArray.Length == 0)
                throw new ArgumentException("viewModels cannot be null or empty");
            if (currentViewModel != null && !viewModelsArray.Contains(currentViewModel))
                throw new ArgumentException("currentViewModel is not contained by viewModels");

            this.viewModels = new ObservableCollection<ViewModelBase>(viewModelsArray);
            this.currentViewModel = currentViewModel ?? viewModelsArray[0];
        }

        public bool NavigateTo<T>(params object[] args)
        {
            var selected = ViewModels.SingleOrDefault(x => x is T);
            if (selected == null)
                return false;

            CurrentViewModel = selected;
            WeakReferenceMessenger.Default.Send(
                new NavigationServiceMessage.NavigatedToViewModelMessage(selected, args),
                NavigationServiceMessage.NavigatedToViewModel);
            return true;
        }
    }
}
