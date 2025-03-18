using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FinanceTracker.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly DataAccess _dataAccess = new();

        // Budget-related properties
        [ObservableProperty]
        private string _budgetInput = "";

        [ObservableProperty]
        private string _budgetDisplay = "No budget set";

        // Expense-related properties
        [ObservableProperty]
        private string _expenseDesc = "";

        [ObservableProperty]
        private string _expenseAmount = "";

        [ObservableProperty]
        private string _selectedCategory = "Add New";  // Default to "Add New"

        public ObservableCollection<string> Categories { get; } = new();

        public MainWindowViewModel()
        {
            LoadCurrentBudget();
            LoadCategories();
        }

        // Load current budget on app startup
        private async void LoadCurrentBudget()
        {
            string month = DateTime.Now.ToString("MM");
            int year = DateTime.Now.Year;
            double currentBudget = await _dataAccess.GetCurrentBudget(month, year);

            BudgetDisplay = currentBudget > 0 ? 
                $"Budget for {month}/{year}: ${currentBudget}" : "No budget set";
        }

        // Load categories on app startup
        private async void LoadCategories()
        {
            Categories.Clear();
            var dbCategories = await _dataAccess.GetCategories();
            
            foreach (var category in dbCategories)
                Categories.Add(category);

            Categories.Add("Add New"); // Ensure "Add New" option is always last
        }

        // Set budget command
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

        // Add expense transaction
        [RelayCommand]
        private async Task AddTransaction()
        {
            if (SelectedCategory == "Add New")
            {
                var newCategory = PromptForNewCategory();
                if (!string.IsNullOrWhiteSpace(newCategory))
                {
                    await _dataAccess.AddCategory(newCategory);
                    LoadCategories(); // Refresh categories
                    SelectedCategory = newCategory;
                }
                return; // Wait for the new category before proceeding
            }

            if (double.TryParse(ExpenseAmount, out double amount) && amount > 0)
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");

                await _dataAccess.AddTransaction(date, ExpenseDesc, SelectedCategory, amount);

                // Clear input fields
                ExpenseDesc = "";
                ExpenseAmount = "";
            }
        }

        // Placeholder for a UI popup to add a new category
        private string PromptForNewCategory()
        {
            return "User Inputted Category"; // Replace with a real UI prompt
        }
    }
}
