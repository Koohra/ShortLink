FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ShortLink.WebAPI/ShortLink.WebAPI.csproj", "ShortLink.WebAPI/"]
RUN dotnet restore "ShortLink.WebAPI/ShortLink.WebAPI.csproj"
COPY . .
WORKDIR "/src/ShortLink.WebAPI"
RUN dotnet build "ShortLink.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ShortLink.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShortLink.WebAPI.dll"]
