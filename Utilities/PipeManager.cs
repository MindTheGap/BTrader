using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BTraderWPF.Windows.LogViewerWindow;

namespace BTraderWPF.Utilities
{
  public class PipeManager
  {
    #region Private Members

    private StreamReader _output;
    private StreamReader _error;
    private Thread _outputThread;
    private Thread _errorThread;
    private Thread _messagesThread;

    private readonly ConcurrentQueue<Tuple<string, bool, LogViewerViewModel.MessageType>> _messages;

    private readonly List<Action<string, bool, LogViewerViewModel.MessageType>> _callbacks = new List<Action<string, bool, LogViewerViewModel.MessageType>>();
    private readonly List<Action<int>> _prccallbacks = new List<Action<int>>();

    #endregion Private Members

    #region Ctor

    public PipeManager()
    {
      _messages = new ConcurrentQueue<Tuple<string, bool, LogViewerViewModel.MessageType>>();
      StartAllThreads();
    }

    #endregion Ctor

    #region Private Methods

    private void UpdateFuncs()
    {
      while (true)
      {
        var item = new Tuple<string, bool, LogViewerViewModel.MessageType>("", false, LogViewerViewModel.MessageType.Regular);
        while (_messages.TryDequeue(out item))
        {
          try
          {
            var line = getFormattedTime() + item.Item1;
            var isCarriageReturnActive = item.Item2;
            var messageType = item.Item3;

            lock (_callbacks)
            {
              foreach (var callback in _callbacks)
              {
                callback(line, isCarriageReturnActive, messageType);
              }
            }
          }
          catch (Exception e)
          {
            MessageBox.Show("UpdateFuncs error! Exception: " + e.Message + ".\n\nInner Exception: " + e.InnerException);
            return;
          }
        }

        // Didn't have anything to dequeue so sleeping for some time
        // this is used to reduce the lag of the system
        Thread.Sleep(500);
      }
    }

    private void UpdatePrc(char[] c)
    {
      lock (_prccallbacks)
      {
        foreach (var callback in _prccallbacks)
        {
          try
          {
            int prc;
            if (int.TryParse(new string(c), out prc))
            {
              callback(prc);
            }
          }
          catch (ThreadAbortException e)
          {
            // swallow
            MessageBox.Show("UpdatePrc had ThreadAbortException: " + e.Message);
          }
          catch (TaskCanceledException)
          {
            // swallow
          }
          catch (Exception e)
          {
            MessageBox.Show("UpdatePrc error! Exception: " + e.Message + ".\n\nInner Exception: " + e.InnerException);
          }
        }
      }
    }

    public bool AllStreamsFinished()
    {
      return ((_output == null || _output.EndOfStream)
             && (_error == null || _error.EndOfStream));
    }

    private void _readOutput()
    {
      try
      {
        var line = "";
        var carriageReturnActive = false;
        while (true)
        {
          if (_output == null || _output.EndOfStream)
          {
            Thread.Sleep(1000);
          }
          while (_output != null && !_output.EndOfStream)
          {
            while (_output.Peek() >= 0)
            {
              var c = new char[1];
              _output.Read(c, 0, c.Length);
              switch (c[0])
              {
                case '\r':
                  if (line.Length > 0)
                  {
                    _messages.Enqueue(new Tuple<string, bool, LogViewerViewModel.MessageType>(line, carriageReturnActive,
                      LogViewerViewModel.MessageType.Regular));
                  }
                  carriageReturnActive = true;
                  line = "";
                  break;
                case '\a':
                  c = new char[4];
                  _output.Read(c, 0, c.Length);
                  UpdatePrc(c);
                  _messages.Enqueue(new Tuple<string, bool, LogViewerViewModel.MessageType>(new string(c), carriageReturnActive, LogViewerViewModel.MessageType.Regular));
                  break;
                case '\n':
                  if (line.Length > 0)
                  {
                    _messages.Enqueue(new Tuple<string, bool, LogViewerViewModel.MessageType>(line, carriageReturnActive,
                      LogViewerViewModel.MessageType.Regular));
                  }
                  carriageReturnActive = false;
                  line = "";
                  break;
                default:
                  line += new string(c);
                  break;
              }
            }
          }
        }
      }
      catch (ThreadAbortException e)
      {
        // swallow
      }
      catch (Exception ex)
      {
        MessageBox.Show("_readOutput:" + ex.Message);
      }
    }

