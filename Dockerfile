FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Level1/BasicWebApi/BasicWebApi.csproj", "Level1/BasicWebApi/"]
RUN dotnet restore "Level1/BasicWebApi/BasicWebApi.csproj"
COPY . .
WORKDIR "/src/Level1/BasicWebApi"
RUN dotnet build "BasicWebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BasicWebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BasicWebApi.dll"] 