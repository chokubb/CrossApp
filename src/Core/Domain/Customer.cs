using Core.Dto;

namespace Core.Domain;

public sealed class Customer
{
    public string Id { get; }
    public string Name { get; }
    public string? Email { get; }

    private Customer(
        string id,
        string name,
        string? email)
    {
        Id = id;
        Name = name;
        Email = email;
    }

    public static Customer Create(
        string id,
        string name,
        string? email = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(
                "Ідентифікатор клієнта обов'язковий",
                nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Ім'я клієнта обов'язкове",
                nameof(name));

        return new Customer(
            id.Trim(),
            name.Trim(),
            string.IsNullOrWhiteSpace(email)
                ? null
                : email.Trim());
    }

    public CustomerDto ToDto() =>
        new(Id, Name, Email);

    public static Customer FromDto(CustomerDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return Create(dto.Id, dto.Name, dto.Email);
    }
}