    private void _readError()
    {
      try
      {
        var line = "";
        var carriageReturnActive = false;
        while (true)
        {
          if (_error == null || _output.EndOfStream)
          {
            Thread.Sleep(1000);
          }

          while (_error != null && !_error.EndOfStream)
          {
            while (_error.Peek() >= 0)
            {
              var c = new char[1];
              _error.Read(c, 0, c.Length);
              switch (c[0])
              {
                case '\r':
                  if (line.Length > 0)
                  {
                    _messages.Enqueue(new Tuple<string, bool, LogViewerViewModel.MessageType>(line, carriageReturnActive, LogViewerViewModel.MessageType.Error));
                  }
                  carriageReturnActive = true;
                  line = "";
                  break;
                case '\n':
                  if (line.Length > 0)
                  {
                    _messages.Enqueue(new Tuple<string, bool, LogViewerViewModel.MessageType>(line, carriageReturnActive, LogViewerViewModel.MessageType.Error));
                  }
                  carriageReturnActive = false;
                  line = "";
                  break;
                default:
                  line += new string(c);
                  break;
              }
            }
          }
        }
      }
      catch (ThreadAbortException e)
      {
        // swallow
      }
      catch (Exception ex)
      {
        MessageBox.Show("_readError: " + ex.Message);
      }
    }

    #endregion Private Methods

    #region Public Methods

    public void WriteToMessageQueue(string line, LogViewerViewModel.MessageType messageType = LogViewerViewModel.MessageType.Regular)
    {
      _messages.Enqueue(new Tuple<string, bool, LogViewerViewModel.MessageType>(line, false, messageType));
    }

    public void ResetStreams()
    {
      _output = null;
      _error = null;
    }

    public void UpdateStreams(StreamReader output, StreamReader error)
    {
      _output = output;
      _error = error;
    }

    private void StartAllThreads()
    {
      if (_outputThread == null || !_outputThread.IsAlive)
      {
        _outputThread = new Thread(_readOutput) { Name = "LogHandler Output", IsBackground = true };
        _outputThread.Start();
      }

      if (_errorThread == null || !_errorThread.IsAlive)
      {
        _errorThread = new Thread(_readError) { Name = "LogHandler Error", IsBackground = true };
        _errorThread.Start();
      }

      if (_messagesThread == null || !_messagesThread.IsAlive)
      {
        _messagesThread = new Thread(UpdateFuncs) { Name = "LogHandler Messages", IsBackground = true };
        _messagesThread.Start();
      }
    }

    public void StopAllThreads()
    {
      try
      {
        if (_errorThread != null && _errorThread.IsAlive)
        {
          _errorThread.Abort();
        }
        _error = null;

        if (_outputThread != null && _outputThread.IsAlive)
        {
          _outputThread.Abort();
        }
        _output = null;

        if (_messagesThread != null && _messagesThread.IsAlive)
        {
          _messagesThread.Abort();
        }

      }
      catch (ThreadAbortException tea)
      {
        // The prcess got ended
      }
      catch (Exception ex)
      {
        MessageBox.Show("StopAllThreads: " + ex.Message);
      }
    }

    public void RemoveListener(Action<string, bool, LogViewerViewModel.MessageType> func)
    {
      if (func == null)
      {
        MessageBox.Show("func is null");
        return;
      }

      lock (_callbacks)
      {
        if (_callbacks.Contains(func))
        {
          _callbacks.Remove(func);
        }
      }
    }

    public void RemovePrcListener(Action<int> func)
    {
      if (func == null)
      {
        MessageBox.Show("PRC func is null");
        return;
      }
      lock (_prccallbacks)
      {
        if (_prccallbacks.Contains(func))
        {
          _prccallbacks.Remove(func);
        }
      }
    }

    public void AddListener(Action<string, bool, LogViewerViewModel.MessageType> func)
    {
      if (func == null)
      {
        MessageBox.Show("func is null");
        return;
      }
      lock (_callbacks)
      {
        if (!_callbacks.Contains(func))
        {
          _callbacks.Add(func);
        }
      }
    }

    public void AddPrcListener(Action<int> func)
    {
      if (func == null)
      {
        MessageBox.Show("PRC func is null");
        return;
      }
      lock (_prccallbacks)
      {
        if (!_prccallbacks.Contains(func))
        {
          _prccallbacks.Add(func);
        }
      }
    }

    #endregion Public Methods

    private static string getFormattedTime()
    {
      return DateTime.UtcNow.ToString("HH:mm:ss.fff") + ": ";
    }
  }
}
