FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Server/IF-Tools Briefings.Server.csproj", "Server/"]
RUN dotnet restore "Server/IF-Tools Briefings.Server.csproj"
COPY . .
WORKDIR "/src/Server"
RUN dotnet build "IF-Tools Briefings.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IF-Tools Briefings.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IF-ToolsBriefings.Server.dll"]
