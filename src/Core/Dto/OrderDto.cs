namespace Core.Dto;

public record OrderDto(
    string Id,
    string CustomerId,
    DateTime CreatedAt);