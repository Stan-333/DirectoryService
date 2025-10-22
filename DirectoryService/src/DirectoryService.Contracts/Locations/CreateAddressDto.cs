namespace DirectoryService.Contracts.Locations;

public record CreateAddressDto(
    string PostalCode,
    string Region,
    string City,
    string Street,
    string House,
    string? Apartment);