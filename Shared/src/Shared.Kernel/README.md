# Stan333.SharedKernel

Базовые типы для сервисов на .NET: ошибки, результат операций и страница списка.
Пакет не зависит от ASP.NET Core и EF Core, его можно подключать в Domain и Contracts.
Единственная зависимость — [CSharpFunctionalExtensions](https://www.nuget.org/packages/CSharpFunctionalExtensions).

Исходный код: [Shared/src/Shared.Kernel](https://github.com/Stan-333/DirectoryService/tree/main/Shared/src/Shared.Kernel) в монорепозитории DirectoryService.

## Что внутри

| Тип | Назначение |
|---|---|
| `Error` | Ожидаемая ошибка: `Code`, `Message`, `Type`, `InvalidField`. Создаётся фабриками `NotFound`, `Validation`, `Conflict`, `Failure`, `Authentication`, `Authorization`. |
| `Errors` | Список ошибок только для чтения. В JSON — массив `Error`. |
| `ErrorType` | Тип ошибки. В JSON — строка (`"NotFound"`), по нему выбирается HTTP-статус. |
| `GeneralErrors` | Типовые ошибки с готовыми кодами: `value.is.invalid`, `record.not.found` и др. |
| `AppException` | Исключение с `Errors` для мест, где ошибку неудобно вернуть через `Result`. |
| `PagedResult<T>` | Страница списка: `Items`, `TotalCount`, `Page`, `PageSize`, `TotalPages`, `HasPreviousPage`, `HasNextPage`. |

## Подключение

Пакет лежит в приватном фиде GitHub Packages. В `nuget.config` решения укажите источник и сопоставление
пакетов `Stan333.*` с ним. Токен в репозиторий не кладите:

```xml
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/Stan-333/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
    <packageSource key="github"><package pattern="Stan333.*" /></packageSource>
  </packageSourceMapping>
</configuration>
```

Учётные данные для `github` добавляются один раз в пользовательский `NuGet.Config` на машине разработчика.
Нужен PAT (classic) с правом `read:packages`: GitHub Packages требует авторизацию даже для чтения.

```bash
dotnet nuget add source https://nuget.pkg.github.com/Stan-333/index.json --name github --username <github-login> --password <PAT>
```

На Windows пароль сохраняется в зашифрованном виде. На Linux и macOS шифрование недоступно, там нужен
флаг `--store-password-in-clear-text`. В CI вместо PAT используется `GITHUB_TOKEN` с `permissions: packages: read`.

```xml
<PackageReference Include="Stan333.SharedKernel" Version="0.1.0" />
```

## Пример

```csharp
public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken ct)
{
    if (await _repository.ExistsByNameAsync(command.Name, ct))
        return GeneralErrors.AlreadyExist("Локация").ToErrors();

    Guid id = await _repository.AddAsync(command.Name, ct);
    return id;
}

public async Task<PagedResult<LocationDto>> Handle(GetLocationsQuery query, CancellationToken ct)
{
    (List<LocationDto> items, long totalCount) = await _readRepository.GetPageAsync(query.Page, query.PageSize, ct);
    return new PagedResult<LocationDto>(items, totalCount, query.Page, query.PageSize);
}
```

## Версии (SemVer)

Версия имеет вид `MAJOR.MINOR.PATCH` ([semver.org](https://semver.org/lang/ru/)):

- **MAJOR** — ломающее изменение публичного API или JSON-контракта: удалён или переименован тип или член,
  изменена сигнатура, изменился код типовой ошибки или имя поля в JSON.
- **MINOR** — совместимое расширение: новый тип, новая фабрика ошибок, новое необязательное свойство.
- **PATCH** — исправление без изменения API: ошибка в логике, текст сообщения, документация.

Пока версия `0.y.z`, API считается нестабильным: ломающие изменения поднимают MINOR (`0.1.0` → `0.2.0`),
совместимые — PATCH. Начиная с `1.0.0` действуют правила выше без исключений.

Опубликованная версия не меняется: чтобы исправить выпущенный пакет, выпускается следующая версия.

## Выпуск релиза

1. В `Shared.Kernel.csproj` поднять `<Version>` по правилам SemVer и дописать раздел в «Историю версий» ниже.
2. Влить изменения в `main` через PR.
3. Поставить аннотированный тег `vX.Y.Z` на коммит релиза в `main` и запушить его:

   ```bash
   git tag -a v0.1.0 -m "Stan333.SharedKernel 0.1.0" <коммит>
   git push origin v0.1.0
   ```

   Теги `v*` в этом репозитории зарезервированы за релизами Stan333.SharedKernel.
4. Workflow [`shared-kernel.yml`](https://github.com/Stan-333/DirectoryService/blob/main/.github/workflows/shared-kernel.yml)
   собирает решение Shared, запускает тесты, проверяет, что тег совпадает с `<Version>`, упаковывает Kernel
   и публикует пакет в GitHub Packages.
5. Создать GitHub Release из тега: в описании перечислить изменения и дать ссылку на коммит релиза.

В пакете есть `Shared.Kernel.pdb` и XML-документация. В `.nuspec` записаны адрес репозитория
и коммит, из которого собран пакет, поэтому по версии всегда можно найти исходный код.

## История версий

### 0.1.0

Первый выпуск: `Error`, `Errors`, `ErrorType`, `GeneralErrors`, `AppException`, `PagedResult<T>`.
