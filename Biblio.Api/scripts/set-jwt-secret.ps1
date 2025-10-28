param(
 [string]$ProjectDir = "Biblio.Api"
)

# Script to generate a strong random JWT key and store it in User Secrets for the given project
# Usage: .\set-jwt-secret.ps1 [-ProjectDir "Biblio.Api"]

Set-StrictMode -Version Latest

function Abort($msg) {
 Write-Error $msg
 exit 1
}

# Resolve project directory
$root = Get-Location
$projPath = Resolve-Path -Path (Join-Path $root.Path $ProjectDir) -ErrorAction SilentlyContinue
if (-not $projPath) { Abort "Project directory '$ProjectDir' not found from current location ($root)." }
$projPath = $projPath.Path

# Find csproj
$csproj = Get-ChildItem -Path $projPath -Filter *.csproj | Select-Object -First 1
if (-not $csproj) { Abort "No .csproj found in '$projPath'" }
$csprojPath = $csproj.FullName

Write-Host "Using project file: $csprojPath"

# Ensure user-secrets is initialized for the project
try {
 dotnet user-secrets init --project "$csprojPath" | Out-Null
} catch {
 Write-Host "User secrets init may have already been run or failed; continuing..." -ForegroundColor Yellow
}

# Generate 64 random bytes and base64 encode (512-bit key)
try {
 $bytes = New-Object System.Byte[] 64
 [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
 $key = [Convert]::ToBase64String($bytes)
} catch {
 Abort "Failed to generate key: $($_.Exception.Message)"
}

# Set the key in user secrets
try {
 dotnet user-secrets set "Jwt:Key" "$key" --project "$csprojPath"
 Write-Host "Successfully set Jwt:Key in user secrets for project: $($csproj.Name)"
 Write-Host "Key (base64) length: $($key.Length)"
 Write-Host "DO NOT commit this key. Use User Secrets / Env vars for production."
} catch {
 Abort "Failed to set user secret: $($_.Exception.Message)"
}
