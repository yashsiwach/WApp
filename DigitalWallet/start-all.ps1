# Start all DigitalWallet microservices in separate windows
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

$services = @(
    @{ Name = "Gateway      (5000)"; Path = "$root\src\Gateway" },
    @{ Name = "AuthService  (5001)"; Path = "$root\src\Services\AuthService" },
    @{ Name = "WalletService(5002)"; Path = "$root\src\Services\WalletService" },
    @{ Name = "RewardsService(5003)"; Path = "$root\src\Services\RewardsService" },
    @{ Name = "NotifService (5004)"; Path = "$root\src\Services\NotificationService" },
    @{ Name = "AdminService (5005)"; Path = "$root\src\Services\AdminService" },
    @{ Name = "SupportSvc   (5006)"; Path = "$root\src\Services\SupportTicketService" }
)

foreach ($svc in $services) {
    Write-Host "Starting $($svc.Name)..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$($svc.Path)'; dotnet run --launch-profile http" -WindowStyle Normal
    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "All services started. Gateway is at http://localhost:5000" -ForegroundColor Green
Write-Host "Start the frontend with: cd frontend && ng serve" -ForegroundColor Yellow
