using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Moq;
using Xunit;

namespace Transaction.Test.Unit
{
    public class TimeSpanConverterTests
    {
        private readonly TimeSpanConverter _converter = new();
        private readonly Mock<IReaderRow> _mockReaderRow = new();
        private readonly Mock<IWriterRow> _mockWriterRow = new();
        private readonly Mock<MemberMapData> _mockMemberMapData = new(null);
        private readonly CsvContext _context;

        public TimeSpanConverterTests()
        {
            var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
            _context = new CsvContext(config);
            _mockReaderRow.Setup(r => r.Context).Returns(_context);
        }

        [Theory]
        [InlineData("11:06:44 AM", 11, 6, 44)]
        [InlineData("01:30:15 PM", 13, 30, 15)]
        [InlineData("9:45:30 PM", 21, 45, 30)]
        [InlineData("12:00:00 AM", 0, 0, 0)]
        [InlineData("12:00:00 PM", 12, 0, 0)]
        public void ConvertFromString_Valid12HourFormat_ReturnsTimeSpan(string input, int expectedHour, int expectedMinute, int expectedSecond)
        {
            // Act
            var result = _converter.ConvertFromString(input, _mockReaderRow.Object, _mockMemberMapData.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TimeSpan>(result);
            var timeSpan = (TimeSpan)result;
            Assert.Equal(expectedHour, timeSpan.Hours);
            Assert.Equal(expectedMinute, timeSpan.Minutes);
            Assert.Equal(expectedSecond, timeSpan.Seconds);
        }

        [Theory]
        [InlineData("14:30:45", 14, 30, 45)]
        [InlineData("09:15:30", 9, 15, 30)]
        [InlineData("23:59:59", 23, 59, 59)]
        [InlineData("00:00:01", 0, 0, 1)]
        public void ConvertFromString_Valid24HourFormat_ReturnsTimeSpan(string input, int expectedHour, int expectedMinute, int expectedSecond)
        {
            // Act
            var result = _converter.ConvertFromString(input, _mockReaderRow.Object, _mockMemberMapData.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TimeSpan>(result);
            var timeSpan = (TimeSpan)result;
            Assert.Equal(expectedHour, timeSpan.Hours);
            Assert.Equal(expectedMinute, timeSpan.Minutes);
            Assert.Equal(expectedSecond, timeSpan.Seconds);
        }

        [Theory]
        [InlineData("2:30 PM", 14, 30, 0)]
        [InlineData("10:45 AM", 10, 45, 0)]
        [InlineData("12:00 AM", 0, 0, 0)]
        public void ConvertFromString_ValidTimeWithoutSeconds_ReturnsTimeSpan(string input, int expectedHour, int expectedMinute, int expectedSecond)
        {
            // Act
            var result = _converter.ConvertFromString(input, _mockReaderRow.Object, _mockMemberMapData.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TimeSpan>(result);
            var timeSpan = (TimeSpan)result;
            Assert.Equal(expectedHour, timeSpan.Hours);
            Assert.Equal(expectedMinute, timeSpan.Minutes);
            Assert.Equal(expectedSecond, timeSpan.Seconds);
        }

        [Theory]
        [InlineData("02:30:45")]
        [InlineData("1.02:30:45")]
        public void ConvertFromString_TimeSpanFormat_ReturnsTimeSpan(string input)
        {
            // Act
            var result = _converter.ConvertFromString(input, _mockReaderRow.Object, _mockMemberMapData.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TimeSpan>(result);
            var expected = TimeSpan.Parse(input, System.Globalization.CultureInfo.InvariantCulture);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void ConvertFromString_NullOrWhitespace_ReturnsZero(string input)
        {
            // Act
            var result = _converter.ConvertFromString(input, _mockReaderRow.Object, _mockMemberMapData.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TimeSpan>(result);
            Assert.Equal(TimeSpan.Zero, result);
        }

        [Theory]
        [InlineData("invalid time")]
        [InlineData("25:61:61")]
        [InlineData("13:30 PM")]
        [InlineData("not a time")]
        [InlineData("1:2:3:4:5")]
        public void ConvertFromString_InvalidFormat_ThrowsTypeConverterException(string invalidInput)
        {
            // Act & Assert
            var exception = Assert.Throws<TypeConverterException>(() =>
                _converter.ConvertFromString(invalidInput, _mockReaderRow.Object, _mockMemberMapData.Object));

            Assert.Equal(_converter, exception.TypeConverter);
            Assert.Equal(_mockMemberMapData.Object, exception.MemberMapData);
            Assert.Equal(invalidInput, exception.Text);
            Assert.Equal(_context, exception.Context);
            Assert.Contains($"Unable to parse '{invalidInput}' as a valid time", exception.Message);
            Assert.Contains("Expected format: h:mm:ss tt", exception.Message);
        }

        [Theory]
        [InlineData(11, 6, 44, "11:06:44 AM")]
        [InlineData(13, 30, 15, "1:30:15 PM")]
        [InlineData(12, 0, 0, "12:00:00 PM")]
        [InlineData(23, 59, 59, "11:59:59 PM")]
        public void ConvertToString_ValidTimeSpan_ReturnsFormattedString(int hours, int minutes, int seconds, string expected)
        {
            // Arrange
            var timeSpan = new TimeSpan(hours, minutes, seconds);

            // Act
            var result = _converter.ConvertToString(timeSpan, _mockWriterRow.Object, _mockMemberMapData.Object);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertToString_TimeSpanWithDays_ReturnsFormattedString()
        {
            // Arrange
            var timeSpan = new TimeSpan(1, 2, 30, 45);

            // Act
            var result = _converter.ConvertToString(timeSpan, _mockWriterRow.Object, _mockMemberMapData.Object);

            // Assert
            var dateTime = DateTime.Today.Add(timeSpan);
            var expected = dateTime.ToString("h:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertToString_ZeroTimeSpan_ReturnsEmptyString()
        {
            // Arrange
            var zeroTimeSpan = TimeSpan.Zero;

            // Act
            var result = _converter.ConvertToString(zeroTimeSpan, _mockWriterRow.Object, _mockMemberMapData.Object);

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
        [InlineData("not a timespan")]
        [InlineData(42)]
        [InlineData(true)]
        public void ConvertToString_NonTimeSpanValue_ReturnsToStringResult(object nonTimeSpanValue)
        {
            // Act
            var result = _converter.ConvertToString(nonTimeSpanValue, _mockWriterRow.Object, _mockMemberMapData.Object);

            // Assert
            Assert.Equal(nonTimeSpanValue.ToString(), result);
        }
    }
}