using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using BTraderWPF.Utilities;
using BTraderWPF.Windows.LogViewerWindow;

namespace BTraderWPF.Windows.ResearchWindow
{
  public class ResearchViewModel : INotifyPropertyChanged
  {
    #region Private Fields

    private readonly ResearchWindow _researchWindow;
    private const string ConfigFile = @"Configs\research.xml";
    private bool _running;
    private readonly Dispatcher _uiDispatcher;
    private Thread _workerThread;
    private List<Step> _workingSteps; 

    #endregion

    #region Ctor

    public ResearchViewModel()
    {
      _researchWindow = new ResearchWindow(this);
      PipeManager = new PipeManager();

      _uiDispatcher = Dispatcher.CurrentDispatcher;
      LogViewerViewModel = new LogViewerViewModel("Research", PipeManager, _uiDispatcher);

      ParseConfigFile();

      StartDate = EndDate = new DateTime(2014, 1, 1);

      _startRelayCommand = new RelayCommand(ExecuteStartButton, o => true);
      _stopRelayCommand = new RelayCommand(ExecuteStopButton, o => true);
      StartButtonCommand = _startRelayCommand;
      ShowLogViewerCommand = new RelayCommand(ExecuteShowLogViewer, o => true);
      ExitCommand = new RelayCommand(ExecuteExit, o => true);
      ReloadConfigCommand = new RelayCommand(ExecuteReloadConfig, o => true);
    }

    #endregion

    #region Parsing Methods

    private void ParseConfigFile()
    {
      if (!File.Exists(ConfigFile))
      {
        MessageBox.Show(string.Format("Couldn't find file {0}", ConfigFile));
        return;
      }

      var document = XDocument.Load(ConfigFile);
      var configurationElement = document.Element("Configuration");
      if (configurationElement == null)
      {
        MessageBox.Show("Couldn't find 'Configuration' section in research.xml file");
        return;
      }

      var globalsElement = configurationElement.Element("Globals");
      if (globalsElement == null)
      {
        MessageBox.Show("Couldn't find 'Globals' section in research.xml file");
        return;
      }

      var globalArgsElement = configurationElement.Element("GlobalArgs");
      if (globalArgsElement == null)
      {
        MessageBox.Show("Couldn't find 'GlobalArgs' section in research.xml file");
        return;
      }

      var stepsElement = configurationElement.Element("Steps");
      if (stepsElement == null)
      {
        MessageBox.Show("Couldn't find 'Steps' section in research.xml file");
        return;
      }

      var processPathAttribute = configurationElement.Element("processPath");
      if (processPathAttribute == null)
      {
        MessageBox.Show("Couldn't find 'processPath' attribute in research.xml file");
        return;
      }

      ProcessPath = processPathAttribute.Value;
      if (!File.Exists(ProcessPath))
      {
        MessageBox.Show(string.Format("No such file as {0}", ProcessPath));
        return;
      }

      var globals = ParseGlobals(globalsElement.Elements());
      var globalArgs = ParseElementsAndEmbedGlobals(globalArgsElement.Elements(), globals);
      var stepsClean = ParseElementsAndEmbedGlobals(stepsElement.Elements(), globals);

      // aggregate global args
      var globalArgsList = new List<String>();
      foreach (var globalArg in globalArgs)
      {
        globalArgsList.Add(globalArg.Key);
        globalArgsList.Add(globalArg.Value);
      }
      var globalArgsAggregated = string.Join(" ", globalArgsList);

      // concatnate global args to each step
      var steps = new Dictionary<string, string>(stepsClean);
      foreach (var keyValuePair in stepsClean)
      {
        var stepName = keyValuePair.Key;
        var args = keyValuePair.Value;

        steps[stepName] = args + " " + globalArgsAggregated;
      }

      // alloc new steps
      Steps = new ObservableCollection<Step>();
      foreach (var keyValuePair in steps)
      {
        var stepName = keyValuePair.Key;
        var args = keyValuePair.Value;

        var argumentsDic = Utils.ArgsToDic(args);

        var newStep = new Step(this) { Name = stepName, ArgumentsDic = argumentsDic };
        Steps.Add(newStep);
      }
    }

