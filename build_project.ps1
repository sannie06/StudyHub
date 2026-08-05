$env:DOTNET_CLI_HOME = "d:\DATN\StudyHub\.dotnet_home"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "true"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = "0"
$env:DOTNET_NOLOGO = "true"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "true"

Write-Host "=== Running MSBuild via dotnet build ==="
dotnet build d:\DATN\StudyHub\StudyHub.sln
