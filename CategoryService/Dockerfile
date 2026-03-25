FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CategoryService/CategoryService.csproj", "CategoryService/"]
COPY ["DTOs/DTOs.csproj", "DTOs/"]
RUN dotnet restore "CategoryService/CategoryService.csproj"
COPY . .
WORKDIR "/src/CategoryService"
RUN dotnet build "CategoryService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CategoryService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CategoryService.dll"]
