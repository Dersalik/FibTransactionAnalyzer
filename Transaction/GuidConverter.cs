using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Transaction
{
    public class GuidConverter : ITypeConverter
    {
        public object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Guid.Empty;

            try
            {
                return Guid.Parse(text);
            }
            catch (FormatException ex)
            {
                throw new TypeConverterException(this, memberMapData, text, row.Context, $"Unable to convert '{text}' to Guid: {ex.Message}");
            }
        }

        public string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
        {
            return value?.ToString() ?? string.Empty;
        }
    }
}
