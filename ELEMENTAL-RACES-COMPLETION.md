# Elemental Races completion - 0.0.120

**Both traits are implemented and selectable: 21 alternate racial traits total.
The owner explicitly approved merge and release after being asked about the
AGENTS exception and Treacherous publication without its remaining native
Player/respec/save qualification. Final repetitive runtime tests are waived;
qualification remains partial. Release execution and exact identities are
recorded below after success.**
Only Nereid Fascination and Treacherous Earth are in scope. This ledger supersedes
historical candidate-only status. Favored-class bonuses and the deliberate
expansion deferrals remain separate work.

## Baseline and retained content

- Public 0.0.117: `f8a2fd996752afb0e361a53bec175328ace5435a`; ZIP SHA-256
  `9368c1ff2c82b76574bab5ed75868d7eb633e759f925e82a1c0da7e861f62f6f`.
- Authoritative origin/master at mission start and the later fetch:
  `8e5eeae7973c71ca4b78dc8216d00d815af7ea26`.
- Public 0.0.118 was published for contextual world-map teleportation during this
  mission, at 2026-09-08T17:48:27Z. Its immutable source is
  `b439f5df22e2260322453c069b23eaee734b8a33`; public ZIP SHA-256
  `2fe00a2ce35f29cfe20c1acdfbb8e2f74923d1790b4a9b617bfba0224ee6eba7`.
- Public 0.0.119 was published at 2026-09-09T00:57:32Z, source
  `72520eeb66a036da50473bec7b2e0019ca6304f6`; ZIP SHA-256
  `4d1b32f7edb31503c9dac9227f92629df9d8278e44ff9738d24bc95e25cd55a3`.
- Isolated checkout: `KingmakerGunslinger-elemental-completion-120`. Feature
  checkpoint `df6b28c3893ab8234140aecc655f8144686329e6` follows public119 and
  verification record `dd0a344226c31e34d251702f5832e75525137260`. Its merge into
  master starts at `8e5eeae7973c71ca4b78dc8216d00d815af7ea26` and is explicitly
  owner-approved. Version 120 was unused at the final upstream check. Original
  and private119 checkouts remain intact; no history rewrite or policy edit.
- Four races, twelve heritages, eleven racial feats, nineteen released alternate
  traits, Heritage group 42 ordering, retain-base, native respec expenditure,
  visual resource retention and ordinary ZFavoredClass Trait ownership remain
  mandatory invariants. The original empty background-Trait trigger is unattributed.
- The released Teleportation production files are unchanged, including module
  count 12 and settings schema 11. Its public qualification limitations remain
  inherited; no new teleportation feature or qualification claim is made here.

## Implementation and publication

| Trait | Implementation | Ordinary selectable | Remaining qualification |
|---|---|---|---|
| Nereid Fascination | Implemented; exact native mechanic/ordinary selector checks passed | Yes | Final Player/save/profile repeats waived; earlier native evidence retained separately |
| Treacherous Earth | Native terrain, daily action/resource, saved areas, 3D ground targeting and source-floor isolation implemented; 68 focused assertions passed | Yes | Native Player creation/respec and full trait/save integration remain unqualified; owner explicitly approved publication with this gap |

The manifest contains **1,883 identities: 1,881 active and two reserved**, with
240 active elemental identities. All 1,872 public 118/119 entries remain an exact
prefix. The existing markers/providers are retained:

| Trait | Marker | Provider |
|---|---|---|
| Nereid Fascination | e117e1e0a17a4acec001000000000040 | e117e1e0a17a4acec001000000000061 |
| Treacherous Earth | e117e1e0a17a4acec001000000000031 | e117e1e0a17a4acec001000000000052 |

Eleven new auxiliary IDs retain their stable e118 namespace. Every auxiliary
blueprint is in Mechanics; registration remains active with the module OFF.
Providers restore remembered daily expenditure before final reconciliation.
Both traits replace the active heritage's SLA slot and preserve compatible
resistance/affinity replacements. Native revision 0 deferred markers stay
outside replacement resolution; an enabled mechanic retires only its old inert
marker, preserving legitimate providers and spent resources. New markers use
revision 1; the nineteen released marker paths are unchanged.

## Rules and limits

