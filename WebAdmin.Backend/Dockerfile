FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["WebAdmin.Backend/WebAdmin.Backend.csproj", "WebAdmin.Backend/"]
RUN dotnet restore "WebAdmin.Backend/WebAdmin.Backend.csproj"
COPY . .
WORKDIR "/src/WebAdmin.Backend"
RUN dotnet build "WebAdmin.Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebAdmin.Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAdmin.Backend.dll"]
