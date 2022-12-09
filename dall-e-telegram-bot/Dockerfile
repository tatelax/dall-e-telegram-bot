FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["dall-e-telegram-bot/dall-e-telegram-bot.csproj", "dall-e-telegram-bot/"]
RUN dotnet restore "dall-e-telegram-bot/dall-e-telegram-bot.csproj"
COPY . .
WORKDIR "/src/dall-e-telegram-bot"
RUN dotnet build "dall-e-telegram-bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dall-e-telegram-bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "dall-e-telegram-bot.dll"]
