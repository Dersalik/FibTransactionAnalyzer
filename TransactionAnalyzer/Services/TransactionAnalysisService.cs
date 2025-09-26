using Transaction;
using TransactionAnalyzer.Models;

namespace TransactionAnalyzer.Services;

public class TransactionAnalysisService : ITransactionAnalysisService
{
    public async Task<TransactionAnalysisResult> AnalyzeTransactionsAsync(IEnumerable<FibTransaction> transactions, Boolean ignoreInternalTransactions, DateTime dateFrom, DateTime dateTo)
    {
        return await Task.Run(() => AnalyzeTransactions(transactions, ignoreInternalTransactions, dateFrom, dateTo));
    }

    private TransactionAnalysisResult AnalyzeTransactions(IEnumerable<FibTransaction> transactions, Boolean ignoreInternalTransactions, DateTime dateFrom, DateTime dateTo)
    {
        var allTransactions = transactions.ToList();

        var filteredTransactions = allTransactions
            .Where(t => t.Date >= dateFrom) 
            .Where(t => t.Date <= dateTo)
            .Where(t => !ignoreInternalTransactions || t.TransactionType != "MONEY_BOX_TRANSFER") 
            .ToList();

        var result = new TransactionAnalysisResult
        {
            TotalTransactionCount = allTransactions.Count,
            FilteredTransactionCount = filteredTransactions.Count
        };

        var currencyGroups = filteredTransactions.GroupBy(t => t.Amount.Currency);

        foreach (var currencyGroup in currencyGroups)
        {
            var currencyTransactions = currencyGroup.ToList();
            var analysis = AnalyzeCurrency(currencyGroup.Key, currencyTransactions);
            result.CurrencyAnalyses[currencyGroup.Key] = analysis;
        }

        return result;
    }

    private CurrencyAnalysis AnalyzeCurrency(Currency currency, List<FibTransaction> transactions)
    {
        var analysis = new CurrencyAnalysis
        {
            Currency = currency,
            TransactionCount = transactions.Count
        };

        CalculateBasicMetrics(analysis, transactions);

        analysis.MonthlyAnalyses = CalculateMonthlyAnalysis(transactions);
        analysis.BalanceHistory = CalculateBalanceHistory(transactions);
        analysis.YearlyAnalyses = CalculateYearlyAnalysis(transactions);

        analysis.TransactionTypeAnalyses = CalculateTransactionTypeAnalysis(transactions);
        analysis.LargestTransactionsByType = CalculateLargestTransactionsByType(transactions);

        analysis.TopCounterparties = CalculateTopCounterparties(transactions);
        analysis.CounterpartiesByTransactionType = CalculateCounterpartiesByTransactionType(transactions);

        CalculateStatistics(analysis);

        return analysis;
    }

    private void CalculateBasicMetrics(CurrencyAnalysis analysis, List<FibTransaction> transactions)
    {
        analysis.TotalInflow = transactions.Where(t => t.Amount.Amount > 0).Sum(t => t.Amount.Amount);
        analysis.TotalOutflow = transactions.Where(t => t.Amount.Amount < 0).Sum(t => Math.Abs(t.Amount.Amount));
        analysis.NetAmount = transactions.Sum(t => t.Amount.Amount);
        analysis.TotalFees = transactions.Sum(t => t.Fee.Amount);
    }

