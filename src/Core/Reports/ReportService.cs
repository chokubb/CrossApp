using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    // 1. Замовлення дорожчі за задану суму.
    public static IReadOnlyList<OrderReportLine> HighValueOrders(
        IEnumerable<Order> source,
        decimal threshold)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .Where(order => order.Total > threshold)
            .OrderByDescending(order => order.Total)
            .ThenBy(order => order.Id, StringComparer.Ordinal)
            .Select(ToLine)
            .ToList()
            .AsReadOnly();
    }

    // 2. Топ клієнтів за загальною сумою замовлень.
    public static IReadOnlyList<CustomerTotalLine> TopCustomers(
        IEnumerable<Order> source,
        int n)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .GroupBy(
                order => order.CustomerId,
                StringComparer.OrdinalIgnoreCase)
            .Select(group => new CustomerTotalLine(
                group.Key,
                group.Count(),
                group.Sum(order => order.Total)))
            .TopBy(
                customer => customer.Total,
                customer => customer.CustomerId,
                n)
            .ToList()
            .AsReadOnly();
    }

    // 3. Топ окремих замовлень.
    // Друге використання власного TopBy.
    public static IReadOnlyList<OrderReportLine> TopOrders(
        IEnumerable<Order> source,
        int n)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .TopBy(
                order => order.Total,
                order => order.Id,
                n)
            .Select(ToLine)
            .ToList()
            .AsReadOnly();
    }

    // 4. Групування замовлень за статусом.
    public static IReadOnlyList<GroupSummary> ByStatus(
        IEnumerable<Order> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .GroupBy(order => order.Status)
            .Select(group => new GroupSummary(
                group.Key.ToString(),
                group.Count(),
                group.Sum(order => order.Total)))
            .OrderByDescending(group => group.Total)
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .ToList()
            .AsReadOnly();
    }

    // 5. Групування за місяцем.
    public static IReadOnlyList<GroupSummary> ByMonth(
        IEnumerable<Order> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .GroupBy(order => new
            {
                order.CreatedAt.Year,
                order.CreatedAt.Month
            })
            .Select(group => new GroupSummary(
                $"{group.Key.Year:D4}-{group.Key.Month:D2}",
                group.Count(),
                group.Sum(order => order.Total)))
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToList()
            .AsReadOnly();
    }

    // 6. Загальні підсумки.
    // Порожня колекція не викликає винятку.
    public static (
        int Orders,
        decimal Total,
        decimal Average,
        decimal Max) Totals(
            IEnumerable<Order> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        List<Order> orders = source.ToList();

        if (orders.Count == 0)
        {
            return (0, 0m, 0m, 0m);
        }

        return (
            orders.Count,
            orders.Sum(order => order.Total),
            orders.Average(order => order.Total),
            orders.Max(order => order.Total));
    }

    // 7. Додаткове №1: GroupJoin клієнтів і замовлень.
    public static IReadOnlyList<CustomerOrderSummary> CustomerOrders(
        IEnumerable<Customer> customers,
        IEnumerable<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(customers);
        ArgumentNullException.ThrowIfNull(orders);

        return customers
            .GroupJoin(
                orders,
                customer => customer.Id,
                order => order.CustomerId,
                (customer, customerOrders) =>
                    new CustomerOrderSummary(
                        customer.Id,
                        customer.Name,
                        customerOrders.Count(),
                        customerOrders.Sum(order => order.Total)),
                StringComparer.OrdinalIgnoreCase)
            .OrderBy(
                customer => customer.CustomerId,
                StringComparer.Ordinal)
            .ToList()
            .AsReadOnly();
    }

    // 8. Додаткове №3: той самий звіт,
    // але через синтаксис запитів LINQ.
    public static IReadOnlyList<GroupSummary> ByStatusQuery(
        IEnumerable<Order> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var query =
            from order in source
            group order by order.Status into groupByStatus
            let total = groupByStatus.Sum(order => order.Total)
            orderby total descending, groupByStatus.Key.ToString()
            select new GroupSummary(
                groupByStatus.Key.ToString(),
                groupByStatus.Count(),
                total);

        return query.ToList().AsReadOnly();
    }

    // 9. Демонстрація відкладеного виконання.
    // Навмисно без ToList(), щоб запит залишався відкладеним.
    public static IEnumerable<OrderReportLine> DeferredHighValue(
        IEnumerable<Order> source,
        decimal threshold)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .Where(order => order.Total > threshold)
            .Select(ToLine);
    }

    // 10. Додаткове №4: порівняння двох
    // варіантів матеріалізації LINQ-запиту.
    public static IReadOnlyList<OrderReportLine> BenchmarkTop(
        IEnumerable<Order> source,
        decimal minimumTotal,
        int n,
        bool intermediateToList)
    {
        ArgumentNullException.ThrowIfNull(source);

        IEnumerable<Order> filtered = source
            .Where(order => order.Total > minimumTotal);

        if (intermediateToList)
        {
            filtered = filtered.ToList();
        }

        return filtered
            .OrderByDescending(order => order.Total)
            .ThenBy(order => order.Id, StringComparer.Ordinal)
            .Take(n)
            .Select(ToLine)
            .ToList()
            .AsReadOnly();
    }

    private static OrderReportLine ToLine(Order order)
    {
        return new OrderReportLine(
            order.Id,
            order.CustomerId,
            order.CreatedAt,
            order.Status.ToString(),
            order.Lines.Count,
            order.Total);
    }
}