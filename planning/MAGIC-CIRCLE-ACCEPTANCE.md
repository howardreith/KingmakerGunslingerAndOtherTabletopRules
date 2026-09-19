# Magic Circle implementation and acceptance

Status: implementation in progress; no Magic Circle runtime or visual acceptance.

## Baseline and boundaries

- Repository: `howardreith/KingmakerGunslingerAndOtherTabletopRules`.
- Base: `21a1c4b25a61f481f76b8fb0d57bd7fc4a5698dc`, clean current checkout,
  `0.0.132` / `0.0.132-icon-art-overhaul` (not a historical version pin).
- Worktree: `.worktrees/magic-circle-alignment-spells`; branch:
  `codex/magic-circle-alignment-spells`. Other worktrees remain untouched.
- Sprint 30 prerequisite: `BUILD-QUALIFICATION-S30.json` records the native
  smoke and two fresh-process generic-firearm-actions PASS runs.
- Applicable instructions: lab ancestor and repository `AGENTS.md`; the
  mission's explicit runtime/retry/scope instructions govern this feature.
- No merge, release tag, public release, normal-play replacement, software
  installation, machine policy changes, or campaign-save mutation authorized.
- Only guarded Steam App ID 640820 runtime qualification and approved disposable
  fixtures. Never select or modify `KMG_AUTOMATION_BASELINE`.

## Implementation contract

Four separate level-3 spells: against Evil/Good/Law/Chaos; opposed descriptors
Good/Evil/Chaotic/Lawful. Standard action, touch a creature, moving 10-foot native
emanation, 10 minutes per original caster level. All covered creatures receive
native alignment-conditioned +2 deflection AC / +2 resistance saves. Reuse the
existing Protection component, catalog, source resolution and fail-open policy;
`protection-from-alignment-control-immunity` is the sole control authority.

Original caster, moving bearer and incoming controller remain distinct. Carrier
owns expiration and dispel context; derivative recipients are owned by each area.
Typed bonuses never stack with themselves. No recipient removal by blueprint.
Register stable identities for hydration even when publication is disabled.

Deferred exactly: existing-control saves/morale/suppression/removal/resumption;
summoned-creature contact/exclusion/SR barriers; inward circles/binding/diagrams;
blanket descriptor immunity; silver-dust bookkeeping; unrelated system changes.

Tabletop references inspected 2026-09-19: [Evil](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Magic%20Circle%20against%20Evil),
[Good](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Magic%20Circle%20against%20Good),
[Law](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Magic%20Circle%20against%20Law),
[Chaos](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Magic%20Circle%20against%20Chaos).
Kingmaker adaptations and native lifecycle decisions will be recorded separately
from these rules after inspecting the installed contracts.

## Checkpoints and evidence

| Gate | Required evidence | Current status |
| --- | --- | --- |
| A baseline/shared contract | installed RuleApplyBuff/area IL, donor inventory, Protection regressions, clean baseline checks | PASS baseline and guarded donor inventory; gameplay pending |
| B Evil vertical slice | native cast on another bearer, area membership, typed outcomes, actual control positive/negative, resource debit | pending |
| C ownership/lifecycle | overlaps, movement/teleport, dispel/expiry, unconscious/dead/unload, transition/save/load, OFF hydration | pending |
| D complete family | four variants, verified lists/optional entitlements, scrolls/vendors/scribing, configuration text, icon graph | pending |
| E final candidate | validation, complete clean domain suite, Release build, package gates, exact-artifact native matrix | pending |
| Art acceptance | original sources/exports/catalog mappings, protected assignments, native UI, exact owner approvals | pending |
| Publication | coherent qualified commits, guarded helper, owner-review branch/PR | branch created; no commits yet |

## Acceptance matrix

Track each independently: original caster level/metamagic/dispel attribution;
bearer/pet/summon/enemy coverage; native radius/line of effect; actual matching and
nonmatching AC/save rules and stronger equipment; each alignment control block;
unprotected/wrong-alignment controls that actually apply; differing caster,
bearer/recipient/controller alignments including neutral and multi-component;
ability/direct terminal/custom/optional/unresolved control; fear/confusion/Hold
exclusions; pre-existing control unchanged across entry/re-entry; all overlap
combinations and standalone/communal/Paladin coexistence; individual carrier and
recipient dispel; expiry/teleport/transitions/persistence/death/unconscious/unload;
startup boundary/optional profiles; native learning/preparation/known spells and
alignment/specialization; scroll charges/scribing/vendor stock; publication
idempotence/foreign-safe rollback; module-OFF hydration; every visible icon/text.

## Resume

