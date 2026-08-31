# Gunslinger Outfit Kitbash Qualification

Status: not qualified. This document is a live gate ledger, not an acceptance
claim.

## Authority

- Intake baseline: `5949165e2a6407ca480d46cd86d8944e4152e2fb`
- Feature branch: `codex/gunslinger-class-outfit-kitbash`
- Intake version: `0.0.110`
- Runtime target: Steam-preserving Pathfinder: Kingmaker 2.1.7

## Gate ledger

| Gate | State | Required evidence |
|---|---|---|
| Installed API contracts | Pass (audit stage) | Exact installed 2.1.7b assembly identity and public/reflected member findings recorded |
| Native class/resource catalog | Pass | Guarded run `20260830T2012181937219Z`, candidate set `dd81603f...03357` |
| Serious candidate renders | Pass (Human stage) | Accepted outer/in-game PASS for 48 cases/96 directly inspected images |
| Best-three scoring | Pass (Human stage) | Magus 81, Rogue 75, Slayer 70; full finalist gates pending |
| Race/gender coverage | Pending | Dynamically discovered supported matrix |
| Color ramps | Partial | Native and alternate valid ramps rendered on Human M/F; systematic grid pending |
| Body/material integrity | Partial | Human M/F direct renders and structured load data pass; all races pending |
| Animation/weapon fit | Pending | Idle/walk/run/turn/fire/reload/melee evidence |
| Equipment overrides | Pending | Light/heavy armor, headgear/hair, cloak, backpack, inactive weapon |
| Preview/gameplay paths | Partial | Four-view preview-like and ordinary isometric Human evidence; finalist grid pending |
| Save/load/rebuild | Pending | Guarded structured evidence |
| Focused tests | Pass (collector checkpoint; repeat final) | Renderer guard/catalog/matrix, exact 600-second collector assertion, and 160 runtime preflight checks |
| Repository validation | Pass (collector checkpoint; repeat final) | Build-Local.ps1, 2026-08-30T21:54:14Z |
| Complete domain suite | Pass (collector checkpoint; repeat final) | 1365/1365, Release clean run |
| Clean Release build | Pass (collector checkpoint; repeat final) | Exact-reference Release construction |
| Installable package | Pass (collector checkpoint; repeat final) | Strict standalone/local validation, SHA-256 2f515302...76849 |
| Compatibility profiles | Pending | Exact applicable command/result |
| Guarded runtime smoke | Pass (candidate stage; repeat final) | Accepted run `20260830T2158516580621Z`, exact build fingerprint |
| Publication | Pending | Commit(s), helper output, identical local/remote SHAs |

## Guarded catalog evidence

Command:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario gunslinger-outfit-audit `
  -ExpectedVersion 0.0.110 `
  -ExitAfterCompletion:$true `
  -AllowDirtyGit `
  -Confirm:$false
