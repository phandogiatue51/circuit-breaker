$projects = @("AuthService", "BrandService", "CategoryService", "ProductService", "APIGateaway")

foreach ($proj in $projects) {
    # 1. Create Dockerfile
    $dockerfile = @"
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["$proj/$proj.csproj", "$proj/"]
COPY ["DTOs/DTOs.csproj", "DTOs/"]
RUN dotnet restore "$proj/$proj.csproj"
COPY . .
WORKDIR "/src/$proj"
RUN dotnet build "$proj.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "$proj.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "$proj.dll"]
"@
    Set-Content -Path "c:\Micro\circuit-breaker\$proj\Dockerfile" -Value $dockerfile

    # 2. Inject Observability into Program.cs
    $path = "c:\Micro\circuit-breaker\$proj\Program.cs"
    $content = Get-Content -Path $path -Raw
    
    # 2a. Add Namespaces
    $usings = "using Serilog;`r`nusing OpenTelemetry.Resources;`r`nusing OpenTelemetry.Trace;`r`nusing OpenTelemetry.Metrics;`r`nusing Prometheus;`r`n"
    if ($content -notmatch "using Serilog;") {
        $content = $usings + $content
    }

    # 2b. Add Serilog
    if ($content -notmatch "Log.Logger = new") {
        $serilog = "Log.Logger = new LoggerConfiguration().WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter()).Enrich.FromLogContext().CreateBootstrapLogger();`r`nvar builder = WebApplication.CreateBuilder(args);`r`nbuilder.Host.UseSerilog((context, services, configuration) => configuration.WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter()).Enrich.FromLogContext());"
        $content = $content -replace "var builder \= WebApplication\.CreateBuilder\(args\);", $serilog
    }

    # 2c. Add OpenTelemetry
    if ($content -notmatch "builder.Services.AddOpenTelemetry\(\)") {
        $otel = "builder.Services.AddOpenTelemetry().ConfigureResource(res => res.AddService(`"$proj`")).WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddOtlpExporter()).WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddPrometheusExporter());"
        $content = $content -replace "var app \= builder\.Build\(\);", "$otel`r`nvar app = builder.Build();"
    }

    # 2d. Add Prometheus Pipeline
    if ($content -notmatch "app.UseHttpMetrics\(\)") {
        $content = $content -replace "app\.UseHttpsRedirection\(\);", "app.UseHttpsRedirection();`r`napp.UseHttpMetrics();"
    }
    
    # 2e. Map Prometheus Metrics Endpoint
    if ($content -notmatch "app.MapMetrics\(\)") {
        if ($proj -eq "APIGateaway") {
            $content = $content -replace "app\.UseOcelot", "app.MapMetrics();`r`napp.UseOcelot"
        } else {
            $content = $content -replace "app\.MapControllers\(\);", "app.MapControllers();`r`napp.MapMetrics();"
        }
    }
    
    Set-Content -Path $path -Value $content
}

Write-Host "Observability Setup complete!"
