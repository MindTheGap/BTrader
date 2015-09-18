using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace BTraderWPF.Utilities
{
  public class ListBoxBehavior
  {
    private static Dictionary<ListBox, Capture> Associations = new Dictionary<ListBox, Capture>();

    public static bool GetScrollOnNewItem(DependencyObject obj)
    {
      return (bool)obj.GetValue(ScrollOnNewItemProperty);
    }

    public static void SetScrollOnNewItem(DependencyObject obj, bool value)
    {
      obj.SetValue(ScrollOnNewItemProperty, value);
    }

    public static readonly DependencyProperty ScrollOnNewItemProperty =
      DependencyProperty.RegisterAttached(
        "ScrollOnNewItem",
        typeof(bool),
        typeof(ListBoxBehavior),
        new UIPropertyMetadata(false, OnScrollOnNewItemChanged));

    public static void OnScrollOnNewItemChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      var listBox = d as ListBox;
      if (listBox == null) return;
      bool oldValue = (bool)e.OldValue, newValue = (bool)e.NewValue;
      if (newValue == oldValue) return;
      if (newValue)
      {
        Associations[listBox] = new Capture(listBox);
      }
      else
      {
        if (Associations.ContainsKey(listBox))
        {
          Associations[listBox].Dispose();
        }
        Associations[listBox] = null;
      }
    }

    private class Capture : IDisposable
    {
      public ListBox listBox { get; set; }
      public INotifyCollectionChanged incc { get; set; }

      public Capture(ListBox listBox)
      {
        this.listBox = listBox;
        incc = listBox.ItemsSource as INotifyCollectionChanged;
        if (incc != null)
        {
          incc.CollectionChanged += incc_CollectionChanged;
        }
      }

      private void incc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
          listBox.ScrollIntoView(e.NewItems[0]);
          listBox.SelectedItem = e.NewItems[0];
        }
      }

      public void Dispose()
      {
        if (incc != null)
          incc.CollectionChanged -= incc_CollectionChanged;
      }
    }
  }
}
