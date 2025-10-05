using System.Text;
using Xunit;

namespace Transaction.Test.Integration;

public class TransactionReaderIntegrationTests : IDisposable
{
    private readonly TransactionReader _reader = new();
    private readonly List<string> _tempFiles;

    public TransactionReaderIntegrationTests()
    {
        _reader = new TransactionReader();
        _tempFiles = new List<string>();
    }

    [Fact]
    public async Task ReadTransactionsAsync_WithValidCsvFile_ReturnsAllTransactions()
    {
        // Arrange
        var csvContent = """
            ID,COUNTERPARTY,AMOUNT,FEE,BALANCE AFTER,TRANSACTION TYPE,DATE,TIME,STATUS,TRANSACTION ID,NOTE
            11111111-1111-1111-1111-111111111111,John Doe,100.50 USD,5.00 USD,1000.00 USD,TRANSFER,05/09/2023,3:13:12 PM,COMPLETED,7ffeee20-418d-4679-afd3-e37a58315e3f,Payment for services
            22222222-2222-2222-2222-222222222222,Jane Smith,250.75 EUR,2.50 EUR,1248.25 EUR,DEPOSIT,15/09/2023,10:30:45 AM,COMPLETED,7ffeee20-418d-4679-afd3-e37a58315e3f,Salary deposit
            33333333-3333-3333-3333-333333333333,Bob Wilson,50.00 IQD,1.00 IQD,1197.25 IQD,WITHDRAWAL,20/09/2023,2:15:30 PM,PENDING,7ffeee20-418d-4679-afd3-e37a58315e3f,ATM withdrawal
            """;

        var filePath = CreateTempFile(csvContent);

        // Act
        var transactions = await _reader.ReadTransactionsAsync(filePath);

        // Assert
        var transactionList = transactions.ToList();
        Assert.Equal(3, transactionList.Count);

        // Verify first transaction
        var firstTransaction = transactionList[0];
        Assert.Equal(new Guid("11111111-1111-1111-1111-111111111111"), firstTransaction.Id);
        Assert.Equal("John Doe", firstTransaction.Counterparty);
        Assert.Equal(new MonetaryValue(100.50m, Currency.USD), firstTransaction.Amount);
        Assert.Equal(new MonetaryValue(5.00m, Currency.USD), firstTransaction.Fee);
        Assert.Equal(new MonetaryValue(1000.00m, Currency.USD), firstTransaction.BalanceAfter);
        Assert.Equal("TRANSFER", firstTransaction.TransactionType);
        Assert.Equal(new DateTime(2023, 9, 5), firstTransaction.Date);
        Assert.Equal(new TimeSpan(15, 13, 12), firstTransaction.Time); // 3:13:12 PM = 15:13:12
        Assert.Equal("COMPLETED", firstTransaction.Status);
        Assert.Equal(Guid.Parse("7ffeee20-418d-4679-afd3-e37a58315e3f"), firstTransaction.TransactionId);
        Assert.Equal("Payment for services", firstTransaction.Note);

        // Verify second transaction has different currency
        var secondTransaction = transactionList[1];
        Assert.Equal(Currency.EUR, secondTransaction.Amount.Currency);
        Assert.Equal(250.75m, secondTransaction.Amount.Amount);

        // Verify third transaction is pending
        var thirdTransaction = transactionList[2];
        Assert.Equal("PENDING", thirdTransaction.Status);
        Assert.Equal(Currency.IQD, thirdTransaction.Amount.Currency);
    }

