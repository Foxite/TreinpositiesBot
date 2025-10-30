FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TreinpositiesBot.csproj", "TreinpositiesBot.csproj"]
RUN dotnet restore "TreinpositiesBot.csproj"
COPY . .
RUN dotnet build "TreinpositiesBot.csproj" -c Release -o /app/build
RUN dotnet publish "TreinpositiesBot.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
CMD ["./TreinpositiesBot"]
