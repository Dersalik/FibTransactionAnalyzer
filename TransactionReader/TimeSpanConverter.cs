using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction;

public class TimeSpanConverter : ITypeConverter
{
    public object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return TimeSpan.Zero;

        try
        {
            string[] timeFormats = { "h:mm:ss tt", "hh:mm:ss tt", "H:mm:ss", "HH:mm:ss", "h:mm tt", "hh:mm tt" };

            foreach (var format in timeFormats)
            {
                if (DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timeResult))
                {
                    return timeResult.TimeOfDay;
                }
            }

            if (TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out TimeSpan result))
            {
                return result;
            }

            throw new FormatException($"Unable to parse '{text}' as a valid time. Expected format: h:mm:ss tt (e.g., 11:06:44 AM)");
        }
        catch (FormatException ex)
        {
            throw new TypeConverterException(this, memberMapData, text, row.Context, ex.Message);
        }
    }

    public string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value is TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero)
                return "";

            var dateTime = DateTime.Today.Add(timeSpan);
            return dateTime.ToString("h:mm:ss tt", CultureInfo.InvariantCulture);
        }
        return value?.ToString() ?? "";
    }
}
