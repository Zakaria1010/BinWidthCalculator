# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy project files
COPY ["BinWidthCalculator.API/BinWidthCalculator.API.csproj", "BinWidthCalculator.API/"]
COPY ["BinWidthCalculator.Tests/BinWidthCalculator.Tests.csproj", "BinWidthCalculator.Tests/"]

# Restore dependencies
RUN dotnet restore "BinWidthCalculator.API/BinWidthCalculator.API.csproj"

# Copy everything else
COPY . .

# Build and publish
WORKDIR "/src/BinWidthCalculator.API"
RUN dotnet publish "BinWidthCalculator.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install SQLite for production
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Create data directory for SQLite database
RUN mkdir -p /data

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BinWidthCalculator.API.dll"]
