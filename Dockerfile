# 1. Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ProgressHub.Web/ProgressHub.Web.csproj", "ProgressHub.Web/"]
COPY ["ProgressHub.Core/ProgressHub.Core.csproj", "ProgressHub.Core/"]
COPY ["ProgressHub.Data/ProgressHub.Data.csproj", "ProgressHub.Data/"]

RUN dotnet restore "ProgressHub.Web/ProgressHub.Web.csproj"

COPY . .
WORKDIR "/src/ProgressHub.Web"
RUN dotnet publish "ProgressHub.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN mkdir -p /app/data
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProgressHub.Web.dll"]
