using Transaction;

namespace TransactionAnalyzer.Models;

public interface ITransactionAnalysisService
{
    TransactionAnalysisResult Analyze(IEnumerable<FibTransaction> transactions, bool ignoreInternalTransactions, DateTime dateFrom, DateTime dateTo);
}