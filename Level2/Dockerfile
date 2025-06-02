# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ./Level2/Level2.WeatherCrud/Level2.WeatherCrud.csproj ./Level2.WeatherCrud/
WORKDIR /src/Level2.WeatherCrud
RUN dotnet restore

# Copy the rest of the source code and publish
COPY ./Level2/Level2.WeatherCrud/. ./
RUN dotnet publish -c Release -o /app/publish

# Use the official ASP.NET runtime for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Bind to the PORT environment variable (Fly.io default is 8080)
ENV ASPNETCORE_URLS=http://*:${PORT:-8080}

ENTRYPOINT ["dotnet", "Level2.WeatherCrud.dll"]
