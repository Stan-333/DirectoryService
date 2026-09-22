FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# Контекст сборки — корень монорепо (docker-compose: context: ..),
# чтобы в образ попали общие props-файлы и библиотеки Shared.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["global.json", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["Shared/src/Shared.Kernel/Shared.Kernel.csproj", "Shared/src/Shared.Kernel/"]
COPY ["Shared/src/Shared.Core/Shared.Core.csproj", "Shared/src/Shared.Core/"]
COPY ["Shared/src/Shared.Framework/Shared.Framework.csproj", "Shared/src/Shared.Framework/"]
COPY ["DirectoryService/src/DirectoryService.Web/DirectoryService.Web.csproj", "DirectoryService/src/DirectoryService.Web/"]
COPY ["DirectoryService/src/DirectoryService.Application/DirectoryService.Application.csproj", "DirectoryService/src/DirectoryService.Application/"]
COPY ["DirectoryService/src/DirectoryService.Contracts/DirectoryService.Contracts.csproj", "DirectoryService/src/DirectoryService.Contracts/"]
COPY ["DirectoryService/src/DirectoryService.Domain/DirectoryService.Domain.csproj", "DirectoryService/src/DirectoryService.Domain/"]
COPY ["DirectoryService/src/DirectoryService.Infrastructure/DirectoryService.Infrastructure.csproj", "DirectoryService/src/DirectoryService.Infrastructure/"]
COPY ["DirectoryService/src/DirectoryService.Presenters/DirectoryService.Presenters.csproj", "DirectoryService/src/DirectoryService.Presenters/"]
RUN dotnet restore "DirectoryService/src/DirectoryService.Web/DirectoryService.Web.csproj"
COPY . .
WORKDIR "/src/DirectoryService/src/DirectoryService.Web"
RUN dotnet build "./DirectoryService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DirectoryService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DirectoryService.Web.dll"]