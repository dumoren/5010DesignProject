using Dapper;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DataAccess
{
    private const string ConnectionString = "Data Source=finance.db;Mode=ReadWriteCreate;";

    public DataAccess()
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS Budgets (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Month TEXT NOT NULL,
                Year INTEGER NOT NULL,
                Amount REAL NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Transactions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Date TEXT NOT NULL,
                Description TEXT NOT NULL,
                Category TEXT NOT NULL,
                Amount REAL NOT NULL
            );
        """);
    }

    public async Task SetMonthlyBudget(string month, int year, double amount)
    {
    using var conn = new SqliteConnection(ConnectionString);

    var existingBudget = await conn.QuerySingleOrDefaultAsync<double?>(
        "SELECT Amount FROM Budgets WHERE Month = @Month AND Year = @Year",
        new { Month = month, Year = year });

    if (existingBudget != null)
    {
        // Update existing budget
        await conn.ExecuteAsync(
            "UPDATE Budgets SET Amount = @Amount WHERE Month = @Month AND Year = @Year",
            new { Month = month, Year = year, Amount = amount });
    }
    else
    {
        // Insert new budget
        await conn.ExecuteAsync(
            "INSERT INTO Budgets (Month, Year, Amount) VALUES (@Month, @Year, @Amount)",
            new { Month = month, Year = year, Amount = amount });
    }
    }

    public async Task<double> GetCurrentBudget(string month, int year)
    {
        using var conn = new SqliteConnection(ConnectionString);
        return await conn.QuerySingleOrDefaultAsync<double>(
            "SELECT Amount FROM Budgets WHERE Month = @Month AND Year = @Year",
            new { Month = month, Year = year });
    }

    public async Task AddTransaction(string date, string description, string category, double amount)
    {
        using var conn = new SqliteConnection(ConnectionString);
        await conn.ExecuteAsync("INSERT INTO Transactions (Date, Description, Category, Amount) VALUES (DATE(@Date), @Description, @Category, @Amount)",
            new { Date = date, Description = description, Category = category, Amount = amount });
    }

    public async Task<double> GetTotalExpensesForMonth(string month, int year)
    {
        using var conn = new SqliteConnection(ConnectionString);
        var total = await conn.QuerySingleOrDefaultAsync<double>(
            @"SELECT IFNULL(SUM(Amount), 0) FROM Transactions
              WHERE strftime('%m', Date) = @Month AND strftime('%Y', Date) = @Year",
            new { Month = month.PadLeft(2, '0'), Year = year.ToString() });

        return total;
    }

    public async Task<IEnumerable<Transaction>> GetTransactions(string month, int year)
    {
        using var conn = new SqliteConnection(ConnectionString);
        return await conn.QueryAsync<Transaction>(
            @"SELECT * FROM Transactions
              WHERE strftime('%m', Date) = @Month AND strftime('%Y', Date) = @Year",
            new { Month = month.PadLeft(2, '0'), Year = year.ToString() });
    }
    // LOGIC TO GET REMAINING BUDGET, DO IT AFTER YOU DO EXPENSES
    // public async Task<double> GetRemainingBudget(string month, int year)
    // {
    // using var conn = new SqliteConnection(ConnectionString);

    // var budget = await GetCurrentBudget(month, year);
    // var expenses = await GetTotalExpensesForMonth(month, year);

    // return budget - expenses;
    // }
}

public class Transaction
{
    public int Id { get; set; }
    public string Date { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public double Amount { get; set; }
}
