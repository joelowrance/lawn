FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LawnCare.CoreApi/LawnCare.CoreApi.csproj", "LawnCare.CoreApi/"]
RUN dotnet restore "LawnCare.CoreApi/LawnCare.CoreApi.csproj"
COPY . .
WORKDIR "/src/LawnCare.CoreApi"
RUN dotnet build "./LawnCare.CoreApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LawnCare.CoreApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LawnCare.CoreApi.dll"]
