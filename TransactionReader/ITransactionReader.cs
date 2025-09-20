namespace Transaction;

public interface ITransactionReader
{
    Task<IEnumerable<FibTransaction>> ReadTransactionsAsync(string filePath);
    Task<IEnumerable<FibTransaction>> ReadTransactionsAsync(Stream stream);
}
