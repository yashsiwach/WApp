# Start all DigitalWallet microservices in separate windows
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$envFile = Join-Path $root ".env"

function Read-EnvFile {
    param([string]$Path)

    $values = @{}

    if (-not (Test-Path $Path)) {
        Write-Host "No .env file found at $Path. Using current environment/appsettings fallback." -ForegroundColor Yellow
        return $values
    }

    foreach ($line in Get-Content $Path) {
        $trimmed = $line.Trim()

        if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed.StartsWith("#")) {
            continue
        }

        if ($trimmed -match '^\s*([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.*)\s*$') {
            $key = $matches[1]
            $value = $matches[2].Trim()

            if ($value.Length -ge 2) {
                $startsDouble = $value.StartsWith('"')
                $endsDouble = $value.EndsWith('"')
                $startsSingle = $value.StartsWith("'")
                $endsSingle = $value.EndsWith("'")

                if (($startsDouble -and $endsDouble) -or ($startsSingle -and $endsSingle)) {
                    $value = $value.Substring(1, $value.Length - 2)
                }
            }

            $values[$key] = $value
        }
        else {
            Write-Host "Skipping invalid .env entry: $line" -ForegroundColor DarkYellow
        }
    }

    return $values
}

function Get-EnvSnapshot {
    param([string[]]$Keys)

    $snapshot = @{}

    foreach ($key in $Keys) {
        $envPath = "Env:$key"
        if (Test-Path $envPath) {
            $snapshot[$key] = @{ Exists = $true; Value = (Get-Item $envPath).Value }
        }
        else {
            $snapshot[$key] = @{ Exists = $false; Value = $null }
        }
    }

    return $snapshot
}

function Restore-EnvSnapshot {
    param([hashtable]$Snapshot)

    foreach ($key in $Snapshot.Keys) {
        $entry = $Snapshot[$key]
        $envPath = "Env:$key"

        if ($entry.Exists) {
            Set-Item -Path $envPath -Value $entry.Value
        }
        else {
            Remove-Item -Path $envPath -ErrorAction SilentlyContinue
        }
    }
}

function Resolve-Value {
    param(
        [hashtable]$Values,
        [string]$PrimaryKey,
        [string]$FallbackKey
    )

    if ($PrimaryKey -and $Values.ContainsKey($PrimaryKey)) {
        return $Values[$PrimaryKey]
    }

    if ($FallbackKey -and $Values.ContainsKey($FallbackKey)) {
        return $Values[$FallbackKey]
    }

    return $null
}

$envValues = Read-EnvFile -Path $envFile

$managedKeys = @(
    "ConnectionStrings__DefaultConnection",
    "ConnectionStrings__RewardsConnection",
    "RabbitMQ__Host",
    "RabbitMQ__Username",
    "RabbitMQ__Password"
)

$baselineEnv = Get-EnvSnapshot -Keys $managedKeys

$services = @(
    @{ Name = "Gateway      (5000)"; Path = "$root\src\Gateway"; EnvPrefix = $null; NeedsRewardsConnection = $false },
    @{ Name = "AuthService  (5001)"; Path = "$root\src\Services\AuthService"; EnvPrefix = "AUTHSERVICE"; NeedsRewardsConnection = $false },
    @{ Name = "WalletService(5002)"; Path = "$root\src\Services\WalletService"; EnvPrefix = "WALLETSERVICE"; NeedsRewardsConnection = $false },
    @{ Name = "RewardsService(5003)"; Path = "$root\src\Services\RewardsService"; EnvPrefix = "REWARDSSERVICE"; NeedsRewardsConnection = $false },
    @{ Name = "NotifService (5004)"; Path = "$root\src\Services\NotificationService"; EnvPrefix = "NOTIFICATIONSERVICE"; NeedsRewardsConnection = $false },
    @{ Name = "AdminService (5005)"; Path = "$root\src\Services\AdminService"; EnvPrefix = "ADMINSERVICE"; NeedsRewardsConnection = $true },
    @{ Name = "SupportSvc   (5006)"; Path = "$root\src\Services\SupportTicketService"; EnvPrefix = "SUPPORTTICKETSERVICE"; NeedsRewardsConnection = $false }
)

foreach ($svc in $services) {
    Restore-EnvSnapshot -Snapshot $baselineEnv

    foreach ($rabbitKey in @("RabbitMQ__Host", "RabbitMQ__Username", "RabbitMQ__Password")) {
        if ($envValues.ContainsKey($rabbitKey) -and -not [string]::IsNullOrWhiteSpace($envValues[$rabbitKey])) {
            Set-Item -Path "Env:$rabbitKey" -Value $envValues[$rabbitKey]
        }
    }

    if ($svc.EnvPrefix) {
        $serviceDefaultConnection = Resolve-Value -Values $envValues -PrimaryKey ("{0}__ConnectionStrings__DefaultConnection" -f $svc.EnvPrefix) -FallbackKey "ConnectionStrings__DefaultConnection"

        if (-not [string]::IsNullOrWhiteSpace($serviceDefaultConnection)) {
            Set-Item -Path "Env:ConnectionStrings__DefaultConnection" -Value $serviceDefaultConnection
        }
        elseif (-not (Test-Path "Env:ConnectionStrings__DefaultConnection")) {
            Write-Host "Warning: $($svc.Name) has no .env DB override. Falling back to appsettings." -ForegroundColor DarkYellow
        }
    }

    if ($svc.NeedsRewardsConnection) {
        $serviceRewardsConnection = Resolve-Value -Values $envValues -PrimaryKey "ADMINSERVICE__ConnectionStrings__RewardsConnection" -FallbackKey "ConnectionStrings__RewardsConnection"

        if (-not [string]::IsNullOrWhiteSpace($serviceRewardsConnection)) {
            Set-Item -Path "Env:ConnectionStrings__RewardsConnection" -Value $serviceRewardsConnection
        }
        elseif (-not (Test-Path "Env:ConnectionStrings__RewardsConnection")) {
            Write-Host "Warning: AdminService has no .env RewardsConnection override. Falling back to appsettings." -ForegroundColor DarkYellow
        }
    }

    Write-Host "Starting $($svc.Name)..." -ForegroundColor Cyan
    Start-Process powershell -WorkingDirectory $svc.Path -ArgumentList "-NoExit", "-Command", "dotnet run --launch-profile http" -WindowStyle Normal
    Start-Sleep -Milliseconds 500
}

Restore-EnvSnapshot -Snapshot $baselineEnv

Write-Host ""
Write-Host "All services started. Gateway is at http://localhost:5000" -ForegroundColor Green
Write-Host "Loaded DB/RabbitMQ overrides from .env when present." -ForegroundColor Green
Write-Host "Start the frontend with: cd frontend && ng serve" -ForegroundColor Yellow
