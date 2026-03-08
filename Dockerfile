FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5053

ENV ASPNETCORE_URLS=http://+:5053

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["EpicGamesBot.csproj", "./"]
RUN dotnet restore "EpicGamesBot.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "EpicGamesBot.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "EpicGamesBot.csproj" -c $configuration -o /app/publish /p:UseAppHost=true -r linux-x64 --self-contained true 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EpicGamesBot.dll"]
