using System.Windows.Media;

namespace BTraderWPF.Windows.LogViewerWindow
{
  public class LogItem
  {
    public string Message { get; set; }
    public SolidColorBrush Color { get; set; }
    public LogViewerViewModel.MessageType MessageType { get; set; }

    public LogItem(string message, LogViewerViewModel.MessageType messageType = LogViewerViewModel.MessageType.Regular)
    {
      Message = message;
      MessageType = messageType;

      switch (messageType)
      {
        case LogViewerViewModel.MessageType.Regular:
          Color = new SolidColorBrush(Colors.Navy);
          break;
        case LogViewerViewModel.MessageType.Information:
          Color = new SolidColorBrush(Colors.Orange);
          break;
        case LogViewerViewModel.MessageType.Warning:
          Color = new SolidColorBrush(Colors.YellowGreen);
          break;
        case LogViewerViewModel.MessageType.Error:
          Color = new SolidColorBrush(Colors.Red);
          break;
      }
    }
  }
}
