# 1) Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj(s) and restore first (speeds up rebuilds)
COPY *.sln .
COPY *.csproj ./
RUN dotnet restore

# copy everything and publish
COPY . .
WORKDIR /src/AdamsScienceHub
RUN dotnet publish -c Release -o /app

# 2) Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AdamsScienceHub.dll"]
