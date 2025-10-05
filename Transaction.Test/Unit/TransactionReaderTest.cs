using System.Text;
using Xunit;

namespace Transaction.Test.Unit;

public class TransactionReaderTest
{
    private readonly TransactionReader _reader = new();

    [Fact]
    public async Task ReadTransactionsAsync_WithNullStream_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _reader.ReadTransactionsAsync((Stream)null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ReadTransactionsAsync_WithInvalidFilePath_ThrowsArgumentException(string filePath)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _reader.ReadTransactionsAsync(filePath));
    }

    [Fact]
    public async Task ReadTransactionsAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = "non-existent-file.csv";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _reader.ReadTransactionsAsync(nonExistentPath));
    }

    [Fact]
    public async Task ReadTransactionsAsync_WithValidCsvStream_ReturnsTransactions()
    {
        // Arrange
        var csvContent = """
            ID,COUNTERPARTY,AMOUNT,FEE,BALANCE AFTER,TRANSACTION TYPE,DATE,TIME,STATUS,TRANSACTION ID,NOTE
            11111111-1111-1111-1111-111111111111,Test User,100.50 USD,5.00 USD,1000.00 USD,TRANSFER,05/09/2023,3:13:12 PM,COMPLETED,7ffeee20-418d-4679-afd3-e37a58315e3f,Test transaction
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        var transactions = await _reader.ReadTransactionsAsync(stream);

        // Assert
        var transactionList = transactions.ToList();
        Assert.Single(transactionList);

        var transaction = transactionList.First();
        Assert.Equal(new Guid("11111111-1111-1111-1111-111111111111"), transaction.Id);
        Assert.Equal("Test User", transaction.Counterparty);
        Assert.Equal(new MonetaryValue(100.50m, Currency.USD), transaction.Amount);
    }

    [Fact]
    public async Task ReadTransactionsAsync_WithEmptyStream_ReturnsEmptyCollection()
    {
        // Arrange
        var csvContent = "ID,COUNTERPARTY,AMOUNT,FEE,BALANCE AFTER,TRANSACTION TYPE,DATE,TIME,STATUS,TRANSACTION ID,NOTE\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        var transactions = await _reader.ReadTransactionsAsync(stream);

        // Assert
        Assert.Empty(transactions);
    }
}
