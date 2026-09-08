# Elemental Races character-creation stabilization acceptance handoff

## Candidate and acceptance status

The disposable candidate is `0.0.117-elemental-char-gen-stabilization` (numeric
0.0.117), built from `132f0650e997579c589d19874a022aa5ee2213f2` on
`codex/elemental-races-expansion`. Mission start was
`c7df5de21ea2860b99a25777b021d7f44a5858fa`; its last production-code parent was
`efc9d54ec29dbdd84ffe328183139f517c2f3350`.

Automated readiness and backup-first installation **PASS** for this exact artifact.
The final matrix has 13,847 passing assertions across 28 fresh
guarded Steam processes (20 final profiles, three pinned migration processes
and five visible-trait persistence processes). No owner UI PASS is claimed.

Owner full-screen UI acceptance is **NOT-RUN**. Native creator/controller tests
are structured functional evidence, and include completed real disposable
characters, but do not relabel the owner's reported failure or substitute for
the owner's ordinary full-screen entry point. The owner-authorized released master was merged into this feature branch.
No feature-to-master merge, tag or public release is performed by this mission. The two deferred mechanics and favored-class bonuses remain out of scope.

## Owner-authorized upstream integration

The owner requested integrating their new public release during final qualification.
Master `58d9511082af30f1a4ec88c1238ae7ae2b3651c2` was merged as
`a3d7288b5930af1ba476e6ccef1d7abe9b680e4a`. Roadwarden, Dead Reckoning,
their native Skeletal Salesman stock integration and the released Protection
wording are retained. The complete GUID ledgers from both parents remain exact.
The final combined artifact was rebuilt and qualified again; earlier evidence
retains its original artifact attribution.

## Diagnosis and exact changes

Kingmaker 2.1.7b's actual CharacterBuildController dispatcher routes Group None
(0), and Racial (11) by itself, to generic Abilities. The original four KMG
heritages and ten replacement-slot selections used None. Native Aasimar and
installed Races Unleashed use AasimarHeritage (42), which creates the native
Determinator/Heritage consumer before allocation/skills. Native Tiefling uses
its corresponding heritage group (45). The repair uses the inspected 42
contract, confirmed in real first-level native phase state.

- Heritage factory: Group 42, Group2 None, Groups Racial; three fixed choices,
  stable GUIDs, existing ordered owned Features/AllFeatures arrays.
- Alternate-trait factory/policy: Group 42, Group2 None, Groups 42, obligatory
  non-class selections, legal unconditional retain-base routes; preserved
  replacement-slot reconciler and trait/provider identities.
- Treacherous Earth and Nereid Fascination: registered hidden markers/providers,
  omitted from every player selection array, excluded before overlap/provider
  resolution. Runtime assertions reject these exact published marker GUIDs and
  any published alternate without its specific implemented mechanic.
- Optional Helpful transaction/resolver/coordinator: retain foreign Features
  exactly and append Helpful once to authoritative AllFeatures. The inspected
  empty-Features contract is required; GUID-aware ordered additions, rollback,
  idempotence and repeated live callbacks preserve foreign entries/references.
- Visual registry/retention: retain exact owned proxies/inner assets and native
  donor/palette assets across creator preview unloading; no global unload rewrite.
- Native respec bridge: preserve owned daily resource amounts and blood healing
  expenditure across replacement creation, preview, native rest and Player
  copyback; no changes to foreign resources, maxima, choices, stats or facts.
- Breeze boundary: recognize four exact registered mundane Special feat attacks
  (Vital Strike ranks and CotW Pinpoint), preserving ordinary +2 ranged defense;
  magical, ray, unknown and nonphysical attacks remain excluded.
- Crystalline policy: fixed reviewed 93-identity semantic ray catalog with native
  delivery/parent/geometry guards; no optional-mod compile-time dependency.
- Guarded scenarios/tests: read-only routing snapshots; real native creators,
  back-navigation and Player respec; native turn costs; all 19 visible traits'
  fresh-process persistence and physical lifecycle matrix.

Primary changed source locations (the branch diff and journal retain the full change history):

