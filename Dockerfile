# 1) Build stage - USING .NET 10.0
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY *.csproj ./

# Restore NuGet packages
RUN dotnet restore

# Copy everything else and publish
COPY . ./
RUN dotnet publish -c Release -o /app

# 2) Runtime stage - USING .NET 10.0 ASP.NET Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app ./

# Expose the port Render expects
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the app
ENTRYPOINT ["dotnet", "AdamsScienceHub.dll"]