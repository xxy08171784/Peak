param(
    [string]$modDir = "c:\Users\shiqian\Peak\localization\zhs",
    [string]$gameDir = "c:\Users\shiqian\Peak\bulid\game_loc_new",
    [string]$outDir = "c:\Users\shiqian\Peak\peak\localization\zhs",
    [string]$filter = "cards,powers,relics,ancients,characters,epochs,potions,card_library,rest_site_ui"
)

# Intentional overrides: game keys that the mod intentionally overrides
$overrides = @{
    "ancients" = @{
        "NEOW.talk.firstVisitEver.0-0.ancient" = $true  # placeholder, set manually
    }
}
# Manual override for NEOW
$neowOverride = "欢迎来到高塔……"

$files = $filter -split ','

foreach ($f in $files) {
    $gp = Join-Path $gameDir "$f.json"
    $mp = Join-Path $modDir "$f.json"
    $op = Join-Path $outDir "$f.json"
    
    if (-not (Test-Path $mp)) {
        Write-Host "$f`: mod source missing"
        continue
    }
    
    $modText = [System.IO.File]::ReadAllText($mp, [System.Text.Encoding]::UTF8)
    $modLines = $modText -split "`r`n|`n"
    
    # Parse mod JSON
    $modData = @{}
    $currentKey = $null
    $currentValue = ""
    $inValue = $false
    
    foreach ($line in $modLines) {
        if ($line -match '^  "(.+?)": (".*)$') {
            if ($currentKey -ne $null) {
                $modData[$currentKey] = $currentValue
            }
            $currentKey = $matches[1]
            $currentValue = $matches[2]
            $inValue = $true
            if ($currentValue.EndsWith('",') -or $currentValue.EndsWith('"}') -or $currentValue.EndsWith('"')) {
                # complete value
            }
        }
        elseIf ($inValue -and $line -match '^  (".*)$') {
            $currentValue += "`n" + $matches[1]
        }
        elseIf ($line -eq '}') {
            if ($currentKey -ne $null) {
                $modData[$currentKey] = $currentValue
            }
        }
    }
    
    Write-Host "$f`: parsed modData=$($modData.Count)"
}