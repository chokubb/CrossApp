using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingOrderStore : IOrderStore
{
    private readonly IOrderStore _inner;

    private IReadOnlyList<Order>? _listCache;

    private readonly Dictionary<string, Order?> _itemCache =
        new(StringComparer.OrdinalIgnoreCase);

    public int CacheHits { get; private set; }
    public int CacheMisses { get; private set; }

    public CachingOrderStore(IOrderStore inner)
    {
        _inner = inner
            ?? throw new ArgumentNullException(nameof(inner));
    }

    public IReadOnlyList<Order> List()
    {
        if (_listCache is not null)
        {
            CacheHits++;
            return _listCache;
        }

        CacheMisses++;

        _listCache = _inner.List();

        return _listCache;
    }

    public Order? GetById(string id)
    {
        if (_itemCache.TryGetValue(id, out Order? cached))
        {
            CacheHits++;
            return cached;
        }

        CacheMisses++;

        Order? order = _inner.GetById(id);
        _itemCache[id] = order;

        return order;
    }

    public void Add(Order order)
    {
        _inner.Add(order);
        Invalidate();
    }

    public void Update(Order order)
    {
        _inner.Update(order);
        Invalidate();
    }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);

        if (removed)
            Invalidate();

        return removed;
    }

    private void Invalidate()
    {
        _listCache = null;
        _itemCache.Clear();
    }
}