using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Transaction
{
    public class MonetaryValueConverter : ITypeConverter
    {
        public object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new MonetaryValue(0, Currency.IQD);

            try
            {
                return MonetaryValue.Parse(text);
            }
            catch (FormatException ex)
            {
                throw new TypeConverterException(this, memberMapData, text, row.Context, ex.Message);
            }
        }

        public string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
        {
            return value?.ToString() ?? "";
        }
    }
}
