# Magic Circle implementation and acceptance

Status: all four spells, separate held touch/scrolls, native area ownership, shared control protection, list publication and approved artwork are implemented. Latest guarded disposable checkpoint: 204/204 PASS. Final persistence/profile matrix, learning and native UI acceptance remain open.

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
| A baseline/shared contract | installed RuleApplyBuff/area IL, donor inventory, Protection regressions, clean baseline checks | PASS installed contracts, donor inventory and shared Protection native regressions |
| B Evil vertical slice | native cast on another bearer, area membership, typed outcomes, actual control positive/negative, resource debit | core PASS; hostile touch/full control-spell coverage pending |
| C ownership/lifecycle | overlaps, movement/teleport, dispel/expiry, unconscious/dead/unload, transition/save/load, OFF hydration | same-caster overlap, native dispels, Extend, faction changes, unconsciousness, expiry, caster and bearer death, four settings persistence, scene reconstruction and cleanup PASS; remaining cases pending |
| D complete family | four variants, verified lists/optional entitlements, scrolls/vendors/scribing, configuration text, icon graph | implemented; four-variant casts/control, acquisition/scribing, typed stacking and custom/optional terminal deliveries PASS; learning/UI and final profiles pending |
| E final candidate | validation, complete clean domain suite, Release build, package gates, exact-artifact native matrix | pending |
| Art acceptance | original sources/exports/catalog mappings, protected assignments, native UI, exact owner approvals | four exact exports owner-approved and integrated; native UI pending |
| Publication | coherent qualified commits, guarded helper, owner-review branch/PR | coherent checkpoints published through helper; no PR yet |

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

Latest published production correction: `145c7a3d`, preserving original caster
references through native context cloning. The current additional-delivery
candidate passes 204/204 native assertions and clean full source/package gates;
see `reports/magic-circle/ADDITIONAL-NATIVE-DELIVERIES.json`. Continue with
communal/Paladin coexistence, hostile held touch/metamagic, native learning/UI,
and final all-variant persistence/settings/optional-profile qualification.
Prepared drafts under ignored `artifacts/magic-circle/next` are not active source.

Owner decisions are resolved: end on bearer death; all four exact reviewed
128px exports are approved. Native UI remains separate. The working-save
persistence fixture is currently absent. Raw packages/evidence remain local.
Original normal-play backup for final restoration:
`C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20260919T2203354648740Z`.

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

## Lifecycle work after published Evil core

Core checkpoint committed/published as
`07db7d7de73b65b887f9a32f0954ca7b03a594a9` with the required helper.
The next candidate adds a narrow Magic-Circle-only native area lifetime guard.
The installed generic `EndEffectIfNecessary` ends source spell areas when their
caster dies/disappears. PF1 timed duration has no concentration requirement here;
the living bearer must retain the original circle deadline and original caster
context. The guard reuses the native serialized AddAreaEffect area-instance link
and exact parent Buff.Context. It adds no custom ownership state, timer or scan.
An absent/inactive/mismatched carrier ends only that exact area. Native membership,
line of effect, entry/exit and scene reconstruction remain authoritative.

The expanded native fixture now uses a real level-8 Sorcerer for a legal level-4
Extended cast, checks original doubled duration and one higher-slot debit,
exercises unconscious caster/bearer through native life processing, crosses both
sides of the original expiration with the existing paused owned-controller clock
pattern, then deals actual lethal damage to the original caster. Source and
runtime qualification of this candidate are pending. Bearer-death adaptation is
still pending owner direction and is not accepted by this work.

Lifecycle candidate 1: repository/full 1,657-test/clean Release/strict package
PASS. Package `4a1a5c8c353f00384d8d6362c6a2a3c87715a2929a8015ce797a92f7dbff7954`;
DLL `49b79b50cabf1cc9a83e7ebbdccb923c45e3cbe20691f92d42db4a3c19d2ae10`;
MVID `1865faa3-6d82-40a9-8e67-94cbb1ecb2ba`.
Native `20260919T2349049074541Z-disposable-magic-circle-evil` FAIL, 23/24
assertions PASS: Extend 9,600 seconds and one level-4 slot (5 -> 4), unconscious
caster/bearer, original expiry boundary, core regression and exact cleanup PASS.
Caster received actual lethal damage and was Dead, but IsFinallyDead stayed false.
Installed UnitLifeController.OnUnitDeath IL shows this depends on player-faction
TrueDeath difficulty, not elapsed death animation. The owned fixture now uses its
existing disposable hostile faction with GiveExperienceOnDeath=false before lethal
damage. No global difficulty, life result flag, or production mechanic is changed.
No save writes occurred. Final caster death remains unqualified until the rerun.

