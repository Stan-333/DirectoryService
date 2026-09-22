FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# Контекст сборки — корень монорепо (docker-compose: context: ..),
# чтобы в образ попали общие props-файлы и библиотеки Shared.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
# Логин GitHub для источника "github" из nuget.config; токен передаётся отдельно, как build secret.
ARG NUGET_USERNAME=Stan-333
WORKDIR /src
COPY ["global.json", "./"]
COPY ["nuget.config", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["Shared/src/Shared.Core/Shared.Core.csproj", "Shared/src/Shared.Core/"]
COPY ["Shared/src/Shared.Framework/Shared.Framework.csproj", "Shared/src/Shared.Framework/"]
COPY ["DirectoryService/src/DirectoryService.Web/DirectoryService.Web.csproj", "DirectoryService/src/DirectoryService.Web/"]
COPY ["DirectoryService/src/DirectoryService.Application/DirectoryService.Application.csproj", "DirectoryService/src/DirectoryService.Application/"]
COPY ["DirectoryService/src/DirectoryService.Contracts/DirectoryService.Contracts.csproj", "DirectoryService/src/DirectoryService.Contracts/"]
COPY ["DirectoryService/src/DirectoryService.Domain/DirectoryService.Domain.csproj", "DirectoryService/src/DirectoryService.Domain/"]
COPY ["DirectoryService/src/DirectoryService.Infrastructure/DirectoryService.Infrastructure.csproj", "DirectoryService/src/DirectoryService.Infrastructure/"]
COPY ["DirectoryService/src/DirectoryService.Presenters/DirectoryService.Presenters.csproj", "DirectoryService/src/DirectoryService.Presenters/"]
# Stan333.SharedKernel лежит в GitHub Packages. PAT с read:packages приходит как build secret nuget_token
# (docker compose берёт его из переменной NUGET_AUTH_TOKEN) и виден только этой команде: в слои образа он не попадает.
RUN --mount=type=secret,id=nuget_token,env=NUGET_AUTH_TOKEN,required=true \
    if [ -z "$NUGET_AUTH_TOKEN" ]; then \
        echo "Не задан NUGET_AUTH_TOKEN (PAT с read:packages для GitHub Packages), см. README.md в корне репозитория" >&2; \
        exit 1; \
    fi && \
    NuGetPackageSourceCredentials_github="Username=${NUGET_USERNAME};Password=${NUGET_AUTH_TOKEN}" \
    dotnet restore "DirectoryService/src/DirectoryService.Web/DirectoryService.Web.csproj"
COPY . .
WORKDIR "/src/DirectoryService/src/DirectoryService.Web"
# Пакеты уже восстановлены выше; без --no-restore сборка снова обратилась бы к фиду без токена.
RUN dotnet build "./DirectoryService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DirectoryService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DirectoryService.Web.dll"]