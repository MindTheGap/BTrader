using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BTraderWPF.Utilities;
using BTraderWPF.Windows.LogViewerWindow;

namespace BTraderWPF.Windows.ResearchWindow
{
  public class Step : INotifyPropertyChanged
  {
    #region Public Properties

    private Process _process;
    public Process Process
    {
	    get { return _process;}
	    set 
        { 
            _process = value;
            OnPropertyChanged("Process");
        }
    }

    private string _name;

    public string Name
    {
      get { return _name; }
      set
      {
        _name = value;
        OnPropertyChanged("Name");
      }
    }

    private double _progress;

    public double Progress
    {
      get { return _progress; }
      set
      {
        _progress = value;
        OnPropertyChanged("Progress");
      }
    }

    private DictionaryPlus<string,string> _argumentsDic;
    public DictionaryPlus<string, string> ArgumentsDic
    {
      get { return _argumentsDic; }
      set
      {
        _argumentsDic = value;
        OnPropertyChanged("ArgumentsDic");
      }
    }

    #endregion Public Properties

    #region Private Fields

    private ResearchViewModel _viewModel;
    private readonly Dispatcher _uiDispatcher;
    private bool _running;
    private bool _isRunValid;
    public string EventHandleName;
    public int EventHandleTimeout;
    protected EventWaitHandle _eventWaitHandle;

    #endregion

    #region Ctor

    public Step(ResearchViewModel viewModel)
    {
      _viewModel = viewModel;
      _uiDispatcher = Dispatcher.CurrentDispatcher;
    }

    #endregion

    #region Command Handlers

    public bool Run()
    {
      if (_running) return false;

      _viewModel.LogViewerViewModel.AnyErrorMessageArrived = false;
      _isRunValid = true;

      if (_viewModel.LogViewerViewModel.OutputConcurrentQueue != null
        && _viewModel.LogViewerViewModel.OutputConcurrentQueue.Count > 0)
      {
        _uiDispatcher.Invoke(() => _viewModel.LogViewerViewModel.OutputConcurrentQueue.Clear());
      }

      _running = true;
      var result = false;
      var watch = Stopwatch.StartNew();

      try
      {
        var processPath = _viewModel.ProcessPath;
        EventHandleName = null;

        if (ArgumentsDic.ContainsKey("-eventHandleName"))
        {
          EventHandleName = ArgumentsDic["-eventHandleName"];
          EventHandleTimeout = ArgumentsDic.ContainsKey("-eventHandleTimeout") ? int.Parse(ArgumentsDic["-eventHandleTimeout"]) : 5000;
        }

        if (Process != null && !Process.HasExited)
        {
          MessageBox.Show("A process was trying to run more than once");
          _isRunValid = false;
          return false;
        }

        _viewModel.LogViewerViewModel.AnyErrorMessageArrived = false;
        _isRunValid = true;

        SetPercentage(0.0);
        _running = true;

        string args = Utils.DicToStr(ArgumentsDic);

        var processCommand = string.Format("{0} {1}", processPath, args);

        Process = new Process()
        {
          StartInfo = new ProcessStartInfo()
          {
            FileName = processPath,
            Arguments = args,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
          },
          EnableRaisingEvents = true
        };
        Process.Start();

        _viewModel.PipeManager.UpdateStreams(Process.StandardOutput, Process.StandardError);
        _viewModel.PipeManager.WriteToMessageQueue("Starting step: " + Name);
        _viewModel.PipeManager.WriteToMessageQueue("Log Path: " + _viewModel.LogViewerViewModel.LogFilePath);
        _viewModel.PipeManager.WriteToMessageQueue("Command: " + processCommand);

        if (EventHandleName != null)
        {
          _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, EventHandleName);
        }

        if (!ArgumentsDic.ContainsKey("-timeout"))
        {
          Process.WaitForExit();
        }
        else
        {
          int timeout = int.Parse(ArgumentsDic["-timeout"]);
          if (!Process.WaitForExit(timeout*1000))
          {
            _viewModel.PipeManager.WriteToMessageQueue("Timeout (" + timeout + ") has passed! Killing process...",
              LogViewerViewModel.MessageType.Error);
            if (_eventWaitHandle != null)
            {
              _viewModel.PipeManager.WriteToMessageQueue(@"Sending signal to process before killing it!");
              _eventWaitHandle.Set();
              _viewModel.PipeManager.WriteToMessageQueue(@"Signal sent. Sleeping (timeout period) for " +
                                                           EventHandleTimeout + " mili-seconds");
              Thread.Sleep(EventHandleTimeout);
            }
            SetPercentage(100.0);
            _isRunValid = false;
            Process.Kill();
          }
        }
      }
      catch (ThreadAbortException tea)
      {
        // The prcess got ended
      }
      catch (TaskCanceledException)
      {
        // swallow
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message);
      }
      finally
      {
        result = ProcessEnded();

        _viewModel.PipeManager.WriteToMessageQueue("Step completed after: " + watch.ElapsedMilliseconds / 1000.0 + " seconds");
      }

      return result;
    }

    public bool ProcessEnded()
    {
      bool result = true;
      if (Process != null && !Process.HasExited)
      {
        _viewModel.PipeManager.WriteToMessageQueue(Process.StandardError.ReadToEnd(), LogViewerViewModel.MessageType.Error);

        _isRunValid = false;

        if (_eventWaitHandle != null)
        {
          _eventWaitHandle.Set();
          if (!Process.WaitForExit(EventHandleTimeout))
          {
            _viewModel.PipeManager.WriteToMessageQueue("Event Handle Timeout has passed and subprocess did not finish yet. killing it");
          }
        }

        Process.Kill();

        result = false;
      }

      // validate that the output and error streams have finished before unregistering listeners
      _viewModel.WaitForAllStreamsToFinish();
      _viewModel.PipeManager.ResetStreams();
      _running = false;

      return result;
    }

    private void SetPercentage(double percentage)
    {
      percentage = double.IsNaN(percentage) ? 0.0 : Math.Max(Math.Min(percentage, 100.0), 0.0);

      if (_uiDispatcher == Dispatcher.CurrentDispatcher)
      {
        _progress = percentage;
      }
      else
      {
        _uiDispatcher.Invoke(() =>
        {
          _progress = percentage;
        });
      }
    }

    #endregion

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
  }
}
