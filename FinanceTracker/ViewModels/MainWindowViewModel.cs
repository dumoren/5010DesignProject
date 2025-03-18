using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace FinanceTracker.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly DataAccess _dataAccess = new();

        [ObservableProperty]
        private string _budgetInput = "";

        [ObservableProperty]
        private string _budgetDisplay = "No budget set";

        public MainWindowViewModel()
        {
            LoadCurrentBudget();
        }

        [RelayCommand]
        private async Task SetBudget()
        {
            if (double.TryParse(BudgetInput, out double budgetAmount))
            {
                string month = DateTime.Now.ToString("MM");
                int year = DateTime.Now.Year;

                await _dataAccess.SetMonthlyBudget(month, year, budgetAmount);
                BudgetDisplay = $"Budget for {month}/{year}: ${budgetAmount}";

                BudgetInput = ""; // Clear input field
            }
            else
            {
                BudgetDisplay = "Invalid budget amount";
            }
        }

        private async void LoadCurrentBudget()
        {
            string month = DateTime.Now.ToString("MM");
            int year = DateTime.Now.Year;
            double currentBudget = await _dataAccess.GetCurrentBudget(month, year);

            if (currentBudget > 0)
                BudgetDisplay = $"Budget for {month}/{year}: ${currentBudget}";
            else
                BudgetDisplay = "No budget set";
        }
    }
}