Lifecycle candidate 2: all source/package checks PASS, complete suite 1,657/1,657.
Package `acdc0d7f26f3534fce1d94fda030677a8d3f5829d6091ace71eb1b41208901a4`;
DLL `45dc2f4584f587fb909ee4c8cc3a7ecc7ce66d1a8652c56572b85135037adeab`.
Native `20260919T2356360754250Z-disposable-magic-circle-evil` FAIL: the new
final-death prerequisite used IsPlayersEnemy, which reads UnitGroup attack
factions, not the cloned blueprint's faction. It stopped before lethal damage;
core, Extend, unconsciousness, expiry and cleanup still pass. Installed native
SwitchFactions(faction, resetAttackFactions) updates the descriptor and faction
handlers. The fixture now uses that API and checks the exact non-player faction
contract that OnUnitDeath actually consumes. Difficulty remains untouched.

The next lifecycle candidate also closes the native area-dispel ownership gap:
RuleDispelMagic.OnTrigger calls AreaEffect.ForceEnd (sets m_ForceEnded only),
leaving AddAreaEffect's serialized carrier link alive. A successful native rule
for an exact registered Magic Circle area now removes only its exact active
parent carrier. Foreign clones and generic ForceEnd/temporary scene unloading
are untouched. Native casts test derivative-recipient non-dispellability, one
carrier removed from two overlaps, and point-area dispel removing its carrier.
Real native dice are bounded to twelve attempts; no outcomes are forced.

Lifecycle candidate 3: repository/full 1,657-test/clean Release/package PASS.
Package `67e8278dcd2164c67a9b622ad9938096019a191913cf6d011b124df7288bd32a`;
DLL `946ff58a4a0c9f34c3344eff366a7549d056c7b75a1df94349ed759dbc6d9697`;
MVID `2d6249a3-0ea1-4593-9911-bcba7bc5b9e9`.
Native `20260920T0005560907500Z-disposable-magic-circle-evil` FAIL before Dispel
casting: the fixture constructed a bare variant AbilityData, which cannot spend
its known parent spell. Core regression and exact cleanup PASS; no save writes.
Native graph: creature variant `143775c49ae6b7446b805d3b2e702298`, point variant
`9f6daa93291737c40b8a432c374226a7`. Installed AbilityData constructors/availability
and existing Brown-Fur native-cast fixtures verify the required production path:
`new AbilityData(new AbilityData(root, book), selectedVariant)`. The fixture now
uses that path (no SpellLevel override or availability bypass). Added exact
ability/level diagnostics and native bearer/recipient faction-change checks.

## Evil lifetime/dispel native checkpoint (candidate 4)

Guarded run `20260920T0014148181378Z-disposable-magic-circle-evil`: PASS 29/29.
Repository validation, full clean 1,657-test suite, clean Release, strict package
all PASS separately. Exact hashes/source state are in
`reports/magic-circle/EVIL-LIFETIME-AND-DISPEL.json`; this is the recorded
pre-commit source candidate, not qualification of a later rebuilt artifact.

Actual targeted Dispel Magic spent one slot and left two derivative contributions
unchanged (zero eligible RuleDispelMagic events). Targeting the bearer removed
one exact carrier/contribution from two. Point-targeting the remaining area
succeeded at native roll 12 + CL8 versus DC19 and removed its exact carrier.
No independent timed recipient spell exists. Both native dispels succeeded on
one attempt in this run; earlier fixture failures remain recorded above.