[Nereid rules](https://www.aonprd.com/RacesDisplay.aspx?ItemName=Undine) and the
[fascinated condition](https://legacy.aonprd.com/coreRulebook/glossary.html#fascinated)
provide the base contract. Nereid is a standard supernatural activation, one use
per ordinary rest, radius 20 feet, DC 10 + floor(total level/2) + current Charisma
modifier, duration max(1, floor(total level/2)) rounds. It uses no spellbook, spell
resistance, arcane spell failure, spell-level DC or elemental affinity bonus.

The exact native person-exclusion contract handles humanoids locally. Allies
are included, the caster excluded, and native area line of effect applies.
First exposure gets one save; failed-save re-entry uses the original expiration.
Successful/interrupted responses are terminal for that activation. An obvious
perceived threat breaks the owned condition; hostile approach permits one new
save per native command. Shake Free is a standard action at touch range.
Native Fascinate uses Dazed; no paralysis, stun, charm or domination is added.
One native -4 Perception penalty survives overlap without multiplying.

Threat perception uses the subject's own sight/LOS or native ranked blindsight;
blindsense can detect approach but cannot identify precise weapon/spell cues.
**Hearing-only threat recognition is unrepresented** and must be named in the
player description. No global fascination patch or invented hearing radius exists.

[Treacherous Earth rules](https://www.aonprd.com/RacesDisplay.aspx?ItemName=Oread)
limit the touched 10-foot patch to earth, unworked stone or sand. The owned
Ground/DifficultTerrain graph lasts one minute per total level, adds no damage,
save or secondary condition, and uses a daily standard activation. Native
stacking, Feather Step and released flight remain authoritative. Saved areas
retain position, caster, creation time and remaining duration.

Sound-grid x/z data, textures and audited mesh metadata do not prove actual
material at the target height or distinguish worked from unworked stone.
The owner delegated the choice: "Use your best judgment based on the available
rules and any web search you can do." The selected CRPG adaptation permits
**any valid walkable ground within touch reach, including built floors**. This
intentionally broadens the printed material restriction; descriptions must say
so. The resolver must identify the actual 3D navigation surface and an unblocked
local path, rejecting wrong floors, gaps, inaccessible or invalid points. No
sound grid, texture name or camera ray is material evidence. The decision is
settled. Production targeting now uses native triangle contact, 3D closest-point
queries, touch range, line of effect and a portal path with terminal-height
verification. Owned area membership also checks the actual source floor. The
trait is selectable under explicit owner approval; full Player/save integration
qualification remains waived, not PASS.
Same-area reload is not different-area travel proof.

## Current 0.0.120 candidate evidence

The reviewed transfer preserves every public 119 Teleportation production file,
all 1,872 released manifest entries, and guarded persistence-plan/owned-save
checks. Transfer: `artifacts/qualification/0.0.120/public119-transfer-record.json`.

Build 01 passed repository validation, **1,557** complete domain/reflection tests,
clean exact-reference Release, strict **135-file** UMM package validation and
**464** runtime preflight checks. Source fingerprint
`e0ec34c57777177972119c2561b610dfc1bd30e96e3e524d3aa455220092fa00`;
ZIP `71c180f102e3703310eb0e6afb5eb4f71d1ff5fcb366196221011dd3bccf8bb8`;
DLL `84efe1f1c08100294c092f15cdee7f5270e7f5cb4a97786415367ae0854e0732`;
MVID `e5e481a1-7061-4235-abd6-2288368a02aa`. Exact build sidecar:
`artifacts/qualification/0.0.120/elemental-completion-build-01.build-local.json`.

That build passed Nereid core **152/152** on the actual installed stack and
Elemental-OFF native creator **11/11 each** on A/G/H/C/D/F. Every driver restored
mods/settings/saves exactly. Run IDs, result hashes and counts:
`artifacts/qualification/0.0.120/build-01-runtime-gates.json`.
G attempt 01 never reached the first UMM update or scenario; after ten minutes
the positively identified owned process closed normally and exact restoration
passed. This is a recorded launch failure with no mechanical result, not PASS.
G attempt 02 passed. Evidence: `120-off-creator-g-01-startup-recovery.json` in the
same qualification directory.

The six-file ordinary Nereid publication patch is now applied after those gates.
Build 01 predates that patch. Build 03 passed all 1,557 tests, clean Release,
strict packaging and 464 preflight checks. The ordinary framework run
`20260909T0221204489935Z-observe-elemental-alternate-trait-framework` verified
Nereid's live selector and exact mechanic but **failed** the final coverage count
(61/183/366 observed versus obsolete 60/180/360 expected). Exact restoration
passed; this run is not an overall PASS. The coverage expectation is corrected
in the next source candidate. Build 02 had failed one obsolete domain expectation
that both missing traits must stay hidden; that test now checks actual Undine
policy ordering. Raw evidence and build 03 identity are under the same scoped
qualification directory. The new ground implementation has no runtime result yet.
Both new traits still need final current-artifact integration. Payload comparison
`payload-comparison-build-01.json` finds 128/135 public117 ZIP entries exact and
129/134 non-DLL public119 payload files exact against released source; all art,
audio and asset bundles are unchanged.

Build 06 passed 1,557 tests, clean Release, strict UMM packaging and 464 preflight
checks. On F, the corrected ordinary framework and Nereid core both passed with
exact restoration. Exact run IDs, counts, hashes and build identity are retained
in `artifacts/qualification/0.0.120/build-06-runtime-gates.json` and
`elemental-completion-build-06.build-local.json`. Terrain A failed: its native
movement fixture had navigation at y=0 but no matching physics collider; actors
moved to the existing scene floor at y=-0.5. Production correctly rejected those
noncontact positions. Slope/bridge/gap/stacked-floor/obstruction and owned-floor
checks passed individually. The next fixture supplies real matching disposable
physics ground; production tolerance is unchanged. A negative walkability control
also moved off a shared triangle edge. Build 04/05 compile failures were missing
namespace qualification; build 05 repeated before the intended fix reached the
correct path. Both were corrected before build 06. No failed run is counted PASS.


Build 08 passed the same complete source/build/package/preflight gates. On A and
F its ground/effect checks passed individually: native movement costs returned
to 27 ordinary / 53 terrain / 53 overlap / 27 Feather Step / 27 flight ticks.
Both runs nevertheless **failed** when the fixture queued an already-rejected
far target: native `UnitUseAbility.Init` attempted its absent QA reporter.
The next fixture respects native target rejection before queuing; real accepted
and canceled RTWP/TB commands remain required. No exception was ignored and no
engine hook changed. `build-08-runtime-gates.json` records exact runs/hashes and
restoration. Build 07 had failed because physics types were not compile-time
references; build 08 binds the game's already-loaded physics module through
reflection, adding no assembly dependency. Production ground validation is unchanged.


## Historical private-119 evidence

Build `elemental-completion-build-03` passes repository validation, all **1,557**
domain/reflection tests, clean exact-reference Release, strict 135-file UMM
package validation and **460** runtime preflight checks. Earlier attempts failed
because the new checkout lacked ignored GamePath.props and a preexisting local
Assembly-CSharp.il test input; both inputs were supplied unchanged. No failing
behavioral test was removed or weakened.

Source baseline: `b439f5df22e2260322453c069b23eaee734b8a33` plus uncommitted candidate changes.
Source-state SHA-256: `0f6f54e3769f1aabc1af7d58fbe7f25b519e8e80878fbe89d8e54a54d7cf8c81`.
DLL SHA-256: `24ec9a96b83c837775edb0efa20ac1355286db903405ba717fffe307e1ad079c`.
MVID: `4156159e-90c5-4c4d-bbb6-145f7c2f82ea`.
ZIP SHA-256: `48ce5713eee10f872fdcffe43156df4223159aff65a500c63e050280e0133fdc`.

The private119 queue was stopped after the owner published public119. D module-OFF creator passed
11 assertions in `20260909T0038133231240Z-disposable-elemental-character-creation-baseline`,
raw SHA-256 `3e055501f30f0ff88c9dfa32cf1ed1e51485b16f65bd36c2cb97a7a0b5d4e2b3`.
Both Human final reviews and cancellations, ordinary Trait roots, Helpful ON,
foreign arrays, creator cleanup and native host restoration pass. The driver
verified exact mods/UMM/save restoration. D creator also passed 13 assertions with six native commits in
`20260909T0041477643093Z-disposable-elemental-nereid-creation`, raw SHA-256
`71a499270f375843804e4ad62af3b4f841256be0e5ffb4cc091dad52aff5557b`,
with exact restoration. D male respec passed 13 assertions in
`20260909T0052180011250Z-disposable-elemental-nereid-respec`, SHA-256
`ac6bd5ae9a02e43badf75da5bf1e200f3d47ad042c04d635a9ce3457766fc243`.
D female respec passed 13 assertions in
`20260909T0103590157324Z-disposable-elemental-nereid-respec`; exact restoration
was verified. Subsequent OFF G/H/F/A/C launches were deliberately stopped before
profile mutation and are NOT-RUN on that build, not mechanical failures.

The package comparison preserves all assets. Against the local public117 ZIP,
127/135 files match exactly; changes are DLL, Info, manifest, three documentation
files, the inherited public118 smoke guide and LICENSE line endings. Against
public118 source blobs, 128/134 non-DLL payload files match exactly; LICENSE is
line-ending-only, and five expected version/manifest/documentation files differ.
The public118 binary ZIP was not downloaded or byte-compared. Its JSON release
manifest was read; exact evidence is under `artifacts/qualification/0.0.119`.
Normalize LICENSE to its released LF content before final packaging.

## Historical private-118 evidence

These results belong to the earlier private completion artifacts, not the public
118/119 Teleportation DLLs or the current 120 candidate. Detailed run records and exact
build sidecars remain in the preserved checkout's `artifacts/qualification/0.0.118`
and the shared `C:/Dev/KingmakerGunslingerLab/runtime-evidence` directory.

- Nereid core/sensory: 152 assertions on each required A/G/H/C/D/F profile,
  `20260908T1928217571413Z`, `1935070002471Z`, `1937520080801Z`,
  `1940024894604Z`, `1942485991185Z`, `1932093229788Z`. Exact
  level/Charisma/multiclass, person/immunity, save, radius/re-entry, action/resource,
  threat, duration and cleanup controls pass; no hearing-only claim.
- Actual native creator: all three Undine heritages and both sexes committed in
  A/G/H/C/F. Native Player respec passed both sexes in those profiles, including
  seven commits and two cancellations per sex, spent uses through setup rest,
  retain/trait/heritage changes, compatible traits and ordinary-rest reset.
  C's final runs are `20260908T2331133066537Z`, `2341277955359Z`, `2352489995974Z`.
- Original A female raw result `20260908T2102205854940Z` was accidentally overwritten
  by an earlier ledger update and is invalid evidence. It is excluded. Fresh
  replacement `20260908T2220053897910Z` passed 13 assertions; raw SHA-256
  `1f670adbdc935d1f0423417af5cc2ef1d6a170fecaa06f893648d803529a80bf`.
- Hydraulic prerequisite/inert behavior: creator `20260908T1842397709124Z`
  includes 240 native prerequisite rows. ON persistence `20260908T1858462549924Z`
  passes 253 assertions, including all twelve stored-feat inert rows.
- Nereid fresh OFF/ON, level-up/rest/removal/reselection, native respec, death/
  resurrection, polymorph/return and equipment/doll rebuild pass. Fixed-area
  extension's five fresh processes are `20260908T2114350244797Z` (9 assertions),
  `2117083245389Z` (68), `2120588752636Z` (176), `2124290200190Z` (255),
  `2132374525003Z` (9). Original owner/position/remaining lifetime, native expiry,
  exact trait-owned cleanup and final fresh absence pass; ground eligibility does not.
- Native same-area scene unload/load passes ON `20260908T2310142162359Z` and
  OFF `20260908T2315334285792Z`, seven assertions each, unchanged non-header
  save bytes and exact restoration. Different-area travel remains NOT-RUN.
- Treacherous effect-only A/F: `20260908T1548066973364Z` / `1551051341042Z`,
  41 assertions each. Four-metre movement is 27 native controller ticks ordinary,
  53 with one/overlapping/one-expired terrain source, and 27 with Feather Step or
  released flight. Exact disposable target checkers do not qualify legal surfaces.
- Public117/deferred117 OFF/ON: `20260908T1809288289313Z`, `1813529844295Z`,
  `1818401993762Z`, `1822123452286Z`, sixteen assertions each. Twelve actors cover
  all Oread/Undine heritages and both sexes; six old Nereid markers retire and
  six hidden Earth markers remain inert. This fixture uses Undine Acid/Ooze,
  not every retained-SLA permutation. No saves are rewritten beyond load count.
- Pinned114 markerless-General migration: transaction `20260908T1737333190792Z`,
  producer/migration/fresh-absence 11/10/8 assertions; exact external restoration.
- Module-OFF native creator A/C: `20260908T2323449848526Z` /
  `20260908T2327381151710Z`, eleven assertions each. The current 120 matrix extends
  this coverage; historical PASS is never relabeled with a new MVID.

Known inherited startup/shutdown diagnostics and the four exact ZFavoredClass
CustomLoadKeyNotFound files are separated in each attribution record. Missing
optional mods are NOT-RUN. No exception category is blanket-ignored.

## Delivery and outstanding work

The latest owner instruction authorizes commit, merge, push and release and waives
final repetitive tests. That supersedes earlier candidate-only stop/approval
statements. The automatic approval review nevertheless rejected two attempts to
publish Treacherous: first for the unresolved action-fixture failure, then (after
that failure was resolved) for missing native creator/respec/persistence and
legacy-save integration. Neither rejected edit executed at that time. The owner
subsequently answered the explicit AGENTS-exception/qualification-gap approval
question: "Yes I approve the merge and release, please execute it." This
supersedes those earlier publication blocks. No allowlist or approval policy changed.

Build 09 passed repository validation, all **1,557 domain/reflection tests**, a
clean exact-reference Release build and strict **135-file** UMM validation.
ZIP `b55398b8e66fce95559d57608d7a04c586d512723e5657d1285bc195041688f3`;
DLL `4305fb22bf4721783fe127f0c84be85877209d639115e75e72e367e79dc16738`;
MVID `85379de3-be74-469f-a803-d77d87a12059`;
source fingerprint `3140c6f46d9e95ad5ebc48a0e83dcbead009507eadedf99bb9618c2456c41582`.
These are development identities, not the final release hashes.

The corrected focused terrain run on the actual installed stack passed **68/68**:
`20260909T0323013221827Z-disposable-elemental-treacherous-earth`.
Raw result SHA-256 `8ed66be4f286850f61e9d53ab7f986652941c5220a8a643b00e0f274ed3db980`.
Driver `artifacts/qualification/0.0.120/120-ground-effect-f-02.json` reports PASS,
exact restoration and no failure. It proves unmodified production touch targeting,
real-time/turn-based native commitment/cancellation, movement, overlap/immunity,
floor/bridge/slope controls and cleanup. It is not native character-selection or
full persistent-trait acceptance evidence. Prior failed runs remain failures.

No additional broad profile matrix or final-artifact Player/save repetition is
claimed. Sound-only Nereid threat recognition remains unrepresented. Different-area
travel and real authored-surface coverage for Treacherous remain unqualified.
There is no unresolved rules choice: the broader walkable-ground adaptation was
chosen under the owner's explicit delegation and is disclosed in the description.

Temporary runtime installs used backup-first transactions and restored the original
mods/settings/save bytes. Final packaging, installation, source/merge hashes and
publication are recorded only after their corresponding actions succeed.
The preserved original staged index and private119 checkout are untouched.
Completion feature-branch pushes are refused by the required wrapper; master is
allowlisted. The newly authorized ordinary merge onto master is the supported
publication route; no branch rename or policy modification is used.

## Earlier delivery checkpoint (superseded by fresh approval)

Implementation checkpoint: `df6b28c3893ab8234140aecc655f8144686329e6`
on `codex/elemental-races-completion-120`. All source was committed after source
validation, 1,557 tests, clean Release and strict package validation passed.
The required wrapper refused that feature branch as non-allowlisted.

The latest owner instruction then authorized the normal merge onto master.
All 93 overlapping-history conflicts were resolved using per-file baseline
comparisons and reviewed in-place hunks that preserve upstream content. The
merged `src`, `tests` and `scripts` exactly match the tested feature commit;
464 merged preflight checks and repository validation pass. Manifest metadata
is corrected to 1,883 / 1,881 / 2, and authoritative historical report bytes are
preserved. No policy allowlist, prior release identity or other worktree changed.

**The merge is fully resolved and staged, but uncommitted.** Automatic approval
review rejected the seal-and-push operation, explicitly citing [AGENTS.md](AGENTS.md)
("Never merge branches autonomously" and the prohibition on direct master commits),
even though it acknowledged the new owner authorization. That tool decision is
an external publication limitation; it is not a failed source/build check. No
master commit, push, tag or public 0.0.120 release is claimed. The current master
ref remains `8e5eeae7973c71ca4b78dc8216d00d815af7ea26`.

The resolved index remains reviewable in the isolated completion-120 worktree.
Do not reset/clean it or overwrite the original staged index in the other checkout.
The final local package/build identity, staged-tree SHA, payload comparison and
precise installed-versus-packaged state are stored in the adjacent ignored
`artifacts/qualification/0.0.120/final-delivery.json` once generated. That record
contains no invented publication result. Treacherous's broader ground rule needs
no further rules choice; its selection and full native Player/save qualification
remain deferred. Publication of the prepared merge requires approval that the
automatic reviewer accepts as an explicit exception to the older AGENTS rule.

The first final local build exposed one pre-existing newline-sensitive source
contract test after Git restored Windows CRLF: it searched for a literal LF in
the otherwise unchanged twelve-module validator. The assertion's input now
normalizes CRLF to LF; all twelve field checks remain intact. This is a test-only
formatting correction, not a validator/gameplay change or waived failure.
`final-local-build-01.log` retains the 1,556 PASS / one FAIL result; the corrected
build is recorded separately. Production `src` remains exact to the feature commit.

## Approved final integration and delivery

The fresh approval authorizes completion of the prepared master merge and public
0.0.120 release, including Treacherous ordinary publication without its remaining
native Player/respec/save qualification. Both exact mechanic validators remain
active. Oread's existing SLA selector is retain/Treacherous; Undine remains
retain/Acid/Nereid/Ooze. All 21 choices use the established Heritage consumer.
Policy coverage now includes 69 legal combinations across 207 heritage rows and
414 activation-order rows; these are test expectations until executed, not runtime
PASS claims. Both old revision-0 completion markers retire without granting a new
provider/resource or consuming the existing heritage SLA. The effect-only terrain
persistence scenario remains labeled as such and is not Player acceptance proof.

Final integration checks passed: repository validation, all **1,557 domain/
reflection tests**, clean exact-reference Release, strict **135-file** UMM package
validation, and **464 preflight checks**. Evidence:
`artifacts/qualification/0.0.120/approved-release-build-03.log`,
`approved-release-preflight-03.log`, `approved-build-identity.json`, and
`approved-payload-comparison.json`. This pre-commit package has ZIP SHA-256
`e9ff01e33fded15078995f68a4f4fe8960976cb017d8c73a626c4088c4e542f9`, DLL SHA-256
`80bfd9d640dd3007e47a4a7307a7891bc7d2d6723ba0d2a52048ee2233f9cbe0`, MVID
`dbe86aa8-c24c-4a41-b623-323f7caf66c9`, and source fingerprint
`e2436901afdb263eb385d5e757ee54da39835720212b26255178394992f04cae`.
It is not the final release identity; the publisher rebuilds from the committed SHA.

All public117 files are retained: 128/135 match exactly. Compared with public119
source, 129/134 non-DLL payloads match exactly; changes are release documentation,
Info.json and the appended manifest. Art/audio/bundles are byte-identical and all
1,872 public119 identities remain an exact prefix. No public119 binary was
downloaded or compared. Teleportation production source and prior release reports
are unchanged.

Earlier attempts are retained as failures: the first build encountered an inherited
implementation-pending metadata default; the second found a stale retain-only Oread
assertion and an accidental test-string encoding change. All were corrected before
the passing third build. The first preflight ran with administrator elevation and
concurrent build artifact changes; it failed. The normal non-elevated, source-only
preflight then passed 464 checks. No new game launch was attempted from this
write-capable elevated context; the runtime guard is unchanged. Additional runtime
qualification remains owner-waived, not PASS. The existing installation remains
0.0.117; no final 0.0.120 installation is claimed. Publication identity will be
recorded after the approved merge, wrapper push and deterministic release workflow.
