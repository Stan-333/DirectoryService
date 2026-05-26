namespace DirectoryService.Contracts.Locations;

public record GetLocationDto
{
    public Guid LocationId { get; init; }

    public required string LocationName { get; init; }

    public required AddressDto Address { get; set; }

    public required string TimeZone { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}