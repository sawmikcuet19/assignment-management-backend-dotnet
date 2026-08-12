# Starts the Assignment Management API.
#
# Any stale process still holding the dev ports (5178 / 7154) is stopped first,
# so you never hit "address already in use" again. Run this instead of
# `dotnet run --project src/AssignmentManagement.Api`:
#
#     .\run.ps1        (PowerShell)
#     run.cmd          (Command Prompt)

$ports = 5178, 7154

foreach ($port in $ports) {
    $stale = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue
    foreach ($conn in $stale) {
        $processToKill = $conn.OwningProcess
        $name = (Get-Process -Id $processToKill -ErrorAction SilentlyContinue).ProcessName
        Write-Host "Stopping stale process PID $processToKill ($name) holding port $port..." -ForegroundColor Yellow
        Stop-Process -Id $processToKill -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "Starting the API (first build takes ~10-20s)... press Ctrl+C to stop." -ForegroundColor Green

# -NoNewWindow shares this console directly (no pipe buffering), so the app's
# "Now listening on:" and Serilog logs appear live instead of freezing at "Building...".
$proc = Start-Process dotnet -ArgumentList 'run', '--project', 'src/AssignmentManagement.Api' -NoNewWindow -PassThru

$timeout = 90
for ($i = 0; $i -lt $timeout; $i++) {
    if (Get-NetTCPConnection -State Listen -LocalPort 5178 -ErrorAction SilentlyContinue) {
        Write-Host ""
        Write-Host "API is UP:  http://localhost:5178/swagger" -ForegroundColor Green
        break
    }
    if ($proc.HasExited) {
        Write-Host "The API process exited before it could start - see messages above." -ForegroundColor Red
        break
    }
    Start-Sleep -Seconds 1
}

Wait-Process -Id $proc.Id
