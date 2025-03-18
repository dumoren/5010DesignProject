using Avalonia.Controls;

namespace FinanceTracker.Views;

public partial class AddCategoryWindow : Window
{
    public string? NewCategory { get; private set; }

    public AddCategoryWindow()
    {
        InitializeComponent();
    }

    private void OnConfirm(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var categoryInput = this.FindControl<TextBox>("CategoryInput").Text;
        if (!string.IsNullOrWhiteSpace(categoryInput))
        {
            NewCategory = categoryInput.Trim();
            Close();
        }
            
    }

    private void OnCancel(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NewCategory = null;
        Close();
    }
}
