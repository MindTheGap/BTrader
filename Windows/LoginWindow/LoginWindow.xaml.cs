using System.Windows;

namespace BTraderWPF.Windows.LoginWindow
{
  /// <summary>
  /// Interaction logic for LoginWindow.xaml
  /// </summary>
  public partial class LoginWindow : Window
  {
    private LoginViewModel _loginViewModel;

    public LoginWindow()
    {
      InitializeComponent();

      _loginViewModel = new LoginViewModel(this);

      DataContext = _loginViewModel;
    }
  }
}
