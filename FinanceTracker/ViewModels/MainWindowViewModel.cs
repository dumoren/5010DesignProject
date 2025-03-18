using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FinanceTracker.Views;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

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

        [ObservableProperty]
        private Transaction? _selectedTransaction;

        public ObservableCollection<string> Categories { get; } = new();

        public ObservableCollection<Transaction> Transactions { get; } = new();

        public MainWindowViewModel()
        {
            LoadCurrentBudget();
            LoadCategories();
            LoadTransactions();
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

        private async void LoadTransactions()
        {
            string month = DateTime.Now.ToString("MM");
            int year = DateTime.Now.Year;
            var transactions = await _dataAccess.GetTransactions(month, year);

            Transactions.Clear();
            Console.WriteLine("wowwwwzerrrrrrrrs"); // Debugging output
           
            foreach (var transaction in transactions)
            {
                
                Transactions.Add(transaction);
            }
        }

        // Add expense transaction
        [RelayCommand]
        private async Task AddTransaction()
        {
            Console.WriteLine($"Attempting to save transaction: {ExpenseDesc}, {SelectedCategory}, {ExpenseAmount}");

            if (SelectedCategory == "Add New")
            {
                var newCategory = await PromptForNewCategory();
                if (!string.IsNullOrWhiteSpace(newCategory))
                {
                    Console.WriteLine($"Adding new category: {newCategory}");
                    await _dataAccess.AddCategory(newCategory);
                    LoadCategories();
                    SelectedCategory = newCategory;
                }
                return; // Stop here until a valid category is chosen
            }

            if (double.TryParse(ExpenseAmount, out double amount) && amount > 0)
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                Console.WriteLine($"Final transaction values: {date}, {ExpenseDesc}, {SelectedCategory}, {amount}");

                await _dataAccess.AddTransaction(date, ExpenseDesc, SelectedCategory, amount);

                Console.WriteLine("Transaction added successfully!");

                LoadTransactions(); // Refresh list

                // Clear input fields
                ExpenseDesc = "";
                ExpenseAmount = "";
            }
            else
            {
                Console.WriteLine("Invalid amount entered.");
            }
        }


        [RelayCommand]
        private async Task DeleteTransaction(int transactionId)
        {
            await _dataAccess.DeleteTransaction(transactionId);
            LoadTransactions();
        }

        // Placeholder for a UI popup to add a new category
        private async Task<string> PromptForNewCategory()
        {
            var categoryWindow = new AddCategoryWindow();

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                await categoryWindow.ShowDialog(desktopLifetime.MainWindow);
            }

            return !string.IsNullOrWhiteSpace(categoryWindow.NewCategory) ? categoryWindow.NewCategory : "";
        }
    }
}
