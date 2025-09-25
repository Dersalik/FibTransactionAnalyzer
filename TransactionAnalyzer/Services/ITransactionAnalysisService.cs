using Transaction;
using TransactionAnalyzer.Models;

namespace TransactionAnalyzer.Models;

public interface ITransactionAnalysisService
{
    Task<TransactionAnalysisResult> AnalyzeTransactionsAsync(IEnumerable<FibTransaction> transactions, Boolean ignoreInternalTransactions);
}