| Change | Source |
| --- | --- |
| Heritage phase | [ElementalHeritageBlueprintFactory.cs](../src/KingmakerGunslinger/ElementalRaces/ElementalHeritageBlueprintFactory.cs) |
| Racial slots and deferred publication | [ElementalAlternateTraitBlueprintFactory.cs](../src/KingmakerGunslinger/ElementalRaces/ElementalAlternateTraitBlueprintFactory.cs) |
| Slot compatibility and deferred normalization | [ElementalAlternateTraitPolicy.cs](../src/KingmakerGunslinger/ElementalRaces/ElementalAlternateTraitPolicy.cs) |
| Foreign Combat selector transaction | [HelpfulPublicationTransaction.cs](../src/KingmakerGunslinger/AidAnotherCompatibility/HelpfulPublicationTransaction.cs) |
| Foreign object/contract resolution | [FavoredClassTraitResolver.cs](../src/KingmakerGunslinger/AidAnotherCompatibility/FavoredClassTraitResolver.cs) |
| Repeated optional lifecycle reconciliation | [AidAnotherOptionalExtensionCoordinator.cs](../src/KingmakerGunslinger/AidAnotherCompatibility/AidAnotherOptionalExtensionCoordinator.cs) |
| Native respec resource ownership | [ElementalNativeRespecResourceRuntime.cs](../src/KingmakerGunslinger/ElementalRaces/ElementalNativeRespecResourceRuntime.cs) |
| Spent-resource policy | [ElementalRespecResourcePolicy.cs](../src/KingmakerGunslinger/ElementalRaces/ElementalRespecResourcePolicy.cs) |
| Blood expenditure persistence | [ElementalBloodRuntime.cs](../src/KingmakerGunslinger/ElementalRaces/ElementalBloodRuntime.cs) |
| Creator visual asset retention | [ElementalCharGenVisualRetentionPatch.cs](../src/KingmakerGunslinger/ElementalRaces/Visuals/ElementalCharGenVisualRetentionPatch.cs) |
| Breeze attack-source boundary | [ElementalBreezeKissedMechanics.cs](../src/KingmakerGunslinger/ElementalRaces/ElementalBreezeKissedMechanics.cs) |
| Semantic ray catalog | [ElementalCrystallineFormPolicy.cs](../src/KingmakerGunslinger/ElementalRaces/ElementalCrystallineFormPolicy.cs) |
| Real native phase observation | [ElementalCharacterCreationRoutingObserver.cs](../src/KingmakerGunslinger/RuntimeTesting/ElementalCharacterCreationRoutingObserver.cs) |
| Native back-navigation and completion | [ElementalCharacterCreationRegression.cs](../src/KingmakerGunslinger/RuntimeTesting/ElementalCharacterCreationRegression.cs) |
| Real Player respec regression | [ElementalCharacterCreationNativeRespec.cs](../src/KingmakerGunslinger/RuntimeTesting/ElementalCharacterCreationNativeRespec.cs) |
| Native critical-effect test isolation | [ElementalBreezeKissedScenario.cs](../src/KingmakerGunslinger/RuntimeTesting/ElementalBreezeKissedScenario.cs) |

The original empty ordinary Trait screen is **not conclusively attributed**.
Profile B with production KMG disabled works, but the original KMG-enabled
General-route controls also exposed legal global choices. Inspection proves
AllFeatures is authoritative and the old Features mutation violates the foreign
contract; it does not prove that mutation caused the owner's empty screen.
Current native first-level tests demonstrate two populated/completable global
Trait roots with Bodyguard OFF and ON. The original UI trigger remains an owner
acceptance check; unrelated ZFavoredClass custom-data exceptions are unchanged.

| Foreign Combat Trait surface | Original 117 | Stabilized Bodyguard ON | Bodyguard OFF |
| --- | --- | --- | --- |
| Features | 0 -> 1, replaces empty reference | 0 -> 0, exact empty reference retained | 0, unchanged |
| AllFeatures | 14 -> 15 | 14 original ordered exact objects + Helpful once | 14 originals, Helpful absent |
| Top-level global Trait roots | two roots, each eight categories | both retained and completable | both retained and completable |

