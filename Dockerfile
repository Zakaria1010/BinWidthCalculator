# Runtime-only image — build happens in CI
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

EXPOSE 80
EXPOSE 443

# Install SQLite for production
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Create data directory for SQLite database
RUN mkdir -p /data

# Copy prebuilt app from GitHub Actions
ARG PUBLISH_DIR=./publish
COPY ${PUBLISH_DIR} .

ENTRYPOINT ["dotnet", "BinWidthCalculator.API.dll"]
