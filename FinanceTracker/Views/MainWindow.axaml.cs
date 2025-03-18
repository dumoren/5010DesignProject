using Avalonia.Controls;
using FinanceTracker.ViewModels;

namespace FinanceTracker.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;
    }
}
