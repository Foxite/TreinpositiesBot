FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TreinpositiesBot/TreinpositiesBot.csproj", "TreinpositiesBot/TreinpositiesBot.csproj"]
RUN dotnet restore "TreinpositiesBot/TreinpositiesBot.csproj"
COPY . .
RUN dotnet build "TreinpositiesBot/TreinpositiesBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TreinpositiesBot/TreinpositiesBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./TreinpositiesBot"]
