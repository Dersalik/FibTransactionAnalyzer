using Xunit;

namespace Transaction.Test.Unit;

public class MonetaryValueTest
{
    [Theory]
    [InlineData(100.50, Currency.USD)]
    [InlineData(0, Currency.EUR)]
    [InlineData(-50.25, Currency.IQD)]
    [InlineData(999999.99, Currency.USD)]
    public void Constructor_WithValidValues_CreatesMonetaryValue(decimal amount, Currency currency)
    {
        // Arrange & Act
        var monetaryValue = new MonetaryValue(amount, currency);

        // Assert
        Assert.Equal(amount, monetaryValue.Amount);
        Assert.Equal(currency, monetaryValue.Currency);
    }

    [Fact]
    public void Constructor_WithAmountOnly_DefaultsToIQD()
    {
        // Arrange & Act
        var monetaryValue = new MonetaryValue(100.50m);

        // Assert
        Assert.Equal(100.50m, monetaryValue.Amount);
        Assert.Equal(Currency.IQD, monetaryValue.Currency);
    }

    [Theory]
    [InlineData("100.50 USD", 100.50, Currency.USD)]
    [InlineData("50.25 EUR", 50.25, Currency.EUR)]
    [InlineData("75.00 IQD", 75.00, Currency.IQD)]
    [InlineData("100.50", 100.50, Currency.IQD)]
    [InlineData("100", 100, Currency.IQD)]
    [InlineData("-50.25 USD", -50.25, Currency.USD)]
    [InlineData("1000.00USD", 1000.00, Currency.USD)]
    [InlineData("1,234.56 USD", 1234.56, Currency.USD)]
    public void Parse_WithValidFormats_ReturnsCorrectMonetaryValue(string input, decimal expectedAmount, Currency expectedCurrency)
    {
        // Arrange & Act
        var result = MonetaryValue.Parse(input);

        // Assert
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(expectedCurrency, result.Currency);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_WithNullOrWhitespace_ReturnsZeroIQD(string input)
    {
        // Arrange & Act
        var result = MonetaryValue.Parse(input);

        // Assert
        Assert.Equal(0m, result.Amount);
        Assert.Equal(Currency.IQD, result.Currency);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("100.50 INVALID")]
    [InlineData("abc EUR")]
    [InlineData("100.50.50 USD")]
    [InlineData("$100.50")]
    public void Parse_WithInvalidFormats_ThrowsFormatException(string input)
    {
        // Arrange & Act & Assert
        Assert.Throws<FormatException>(() => MonetaryValue.Parse(input));
    }

    [Fact]
    public void Parse_WithCommaAsDecimalSeparator_ParsesCorrectly()
    {
        // Arrange & Act
        var result = MonetaryValue.Parse("123,45 EUR");

        // Assert
        Assert.Equal(123.45m, result.Amount);
        Assert.Equal(Currency.EUR, result.Currency);
    }

    [Theory]
    [InlineData(100.50, Currency.USD, "100.50 USD")]
    [InlineData(0, Currency.EUR, "0.00 EUR")]
    [InlineData(-50.25, Currency.IQD, "-50.25 IQD")]
    [InlineData(1000, Currency.USD, "1000.00 USD")]
    public void ToString_WithDefaultFormat_ReturnsExpectedString(decimal amount, Currency currency, string expected)
    {
        // Arrange
        var monetaryValue = new MonetaryValue(amount, currency);

        // Act
        var result = monetaryValue.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(100.50, Currency.USD, "F0", "101 USD")]
    [InlineData(100.1234, Currency.EUR, "F4", "100.1234 EUR")]
    [InlineData(1000, Currency.IQD, "N0", "1,000 IQD")]
    public void ToString_WithCustomFormat_ReturnsFormattedString(decimal amount, Currency currency, string format, string expected)
    {
        // Arrange
        var monetaryValue = new MonetaryValue(amount, currency);

        // Act
        var result = monetaryValue.ToString(format);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(100.50, Currency.USD, "$100.50")]
    [InlineData(75.25, Currency.EUR, "€75.25")]
    [InlineData(50.00, Currency.IQD, "د.ع50.00")]
    [InlineData(0, Currency.USD, "$0.00")]
    public void ToDisplayString_WithVariousCurrencies_ReturnsExpectedFormat(decimal amount, Currency currency, string expected)
    {
        // Arrange
        var monetaryValue = new MonetaryValue(amount, currency);

        // Act
        var result = monetaryValue.ToDisplayString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(100.50, Currency.USD, 100.50, Currency.USD, true)]
    [InlineData(100.50, Currency.USD, 100.50, Currency.EUR, false)]
    [InlineData(100.50, Currency.USD, 50.25, Currency.USD, false)]
    [InlineData(0, Currency.EUR, 0, Currency.EUR, true)]
    public void Equals_WithVariousComparisons_ReturnsExpectedResult(decimal amount1, Currency currency1, decimal amount2, Currency currency2, bool expected)
    {
        // Arrange
        var monetaryValue1 = new MonetaryValue(amount1, currency1);
        var monetaryValue2 = new MonetaryValue(amount2, currency2);

        // Act
        var result = monetaryValue1.Equals(monetaryValue2);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Equals_WithNullObject_ReturnsFalse()
    {
        // Arrange
        var monetaryValue = new MonetaryValue(100.50m, Currency.USD);

        // Act
        var result = monetaryValue.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithDifferentObjectType_ReturnsFalse()
    {
        // Arrange
        var monetaryValue = new MonetaryValue(100.50m, Currency.USD);

        // Act
        var result = monetaryValue.Equals("100.50 USD");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_WithEqualObjects_ReturnsSameHashCode()
    {
        // Arrange
        var monetaryValue1 = new MonetaryValue(100.50m, Currency.USD);
        var monetaryValue2 = new MonetaryValue(100.50m, Currency.USD);

        // Act
        var hashCode1 = monetaryValue1.GetHashCode();
        var hashCode2 = monetaryValue2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentObjects_ReturnsDifferentHashCodes()
    {
        // Arrange
        var monetaryValue1 = new MonetaryValue(100.50m, Currency.USD);
        var monetaryValue2 = new MonetaryValue(100.50m, Currency.EUR);

        // Act
        var hashCode1 = monetaryValue1.GetHashCode();
        var hashCode2 = monetaryValue2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Theory]
    [InlineData(100.50, Currency.USD, 100.50)]
    [InlineData(0, Currency.EUR, 0)]
    [InlineData(-50.25, Currency.IQD, -50.25)]
    public void ImplicitOperator_ToDecimal_ReturnsAmount(decimal amount, Currency currency, decimal expected)
    {
        // Arrange
        var monetaryValue = new MonetaryValue(amount, currency);

        // Act
        decimal result = monetaryValue;

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_WithLargeNumbers_HandlesCorrectly()
    {
        // Arrange & Act
        var result = MonetaryValue.Parse("999999999.99 USD");

        // Assert
        Assert.Equal(999999999.99m, result.Amount);
        Assert.Equal(Currency.USD, result.Currency);
    }

    [Fact]
    public void Parse_WithNumbersContainingCommas_ParsesCorrectly()
    {
        // Arrange & Act
        var result = MonetaryValue.Parse("1,234,567.89 USD");

        // Assert
        Assert.Equal(1234567.89m, result.Amount);
        Assert.Equal(Currency.USD, result.Currency);
    }

    [Theory]
    [InlineData("123.45USD")]
    [InlineData("123.45 USD")]
    [InlineData("  123.45   USD  ")]
    public void Parse_WithVariousSpacing_ParsesCorrectly(string input)
    {
        // Arrange & Act
        var result = MonetaryValue.Parse(input);

        // Assert
        Assert.Equal(123.45m, result.Amount);
        Assert.Equal(Currency.USD, result.Currency);
    }

    [Fact]
    public void Parse_WithZeroValue_ReturnsZero()
    {
        // Arrange & Act
        var result = MonetaryValue.Parse("0 EUR");

        // Assert
        Assert.Equal(0m, result.Amount);
        Assert.Equal(Currency.EUR, result.Currency);
    }
}

