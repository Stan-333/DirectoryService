using System.Text.RegularExpressions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Locations;

public record Address
{
    public string PostalCode { get; }

    public string Region { get; }

    public string City { get; }

    public string Street { get; }

    public string House { get; }

    public string? Apartment { get; }

    private Address(string postalCode, string region, string city, string street, string house, string? apartment)
    {
        PostalCode = postalCode;
        Region = region;
        City = city;
        Street = street;
        House = house;
        Apartment = apartment;
    }

    public static Result<Address> Create(string postalCode, string region, string city, string street, string house, string? apartment = null)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return Result<Address>.Failure("Индекс обязателен.");

        if (!Regex.IsMatch(postalCode, @"^\d{6}"))
            return Result<Address>.Failure("Индекс должен состоять из 6 цифр.");

        if (string.IsNullOrWhiteSpace(region))
            return Result<Address>.Failure("Регион обязателен.");

        if (string.IsNullOrWhiteSpace(city))
            return Result<Address>.Failure("Город обязателен.");

        if (string.IsNullOrWhiteSpace(street))
            return Result<Address>.Failure("Улица обязательна.");

        if (string.IsNullOrWhiteSpace(house))
            return Result<Address>.Failure("Дом обязателен.");

        return Result<Address>.Success(new Address(
            postalCode.Trim(),
            region.Trim(),
            city.Trim(),
            street.Trim(),
            house.Trim(),
            apartment?.Trim()));
    }

    public override string ToString()
    {
        return $"{PostalCode}, {Region}, г. {City}, ул. {Street}, д. {House}"
               + (string.IsNullOrEmpty(Apartment) ? string.Empty : $", кв. {Apartment}");
    }
}