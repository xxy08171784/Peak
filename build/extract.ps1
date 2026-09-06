$gameDir = "c:\Users\shiqian\Peak\bulid\game_loc_new"
$modDir = "c:\Users\shiqian\Peak\localization\zhs"
$outDir = "c:\Users\shiqian\Peak\peak\localization\zhs"
$files = @("cards","powers","relics","ancients","characters","epochs","potions","card_library","rest_site_ui")

function Get-JsonKeys($path) {
    $text = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
    $result = @{}
    $text -split "`r`n|`n" | ForEach-Object {
        if ($_ -match '^  "(.+?)":') { $result[$matches[1]] = $true }
    }
    $result
}

function Write-JsonFromModOnly($modPath, $gameKeys, $outPath, $manuals) {
    $text = [System.IO.File]::ReadAllText($modPath, [System.Text.Encoding]::UTF8)
    $lines = $text -split "`r`n|`n"
    $output = @()
    $currentKey = $null
    $currentVal = @()
    $inBlock = $false
    foreach ($line in $lines) {
        if ($line -match '^  "(.+?)":') {
            if ($currentKey -ne $null -and (-not $gameKeys.ContainsKey($currentKey) -or ($manuals -and $manuals.ContainsKey($currentKey)))) {
                $output += $currentVal
            }
            $currentKey = $matches[1]
            $currentVal = @($line)
            $inBlock = $true
        } elseif ($inBlock) {
            $currentVal += $line
            if ($line -match ',$' -or $line -match '"$') {
                # end of value, continue
            }
        }
    }
    if ($currentKey -ne $null -and (-not $gameKeys.ContainsKey($currentKey) -or ($manuals -and $manuals.ContainsKey($currentKey)))) {
        $output += $currentVal
    }
    $result = "{$([Environment]::NewLine)" + ($output -join [Environment]::NewLine) + "$([Environment]::NewLine)}"
    [System.IO.File]::WriteAllText($outPath, $result, [System.Text.UTF8Encoding]::new($false))
    $output.Count
}

foreach ($f in $files) {
    $gp = Join-Path $gameDir "$f.json"
    $mp = Join-Path $modDir "$f.json"
    $op = Join-Path $outDir "$f.json"
    if (!(Test-Path $mp)) { Write-Host "$f: mod missing"; continue }
    if (!(Test-Path $gp)) { Write-Host "$f: game_loc_new missing"; continue }
    $gameKeys = Get-JsonKeys $gp
    $manuals = @{}
    if ($f -eq "ancients") { $manuals["NEOW.talk.firstVisitEver.0-0.ancient"] = $true }
    $cnt = Write-JsonFromModOnly $mp $gameKeys $op $manuals
    Write-Host "$f: modOnly=$cnt"
}
