FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./Common/Csharp/CommonDis/CommonDis.csproj", "/Common/Csharp/CommonDis/"]
COPY ["./SkyPanel/SkyPanel.sln", "./SkyPanel/"]
COPY ["./SkyPanel/SkyPanel.csproj", "./SkyPanel/"]
RUN dotnet restore "./SkyPanel/SkyPanel.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "./SkyPanel/SkyPanel.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SkyPanel/SkyPanel.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SkyPanel.dll"]
