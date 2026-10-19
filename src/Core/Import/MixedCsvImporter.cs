using System.Globalization;
using Core.Dto;

namespace Core.Import;

public abstract record MixedImportItem;

public sealed record ImportedProduct(ProductDto Value)
    : MixedImportItem;

public sealed record ImportedCustomer(CustomerDto Value)
    : MixedImportItem;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<MixedImportItem> Load(string path)
    {
        var items = new List<MixedImportItem>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
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

        return new ImportResult<MixedImportItem>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts =
            line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            ["P", "", _, _, _] or ["P", _, "", _, _]
                => new ParseFailed("ID або назва товару порожні"),

            ["P", _, _, var priceText, _]
                when !decimal.TryParse(
                    priceText,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal price) || price < 0
                => new ParseFailed($"некоректна ціна '{priceText}'"),

            ["P", var id, var name, var priceText, var description]
                => new ParseOk(
                    new ImportedProduct(
                        new ProductDto(
                            id,
                            name,
                            decimal.Parse(
                                priceText,
                                CultureInfo.InvariantCulture),
                            string.IsNullOrWhiteSpace(description)
                                ? null
                                : description))),

            ["C", "", _, _] or ["C", _, "", _]
                => new ParseFailed("ID або ім'я клієнта порожні"),

            ["C", var id, var name, var email]
                => new ParseOk(
                    new ImportedCustomer(
                        new CustomerDto(
                            id,
                            name,
                            string.IsNullOrWhiteSpace(email)
                                ? null
                                : email))),

            [var prefix, ..]
                => new ParseFailed(
                    $"невідомий тип запису '{prefix}'"),

            _ => new ParseFailed("невідомий формат рядка")
        };
    }

    private abstract record ParseOutcome;

    private sealed record ParseOk(MixedImportItem Value)
        : ParseOutcome;

    private sealed record ParseFailed(string Reason)
        : ParseOutcome;
}