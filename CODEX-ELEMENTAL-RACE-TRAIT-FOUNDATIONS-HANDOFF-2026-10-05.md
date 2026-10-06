# Elemental race-trait mechanics foundations handoff — 2026-10-05

## Status and repository provenance

FieryGlarePublished: false
StoicDignityPublished: false

**COMPLETE:** both unpublished mechanical foundations passed real-engine qualification twice from independent fresh processes on the exact clean committed artifact, followed by exact-artifact working-save smoke and byte-exact live restoration. The initial candidate evidence below remains historical; the clean-commit closure is authoritative. Neither trait is player-acquirable or claimed to ship. Howie supplied no human visual or acceptance results in this mission.

- Repository: `howardreith/KingmakerGunslingerAndOtherTabletopRules`; origin `https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules.git`.
- Exact starting SHA: `b28b5786d10a94f3257fafa5cf02bbd50471d182`, fetched solely from `origin/codex/gunslinger-content-followup-2026-10-04` without pull, rebase or tags; local and remote content tips verified equal before isolation and before this checkpoint.
- Branch: `codex/elemental-race-trait-foundations-2026-10-05`.
- Worktree: `C:\Dev\KingmakerGunslingerLab\worktrees\elemental-race-trait-foundations-2026-10-05`; freshly created at the exact base, with no competing ownership.
- Intake `master`: `2ce70e4e7e9d3c97ca1008ab05a341e758f5cf5a`. No master, content or Expanded Summoning movement was caused by this mission.
- No merge, rebase, reset, history rewrite, force push, tag, release, version bump or PR operation occurred. Only this dedicated branch is committed and pushed through the prescribed policy wrapper.
- Fresh checkout normalized one inherited mixed-EOL line in `scripts/test-domain.ps1`. Canonical text was compared to the exact base; its behavior is unchanged. This explained normalization is included rather than hiding a dirty checkout.

## Publication ledger

The mechanics builders have no ordinary initialization call. The only callers are the exact guarded foundation fixture. No blueprint manifest, selection, race grant, setting, localization entry, icon assignment, README feature list or release note was added.

| Identity or component | Identity disposition | Ordinary bootstrap / acquisition | Icon and save disposition |
|---|---|---|---|
| `UnpublishedRaceTraitFoundationFactory` | Compiled internal builder, unregistered | None; guarded fixture only | No icon consumer or save identity |
| `KMG_Unpublished_FieryGlare_Buff` | Hidden, unregistered request-local buff; fixture assigns a random GUID | No selection, race, feature or setting references | Null icon; disposable actor only; no save write |
| `KMG_Unpublished_FieryGlare_Toggle` | Unregistered request-local native activatable ability; random fixture GUID | No ordinary fact grant or action-bar acquisition | No art or localization; action-bar autofill ignored; removed before exit |
| Native `Take10ForSuccess` | Native component, configured solely for `CheckIntimidate` | Only the unpublished buff carries it | No new identity, icon or persistence path |
| `KMG_Unpublished_StoicDignity_Provider` | Hidden, unregistered request-local buff | Disposable holders only | Null icon; self-save component plus native area lifecycle |
| `KMG_Unpublished_StoicDignity_Recipient` | Hidden, unregistered request-local buff | Native area recipients among four closed fixture actors only | Null icon; removed on area exit/deactivation |
| `KMG_Unpublished_StoicDignity_Area` | Unregistered builder output; fixture-only GUID lookup lease | Native `BlueprintReference.Get()` requires a temporary lookup; exact owned entry in both indexes is removed before completion | Request-random GUID, no permanent registered symbol or acquisition; no icon consumer or save write |
| `StoicDignitySaveBonus` | Compiled mechanics component; no registered blueprint identity | Only transient provider/recipient facts | Per-fact, nonserialized weak replay ledger; no global unit state |
| `StoicDignityPolicy` / `StoicEffectIdentity` | Pure unregistered identity/eligibility policies | No bootstrap state | No icon or save representation |
| `RaceTraitFixtureActors` / `TraitAreaLookupLease` | Guarded runtime-only population condition and owned lookup rollback | Four disposable actors; no UI or ordinary initialization | Both library indexes, original units/areas/party/buffs restored exactly |

`Guid.NewGuid()` identities exist solely inside the no-write fixture. No stable production trait GUID, hidden persistent trait registration, icon waiver, donor art, placeholder, monogram or weakened validator was introduced. Temporary area lookup follows the exact `ElementalRaceDevelopmentProbeScenario.ProbeRegistration` precedent; it is not added to any player selector.

