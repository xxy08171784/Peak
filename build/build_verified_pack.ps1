param(
	[string]$GodotConsole = "D:\DevTools\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64_console.exe",
	[string]$Dotnet = "D:\DevTools\dotnet9\dotnet.exe",
	[string]$DotnetCliHome = "D:\DevTools\peak-build-cache\cli-home",
	[string]$NugetPackages = "D:\DevTools\peak-build-cache\nuget",
	[string]$TempRoot = "D:\DevTools\peak-build-cache\temp",
	[Parameter(Mandatory = $true)]
	[string]$OutputPath
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$auditProject = Join-Path $PSScriptRoot "pack_audit_project"
$outputFullPath = [System.IO.Path]::GetFullPath($OutputPath)
$logDirectory = Split-Path -Parent $outputFullPath

if (-not $GodotConsole.EndsWith("_console.exe", [System.StringComparison]::OrdinalIgnoreCase)) {
	throw "GodotConsole must point to the blocking _console.exe executable: $GodotConsole"
}
if (-not (Test-Path -LiteralPath $GodotConsole -PathType Leaf)) {
	throw "Godot console executable not found: $GodotConsole"
}
if (-not (Test-Path -LiteralPath $Dotnet -PathType Leaf)) {
	throw ".NET executable not found: $Dotnet"
}
if (Test-Path -LiteralPath $outputFullPath) {
	throw "Refusing to overwrite an existing artifact: $outputFullPath"
}

New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $DotnetCliHome -Force | Out-Null
New-Item -ItemType Directory -Path $NugetPackages -Force | Out-Null
New-Item -ItemType Directory -Path $TempRoot -Force | Out-Null
$env:DOTNET_ROOT = Split-Path -Parent $Dotnet
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
$env:DOTNET_CLI_HOME = $DotnetCliHome
$env:NUGET_PACKAGES = $NugetPackages
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:TEMP = $TempRoot
$env:TMP = $TempRoot

function Invoke-CheckedProcess {
	param(
		[Parameter(Mandatory = $true)]
		[string]$Executable,
		[Parameter(Mandatory = $true)]
		[string[]]$Arguments,
		[string]$LogPath,
		[switch]$RejectGodotErrors
	)

	if ($LogPath) {
		& $Executable @Arguments *> $LogPath
	} else {
		& $Executable @Arguments
	}
	$exitCode = $LASTEXITCODE

	if ($exitCode -ne 0) {
		if ($LogPath) {
			Get-Content -LiteralPath $LogPath
		}
		throw "Process failed with exit code $exitCode`: $Executable"
	}

	if ($RejectGodotErrors -and $LogPath) {
		$errorLines = @(Select-String -LiteralPath $LogPath -Pattern "ERROR:")
		if ($errorLines.Count -gt 0) {
			Get-Content -LiteralPath $LogPath
			throw "Godot reported $($errorLines.Count) error line(s): $LogPath"
		}
	}
}

Push-Location $projectRoot
try {
	Invoke-CheckedProcess -Executable $Dotnet -Arguments @("build", "peak.csproj", "-c", "Debug")
	Invoke-CheckedProcess -Executable $Dotnet -Arguments @("build", "peak.csproj", "-c", "Release")
	Invoke-CheckedProcess -Executable "python" -Arguments @("build/audit_scout_cards.py")

	$importLog = Join-Path $logDirectory "peak-godot-import.log"
	Invoke-CheckedProcess `
		-Executable $GodotConsole `
		-Arguments @("--headless", "--path", $projectRoot, "--import") `
		-LogPath $importLog `
		-RejectGodotErrors

	$exportLog = Join-Path $logDirectory "peak-godot-export.log"
	Invoke-CheckedProcess `
		-Executable $GodotConsole `
		-Arguments @("--verbose", "--headless", "--path", $projectRoot, "--export-pack", "Windows Desktop 2", $outputFullPath) `
		-LogPath $exportLog `
		-RejectGodotErrors

	if (-not (Test-Path -LiteralPath $outputFullPath -PathType Leaf)) {
		throw "Godot returned success but did not create the PCK: $outputFullPath"
	}

	Invoke-CheckedProcess `
		-Executable $GodotConsole `
		-Arguments @(
			"--headless",
			"--path", $auditProject,
			"--script", "res://audit_peak_pack.gd",
			"--", $outputFullPath
		)

	Write-Host "Verified Peak pack: $outputFullPath"
} finally {
	Pop-Location
}
