using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];
    private OrderStatus _status;

    public string Id { get; }
    public string CustomerId { get; }
    public DateTime CreatedAt { get; }

    public OrderStatus Status => _status;

    public IReadOnlyList<OrderLine> Lines =>
        _lines.AsReadOnly();

    public decimal Total =>
        _lines.Sum(line => line.Total);

    private Order(
        string id,
        string customerId,
        DateTime createdAt)
    {
        Id = id;
        CustomerId = customerId;
        CreatedAt = createdAt;
        _status = OrderStatus.Draft;
    }

    public static Order Create(
        string customerId,
        string? id = null,
        DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException(
                "Ідентифікатор клієнта не може бути порожнім",
                nameof(customerId));

        string orderId = id is null
            ? $"O-{Guid.NewGuid():N}"
            : id.Trim();

        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException(
                "Ідентифікатор замовлення не може бути порожнім",
                nameof(id));

        DateTime date = createdAt ?? DateTime.UtcNow;

        if (date == default)
            throw new ArgumentOutOfRangeException(
                nameof(createdAt),
                "Дата створення замовлення некоректна");

        return new Order(
            orderId,
            customerId.Trim(),
            date);
    }

    public void AddLine(
        string productId,
        string name,
        decimal price,
        int quantity)
    {
        if (_status != OrderStatus.Draft)
            throw new InvalidOperationException(
                $"Не можна додати товар: замовлення {Id} " +
                $"має стан {_status}");

        OrderLine line = OrderLine.Create(
            productId,
            name,
            price,
            quantity);

        _lines.Add(line);
    }

    public void Confirm()
    {
        if (_status != OrderStatus.Draft)
            throw new InvalidOperationException(
                $"Замовлення {Id} не можна підтвердити " +
                $"зі стану {_status}");

        if (_lines.Count == 0)
            throw new InvalidOperationException(
                $"Не можна підтвердити порожнє замовлення {Id}");

        ChangeStatus(OrderStatus.Confirmed);
    }

    public void Cancel()
    {
        ChangeStatus(OrderStatus.Cancelled);
    }

    private void ChangeStatus(OrderStatus nextStatus)
    {
        _status = (_status, nextStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed)
                => OrderStatus.Confirmed,

            (OrderStatus.Draft, OrderStatus.Cancelled)
                => OrderStatus.Cancelled,

            _ => throw new InvalidOperationException(
                $"Перехід замовлення {Id} зі стану " +
                $"{_status} до {nextStatus} заборонено")
        };
    }

    public OrderDto ToDto()
    {
        OrderLineDto[] lines = _lines
            .Select(line => line.ToDto())
            .ToArray();

        return new OrderDto(
            Id,
            CustomerId,
            CreatedAt,
            lines,
            Status.ToString());
    }

    public static Order FromDto(OrderDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        OrderStatus status = dto.Status switch
        {
            "Draft" => OrderStatus.Draft,
            "Confirmed" => OrderStatus.Confirmed,
            "Cancelled" => OrderStatus.Cancelled,

            _ => throw new ArgumentException(
                $"Невідомий стан замовлення: {dto.Status}")
        };

        Order order = Create(
            dto.CustomerId,
            dto.Id,
            dto.CreatedAt);

        foreach (OrderLineDto line in dto.Lines
                     ?? Array.Empty<OrderLineDto>())
        {
            ArgumentNullException.ThrowIfNull(line);

            order.AddLine(
                line.ProductId,
                line.Name,
                line.Price,
                line.Quantity);
        }

        switch (status)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                break;

            case OrderStatus.Cancelled:
                order.Cancel();
                break;
        }

        return order;
    }

    public override string ToString() =>
        $"Замовлення {Id} | Клієнт: {CustomerId} | " +
        $"Стан: {Status} | Рядків: {Lines.Count} | " +
        $"Сума: {Total:F2}";
}