using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileOrderStore : IOrderStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    private readonly string _path;

    private readonly Dictionary<string, Order> _orders =
        new(StringComparer.OrdinalIgnoreCase);

    private bool _loaded;

    public FileOrderStore(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "Шлях до файла не може бути порожнім",
                nameof(path));
        }

        _path = Path.GetFullPath(path);
    }

    private void EnsureLoaded()
    {
        if (_loaded)
            return;

        if (File.Exists(_path))
        {
            string json = File.ReadAllText(_path);

            List<OrderDto> dtos =
                JsonSerializer.Deserialize<List<OrderDto>>(
                    json, Options) ?? [];

            foreach (OrderDto dto in dtos)
            {
                Order order = Order.FromDto(dto);
                _orders.Add(order.Id, order);
            }
        }

        _loaded = true;
    }

    private void Save()
    {
        string? directory = Path.GetDirectoryName(_path);

        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        List<OrderDto> dtos = _orders.Values
            .Select(order => order.ToDto())
            .ToList();

        string json = JsonSerializer.Serialize(dtos, Options);

        File.WriteAllText(_path, json);
    }

    public IReadOnlyList<Order> List()
    {
        EnsureLoaded();

        return _orders.Values.ToList().AsReadOnly();
    }

    public Order? GetById(string id)
    {
        EnsureLoaded();

        return _orders.GetValueOrDefault(id);
    }

    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        EnsureLoaded();

        if (_orders.ContainsKey(order.Id))
        {
            throw new InvalidOperationException(
                $"Замовлення з id={order.Id} уже існує.");
        }

        _orders.Add(order.Id, order);
        Save();
    }

    public void Update(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        EnsureLoaded();

        if (!_orders.ContainsKey(order.Id))
        {
            throw new InvalidOperationException(
                $"Замовлення з id={order.Id} не знайдено.");
        }

        _orders[order.Id] = order;
        Save();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();

        bool removed = _orders.Remove(id);

        if (removed)
        {
            Save();
        }

        return removed;
    }
}