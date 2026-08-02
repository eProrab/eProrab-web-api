FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY eProrab.sln .
COPY src/eProrab.Domain/eProrab.Domain.csproj src/eProrab.Domain/
COPY src/eProrab.Application/eProrab.Application.csproj src/eProrab.Application/
COPY src/eProrab.Infrastructure/eProrab.Infrastructure.csproj src/eProrab.Infrastructure/
COPY src/eProrab.API/eProrab.API.csproj src/eProrab.API/
RUN dotnet restore eProrab.sln

COPY src/ src/
RUN dotnet publish src/eProrab.API/eProrab.API.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app .

ENTRYPOINT ["dotnet", "eProrab.API.dll"]
