using Core.Domain;
using Core.Dto;
using Core.Import;

public static class DomainDemo
{
    public static int Run()
    {
        Console.WriteLine("=== Лабораторна 4: доменна модель ===");
        Console.WriteLine();

        // Сценарій 1: успішне створення замовлення

        Console.WriteLine("=== 1. Успішний сценарій ===");

        Customer customer = Customer.Create(
            "C-001",
            "Alice Brown",
            "alice@example.com");

        Order order = Order.Create(
            customer.Id,
            "O-001",
            new DateTime(
                2026, 10, 26, 12, 0, 0,
                DateTimeKind.Utc));

        Console.WriteLine(order);

        order.AddLine(
            "P-001",
            "Wireless Mouse",
            24.90m,
            2);

        order.AddLine(
            "P-002",
            "USB-C Hub",
            39.99m,
            1);

        Console.WriteLine("Після додавання товарів:");
        Console.WriteLine(order);

        order.Confirm();

        Console.WriteLine("Після підтвердження:");
        Console.WriteLine(order);

        // Перевірка ToDto та FromDto

        Console.WriteLine();
        Console.WriteLine("=== 2. ToDto / FromDto ===");

        OrderDto dto = order.ToDto();
        Order restored = Order.FromDto(dto);

        Console.WriteLine("Відновлене замовлення:");
        Console.WriteLine(restored);

        // Сценарій 2: порушення інваріантів

        Console.WriteLine();
        Console.WriteLine("=== 3. Порушення інваріантів ===");

        int originalCount = order.Lines.Count;
        decimal originalTotal = order.Total;
        OrderStatus originalStatus = order.Status;

        TryDo(
            "Додавання після підтвердження",
            () => order.AddLine(
                "P-003", "Headphones", 50m, 1));

        TryDo(
            "Повторне підтвердження",
            () => order.Confirm());

        TryDo(
            "Порожній CustomerId",
            () => Order.Create(" "));

        TryDo(
            "Від'ємна ціна",
            () => OrderLine.Create(
                "P-004", "Keyboard", -10m, 1));

        TryDo(
            "Нульова кількість",
            () => OrderLine.Create(
                "P-005", "Monitor", 100m, 0));

        Order emptyOrder = Order.Create(
            customer.Id,
            "O-EMPTY");

        TryDo(
            "Підтвердження порожнього замовлення",
            () => emptyOrder.Confirm());

        TryDo(
            "Скасування підтвердженого замовлення",
            () => order.Cancel());

        bool unchanged =
            order.Lines.Count == originalCount &&
            order.Total == originalTotal &&
            order.Status == originalStatus;

        Console.WriteLine();
        Console.WriteLine(
            $"Стан замовлення не змінився після відмов: " +
            $"{(unchanged ? "ТАК" : "НІ")}");

        Console.WriteLine(order);

        // Перевіряємо, що FromDto теж захищає правила

        Console.WriteLine();
        Console.WriteLine("=== 4. Перевірка пошкодженого DTO ===");

        OrderDto invalidDto = new(
            "O-BAD",
            customer.Id,
            DateTime.UtcNow,
            new[]
            {
                new OrderLineDto(
                    "P-999", "Invalid Product", -5m, 1)
            },
            "Draft");

        TryDo(
            "Відновлення DTO з від'ємною ціною",
            () => Order.FromDto(invalidDto));

        // Додаткове №1: імпорт DTO -> доменні сутності

        Console.WriteLine();
        Console.WriteLine("=== 5. Додаткове 1: імпорт ===");

        string jsonPath = Path.Combine(
            "data", "lab04-products.json");

        if (!File.Exists(jsonPath))
        {
            Console.WriteLine(
                $"Файл не знайдено: {Path.GetFullPath(jsonPath)}");
            return 1;
        }

        ImportResult<ProductDto> imported =
            ProductJsonImporter.Load(jsonPath);

        ImportResult<OrderLine> mapped =
            DomainImportMapper.MapToOrderLines(imported);

        Console.WriteLine(
            $"Прочитано DTO: {imported.Items.Count}");

        Console.WriteLine(
            $"Створено сутностей: {mapped.Items.Count}");

        Console.WriteLine(
            $"Помилок: {mapped.Errors.Count}");

        foreach (string error in mapped.Errors)
            Console.WriteLine($"! {error}");

        // Додаткове №2: правило між Customer та Order

        Console.WriteLine();
        Console.WriteLine("=== 6. Додаткове 2: ліміт замовлень ===");

        List<Order> openOrders = Enumerable
            .Range(1, 5)
            .Select(i => Order.Create(
                customer.Id,
                $"O-LIMIT-{i}"))
            .ToList();

        TryDo(
            "Створення шостого відкритого замовлення",
            () => CustomerOrderPolicy.EnsureCanCreate(
                customer,
                openOrders));

        openOrders[0].Cancel();

        CustomerOrderPolicy.EnsureCanCreate(
            customer,
            openOrders);

        Console.WriteLine(
            "Після скасування одного замовлення " +
            "створення нового дозволено.");

        // Додаткове №3: стани вже перевірені вище

        Console.WriteLine();
        Console.WriteLine("=== 7. Додаткове 3: OrderStatus ===");

        Order cancelledOrder = Order.Create(
            customer.Id,
            "O-CANCEL");

        cancelledOrder.Cancel();

        Console.WriteLine(
            $"Стан після Cancel(): {cancelledOrder.Status}");

        TryDo(
            "Підтвердження скасованого замовлення",
            () => cancelledOrder.Confirm());

        Console.WriteLine();
        Console.WriteLine("Демонстрацію завершено.");

        return 0;
    }

    private static void TryDo(
        string title,
        Action action)
    {
        try
        {
            action();

            Console.WriteLine(
                $"! {title}: виняток НЕ спрацював");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"{title}: {ex.GetType().Name} — {ex.Message}");
        }
    }
}