using Cli.Notifications;
using Core.Domain;
using Core.Dto;
using Core.Events;
using Core.Import;
using Core.Services;
using Core.Storage;

public static class OrderEventsDemo
{
    public static int Run(OrderService service)
    {
        Console.WriteLine(
            "=== Лабораторна №6: доменні події ===");

        // Шлях до журналу подій.
        string logPath = Path.Combine(
            AppContext.BaseDirectory,
            "logs",
            "app.log");

        Directory.CreateDirectory(
            Path.GetDirectoryName(logPath)!);

        // Очищуємо журнал перед демонстрацією.
        File.WriteAllText(logPath, string.Empty);

        // Перший підписник: консоль і лог-файл.
        void OnOrderChanged(
            object? sender,
            OrderChangedEventArgs e)
        {
            string line =
                $"{e.At:HH:mm:ss} {e.Kind,-16} " +
                $"{e.OrderId} | позицій: {e.LinesCount} | " +
                $"сума: {e.Total:F2}";

            Console.WriteLine(line);

            File.AppendAllText(
                logPath,
                line + Environment.NewLine);
        }

        // Другий підписник: лямбда-лічильник.
        int changes = 0;

        EventHandler<OrderChangedEventArgs> counterHandler =
            (_, _) => changes++;

        service.Changed += OnOrderChanged;
        service.Changed += counterHandler;

        Console.WriteLine();
        Console.WriteLine("Підписано два обробники.");

        // 1. Створення замовлення.
        Order order = service.CreateOrder("C-LAB06");

        // 2. Додавання першого товару.
        service.AddLine(
            order.Id,
            "P-L06-1",
            "Keyboard",
            120m,
            1);

        // 3. Додавання другого товару.
        service.AddLine(
            order.Id,
            "P-L06-2",
            "Mouse",
            45m,
            2);

        // 4. Підтвердження замовлення.
        service.Confirm(order.Id);

        Console.WriteLine();
        Console.WriteLine($"Подій отримано: {changes}");
        Console.WriteLine(service.Find(order.Id));

        // Сценарії відмов.
        Console.WriteLine();
        Console.WriteLine("=== Сценарії відмов ===");

        int beforeErrors = changes;

        TryFailure(
            "Повторне підтвердження",
            () => service.Confirm(order.Id));

        TryFailure(
            "Неіснуючий ID",
            () => service.Confirm("O-NOT-FOUND"));

        Console.WriteLine(
            $"Лічильник до помилок: {beforeErrors}");

        Console.WriteLine(
            $"Лічильник після помилок: {changes}");

        // Відписка першого обробника.
        Console.WriteLine();
        Console.WriteLine("=== Перевірка відписки ===");

        service.Changed -= OnOrderChanged;

        int linesBefore = File.ReadAllLines(logPath).Length;

        // Замовлення створюється, але у консоль
        // перший обробник уже нічого не виведе.
        Order silentOrder =
            service.CreateOrder("C-SILENT");

        int linesAfter = File.ReadAllLines(logPath).Length;

        Console.WriteLine(
            $"Нове замовлення створено: {silentOrder.Id}");

        Console.WriteLine(
            $"Лічильник після відписки: {changes}");

        Console.WriteLine(
            $"Рядків у логу до: {linesBefore}, " +
            $"після: {linesAfter}");

        service.Changed -= counterHandler;

        Console.WriteLine($"Файл журналу: {logPath}");

        // Пошук через Func<Order, bool>.
        Console.WriteLine();
        Console.WriteLine("=== Пошук через Func ===");

        IReadOnlyList<Order> confirmed =
            service.FindAll(o =>
                o.Status == OrderStatus.Confirmed);

        Console.WriteLine(
            $"Підтверджених замовлень: {confirmed.Count}");

        foreach (Order item in confirmed.Take(3))
        {
            Console.WriteLine(item);
        }

        // Додаткове завдання №1:
        // два незалежні INotificationSink.
        Console.WriteLine();
        Console.WriteLine(
            "=== Додаткове №1: NotificationSink ===");

        string sinkPath = Path.Combine(
            AppContext.BaseDirectory,
            "logs",
            "sinks.log");

        File.WriteAllText(sinkPath, string.Empty);

        OrderService sinkService = new(
            new InMemoryOrderStore(),
            new ConsoleSink(),
            new FileSink(sinkPath));

        Order sinkOrder =
            sinkService.CreateOrder("C-SINK");

        sinkService.AddLine(
            sinkOrder.Id,
            "P-SINK-1",
            "USB-C Cable",
            80m,
            1);

        sinkService.Confirm(sinkOrder.Id);

        Console.WriteLine(
            $"Рядків у sinks.log: " +
            $"{File.ReadAllLines(sinkPath).Length}");

        Console.WriteLine(
            $"Файл додаткового журналу: {sinkPath}");

        // Додаткове завдання №2:
        // Action<string> під час CSV-імпорту.
        Console.WriteLine();
        Console.WriteLine(
            "=== Додаткове №2: Action<string> ===");

        string csvPath = Path.Combine(
            "data",
            "sample.csv");

        ImportResult<ProductDto> importResult =
            ProductCsvImporter.Load(
                csvPath,
                message => Console.WriteLine(
                    $"[Пропущено] {message}"));

        Console.WriteLine(
            $"Успішно імпортовано: {importResult.Items.Count}");

        Console.WriteLine(
            $"Пропущено: {importResult.Errors.Count}");

        Console.WriteLine();
        Console.WriteLine("Демонстрацію №6 завершено.");

        return 0;
    }

    private static void TryFailure(
        string title,
        Action action)
    {
        try
        {
            action();

            Console.WriteLine(
                $"! {title}: помилка не виникла");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(
                $"{title}: {ex.GetType().Name} — {ex.Message}");
        }
    }
}