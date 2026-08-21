# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["global.json", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/Order.Domain/Order.Domain.csproj", "src/Order.Domain/"]
COPY ["src/Order.Application/Order.Application.csproj", "src/Order.Application/"]
COPY ["src/Order.Infrastructure/Order.Infrastructure.csproj", "src/Order.Infrastructure/"]
COPY ["src/Order.ServiceDefaults/Order.ServiceDefaults.csproj", "src/Order.ServiceDefaults/"]
COPY ["src/Order.WebApi/Order.WebApi.csproj", "src/Order.WebApi/"]
RUN dotnet restore "src/Order.WebApi/Order.WebApi.csproj"

COPY . .

# Generate Wolverine handlers before publishing so production starts in static mode.
RUN dotnet run --project "src/Order.WebApi/Order.WebApi.csproj" --configuration Release --no-restore -- codegen write
RUN dotnet publish "src/Order.WebApi/Order.WebApi.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "Order.WebApi.dll"]
