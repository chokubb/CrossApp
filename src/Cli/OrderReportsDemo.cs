using System.Diagnostics;
using Core;
using Core.Domain;
using Core.Reports;
using Core.Services;

public static class OrderReportsDemo
{
    public static int Run(
        OrderService service,
        string[] args)
    {
        string command = args.FirstOrDefault(
            arg => !arg.StartsWith("--")) ?? "report-top";

        IReadOnlyList<Order> orders = service.All();

        Console.WriteLine(
            "=== Лабораторна №7: LINQ-звіти ===");

        Console.WriteLine(
            $"Кількість замовлень у джерелі: {orders.Count}");

        Console.WriteLine();

        switch (command)
        {
            case "report-top":
                Console.WriteLine(
                    "=== Топ-5 клієнтів за сумою ===");

                PrintCustomers(
                    ReportService.TopCustomers(orders, 5));
                break;

            case "report-high":
                Console.WriteLine(
                    "=== Замовлення дорожчі за 40 грн ===");

                PrintOrders(
                    ReportService.HighValueOrders(orders, 40m));
                break;

            case "report-groups":
                Console.WriteLine(
                    "=== Групування за статусом ===");

                PrintGroups(
                    ReportService.ByStatus(orders));
                break;

            case "report-months":
                Console.WriteLine(
                    "=== Групування за місяцем ===");

                PrintGroups(
                    ReportService.ByMonth(orders));
                break;

            case "report-sum":
                Console.WriteLine(
                    "=== Загальні підсумки ===");

                PrintTotals(orders);
                break;

            case "report-orders-top":
                Console.WriteLine(
                    "=== Топ-5 окремих замовлень ===");

                PrintOrders(
                    ReportService.TopOrders(orders, 5));
                break;

            case "report-deferred":
                RunDeferred(orders);
                break;

            case "report-empty":
                RunEmpty();
                break;

            case "report-join":
                RunJoin(orders);
                break;

            case "report-query":
                RunQuerySyntax(orders);
                break;

            case "report-benchmark":
                RunBenchmark();
                break;

            default:
                Console.WriteLine(
                    "Невідома команда: " + command);

                PrintHelp();
                return 1;
        }

        return 0;
    }

    private static void PrintOrders(
        IReadOnlyList<OrderReportLine> lines)
    {
        Console.WriteLine(
            $"{"ID",-15} {"Клієнт",-8} " +
            $"{"Статус",-11} {"Сума",10}");

        foreach (OrderReportLine line in lines)
        {
            Console.WriteLine(
                $"{line.OrderId,-15} " +
                $"{line.CustomerId,-8} " +
                $"{line.Status,-11} " +
                $"{line.Total,10:F2}");
        }

        Console.WriteLine($"Знайдено: {lines.Count}");
    }

    private static void PrintCustomers(
        IReadOnlyList<CustomerTotalLine> lines)
    {
        Console.WriteLine(
            $"{"Клієнт",-10} {"Замовлень",10} {"Сума",12}");

        foreach (CustomerTotalLine line in lines)
        {
            Console.WriteLine(
                $"{line.CustomerId,-10} " +
                $"{line.OrdersCount,10} " +
                $"{line.Total,12:F2}");
        }
    }

    private static void PrintGroups(
        IReadOnlyList<GroupSummary> groups)
    {
        Console.WriteLine(
            $"{"Група",-15} {"Замовлень",10} {"Сума",12}");

        foreach (GroupSummary group in groups)
        {
            Console.WriteLine(
                $"{group.Key,-15} " +
                $"{group.Count,10} " +
                $"{group.Total,12:F2}");
        }
    }

    private static void PrintTotals(
        IEnumerable<Order> orders)
    {
        var totals = ReportService.Totals(orders);

        Console.WriteLine(
            $"Кількість замовлень: {totals.Orders}");

        Console.WriteLine(
            $"Загальна сума: {totals.Total:F2}");

        Console.WriteLine(
            $"Середній чек: {totals.Average:F2}");

        Console.WriteLine(
            $"Максимальна сума: {totals.Max:F2}");
    }

    // Основне завдання:
    // відкладене виконання LINQ.
    private static void RunDeferred(
        IReadOnlyList<Order> orders)
    {
        Console.WriteLine(
            "=== Відкладене виконання LINQ ===");

        List<Order> source = orders.ToList();

        IEnumerable<OrderReportLine> query =
            ReportService.DeferredHighValue(
                source,
                40m);

        int before = query.Count();

        Console.WriteLine(
            $"Замовлень дорожчих за 40 до зміни: {before}");

        Order added = Order.Create(
            "C-DEMO",
            "O-LINQ-TEMP");

        added.AddLine(
            "P-TEMP",
            "Тестовий товар",
            999m,
            1);

        source.Add(added);

        int after = query.Count();

        Console.WriteLine(
            $"Після додавання замовлення: {after}");

        List<OrderReportLine> snapshot =
            query.ToList();

        source.Clear();

        Console.WriteLine(
            $"Після очищення джерела у знімку: " +
            $"{snapshot.Count}");

        Console.WriteLine(
            $"Різниця між першим і другим Count(): " +
            $"{after - before}");
    }

