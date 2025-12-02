using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Zira.ViewModels;

namespace Zira.Views;

public partial class LoginView : UserControl<LoginViewModel>
{
    public LoginView()
    {
        InitializeComponent();
    }
}
