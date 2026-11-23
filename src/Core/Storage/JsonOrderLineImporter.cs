using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonOrderLineImporter : IOrderLineImporter
{
    public async Task<IReadOnlyList<OrderLineDto>> LoadAsync(
        string path,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true);

        List<OrderLineDto> lines =
            await JsonSerializer.DeserializeAsync<List<OrderLineDto>>(
                stream,
                cancellationToken: ct)
            ?? throw new DomainException(
                "Файл не містить списку позицій замовлення.");

        for (int i = 0; i < lines.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            ValidateLine(lines[i], i + 1);
        }

        return lines;
    }

    // Додаткове №1: потоковий імпорт.
    public async IAsyncEnumerable<OrderLineDto> StreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true);

        int number = 0;

        await foreach (OrderLineDto? line in
            JsonSerializer.DeserializeAsyncEnumerable<OrderLineDto>(
                stream,
                cancellationToken: ct)
            .WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();

            number++;

            ValidateLine(line, number);

            yield return line!;
        }
    }

    private static void ValidateLine(
        OrderLineDto? line,
        int number)
    {
        if (line is null)
        {
            throw new DomainException(
                $"Запис {number}: порожня позиція замовлення.");
        }

        try
        {
            // інваріанти з лабораторної №4.
            _ = OrderLine.FromDto(line);
        }
        catch (ArgumentException ex)
        {
            throw new DomainException(
                $"Запис {number}: {ex.Message}",
                ex);
        }
    }
}