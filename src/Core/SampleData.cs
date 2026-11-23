using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Order> Orders()
    {
        var orders = new List<Order>();

        DateTime startDate = new DateTime(
            2026, 7, 1, 12, 0, 0,
            DateTimeKind.Utc);

        for (int i = 1; i <= 18; i++)
        {
            string customerId = $"C-{(i % 4) + 1:000}";
            string orderId = $"O-SEED-{i:000}";

            DateTime createdAt =
                startDate.AddDays((i - 1) * 6);

            Order order = Order.Create(
                customerId,
                orderId,
                createdAt);

            order.AddLine(
                $"P-{i:000}",
                $"Тестовий товар {i}",
                10m + i,
                i % 3 + 1);

            if (i % 3 == 0)
            {
                order.Confirm();
            }
            else if (i % 7 == 0)
            {
                order.Cancel();
            }

            orders.Add(order);
        }

        return orders;
    }

    // Для додаткового №1, п'ятий клієнт без замовлень.
    public static IReadOnlyList<Customer> Customers()
    {
        return new List<Customer>
        {
            Customer.Create("C-001", "Alice Brown"),
            Customer.Create("C-002", "Mark Wilson"),
            Customer.Create("C-003", "Emma Clark"),
            Customer.Create("C-004", "John Smith"),
            Customer.Create("C-005", "Olivia Davis")
        };
    }
}