Native Extend: 9,600 seconds at CL8, one level-4 slot (5 -> 4). Bearer and
recipient faction switches preserve the same contribution and original deadline.
Unconscious caster/bearer retain coverage. Original expiry removes all owned
contributions. Actual lethal damage yields Dead and FinallyDead while the living
bearer's original timed circle remains active, retaining caster/CL context.
Exact final actor/area/buff cleanup and no-save-write checks PASS.

Persistence, temporary unload/scene reload, permanent despawn, stronger equipment,
other casters and alignment combinations, learning/acquisition and remaining
control deliveries still need their own qualification. Bearer death remains an
owner rules-adaptation decision; artwork remains unapproved. No release claim.

## Checkpoint C: persistence and bearer death

See [curated evidence](../reports/magic-circle/EVIL-PERSISTENCE-AND-DEATH.json)
for exact per-run package/DLL identities. Native casts saved two independent
casters on one bearer (CL8 Extended and CL10 normal), exact spent slots/known
spell IDs and a pre-existing domination. Four fresh startup combinations kept
original deadlines and ordinary defenses; only the shared enhancement controlled
new control protection. Content OFF hydrated known spells and active circles
without publishing or allowing new casts.

A native scene reload passed using four temporary traveling references, removed
without saving; exact original contexts survived while native area IDs rebuilt.
The initial non-party scene fixture disappeared during native reload and was
reported FAIL, diagnosed and corrected only in fixture setup. This does not
qualify every non-party unload/transition case. Native cleanup then wrote one
exact working save; another fresh load passed absence. The working fixture is
now clean. Original settings bytes were restored after every phase.

Clean validation, 1,657 domain tests (all 8,192 settings combinations), Release
compilation, strict package/icon gates and focused request tests pass. Current
checkpoint archive: `artifacts/magic-circle/checkpoints/persistence/`. The family
remains incomplete, and none of this evidence grants native UI acceptance or
uninstall safety.

## Checkpoint D: four-variant integration in progress

The shared factory now registers four independent spells, held-touch deliveries,
carrier/area/recipient identities and scrolls (24 stable identities). Native
standard-action sticky touch retains the original caster; no selector bundles
known spells. Opposing descriptors use Kingmaker enum names Good/Evil/Chaos/Law.
PF1 recheck confirms no spell resistance for the implemented defensive effect;
the spell-resistance exception belongs to the deferred summoned-creature barrier.
A hostile touched bearer receives the native harmless Will-negates check, while
entry and re-entry cause no saves. Native radius preview is ten feet.

Native Cleric, Wizard/Sorcerer and Inquisitor lists receive all four at level 3;
Paladin receives Evil/Chaos. Actual filtered specialist lists retain their school
restrictions. Narrow verified CotW primary-book and Abjuration-implement adapters
cover entitled installed classes; Medium is absent in the inspected installation.
No Witch, Bard, Bloodrager, Magus or Psychic entitlement is invented. Publication
reconciles after optional mods load and rolls back only owned entries/components.

Scrolls retain native level-3/CL5, 375-gp, single-charge economics and independent
CopyScroll associations. Existing supplier selection and finite five-scroll batch
conventions are reused, with a separate minimal serialized grant marker so saved
merchants do not depend on Teleportation being enabled. Native acquisition,
scribing, consumption and saved-marker qualification are still pending.

All four exact owner-approved exports are installed through the existing icon
cache/consumer mapping pipeline. Each alignment intentionally shares its painting
across the spell, held touch, carrier, recipient and scroll; area blueprints have
no icon surface. Native UI acceptance remains pending. Catalog validation and
all 24 icon-corruption tests pass; protected assignments remain unchanged.

The installed-reference compile passes. Full validation/build/package and native
qualification of this integrated candidate are next. The expanded guarded fixture
requires actual Dominate Person casts, observed failed Will saves and real
RuleApplyBuff results for protected and equivalent positive controls for all four
alignments. No native acceptance of the new integrated artifact is claimed yet.

