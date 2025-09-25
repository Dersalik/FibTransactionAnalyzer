using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Moq;
using Xunit;

namespace Transaction.Test.Unit;

public class DateTimeConverterTests
{
    private readonly DateTimeConverter _converter = new();
    private readonly Mock<IReaderRow> _mockReaderRow = new();
    private readonly Mock<IWriterRow> _mockWriterRow = new();
    private readonly Mock<MemberMapData> _mockMemberMapData = new(null);
    private readonly CsvContext _context;

    public DateTimeConverterTests()
    {
        var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
        _context = new CsvContext(config);
        _mockReaderRow.Setup(r => r.Context).Returns(_context);
    }

    [Theory]
    [InlineData("25/12/2023", 2023, 12, 25)]
    [InlineData("01/01/2000", 2000, 1, 1)]
    [InlineData("31/03/2024", 2024, 3, 31)]
    public void ConvertFromString_ValidDateFormat_ReturnsDateTime(string input, int expectedYear, int expectedMonth, int expectedDay)
    {
        // Act
        var result = _converter.ConvertFromString(input, _mockReaderRow.Object, _mockMemberMapData.Object);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<DateTime>(result);
        var dateTime = (DateTime)result;
        Assert.Equal(expectedYear, dateTime.Year);
        Assert.Equal(expectedMonth, dateTime.Month);
        Assert.Equal(expectedDay, dateTime.Day);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void ConvertFromString_NullOrWhitespace_ReturnsMinValue(string input)
    {
        // Act
        var result = _converter.ConvertFromString(input, _mockReaderRow.Object, _mockMemberMapData.Object);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<DateTime>(result);
        Assert.Equal(DateTime.MinValue, result);
    }

    [Theory]
    [InlineData("2023-12-25")]
    [InlineData("12/25/2023")]
    [InlineData("25-12-2023")]
    [InlineData("invalid")]
    [InlineData("32/01/2023")]
    [InlineData("01/13/2023")]
    [InlineData("1/1/2023")]
    public void ConvertFromString_InvalidFormat_ThrowsTypeConverterException(string invalidInput)
    {
        // Act & Assert
        var exception = Assert.Throws<TypeConverterException>(() =>
            _converter.ConvertFromString(invalidInput, _mockReaderRow.Object, _mockMemberMapData.Object));

        Assert.Equal(_converter, exception.TypeConverter);
        Assert.Equal(_mockMemberMapData.Object, exception.MemberMapData);
        Assert.Equal(invalidInput, exception.Text);
        Assert.Equal(_context, exception.Context);
        Assert.Contains($"Unable to parse '{invalidInput}' as a valid date", exception.Message);
        Assert.Contains("Expected format: dd/MM/yyyy", exception.Message);
    }

    [Theory]
    [InlineData(2023, 12, 25, "25/12/2023")]
    [InlineData(2000, 1, 1, "01/01/2000")]
    [InlineData(2024, 3, 31, "31/03/2024")]
    public void ConvertToString_ValidDateTime_ReturnsFormattedString(int year, int month, int day, string expected)
    {
        // Arrange
        var dateTime = new DateTime(year, month, day);

        // Act
        var result = _converter.ConvertToString(dateTime, _mockWriterRow.Object, _mockMemberMapData.Object);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertToString_MinDateTime_ReturnsEmptyString()
    {
        // Arrange
        var minDateTime = DateTime.MinValue;

        // Act
        var result = _converter.ConvertToString(minDateTime, _mockWriterRow.Object, _mockMemberMapData.Object);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ConvertToString_NullValue_ReturnsEmptyString()
    {
        // Act
        var result = _converter.ConvertToString(null, _mockWriterRow.Object, _mockMemberMapData.Object);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Theory]
    [InlineData("not a datetime")]
    [InlineData(42)]
    [InlineData(true)]
    public void ConvertToString_NonDateTimeValue_ReturnsToStringResult(object nonDateTimeValue)
    {
        // Act
        var result = _converter.ConvertToString(nonDateTimeValue, _mockWriterRow.Object, _mockMemberMapData.Object);

        // Assert
        Assert.Equal(nonDateTimeValue.ToString(), result);
    }
}