Helpful remains GUID `e4b29a7c8d5f4c1796ab03e1f72d8456`. The separate existing
Eastern Heirloom Equipment addition remains additive (49 -> 50 in applicable
profiles) and preserves empty Features. The earlier exact314-artifact observer
`20260907T1936273690252Z` captures the same Combat object `ref-46` throughout:
Features stays the empty `ref-3141`; AllFeatures changes once from `ref-3142`
(14) to `ref-10185` (15), then retains that exact reference at runtime readiness.
All 14 original choice records, including object identities and metadata, are
identical and ordered. Those choices are Anatomist, Armor Expert, Berserker of
the Society, Blade of the Society, Defender of the Society, Deft Dodger, Dirty
Fighter, Reactionary, Resilient, Slippery, Dragon Armor, Fencer, Honored Fist of
the Society and Threatening Defender. Reference labels apply only within this
process; the GUIDs carry cross-process identity. Elemental feat factory/publication
source is byte-identical to mission start.

## Visible inventory

| Race | Heritage choices | Implemented alternate racial traits |
| --- | --- | --- |
| Ifrit | General, Lavasoul, Sunsoul | Wildfire Heart; Brazen Flame; Fire in the Blood; Efreeti Magic; Forge-Hardened; Fire Insight |
| Oread | General, Gemsoul, Ironsoul | Crystalline Form; Earth Insight; Granite Skin; Stone in the Blood |
| Sylph | General, Smokesoul, Stormsoul | Air Insight; Breeze-Kissed; Like the Wind; Secretive; Storm in the Blood; Thunderous Resilience; Whispering Wind |
| Undine | General, Mistsoul, Rimesoul | Acid Breath; Ooze Breath |

Ten existing replacement-slot selections retain their retain-base choices.
Oread's SLA slot currently offers retain base only. Multi-slot exclusions leave
that completion route available. All four races, twelve heritage markers and
all previous provider, affinity, SLA, resource, visual and feat GUIDs remain
stable. The original ordered 1,867-entry manifest identity/type/status inventory
matches mission start. Four descriptive notes, for the two deferred markers and
their providers, explicitly record their unpublished/inert status; identity,
type and registration status are unchanged. There are 21 registered traits, 19
published choices.

Deferred marker identities remain resolvable and are never selectable:
Treacherous Earth `e117e1e0a17a4acec001000000000031`;
Nereid Fascination `e117e1e0a17a4acec001000000000040`.
Their providers/other reserved identities are retained; deferred marker-only
states reconcile conservatively without consuming an SLA replacement slot.

## Qualification evidence

Repository validation, all **1,458** domain/reflection tests, clean exact-reference
Release compilation, deterministic package creation and strict **135-entry**
validation pass. The manifest has **1,869 identities: 1,867 active, two reserved**.

Commands include:

- `python tools/validate_repository.py`
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1`
  (includes clean full domain suite, Release, deterministic/strict package gates).
- Guarded `scripts/Invoke-KingmakerRuntimeTest.ps1`, Steam App 640820, through
  exact reversible profile transactions and the named `KMG_AUTOMATION_WORKING`
  save. The immutable local matrix command and per-process run IDs are below.

| Final profile / scenario | Guarded run | Assertions | Creator evidence | Restoration |
| --- | --- | ---: | --- | --- |
| mechanics-a | `20260907T2304369475236Z-observe-elemental-alternate-trait-framework` | 6325 | mechanical / compatibility / smoke | exact |
| mechanics-f | `20260907T2307155108037Z-observe-elemental-alternate-trait-framework` | 6517 | mechanical / compatibility / smoke | exact |
| turn-costs-a | `20260907T2310214408383Z-disposable-elemental-trait-turn-costs` | 103 | mechanical / compatibility / smoke | exact |
| turn-costs-f | `20260907T2312287709697Z-disposable-elemental-trait-turn-costs` | 103 | mechanical / compatibility / smoke | exact |
| firearms-f | `20260907T2315194825862Z-disposable-midgame-firearms` | 92 | mechanical / compatibility / smoke | exact |
| creator-a | `20260907T2348135161663Z-disposable-elemental-character-creation-baseline` | 11 | characters: 4, commits: 0 | exact |
| creator-g | `20260907T2350594465597Z-disposable-elemental-character-creation-baseline` | 11 | characters: 4, commits: 0 | exact |
| creator-h | `20260907T2354359368905Z-disposable-elemental-character-creation-baseline` | 11 | characters: 4, commits: 0 | exact |
| control-b | `20260907T2357227910606Z-disposable-global-traits-kmg-disabled-control` | 8 | characters: 1, commits: 0 | exact |
| creator-c | `20260908T0000055911468Z-disposable-elemental-character-creation-baseline` | 11 | characters: 4, commits: 0 | exact |
| creator-d | `20260908T0004438265554Z-disposable-elemental-character-creation-baseline` | 11 | characters: 4, commits: 0 | exact |
| creator-e | `20260908T0009222820644Z-disposable-elemental-character-creation-baseline` | 11 | characters: 4, commits: 0 | exact |
| creator-f-ifrit | `20260908T0014115124448Z-working-save-elemental-character-creation-regression` | 12 | characters: 3, commits: 3 | exact |
| creator-f-oread | `20260908T0020317226892Z-working-save-elemental-character-creation-regression` | 12 | characters: 3, commits: 3 | exact |
| creator-f-sylph | `20260908T0026382486968Z-working-save-elemental-character-creation-regression` | 12 | characters: 3, commits: 3 | exact |
| creator-f-undine | `20260908T0032393745170Z-working-save-elemental-character-creation-regression` | 12 | characters: 3, commits: 3 | exact |
| respec-f-sylph | `20260908T0038274374916Z-working-save-elemental-native-respec` | 12 | characters: 8, commits: 8 | exact |
| respec-f-oread | `20260908T0050248888147Z-working-save-elemental-native-respec` | 12 | characters: 8, commits: 8 | exact |
| elemental-off-f | `20260908T0102208611442Z-elemental-races-races-unleashed-compatibility` | 13 | mechanical / compatibility / smoke | exact |
| smoke-f | `20260908T0105054634241Z-working-save-smoke` | 11 | mechanical / compatibility / smoke | exact |

| Final migration / persistence process | Assertions | Result |
| --- | ---: | --- |
| `20260907T2318128194308Z-elemental-race-persistence-prepare` | 11 | PASS |
| `20260907T2321060370196Z-elemental-race-legacy-migration` | 10 | PASS |
| `20260907T2323466381836Z-elemental-race-persistence-verify-absent` | 8 | PASS |
| `20260907T2327205130928Z-elemental-race-persistence-verify-absent` | 8 | PASS |
| `20260907T2329536201419Z-elemental-race-persistence-prepare` | 66 | PASS |
| `20260907T2333420072446Z-elemental-race-module-disabled-persistence` | 173 | PASS |
| `20260907T2337019456951Z-elemental-race-module-restored-persistence` | 253 | PASS |
| `20260907T2345095531645Z-elemental-race-persistence-verify-absent` | 8 | PASS |

The independent native-state audit covers 53 character records and 28 native
commits. Every observed final review has an enabled completion button and zero
unresolved selections. The reduced profiles have no commits; F contains the
actual creator and Player-respec completions. All exact phase consumers,
three-choice heritage lists and two completed global roots are verified.

Final matrix command (machine-local ignored wrapper, using committed guards):

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File artifacts/qualification/0.0.117/character-creation-stabilization/Invoke-MergedFinalQualificationMatrix.ps1 -RunPrefix merged-final-01 -IdentityName final-acceptance-candidate-02
```

Pinned 114 producer package SHA-256 is
`b5c88113624879cc3c8a718d37ff39acb03f839ff41978f49f7716f9fefb6694`,
from `6874dc15a27ded132456dbdd480f47c794543a05`, without an overlay. Eight
legacy characters lacking heritage markers load as General with spent state
preserved; cleanup passes in a fresh final-artifact process. Release A and B
persistence remains covered in the five-process final matrix (11 feats / 25
feat identities, 24 race/sex/heritage actors).