Integrated family candidate 1: clean full 1,657-test/Release/package PASS.
Native `20260920T0208080145644Z-disposable-magic-circle-evil` FAIL after 23
passing assertions. Native held touch, original caster/CL/duration, one slot,
coverage, existing-control preservation and Evil actual Dominate Person
positive/matching/wrong/neutral outcomes passed. The fixture then incorrectly
constructed MechanicsContext with a null blueprint, before testing the other
three variants. No save writes; exact original world cleanup passed.
Package `97e28a9f2303b357bbc0d637eedc069705c660a739b28a2c48e498288cf38e89`;
DLL `29de574790282ca518a6eb4c2c146a5b06468b9c21de9cc5c7692a473ec902eb`;
MVID `dcb582a4-2ede-4a7c-8f5e-ac22aeb5ca4b`.

Installed IL requires a nonnull associated blueprint. Terminal-only contexts
now use the actual native terminal buff blueprint. A null constructor caster
also substitutes the owner, so that is not an unresolved-source test. The new
fixture assembles a pending native terminal context while its request-local
controller exists, permanently removes that controller, verifies MaybeCaster
is null, then invokes the native pending RuleApplyBuff boundary. It changes no
rule result, catalog metadata or shared source policy. The recipient temporarily
has the matching alignment to catch any accidental target substitution.

Vendor definitions now follow the existing Teleportation hydration contract:
finite stock definitions remain registered OFF, preserving native purchase
memory. New casting, learning and saved-vendor migration remain gated; native
OFF/ON stock persistence qualification remains pending. Existing inert scrolls
and known spells may remain visible OFF, as in the established module model.

Four-variant casting checkpoint: `20260920T0221538969494Z-disposable-magic-circle-evil`
PASS 61/61 on the exact context-corrected candidate. See
[FOUR-VARIANT-NATIVE-CASTING.json](../reports/magic-circle/FOUR-VARIANT-NATIVE-CASTING.json)
for identities, assertions and limits. Native held touch, one slot, typed defenses,
actual Dominate Person failed-save/blocked and positive controls, terminal-only
classification and prior Evil lifecycle checks pass. The unresolved-source test
currently reaches the native pending RuleApplyBuff boundary after permanent
controller removal. Public AddBuff performs another CloneFor that can replace a
missing caster with the owner; explicitly test that upstream path next. This
checkpoint does not accept every unresolved-source delivery or acquisition/UI.

Full clean 1,657-test suite, all 8,192 settings combinations, Release and strict
package pass. Exact archive: `artifacts/magic-circle/checkpoints/family-context/`.
Acquisition, final persistence/profile combinations and native UI remain open.

Public missing-source regression is now an explicit native test for all four
circles and all four independently cast native Protection spells. It first
assembles a real pending context, destroys its disposable controller, then uses
public BuffCollection.AddBuff. The observer distinguishes a genuinely missing
parent source from the native clone's owner fallback. It expects fail-open and
actual application, while separate real matching-controller casts must still
be vetoed. No production source-resolution change has been made yet; qualify
this regression before choosing a shared correction.

`20260920T0237444408343Z-disposable-magic-circle-evil` confirmed the public
source-loss regression: FAIL, 69/77 PASS. The eight failures are exactly the
four circles and four native individual Protection wards under public AddBuff
after controller destruction. Each saw `nativeOwnerFallback=True`, one actual
RuleApplyBuff, and `CanApply=False` instead of the required fail-open result.
Native individual casts retained their original level-1 slot debit and 1 minute
per CL; real matching-controller casts still blocked. Exact cleanup and no save
writes passed. This is a pre-existing shared-source bug, not a separate circle
policy defect. Package `7ff23a1a54df3fc3544531c54a8c62991936a205a5706fad5975707ec5f00856`,
DLL `58a2d02baa8089675b40b904fd281461c6e6f4e255265900b0d0e1f9a6f767ef`,
MVID `33475602-05cf-4d55-b85f-d185efc410de`.

A narrow shared resolver now rejects the native recipient fallback when its
plain buff-clone parent has lost the controller. It stops at actual ability
execution contexts, preserving a summoned creature's own cast source. Catalog,
alignment predicate, trusted metadata, settings and RuleApplyBuff initiator
contract remain unchanged. Native qualification of the correction is pending.

