using Xunit;

namespace Transaction.Test.Unit;

public class FibTransactionTest
{
    [Fact]
    public void Constructor_WithDefaults_InitializesCorrectly()
    {
        // Arrange & Act
        var transaction = new FibTransaction();

        // Assert
        Assert.Equal(Guid.Empty, transaction.Id);
        Assert.Equal(string.Empty, transaction.Counterparty);
        Assert.Equal(new MonetaryValue(0, Currency.IQD), transaction.Amount);
        Assert.Equal(new MonetaryValue(0, Currency.IQD), transaction.Fee);
        Assert.Equal(new MonetaryValue(0, Currency.IQD), transaction.BalanceAfter);
        Assert.Equal(string.Empty, transaction.TransactionType);
        Assert.Equal(DateTime.MinValue, transaction.Date);
        Assert.Equal(TimeSpan.Zero, transaction.Time);
        Assert.Equal(string.Empty, transaction.Status);
        Assert.Equal(null, transaction.TransactionId);
        Assert.Equal(string.Empty, transaction.Note);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var transaction = new FibTransaction();
        var transactionId = Guid.NewGuid();
        var amount = new MonetaryValue(100.50m, Currency.EUR);

        // Act
        transaction.Id = transactionId;
        transaction.Amount = amount;
        transaction.TransactionType = "TRANSFER";

        // Assert
        Assert.Equal(transactionId, transaction.Id);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal("TRANSFER", transaction.TransactionType);
    }

    [Fact]
    public void MonetaryProperties_HandleDifferentCurrencies()
    {
        // Arrange & Act
        var transaction = new FibTransaction
        {
            Amount = new MonetaryValue(100m, Currency.EUR),
            Fee = new MonetaryValue(5m, Currency.IQD),
            BalanceAfter = new MonetaryValue(95m, Currency.USD)
        };

        // Assert
        Assert.Equal(Currency.EUR, transaction.Amount.Currency);
        Assert.Equal(Currency.IQD, transaction.Fee.Currency);
        Assert.Equal(Currency.USD, transaction.BalanceAfter.Currency);
    }

    [Theory]
    [InlineData("MONEY_BOX_TRANSFER", true)]
    [InlineData("AUTO_SAVING_TRANSFER", true)]
    [InlineData("TRANSFER", false)]
    [InlineData("PAYMENT", false)]
    [InlineData("DEPOSIT", false)]
    [InlineData("WITHDRAWAL", false)]
    [InlineData("ACCOUNT_TRANSFER", false)]
    [InlineData("", false)]
    public void IsInternal_ReturnsCorrectValue_ForTransactionType(string transactionType, bool expectedIsInternal)
    {
        // Arrange
        var transaction = new FibTransaction
        {
            TransactionType = transactionType
        };

        // Act
        var isInternal = transaction.IsInternal;

        // Assert
        Assert.Equal(expectedIsInternal, isInternal);
    }
}