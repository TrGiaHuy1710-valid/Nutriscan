Set-Location (Join-Path $PSScriptRoot "..")
.\.dotnet\dotnet.exe run --project apps\frontend\NutriScan\NutriScan.csproj --urls http://127.0.0.1:5099