Shared-source green checkpoint: `20260920T0253030021863Z-disposable-magic-circle-evil` PASS 77/77. All eight previously failing public missing-controller cases now apply as required, while real matching-controller casts still block and native individual spells retain original slot/duration behavior. Exact cleanup and zero save writes pass. See [shared regression evidence](../reports/magic-circle/SHARED-SOURCE-REGRESSION.json). Clean 1,657-test/8,192-settings Release/package gates pass; the exact source gate rejects six corruption cases. Archive: `artifacts/magic-circle/checkpoints/shared-source/`. Acquisition, broader delivery/lifecycle cases, final profiles and native UI remain open.

Acquisition qualification is now implemented in the existing guarded disposable circle scenario. It uses native shared-table stock, real gold purchases, CopyScroll inventory actions, public item casting, ordinary prepared spells, specialization eligibility and production publication rollback on isolated native blueprint objects. The request temporarily owns a fresh native SharedVendorTables object, restores its original readonly field reference, and restores inventory/money and both Magic Circle and Teleportation grant state. No save operation is added. Installed-reference compilation passes; full build and native acquisition results are pending.

Acquisition checkpoint passed: `20260920T0312194897210Z-disposable-magic-circle-evil`, 107/107 native assertions (30 acquisition/publication plus 77 regression). Actual purchased scrolls learn exactly one canonical spell, native item use consumes one scroll with CL5/3000 seconds and no book debit, entry/re-entry costs nothing, and copied spells spend one prepared slot. Native finite vendor buyout/revisit, Abjuration favorite eligibility, duplicate publication, cache invalidation, foreign replacement preservation and exact inventory/money/vendor/ledger cleanup pass. Clean full 1,657-test/8,192-settings Release/package PASS. See [curated native acquisition evidence](../reports/magic-circle/NATIVE-ACQUISITION.json); archive `artifacts/magic-circle/checkpoints/acquisition/`. Broader control/lifecycle deliveries, full persistence/profiles and native learning/UI remain open.

Next native regression targets original cast attribution after permanent source removal. Four real circles from the second caster exercise mixed-alignment typed stacking, permanent native destruction, re-entry and the actual AddAreaEffect unload/reconstruction callbacks. Installed IL shows SpawnAttachedToTarget uses MechanicsContext.CloneFor, which can substitute owner when the source no longer resolves. The test requires the stored original caster ID, CL and deadline on each rebuilt area/recipient. No production context change has been made; qualify this exact path before selecting a correction. These scoped callbacks are not labeled actual scene travel.

`20260920T0325278816184Z-disposable-magic-circle-evil` confirmed the original-caster propagation bug: FAIL, 111/119 assertions PASS. Exactly eight rows failed: all four new recipients after re-entry acquired the recipient source ID, and all four reconstructed areas/recipients acquired the bearer source ID. Original carrier IDs, CL/deadlines, mixed typed stacking, other ownership/cleanup and all 107 prior regressions passed; no save writes. Archive: `artifacts/magic-circle/checkpoints/removed-source-regression/`. Package `8de48b16ced7012dc8fd54cfb544c149927698713197c4c3724910742e76252d`, DLL `8b41aaef36ac3d40da10820fddf1fb9dd23ee67d620801b687d24b9d9c8b1fce`. A narrowly scoped native-reference copy for owned Circle contexts is next; no custom saved tracking or control catalog is needed.

Cast-context correction passed: `20260920T0337086796527Z-disposable-magic-circle-evil`, 120/120 native assertions. All eight red attribution cases now retain the original stored caster ID after permanent removal, and foreign same-GUID objects retain native behavior. Four mixed-alignment casts debit four slots, retain +2 typed stacking, and exact one-carrier removal preserves the other three. Cleanup and zero save writes pass. Clean 1,657-test/8,192-settings Release/package gates pass. See [curated red/green evidence](../reports/magic-circle/CAST-CONTEXT-REGRESSION.json); exact archive `artifacts/magic-circle/checkpoints/cast-context/`. These scoped native unload callbacks do not replace final actual scene/save qualification.

