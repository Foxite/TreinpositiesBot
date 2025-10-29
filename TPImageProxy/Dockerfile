FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["TPImageProxy/TPImageProxy.csproj", "TPImageProxy/"]
RUN dotnet restore "TPImageProxy/TPImageProxy.csproj"
COPY . .

WORKDIR "/src/TPImageProxy"
RUN dotnet build "TPImageProxy.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "TPImageProxy.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
CMD ["./TPImageProxy"]
