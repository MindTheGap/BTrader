using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using BTraderWPF.Utilities;
using StreamingWPF.LogViewerNS;

namespace BTraderWPF.Windows.LogViewerWindow
{
  public class LogViewerViewModel : INotifyPropertyChanged, INotifyCollectionChanged
  {
    #region Consts

    private const int MaximumLogItemsAllowed = 2000;

    #endregion Consts

    #region Public Members

    public enum MessageType
    {
      Regular,
      Information,
      Warning,
      Error
    };

    public string LogFilePath { get; set; }
    public StreamWriter LogFile { get; set; }

    private bool _anyErrorMessageArrived;
    public bool AnyErrorMessageArrived
    {
      get { return _anyErrorMessageArrived; }
      set
      {
        _anyErrorMessageArrived = value;
        OnPropertyChanged("AnyErrorMessageArrived");
      }
    }

    private bool _autoScroll;
    public bool AutoScroll
    {
      get { return _autoScroll; }
      set
      {
        _autoScroll = value;
        OnPropertyChanged("AutoScroll");
      }
    }

    private ObservableConcurrentQueue<LogItem> _outputConcurrentQueue;
    public ObservableConcurrentQueue<LogItem> OutputConcurrentQueue
    {
      get { return _outputConcurrentQueue; }
      set
      {
        _outputConcurrentQueue = value;
        OnPropertyChanged("OutputConcurrentQueue");
      }
    }

    #endregion Public Members

    #region Private Members

    private readonly PipeManager _pipeManager;
    private readonly Dispatcher _uiDispatcher;
    private readonly LogViewer _viewer;

    #endregion Private Members

    public LogViewerViewModel(string name, PipeManager pipeManager, Dispatcher uiDispatcher)
    {
      _uiDispatcher = uiDispatcher;
      _pipeManager = pipeManager;
      OutputConcurrentQueue = new ObservableConcurrentQueue<LogItem>(MaximumLogItemsAllowed);
      _viewer = new LogViewer(this);
      AutoScroll = true;

      _viewer.Title = string.Format("LogViewer - {0}", name);
    }

    public void ShowViewer()
    {
      _viewer.Topmost = true;
      _viewer.Show();
    }

    public void ShowLogFile()
    {
      Process.Start("notepad.exe", LogFilePath);
    }

    public void RegisterListeners()
    {
      _pipeManager.AddListener(WriteToLogFile);
      RegisterOutputCollectionListener();
    }

    public void RegisterOutputCollectionListener()
    {
      _pipeManager.AddListener(WriteToOutputCollection);
    }

    public void UnregisterOutputCollectionListener()
    {
      _pipeManager.RemoveListener(WriteToOutputCollection);
    }

    public void UnregisterListeners()
    {
      _pipeManager.RemoveListener(WriteToLogFile);
      UnregisterOutputCollectionListener();
    }

    public void PrepareLog(DateTime startDate, DateTime endDate)
    {
      try
      {
        OutputConcurrentQueue.Clear();

        var logFileName = string.Format("{0}-{1}", startDate.YMD(), endDate.YMD());

        LogFilePath = @"logs";

        if (!Directory.Exists(LogFilePath))
        {
          Directory.CreateDirectory(LogFilePath);
        }

        var currentDateTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        LogFilePath = Path.Combine(LogFilePath, string.Format("{0}_{1}.log", logFileName, currentDateTime));
        LogFile = new StreamWriter(LogFilePath)
        {
          AutoFlush = true
        };
      }
      catch (Exception ex)
      {
        Utils.ShowMessageBoxSafely(ex.Message);
      }
    }

    private void WriteToLogFile(string line, bool isCarriageReturnActive, LogViewerViewModel.MessageType messageType)
    {
      try
      {
        if (LogFile != null)
        {
          LogFile.WriteLine(line);
        }
      }
      catch (Exception ex)
      {
        Utils.ShowMessageBoxSafely(ex.Message);
      }
    }

    private void WriteToOutputCollection(string line, bool isCarriageReturnActive, LogViewerViewModel.MessageType messageType)
    {
      if (messageType == LogViewerViewModel.MessageType.Error)
      {
        AnyErrorMessageArrived = true;
      }

      if (Dispatcher.CurrentDispatcher != _uiDispatcher)
      {
        _uiDispatcher.Invoke(() => WriteToOutputCollectionDo(line, isCarriageReturnActive, messageType));
      }
      else
      {
        WriteToOutputCollectionDo(line, isCarriageReturnActive, messageType);
      }
    }

    private void WriteToOutputCollectionDo(string line, bool isCarriageReturnActive, LogViewerViewModel.MessageType messageType)
    {
      try
      {
        var logItem = new LogItem(line, messageType);

        OutputConcurrentQueue.Enqueue(logItem);
      }
      catch (Exception e)
      {
        Utils.ShowMessageBoxSafely("Exception: " + e.Message + "\n\nInner Exception: " + e.InnerException);
      }

    }

    #region Property Changed

    public void OnPropertyChanged(string info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion Property Changed

    public event NotifyCollectionChangedEventHandler CollectionChanged;
  }
}