```

Passing run: `runtime-evidence/20260830T2012181937219Z-gunslinger-outfit-audit`.

- result: PASS, all nine assertions;
- loaded mod: `0.0.110`;
- loaded game contract: supported version `2.1.7b`, exact
  `Assembly-CSharp.dll` SHA-256
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`,
  MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`;
- dynamically discovered race IDs: Aasimar, Dwarf, Elf, Gnome, Half-Elf,
  Half-Orc, Halfling, Human, and Tiefling, each inventoried for male and female;
- sources: 49 class, 163 item-linked, 361 bounded raw;
- inventory: 1,206 unique loaded equipment entities, 3,816 matrix rows,
  4,878 resolved links, zero unresolved links, zero inspection errors;
- state: no save-owned state, inventory, progression, or avatar mutation;
- deterministic candidate-set SHA-256:
  `dd81603f583444f335381d72cc69b73f1c036c4625e8227cb1e1f9db18603357`;
- ignored catalog SHA-256:
  `73af097a4dd21fe905d2f9b4388f2ef6a68503f4b6723040e1dd00d3e3e2e294`.

Two preceding FAIL runs are retained as diagnostic evidence. Both failed
closed, exited automatically, had zero game exceptions, and led to narrower
instrumentation. They are not acceptance evidence.

The passing launch pipeline also recorded repository validation PASS, complete
domain suite PASS (`1362/1362`), compilation PASS, and strict standalone package
validation PASS. These are audit-checkpoint checks and must be repeated after
production implementation.

## Guarded renderer source qualification

The first serious batch fixes six native class presentations and 32 exact
gender-specific IDs: Bard, Alchemist, Magus, cap/cape-free Ranger,
cap/cape-free Rogue, and cap-free Slayer. Investigation code excludes the
structurally unsafe caps/capes, uses disposable Human actors, captures native
and alternate valid ramps with no weapon/pistol/musket, restores exact avatar
state, and verifies cleanup.

At 2026-08-30T21:05:28Z, .\scripts\Build-Local.ps1 passed repository
validation, all 1365/1365 tests, exact-reference Release construction,
deterministic packaging, and strict validation. Standalone and local-runtime
packages have SHA-256
693c09684256fab77b4835b78eff12ab974c2bc460a63824f877768cd9c16ce8;
the staged DLL SHA-256 is
17bfe03b52e85cab627be425c680b1ccf6db88275ba4e253081065685304e377.
Runtime preflight passes 160 checks.

The initial runtime invocation was rejected before deployment or game launch
because the harness requires a clean Git state. It is not visual evidence.
Candidate-render gates remain pending until a clean published checkpoint is
run and the images are directly inspected.

The first clean published attempt is retained at
runtime-evidence/20260830T2109519221444Z-gunslinger-outfit-candidate-render.
It loaded commit 189ae46fa19552fa3b906740d9f30372c588f7f5 through
Steam, then failed closed at request acceptance with
scenario-timeouts-not-allowed. No hook, UI action, save action, render, or
score occurred. The missing in-mod working-save predicate entry was repaired
and covered by the focused guard test. After repair, all 1365 tests,
Build-Local, strict package validation, and the quiescent 160-check runtime
preflight pass. A new clean-commit runtime attempt remains required.

The next clean run is retained at
runtime-evidence/20260830T2119065677129Z-gunslinger-outfit-candidate-render.
It accepted the request, identified and loaded exactly
KMG_AUTOMATION_WORKING, reached a stable fingerprint with no save-writing API,
then failed closed before the first render because the otherwise valid male
pregen rig exposed no optional Progression equipment class. Cleanup passed,
automatic exit completed, and no candidate was scored. The renderer now uses
the reported class when available and the exact audited native Fighter class
otherwise, while recording original/donor entities and their intersection.
All 1365 tests, game-facing compilation, Build-Local, and strict package
validation pass after that repair. Installed candidate images remain pending.

The next run is retained at
`runtime-evidence/20260830T2130124467293Z-gunslinger-outfit-candidate-render`.
It loaded exact commit `9de7c4ef40483150ffba40782deb71714d2a0307`,
mod version `0.0.110`, and DLL SHA-256
`7fb96cd42ed986241fa63f79a52e01633da7c8b7bc18e1ed68d0a1562e4d5aac`
through Steam App ID 640820. Exact working-save correlation and a stable
fingerprint passed with no save-writing API.

The outer collector timed out at 21:32:28Z under the generic 120-second
deadline, approximately five seconds before the post-load fingerprint
completed. It left Kingmaker running and did not force terminate. The guarded
scenario then completed safely at 21:34:30Z, passed all ten assertions,
removed hooks, initiated automatic exit, and left no game process.

The terminal index records candidate set
`ef38c5c841510df7f03bbf68a8ca9e7fbef3f3403369022505449cb038d347be`,
six candidates, two Human gender fixtures, four cases per candidate/gender,
48 exact held states, 48 preview images, 48 isometric images, 24 palette
applications, and 12/12 restorations. It records
`saveApiCalled=false` and `productionBlueprintMutated=false`. All 96 images
were directly inspected from ignored local evidence. Eleven isometric images
were flagged low pixel density; paired four-view previews remained usable,
but this cannot close the final presentation gate.

Human-stage weighted scores are Magus complete 81/100, Rogue capless/capeless
75/100, and Slayer capless 70/100. Magus advances as the provisional finalist
because its native fitted torso, split waist tails, belts, bracers, and boots
best satisfy the swashbuckler/privateer brief. Bard, Alchemist, and Ranger
were below the shortlist for traveler-pack, apron/tank, and bedroll-heavy
wilderness identities respectively. No enumerated hard rejection is inferred
for the best three before the exhaustive matrix.

The collector now grants only this scenario a bounded
`max(request timeout, 600) + 15` seconds. The focused test requires that exact
ceiling, and all 1365/1365 Release domain tests pass. A clean validated
published checkpoint and accepting rerun remain required before the Human
render gate can close.
The full `Build-Local.ps1` gate then passed at 2026-08-30T21:54:14Z.
Standalone and local-runtime packages are byte-identical at SHA-256
`2f515302e2d0263adccb837b4e4f079d1120fcb0074054fae9ba4093aef76849`;
the DLL SHA-256 is
`7fb96cd42ed986241fa63f79a52e01633da7c8b7bc18e1ed68d0a1562e4d5aac`.
Runtime preflight first observed the immediate post-build timestamp-invariance
guard, then passed all 160 checks once outputs were quiescent.

Accepting rerun
`runtime-evidence/20260830T2158516580621Z-gunslinger-outfit-candidate-render`
used a clean published tree, `-TimeoutSeconds 600`, exact save
`KMG_AUTOMATION_WORKING`, automatic exit, and no force-termination option.
Outer orchestration reached `final-result-received` with PASS, and all 10
in-game assertions passed without exception.

The run loaded commit `8f47f2db723fdfe6146ca30c352ea83ba7d3589f`,
package SHA-256
`2fdaa2813262e237e687ce277d1a67cc56a8e8ce72c778dcdbd01da995b5d7f4`,
DLL SHA-256
`c9ace6013911f041e5e824c340b04e06d2b09a5a1bbdee5e123396853e0900c0`,
and MVID `5a02e6db-4452-4a75-a2cf-f836f98a3407`. It records exact
request/save/build correlation, no save interaction/write, no production
mutation, 48 records, 96 images, 48 exact held states, 12/12 restorations,
and automatic exit with no remaining Kingmaker process.

All 96 accepted images were directly inspected from ignored local evidence.
The candidate-set identity reproduced exactly and the scored order remained
Magus, Rogue, Slayer. Zero preview images and eight ordinary-isometric images
were tagged low density; the latter remain an explicit final-matrix concern.

## Finalist race/gender source checkpoint

Guarded scenario `gunslinger-outfit-finalist-race-matrix` is now
source-qualified. It discovers supported races from the installed progression
root rather than an assumed list, derives two gender cells per discovered
race, and validates that native Magus `LoadClothes` produces the exact
audited ordered entity pair in every cell. Each request-local native body is
captured with native and alternate valid palettes, no visible weapon,
preview-like four-view framing, elevated isometric framing, and exact
entity/ramp/saved-link restoration. The bounded expected result is 18
fixtures, 36 records, 72 images, 180 views, and 18 restorations.

Repository validation, game-facing compilation, all 1365/1365 tests,
`Build-Local.ps1`, deterministic packaging, and strict standalone and local
package validation pass. Package SHA-256 is
`cdd85e981f9847b0259a965506db457af98818d25aaf7c87d619022eae9559dc`;
DLL SHA-256 is
`36cf201fca3040c3a7b9a35f4253207d87b5480b3f13b1df14897860fdb02b7b`.
The unchanged preflight passed 163 checks after the known immediate-post-build
timestamp guard became quiescent.

This checkpoint does not claim installed-game race visual acceptance.
Steam-backed execution, direct image inspection, and separate equipment,
animation, production, and persistence gates remain required.

## First finalist matrix diagnostic

The clean published run at
`runtime-evidence/20260830T2237589386140Z-gunslinger-outfit-finalist-race-matrix`
used Steam App ID 640820, exact save `KMG_AUTOMATION_WORKING`, and commit
`fe86bce4484d45ca8f6a6f7070bfd7942fd5a0fc`. It loaded MVID
`985fe6cf-03f4-4120-9a8e-e586315c1135` and DLL SHA-256
`09fcb5096344aac82da288b8306b212b8b0dc44c9c7654f4d6515ff293a735a9`.
The outer and in-game result correctly reported FAIL after 122,368 ms.

Male Aasimar completed native-default and alternate palettes, producing two
records, four PNGs, ten views, and one exact restoration. Before female
Aasimar finalist application, the first deterministic native donor failed the
original-avatar exactness requirement. The gate stopped rather than applying
or scoring against an unproven body. Guard, working-save boundary, installed
Assembly-CSharp identity, cleanup, no-save behavior, no-production mutation,
and automatic exit passed. No Kingmaker process remained.

The retained donor inventory also showed a Medium initial female Halfling
source. The repaired selector now requires canonical size (Small for Gnome and
Halfling; Medium for all other installed player races), retains all exact
race/gender/size sources in deterministic order, and probes each source before
acceptance. The probe removes and re-adds the original avatar entities and
requires exact order, primary/secondary ramps, and saved links. A rejected
disposable source is recorded with its attempt index, identity, reason, and
mismatch details, retired, and followed by the next source. This does not
weaken restoration or touch a campaign actor.

After repair, repository validation, game-reference compilation, all
1365/1365 tests, clean Release build, package construction, strict standalone
and local-runtime validation, and the quiescent 163-check runtime preflight
pass. Pre-publication repair package SHA-256 is
`255de7da0529767b089d65fbd9638fb4964020a562797f1c6048d3315014c624`;
DLL SHA-256 is
`c9840e31c00997b9c6d50b6f6b044175cbe34165d3f00414ce90fc7781040bef`.
Full installed-game rerun and direct inspection remain open; the partial images
make no aesthetic or compatibility acceptance claim.

The clean published retry at
`runtime-evidence/20260830T2257046480918Z-gunslinger-outfit-finalist-race-matrix`
loaded commit `a27c4a7ecb061bf972df799ee096dc1b31e5e62d`, MVID
`7b7c4298-4aa5-4689-b317-23f15ccbfbc5`, and DLL SHA-256
`a4ffb80afa5a9574e5138bdd72b196708c23de9f7218cdb43322158f5ddbe1c7`.
It safely exercised all six deterministic female Aasimar donors and recorded
the same detail for each: avatar present, original entity count zero. The run
then failed closed with donor exhaustion. Cleanup, no-save behavior,
no-production mutation, and automatic exit passed.

Zero entities is a legitimate exact ordered baseline. The corrected probe
removes/re-adds it, requires the resulting sequence to remain empty, verifies
saved links are unchanged, records `originalEmpty=true`, and still fails a
null avatar. The exceptional cleanup path performs the same verification.
Nonempty entity order and ramp requirements are unchanged. All 1365 tests and
the clean strict package gate pass; pre-publication repair package SHA-256 is
`3b7e2deb7b96dac8e62eba66d1628af2355e0ab2c4ff4259ab245e5710b3168a`
and DLL SHA-256 is
`8621f5402e652fbdc1b3eb7d0657d0450f3f5c00cfd861a02961c5563cb0e46f`.
This remains instrumentation qualification, not race-grid acceptance.

The next clean published run at
`runtime-evidence/20260830T2309022972406Z-gunslinger-outfit-finalist-race-matrix`
loaded commit `5116a127d92ca09ea13e5822439e6b833b47c7e7`, MVID
`79cd33b7-76d0-4240-bd20-6bc51d4a5729`, and DLL SHA-256
`4fb8ef472bf4ac602b1861a0d1d42094d0995cdd3090bc7e8fee457ce04f26bb`.
It passed every mechanical matrix assertion except cleanup: 18 dynamic
race/gender fixtures, 18 exact donor probes, exact native links, 36 palette
records, 72 PNGs, 180 views, and 18/18 restorations. No exception, save call,
production mutation, or party delta was recorded.

The global unit set alone remained nonidentical after 360 cleanup updates.
Because the evidence did not yet identify the differing reference, the run is
FAIL and its images are not accepted. A diagnostic-only checkpoint now emits
expected/current counts and described missing/unexpected unit and party
references while retaining the exact cleanup condition. Compilation, all
1365 tests, clean Release/package construction, and strict validation pass;
pre-publication package SHA-256 is
`368140973c5e42aacf420168159b30b4a48fe26c7476984a282f621b529721f2`
and DLL SHA-256 is
`93edda11b82111e8a76c1c2298e7260ae142e8d1c68ba127e004b6cef7ea24aa`.

The resulting published diagnostic evidence is
`runtime-evidence/20260830T2323563433313Z-gunslinger-outfit-finalist-race-matrix`.
All 18 fixtures, 36 records, 72 images/180 views, native links, palettes, and
18 restorations passed again. The exact cleanup difference was one unexpected
unit and no missing units: `Leopard`, blueprint
`AnimalCompanionUnitLeopard` (`54cf380dee486ff42b803174d1b9da1b`), unique ID
`e8019935-e26e-4be8-a799-c00d8fb7a26f`. The global count changed 265 to 266;
the party remained exact at 3 and the disposable actor was absent. The run is
still FAIL and no images from it are accepted.

The next checkpoint retains whole-snapshot equality and adds no world-delta
heuristic. A unit is eligible for request-owned cleanup only when reached by
exact reference through the active disposable actor's native
`UnitDescriptor.Pet` property and absent by reference from the initial unit
snapshot. That exact dependent is recorded and retired before the actor; the
gate still requires the original global-unit and party sets. Repository
validation, installed-reference compilation, 1365/1365 tests, clean Release
build, and strict installable-package validation pass. Pre-publication package
SHA-256 is
`ddb92778082adc354b1e574abad9a467a10246c17cefa75ab61281f410feab62`;
DLL SHA-256 is
`af8262f6593053ceadf56af84c26e56e61d38964b816ed39896ce7b5f7885b39`.

The published cleanup run at
`runtime-evidence/20260830T2341080018300Z-gunslinger-outfit-finalist-race-matrix`
loaded commit `8b8d0b17aa90318425404efac56f6977bb2ad11c`, MVID
`3595f627-40de-4b76-830b-99920d2838ac`, and reached terminal PASS. It
completed 9 installed races/18 gender cells, 36 records, 72 PNGs/180 views,
18/18 exact restorations, exact 265-unit and 3-party-member snapshots, and
recorded no save or production mutation. Its request-owned
`AnimalCompanionUnitLeopard` relationship belongs to male HalfElf and was
retired exactly; this corrects the earlier inferred female-Elf association.

Direct visual inspection of all 72 PNGs rejects that batch. Several donor
prefabs rendered non-avatar clothing or equipment, including stored shields,
bows, quivers, capes, and large weapons. A mechanically empty avatar was thus
not a neutral player body. These images do not qualify race compatibility or
change candidate scoring.

The replacement fixture follows Kingmaker's native character-creation path:

- resolve the exact `BlueprintRace.Presets` entry for race and gender;
- configure `DollState` with that preset and the exact native Magus class;
- create `DollData` and its native `CreateUnitView(false)` player view;
- spawn a disposable same-race/gender descriptor with that view;
- clear every `UnitBody.AllSlots` item and both weapon-model channels;
- require a nonempty exact doll entity set with zero unexpected avatar entity;
- retain exact avatar restoration, global cleanup, no-save, and no-production
  gates.

Focused source contracts enforce each step. Repository validation, installed-
reference compilation, 1365/1365 tests, clean Release construction, package
creation, and independent strict package validation pass. The pre-publication
package SHA-256 is
`04f13af8fd17a0d9e18611e13c3cc3d27d83f6c7cf1e7dca3b05e094e5f73d18`;
DLL SHA-256 is
`d3ec07a2238ff2c062686dfc4e570ee602afaa716a26ddfa01607cb2627653bc`.
Runtime and visual acceptance remain open until a published commit-bound run
replaces and directly reviews the complete image matrix.

## Local evidence policy

Raw catalogs, extracted metadata, screenshots/contact sheets, runtime result
batches, assemblies, saves, and machine-local configuration stay ignored and
untracked. This file will contain only concise curated findings, reproducible
commands, hashes/fingerprints permitted by policy, and honest uncertainties.

## Acceptance threshold

The selected candidate must score at least 75/100 and have no missing geometry,
broken material, baked weapon duplication, unacceptable body-part hiding,
severe animation clipping, unsafe race/gender gap, broken armor transition,
optional dependency, or generic-Fighter identity. Until every applicable gate
above passes, the mission remains unqualified.
