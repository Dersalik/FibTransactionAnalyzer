using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Moq;
using Xunit;

namespace Transaction.Test.Unit;

public class MonetaryValueConverterTests
{
    private readonly MonetaryValueConverter _converter = new();
    private readonly Mock<IReaderRow> _mockReaderRow = new();
    private readonly Mock<IWriterRow> _mockWriterRow = new();
    private readonly Mock<MemberMapData> _mockMemberMapData = new(null);
    private readonly CsvContext _context;

    public MonetaryValueConverterTests()
    {
        var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
        _context = new CsvContext(config);
        _mockReaderRow.Setup(r => r.Context).Returns(_context);
    }

    [Fact]
    public void ConvertFromString_ValidInput_ReturnsMonetaryValue()
    {
        // Arrange
        var input = "100.50 USD";

        // Act
        var result = _converter.ConvertFromString(input, _mockReaderRow.Object, _mockMemberMapData.Object);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<MonetaryValue>(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ConvertFromString_NullOrWhitespace_ReturnsZeroIQD(string input)
    {
        // Act
        var result = _converter.ConvertFromString(input, _mockReaderRow.Object, _mockMemberMapData.Object);

        // Assert
        var monetaryValue = Assert.IsType<MonetaryValue>(result);
        Assert.Equal(0, monetaryValue.Amount);
        Assert.Equal(Currency.IQD, monetaryValue.Currency);
    }

    [Fact]
    public void ConvertFromString_InvalidFormat_ThrowsTypeConverterException()
    {
        // Arrange
        var invalidInput = "invalid format";

        // Act & Assert
        var exception = Assert.Throws<TypeConverterException>(() =>
            _converter.ConvertFromString(invalidInput, _mockReaderRow.Object, _mockMemberMapData.Object));

        Assert.Equal(_converter, exception.TypeConverter);
        Assert.Equal(invalidInput, exception.Text);
    }

    [Fact]
    public void ConvertToString_ValidMonetaryValue_ReturnsString()
    {
        // Arrange
        var monetaryValue = new MonetaryValue(100.50m, Currency.USD); // Adjust constructor as needed

        // Act
        var result = _converter.ConvertToString(monetaryValue, _mockWriterRow.Object, _mockMemberMapData.Object);

        // Assert
        Assert.Equal(monetaryValue.ToString(), result);
    }

    [Fact]
    public void ConvertToString_NullValue_ReturnsEmptyString()
    {
        // Act
        var result = _converter.ConvertToString(null, _mockWriterRow.Object, _mockMemberMapData.Object);

        // Assert
        Assert.Equal(string.Empty, result);
    }
}