    private List<MonthlyAnalysis> CalculateMonthlyAnalysis(List<FibTransaction> transactions)
    {
        return transactions
            .Where(t => t.Date != DateTime.MinValue) // Filter out invalid dates
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new MonthlyAnalysis
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Income = g.Where(t => t.Amount.Amount > 0).Sum(t => t.Amount.Amount),
                Expenses = g.Where(t => t.Amount.Amount < 0).Sum(t => Math.Abs(t.Amount.Amount)),
                TransactionCount = g.Count()
            })
            .OrderByDescending(m => m.Year)
            .ThenByDescending(m => m.Month)
            .ToList();
    }

    private List<BalanceTracking> CalculateBalanceHistory(List<FibTransaction> transactions)
    {
        // Sort transactions by date and time for accurate balance tracking
        var sortedTransactions = transactions
            .Where(t => t.Date != DateTime.MinValue)
            .OrderBy(t => t.Date)
            .ThenBy(t => t.Time)
            .ToList();

        var balanceHistory = new List<BalanceTracking>();

        foreach (var transaction in sortedTransactions)
        {
            if (transaction.BalanceAfter.Amount != 0)
            {
                balanceHistory.Add(new BalanceTracking
                {
                    Date = transaction.Date.Add(transaction.Time),
                    Balance = transaction.BalanceAfter.Amount
                });
            }
        }

        if (!balanceHistory.Any() && sortedTransactions.Any())
        {
            decimal runningBalance = 0;
            foreach (var transaction in sortedTransactions)
            {
                runningBalance += transaction.Amount.Amount;
                balanceHistory.Add(new BalanceTracking
                {
                    Date = transaction.Date.Add(transaction.Time),
                    Balance = runningBalance
                });
            }
        }

        return balanceHistory.OrderBy(b => b.Date).ToList();
    }

    private List<YearlyAnalysis> CalculateYearlyAnalysis(List<FibTransaction> transactions)
    {
        return transactions
            .Where(t => t.Date != DateTime.MinValue)
            .GroupBy(t => t.Date.Year)
            .Select(g => new YearlyAnalysis
            {
                Year = g.Key,
                Income = g.Where(t => t.Amount.Amount > 0).Sum(t => t.Amount.Amount),
                Expenses = g.Where(t => t.Amount.Amount < 0).Sum(t => Math.Abs(t.Amount.Amount)),
                TransactionCount = g.Count()
            })
            .OrderByDescending(y => y.Year)
            .ToList();
    }

    private List<TransactionTypeAnalysis> CalculateTransactionTypeAnalysis(List<FibTransaction> transactions)
    {
        return transactions
            .GroupBy(t => t.TransactionType)
            .Select(g => new TransactionTypeAnalysis
            {
                TransactionType = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(t => Math.Abs(t.Amount.Amount)),
                LargestAmount = g.Max(t => Math.Abs(t.Amount.Amount))
            })
            .OrderByDescending(t => t.Count)
            .ToList();
    }

    private List<TransactionTypeAnalysis> CalculateLargestTransactionsByType(List<FibTransaction> transactions)
    {
        return transactions
            .GroupBy(t => t.TransactionType)
            .Select(g =>
            {
                var largestTransaction = g.OrderByDescending(t => Math.Abs(t.Amount.Amount)).First();
                return new TransactionTypeAnalysis
                {
                    TransactionType = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(t => Math.Abs(t.Amount.Amount)),
                    LargestAmount = Math.Abs(largestTransaction.Amount.Amount),
                    LargestTransaction = largestTransaction
                };
            })
            .OrderByDescending(t => t.LargestAmount)
            .ToList();
    }

    private List<CounterpartyAnalysis> CalculateTopCounterparties(List<FibTransaction> transactions)
    {
        return transactions
            .Where(t => !string.IsNullOrEmpty(t.Counterparty))
            .GroupBy(t => t.Counterparty)
            .Select(g => new CounterpartyAnalysis
            {
                Counterparty = g.Key,
                TransactionCount = g.Count(),
                TotalAmount = g.Sum(t => Math.Abs(t.Amount.Amount)),
                AmountSent = g.Where(t => t.Amount.Amount < 0).Sum(t => Math.Abs(t.Amount.Amount)),
                AmountReceived = g.Where(t => t.Amount.Amount > 0).Sum(t => Math.Abs(t.Amount.Amount))
            })
            .ToList();
    }

    private Dictionary<string, List<CounterpartyTransactionTypeAnalysis>> CalculateCounterpartiesByTransactionType(List<FibTransaction> transactions)
    {
        return transactions
            .Where(t => !string.IsNullOrEmpty(t.Counterparty))
            .GroupBy(t => t.TransactionType)
            .ToDictionary(
                typeGroup => typeGroup.Key,
                typeGroup => typeGroup
                    .GroupBy(t => t.Counterparty)
                    .Select(counterpartyGroup => new CounterpartyTransactionTypeAnalysis
                    {
                        Counterparty = counterpartyGroup.Key,
                        TransactionType = typeGroup.Key,
                        Count = counterpartyGroup.Count(),
                        TotalAmount = counterpartyGroup.Sum(t => Math.Abs(t.Amount.Amount))
                    })
                    .OrderByDescending(c => c.TotalAmount)
                    .ToList()
            );
    }

    private void CalculateStatistics(CurrencyAnalysis analysis)
    {
        if (analysis.MonthlyAnalyses.Any())
        {
            var incomes = analysis.MonthlyAnalyses.Select(m => m.Income).Where(i => i > 0).ToList();

            if (incomes.Any())
            {
                analysis.AverageMonthlyIncome = incomes.Average();
                analysis.MaxMonthlyIncome = incomes.Max();
                analysis.MinMonthlyIncome = incomes.Min();

                analysis.BestIncomeMonth = analysis.MonthlyAnalyses
                    .Where(m => m.Income > 0)
                    .OrderByDescending(m => m.Income)
                    .FirstOrDefault();

                analysis.WorstIncomeMonth = analysis.MonthlyAnalyses
                    .Where(m => m.Income > 0)
                    .OrderBy(m => m.Income)
                    .FirstOrDefault();
            }
        }
    }
}