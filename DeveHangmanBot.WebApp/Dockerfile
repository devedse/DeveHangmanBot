#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["DeveHangmanBot.WebApp/DeveHangmanBot.WebApp.csproj", "DeveHangmanBot.WebApp/"]
COPY ["DeveHangmanBot.TelegramBot/DeveHangmanBot.TelegramBot.csproj", "DeveHangmanBot.TelegramBot/"]
COPY ["DeveHangmanBot/DeveHangmanBot.csproj", "DeveHangmanBot/"]
RUN dotnet restore "DeveHangmanBot.WebApp/DeveHangmanBot.WebApp.csproj"
COPY . .
WORKDIR "/src/DeveHangmanBot.WebApp"
RUN dotnet build "DeveHangmanBot.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DeveHangmanBot.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DeveHangmanBot.WebApp.dll"]