Earlier focused qualifications remain preserved in the mission journal:
12 real native creator commits / 32 final reviews / 184 exact racial graphs
with all three heritages per race and point-buy/Dice Roller back-navigation;
32 commits / 28 real Player respec callbacks / 240 exact graphs for daily
resources; 24 blood-resource commits / 21 callbacks / 192 blood observations;
103 native turn assertions in each of A and F, covering all Sylph/Undine
heritages, actual swift/standard spending, same-turn rejection and cancellation;
93 semantic ray identities with 294 full-stack paired AC controls; 24 paired
Breeze mundane/magical native-command comparisons. Failed precursor runs are
retained as FAIL and do not count toward readiness.

All 19 traits are covered by the 24-actor save matrix, including the nine
previously unqualified visible traits. The matrix exercises OFF loading,
level-up before rest, rest/re-spend, ON restoration, respec and exact cleanup.
Physical coverage includes seven ordered states per actor: death/resurrection,
polymorph/return, equipment and doll rebuild, with exact fact/resource/provider
state and 72 returned-state mechanic checks. Native removal of transient buffs
is recorded and not reversed by the fixture. Fresh-process absence is required.
Actual Player respec and fixed-shell persistence respec are reported separately.
Reduced-stack A/G/H/C/D/E creator checks reach the native final review without
committing a character. The actual completed characters and Player respec
callbacks come from the full-stack F runs; these evidence scopes are distinct.

All final profiles retain exact settings hashes and mod manifests, and restore
their original trees and UMM bytes/timestamps. In per-phase OFF/ON transactions,
PowerShell and KMG serialization hashes differ while the exact 12 setting values
match; both forms and final byte-exact restoration are verified. The baseline
and personal saves are protected. Only the named disposable working save receives
authorized persistence/cleanup writes; native fresh-load absence, not archive
byte equality, proves functional cleanup.

## Final harness preflight correction

The final preflight initially failed its fixed inventory list and a directory
metadata comparison. The two live scenario catalogs already agreed: 222 entries;
the test's 218-entry list omitted the four newly released midgame firearm
scenarios. The directory comparison reported only three old directory timestamps,
with no file entry differences. The test now refreshes filesystem metadata before
both snapshots, retaining all path, length and timestamp comparisons, and adds
four explicit firearm scenario safety assertions.

The corrected test passes **259 checks**, including unsupported-request rejection,
no build/staging, no backup/evidence creation, and no Steam/game process launch.
It was qualified in an isolated copy against the unchanged candidate before
installation; the identical test correction accompanies this handoff. The
original failed run remains recorded. The canonical command subsequently passed
all 259 checks from the final working tree. Repository validation and a fresh
Release rebuild/run of the full domain/reflection suite also pass: 1,458 tests,
zero failures. The game code and installed DLL are unchanged by this test
correction. Run the canonical test with:

`powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-RuntimeScenarioPreflight.ps1`

## Compatibility and warnings

Profile definitions and optional DLL/settings hashes are retained in the journal
and local startup identity record. A=KMG; G=KMG+CotW; H=KMG+Races Unleashed;
B=CotW+ZFavoredClass with production KMG disabled; C=CotW+ZFavoredClass+KMG,
Bodyguard OFF; D=same, Bodyguard ON; E=KMG+CotW+ZFavoredClass+Races Unleashed;
F=the owner's complete eleven-mod stack. ZFavoredClass requires CotW, so its
requested combinations are tested with that dependency, not reported as an
unsupported standalone ZFavoredClass launch. F includes Bag of Tricks 1.16.4,
Dice Roller 0.1.2 and Tweak or Treat 1.1.0 with the exact original settings.

An earlier exact314-artifact matrix stopped after Ifrit's successful native
run because its external auditor incorrectly required a Dice-specific flag on
point-buy reviews. The source writes that field only for rolled routes; the
request explicitly selected point-buy. Corrected auditing verified the unchanged
Ifrit evidence (three commits, eight reviews, 50 graphs). That outer audit
failure remains recorded. Matrix06 then completed the other three races, two
Player respec routes and module OFF, before its mechanics failure below.
These earlier matrices are historical, not the final merged-artifact readiness
ledger. The new merged matrix repeats all required profiles on one exact
combined artifact, with mechanics and turn costs first.

