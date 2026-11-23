namespace Core.Reports;

public sealed record OrderReportLine(
    string OrderId,
    string CustomerId,
    DateTime CreatedAt,
    string Status,
    int LinesCount,
    decimal Total);