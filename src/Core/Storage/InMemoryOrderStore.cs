using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryOrderStore : IOrderStore
{
    private readonly Dictionary<string, Order> _orders;

    public InMemoryOrderStore(IEnumerable<Order>? seed = null)
    {
        _orders = (seed ?? [])
            .ToDictionary(
                order => order.Id,
                StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<Order> List()
    {
        return _orders.Values.ToList().AsReadOnly();
    }

    public Order? GetById(string id)
    {
        return _orders.GetValueOrDefault(id);
    }

    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (_orders.ContainsKey(order.Id))
        {
            throw new InvalidOperationException(
                $"Замовлення з id={order.Id} уже існує.");
        }

        _orders.Add(order.Id, order);
    }

    public void Update(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (!_orders.ContainsKey(order.Id))
        {
            throw new InvalidOperationException(
                $"Неможливо оновити замовлення: id={order.Id} не знайдено.");
        }

        _orders[order.Id] = order;
    }

    public bool Remove(string id)
    {
        return _orders.Remove(id);
    }
}