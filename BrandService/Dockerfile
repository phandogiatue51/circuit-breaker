FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BrandService/BrandService.csproj", "BrandService/"]
COPY ["DTOs/DTOs.csproj", "DTOs/"]
RUN dotnet restore "BrandService/BrandService.csproj"
COPY . .
WORKDIR "/src/BrandService"
RUN dotnet build "BrandService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BrandService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BrandService.dll"]
