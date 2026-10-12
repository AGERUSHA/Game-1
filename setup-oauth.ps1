# Saves Google / Microsoft sign-in keys into .NET user-secrets (outside the project folder)
# and restarts the site. Leave a field empty to skip that provider.
$project = Join-Path $PSScriptRoot "Game1\Game1.csproj"

function Save-Secret($name, $value) {
    if ($value) { dotnet user-secrets set $name $value --project $project | Out-Null; Write-Host "  saved $name" -ForegroundColor Green }
}
function Read-Secret($prompt) {
    $s = Read-Host $prompt -AsSecureString
    [Runtime.InteropServices.Marshal]::PtrToStringBSTR([Runtime.InteropServices.Marshal]::SecureStringToBSTR($s))
}

Write-Host "`n=== Google (redirect URI: http://localhost:5177/signin-google) ===" -ForegroundColor Cyan
$gId = Read-Host "Google Client ID (Enter = skip)"
if ($gId) {
    Save-Secret "Authentication:Google:ClientId" $gId.Trim()
    Save-Secret "Authentication:Google:ClientSecret" (Read-Secret "Google Client secret").Trim()
}

Write-Host "`n=== Microsoft (redirect URI: http://localhost:5177/signin-microsoft) ===" -ForegroundColor Cyan
$mId = Read-Host "Microsoft Application (client) ID (Enter = skip)"
if ($mId) {
    Save-Secret "Authentication:Microsoft:ClientId" $mId.Trim()
    Save-Secret "Authentication:Microsoft:ClientSecret" (Read-Secret "Microsoft client secret VALUE").Trim()
}

Write-Host "`nRestarting the site..." -ForegroundColor Cyan
Get-Process Game1 -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep 1
Start-Process powershell -ArgumentList '-NoExit', '-Command', "dotnet run --project `"$project`" --launch-profile http"
Start-Sleep 8
Start-Process "http://localhost:5177/Account/Login"
Write-Host "Done. Open http://localhost:5177/Account/Login" -ForegroundColor Green
