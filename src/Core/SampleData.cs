using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Order> Orders()
    {
        var orders = new List<Order>();

        for (int i = 1; i <= 15; i++)
        {
            string customerId = $"C-{(i % 4) + 1:000}";
            string orderId = $"O-SEED-{i:000}";

            DateTime createdAt = new DateTime(
                2026, 10, 1, 12, 0, 0,
                DateTimeKind.Utc).AddDays(i);

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
}