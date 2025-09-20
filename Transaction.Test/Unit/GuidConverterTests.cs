using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Moq;
using Xunit;

namespace Transaction.Test.Unit;

public class GuidConverterTests
{
    private readonly GuidConverter _converter = new();
    private readonly Mock<IReaderRow> _mockReaderRow = new();
    private readonly Mock<IWriterRow> _mockWriterRow = new();
    private readonly Mock<MemberMapData> _mockMemberMapData = new(null);
    private readonly CsvContext _context;

    public GuidConverterTests()
    {
        var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
        _context = new CsvContext(config);
        _mockReaderRow.Setup(r => r.Context).Returns(_context);
    }

    [Fact]
    public void ConvertFromString_ValidGuid_ReturnsGuid()
    {
        // Arrange
        var validGuid = Guid.NewGuid();
        var input = validGuid.ToString();

        // Act
        var result = _converter.ConvertFromString(input, _mockReaderRow.Object, _mockMemberMapData.Object);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Guid>(result);
        Assert.Equal(validGuid, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void ConvertFromString_NullOrWhitespace_ReturnsEmptyGuid(string input)
    {
        // Act
        var result = _converter.ConvertFromString(input, _mockReaderRow.Object, _mockMemberMapData.Object);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Guid>(result);
        Assert.Equal(Guid.Empty, result);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("12345")]
    [InlineData("not-a-guid-at-all")]
    [InlineData("123e4567-e89b-12d3-a456-42661417400")] 
    public void ConvertFromString_InvalidFormat_ThrowsTypeConverterException(string invalidInput)
    {
        // Act & Assert
        var exception = Assert.Throws<TypeConverterException>(() =>
            _converter.ConvertFromString(invalidInput, _mockReaderRow.Object, _mockMemberMapData.Object));

        Assert.Equal(_converter, exception.TypeConverter);
        Assert.Equal(_mockMemberMapData.Object, exception.MemberMapData);
        Assert.Equal(invalidInput, exception.Text);
        Assert.Equal(_context, exception.Context);
        Assert.Contains($"Unable to convert '{invalidInput}' to Guid", exception.Message);
    }

    [Fact]
    public void ConvertToString_ValidGuid_ReturnsGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = _converter.ConvertToString(guid, _mockWriterRow.Object, _mockMemberMapData.Object);

        // Assert
        Assert.Equal(guid.ToString(), result);
    }

    [Fact]
    public void ConvertToString_EmptyGuid_ReturnsEmptyGuidString()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var result = _converter.ConvertToString(emptyGuid, _mockWriterRow.Object, _mockMemberMapData.Object);

        // Assert
        Assert.Equal(emptyGuid.ToString(), result);
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