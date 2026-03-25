$ErrorActionPreference = "Stop"
$procs = @()
try {
    # Trust self-signed certs
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

    Write-Host "--- START SERVICES ---"
    $procs += Start-Process dotnet "run --project AuthService\AuthService.csproj --launch-profile https" -PassThru -WindowStyle Hidden
    $procs += Start-Process dotnet "run --project BrandService\BrandService.csproj --launch-profile https" -PassThru -WindowStyle Hidden
    $procs += Start-Process dotnet "run --project CategoryService\CategoryService.csproj --launch-profile https" -PassThru -WindowStyle Hidden
    $procs += Start-Process dotnet "run --project ProductService\ProductService.csproj --launch-profile https" -PassThru -WindowStyle Hidden
    $procs += Start-Process dotnet "run --project APIGateaway\APIGateaway.csproj --launch-profile https" -PassThru -WindowStyle Hidden

    Write-Host "Waiting 25 seconds for all services to boot..."
    Start-Sleep -Seconds 25

    $baseUrl = "https://localhost:7246"

    # A. Register
    Write-Host "`n[1] Registering Admin..."
    $regBody = @{ Username = "admin3"; Email = "admin3@example.com"; Password = "password123"; Role = 1 } | ConvertTo-Json
    $regRes = Invoke-RestMethod -Uri "$baseUrl/api/commands/auth/register" -Method Post -Body $regBody -ContentType "application/json"
    Write-Host "Register Response: " ($regRes | ConvertTo-Json -Depth 5)

    # B. Login
    Write-Host "`n[2] Logging in..."
    $logBody = @{ Email = "admin3@example.com"; Password = "password123" } | ConvertTo-Json
    $logRes = Invoke-RestMethod -Uri "$baseUrl/api/commands/auth/login" -Method Post -Body $logBody -ContentType "application/json"
    Write-Host "Login Response: " ($logRes | ConvertTo-Json -Depth 5)

    $token = $logRes.data.token
    $headers = @{ "Authorization" = "Bearer $token" }

    # C. Create Brand
    Write-Host "`n[3] Creating Brand..."
    $brandBody = @{ Name = "Nike"; Description = "Just Do It" } | ConvertTo-Json
    $brandRes = Invoke-RestMethod -Uri "$baseUrl/api/commands/brands" -Method Post -Headers $headers -Body $brandBody -ContentType "application/json"
    Write-Host "Create Brand Response: " ($brandRes | ConvertTo-Json -Depth 5)

    # D. Create Product (with Category GetByIds returning null intentionally)
    Write-Host "`n[4] Creating Product (Should Fail with 400 Bad Request!)..."
    $prodBody = @{ Name = "New Product"; Description = "Desc"; Price = 100; Origin = "VN"; Material = "Wood"; BrandId = $brandRes.data.id; CategoryIds = @(1, 2) } | ConvertTo-Json
    
    try {
        $createRes = Invoke-RestMethod -Uri "$baseUrl/api/commands/products" -Method Post -Headers $headers -Body $prodBody -ContentType "application/json"
        Write-Host "Create Product Response: " ($createRes | ConvertTo-Json -Depth 5)
    } catch {
        Write-Host "🔥 Create Product Failed with exception: " $_.Exception.Message
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $reader.BaseStream.Position = 0
            Write-Host "🔥 Error Body: " $reader.ReadToEnd()
        }
    }
} finally {
    Write-Host "`n--- KILLING SERVICES ---"
    foreach ($p in $procs) {
        Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
    }
    Stop-Process -Name dotnet -Force -ErrorAction SilentlyContinue
}
