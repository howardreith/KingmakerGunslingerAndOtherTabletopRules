# Cleanup 2 module-OFF native observer

2026-10-21. Source checkpoint only. Production compilation and the fresh guarded
native observation remain pending with the integrating runner. No native PASS,
visible learning UI, natural level-up, cold-save or serialized-reference claim
is made by this checkpoint.

## Scope and integration

Owned source changes are the new
`RuntimeTesting/RuntimeTestRunner.SummonCleanup2ModuleOff.cs`, one call immediately
before `CreateResult` in `RuntimeTestRunner.ExpandedSummoningBoundary.cs`, and
one read-only property in `StandaloneElementalPresentationPolicy.cs`. The
property returns an ordinally sorted read-only copy of its existing ten-GUID
ordinary-list set; there is no second identity map.

The call runs only when both the existing request's `expandedSummoning` value
and the actual active module flag are false. The helper also verifies the exact
existing `observe-expanded-summoning-module-boundary` scenario and those OFF
values. The enabled branch is unchanged. Existing scenario admission, thirteen
boolean module parameters, fresh-process and outer install/save leases remain
owned by the existing guarded orchestration. No fixture, world, unit, book,
selection, progression or save is created here. Project compile registration,
scripts, guards, packages and actual game execution are integration work.

## Actual observations and assertions

- Resolve all six standalone parents, twenty-four children and the unique
  Wriggling Man definition through the existing catalog and actual
  `BlueprintLibraryLookup.RequireExact`. Record actual identity, parent,
  component-array and component references, and variant-array/row references.
- Resolve the ten existing ordinary spell-list identities. Census actual raw
  catalog memberships across the loaded native spell lists, including raw
  array index, declared level, row order, exact resolved-definition identity
  and ordinary/special/child/unique disposition.
- Record each ordinary-list/parent/level membership or absence individually.
  For each special consumer discovered by actual raw catalog membership,
  record its six parent/level memberships and absences separately. A missing
  Bard high-level row or special-list parent is an explicit exception, not a
  failed requirement that every list contain all six parents. Each parent
  must nevertheless have positive actual ordinary membership somewhere.
- For every native array index plus catalog parent tier, call the actual
  native `GetSpells`, then actual `FilterLearningSpells(rows, list, level,
  null, null)` twice. Both results must be fresh lists, distinct from each
  other and the native source, with identical row references and order.
  Require positive parent rows in actual helper inputs to avoid an entirely
  empty-copy observation.
- Record the exact active module/helper flags, PC and console seam states,
  actual native methods and this mod's exact owned transpilers. Both installed
  learning hooks are expected while their helper's module guard is OFF.
- Check captured native raw-list/level-array identities and order, post-read
  native cache identities and order, ability parent references, component
  array/object references and variant array/row references after all probes.
  The helper's own bounded diagnostics may record its OFF disposition.

There are no assignments to native blueprint, spell-list, component, variant,
book or save fields. Native `GetSpells` may lazily populate its own filtered
cache before that cache is captured; this is explicitly allowed and is not
reported as a pre-read cache-invariance claim. Special consumers are raw reads
only. Missing definitions/lists, exceptions, changed surfaces or missing
positive memberships produce failures rather than substitute fixtures.

## Evidence schema

The new atomic evidence file is `summon-cleanup2-module-off.json`, schema 1.
It contains `runId`, exact scenario and flags, scope, `definitions`,
`rawMemberships`, `ordinaryParentMembershipsAndExceptions`,
`specialParentMembershipsAndExceptions`, explicit `checks` rows and aggregate
status. Each result assertion links that file. Helper checks carry native
source/copy object identifiers and explicit ordered GUID/reference rows;
reference-equality assertions use `ReferenceEquals`, not diagnostic hash IDs.

All objects and arrays are constructed as explicit `JObject`/`JArray` records
and written directly as JSON text. No dictionary is passed to
`JObject.FromObject` or the native global serializer converters.

## Installed API inspection and checks

Actual Kingmaker 2.1.7b `Assembly-CSharp.dll` read from the assigned installation:

- SHA-256: `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`
- MVID: `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`

Read-only IL inspection confirms `BlueprintSpellList.GetSpells(int)` (36 bytes,
token `0x06009B6D`) indexes `SpellsByLevel` by array index; an out-of-range
index returns the native `EmptyAbilitiesList`. It does not search the declared
`SpellLevel` field. `SpellLevelList.get_SpellsFiltered` (83 bytes, token
`0x06009B71`) lazily initializes or reuses its native list and adds unlocked raw
spells. The observer derives probe indexes accordingly and records both the
index and actual declared level.

Reproduction from this worktree in Windows PowerShell 5.1, using the existing
local IL reader without editing or writing it:

```powershell
$auditPath = 'artifacts/cleanup2-source-audit/read-native-methods.ps1'
$auditSource = Get-Content -LiteralPath $auditPath -Raw
$requestStart = $auditSource.IndexOf('$requests = @{')
$loopStart = $auditSource.IndexOf('foreach ($typeName in $requests.Keys)')
$boundedRequests = '$requests = @{ ''Kingmaker.Blueprints.Classes.Spells.BlueprintSpellList'' = @(''GetSpells''); ''Kingmaker.Blueprints.Classes.Spells.SpellLevelList'' = @(''get_SpellsFiltered'') }' + [Environment]::NewLine
& ([ScriptBlock]::Create($auditSource.Substring(0, $requestStart) + $boundedRequests + $auditSource.Substring($loopStart)))
```

Final repeated read: exit 0, 0.1938 seconds measured by `Stopwatch`. This is API
inspection, not a behavioral test or a native scenario result. Scoped source
diff review confirmed only the one boundary call and read-only policy property
in existing files. `git diff --check -- <four owned files>` plus an explicit
trailing-whitespace scan covering the new files passed, exit 0, 0.4537 seconds.
No build, game, installation, save, package or Git mutation was run by this task.

Remaining integration checks: include the new partial source, compile the
coherent candidate, then run the exact existing guarded module-OFF boundary
scenario in a fresh process with its current package and outer restoration
leases. Inspect both the scenario assertions and the explicit new evidence
file. Any UI or cold-save acceptance requires its separately authorized native
procedure and cannot be inferred from this observer.