    // Перевірка порожньої колекції.
    private static void RunEmpty()
    {
        Console.WriteLine(
            "=== Порожнє джерело ===");

        var empty = new List<Order>();

        var high =
            ReportService.HighValueOrders(empty, 40m);

        var top =
            ReportService.TopCustomers(empty, 5);

        var groups =
            ReportService.ByStatus(empty);

        Console.WriteLine(
            $"Дорогих замовлень: {high.Count}");

        Console.WriteLine(
            $"Клієнтів у топі: {top.Count}");

        Console.WriteLine(
            $"Груп: {groups.Count}");

        PrintTotals(empty);
    }

    // Додаткове №1: GroupJoin.
    private static void RunJoin(
        IReadOnlyList<Order> orders)
    {
        Console.WriteLine(
            "=== Додаткове №1: GroupJoin ===");

        IReadOnlyList<CustomerOrderSummary> result =
            ReportService.CustomerOrders(
                SampleData.Customers(),
                orders);

        Console.WriteLine(
            $"{"ID",-8} {"Ім'я",-18} " +
            $"{"Замовлень",10} {"Сума",12}");

        foreach (CustomerOrderSummary line in result)
        {
            Console.WriteLine(
                $"{line.CustomerId,-8} " +
                $"{line.CustomerName,-18} " +
                $"{line.OrdersCount,10} " +
                $"{line.Total,12:F2}");
        }
    }

    // Додаткове №3: порівняння синтаксисів.
    private static void RunQuerySyntax(
        IReadOnlyList<Order> orders)
    {
        Console.WriteLine(
            "=== Додаткове №3: синтаксис LINQ ===");

        IReadOnlyList<GroupSummary> methodResult =
            ReportService.ByStatus(orders);

        IReadOnlyList<GroupSummary> queryResult =
            ReportService.ByStatusQuery(orders);

        Console.WriteLine("Методний синтаксис:");
        PrintGroups(methodResult);

        Console.WriteLine();
        Console.WriteLine("Синтаксис запитів:");
        PrintGroups(queryResult);

        Console.WriteLine();
        Console.WriteLine(
            $"Результати однакові: " +
            $"{methodResult.SequenceEqual(queryResult)}");
    }

    // Додаткове №4: Stopwatch на 100 000 записах.
    private static void RunBenchmark()
    {
        Console.WriteLine(
            "=== Додаткове №4: Stopwatch ===");

        var orders = new List<Order>(100_000);

        DateTime createdAt = new DateTime(
            2026, 10, 1, 12, 0, 0,
            DateTimeKind.Utc);

        for (int i = 1; i <= 100_000; i++)
        {
            Order order = Order.Create(
                "C-BENCH",
                $"O-BENCH-{i:000000}",
                createdAt);

            order.AddLine(
                "P-BENCH",
                "Benchmark item",
                1m + i % 250,
                1 + i % 3);

            orders.Add(order);
        }

        // Прогрів перед вимірюванням.
        IReadOnlyList<OrderReportLine> normal =
            ReportService.BenchmarkTop(
                orders, 100m, 5, false);

        IReadOnlyList<OrderReportLine> early =
            ReportService.BenchmarkTop(
                orders, 100m, 5, true);

        double withoutIntermediate =
            Measure(orders, false);

        double withIntermediate =
            Measure(orders, true);

        Console.WriteLine(
            $"Записів: {orders.Count}");

        Console.WriteLine(
            $"Без проміжного ToList(): " +
            $"{withoutIntermediate:F3} мс");

        Console.WriteLine(
            $"Із проміжним ToList(): " +
            $"{withIntermediate:F3} мс");

        Console.WriteLine(
            $"Результати однакові: " +
            $"{normal.SequenceEqual(early)}");

        Console.WriteLine(
            "Показано середній час одного виконання " +
            "за 3 повтори.");
    }

    private static double Measure(
        IReadOnlyList<Order> orders,
        bool intermediateToList)
    {
        const int repeats = 3;

        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < repeats; i++)
        {
            IReadOnlyList<OrderReportLine> result =
                ReportService.BenchmarkTop(
                    orders,
                    100m,
                    5,
                    intermediateToList);

            if (result.Count != 5)
            {
                throw new InvalidOperationException(
                    "Некоректний результат тесту.");
            }
        }

        stopwatch.Stop();

        return stopwatch.Elapsed.TotalMilliseconds / repeats;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Доступні команди:");
        Console.WriteLine("report-top");
        Console.WriteLine("report-high");
        Console.WriteLine("report-groups");
        Console.WriteLine("report-months");
        Console.WriteLine("report-sum");
        Console.WriteLine("report-orders-top");
        Console.WriteLine("report-deferred");
        Console.WriteLine("report-empty");
        Console.WriteLine("report-join");
        Console.WriteLine("report-query");
        Console.WriteLine("report-benchmark");
    }
}