# =========================
# 🏗️ Build Stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy only the project files first (for caching)
COPY BinWidthCalculator.API/BinWidthCalculator.API.csproj BinWidthCalculator.API/
COPY BinWidthCalculator.Tests/BinWidthCalculator.Tests.csproj BinWidthCalculator.Tests/

# Restore dependencies for the API project
RUN dotnet restore BinWidthCalculator.API/BinWidthCalculator.API.csproj

# Copy the rest of the source code
COPY . .

# Publish the API (Release mode, no apphost to save space)
WORKDIR /src/BinWidthCalculator.API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# =========================
# 🚀 Runtime Stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS final
WORKDIR /app

# Install SQLite (small and fast on Alpine)
RUN apk add --no-cache sqlite

# Create and set permissions for data directory
RUN mkdir -p /data && chmod 777 /data

# Copy published output from the build stage
COPY --from=build /app/publish .

# Expose ports
EXPOSE 80
EXPOSE 443

# Environment variable for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:80

# Run the app
ENTRYPOINT ["dotnet", "BinWidthCalculator.API.dll"]