The failed framework run `20260907T2159441652088Z` and instrumented reproduction
`20260907T2243476180515Z` remain FAIL. Structured before/after state proves the
native Longbow of Cold Moon (`65d29ca8c81c124418417bff73f8eaae`) inflicted its
critical-hit Paralysis buff (`af1e2d232ebbb334aaf25e2a46a92591`), changing
CanAct from true to false and removing Dexterity from the next AC calculation.
The next Calm command therefore could not act. This was contamination between
native test controls; Breeze gameplay mechanics were unchanged. The fixture
now requires each finite, correctly sourced native critical condition to remain
until its recorded EndTime and expire through Buffs.Tick, then verifies exact
original actor state and clock before the next comparison. A seeded native
critical witness is required for every Sylph heritage; no hit/save result,
condition, duration or registered weapon is overridden. All later Calm commands
must start from an active, unconditioned native state. Full A/F framework checks
and the final combined artifact requalify this correction.

The first imported firearm test passed 91 functional assertions but had KMG
logging errors because its main-menu fixture lacked BattleLogView. Its scoped
final-UI-sink capture now verifies every native message is delivered without
new publication faults; gameplay logging is unchanged. The qualified merged
run `20260907T2231077396456Z` passes 92 assertions with 12/12 messages and zero
KMG errors. The final exact artifact repeats that check.

Two final-matrix launches failed before the guarded request was accepted because
Unity could not load the unchanged signed native steam_api64.dll (error 126).
Neither touched saves; both restored exactly and remain FAIL. A separate native
Steam readiness run and the subsequent five fresh-process persistence runs pass.
The library, Steam configuration and launch contract were not modified.

An attempted A saved-world run then failed in native Player.PostLoad at the
main-character UniqueId lookup, before creator initialization. The cause of that
reduced-stack save incompatibility is unresolved; the same native failure was
already recorded in early run A05 (20260907T0444519014086Z), with a successful
full-stack working-save control immediately afterward. It is not called a KMG
routing defect or a successful load. No save contents were altered. The reduced-stack
creator tests use the registered native disposable first-level review fixture
with no saved-world load. FullF creation, migration and persistence still require
actual working-save load and native completion.

| Final profile | KMG ERROR | Known ZFavoredClass custom JSON exceptions | Native exceptions |
| --- | ---: | ---: | ---: |
| mechanics-a | 0 | 0 | 19 |
| mechanics-f | 0 | 4 | 19 |
| turn-costs-a | 0 | 0 | 1 |
| turn-costs-f | 0 | 4 | 1 |
| firearms-f | 0 | 4 | 1 |
| creator-a | 0 | 0 | 1 |
| creator-g | 0 | 0 | 1 |
| creator-h | 0 | 0 | 1 |
| control-b | 0 | 4 | 1 |
| creator-c | 0 | 4 | 1 |
| creator-d | 0 | 4 | 1 |
| creator-e | 0 | 4 | 1 |
| creator-f-ifrit | 0 | 4 | 1 |
| creator-f-oread | 0 | 4 | 1 |
| creator-f-sylph | 0 | 4 | 1 |
| creator-f-undine | 0 | 4 | 1 |
| respec-f-sylph | 0 | 4 | 1 |
| respec-f-oread | 0 | 4 | 1 |
| elemental-off-f | 0 | 4 | 1 |
| smoke-f | 0 | 4 | 1 |
| migration: `20260907T2318128194308Z-elemental-race-persistence-prepare` | 0 | 4 | 1 |
| migration: `20260907T2321060370196Z-elemental-race-legacy-migration` | 0 | 4 | 1 |
| migration: `20260907T2323466381836Z-elemental-race-persistence-verify-absent` | 0 | 4 | 1 |
| persistence: `20260907T2327205130928Z-elemental-race-persistence-verify-absent` | 0 | 4 | 1 |
| persistence: `20260907T2329536201419Z-elemental-race-persistence-prepare` | 0 | 4 | 2 |
| persistence: `20260907T2333420072446Z-elemental-race-module-disabled-persistence` | 0 | 4 | 1 |
| persistence: `20260907T2337019456951Z-elemental-race-module-restored-persistence` | 0 | 4 | 2 |
| persistence: `20260907T2345095531645Z-elemental-race-persistence-verify-absent` | 0 | 4 | 1 |

