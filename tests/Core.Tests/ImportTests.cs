using System.Runtime.CompilerServices;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class ImportTests
{
    [Fact]
    public async Task ImportAsync_AddsLines_UpdatesTotal_AndRaisesEvents()
    {
        var service = new OrderService(
            new InMemoryOrderStore());

        Order order = service.CreateOrder(
            "C-001",
            "O-IMPORT");

        var events = new List<OrderChangedEventArgs>();
        var progress = new CollectingProgress();

        service.Changed += (_, e) => events.Add(e);

        var importer = new StubImporter(
        [
            new OrderLineDto("P-1", "Pen", 10m, 2),
            new OrderLineDto("P-2", "Notebook", 7m, 1)
        ]);

        int added = await service.ImportAsync(
            importer,
            order.Id,
            "unused.json",
            CancellationToken.None,
            progress);

        Assert.Equal(2, added);
        Assert.Equal(27m, order.Total);
        Assert.Equal(2, order.Lines.Count);

        Assert.Equal(
            new[] { 50, 100 },
            progress.Values.ToArray());

        Assert.Equal(2, events.Count);

        Assert.All(events, e =>
            Assert.Equal(
                OrderChangeKind.LineAdded,
                e.Kind));

        // Повторний імпорт не дублює товари.
        int repeated = await service.ImportAsync(
            importer,
            order.Id,
            "unused.json");

        Assert.Equal(0, repeated);
        Assert.Equal(2, order.Lines.Count);
        Assert.Equal(2, events.Count);
    }

    [Fact]
    public async Task ImportStreamAsync_AddsLines()
    {
        var service = new OrderService(
            new InMemoryOrderStore());

        Order order = service.CreateOrder("C-001");

        var importer = new StubImporter(
        [
            new OrderLineDto("P-1", "Mouse", 25m, 2),
            new OrderLineDto("P-2", "Cable", 10m, 1)
        ]);

        int added = await service.ImportStreamAsync(
            importer,
            order.Id,
            "unused.json");

        Assert.Equal(2, added);
        Assert.Equal(60m, order.Total);
    }

    [Fact]
    public async Task ImportAsync_AlreadyCancelled_Throws()
    {
        var service = new OrderService(
            new InMemoryOrderStore());

        Order order = service.CreateOrder("C-001");

        using var cts = new CancellationTokenSource();

        cts.Cancel();

        var importer = new StubImporter([]);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(
                importer,
                order.Id,
                "unused.json",
                cts.Token));

        Assert.Empty(order.Lines);
    }

    [Fact]
    public async Task ImportAsync_ConfirmedOrder_ThrowsDomainException()
    {
        var service = new OrderService(
            new InMemoryOrderStore());

        Order order = service.CreateOrder("C-001");

        service.AddLine(
            order.Id, "P-BASE", "Mouse", 20m, 1);

        service.Confirm(order.Id);

        var importer = new StubImporter(
        [
            new OrderLineDto("P-NEW", "Cable", 10m, 1)
        ]);

        await Assert.ThrowsAsync<DomainException>(
            () => service.ImportAsync(
                importer,
                order.Id,
                "unused.json"));

        Assert.Single(order.Lines);
        Assert.Equal(20m, order.Total);
    }

    [Fact]
    public async Task JsonImporter_InvalidSecondRow_ThrowsDomainException()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"crossapp-lab08-{Guid.NewGuid():N}.json");

        string json = """
        [
          {
            "ProductId": "P-1",
            "Name": "Mouse",
            "Price": 20,
            "Quantity": 1
          },
          {
            "ProductId": "P-2",
            "Name": "Broken Item",
            "Price": 5,
            "Quantity": 0
          }
        ]
        """;

        try
        {
            await File.WriteAllTextAsync(path, json);

            var service = new OrderService(
                new InMemoryOrderStore());

            Order order = service.CreateOrder("C-001");

            DomainException ex =
                await Assert.ThrowsAsync<DomainException>(
                    () => service.ImportAsync(
                        new JsonOrderLineImporter(),
                        order.Id,
                        path));

            Assert.Contains("Запис 2", ex.Message);

            Assert.IsType<ArgumentOutOfRangeException>(
                ex.InnerException);

            Assert.Empty(order.Lines);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    // Додаткове №2: скасування саме таймаутом.
    [Fact]
    public async Task LinkedTokens_TimeoutCancelsImport()
    {
        var service = new OrderService(
            new InMemoryOrderStore());

        Order order = service.CreateOrder("C-001");

        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource();

        using var linkedCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                userCts.Token,
                timeoutCts.Token);

        timeoutCts.CancelAfter(
            TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(
                new WaitingImporter(),
                order.Id,
                "unused.json",
                linkedCts.Token));

        Assert.True(timeoutCts.IsCancellationRequested);
        Assert.False(userCts.IsCancellationRequested);
        Assert.Empty(order.Lines);
    }

    private sealed class CollectingProgress : IProgress<int>
    {
        public List<int> Values { get; } = [];

        public void Report(int value)
        {
            Values.Add(value);
        }
    }

    private sealed class StubImporter(
        IReadOnlyList<OrderLineDto> rows) : IOrderLineImporter
    {
        public Task<IReadOnlyList<OrderLineDto>> LoadAsync(
            string path,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            return Task.FromResult(rows);
        }

        public async IAsyncEnumerable<OrderLineDto> StreamAsync(
            string path,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (OrderLineDto row in rows)
            {
                ct.ThrowIfCancellationRequested();

                await Task.Yield();

                yield return row;
            }
        }
    }

    private sealed class WaitingImporter : IOrderLineImporter
    {
        public async Task<IReadOnlyList<OrderLineDto>> LoadAsync(
            string path,
            CancellationToken ct = default)
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                ct);

            return Array.Empty<OrderLineDto>();
        }

        public async IAsyncEnumerable<OrderLineDto> StreamAsync(
            string path,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                ct);

            yield break;
        }
    }
}