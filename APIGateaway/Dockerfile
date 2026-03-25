FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["APIGateaway/APIGateaway.csproj", "APIGateaway/"]
COPY ["DTOs/DTOs.csproj", "DTOs/"]
RUN dotnet restore "APIGateaway/APIGateaway.csproj"
COPY . .
WORKDIR "/src/APIGateaway"
RUN dotnet build "APIGateaway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "APIGateaway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "APIGateaway.dll"]
