namespace DirectoryService.Contracts.Locations;

public record CreateLocationRequest(
    string Name,
    CreateAddressDto Address,
    string Timezone);