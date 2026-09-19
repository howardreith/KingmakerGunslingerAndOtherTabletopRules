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
