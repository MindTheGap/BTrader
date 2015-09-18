using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BTraderWPF.Utilities;
using BTraderWPF.Windows.MainWindow;

namespace BTraderWPF.Windows.LoginWindow
{
  public class LoginViewModel : INotifyPropertyChanged
  {
    #region Private Fields

    private readonly LoginWindow _window;

    #endregion

    #region Public Binded Properties

    public ICommand LoginButtonCommand { get; set; }

    private string _user;

    public string User
    {
      get { return _user; }
      set
      {
        _user = value;
        OnPropertyChanged("User");
      }
    }

    private string _pass;

    public string Pass
    {
      get { return _pass; }
      set
      {
        _pass = value;
        OnPropertyChanged("Pass");
      }
    }

    #endregion

    public LoginViewModel(LoginWindow window)
    {
      _window = window;

      LoginButtonCommand = new RelayCommand(ExecuteLoginButton, o => true);
    }

    private void ExecuteLoginButton(object o)
    {
      // TODO: implement password checking with SHA1:
      //        http://stackoverflow.com/questions/26256351/encrypt-passwords-on-sql-server-2008-using-sha1

      var mainWindow = new MainWindowViewModel();
      mainWindow.ShowWindow();
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
  }
}