    [Fact]
    public async Task ReadTransactionsAsync_WithLargeCsvFile_HandlesPerformanceEfficiently()
    {
        // Arrange - Create a large CSV file with 1000 transactions
        var csvHeader = "ID,COUNTERPARTY,AMOUNT,FEE,BALANCE AFTER,TRANSACTION TYPE,DATE,TIME,STATUS,TRANSACTION ID,NOTE\n";
        var csvBuilder = new StringBuilder(csvHeader);

        for (int i = 1; i <= 1000; i++)
        {
            var guid = Guid.NewGuid();
            csvBuilder.AppendLine($"{guid},User{i},{i * 10}.50 USD,{i}.00 USD,{1000 + i}.00 USD,TRANSFER,{05 + (i % 25):D2}/09/2023,{(i % 12) + 1}:30:00 PM,COMPLETED,{Guid.NewGuid()},Note {i}");
        }

        var filePath = CreateTempFile(csvBuilder.ToString());

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var transactions = await _reader.ReadTransactionsAsync(filePath);
        stopwatch.Stop();

        // Assert
        var transactionList = transactions.ToList();
        Assert.Equal(1000, transactionList.Count);
        Assert.True(stopwatch.ElapsedMilliseconds < 7000, $"Reading 1000 transactions took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");

        // Verify first and last transactions are correctly parsed
        Assert.Equal("User1", transactionList[0].Counterparty);
        Assert.Equal("User1000", transactionList[999].Counterparty);
    }

    [Fact]
    public async Task ReadTransactionsAsync_WithMixedCurrencies_ParsesAllCorrectly()
    {
        // Arrange
        var csvContent = """
            ID,COUNTERPARTY,AMOUNT,FEE,BALANCE AFTER,TRANSACTION TYPE,DATE,TIME,STATUS,TRANSACTION ID,NOTE
            11111111-1111-1111-1111-111111111111,USD User,100.50 USD,5.00 USD,1000.00 USD,TRANSFER,05/09/2023,3:13:12 PM,COMPLETED,7ffeee20-418d-4679-afd3-e37a58315e3f,USD transaction
            22222222-2222-2222-2222-222222222222,EUR User,250.75 EUR,2.50 EUR,1248.25 EUR,TRANSFER,15/09/2023,10:30:45 AM,COMPLETED,7ffeee20-418d-4679-afd3-e37a58315e3f,EUR transaction
            33333333-3333-3333-3333-333333333333,IQD User,1000.00 IQD,10.00 IQD,5000.00 IQD,TRANSFER,20/09/2023,2:15:30 PM,COMPLETED,7ffeee20-418d-4679-afd3-e37a58315e3f,IQD transaction
            """;

        var filePath = CreateTempFile(csvContent);

        // Act
        var transactions = await _reader.ReadTransactionsAsync(filePath);

        // Assert
        var transactionList = transactions.ToList();
        Assert.Equal(3, transactionList.Count);

        Assert.Equal(Currency.USD, transactionList[0].Amount.Currency);
        Assert.Equal(Currency.EUR, transactionList[1].Amount.Currency);
        Assert.Equal(Currency.IQD, transactionList[2].Amount.Currency);
    }

    [Fact]
    public async Task ReadTransactionsAsync_WithSpecialCharactersInNotes_PreservesData()
    {
        // Arrange
        var csvContent = """
            ID,COUNTERPARTY,AMOUNT,FEE,BALANCE AFTER,TRANSACTION TYPE,DATE,TIME,STATUS,TRANSACTION ID,NOTE
            11111111-1111-1111-1111-111111111111,"Test User",100.50 USD,5.00 USD,1000.00 USD,TRANSFER,05/09/2023,3:13:12 PM,COMPLETED,7ffeee20-418d-4679-afd3-e37a58315e3f,"Payment for ""special"" services"
            22222222-2222-2222-2222-222222222222,Test User 2,250.75 USD,2.50 USD,1248.25 USD,TRANSFER,15/09/2023,10:30:45 AM,COMPLETED,7ffeee20-418d-4679-afd3-e37a58315e3f,"Note with, comma and newline
            continuation"
            """;

        var filePath = CreateTempFile(csvContent);

        // Act
        var transactions = await _reader.ReadTransactionsAsync(filePath);

        // Assert
        var transactionList = transactions.ToList();
        Assert.Equal(2, transactionList.Count);

        Assert.Equal("Payment for \"special\" services", transactionList[0].Note);
        Assert.Contains("Note with, comma and newline", transactionList[1].Note);
    }

