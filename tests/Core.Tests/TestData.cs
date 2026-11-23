using Core.Domain;

namespace Core.Tests;

internal static class TestData
{
    public static Order CreateOrder(
        string id,
        decimal total,
        string customerId = "C-TEST")
    {
        Order order = Order.Create(
            customerId,
            id,
            new DateTime(
                2026, 11, 1, 12, 0, 0,
                DateTimeKind.Utc));

        order.AddLine(
            $"P-{id}",
            "Test item",
            total,
            1);

        return order;
    }
}