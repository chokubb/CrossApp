using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Events;

namespace Core.Services;

public sealed class OrderService
{
    private readonly IOrderStore _store;
    private readonly IReadOnlyList<INotificationSink> _sinks;

    public OrderService(
        IOrderStore store,
        params INotificationSink[] sinks)
    {
        _store = store
            ?? throw new ArgumentNullException(nameof(store));

        _sinks = sinks
            ?? throw new ArgumentNullException(nameof(sinks));
    }

    public event EventHandler<OrderChangedEventArgs>? Changed;

    private void OnChanged(Order order, OrderChangeKind kind)
    {
        var e = new OrderChangedEventArgs(
            order.Id,
            kind,
            order.Lines.Count,
            order.Total);

        Changed?.Invoke(this, e);

        foreach (INotificationSink sink in _sinks)
        {
            sink.Notify(e);
        }
    }

    public Order CreateOrder(
        string customerId,
        string? id = null)
    {
        Order order = Order.Create(customerId, id);

        _store.Add(order);

        OnChanged(order, OrderChangeKind.OrderCreated);

        return order;
    }

    public void AddLine(
        string orderId,
        string productId,
        string name,
        decimal price,
        int quantity)
    {
        Order order = GetRequired(orderId);

        order.AddLine(
            productId,
            name,
            price,
            quantity);

        _store.Update(order);

        OnChanged(order, OrderChangeKind.LineAdded);
    }

    public void Confirm(string orderId)
    {
        Order order = GetRequired(orderId);

        order.Confirm();

        _store.Update(order);

        OnChanged(order, OrderChangeKind.OrderConfirmed);
    }

    public void Cancel(string orderId)
    {
        Order order = GetRequired(orderId);

        order.Cancel();

        _store.Update(order);

        OnChanged(order, OrderChangeKind.OrderCancelled);
    }

    public IReadOnlyList<Order> All()
    {
        return _store.List();
    }

    public Order? Find(string id)
    {
        return _store.GetById(id);
    }

    public bool Remove(string id)
    {
        Order? order = _store.GetById(id);

        if (order is null)
            return false;

        bool removed = _store.Remove(id);

        if (removed)
        {
            OnChanged(order, OrderChangeKind.OrderRemoved);
        }

        return removed;
    }

    public IReadOnlyList<Order> Search(
        Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return _store.List()
            .Where(predicate)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<Order> FindAll(
        Func<Order, bool> predicate)
    {
        return Search(predicate);
    }

    // Основне завдання: асинхронний імпорт.
    public async Task<int> ImportAsync(
        IOrderLineImporter importer,
        string orderId,
        string path,
        CancellationToken ct = default,
        IProgress<int>? progress = null)
    {
        ArgumentNullException.ThrowIfNull(importer);

        ct.ThrowIfCancellationRequested();

        Order order = GetRequired(orderId);

        EnsureDraft(order);

        IReadOnlyList<OrderLineDto> lines =
            await importer.LoadAsync(path, ct);

        ct.ThrowIfCancellationRequested();

        int added = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            OrderLineDto line = lines[i];

            if (!ContainsProduct(order, line.ProductId))
            {
                AddLine(
                    orderId,
                    line.ProductId,
                    line.Name,
                    line.Price,
                    line.Quantity);

                added++;
            }

            if (lines.Count > 0)
            {
                int percent =
                    (int)((i + 1) * 100L / lines.Count);

                progress?.Report(percent);
            }
        }

        if (lines.Count == 0)
        {
            progress?.Report(100);
        }

        return added;
    }

    // Додаткове №1: потоковий імпорт.
    public async Task<int> ImportStreamAsync(
        IOrderLineImporter importer,
        string orderId,
        string path,
        CancellationToken ct = default,
        IProgress<int>? progress = null)
    {
        ArgumentNullException.ThrowIfNull(importer);

        ct.ThrowIfCancellationRequested();

        Order order = GetRequired(orderId);

        EnsureDraft(order);

        int added = 0;
        int processed = 0;

        await foreach (OrderLineDto line in
            importer.StreamAsync(path, ct)
                .WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();

            processed++;

            if (!ContainsProduct(order, line.ProductId))
            {
                AddLine(
                    orderId,
                    line.ProductId,
                    line.Name,
                    line.Price,
                    line.Quantity);

                added++;
            }

            // Для потокового режиму це кількість
            // оброблених записів, а не відсоток.
            progress?.Report(processed);
        }

        return added;
    }

    private static void EnsureDraft(Order order)
    {
        if (order.Status != OrderStatus.Draft)
        {
            throw new DomainException(
                $"Замовлення {order.Id} має стан " +
                $"{order.Status}. Імпорт дозволено " +
                "лише для чернетки.");
        }
    }

    private static bool ContainsProduct(
        Order order,
        string productId)
    {
        return order.Lines.Any(line =>
            string.Equals(
                line.ProductId,
                productId,
                StringComparison.OrdinalIgnoreCase));
    }

    private Order GetRequired(string id)
    {
        return _store.GetById(id)
            ?? throw new InvalidOperationException(
                $"Замовлення з id={id} не знайдено.");
    }
}