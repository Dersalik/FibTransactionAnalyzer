using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;

namespace Transaction;

public class DateTimeConverter : ITypeConverter
{
    public object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return DateTime.MinValue;

        try
        {
            if (DateTime.TryParseExact(text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            throw new FormatException($"Unable to parse '{text}' as a valid date. Expected format: dd/MM/yyyy");
        }
        catch (FormatException ex)
        {
            throw new TypeConverterException(this, memberMapData, text, row.Context, ex.Message);
        }
    }

    public string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value is DateTime dateTime)
        {
            return dateTime == DateTime.MinValue ? "" : dateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }
        return value?.ToString() ?? "";
    }
}
