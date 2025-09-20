using CsvHelper;
using System.Globalization;

namespace Transaction;

public class TransactionReader : ITransactionReader
{
    public async Task<IEnumerable<FibTransaction>> ReadTransactionsAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        using var reader = new StreamReader(filePath);
        return await ReadTransactionsFromReaderAsync(reader);
    }

    public async Task<IEnumerable<FibTransaction>> ReadTransactionsAsync(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        using var reader = new StreamReader(stream);
        return await ReadTransactionsFromReaderAsync(reader);
    }

    private static async Task<IEnumerable<FibTransaction>> ReadTransactionsFromReaderAsync(TextReader reader)
    {
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = new List<FibTransaction>();

        await foreach (var record in csv.GetRecordsAsync<FibTransaction>())
        {
            records.Add(record);
        }
        return records;
    }
}