Inspect native area ownership and donor graphs, then implement the Evil vertical
slice. Continue the mission through the remaining checkpoints without treating a
build, checkpoint report, or policy test as native acceptance. Record exact hashes,
profiles and evidence paths here; preserve raw artifacts locally under `artifacts`
or the guarded evidence root. No blocker identified yet.

## Checkpoint A: observed native contract

Unchanged baseline: repository validation PASS, all 1,657 domain tests PASS,
clean Release compilation PASS, strict installable package PASS. The first run
had two unrelated Bodyguard source-contract failures because its ignored native
IL prerequisite was absent from the new worktree. Copying the existing local
inspection artifact resolved both without a source change. Logs:
`artifacts-baseline-build.log`, `artifacts-baseline-qualified.log`.
The separate old `Test-RuntimeRequest.ps1` still pins 0.0.87 and fails its version
guard; new Magic Circle request tests read the current `Info.json`.

Installed Assembly-CSharp SHA-256:
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
`BuffCollection.TriggerRuleApplyBuff` supplies the recipient as Initiator;
retain the existing initiator handler. `AddAreaEffect` passes the carrier's
original context to `SpawnAttachedToTarget` with the bearer as attachment.
`AbilityAreaEffectBuff` uses exact `SourceAreaEffectId` ownership for entry/exit.
Recipients must use Stack with separate circle identities; native typed bonuses
supply maximum-bonus semantics. Native membership includes corpulence and a
line-of-sight obstacle test. The Paladin donor has an ally condition and
AffectEnemies=false, so it cannot be copied wholesale. There is no pre-existing
Magic Circle spell in the observed final library (only a script zone with that
name). Native caster-death termination and recipient dispel behavior need
explicit lifecycle resolution before acceptance.

Read-only guarded Steam audit PASS:
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260919T2204045352199Z-observe-magic-circle-native-contracts/runtime-result.json`.
No save was selected, loaded, or written. Observed profile includes Call of the
Wild. Audit evidence is donor inventory, not spell mechanical qualification.
Audit source also passed validation, 1,657 tests, Release, package gates.
Package SHA-256: `f6329ebb6c6e16abe33a1ab6044c179bac993394b2a635a7ac1546dbc9edaf24`.
DLL SHA-256: `37deae53652ec6d247d8bcec1176c567ee1e1088221c646716574fdb1da6737f`.
MVID: `fd86118b-799c-47ad-ba61-59442765dd0d`; loaded version 0.0.132.
Source-state hash: `2c5631ef4a5337320475cbaf8d661ba3a15074bd75d3458e986637aaae65dde8`.
Local raw audit/IL records remain ignored, not committed.

Authorized qualification deployment retained original normal-play installation
at `C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20260919T2203354648740Z`.
Restore it through `Restore-Live-Mod.ps1` before handoff when the game has exited.
The audit process exited normally. Original settings hash:
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.


## Evil vertical slice in progress

Checkpoint A committed/published as
`2c396bcdb8b2738f4ef25d0f940b593753bdc5f8` through the required guarded helper.
The Evil implementation now registers four stable identities and uses the
native carrier/area/recipient path. The independent content setting adds schema
12 and the thirteenth module; all 8,192 combinations round-trip in the complete
1,657-case suite. Base spell lists are Cleric, Wizard/Sorcerer, Inquisitor and
Paladin at level 3. Only the shared Protection enhancement enables the added
control component. Recipient contributions are not independently dispellable;
the carrier owns dispel/expiration. Remaining variants, hostile touch delivery,
metamagic, stronger equipment, optional lists, scrolls, full lifecycle/settings
persistence, and final art/UI qualification remain incomplete.

Candidate 1: validation PASS, clean full domain suite 1,657/1,657 PASS, clean
Release PASS, strict package PASS (`artifacts-magic-circle-evil-build-second.log`).
Package `0c27d2816ca855137bb1395bff860097bf27beb44412975c1fb34221ad12e14f`;
DLL `e09cd4b837089f1d9da59de951a91e3312bc0b2e3c11e93e938582f1c1348ab8`;
MVID `eb14e4e4-5244-4d69-b682-b800243e0458`;
source state `9fae50a7207136965bafd20ce1529ec2ca646994b2ab1566b4ba59c63d52a7dc`.
Native run FAIL at fixture animation completion:
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260919T2251280708741Z-disposable-magic-circle-evil/runtime-result.json`.
The exact working save loaded; targeting/availability/CanStart were true.
The fixture incorrectly looked for the inherited animation setter only on the
runtime type. Candidate IL identifies the failing reflection call. Use the
existing `FinishExpandedSummoningAnimation` helper, which walks the native base
types. Cleanup PASS: original units, party, areas and buff instances restored;
no save writes. No mechanical PASS is inferred from this partial cast.

