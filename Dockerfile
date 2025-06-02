# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["DotnetLearningShowcase.csproj", "./"]
RUN dotnet restore "DotnetLearningShowcase.csproj"

# Copy the rest of the source code and publish
COPY . .
RUN dotnet publish "DotnetLearningShowcase.csproj" -c Release -o /app/publish

# Use the official ASP.NET runtime for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Bind to the PORT environment variable (Fly.io default is 8080)
ENV ASPNETCORE_URLS=http://*:${PORT:-8080}

ENTRYPOINT ["dotnet", "DotnetLearningShowcase.dll"] 