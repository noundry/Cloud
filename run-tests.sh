#!/bin/bash

# NDC CLI Test Runner for Linux/macOS
# Usage: ./run-tests.sh [configuration] [coverage] [watch]
#   configuration: Debug or Release (default: Release)
#   coverage: true or false (default: true)  
#   watch: true or false (default: false)

set -e

CONFIGURATION=${1:-Release}
COVERAGE=${2:-true}
WATCH=${3:-false}

echo "🔧 NDC CLI Test Runner"
echo "Configuration: $CONFIGURATION"
echo "Coverage: $COVERAGE"
echo "Watch mode: $WATCH"
echo ""

# Clean previous test results
echo "🧹 Cleaning previous test results..."
if [ -d "TestResults" ]; then
    rm -rf TestResults
fi

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore ndc-csharp.sln

# Build solution
echo "🔨 Building solution..."
dotnet build ndc-csharp.sln --no-restore --configuration $CONFIGURATION

# Run tests
echo "🧪 Running tests..."

TEST_ARGS="test tests/NDC.Cli.Tests/NDC.Cli.Tests.csproj --no-build --configuration $CONFIGURATION --verbosity normal --logger trx --results-directory TestResults"

if [ "$COVERAGE" = "true" ]; then
    TEST_ARGS="$TEST_ARGS --collect \"XPlat Code Coverage\" --settings tests/NDC.Cli.Tests/TestSettings.runsettings"
fi

if [ "$WATCH" = "true" ]; then
    TEST_ARGS="$TEST_ARGS --watch"
fi

dotnet $TEST_ARGS

# Generate coverage report
if [ "$COVERAGE" = "true" ] && [ "$WATCH" = "false" ]; then
    echo "📊 Generating coverage report..."
    
    # Find coverage file
    COVERAGE_FILE=$(find TestResults -name "coverage.cobertura.xml" | head -1)
    
    if [ -n "$COVERAGE_FILE" ]; then
        echo "Coverage file: $COVERAGE_FILE"
        
        # Try to install and use reportgenerator if available
        if dotnet tool install --global dotnet-reportgenerator-globaltool --ignore-failed-sources 2>/dev/null; then
            REPORT_DIR="TestResults/coverage-report"
            dotnet reportgenerator -reports:$COVERAGE_FILE -targetdir:$REPORT_DIR -reporttypes:Html
            
            if [ -f "$REPORT_DIR/index.html" ]; then
                echo "📋 Coverage report generated: $REPORT_DIR/index.html"
            fi
        else
            echo "⚠️  Could not generate HTML coverage report. Raw coverage data available in TestResults/"
        fi
    fi
fi

echo "✅ Tests completed successfully!"