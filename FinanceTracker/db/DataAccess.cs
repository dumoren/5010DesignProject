using Dapper;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DataAccess
{
    private const string ConnectionString = "Data Source=db/finance.db";

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
        await conn.ExecuteAsync("INSERT INTO Budgets (Month, Year, Amount) VALUES (@Month, @Year, @Amount)",
            new { Month = month, Year = year, Amount = amount });
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
        await conn.ExecuteAsync("INSERT INTO Transactions (Date, Description, Category, Amount) VALUES (@Date, @Description, @Category, @Amount)",
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
}

public class Transaction
{
    public int Id { get; set; }
    public required string Date { get; set; }
    public required string Description { get; set; }
    public required string Category { get; set; }
    public double Amount { get; set; }
}
