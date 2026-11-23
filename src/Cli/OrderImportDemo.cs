using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Events;
using Core.Services;
using Core.Storage;

public static class OrderImportDemo
{
    public static async Task<int> RunAsync(
        OrderService service,
        string command)
    {
        Console.WriteLine(
            "=== Лабораторна №8: async/await та імпорт ===");

        string goodPath = Path.Combine(
            "data",
            "lab08-order-lines.json");

        string badPath = Path.Combine(
            "data",
            "lab08-order-lines-bad.json");

        string path = command == "import-bad"
            ? badPath
            : goodPath;

        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource();

        // Додаткове №2: зв'язані токени.
        using var linkedCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                userCts.Token,
                timeoutCts.Token);

        void OnCancel(
            object? sender,
            ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            userCts.Cancel();
        }

        Console.CancelKeyPress += OnCancel;

        try
        {
            Order order = service.CreateOrder("C-LAB08");

            // Сценарій порушення доменного правила.
            if (command == "import-closed")
            {
                service.AddLine(
                    order.Id,
                    "P-BASE",
                    "Already added",
                    10m,
                    1);

                service.Confirm(order.Id);
            }

            int events = 0;

            service.Changed += (_, e) =>
            {
                if (e.Kind == OrderChangeKind.LineAdded)
                {
                    events++;
                }
            };

            IOrderLineImporter importer =
                new JsonOrderLineImporter();

            // Для скасування імітуємо тривалу обробку
            // після реального читання JSON.
            if (command is "import-timeout" or "import-wait")
            {
                importer = new SlowImporter(importer);
            }

            if (command == "import-timeout")
            {
                timeoutCts.CancelAfter(
                    TimeSpan.FromMilliseconds(200));
            }

            int added;

            if (command == "import-stream")
            {
                Console.WriteLine(
                    "Режим: потоковий імпорт");

                added = await service.ImportStreamAsync(
                    importer,
                    order.Id,
                    path,
                    linkedCts.Token,
                    new ConsoleItemsProgress());
            }
            else
            {
                Console.WriteLine(
                    $"Режим: {command}");

                added = await service.ImportAsync(
                    importer,
                    order.Id,
                    path,
                    linkedCts.Token,
                    new ConsolePercentProgress());
            }

            Console.WriteLine();
            Console.WriteLine($"Імпортовано позицій: {added}");
            Console.WriteLine($"Подій LineAdded: {events}");

            Console.WriteLine(
                $"Кількість позицій у замовленні: " +
                $"{order.Lines.Count}");

            Console.WriteLine(
                $"Сума замовлення: {order.Total:F2}");

            // Перевірка повторного імпорту.
            if (command == "import")
            {
                int repeated = await service.ImportAsync(
                    importer,
                    order.Id,
                    path,
                    linkedCts.Token);

                Console.WriteLine(
                    $"Повторно додано позицій: {repeated}");
            }

            Console.WriteLine();
            Console.WriteLine("Демонстрацію завершено.");

            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Імпорт скасовано.");

            if (timeoutCts.IsCancellationRequested &&
                !userCts.IsCancellationRequested)
            {
                Console.WriteLine(
                    "Причина: спрацював таймаут.");
            }
            else if (userCts.IsCancellationRequested)
            {
                Console.WriteLine(
                    "Причина: Ctrl+C.");
            }

            return 0;
        }
        catch (DomainException ex)
        {
            Console.WriteLine(
                $"Дані відхилено: {ex.Message}");

            return 0;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine(
                $"Файл не знайдено: {path}");

            return 1;
        }
        catch (JsonException ex)
        {
            Console.WriteLine(
                $"Некоректний формат JSON: {ex.Message}");

            return 1;
        }
        catch (IOException ex)
        {
            Console.WriteLine(
                $"Помилка читання файла: {ex.Message}");

            return 1;
        }
        finally
        {
            Console.CancelKeyPress -= OnCancel;
        }
    }

    // Прогрес звичайного імпорту у відсотках.
    private sealed class ConsolePercentProgress : IProgress<int>
    {
        public void Report(int value)
        {
            Console.WriteLine(
                $"Прогрес імпорту: {value}%");
        }
    }

    // У потоковому режимі загальна кількість
    // записів заздалегідь не відома.
    private sealed class ConsoleItemsProgress : IProgress<int>
    {
        public void Report(int value)
        {
            Console.WriteLine(
                $"Оброблено записів: {value}");
        }
    }

    // Демонстраційна затримка для таймауту
    // та ручного скасування через Ctrl+C.
    private sealed class SlowImporter(
        IOrderLineImporter inner) : IOrderLineImporter
    {
        public async Task<IReadOnlyList<OrderLineDto>> LoadAsync(
            string path,
            CancellationToken ct = default)
        {
            IReadOnlyList<OrderLineDto> lines =
                await inner.LoadAsync(path, ct);

            await Task.Delay(
                TimeSpan.FromSeconds(8),
                ct);

            return lines;
        }

        public IAsyncEnumerable<OrderLineDto> StreamAsync(
            string path,
            CancellationToken ct = default)
        {
            return inner.StreamAsync(path, ct);
        }
    }
}