Additional guarded delivery coverage now uses actual native equipment, public excluded/optional terminal applications, UnitEntityData.Translocate and a native RuleSummonUnit-created Succubus whose own domination is cast through UnitUseAbility. Its natural Chaotic Evil alignment differs from the Lawful Good summoner. This does not claim a summon spell resource test or actual area Confusion casting; it exercises the shared application boundary without affecting campaign actors. Full source and runtime qualification are pending.

Additional-delivery candidate 1: full clean 1,657-test/Release/package PASS; native `20260920T0351141221915Z-disposable-magic-circle-evil` FAIL on a fixture internal-name lookup before new delivery tests (66 earlier assertions including exact cleanup PASS; no save writes). No new mechanic result was accepted. Exact archive `artifacts/magic-circle/checkpoints/deliveries-lookup-failure/`, package `c0fdb437593ce2bf687ff972bf59103d417e6e6e58f879089e5ddf58d16c6523`, DLL `01b217182bd4ca83567dfa5b217b6a97a27b682657958991836f04bd6e35c856`. Native spell identities are now resolved by the exact IDs corroborated in the local CotW source and installed lookup, with names recorded diagnostically.

Additional delivery checkpoint: `20260920T0401029525146Z-disposable-magic-circle-evil` PASS 204/204 (84 new, 120 regression). Excluded native terminals apply; all three installed optional control terminals block matching and apply from neutral controllers; native +3 ring/cloak bonuses survive all four weaker circles and their removal; native same-scene translocation preserves carrier deadlines. Real Succubus domination uses the summoned controller's Chaotic Evil alignment instead of its Lawful Good summoner: Evil/Chaos block, Good/Law and unprotected cases apply after observed failed saves. Native summon coverage and permanent bearer despawn cleanup pass. Clean full validation/1,657 tests/Release/package PASS. See [curated evidence](../reports/magic-circle/ADDITIONAL-NATIVE-DELIVERIES.json); archive `artifacts/magic-circle/checkpoints/deliveries/`. Native UI, learning and final persistence/profiles remain open.

Touch/metamagic/coexistence checkpoint: `20260920T0505176211970Z-disposable-magic-circle-evil` PASS 231/231 (27 new, 204 regression). Real communal Protection retains level-2 cost and one-minute duration; exact circle removal preserves communal and native Paladin recipients, with real incoming-control positives after the last source is removed. Fresh hostile touch naturally misses and retains its charge; retry consumes no extra slot, passed Will negates the bearer application, failed Will applies once, and entry causes no saves. All four circles pass native Extend/Quicken/Heighten/Reach adjusted slot/action/context/duration checks. Reach follows the installed constructor conversion and native ranged-touch/projectile path with isolated synchronous projectile ticks, not forced outcomes or real-time animation claims. Full clean validation/1,657 tests/8,192 settings/Release/package PASS. Three fixture failures and one intermediate diagnostic compile error remain recorded in [curated evidence](../reports/magic-circle/TOUCH-METAMAGIC-COEXISTENCE.json). Archive `artifacts/magic-circle/checkpoints/touch-metamagic/`. Native UI, committed learning, expanded persistence and final profiles remain open.

Native UI/learning checkpoint: `20260920T0610338063870Z-disposable-magic-circle-ui` PASS 56/56. Real Sorcerer 5-to-6 cancel/reopen/commit learns one chosen Circle, whose cast spends one slot. Four native prepared spellbook entries, actual held-delivery popup icons, eight carrier/recipient rows with full tooltips, and four scroll inventory/description consumers pass; exact world/inventory/UI cleanup and zero save writes pass. Twenty selected actual native screens were visually inspected; all twenty visible consumer identities are covered. Native untimed recipient rows say Permanent, with explicit Within Circle title and proximity tooltip; carrier rows show their original remaining time. Exact export owner approval remains separate. Five fixture failures are retained in [curated evidence](../reports/magic-circle/NATIVE-UI-AND-LEARNING.json). Full clean validation/1,657 tests/8,192 settings/Release/package PASS; archive `artifacts/magic-circle/checkpoints/native-ui/`. Remaining checks include pets/real-wall geometry, expanded four-variant persistence/scroll/vendor/held hydration, optional profiles and the final frozen candidate.