    [Fact]
    public async Task ReadTransactionsAsync_WithStreamFromFile_ProducesSameResultAsFilePath()
    {
        // Arrange
        var csvContent = """
            ID,COUNTERPARTY,AMOUNT,FEE,BALANCE AFTER,TRANSACTION TYPE,DATE,TIME,STATUS,TRANSACTION ID,NOTE
            11111111-1111-1111-1111-111111111111,John Doe,100.50 USD,5.00 USD,1000.00 USD,TRANSFER,05/09/2023,3:13:12 PM,COMPLETED,7ffeee20-418d-4679-afd3-e37a58315e3f,Test transaction
            """;

        var filePath = CreateTempFile(csvContent);

        // Act
        var transactionsFromFile = await _reader.ReadTransactionsAsync(filePath);

        using var fileStream = File.OpenRead(filePath);
        var transactionsFromStream = await _reader.ReadTransactionsAsync(fileStream);

        // Assert
        var fileList = transactionsFromFile.ToList();
        var streamList = transactionsFromStream.ToList();

        Assert.Equal(fileList.Count, streamList.Count);

        for (int i = 0; i < fileList.Count; i++)
        {
            Assert.Equal(fileList[i].Id, streamList[i].Id);
            Assert.Equal(fileList[i].Counterparty, streamList[i].Counterparty);
            Assert.Equal(fileList[i].Amount, streamList[i].Amount);
            Assert.Equal(fileList[i].TransactionId, streamList[i].TransactionId);
        }
    }

    [Fact]
    public async Task ReadTransactionsAsync_WithEmptyFileButHeaders_ReturnsEmptyCollection()
    {
        // Arrange
        var csvContent = "ID,COUNTERPARTY,AMOUNT,FEE,BALANCE AFTER,TRANSACTION TYPE,DATE,TIME,STATUS,TRANSACTION ID,NOTE\n";
        var filePath = CreateTempFile(csvContent);

        // Act
        var transactions = await _reader.ReadTransactionsAsync(filePath);

        // Assert
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task ReadTransactionsAsync_WithDifferentTimeFormats_ParsesCorrectly()
    {
        // Arrange
        var csvContent = """
            ID,COUNTERPARTY,AMOUNT,FEE,BALANCE AFTER,TRANSACTION TYPE,DATE,TIME,STATUS,TRANSACTION ID,NOTE
            11111111-1111-1111-1111-111111111111,Morning User,100.50 USD,5.00 USD,1000.00 USD,TRANSFER,05/09/2023,9:13:12 AM,COMPLETED,7ffeee20-418d-4679-afd3-e37a58315e3f,Morning transaction
            22222222-2222-2222-2222-222222222222,Evening User,250.75 USD,2.50 USD,1248.25 USD,TRANSFER,15/09/2023,11:30:45 PM,COMPLETED,7ffeee20-418d-4679-afd3-e37a58315e3f,Evening transaction
            33333333-3333-3333-3333-333333333333,Noon User,75.00 USD,1.50 USD,1321.75 USD,TRANSFER,20/09/2023,12:00:00 PM,COMPLETED,7ffeee20-418d-4679-afd3-e37a58315e3f,Noon transaction
            """;

        var filePath = CreateTempFile(csvContent);

        // Act
        var transactions = await _reader.ReadTransactionsAsync(filePath);

        // Assert
        var transactionList = transactions.ToList();
        Assert.Equal(3, transactionList.Count);

        // 9:13:12 AM = 09:13:12
        Assert.Equal(new TimeSpan(9, 13, 12), transactionList[0].Time);

        // 11:30:45 PM = 23:30:45
        Assert.Equal(new TimeSpan(23, 30, 45), transactionList[1].Time);

        // 12:00:00 PM = 12:00:00
        Assert.Equal(new TimeSpan(12, 0, 0), transactionList[2].Time);
    }

    private string CreateTempFile(string content)
    {
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);
        File.WriteAllText(tempFile, content, Encoding.UTF8);
        return tempFile;
    }

    public void Dispose()
    {
        foreach (var tempFile in _tempFiles)
        {
            if (File.Exists(tempFile))
            {
                try
                {
                    File.Delete(tempFile);
                }
                catch
                {
                    // Ignore cleanup failures
                }
            }
        }
    }
}