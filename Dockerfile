FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Spellcard/Spellcard.csproj", "Spellcard/"]
RUN dotnet restore "Spellcard/Spellcard.csproj"
COPY . .
WORKDIR "/src/Spellcard"
RUN dotnet build "Spellcard.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Spellcard.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Spellcard.dll"]