Historical native startup BugReportCanvas, Pixel8bits shader diagnostics, and
native game-exit ObstaclesHelper teardown are recorded separately from KMG
failures. The teardown's `0x51` and `0x6d` offsets dereference the first/second
argument's missing EntityData in the same native method; the `0x6d` variant was
first observed in this mission's physical fixture and is explicitly disclosed.
No exception handler, third-party implementation or native teardown is patched
or suppressed. ZFavoredClass's four missing custom JSON entries
(`bonus_charmed_life.json`, `bonus_panache.json`, `arcane_archer.json`,
`deadeye_devotee.json`) persist in the KMG-disabled control; no general
ZFavoredClass repair is attempted.

Native tests use real engine creator phases and completed disposable characters.
Owner full-screen acceptance remains separate. The observed native ordering is
Race -> ClassInChargen -> Determinator (Choose heritage) -> Skills (allocation
and skills) -> Abilities (feats and ordinary Trait selections) -> Character
-> Total. The native enum's broad Abilities phase contains the later ordinary
Trait selectors; elemental heritages and alternate racial selections have only
the Determinator consumer. The installed catalog contains
no demonstrated nonmagical ordinary energy ranged weapon; Breeze's nonphysical
policy is checked structurally/purely and against the available native magical
energy weapon. Unknown weapon/ability metadata fails closed. Unclassified opaque
spell wrappers, including the installed Prismatic Surge wrapper, are not treated
as semantic rays merely for using shared transport. These limits do not claim coverage of arbitrary future optional mods.

## Package and installation

| Identity | Exact value |
| --- | --- |
| Artifact source commit | `132f0650e997579c589d19874a022aa5ee2213f2` |
| Source fingerprint | `86e70e45065704e67d0bc161c336c99d902d1cae48a2b40c327436ed6db29d8f` |
| Informational version | `0.0.117-elemental-char-gen-stabilization` |
| ZIP SHA-256 | `7b930f3f084796f81e9d7cb0a6babea248404c87eb72381509019c8843c9ec14` |
| DLL SHA-256 | `fcfe67df81077f9add11ad3770168161c65f3906805fe1c483ae096130892fef` |
| DLL MVID | `35dd73ec-4168-457b-8c03-5135e0e9e1ee` |
| Manifest / package entries | 1,869 / 135 |

The standalone archive is
`artifacts/packages/KingmakerGunslinger-0.0.117-elemental-char-gen-stabilization.zip`.
The guarded installation uses the byte-identical
`artifacts/local-runtime/0.0.117/KingmakerGunslinger-0.0.117-local-runtime.zip`
and its immutable `.build-local.json` sidecar. An immutable copy is retained in
the ignored stabilization qualification directory. Generated artifacts are not
committed. Final documentation and the preflight test correction follow the artifact source commit; no
unqualified source change is folded into the installed DLL.

Installed mod:
`C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger`.

Recoverable previous installation:
`C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260908T0120399445825Z`.

