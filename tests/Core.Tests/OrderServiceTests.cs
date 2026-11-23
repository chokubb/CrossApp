using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class OrderServiceTests
{
    [Fact]
    public void AddLine_AfterConfirmation_ThrowsDomainException()
    {
        var service = new OrderService(
            new InMemoryOrderStore());

        Order order = service.CreateOrder(
            "C-001",
            "O-001");

        service.AddLine(
            order.Id, "P-001", "Keyboard", 100m, 1);

        service.Confirm(order.Id);

        Assert.Throws<DomainException>(() =>
            service.AddLine(
                order.Id, "P-002", "Mouse", 25m, 1));

        Assert.Single(order.Lines);
        Assert.Equal(100m, order.Total);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void AddLine_ZeroQuantity_ThrowsArgumentException()
    {
        var service = new OrderService(
            new InMemoryOrderStore());

        Order order = service.CreateOrder("C-001");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            service.AddLine(
                order.Id, "P-001", "Mouse", 20m, 0));

        Assert.Empty(order.Lines);
        Assert.Equal(0m, order.Total);
    }

    [Fact]
    public void AddLine_RaisesChangedExactlyOnce()
    {
        var service = new OrderService(
            new InMemoryOrderStore());

        Order order = service.CreateOrder("C-001");

        var events = new List<OrderChangedEventArgs>();

        service.Changed += (_, e) => events.Add(e);

        service.AddLine(
            order.Id, "P-001", "Mouse", 20m, 1);

        Assert.Single(events);
        Assert.Equal(
            OrderChangeKind.LineAdded,
            events[0].Kind);

        Assert.Equal(order.Id, events[0].OrderId);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            service.AddLine(
                order.Id, "P-002", "Bad", 5m, 0));

        Assert.Single(events);
    }

    [Fact]
    public void Confirm_EmptyOrder_ThrowsDomainException()
    {
        var service = new OrderService(
            new InMemoryOrderStore());

        Order order = service.CreateOrder("C-001");

        Assert.Throws<DomainException>(() =>
            service.Confirm(order.Id));

        Assert.Equal(OrderStatus.Draft, order.Status);
    }
}