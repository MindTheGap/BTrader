using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;

namespace BTraderWPF.Utilities
{
  public class ObservableConcurrentQueue<T> : INotifyCollectionChanged, IEnumerable<T>
  {
    public event NotifyCollectionChangedEventHandler CollectionChanged;
    private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
    private readonly int _maximumLogItemsAllowed;

    public ObservableConcurrentQueue(int maximumLogItemsAllowed)
    {
      _maximumLogItemsAllowed = maximumLogItemsAllowed;
    }

    public void Enqueue(T item)
    {
      if (Count == _maximumLogItemsAllowed) Dequeue();

      _queue.Enqueue(item);
      if (CollectionChanged != null)
      {
        CollectionChanged(this,
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item));
      }
    }

    public T Dequeue()
    {
      T item;
      _queue.TryDequeue(out item);
      if (CollectionChanged != null)
      {
        CollectionChanged(this,
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove, item, 0));
      }
      return item;
    }

    public void Clear()
    {
      var newQueue = new ConcurrentQueue<T>();
      Interlocked.Exchange(ref _queue, newQueue);
      if (CollectionChanged != null)
      {
        CollectionChanged(this,
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Reset, null));
      }
    }

    public int Count { get { return _queue.Count; } }

    public IEnumerator<T> GetEnumerator()
    {
      return _queue.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
