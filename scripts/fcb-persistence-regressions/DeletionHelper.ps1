# Deletion-helper filesystem regressions: the unchanged contract from the
# first corrective pass, exercised against real temporary files.
$scenario = New-ScenarioRoot 'helper'
$catalog = [pscustomobject]@{ Directory = (Join-Path $scenario 'saves'); Files = @([pscustomobject]@{ path = (Join-Path $scenario 'saves\Manual_299_KMG_AUTOMATION_WORKING.zks') }) }
$save = New-OwnedSave $scenario 'original campaign bytes'
[IO.File]::WriteAllText($save.path, 'modified campaign bytes')
$failures = New-Object 'System.Collections.Generic.List[object]'
$deleted = Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $failures
if ($deleted -or -not (Test-Path -LiteralPath $save.path -PathType Leaf) -or $failures.Count -ne 1 -or
    -not ([string]$failures[0].reason).Contains('completed-save receipt')) { throw 'Changed owned-save output was deleted or not reported.' }
$script:checks++
$save = New-OwnedSave $scenario 'first campaign'
Remove-Item -LiteralPath $save.path -Force
[IO.File]::WriteAllText($save.path, 'replaced campaign with different length bytes')
$failures = New-Object 'System.Collections.Generic.List[object]'
$deleted = Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $failures
if ($deleted -or -not (Test-Path -LiteralPath $save.path -PathType Leaf) -or $failures.Count -ne 1 -or
    -not ([string]$failures[0].reason).Contains('completed-save receipt')) { throw 'Replaced owned-save output was deleted or not reported.' }
$script:checks++
$save = New-OwnedSave $scenario 'unproven campaign'
$save.completedSha256 = $null
$failures = New-Object 'System.Collections.Generic.List[object]'
$deleted = Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $failures
if ($deleted -or -not (Test-Path -LiteralPath $save.path -PathType Leaf) -or
    -not ([string]$failures[0].reason).Contains('missing authoritative owned-save proof')) { throw 'Unproven owned-save output was deleted or not reported.' }
$script:checks++
$save = New-OwnedSave $scenario 'vanished campaign'
Remove-Item -LiteralPath $save.path -Force
$failures = New-Object 'System.Collections.Generic.List[object]'
$deleted = Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $failures
if ($deleted -or $failures.Count -ne 1 -or
    -not ([string]$failures[0].reason).Contains('owned save file is absent')) { throw 'Absent owned-save file was not reported.' }
$script:checks++
$save = New-OwnedSave $scenario 'escaped campaign'
$save.path = Join-Path (Join-Path $scenario 'outside') 'Manual_88_escape.zks'
$failures = New-Object 'System.Collections.Generic.List[object]'
$deleted = Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $failures
if ($deleted -or -not ([string]$failures[0].reason).Contains('escaped its proven transaction')) { throw 'Escaping cleanup target was deleted or not refused.' }
$script:checks++
$save = New-OwnedSave $scenario 'exact campaign bytes'
$failures = New-Object 'System.Collections.Generic.List[object]'
$deleted = Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $failures
if (-not $deleted -or (Test-Path -LiteralPath $save.path -PathType Leaf) -or $failures.Count -ne 0) { throw 'A fully proven unchanged owned save was not deleted.' }
$script:checks++
$save = New-OwnedSave $scenario 'protected bytes'
$save.path = $catalog.Files[0].path
[IO.File]::WriteAllText($save.path, 'protected')
$failures = New-Object 'System.Collections.Generic.List[object]'
$deleted = Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $failures
if ($deleted -or -not (Test-Path -LiteralPath $save.path -PathType Leaf)) { throw 'A protected-catalog file was deleted.' }
$script:checks++
