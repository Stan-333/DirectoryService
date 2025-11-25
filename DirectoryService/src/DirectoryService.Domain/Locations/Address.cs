using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Locations;

public record Address
{
    public string PostalCode { get; }

    public string Region { get; }

    public string City { get; }

    public string Street { get; }

    public string House { get; }

    public string? Apartment { get; }

    // EF Core
    private Address() { }

    private Address(string postalCode, string region, string city, string street, string house, string? apartment)
    {
        PostalCode = postalCode;
        Region = region;
        City = city;
        Street = street;
        House = house;
        Apartment = apartment;
    }

    public static Result<Address, Error> Create(string postalCode, string region, string city, string street, string house, string? apartment = null)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return GeneralErrors.ValueIsRequired("postal code");

        if (!Regex.IsMatch(postalCode, @"^\d{6}"))
            return GeneralErrors.ValueIsInvalid("postal code");

        if (string.IsNullOrWhiteSpace(region))
            return GeneralErrors.ValueIsRequired("region");
        if (region.Length is < LengthConstants.LENGTH1 or > LengthConstants.LENGTH100)
            return GeneralErrors.ValueIsInvalid("region");

        if (string.IsNullOrWhiteSpace(city))
            return GeneralErrors.ValueIsRequired("city");
        if (city.Length is < LengthConstants.LENGTH1 or > LengthConstants.LENGTH100)
            return GeneralErrors.ValueIsInvalid("city");

        if (string.IsNullOrWhiteSpace(street))
            return GeneralErrors.ValueIsRequired("street");
        if (street.Length is < LengthConstants.LENGTH1 or > LengthConstants.LENGTH100)
            return GeneralErrors.ValueIsInvalid("street");

        if (string.IsNullOrWhiteSpace(house))
            return GeneralErrors.ValueIsRequired("house");
        if (house.Length is < LengthConstants.LENGTH1 or > LengthConstants.LENGTH10)
            return GeneralErrors.ValueIsInvalid("house");

        if (!string.IsNullOrWhiteSpace(apartment))
        {
            if (apartment.Length is < LengthConstants.LENGTH1 or > LengthConstants.LENGTH10)
                return GeneralErrors.ValueIsInvalid("apartment");
        }

        return new Address(
            postalCode.Trim(),
            region.Trim(),
            city.Trim(),
            street.Trim(),
            house.Trim(),
            apartment?.Trim());
    }

    public override string ToString()
    {
        return $"{PostalCode}, {Region}, г. {City}, ул. {Street}, д. {House}"
               + (string.IsNullOrEmpty(Apartment) ? string.Empty : $", кв. {Apartment}");
    }
}