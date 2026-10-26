namespace Core.Domain;

public static class CustomerOrderPolicy
{
    public static void EnsureCanCreate(
        Customer customer,
        IReadOnlyCollection<Order> existingOrders)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(existingOrders);

        int openOrders = existingOrders.Count(order =>
            order.CustomerId == customer.Id &&
            order.Status == OrderStatus.Draft);

        if (openOrders >= 5)
        {
            throw new InvalidOperationException(
                $"Клієнт {customer.Id} уже має " +
                $"{openOrders} відкритих замовлень. " +
                "Створювати нове замовлення не можна.");
        }
    }
}