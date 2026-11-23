namespace Core.Reports;

public static class TopByExtensions
{
    public static IEnumerable<T> TopBy<T>(
        this IEnumerable<T> source,
        Func<T, decimal> scoreSelector,
        Func<T, string> idSelector,
        int n)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(scoreSelector);
        ArgumentNullException.ThrowIfNull(idSelector);

        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n));
        }

        return source
            .OrderByDescending(scoreSelector)
            .ThenBy(idSelector, StringComparer.Ordinal)
            .Take(n);
    }
}