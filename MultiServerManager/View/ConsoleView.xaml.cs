using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MultiServerManager.Common.Collections;
using MultiServerManager.Core;
using MultiServerManager.ViewModel;

namespace MultiServerManager.View
{
    /// <summary>
    /// ConsoleView.xaml 的交互逻辑
    /// </summary>
    public partial class ConsoleView : UserControl
    {
        public ConsoleView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        

        private void CommandTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;

            SendText();
        }

        //private void ConsoleWindow_OnLoaded(object sender, RoutedEventArgs e)
        //{
        //    //var ctx = (ConsoleViewModel)DataContext;

        //    //previousConsoleLines = ctx.CurrentActiveServer?.ConsoleLines;
        //    //PropertyChangedEventManager.AddHandler(ctx, OnCurrentActiveServerChanged, nameof(ctx.CurrentActiveServer));

        //    //if (ctx.CurrentActiveServer == null)
        //    //    return;

        //    //ConsoleWindow.Inlines.AddRange(ctx.CurrentActiveServer.ConsoleLines.Select(x =>
        //    //    new Run(x.Text) { Foreground = new SolidColorBrush(x.Foreground), Background = new SolidColorBrush(x.Background) }));
        //    //CollectionChangedEventManager.AddHandler(ctx.CurrentActiveServer.ConsoleLines, OnConsoleLinesChanged);
        //}

        private void ConsoleWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ConsoleRichTextBox.ScrollToEnd();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldCtx = (ConsoleViewModel?)e.OldValue;
            if (oldCtx != null)
            {
                PropertyChangedEventManager.RemoveHandler(oldCtx, OnCurrentActiveServerChanged, nameof(oldCtx.CurrentActiveServer));

                if (previousConsoleLines != null)
                    CollectionChangedEventManager.RemoveHandler(previousConsoleLines, OnConsoleLinesChanged);
                previousConsoleLines = null;
                
                if (oldCtx.CurrentActiveServer?.ConsoleLines != null)
                    CollectionChangedEventManager.RemoveHandler(oldCtx.CurrentActiveServer.ConsoleLines, OnConsoleLinesChanged);

                ConsoleWindow.Inlines.Clear();
            }
            
            var newCtx = (ConsoleViewModel?)e.NewValue;
            if (newCtx != null)
            {
                PropertyChangedEventManager.AddHandler(newCtx, OnCurrentActiveServerChanged, nameof(newCtx.CurrentActiveServer));
                
                if (newCtx.CurrentActiveServer == null)
                    return;

                ConsoleWindow.Inlines.AddRange(newCtx.CurrentActiveServer.ConsoleLines.ToArray().Where(x => x != null).Select(x =>
                    new Run(x.Text) { Foreground = new SolidColorBrush(x.Foreground), Background = new SolidColorBrush(x.Background) }));
                CollectionChangedEventManager.AddHandler(newCtx.CurrentActiveServer.ConsoleLines, OnConsoleLinesChanged);
            }
        }

        private ObservableQueue<ServerConsoleLine>? previousConsoleLines;
        private void OnCurrentActiveServerChanged(object? sender, PropertyChangedEventArgs args)
        {
            var ctx = (ConsoleViewModel)DataContext;
            
            if (previousConsoleLines != null)
                CollectionChangedEventManager.RemoveHandler(previousConsoleLines, OnConsoleLinesChanged);
            previousConsoleLines = ctx.CurrentActiveServer?.ConsoleLines;
            ConsoleWindow.Inlines.Clear();

            if (ctx.CurrentActiveServer == null)
                return;

            ConsoleWindow.Inlines.AddRange(ctx.CurrentActiveServer.ConsoleLines.ToArray().Where(x => x != null).Select(x =>
                new Run(x.Text) { Foreground = new SolidColorBrush(x.Foreground), Background = new SolidColorBrush(x.Background) }));
            CollectionChangedEventManager.AddHandler(ctx.CurrentActiveServer.ConsoleLines, OnConsoleLinesChanged);
        }

        private void OnConsoleLinesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO: Totally APOS
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var i = (ServerConsoleLine)e.NewItems![0]!;
                        ConsoleWindow.Inlines.Add(new Run(i.Text) { Foreground = new SolidColorBrush(i.Foreground), Background = new SolidColorBrush(i.Background) });
                        ConsoleRichTextBox.ScrollToEnd();
                    });
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // TODO: Change this dirty method
                        ConsoleWindow.Inlines.Remove(ConsoleWindow.Inlines.FirstInline);
                    });
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ConsoleWindow.Inlines.Clear();
                    });
                    break;
            }
        }

        private void ButtonSend_OnClick(object sender, RoutedEventArgs e) => SendText();

        private void SendText()
        {
            var tb = CommandTextBox;
            var cmd = ((ConsoleViewModel)DataContext).AddTextToConsoleCommand;
            if (cmd.CanExecute(tb.Text))
                cmd.Execute(tb.Text);

            tb.Clear();
        }
    }
}
