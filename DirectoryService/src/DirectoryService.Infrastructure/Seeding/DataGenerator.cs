using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using TimeZoneConverter;
using TimeZone = DirectoryService.Domain.Locations.TimeZone;

namespace DirectoryService.Infrastructure.Seeding;

public static class DataGenerator
{
    private static int _positionCounter = 1;

    private static readonly string[] _companyNames =
    [
        "Technology", "Innovation", "Digital", "Software", "Systems", "Solutions",
        "Analytics", "Development", "Engineering", "Consulting", "Management", "Research"
    ];

    private static readonly string[] _departmentTypes =
    [
        "Engineering", "Marketing", "Sales", "HR", "Finance", "Operations",
        "Support", "Legal", "IT", "Product", "Design", "Quality"
    ];

    private static readonly string[] _positionTitles =
    [
        "Senior", "Lead", "Principal", "Junior", "Middle", "Chief", "Head", "Director",
        "Manager", "Specialist", "Engineer", "Developer", "Analyst", "Consultant"
    ];

    private static readonly string[] _positionRoles =
    [
        "Engineer", "Developer", "Manager", "Analyst", "Specialist", "Consultant",
        "Designer", "Architect", "Administrator", "Coordinator", "Associate", "Assistant"
    ];

    private static readonly string[] _cities =
    [
        "Moscow", "St. Petersburg", "Novosibirsk", "Yekaterinburg", "Kazan",
        "Nizhny Novgorod", "Chelyabinsk", "Samara", "Rostov-on-Don", "Ufa"
    ];

    private static readonly string[] _streets =
    [
        "Lenina", "Nevsky", "Tverskaya", "Arbat", "Kirova", "Sovetskaya",
        "Gagarina", "Pushkina", "Lermontova", "Chekhova"
    ];

    private static readonly string[] _timeZones = TZConvert.KnownIanaTimeZoneNames
        .Where(tz => tz.StartsWith("Europe/") || tz.StartsWith("Asia/"))
        .Take(10)
        .ToArray();

    public static Location GenerateRandomLocation(HashSet<string> usedNames)
    {
        string name;
        int attempt = 0;
        const int maxAttempts = 100; // Защита от бесконечного цикла

        // Генерируем уникальное имя
        do
        {
            string city = _cities[Random.Shared.Next(_cities.Length)];
            string street = _streets[Random.Shared.Next(_streets.Length)];
            int number = Random.Shared.Next(1, 200);
            string suffix = attempt > 0 ? $" #{attempt + 1}" : string.Empty;
            name = $"{city} Office {street} {number}{suffix}";
            attempt++;
        }
        while (usedNames.Contains(name) && attempt < maxAttempts);

        // Если не удалось сгенерировать уникальное имя, добавляем GUID
        if (usedNames.Contains(name))
        {
            name = $"{name} {Guid.NewGuid().ToString("N")[..8]}";
        }

        usedNames.Add(name);

        // Остальная логика генерации локации
        string postalCode = Random.Shared.Next(100000, 1000000).ToString();
        string region = "Region " + Random.Shared.Next(1, 100);
        string cityName = _cities[Random.Shared.Next(_cities.Length)];
        string streetName = _streets[Random.Shared.Next(_streets.Length)];
        string house = Random.Shared.Next(1, 200).ToString();
        string? apartment = Random.Shared.Next(0, 10) > 7 ?
            Random.Shared.Next(1, 500).ToString() : null;
        var address = Address.Create(postalCode, region, cityName, streetName, house, apartment).Value;
        var timeZone = TimeZone.Create(_timeZones[Random.Shared.Next(_timeZones.Length)]).Value;

        return Location.Create(LocationName.Create(name).Value, address, timeZone, true,
            DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 365))).Value;
    }

    public static Department GenerateRandomDepartment(
        List<Location> locations,
        HashSet<string> usedIdentifiers,
        Department? parent = null)
    {
        string companyName = _companyNames[Random.Shared.Next(_companyNames.Length)];
        string deptType = _departmentTypes[Random.Shared.Next(_departmentTypes.Length)];
        var name = DepartmentName.Create($"{companyName} {deptType} Department").Value;

        string baseIdentifier = new string($"{companyName}{deptType}"
            .ToLower()
            .Where(c => c is >= 'a' and <= 'z')
            .ToArray());

        const string letters = "abcdefghijklmnopqrstuvwxyz";
        string identifier;
        do
        {
            string randomSuffix = new string(Enumerable.Range(0, Random.Shared.Next(1, 4))
                .Select(_ => letters[Random.Shared.Next(letters.Length)]).ToArray());
            identifier = $"{baseIdentifier}{randomSuffix}";
        }
        while (!usedIdentifiers.Add(identifier));

        var validIdentifier = Identifier.Create(identifier).Value;

        // Создаем один DepartmentId для использования везде
        var departmentId = new DepartmentId(Guid.NewGuid());
        int locationCount = Random.Shared.Next(1, Math.Min(SeedingConstants.MAX_LOCATIONS_PER_DEPARTMENT + 1, locations.Count + 1));
        var selectedLocations = locations.OrderBy(x => Random.Shared.Next()).Take(locationCount).ToList();
        var departmentLocations = selectedLocations.Select(l => new DepartmentLocation(departmentId, l.Id)).ToList();

        if (parent == null)
        {
            var dept = Department.CreateParent(name, validIdentifier, departmentLocations, departmentId).Value;
            return dept;
        }
        else
        {
            var dept = Department.CreateChild(name, validIdentifier, parent, departmentLocations, departmentId).Value;
            return dept;
        }
    }

    public static Position GenerateRandomPosition(List<Department> departments)
    {
        string title = _positionTitles[Random.Shared.Next(_positionTitles.Length)];
        string description = $"{title} position with various responsibilities";

        // Generate a unique position name with a counter
        string baseName = new string(title.ToLower()
                .Where(c => c is >= 'a' and <= 'z' or ' ')
                .ToArray())
            .Replace(" ", "_");

        // Add a unique number to the position name
        string positionName = $"{baseName}_{_positionCounter++}";

        var positionId = new PositionId(Guid.NewGuid());
        int departmentCount = Random.Shared.Next(1, Math.Min(SeedingConstants.MAX_DEPARTMENTS_PER_POSITION + 1, departments.Count + 1));
        var selectedDepartments = departments.OrderBy(x => Random.Shared.Next()).Take(departmentCount).ToList();
        var departmentPositions = selectedDepartments.Select(d => new DepartmentPosition(d.Id, positionId)).ToList();

        return Position.Create(
            PositionName.Create(positionName).Value,
            description,
            departmentPositions,
            positionId).Value;
    }
}