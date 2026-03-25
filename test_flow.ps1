# test_flow.ps1
$ErrorActionPreference = "Stop"
Write-Host "--- 1. UPDATE TO SQL SERVER ---"
$services = @("AuthService", "BrandService", "CategoryService", "ProductService")

foreach ($svc in $services) {
    # 1. Update Program.cs
    $progPath = "c:\Micro\circuit-breaker\$svc\Program.cs"
    $content = Get-Content $progPath -Raw
    $content = $content -replace 'UseNpgsql\(', 'UseSqlServer('
    
    # 2. Add Graceful Shutdown
    if ($content -notmatch 'ShutdownTimeout') {
        $graceful = "builder.Services.Configure<Microsoft.Extensions.Hosting.HostOptions>(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(15); });`r`nvar app = builder.Build();"
        $content = $content -replace 'var app = builder.Build\(\);', $graceful
    }
    Set-Content $progPath $content

    # 3. Update appsettings.json
    $appPath = "c:\Micro\circuit-breaker\$svc\appsettings.json"
    $appContent = Get-Content $appPath -Raw
    $dbName = if ($svc -eq "AuthService") { "AccountDb" } elseif ($svc -eq "ProductService") { "ProductDb" } else { $svc }
    $sqlConn = "Server=localhost;Database=$dbName;Trusted_Connection=True;TrustServerCertificate=True"
    $appContent = $appContent -replace '"DefaultConnection"\s*:\s*".*?"', "`"DefaultConnection`": `"$sqlConn`""
    Set-Content $appPath $appContent

    # 4. Remove old migrations
    $migPath = "c:\Micro\circuit-breaker\$svc\Migrations"
    if (Test-Path $migPath) {
        Remove-Item -Recurse -Force $migPath
    }
}

Write-Host "--- 2. RUN EF MIGRATIONS ---"
Set-Location "c:\Micro\circuit-breaker"
foreach ($svc in $services) {
    Set-Location "c:\Micro\circuit-breaker\$svc"
    Write-Host "Migrating $svc..."
    dotnet ef migrations add InitialCreate --context $(if ($svc -eq "AuthService") {"AccountDbContext"} elseif ($svc -eq "ProductService") {"ProductDbContext"} elseif ($svc -eq "BrandService") {"BrandDbContext"} else {"CategoryDbContext"})
    dotnet ef database update --context $(if ($svc -eq "AuthService") {"AccountDbContext"} elseif ($svc -eq "ProductService") {"ProductDbContext"} elseif ($svc -eq "BrandService") {"BrandDbContext"} else {"CategoryDbContext"})
}

Write-Host "--- 3. START SERVICES ---"
Set-Location "c:\Micro\circuit-breaker"
$procs = @()
$procs += Start-Process dotnet "run --project AuthService\AuthService.csproj" -PassThru -WindowStyle Hidden
$procs += Start-Process dotnet "run --project BrandService\BrandService.csproj" -PassThru -WindowStyle Hidden
$procs += Start-Process dotnet "run --project CategoryService\CategoryService.csproj" -PassThru -WindowStyle Hidden
$procs += Start-Process dotnet "run --project ProductService\ProductService.csproj" -PassThru -WindowStyle Hidden
$procs += Start-Process dotnet "run --project APIGateaway\APIGateaway.csproj" -PassThru -WindowStyle Hidden

Write-Host "Waiting 15 seconds for services to start..."
Start-Sleep -Seconds 15

Write-Host "--- 4. TEST FLOW ---"
# Trust self-signed certs
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

$baseUrl = "https://localhost:7246"

# A. Register
Write-Host "Registering..."
$regBody = @{
    Username = "admin"
    Email = "admin@example.com"
    Password = "password123"
    Role = 1
} | ConvertTo-Json
$regRes = Invoke-RestMethod -Uri "$baseUrl/api/commands/auth/register" -Method Post -Body $regBody -ContentType "application/json"
Write-Host "Register Response: " ($regRes | ConvertTo-Json -Depth 5)

# B. Login
Write-Host "Logging in..."
$logBody = @{
    Email = "admin@example.com"
    Password = "password123"
} | ConvertTo-Json
$logRes = Invoke-RestMethod -Uri "$baseUrl/api/commands/auth/login" -Method Post -Body $logBody -ContentType "application/json"
$token = $logRes.data.token
$headers = @{
    "Authorization" = "Bearer $token"
}

# Create Brand
Write-Host "Creating Brand..."
$brandBody = @{
    Name = "Nike"
    Description = "Just Do It"
} | ConvertTo-Json
$brandRes = Invoke-RestMethod -Uri "$baseUrl/api/commands/brands" -Method Post -Headers $headers -Body $brandBody -ContentType "application/json"

# C. Create Product
Write-Host "Creating Product..."
$prodBody = @{
    Name = "New Product"
    Description = "Desc"
    Price = 100
    Origin = "VN"
    Material = "Wood"
    BrandId = 1
    CategoryIds = @(1, 2)
} | ConvertTo-Json

try {
    $createRes = Invoke-RestMethod -Uri "$baseUrl/api/commands/products" -Method Post -Headers $headers -Body $prodBody -ContentType "application/json"
    Write-Host "Create Product Response: " ($createRes | ConvertTo-Json -Depth 5)
} catch {
    Write-Host "Create Product Failed with exception: " $_.Exception.Message
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        Write-Host "Error Body: " $reader.ReadToEnd()
    }
}

Write-Host "--- 5. KILL SERVICES ---"
foreach ($p in $procs) {
    Stop-Process -Id $p.Id -Force
}
