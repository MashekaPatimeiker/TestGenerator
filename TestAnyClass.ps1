# SimpleGenerator.ps1
param([string]$CsFile)

if (-not $CsFile) {
    $CsFile = Read-Host "Enter path to C# file"
}

if (-not (Test-Path $CsFile)) {
    Write-Host "File not found!" -ForegroundColor Red
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   TEST GENERATOR" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "File: $CsFile" -ForegroundColor Yellow
Write-Host ""

# Переходим в папку проекта
cd D:\SPP\TestGenerator\TestGenerator

# Генерируем тесты
Write-Host "Generating tests..." -ForegroundColor Green
dotnet run -- $CsFile "./GeneratedTests"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Generated files:" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan

# Показываем сгенерированные файлы
Get-ChildItem ./GeneratedTests/*.cs | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor White
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "First generated test preview:" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan

# Показываем содержимое первого теста
$firstTest = Get-ChildItem ./GeneratedTests/*.cs | Select-Object -First 1
if ($firstTest) {
    Get-Content $firstTest.FullName -First 25
}

Write-Host ""
Write-Host "DONE! Tests generated successfully!" -ForegroundColor Green