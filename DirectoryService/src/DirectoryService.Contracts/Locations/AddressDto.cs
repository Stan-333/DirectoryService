namespace DirectoryService.Contracts.Locations;

public record AddressDto
{
    public required string PostalCode { get; init; }

    public required string Region { get; init; }

    public required string City { get; init; }

    public required string Street { get; init; }

    public required string House { get; init; }

    public string? Apartment { get; init; }
}