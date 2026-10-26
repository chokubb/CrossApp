using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

public static class OrderServiceDemo
{
    public static int Run(
        OrderService service,
        IOrderStore store)
    {
        Console.WriteLine(
            "=== Лабораторна 5: сервіс і сховища ===");

        Console.WriteLine(
            $"Сховище: {store.GetType().Name}");

        Console.WriteLine();

        int initialCount = service.All().Count;

        Console.WriteLine(
            $"Початкова кількість замовлень: {initialCount}");

        // Створення замовлення

        Order created = service.CreateOrder("C-DEMO");

        // Додавання товарів

        service.AddLine(
            created.Id,
            "P-001",
            "Wireless Mouse",
            24.90m,
            2);

        service.AddLine(
            created.Id,
            "P-002",
            "USB-C Hub",
            39.99m,
            1);

        // Підтвердження

        service.Confirm(created.Id);

        Console.WriteLine();
        Console.WriteLine("Створено та підтверджено:");

        Console.WriteLine(service.Find(created.Id));

        Console.WriteLine();

        IReadOnlyList<Order> allOrders = service.All();

        Console.WriteLine(
            $"Кількість після додавання: {allOrders.Count}");

        Console.WriteLine();
        Console.WriteLine("Перші 5 замовлень:");

        foreach (Order order in allOrders.Take(5))
        {
            Console.WriteLine(order);
        }

        // Пошук за ID

        Console.WriteLine();
        Console.WriteLine("Пошук за ідентифікатором:");

        Order? found = service.Find(created.Id);

        Console.WriteLine(
            found is null
                ? "Замовлення не знайдено"
                : found.ToString());

        // Додаткове №2: пошук через Func

        Console.WriteLine();
        Console.WriteLine("=== Пошук через Func ===");

        IReadOnlyList<Order> confirmedOrders =
            service.Search(order =>
                order.Status == OrderStatus.Confirmed);

        Console.WriteLine(
            $"Підтверджених замовлень: {confirmedOrders.Count}");

        // Сценарії відмов

        Console.WriteLine();
        Console.WriteLine("=== Сценарії відмов ===");

        TryDo(
            "Додавання дубліката ID",
            () => store.Add(created));

        TryDo(
            "Підтвердження неіснуючого замовлення",
            () => service.Confirm("O-NOT-FOUND"));

        // Додаткове №1: перевірка кешування

        if (store is CachingOrderStore cache)
        {
            Console.WriteLine();
            Console.WriteLine("=== Статистика кешування ===");

            service.Find(created.Id);
            service.Find(created.Id);

            service.All();
            service.All();

            Console.WriteLine(
                $"Влучання в кеш: {cache.CacheHits}");

            Console.WriteLine(
                $"Промахи кешу: {cache.CacheMisses}");
        }

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
                $"! {title}: виняток не виник");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(
                $"{title}: {ex.GetType().Name} — {ex.Message}");
        }
    }
}