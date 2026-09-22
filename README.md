# DirectoryService

Монорепозиторий:

- `DirectoryService/` — сервис справочника подразделений, локаций и должностей (решение `DirectoryService.sln`);
- `Shared/` — общие библиотеки сервисов (решение `Shared.sln`):
  - `Shared.Kernel` — базовые типы, публикуется как NuGet-пакет `Stan333.SharedKernel` ([README](Shared/src/Shared.Kernel/README.md));
  - `Shared.Core` — абстракции CQRS, валидация, `BaseHttpClient`;
  - `Shared.Framework` — ASP.NET Core: `EndpointResult`, `ExceptionMiddleware`, OpenAPI, логирование.

Core и Framework подключаются через `ProjectReference`, Kernel — из GitHub Packages (`PackageReference`).

## Доступ к GitHub Packages

Источники пакетов заданы в `nuget.config`. Для `Stan333.*` нужен PAT (classic) с правом `read:packages`.
Токен в репозиторий не кладётся.

- **Локальная сборка** — один раз добавить учётные данные в пользовательский `NuGet.Config`:

  ```bash
  dotnet nuget add source https://nuget.pkg.github.com/Stan-333/index.json --name github --username <github-login> --password <PAT>
  ```

- **Docker** — `docker compose` передаёт токен в сборку образа как build secret из переменной
  `NUGET_AUTH_TOKEN`. Её можно задать в окружении или в `DirectoryService/.env` (файл в `.gitignore`):

  ```
  NUGET_AUTH_TOKEN=<PAT>
  ```

- **GitHub Actions** — `GITHUB_TOKEN` с `permissions: packages: read`, PAT не нужен.