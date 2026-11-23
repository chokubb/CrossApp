using Core.Domain;
using Core.Reports;
using Xunit;

namespace Core.Tests;

public class ReportServiceTests
{
    [Theory]
    [InlineData(10, 2)]
    [InlineData(40, 1)]
    [InlineData(100, 0)]
    public void HighValueOrders_FiltersCorrectly(
        int threshold,
        int expected)
    {
        var orders = new List<Order>
        {
            TestData.CreateOrder("O-001", 20m),
            TestData.CreateOrder("O-002", 50m)
        };

        IReadOnlyList<OrderReportLine> result =
            ReportService.HighValueOrders(
                orders,
                threshold);

        Assert.Equal(expected, result.Count);
    }

    [Fact]
    public void TopOrders_UsesIdToBreakTies()
    {
        var orders = new List<Order>
        {
            TestData.CreateOrder("O-B", 50m),
            TestData.CreateOrder("O-C", 10m),
            TestData.CreateOrder("O-A", 50m)
        };

        IReadOnlyList<OrderReportLine> result =
            ReportService.TopOrders(orders, 2);

        Assert.Equal(
            new[] { "O-A", "O-B" },
            result.Select(item => item.OrderId));
    }

    [Fact]
    public void Totals_EmptySource_ReturnsZeros()
    {
        var result = ReportService.Totals(
            Array.Empty<Order>());

        Assert.Equal(0, result.Orders);
        Assert.Equal(0m, result.Total);
        Assert.Equal(0m, result.Average);
        Assert.Equal(0m, result.Max);
    }
}