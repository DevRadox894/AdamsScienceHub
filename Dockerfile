# 1) Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first
COPY *.sln .
COPY AdamsScienceHub/AdamsScienceHub/*.csproj AdamsScienceHub/

# Restore dependencies
RUN dotnet restore

# Copy the entire project
COPY AdamsScienceHub/ AdamsScienceHub/
WORKDIR /src/AdamsScienceHub

# Build and publish
RUN dotnet publish -c Release -o /app

# 2) Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

# Run the app
ENTRYPOINT ["dotnet", "AdamsScienceHub.dll"]
