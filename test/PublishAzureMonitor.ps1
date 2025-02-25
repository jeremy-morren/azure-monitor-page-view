$project = Join-Path $PSScriptRoot "../azure-sdk-for-net/sdk/monitor/Azure.Monitor.OpenTelemetry.Exporter/src/Azure.Monitor.OpenTelemetry.Exporter.csproj"

# Pack the project
$nugets = Join-Path $PSScriptRoot "Packages"
New-Item -ItemType Directory -Path $nugets -ErrorAction SilentlyContinue

dotnet pack -c Release $project -o $nugets -p "PackageVersion=1.4.0-beta.3-pageView"