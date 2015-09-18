using System.ComponentModel;
using System.Windows;

namespace BTraderWPF.Windows.MainWindow
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow(MainWindowViewModel viewModel)
    {
      InitializeComponent();

      DataContext = viewModel;
    }

    private void MainWindow_OnClosing(object sender, CancelEventArgs e)
    {
      var window = sender as MainWindow;
      if (window != null)
      {
        window.Visibility = Visibility.Hidden;
        e.Cancel = true;
      }
    }
  }
}
