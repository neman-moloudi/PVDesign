#Requires -Version 5.1
param(
    [string]$BuildConfig = "Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ProjectDir   = $PSScriptRoot
$DllPath      = "$ProjectDir\bin\$BuildConfig\PVDesigner.dll"
$AddinSrc     = "$ProjectDir\PVDesigner.addin"
$AddinsFolder = "$env:APPDATA\Autodesk\Inventor 2025\Addins"

Write-Host "=== PV Designer - Build and Install ===" -ForegroundColor Cyan

# 1. Build
Write-Host ""
Write-Host "Step 1: Building project..." -ForegroundColor Yellow
Push-Location $ProjectDir
dotnet build PVDesigner.csproj -c $BuildConfig
Pop-Location

if (-not (Test-Path $DllPath)) {
    Write-Error "Build failed - DLL not found at: $DllPath"
    exit 1
}
Write-Host "  Built: $DllPath" -ForegroundColor Green

# 2. Prepare .addin with correct path
Write-Host ""
Write-Host "Step 2: Updating .addin assembly path..." -ForegroundColor Yellow
[xml]$addin = Get-Content $AddinSrc
$addin.Addin.Assembly = $DllPath
$AddinOut = "$ProjectDir\bin\$BuildConfig\PVDesigner.addin"
$addin.Save($AddinOut)
Write-Host "  Written: $AddinOut" -ForegroundColor Green

# 3. Copy .addin to Inventor Addins folder
Write-Host ""
Write-Host "Step 3: Installing .addin file..." -ForegroundColor Yellow
if (-not (Test-Path $AddinsFolder)) {
    New-Item -ItemType Directory -Path $AddinsFolder | Out-Null
}
Copy-Item $AddinOut "$AddinsFolder\PVDesigner.addin" -Force
Write-Host "  Installed to: $AddinsFolder\PVDesigner.addin" -ForegroundColor Green

# 4. Register the COM server (required so Inventor can load the .NET DLL)
Write-Host ""
Write-Host "Step 4: Registering COM server..." -ForegroundColor Yellow
$regasm = "$env:SystemRoot\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"
if (Test-Path $regasm) {
    & $regasm $DllPath /codebase /nologo
    Write-Host "  COM registration complete." -ForegroundColor Green
} else {
    Write-Warning "RegAsm not found. If Inventor cannot load the add-in, run RegAsm manually."
}

Write-Host ""
Write-Host "=== Installation complete ===" -ForegroundColor Cyan
Write-Host "Start (or restart) Autodesk Inventor 2025."
Write-Host "Go to Tools ribbon tab, find the 'PV Designer' panel and click the button."
