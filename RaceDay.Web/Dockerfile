# syntax=docker/dockerfile:1

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy just the project file first so Docker can cache the NuGet restore layer
# and skip it on rebuilds where only .cshtml/.cs files changed.
COPY ["RaceDay.Web/RaceDay.Web.csproj", "RaceDay.Web/"]
RUN dotnet restore "RaceDay.Web/RaceDay.Web.csproj"

# Now copy the rest of the source and publish.
COPY ["RaceDay.Web/", "RaceDay.Web/"]
WORKDIR "/src/RaceDay.Web"
RUN dotnet publish "RaceDay.Web.csproj" -c Release -o /app/publish --no-restore

# ---------- Runtime stage ----------
# The much smaller ASP.NET runtime image (no SDK/compiler) is what actually ships.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Run as a non-root user -- standard container hardening practice.
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]
