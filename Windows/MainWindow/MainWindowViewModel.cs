using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BTraderWPF.Utilities;
using BTraderWPF.Windows.ResearchWindow;

namespace BTraderWPF.Windows.MainWindow
{
  public class MainWindowViewModel : INotifyPropertyChanged
  {
    private readonly MainWindow _mainWindow;
    private ResearchViewModel _researchViewModel;

    public ICommand ResearchCommand { get; set; }

    public MainWindowViewModel()
    {
      _mainWindow = new MainWindow(this);

      ResearchCommand = new RelayCommand(ExecuteResearchButton, o => true);
    }

    public void ShowWindow()
    {
      if (_mainWindow != null)
      {
        _mainWindow.Show();
      }
    }

    #region Command Handlers

    private void ExecuteResearchButton(object o)
    {
      if (_researchViewModel == null) _researchViewModel = new ResearchViewModel();

      _researchViewModel.ShowWindow();
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
