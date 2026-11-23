namespace Core.Reports;

public sealed record CustomerTotalLine(
    string CustomerId,
    int OrdersCount,
    decimal Total);