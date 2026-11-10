# syntax=docker/dockerfile:1

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RaceDay/RaceDay.csproj", "RaceDay/"]
RUN dotnet restore "RaceDay/RaceDay.csproj"

COPY ["RaceDay/", "RaceDay/"]
WORKDIR "/src/RaceDay"
RUN dotnet publish "RaceDay.csproj" -c Release -o /app/publish --no-restore

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RaceDay.dll"]