    private Dictionary<string, string> ParseElementsAndEmbedGlobals(IEnumerable<XElement> elements,
      Dictionary<string, string> globals)
    {
      var parsedElements = GetDictionaryFromElements(elements);

      var result = new Dictionary<string, string>(parsedElements);
      foreach (var keyValuePair in parsedElements)
      {
        var key = keyValuePair.Key;

        foreach (var global in globals)
        {
          result[key] = result[key].Replace(global.Key, global.Value);
        }
      }

      return result;
    }

    private Dictionary<string, string> ParseGlobals(IEnumerable<XElement> globals)
    {
      var parsedElements = GetDictionaryFromElements(globals);

      var result = new Dictionary<string, string>(parsedElements);
      for (var i = 0; i < 5; i++)
      {
        var temp = new Dictionary<string, string>(result);

        foreach (var keyValuePair in temp)
        {
          var key = keyValuePair.Key;
          var value = keyValuePair.Value;

          if (!value.Contains("%"))
          {
            foreach (var keyValuePair2 in temp)
            {
              if (keyValuePair2.Key != key)
              {
                result[keyValuePair2.Key] = result[keyValuePair2.Key].Replace(key, value);
              }
            }
          }
        }
      }

      return result;
    }

    private static Dictionary<string, string> GetDictionaryFromElements(IEnumerable<XElement> elements)
    {
      var result = new Dictionary<string, string>();

      foreach (var element in elements)
      {
        var keyAttribute = element.Attribute("key");
        var valueAttribute = element.Attribute("value");
        if (keyAttribute == null || valueAttribute == null)
        {
          MessageBox.Show("Could not parse key/value for some global element");
          return null;
        }

        result.Add(keyAttribute.Value, valueAttribute.Value);
      }

      return result;
    }

    #endregion

    #region Private Methods

    public void ChangeStartButton(ButtonState toState)
    {
      if (Dispatcher.CurrentDispatcher != _uiDispatcher)
      {
        _uiDispatcher.Invoke(() => ChangeStartButton_aux(toState));
      }
      else
      {
        ChangeStartButton_aux(toState);
      }
    }

    private void ChangeStartButton_aux(ButtonState toState)
    {
      switch (toState)
      {
        case ButtonState.Start:
          StartButtonText = "Start";
          StartButtonEnabled = true;
          StartButtonCommand = _startRelayCommand;
          break;
        case ButtonState.Starting:
          StartButtonText = "Starting";
          StartButtonEnabled = false;
          break;
        case ButtonState.Stop:
          StartButtonText = "Stop";
          StartButtonEnabled = true;
          StartButtonCommand = _stopRelayCommand;
          break;
        case ButtonState.Stopping:
          StartButtonText = "Stopping";
          StartButtonEnabled = false;
          break;
      }
    }

    #endregion

    #region Public Methods

    public void WaitForAllStreamsToFinish(int waitInterval = 200)
    {
      var counter = 0;
      var finished = false;
      while (counter < 50)
      {
        finished = PipeManager.AllStreamsFinished();
        if (finished) break;

        counter++;
        Thread.Sleep(waitInterval);
      }

      if (!finished)
      {
        PipeManager.WriteToMessageQueue("All streams did not finish in a timely manner!", LogViewerViewModel.MessageType.Error);
      }
    }

    public void ShowWindow()
    {
      if (_researchWindow != null)
      {
        _researchWindow.Show();
      }
    }

    #endregion

    #region Public Properties

    public PipeManager PipeManager { get; set; }
    public LogViewerViewModel LogViewerViewModel { get; set; }

    public string ProcessPath { get; set; }

    public enum ButtonState
    {
      Start,
      Starting,
      Stop,
      Stopping
    }

    private ICommand _reloadConfigCommand;
    public ICommand ReloadConfigCommand
    {
      get { return _reloadConfigCommand; }
      set
      {
        _reloadConfigCommand = value;
        OnPropertyChanged("ReloadConfigCommand");
      }
    }

    private ICommand _exitCommand;
    public ICommand ExitCommand
    {
      get { return _exitCommand; }
      set
      {
        _exitCommand = value;
        OnPropertyChanged("ExitCommand");
      }
    }

