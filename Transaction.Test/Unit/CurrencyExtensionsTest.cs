using Xunit;

namespace Transaction.Test.Unit;

public class CurrencyExtensionsTest
{
    [Theory]
    [MemberData(nameof(AllCurrencyValues))]
    public void ToCode_WithAllCurrencies_ReturnsCorrectStrings(Currency currency)
    {
        // Arrange & Act
        var code = currency.ToCode();

        // Assert
        switch (currency)
        {
            case Currency.USD:
                Assert.Equal("USD", code);
                break;
            case Currency.EUR:
                Assert.Equal("EUR", code);
                break;
            case Currency.IQD:
                Assert.Equal("IQD", code);
                break;
        }
    }

    [Theory]
    [InlineData("USD", Currency.USD)]
    [InlineData("EUR", Currency.EUR)]
    [InlineData("IQD", Currency.IQD)]
    public void FromCode_WithValidCodes_ReturnsCorrectCurrencies(string code, Currency expectedCurrency)
    {
        // Arrange & Act
        var result = CurrencyExtensions.FromCode(code);

        // Assert
        Assert.Equal(expectedCurrency, result);
    }

    [Theory]
    [InlineData("usd", Currency.USD)]
    [InlineData("eur", Currency.EUR)]
    [InlineData("iqd", Currency.IQD)]
    public void FromCode_WithLowercaseCodes_ReturnsCorrectCurrencies(string code, Currency expectedCurrency)
    {
        // Arrange & Act
        var result = CurrencyExtensions.FromCode(code);

        // Assert
        Assert.Equal(expectedCurrency, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromCode_WithNullOrEmpty_ReturnsDefaultUSD(string code)
    {
        // Arrange & Act
        var result = CurrencyExtensions.FromCode(code);

        // Assert
        Assert.Equal(Currency.IQD, result);
    }

    [Fact]
    public void FromCode_WithInvalidCode_ThrowsArgumentException()
    {
        // Arrange
        var invalidCode = "INVALID";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => CurrencyExtensions.FromCode(invalidCode));
        Assert.Contains("Unknown currency code: INVALID", exception.Message);
        Assert.Equal("currencyCode", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(AllCurrencyValues))]
    public void GetSymbol_WithAllCurrencies_ReturnsCorrectSymbols(Currency currency)
    {
        // Arrange 
        var symbol = currency.GetSymbol();

        // Act & Assert
        switch (currency)
        {
            case Currency.USD:
                Assert.Equal("$", symbol);
                break;
            case Currency.IQD:
                Assert.Equal("د.ع", symbol);
                break;
            case Currency.EUR:
                Assert.Equal("€", symbol);
                break;
        }
    }

    [Theory]
    [MemberData(nameof(AllCurrencyValues))]
    public void GetCultureCode_WithAllCurrencies_ReturnsCorrectCulture(Currency currency)
    {
        // Arrange 
        var cultureCode = currency.GetCultureCode();

        // Act & Assert
        switch (currency)
        {
            case Currency.USD:
                Assert.Equal("en-US", cultureCode);
                break;
            case Currency.IQD:
                Assert.Equal("ar-IQ", cultureCode);
                break;
            case Currency.EUR:
                Assert.Equal("de-DE", cultureCode);
                break;
        }
    }

    public static IEnumerable<object[]> AllCurrencyValues =>
        Enum.GetValues<Currency>().Select(c => new Object[] { c });
}