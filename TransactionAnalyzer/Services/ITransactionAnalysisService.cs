using Transaction;

namespace TransactionAnalyzer.Models;

public interface ITransactionAnalysisService
{
    Task<TransactionAnalysisResult> AnalyzeTransactionsAsync(IEnumerable<FibTransaction> transactions, Boolean ignoreInternalTransactions, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken = default);
}