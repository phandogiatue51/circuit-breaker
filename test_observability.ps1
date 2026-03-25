$ErrorActionPreference = "Stop"
try {
    # Delete old logs if any
    if (Test-Path "gateway_obs_log.txt") { Remove-Item "gateway_obs_log.txt" -Force }

    Write-Host "--- START APIGateaway (Observability Test) ---"
    $proc = Start-Process dotnet "run --project APIGateaway\APIGateaway.csproj --launch-profile http" -PassThru -WindowStyle Hidden -RedirectStandardOutput "gateway_obs_log.txt" -RedirectStandardError "gateway_obs_err.txt"

    Write-Host "Waiting 12 seconds for APIGateaway to boot..."
    Start-Sleep -Seconds 12

    $metricsUrl = "http://localhost:5236/metrics"
    $healthUrl = "http://localhost:5236/api/commands/auth/health" # Just throwing a 404 to generate HTTP logs

    # Make standard request
    try {
        Invoke-RestMethod -Uri $healthUrl -Method Get
    } catch {
        # Catch 404
    }

    # Fetch Metrics
    Write-Host "`n--- FETCHING PROMETHEUS METRICS ---"
    try {
        $metrics = Invoke-RestMethod -Uri $metricsUrl -Method Get
        Write-Host ($metrics.Substring(0, [math]::Min($metrics.Length, 800)))
        Write-Host "`n... (Trimming output to 800 chars) ..."
    } catch {
        Write-Host "Failed to fetch metrics: " $_.Exception.Message
    }

    Write-Host "`n--- CAPTURED SERILOG (JSON) OUTPUT ---"
    $logs = Get-Content "gateway_obs_log.txt" -Raw
    Write-Host ($logs.Substring(0, [math]::Min($logs.Length, 1000)))
} finally {
    Write-Host "`n--- KILLING SERVICES ---"
    if ($proc) { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue }
    Stop-Process -Name dotnet -Force -ErrorAction SilentlyContinue
}
