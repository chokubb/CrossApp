using Core.Dto;
using Core.Import;

string path = args.Length > 0
    ? args[0]
    : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

ImportResult<ProductDto> result = ProductCsvImporter.Load(path);

Console.WriteLine($"Файл: {Path.GetFullPath(path)}");
Console.WriteLine($"Завантажено записів: {result.Items.Count}");
Console.WriteLine();

foreach (ProductDto product in result.Items.Take(5))
{
    Console.WriteLine(
        $"{product.Id,-7} {product.Name,-24} {product.Price,8:F2}");
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

return 0;