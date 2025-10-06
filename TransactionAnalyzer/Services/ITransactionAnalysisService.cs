using Transaction;

namespace TransactionAnalyzer.Models;

public interface ITransactionAnalysisService
{
    TransactionAnalysisResult Analyze(IEnumerable<FibTransaction> transactions, Boolean ignoreInternalTransactions, DateTime dateFrom, DateTime dateTo);
}