Deployment record:
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260908T0120432427216Z\deployment.json`.
All 135 package files independently match the ZIP byte for byte; the only extra
file is the preserved FeatureModules.json. Its bytes/timestamp and the original
UMM bytes/timestamp remain exact. All unrelated mod folders and protected saves
remain exact, and no game is left running. The old DLL in the backup retains
SHA-256 `b3839a63fb83a5894169fa7ea2cbc1ef6e15f01229081b51d0f74b0d88984f05`.
The owner can recover the previous installation from that backup.

## Owner full-screen acceptance checklist

1. Restart with the complete original eleven-mod stack and verify the final
   candidate informational identity in KMG's log. Start character creation through
   your ordinary entry point (the entry point used for the original failure is
   especially useful).
2. For each Ifrit, Oread, Sylph and Undine, select a class and confirm the racial
   Heritage section appears before ability allocation and skills. Each heritage
   list must show exactly General and its two named alternatives: Lavasoul/Sunsoul,
   Gemsoul/Ironsoul, Smokesoul/Stormsoul, or Mistsoul/Rimesoul, respectively.
3. Confirm alternate racial choices appear in the racial route. Every applicable
   slot has a legal retain-base choice and the implemented compatible choices.
   Treacherous Earth and Nereid Fascination must be absent. Suggested independent
   examples already used in native tests: Ifrit Wildfire Heart + Fire in the Blood
   + Efreeti Magic; Oread Granite Skin + Earth Insight; Sylph Thunderous Resilience
   + Breeze-Kissed + Whispering Wind; Undine Acid Breath (then Ooze Breath on a
   second choice pass). Retain any remaining required slots.
4. Go back, change heritage, and go forward. Check the displayed racial modifiers
   and descriptions change once and your point-buy/rolled baseline remains yours.
   Check a multi-slot choice and then retain base or another legal combination;
   every required step must remain completable.
5. Complete abilities, skills and feats. Elemental Strike and other legal existing
   elemental feats must remain available. Heritage/alternate-racial choices must
   not recur as generic Abilities or ordinary global Traits.
6. In the later global Trait phase, confirm both selections contain categories and
   real choices. Choose two legal traits (different categories make this easy).
   With Bodyguard enabled, Helpful should occur once in Combat. A separate restart
   with Bodyguard disabled should remove Helpful while keeping ordinary Traits
   functional; return the setting to the original state afterward.
7. Complete normal details and finish one disposable character per elemental race.
   Include at least one alternate heritage and an alternate racial-trait combination.
   Repeat one route using Dice Roller with your ordinary Bag of Tricks settings.
8. On a disposable character, spend a racial daily resource, save/reload, and respec
   heritage/trait choices. Confirm the spent use stays spent until ordinary rest,
   then returns once; confirm original and replacement facts/stats do not duplicate.

Report which exact race, class, heritage, trait combination and entry point failed
if any phase is empty or cannot advance. Screenshots can support the visual review;
they do not replace the structured mechanical or completed-character evidence.


## Exact starting environment and optional mods

Windows 10 build 19045; Kingmaker 2.1.7b; UMM 0.32.4. The original output log
was preserved before any launch (SHA-256
`948d59a90bc88669e53a1c9ca4bfe2b700527403a4ed80f55a752047d795d5e8`).
Game assembly SHA-256:
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`;
UMM SHA-256:
`1387468bc3af41c50fe51859a3bb7af4922891aa8f13a6187e7a348ceaabfd88`.

| Installed mod | Version | Original DLL SHA-256 |
| --- | --- | --- |
| BagOfTricks | 1.16.4 | `d03626594ece0f339aeef03ec7259684af7ddeb8b2edf41936f144848059a0e6` |
| CallOfTheWild | 1.14.4c-2.1 | `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915` |
| CheatMenu | 1.2.3 | `7d659eb092073ab9e059414f8bfbdfe46991cede56b394f58f7ea2bcc1ff0845` |
| CraftMagicItems | 2.1.0 | `4ae2da61470350b31beef162717a604c9ccd322f66193917944ea4a9596e392d` |
| KingmakerBuffPlanner | 0.0.16 | `4c7249ad7a953522ea755e8bb46c5b89b136b9479a7360440526f417fb171597` |
| KingmakerDiceRoller | 0.1.2 | `962d5968d5021db2868d39104b4cbfc3911fe1de00ebde85974a1a2b1b977acd` |
| KingmakerGunslinger | 0.0.117 | `b3839a63fb83a5894169fa7ea2cbc1ef6e15f01229081b51d0f74b0d88984f05` |
| KingmakerLastAzlantiPreserver | 0.1.0 | `89dc72c1b331a818a7e6112b5c1d6b5d8d51d8027d74e1bc3d3168fb653ada23` |
| RacesUnleashed | 1.0.11 | `6d18168cb90ffe60931addc8ee11e42b3ef647ef0e6d4b7ce8980d44659f4cb0` |
| TweakOrTreat | 1.1.0 | `a518324e15632aba46d6c467b156a31e9afd282e9827dee3e79ad14673852b92` |
| ZFavoredClass | 1.3.1 | `dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c` |

The original KMG row identifies the failed starting installation. The replacement
KMG identity is the exact stabilized artifact table above; optional DLLs are
unchanged. Per-profile full manifests, settings hashes, active selector snapshots
and exact restoration records remain in the ignored stabilization evidence
directory. No raw logs, generated ZIPs, saves or proprietary assemblies are
committed. All earlier journal evidence remains intact.
