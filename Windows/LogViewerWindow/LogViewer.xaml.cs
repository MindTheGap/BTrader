using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BTraderWPF.Utilities;
using BTraderWPF.Windows.LogViewerWindow;

namespace StreamingWPF.LogViewerNS
{
  /// <summary>
  /// Interaction logic for LogViewer.xaml
  /// </summary>
  public partial class LogViewer : Window
  {
    private readonly LogViewerViewModel _logViewerViewModel;

    public LogViewer(LogViewerViewModel logViewerViewModel)
    {
      _logViewerViewModel = logViewerViewModel;

      DataContext = logViewerViewModel;

      InitializeComponent();
    }

    private void LogViewer_OnClosing(object sender, CancelEventArgs e)
    {
      var window = sender as LogViewer;
      if (window != null)
      {
        window.Visibility = Visibility.Hidden;
        e.Cancel = true;
      }
    }

    private void OpenLogFileButtonClick(object sender, RoutedEventArgs e)
    {
      if (_logViewerViewModel != null)
      {
        _logViewerViewModel.ShowLogFile();
      }
    }

    private void CopyListBoxMenuClick(object sender, RoutedEventArgs e)
    {
      int index = MainListBox.SelectedIndex;
      if (index != -1)
      {
        var logItem = MainListBox.Items[index] as LogItem;
        if (logItem != null)
        {
          string text = logItem.Message;
          Clipboard.SetText(text);

          Utils.ShowMessageBoxSafely("Text Copied!");
        }
      }
    }
  }
}
