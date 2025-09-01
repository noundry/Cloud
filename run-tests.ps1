#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Runs tests for the NDC CLI project
.DESCRIPTION
    This script builds the solution and runs all unit tests with code coverage
.PARAMETER Configuration
    The build configuration (Debug or Release). Default is Release.
.PARAMETER Coverage
    Whether to generate code coverage reports. Default is true.
.PARAMETER Watch
    Whether to run tests in watch mode. Default is false.
#>

param(
    [string]$Configuration = "Release",
    [bool]$Coverage = $true,
    [bool]$Watch = $false
)

$ErrorActionPreference = "Stop"

Write-Host "🔧 NDC CLI Test Runner" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Gray
Write-Host "Coverage: $Coverage" -ForegroundColor Gray
Write-Host "Watch mode: $Watch" -ForegroundColor Gray
Write-Host ""

try {
    # Clean previous test results
    Write-Host "🧹 Cleaning previous test results..." -ForegroundColor Yellow
    if (Test-Path "TestResults") {
        Remove-Item "TestResults" -Recurse -Force
    }

    # Restore dependencies
    Write-Host "📦 Restoring dependencies..." -ForegroundColor Yellow
    dotnet restore ndc-csharp.sln
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

    # Build solution
    Write-Host "🔨 Building solution..." -ForegroundColor Yellow
    dotnet build ndc-csharp.sln --no-restore --configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }

    # Run tests
    Write-Host "🧪 Running tests..." -ForegroundColor Yellow
    
    $testArgs = @(
        "test",
        "tests/NDC.Cli.Tests/NDC.Cli.Tests.csproj",
        "--no-build",
        "--configuration", $Configuration,
        "--verbosity", "normal",
        "--logger", "trx",
        "--results-directory", "TestResults"
    )

    if ($Coverage) {
        $testArgs += @("--collect", "XPlat Code Coverage")
        $testArgs += @("--settings", "tests/NDC.Cli.Tests/TestSettings.runsettings")
    }

    if ($Watch) {
        $testArgs += "--watch"
    }

    & dotnet @testArgs
    if ($LASTEXITCODE -ne 0) { throw "Tests failed" }

    # Generate coverage report
    if ($Coverage -and !$Watch) {
        Write-Host "📊 Generating coverage report..." -ForegroundColor Yellow
        
        # Find coverage file
        $coverageFile = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse | Select-Object -First 1
        
        if ($coverageFile) {
            Write-Host "Coverage file: $($coverageFile.FullName)" -ForegroundColor Gray
            
            # Try to install and use reportgenerator if available
            try {
                dotnet tool install --global dotnet-reportgenerator-globaltool --ignore-failed-sources 2>$null
                
                $reportDir = "TestResults/coverage-report"
                dotnet reportgenerator -reports:$($coverageFile.FullName) -targetdir:$reportDir -reporttypes:Html
                
                if (Test-Path "$reportDir/index.html") {
                    Write-Host "📋 Coverage report generated: $reportDir/index.html" -ForegroundColor Green
                }
            }
            catch {
                Write-Host "⚠️  Could not generate HTML coverage report. Raw coverage data available in TestResults/" -ForegroundColor Yellow
            }
        }
    }

    Write-Host "✅ Tests completed successfully!" -ForegroundColor Green

} catch {
    Write-Host "❌ Test run failed: $_" -ForegroundColor Red
    exit 1
}