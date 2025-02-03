#grab the sdk as the project will need built in the publish step
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

EXPOSE 5000

COPY --from=build /out ./
ENTRYPOINT ["dotnet", "HealthChecksTesting.WorkerService.dll"]