## Fiery Glare ledger

Classification: **ADAPTED — native success-only take-10**, not exact tabletop choice.

- Exact carrier: `Kingmaker.Designers.Mechanics.Facts.Take10ForSuccess.Skill = StatType.CheckIntimidate` (installed value `0x67`). Exact native type, field type and rule-handler signature are checked before construction.
- Native toggle: off by default, `AbilityActivationType.Immediately`, private exact `m_ActivateWithUnitCommand` set to `UnitCommand.CommandType.Free`, no resource logic, `DeactivateImmediately = true`, no combat-only restriction. Activation does not enqueue an action.
- Real `RuleSkillCheck`: absent/off controls roll 17 under an isolated native RNG seed; enabled successful take-10 produces exactly 10; DC 1000 preserves the native ordinary-roll path and rolls 17. Bluff, Diplomacy, general Persuasion, UMD and Perception remain unchanged.
- `CheckIntimidate` is the dialogue stat as well as the combat-compatible check. Actual combat state is established only on the disposable holder; no dialogue UI, campaign conversation or combat-content scenario is driven.
- Repeated off/on cycles retain one buff and one native component; immediate deactivation removes its effect and restores the control roll; two units remain independent; fact removal cleans the activatable ability. Missing exact component contract rejects construction.
- Future description constant: “While Fiery Glare is enabled, Intimidate checks use a result of 10 whenever that would succeed; otherwise, they are rolled normally. This functions even during combat.” It is not a visible localization entry.

Original art, the visible feature graph, honest resolved player text, acquisition and player/manual qualification remain follow-up work.

## Stoic Dignity ledger

**Exact identity hierarchy and policy:**

1. Incoming non-ability `BlueprintUnitFact` from `RuleReason.Fact.Blueprint`, otherwise the nearest exact associated fact/buff blueprint in current/parent `MechanicsContext`.
2. Exact `RuleReason.Ability.Blueprint`, otherwise the nearest associated `BlueprintAbility` in that explicit context chain.
3. Exact native `BlueprintAbility.Parent` ancestors. Context and ability walks are bounded at 32 and detect cycles; ambiguous identities never suppress.
4. Compare canonical blueprint object references. If both concrete effect facts are known, identical effect blueprints suppress and distinct effects grant, even when their granting ability is shared. Otherwise an identical source ability or direct child/ancestor relationship suppresses. Sibling abilities sharing only a generic ancestor remain unrelated.
5. Caster identity is intentionally irrelevant: the same exact effect blueprint/source ability from a different caster still counts as that effect. A different ability from the same caster remains unrelated. Names, localization, broad descriptors and guessed GUID relationships are never correlation inputs.

Only active, nonsuppressed buffs on the beneficiary are examined. Missing or ambiguous correlation grants the bonus; unsupported effect carriers are not approximated. Mind-affecting eligibility resolves exact native descriptors from current/parent contexts, associated blueprints, source fact and source ability/explicit parents. Descriptor equality alone never suppresses.

- Holder self: +1 `ModifierDescriptor.Trait`.
- Ally recipient: +1 `ModifierDescriptor.Morale`; native ally-only 10-foot cylinder and `ContextConditionIsCaster.Not = true` exclude the holder and enemies.
- Moving emanation: native `Kingmaker.UnitLogic.Buffs.Components.AddAreaEffect.OnFactActivate/OnFactDeactivate`; native area delivery and recipient cleanup. Radius uses the native float 3.048-metre boundary.
- Every save rechecks holder `UnitState.IsConscious`, `IsDead`, alliance and actual holder/recipient distance, so stale area membership cannot grant an out-of-range or unconscious-holder bonus.
- Real save rules prove exact ability identity, exact buff identity, explicit parent/source lineage, distinct effects under one source, unrelated Charm/Fear in both directions, and descriptor-only source fallback. The ambiguity fixture has a real native area source with a mind-affecting descriptor and no ability/buff/fact identity.
- Real native `LifeState.Unconscious` and `Dead` controls deny self and ally bonuses; restoring consciousness allows a later save before polling. Native `UnitCondition.Unconscious` alone is a separate flag and was rejected as the lifecycle fixture.
- Native temporary modifiers obey morale/trait stacking: two providers give one effective +1 morale; foreign +2 morale wins; self +1 trait and a different holder's +1 morale stack; foreign +2 trait wins over own +1 trait.
- Per-component `ConditionalWeakTable<RuleSavingThrow, object>` prevents duplicate delivery of the same rule object. The request-local replay probe deliberately delivers each native component twice. The production handler has no global patch, polling or unrelated-unit scan.
- Native rule cleanup removes temporary modifiers; area/provider removal cleans recipients; no existing effect reference, active state, duration or immunity is changed. Exact native context-contract mismatch fails closed before construction.

