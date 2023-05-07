using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MultiServerManager.Common.Collections;

public class ObservableQueue<T> : Queue<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    public ObservableQueue()
    {
    }

    public ObservableQueue(IEnumerable<T> collection) : base(collection)
    {
    }

    public ObservableQueue(int capacity) : base(capacity)
    {
    }


    public new virtual void Clear()
    {
        base.Clear();
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public new virtual T Dequeue()
    {
        var item = base.Dequeue();
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        return item;
    }

    public new virtual void Enqueue(T item)
    {
        base.Enqueue(item);
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    }


    public event NotifyCollectionChangedEventHandler? CollectionChanged;


    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        this.RaiseCollectionChanged(e);
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        this.RaisePropertyChanged(e);
    }


    protected event PropertyChangedEventHandler? PropertyChanged;


    private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        this.CollectionChanged?.Invoke(this, e);
    }

    private void RaisePropertyChanged(PropertyChangedEventArgs e)
    {
        this.PropertyChanged?.Invoke(this, e);
    }


    event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
    {
        add => this.PropertyChanged += value;
        remove => this.PropertyChanged -= value;
    }
}