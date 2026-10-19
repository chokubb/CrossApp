using Core.Dto;
using Core.Import;

bool mixedMode = args.Contains("--mixed");

string? pathArgument =
    args.FirstOrDefault(arg => !arg.StartsWith("--"));

string path = pathArgument
    ?? Path.Combine("data", mixedMode ? "mixed.csv" : "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}


// Режим імпорту різнорідних записів
if (mixedMode)
{
    ImportResult<MixedImportItem> mixedResult =
        MixedCsvImporter.Load(path);

    Console.WriteLine($"Файл: {Path.GetFullPath(path)}");
    Console.WriteLine($"Завантажено записів: {mixedResult.Items.Count}");
    Console.WriteLine();

    foreach (MixedImportItem item in mixedResult.Items.Take(5))
    {
        switch (item)
        {
            case ImportedProduct product:
                Console.WriteLine(
                    $"PRODUCT  {product.Value.Id,-7} " +
                    $"{product.Value.Name,-24} " +
                    $"{product.Value.Price,8:F2}");
                break;

            case ImportedCustomer customer:
                Console.WriteLine(
                    $"CUSTOMER {customer.Value.Id,-7} " +
                    $"{customer.Value.Name,-24} " +
                    $"{customer.Value.Email ?? "(без email)"}");
                break;
        }
    }

    if (mixedResult.Errors.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine($"Пропущено рядків: {mixedResult.Errors.Count}");

        foreach (string error in mixedResult.Errors)
        {
            Console.WriteLine($"! {error}");
        }
    }

    PrintStatistics(
        mixedResult.Items.Count,
        mixedResult.Errors.Count);

    return 0;
}


// Звичайний імпорт CSV або JSON
ImportResult<ProductDto> result =
    Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".csv" => ProductCsvImporter.Load(path),

        ".json" => ProductJsonImporter.Load(path),

        _ => new ImportResult<ProductDto>(
            Array.Empty<ProductDto>(),
            new[]
            {
                $"Непідтримуваний формат файлу: {Path.GetExtension(path)}"
            })
    };

Console.WriteLine($"Файл: {Path.GetFullPath(path)}");
Console.WriteLine($"Завантажено записів: {result.Items.Count}");
Console.WriteLine();

foreach (ProductDto product in result.Items.Take(5))
{
    Console.WriteLine(
        $"{product.Id,-7} " +
        $"{product.Name,-24} " +
        $"{product.Price,8:F2}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");

    foreach (string error in result.Errors)
    {
        Console.WriteLine($"! {error}");
    }
}

PrintStatistics(
    result.Items.Count,
    result.Errors.Count);

return 0;


// Статистика імпорту
static void PrintStatistics(int accepted, int skipped)
{
    int total = accepted + skipped;

    double errorPercent =
        total == 0
            ? 0
            : (double)skipped / total * 100;

    Console.WriteLine();
    Console.WriteLine(
        $"Усього: {total} | " +
        $"Прийнято: {accepted} | " +
        $"Пропущено: {skipped} | " +
        $"Помилки: {errorPercent:F2}%");
}