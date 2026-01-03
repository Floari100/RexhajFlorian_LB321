FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# csproj zuerst (Cache)
COPY ArticleService/ArticleService.csproj ArticleService/
COPY SharedContracts/SharedContracts.csproj SharedContracts/

RUN dotnet restore ArticleService/ArticleService.csproj

# Rest kopieren
COPY . .

RUN dotnet publish ArticleService/ArticleService.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ArticleService.dll"]