The optional broad preflight script exposes a pre-existing omitted expected
`disposable-firearm-higher-feat-roots` entry. Record it separately rather than
repairing that unrelated case. Its new-module count assertion is updated to 13.
Focused Magic Circle guarded request tests PASS and reject the protected
baseline save. The older generic request test remains pinned to 0.0.87.

Native area IL confirms `Size.Meters` is the cylinder radius (10 feet = 3.048m),
with 2D containment including unit corpulence and a native obstacle check.
The next candidate adds hostile-recipient, actual AC/save, radius/bearer motion,
same-caster stacking and standalone coexistence observations.
Four original imagegen sources and exact 128px exports are preserved locally in
`artifacts/magic-circle/art-preview`; export review was presented to the owner.
Approval is pending; no response is approval. Native UI evidence is also pending.
The candidate still uses explicitly provisional Protection sprites.

Candidate 2: all required source/package checks PASS; full suite 1,657/1,657.
Package `a65ae6b04c1f1275a12a721e99a5f7db569ed30ccd7146e0f221189bc71108e3`;
DLL `db46385f66cf5c98f1a1b324835ef8768ceaf887b95de5564c3210b54f7e6fb4`;
MVID `42b0f19c-6c78-4d1e-bde7-ae0bf4bcd929`;
source state `1b7f66029895403c3b99a1ca2a748cacd2ed9c688825b0e7642c22e8d2746032`.
Native run `20260919T2310294791518Z-disposable-magic-circle-evil` FAIL:
cast command Success and carrier present, then area lookup failed before the
native EntityCreator queue was ticked. Candidate IL at 0x044e confirms that
lookup, not carrier application. Cleanup PASS and no save writes. The fixture
now ticks the native creation queue before reading areas and records the exact
published identities. No production behavior is changed to accommodate it.

Candidate 3: repository/full 1,657-test/Release/package checks PASS.
Package `c6bcf9f1c7f10f4a73a9c42aa2b7b6dc56aedff134c8c5f56a86e302b143cb35`;
DLL `74beb3cfda5e24f17b2e3d7a5cf578ac5ef213fbca690d5b4ebe8557d9a7657a`.
Native run `20260919T2318073456637Z-disposable-magic-circle-evil` FAIL.
Native cast spends exactly one slot (5 -> 4); another bearer owns the carrier;
original caster is retained in carrier/area and CL6 gives 3,600 seconds.
The area follows the bearer. Coverage is absent (0/0), so subsequent immunity
and stacking observations do not qualify those mechanics. Positive control
applications and final exact cleanup pass; no save writes.
Installed IL shows area membership enumerates the native InteractiveObjectGrid,
which UnitMoveController.Tick reconciles. The paused one-frame fixture had not
run that controller. Next fixture uses the existing bounded, zero-time native
movement pattern on only its standing actors, records ShouldUnitBeInside/shape
before reconciliation, and fails before downstream claims if coverage is absent.
Production membership rules and entry conditions remain untouched.

## Evil core native checkpoint (candidate 4)

Validation PASS; complete clean domain suite 1,657/1,657 PASS; clean Release
PASS; strict package PASS. Build log: `artifacts-magic-circle-evil-build-fifth.log`.
Guarded Steam run `20260919T2326498442084Z-disposable-magic-circle-evil` PASS,
18/18 assertions, exact working save, no save writes, exact final cleanup.
Curated results and exact package/DLL/MVID/source-state identities are in
`reports/magic-circle/EVIL-VERTICAL-SLICE.json`. This is the pre-commit candidate
built from the recorded dirty source state, not qualification of a later commit.

Observed native slot debit 5 -> 4; original caster on the different bearer and
area; 3,600-second duration at CL6; bearer and hostile creature each receive an
area-owned contribution. Actual matching attack AC 10 -> 12 and Will save 0 -> 2.
New matching-source Dominate Person terminal application is vetoed by RuleApplyBuff;
wrong-alignment and unprotected controls actually apply. Existing domination
survives entry. Native 3.048m boundary includes recipient corpulence (0.5m).
Bearer motion follows correctly; two same-caster contributions coexist without
+4 bonuses; removing one leaves the other; standalone Protection survives both.

No production change was needed for the failed paused-fixture coverage: native
spatial-grid reconciliation resolved it. Remaining acceptance items in the table
still apply. Full incoming spell casts, more catalog paths, metamagic, hostile
touch, other casters/alignments, native dispel/expiry, persistence/transitions,
module boundary profiles, learning/acquisition, and final presentation are next.

A rules clarification is pending: end on bearer death (native ordinary buff
cleanup) versus persistence around the corpse until original expiry. PF1's
entry does not specify that case; AGENTS design authority reserves unresolved
adaptations for the owner. Original caster death and unconsciousness are separate.
The four exported paintings remain awaiting owner approval; silence is not approval.
