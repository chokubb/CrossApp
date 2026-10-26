using Core.Domain;
using Core.Dto;

namespace Core.Import;

public static class DomainImportMapper
{
    public static ImportResult<OrderLine> MapToOrderLines(
        ImportResult<ProductDto> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var items = new List<OrderLine>();
        var errors = new List<string>(source.Errors);

        for (int i = 0; i < source.Items.Count; i++)
        {
            ProductDto product = source.Items[i];

            try
            {
                OrderLine line = OrderLine.Create(
                    product.Id,
                    product.Name,
                    product.Price,
                    1);

                items.Add(line);
            }
            catch (ArgumentException ex)
            {
                errors.Add(
                    $"імпортований запис {i + 1}: {ex.Message}");
            }
        }

        return new ImportResult<OrderLine>(
            items,
            errors);
    }
}