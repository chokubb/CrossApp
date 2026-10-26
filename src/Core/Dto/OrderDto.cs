namespace Core.Dto;

public record OrderDto(
    string Id,
    string CustomerId,
    DateTime CreatedAt,
    IReadOnlyList<OrderLineDto>? Lines = null,
    string Status = "Draft");