    private bool _startButtonEnabled;
    public bool StartButtonEnabled
    {
      get { return _startButtonEnabled; }
      set
      {
        _startButtonEnabled = value;
        OnPropertyChanged("StartButtonEnabled");
      }
    }

    private string _startButtonText;
    public string StartButtonText
    {
      get { return _startButtonText; }
      set
      {
        _startButtonText = value;
        OnPropertyChanged("StartButtonText");
      }
    }

    private ObservableCollection<Step> _steps;
    public ObservableCollection<Step> Steps
    {
      get { return _steps; }
      set
      {
        _steps = value;
        OnPropertyChanged("Steps");
      }
    }

    private DateTime _startDate;
    public DateTime StartDate
    {
      get { return _startDate; }
      set
      {
        _startDate = value;
        OnPropertyChanged("StartDate");
      }
    }

    private DateTime _endDate;
    public DateTime EndDate
    {
      get { return _endDate; }
      set
      {
        _endDate = value;
        OnPropertyChanged("EndDate");
      }
    }

    private RelayCommand _startRelayCommand;
    private RelayCommand _stopRelayCommand;
    private ICommand _startButtonCommand;
    public ICommand StartButtonCommand
    {
      get { return _startButtonCommand; }
      set
      {
        _startButtonCommand = value;
        OnPropertyChanged("StartButtonCommand");
      }
    }

    private ICommand _showLogViewerCommand;
    public ICommand ShowLogViewerCommand
    {
      get { return _showLogViewerCommand; }
      set
      {
        _showLogViewerCommand = value;
        OnPropertyChanged("ShowLogViewerCommand");
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

    #region Command Handlers

    private void ExecuteStartButton(object o)
    {
      LogViewerViewModel.PrepareLog(StartDate, EndDate);

      RunSelectedSteps(Steps.ToList());
    }

    private void ExecuteStopButton(object obj)
    {
      if (_workingSteps != null)
      {
        _workingSteps.ForEach(s => s.ProcessEnded());
      }
    }

    private void ExecuteShowLogViewer(object obj)
    {
      LogViewerViewModel.ShowViewer();
    }

    public void RunStepsFromHere(Step step)
    {
      var steps = Steps.Skip(Steps.IndexOf(step)).ToList();
      RunSelectedSteps(steps);
    }

    public void RunSelectedSteps(List<Step> steps)
    {
      if (_workerThread.IsAlive)
      {
        MessageBox.Show("Worker thread is still alive");
        return;
      }

      RunWorkerThreadWithSteps(steps);
    }

    public void RunStep(Step step)
    {
      step.ArgumentsDic["-startDate"] = StartDate.YMD();
      step.ArgumentsDic["-endDate"] = EndDate.YMD();

      RunWorkerThreadWithSteps(new List<Step>(){step});
    }

    private void RunWorkerThreadWithSteps(List<Step> steps)
    {
      _workerThread = new Thread(() => _runStepsBackground(steps));
      _workerThread.Start();
      _workerThread.Join();
    }

    private void _runStepsBackground(List<Step> steps)
    {
      if (StartDate > DateTime.Today || EndDate > DateTime.Today)
      {
        MessageBox.Show("You can't run on future dates");
        return;
      }

      _workingSteps = steps;
      steps.ForEach(s => s.Progress = 0.0);

      _running = true;

      var watch = Stopwatch.StartNew();

      for (var stepIndex = 0; stepIndex < steps.Count; stepIndex++)
      {
        var step = steps[stepIndex];

        if (!step.Run())
        {
          _running = false;
          return;
        }
      }

      LogViewerViewModel.LogFile.Flush();

      _running = false;

      PipeManager.WriteToMessageQueue("Total running time is: " + watch.ElapsedMilliseconds / 1000.0 + " seconds");

      ChangeStartButton(ButtonState.Start);
      Utils.ShowMessageBoxSafely("Done!");
    }

    private void ExecuteReloadConfig(object obj)
    {
      ParseConfigFile();
    }

    private void ExecuteExit(object obj)
    {
      Environment.Exit(0);
    }

    #endregion
  }
}
