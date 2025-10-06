using Transaction;

namespace TransactionAnalyzer.Models;

public interface ITransactionAnalysisService
{
    Task<TransactionAnalysisResult> AnalyzeAsync(IEnumerable<FibTransaction> transactions, Boolean ignoreInternalTransactions, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken = default);

    TransactionAnalysisResult Analyze(IEnumerable<FibTransaction> transactions, Boolean ignoreInternalTransactions, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken = default);
}