This is proven mechanics on exact request-local native constructions, not player publication or a claim of completed player-facing acceptance.

## Validation and first qualified candidate

Baseline: complete unfiltered domain suite **2040/2040 PASS** after producing the already-required local IL inspection artifact. Final source catalog: **2093** actual registered tests, with **20 Fiery + 33 Stoic** tests appended while preserving all baseline entries and their order.

Commands and results:

- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/test-domain.ps1 -Configuration Release -Clean`: intake baseline PASS.
- `KMG_TEST_FILTER=fiery-foundation.,stoic-foundation.` with the Release domain executable: **53/53 PASS** (focused evidence, not the full gate).
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Invoke-KmgGate.ps1 -Level Sprint`: repository/static/icon/manifest validation, complete unfiltered **2093/2093 PASS**, clean exact-reference Release build, deterministic package and strict standalone validation PASS. No runtime scenario is launched by this gate.
- Changed PowerShell parser validation: three scripts PASS. Changed Python syntax (`ast.parse`) PASS; no Python behavior changed.
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-RuntimeScenarioPreflight.ps1`: **484 checks PASS**, plus the existing world-map metadata check; source/WhatIf tests only, no excluded game scenario launch.
- `git diff --check`, staged-diff checks, tracked/untracked, generated/binary/proprietary/private/credential, machine-path and ancestry audits are required before each checkpoint and at closure. Native assemblies, IL, packages, logs, configuration, saves and machine-local helpers remain ignored/uncommitted.

Only the shared **current** deterministic-count pins in `tools/validate_expanded_summoning_phase2a141.py` and `validation/static-validation.json` change from 2040 to 2093 because of these 53 new tests, as explicitly permitted by the mission. Existing Expanded Summoning entries, package counts, assets, registrations and historical qualification records remain unchanged.

Candidate artifact (embedded base commit with exact dirty-source attestation, before the clean-commit closure):

| Identity | Value |
|---|---|
| Version | `0.0.141` — unchanged |
| Embedded commit | `b28b5786d10a94f3257fafa5cf02bbd50471d182` |
| Source fingerprint | `3cce264cda1f676ffe6717233a429b17b901922090bdb4e8259d8efa7fa6b188` |
| DLL SHA-256 | `dc5aae7dd36f902dc97367654f5dca74371ff54ee80cec2e7b121308cebeaeff` |
| DLL MVID | `702e552d-3eeb-409f-bdac-c9269335e487` |
| ZIP SHA-256 | `3db6eb5aedc23cd2d377f49af05ce5f71fc346b74f8ebc8febbc2d31d7495746` |

Guarded invocation uses `scripts/Invoke-KingmakerRuntimeTest.ps1 -Scenario <exact scenario> -ExpectedVersion 0.0.141 -SaveName KMG_AUTOMATION_WORKING -ExitAfterCompletion:$true`, the immutable package and deployment receipt, closed parameters, automatic exit, and Steam App ID **640820**. `AllowDirtyGit` was used only for this first candidate; its exact source fingerprint was verified. The clean-artifact closure uses a clean tree without that switch.

| Scenario | Run/evidence identifier | Fresh game PID | Assertions |
|---|---|---:|---|
| observe-unpublished-race-trait-foundations | `20261005T2227347799665Z-e92bee25825a44bca74b417443f90885` | 39548 | 49/49 PASS |
| observe-unpublished-race-trait-foundations | `20261005T2228425382450Z-95cfd21b149d41bfae56f070cb4a87be` | 21288 | 49/49 PASS |
| working-save-smoke | `20261005T2229507044344Z-a688985fbf754904ac4eaa24990a2bc6` | 22708 | 11/11 PASS |

Both foundation runs are independent fresh processes on the same DLL/ZIP/MVID. All three native results report automatic exit and `saveWritingApiObserved = false`; existing read-only load suppression and descriptor restoration events were observed. No raw save was copied, parsed, renamed, replaced, edited or written. `KMG_AUTOMATION_BASELINE` was not loaded or modified. No screenshots, OCR, arbitrary input, merchant navigation or human approval was used as proof.

Exact live transaction:

- Lease: `C:\Dev\KingmakerGunslingerLab\compatibility-state\runtime-20261005T222710Z-2130ff6316a94380a79a284e6bba79f6\runtime-lease.json`; final status **Completed**.
- Deployment receipt: `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20261005T2227331067361Z\deployment.json`.
- Snapshot/backup taken only after lease acquisition: `C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20261005T2227272533031Z`.
- All **254 original files**, lengths, SHA-256 values and directory names compare exactly after restoration, including current settings and pre-existing cache files.
- Original/restored live DLL SHA-256: `00ef67570d30e4ff9c54a2ad2027393660436d5724bcee2bb99e4c5718e966fd`.
- Full before/after inventories and status live only in ignored `artifacts/mission/candidate-4-*` evidence. No permanent installation remains.

## Meaningful failures and repairs

- One focused static check initially mistook the factory method declaration for retained static graph state; narrowed to a field declaration, then PASS.
- New source's initial exact-reference compile failures exposed private command/flag fields, the precise `AddAreaEffect` namespace, native `Buff.Active`, descriptor-wrapper casting and the four-argument modifier overload. Exact contracts were corrected; no validator was weakened.
- Candidate 1: Fiery native delayed stop failed immediate deactivation; set the existing native `DeactivateImmediately` flag. Native attached-area lookup could not resolve an unregistered GUID-only `BlueprintReference`; added the owned request-local area lookup lease using repository precedent. Original scene/actors were cleaned and the live tree restored exactly.
- Candidate 2: native `MechanicsContext` rejects a null blueprint. Replaced the invalid fixture with an exact descriptor-only native source, leaving ambiguous correlation permissive.
- Candidate 3: 46/47 runtime assertions passed; `UnitCondition.Unconscious` did not change native `LifeState`. Used the exact qualified native life-state fixture, restored in `finally`, and added the dead-holder control.
- Moving own test entries to the end exposed the baseline catalog's omitted final comma; the delimiter was corrected without reordering any baseline entry.
- Elevated preflight/runtime execution was rejected by the guard. A hidden launcher through the exact current-user desktop shell dispatch was verified unelevated; no safety guard or Windows configuration changed.
- Every actual failed game attempt exited automatically, restored the exact original live tree and completed its lease. No unrelated Expanded Summoning gate failure was found or repaired.

## Changed files

- `scripts/RuntimeAutomation.Common.ps1`
- `scripts/Test-RuntimeScenarioPreflight.ps1`
- `scripts/test-domain.ps1`
- `src/KingmakerGunslinger/ElementalRaces/StoicDignityMechanics.cs`
- `src/KingmakerGunslinger/ElementalRaces/StoicDignityPolicy.cs`
- `src/KingmakerGunslinger/ElementalRaces/UnpublishedRaceTraitFoundationFactory.cs`
- `src/KingmakerGunslinger/KingmakerGunslinger.csproj`
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs`
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.UnpublishedRaceTraitFoundations.cs`
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs`
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs`
- `tests/KingmakerGunslinger.DomainTests/FieryGlareFoundationTests.cs`
- `tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj`
- `tests/KingmakerGunslinger.DomainTests/Program.cs`
- `tests/KingmakerGunslinger.DomainTests/StoicDignityFoundationTests.cs`
- `tools/validate_expanded_summoning_phase2a141.py`
- `validation/static-validation.json`
- `CODEX-ELEMENTAL-RACE-TRAIT-FOUNDATIONS-HANDOFF-2026-10-05.md`

## Clean committed artifact closure

- Qualified source commit and verified local/remote SHA at completion of mechanics qualification: `2c5bbcaf428b3e0ac017ba5eb5a7969674bcf56e`.
- Source tree at build and all three final launches: **clean**; no `AllowDirtyGit` switch.
- First commit: `feat(traits): qualify unpublished elemental mechanics foundations`, `2c5bbcaf428b3e0ac017ba5eb5a7969674bcf56e`; pushed through the required wrapper before clean rebuild.
- Final checkpoint: a direct documentation-only child, `docs: close elemental trait foundation qualification`. Its exact final local/remote SHA is recorded after guarded push in the completion message and ignored `artifacts/mission/final-git-state.json`. `git rev-parse HEAD` and the exact remote branch SHA must agree; the handoff cannot embed its own commit hash.
- All packaged inputs are identical to the qualified source commit. ZIP inspection confirms this handoff is not packaged; no rebuild merely for documentation identity, no claim that the final documentation tip is the DLL's embedded commit.

| Clean artifact identity | Value |
|---|---|
| Version | `0.0.141` — unchanged |
| Embedded/source commit | `2c5bbcaf428b3e0ac017ba5eb5a7969674bcf56e` |
| Source fingerprint | `c8dff63bb1a9148de4f0eee8e14b2b0cf536089b5abbd37551fda3754cc42f91` |
| DLL SHA-256 | `508025890be9b9cf682f1fc70377dd11ab1f9b87d2a1d6b132cdca1bead4d046` |
| DLL MVID | `922d085c-1755-4595-b16e-da0d43a9343e` |
| ZIP SHA-256 | `ab0edb84d065a003415cf802d5c1d086ea244320cdecbf8a5c4676e5e475b993` |
| Package location | `artifacts/local-runtime/0.0.141/KingmakerGunslinger-0.0.141-local-runtime.zip` |

Clean source `Invoke-KmgGate.ps1 -Level Sprint`: **2093/2093**, repository/static/icon/manifest checks, exact-reference clean Release build, deterministic package and strict standalone validation **PASS** (98.5 seconds). Focused 53/53, changed PowerShell parser checks and changed Python syntax pass. The 484 runtime-request/preflight checks passed on the unchanged request/PowerShell contracts. Final documentation validation and staged audits run again before the final checkpoint.

| Final scenario | Run/evidence identifier | Fresh game PID | Assertions |
|---|---|---:|---|
| observe-unpublished-race-trait-foundations | `20261005T2242315307611Z-ca9af45095bf44359b7f5a3a39347957` | 20856 | 49/49 PASS |
| observe-unpublished-race-trait-foundations | `20261005T2244356552048Z-c6c5cb7779bc4d02b004134b3b937c59` | 38432 | 49/49 PASS |
| working-save-smoke | `20261005T2245417200280Z-9f2961040df44d39a1bacf1cebb18140` | 33156 | 11/11 PASS |

Final native runs use the exact same immutable clean artifact, Steam App 640820, closed requests, `KMG_AUTOMATION_WORKING`, no arbitrary input, automatic exit and no forced termination. All results have `saveWritingApiObserved = false`; read-only native header/counter suppression and descriptor restoration are recorded. No save write or raw-save operation occurred. Curated assertions are in each runtime-evidence directory's `unpublished-race-trait-foundations.json` / `runtime-result.json`; no machine-local or proprietary artifacts are committed.

- Final lease: `C:\Dev\KingmakerGunslingerLab\compatibility-state\runtime-20261005T224151Z-6eb549fa1d654427a548f808c073710c\runtime-lease.json` — **Completed**.
- Final deployment receipt: `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20261005T2242213583917Z\deployment.json`.
- Exact current-install backup, acquired after lease: `C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20261005T2242146583778Z`.
- **254/254 original files** and every directory restored, with identical filename, length and SHA-256 inventories; original settings and pre-existing cache files preserved. Ignored `clean-2c5bbcaf4-live-before.json` equals `clean-2c5bbcaf4-live-after.json`.
- Final helper status: **PASS**; restoration **true**; no active Kingmaker process or compatibility lock after closure. Final branch clean/push and process/lease/source/ancestry audits are recorded in the final receipt.

Neither trait is player-acquirable. No concern was converted into publication, art or unrelated engineering. Stoic Dignity is a **qualified unpublished mechanics foundation**, not `BLOCKED-WITH-EVIDENCE`. Fiery Glare remains explicitly **ADAPTED**.

## Finite future publication work

1. Original icon authoring for the approved Fiery Glare and Stoic Dignity families.
2. Owner visual approval of that art.
3. Deliberate final visible blueprint graph and stable project-owned identities, using these qualified builders.
4. Favored Class `racial_traits` publication.
5. Combined player-facing runtime and owner manual acceptance.

No Whiteout, Earthsense, Lunge, Aerial Observer, vendor/firearm work, inventory migration, another trait, branch integration or release work began. No Expanded Summoning branch, foreign worktree, PR, source family, asset, test definition, runtime scenario or save fixture was developed, qualified or repaired. Immutable Phase 2A baseline material naturally remains in the package; only the explicitly permitted shared current test-count pins include this mission's added tests.
