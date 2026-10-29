FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/Guardian.Api/Guardian.Api.csproj src/Guardian.Api/
RUN dotnet restore src/Guardian.Api/Guardian.Api.csproj
COPY src/Guardian.Api/ src/Guardian.Api/
WORKDIR /src/src/Guardian.Api
RUN dotnet publish Guardian.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Guardian.Api.dll"]
