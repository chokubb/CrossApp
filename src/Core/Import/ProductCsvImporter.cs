using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

public static class ProductCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (number == 1 &&
                line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;

                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<ProductDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts =
            line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 4 }
                => new ParseFailed(
                    $"очікую 4 колонки, отримано {parts.Length}"),

            ["", _, _, _] or [_, "", _, _]
                => new ParseFailed("ID або назва порожні"),

            [_, _, var priceText, _]
                when !decimal.TryParse(
                    priceText,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal price) || price < 0
                => new ParseFailed(
                    $"ціна '{priceText}' не є коректним невід'ємним числом"),

            [var id, var name, var priceText, var description]
                => new ParseOk(
                    new ProductDto(
                        id,
                        name,
                        decimal.Parse(
                            priceText,
                            CultureInfo.InvariantCulture),
                        string.IsNullOrWhiteSpace(description)
                            ? null
                            : description)),

            _ => new ParseFailed(
                $"занадто багато колонок: {parts.Length}")
        };
    }

    private abstract record ParseOutcome;

    private sealed record ParseOk(ProductDto Value)
        : ParseOutcome;

    private sealed record ParseFailed(string Reason)
        : ParseOutcome;
}