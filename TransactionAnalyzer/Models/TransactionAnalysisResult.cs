using Transaction;

namespace TransactionAnalyzer.Models;

public class TransactionAnalysisResult
{
    public Dictionary<Currency, CurrencyAnalysis> CurrencyAnalyses { get; set; } = new();
    public int TotalTransactionCount { get; set; }
    public int FilteredTransactionCount { get; set; }
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class CurrencyAnalysis
{
    public Currency Currency { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalInflow { get; set; }
    public decimal TotalOutflow { get; set; }
    public decimal NetAmount { get; set; }
    public decimal TotalFees { get; set; }

    public List<MonthlyAnalysis> MonthlyAnalyses { get; set; } = new();
    public List<BalanceTracking> BalanceHistory { get; set; } = new();
    public List<TransactionTypeAnalysis> TransactionTypeAnalyses { get; set; } = new();
    public List<TransactionTypeAnalysis> LargestTransactionsByType { get; set; } = new();
    public List<YearlyAnalysis> YearlyAnalyses { get; set; } = new();
    public List<CounterpartyAnalysis> TopCounterparties { get; set; } = new();
    public Dictionary<string, List<CounterpartyTransactionTypeAnalysis>> CounterpartiesByTransactionType { get; set; } = new();

    public decimal AverageMonthlyIncome { get; set; }
    public decimal MaxMonthlyIncome { get; set; }
    public decimal MinMonthlyIncome { get; set; }
    public MonthlyAnalysis? BestIncomeMonth { get; set; }
    public MonthlyAnalysis? WorstIncomeMonth { get; set; }
}

public class MonthlyAnalysis
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetIncome => Income - Expenses;
    public int TransactionCount { get; set; }
    public decimal AverageTransactionSize => TransactionCount > 0 ? (Income + Expenses) / TransactionCount : 0;
}

public class BalanceTracking
{
    public DateTime Date { get; set; }
    public decimal Balance { get; set; }
    public string FormattedDate => Date.ToString("yyyy-MM-dd");
}

public class TransactionTypeAnalysis
{
    public string TransactionType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount => Count > 0 ? TotalAmount / Count : 0;
    public decimal LargestAmount { get; set; }
    public FibTransaction? LargestTransaction { get; set; }
}

public class YearlyAnalysis
{
    public int Year { get; set; }
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetIncome => Income - Expenses;
    public int TransactionCount { get; set; }
}

public class CounterpartyAnalysis
{
    public string Counterparty { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount => TransactionCount > 0 ? TotalAmount / TransactionCount : 0;
    public decimal AmountSent { get; set; }
    public decimal AmountReceived { get; set; }
    public decimal NetAmount => AmountReceived - AmountSent;
}

public class CounterpartyTransactionTypeAnalysis
{
    public string Counterparty { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount => Count > 0 ? TotalAmount / Count : 0;
}