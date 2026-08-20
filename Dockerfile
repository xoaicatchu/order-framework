# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["WolverineApp/WolverineApp.csproj", "WolverineApp/"]
RUN dotnet restore "WolverineApp/WolverineApp.csproj"

COPY . .
WORKDIR /src/WolverineApp

# Generate Wolverine handlers before publishing so production starts in static mode.
RUN dotnet run --configuration Release --no-restore -- codegen write
RUN dotnet publish "WolverineApp.csproj" \
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
ENTRYPOINT ["dotnet", "WolverineApp.dll"]
