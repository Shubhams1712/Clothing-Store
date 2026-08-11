# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and all project files for restore caching
COPY Backend.sln .
COPY API/API.csproj             API/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj       Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/

RUN dotnet restore

# Copy everything else and publish
COPY . .
RUN dotnet publish API/API.csproj -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Render injects PORT; default to 8080
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

COPY --from=build /app/publish .

# Create uploads dir and give the non-root user write access
RUN mkdir -p /app/wwwroot/uploads && \
    chown -R app:app /app/wwwroot/uploads

EXPOSE 8080

USER app

ENTRYPOINT ["dotnet", "API.dll"]
