# Overnight Gunslinger Bug-Fix Mission

Status: ACTIVE

## Controlling work order

This document preserves the complete operational scope and stopping contract
issued by the user on 2026-08-19 for the Codex Autonomous Overnight Mission:
Kingmaker Gunslinger Bug-Fix Batch. The fresh human findings in this contract
override older automated qualification wherever they conflict.

Repository:

- Root: C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger
- Remote identity: howardreith/KingmakerGunslingerAndOtherTabletopRules
- Required branch: codex/gunslinger-overnight-bugfixes
- Verified fetched baseline: d13268d3abe9ffe89c8195b213c1eee194328672
- Starting version: 0.0.87-urban-barbarian-human-review-repair-4

Work continuously through Issues 1 through 12 in order. Each issue receives a
separate honest commit, all required source/build/package/runtime qualification,
approved-helper publication, exact remote equality verification, and immediate
continuation. A failed experiment, blocked issue, visual/audio human gate,
runtime failure, or context compaction is not a mission stop.

Never work on master/main, merge, force-push, rebase published work, rewrite
history, raw-push, bypass or edit the external helper, destructively reset or
clean, mutate protected saves, commit proprietary binaries or machine-local
configuration, install prohibited software, accept licenses, or redistribute
unlicensed/proprietary assets. Every publication uses exactly:

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1

If that helper rejects the exact branch because it is not allowlisted, record
the rejection and stop without workaround. After every publication, local HEAD,
the local branch ref, and origin/codex/gunslinger-overnight-bugfixes must be the
same SHA.

The ordinary worktree contained unrelated untracked tmp-phase0-clone content.
It was preserved untouched. This mission uses the isolated repository-local
worktree .worktrees/gunslinger-overnight-bugfixes.

## Durable continuity

Maintain this mission, planning/OVERNIGHT-GUNSLINGER-BUGFIX-MATRIX.md,
OVERNIGHT-GUNSLINGER-BUGFIX-JOURNAL.md,
OVERNIGHT-GUNSLINGER-BUGFIX-IMPLEMENTATION-REPORT.md,
docs/OVERNIGHT-GUNSLINGER-BUGFIX-QUALIFICATION.md,
docs/OVERNIGHT-GUNSLINGER-BUGFIX-HUMAN-ACCEPTANCE.md,
AUTONOMOUS-RESUME.md, AUTONOMOUS-BLOCKERS.md, and the existing Gunslinger
coverage/fidelity ledgers.

At every meaningful checkpoint record branch/SHA, version, completed/current
issues, current hypothesis, exact commands/results, runtime IDs, relevant
package/DLL/AssetBundle/SoundBank hashes, real blockers, and next action.
Preserve historical statements rather than rewriting them. After compaction,
reread the overnight mission, matrix, journal, report, blocker, and resume
records, select the next unfinished safe action, and continue.

The supplied ChatGPT URLs are supplementary only. Failure to open them is not a
blocker; repository acquisition/location records, current source, local asset
evidence, and this contract remain authoritative.

## Per-issue workflow

For each numbered issue, in order:

1. Inspect relevant source, tests, current reports, qualification, installed
   Kingmaker 2.1.7b API evidence, and recent history.
2. Establish the failing boundary with reproduction or narrow diagnostics.
3. Add a focused behavioral/integration regression test where representable.
4. Implement the narrowest evidence-supported repair without unrelated
   refactoring or stable-ID changes.
5. Run focused tests, repository validation, the complete domain/reflection
   suite, clean exact-reference Release build, build-output validation,
   package/SoundBank/AssetBundle validators as applicable, strict package
   validation, git diff --check, staged-diff and forbidden-file audits, and the
   narrowest safe guarded runtime scenario.
6. Update the durable matrix, journal, report, qualification, human acceptance,
   and relevant product documentation.
7. Commit only the current issue, publish with the approved helper, verify exact
   remote equality, and continue.

Do not create a misleading fix commit for unresolved behavior. Commit safe,
useful diagnostics/tests/docs under an honest test/docs/investigate message,
mark the exact blocker, and continue.

## Issue 1 - Acadamae Graduate action economy, save, and fatigue

Preserve the existing per-character Use Acadamae Graduate toggle and stable
identities. OFF means native casting time and no Acadamae save/fatigue. ON
accelerates only an actual prepared arcane spellbook cast by the exact feat
owner when it is Conjuration, has the exact Summoning descriptor, and its
effective pre-Acadamae time is longer than Standard. The ordinary one-round/
full-round summon becomes Standard in both UI and execution, never Swift,
Move, Immediate, or Free.

Only a completed cast that actually received acceleration makes exactly one
native Fortitude save at DC 15 + spell level using the real caster modifier and
native rule path. Diagnostics/combat-log evidence must expose roll, total, DC,
success/failure, and fatigue disposition without unrelated spam. Success causes
no fatigue. Failure applies canonical ordinary Fatigued independently of the
spell/summon MechanicsContext, persists through summon expiration and area
transition, and remains until native removal such as rest. Do not fake it with
an arbitrary long duration.

No save from UI/tooltip probes, cancellation, interruption/failure, ineligible
casts, already-Standard casts, scrolls, wands, item abilities, spell-like or
supernatural abilities, or spontaneous casting. Deduplicate callbacks. Test
forced success/failure, displayed and executed action, exactly one save,
persistence/rest removal, OFF and ineligible controls, and Cord-equipped and
unequipped behavior. The Cord may substitute the consequence but may not hide
the attempted save/fatigue application.

Suggested commit: fix(acadamae): restore accelerated casting save and fatigue behavior

## Issue 2 - Focused Aim Grit transaction

Preserve the established Mysterious Stranger Focused Aim action, duration,
damage, and firearm restriction. A successful ordinary use spends exactly one
point from the shared live Mysterious Stranger Grit resource unless a valid
current True Grit selection legally reduces its effective cost. Bonus and spend
are one transaction: no bonus if spend fails, no negative Grit, no canceled/
rejected spend, and no duplicate-callback double-spend. Repeated legal uses
spend once each and the visible counter reads the same authoritative resource.

Test positive, exactly-one, zero, repeated use, duplicate callbacks, True Grit
selected/unselected, wrong archetype/weapon, damage, rollback where relevant,
save/load, and respec/reconciliation.

Suggested commit: fix(mysterious-stranger): spend grit transactionally for Focused Aim

## Issue 3 - Touch AC and penetration feedback

Early firearms use touch AC inside the exact first effective range increment and
ordinary AC outside it. Preserve the distinct project advanced-firearm rule and
use effective range after legal modifiers such as Steady Aim. Prove inside,
exactly-at, and outside boundaries for Pistol, Musket, direct-fire Blunderbuss,
Rifle, and Revolver as applicable. Scatter/cone remains separate and does not
gain touch AC from a direct-fire shortcut.

Touch AC does not bypass concealment, Mirror Image, cover, line of sight/effect,
range penalties, natural 1, critical confirmation, or damage. Fail closed to
ordinary AC when firearm identity, distance, or attack identity is unavailable.
Revalidate authoritative distance at attack resolution for queued/moving units.

Every firearm help/tooltip states the exact base penetration rule/distance.
Boundedly investigate existing hover/preview/inspection/cursor/combat-tooltip
surfaces for concise Touch AC in range / Normal AC outside range status without
enemy AC numbers. If no safe pre-attack seam is proven after distinct attempts,
use truthful item text plus a non-spammy resolution log with actual distance,
effective range, and Touch/Normal branch; record the UI follow-up. Add decision
service tests and guarded live evidence of the actual AC branch.

Suggested commit: fix(firearms): qualify touch AC range and expose penetration feedback

## Issue 4 - Acadamae prerequisite presentation

Distinguish native prerequisite rendering from embedded manual prose. Preserve
all real prerequisite components and native failure messages, but remove only
redundant description/localization copy. The selection UI must show one coherent
prerequisite presentation. Preserve identity, alphabetical placement, existing
ownership, module toggles, and Call of the Wild compatibility. Add a narrow
semantic presentation test.

Suggested commit: fix(acadamae): remove duplicated prerequisite copy from feat text

## Issue 5 - Oleg maintenance stock

Use exact Oleg installed table GUID/type/owner evidence and the existing bounded
transactional publication architecture. Publish exactly Firearm Repair Kit x5
and Overhaul Kit x2. Preserve every native/foreign entry and order. Repeated
publication is idempotent and normalizes only stale/duplicate/wrong-count
project entries; later initialization failure restores the exact snapshot.
Do not add firearms, magic firearms, or unrelated capital stock. Prove the live
blueprint and, where safe, actual disposable/new-game materialization. Document
old-save limits without claiming already-instantiated refresh.

Suggested commit: fix(vendors): stock firearm maintenance kits at Olegs

## Issue 6 - Bokken ammunition stock

Paper bullets means the existing Paper Cartridges identity. Resolve Bokken's
exact installed table/lifecycle without GUID guessing or display-name-only
publication. Reuse the vendor observer and transactional style. Publish exact
project-owned Black Powder x100, Lead Balls/Bullets x100, and Paper Cartridges
x100 once each, preserving unrelated order, idempotence, and rollback.
Do not restore Jhod stock or duplicate capital/BTSL logic. Renew the prior
bounded investigation using current integrated source/runtime evidence. Only
genuinely unresolved exact-table ambiguity after distinct safe strategies may
block this issue; continue afterward. Prove actual inventory where safe and
document old-save materialization limits.

Suggested commit: fix(vendors): stock firearm ammunition supplies at Bokken

## Issue 7 - Border Sentinel later placement

Resolve Border Sentinel's exact blueprint, power/price, acquisition path, and
all publication references. Remove future deliberate Oleg acquisition and
place it exactly once in a later deterministic base-campaign source appropriate
to its power, preferably a thematic frontier/border/patrol/guard/officer/
military-cache/fortification target supported by exact evidence.

Prefer an exact unique fixed container, then exact named-unit fixed loot. Reject
random loot, artisan rewards, broad hooks, DLC-only, multiply reused tables,
dialogue grants, and targets already holding a project unique. Preserve all
contents and use an idempotent rollback-safe exact-item transaction. Never
delete from existing players, companions, stash, drops, sales, or saves.
Document static/new/not-yet-instantiated semantics and add a development-only
location observer plus concise human route.

Suggested commit: fix(loot): move Border Sentinel to later organic campaign loot

## Issue 8 - Native firearm sounds

Treat missing Pistol/other sounds as a real regression. Audit integrated audio,
manifests, prior Wwise records, bank staging/loading, event posting, emitters,
packaging, and deployment. Preserve the qualified native Wwise architecture:
bank KMG_Firearms / file KMG_Firearms.bnk and events
KMG_Firearm_Pistol_Shot, KMG_Firearm_Musket_Shot,
KMG_Firearm_Blunderbuss_Shot, KMG_Firearm_Revolver_Shot, and
KMG_Firearm_Rifle_Shot. Do not use Unity AudioSource.

Audit ordinary, Scatter, Dead Shot, Startling Shot, Menacing Shot, Stop
Bleeding, and newly discovered physical discharge paths. A committed discharge,
including ordinary miss, posts exactly one correct event; empty, Wrecked,
rejected, canceled, rolled-back, probe-only, and true-misfire paths post zero.
Scatter posts once per volley. Audio failure never alters mechanics.

Suppress inherited crossbow sounds only through exact qualified hooks/fields;
do not hide weapon models or remove delivery projectiles. Package only the
allowlisted bank, never Init.bnk/vanilla banks. Verify manifest/hash,
destination, one-time post-Wwise load, playing IDs, emitter readiness, and
fresh processes. Use existing approved recordings/bank; no downloads or Wwise
installation/license acceptance. If regeneration is irreducibly blocked,
complete all code/package/test work and leave only generation/listening gated.
Final human listening covers all five families, empty rejection, and no
crossbow release/bolt sound.

Suggested commit: fix(audio): restore Wwise firearm discharge sounds

## Issue 9 - Native-style distinct firearm feat icons

Inspect real native Weapon Focus, Greater Weapon Focus, Weapon Specialization,
Greater Weapon Specialization, Improved Critical, and Rapid Reload at in-game
scale. Preserve native top-level presentation and non-firearm choices. Give
each custom firearm parameter a project-owned native-style monogram:
Pistol P, Musket M, Blunderbuss B, Rifle Ri or equally distinct abbreviation,
and Revolver Rv or equally distinct abbreviation.

Use an aged/parchment-compatible field, muted lines, native border/wear,
readable 32/64px contrast, and original/project-legal lettering without
proprietary fonts. Replace Rapid Reload's bright modern icon with a native-feat
red/salmon circular field, cream/gold line work, and a simple period reload
symbol. Audit Improved Critical and the Weapon Focus dependent chain without
mutating native icons. Keep editable source and deterministic exports,
provenance, and allowlists. Add semantic publication tests and guarded UI
observation; produce a before/after map and leave aesthetics human-gated.

Suggested commit: fix(icons): align firearm feat choices with Kingmaker visual language

## Issue 10 - Elven Branched Spear scale/grip

This supersedes earlier icon-only scope. Audit item/type, donor visual
parameters, prefab/model, pivot, scale, attach slot, animation, belt/back, and
bundle build. Use exact Kingmaker spear/polearm donor evidence. The world weapon
must have credible native-spear length and a grip pivot that keeps the hand on
the haft through idle/attack. Use native spear/piercing-two-handed hand-slot and
animation or the narrowest proven equivalent; do not spawn an independent model
under guessed skeleton transforms.

Preserve mechanics, reach, damage, critical, proficiency, feat/save identity,
and icon unless directly causal. Do not globally mutate donors/categories.
Normalize source units/transforms rather than hide extreme scale errors.
Preserve provenance, editable source, deterministic bundle, package validation,
and old-save identity. Validate world/inventory dolls, idle, movement, attack,
switching, sheath, male/female Medium, and safe size variation. Automated
evidence cannot replace final human visual acceptance.

Suggested commit: fix(visuals): correct Elven Branched Spear scale and grip

## Issue 11 - Musket/Blunderbuss rig and texture

Preserve mechanics, crossbow-derived delivery, projectile identity, firearm
state, audio, reload, and saves. The held result must be the actual long-gun
model, not a crossbow placeholder. Normalize source units/coordinates. Author
per-gun prefab contracts with firing-hand trigger/stock grip pivot, correct
forward barrel, rearward stock, actual muzzle, native EquipmentOffsets/
IkTargetLeftHand fore-end support, and separate belt/back calibration.

Retain Heavy Crossbow/Crossbow animation as first candidate. Do not remove the
delivery projectile; hide only cloned renderers as already qualified. Re-enable
Musket first, then Blunderbuss. Extend live calibration for position, rotation,
scale, support hand, muzzle, and world/inventory refresh. Repair root/mesh/IK,
not broad race hacks. Retexture with approved/project-owned deterministic assets
so the guns remain distinct, period-appropriate, and Kingmaker-coherent. Record
provenance/modifications and update notices. Validate bundle/blueprint rig plus
world/inventory, movement, firing, reload, switching, back state, muzzle,
support hand, male/female, and safe size variation; final visual acceptance is
human.

Suggested commit: fix(visuals): rig and retexture muskets and blunderbusses

## Issue 12 - Organic magic-item distribution

Audit every project-owned unique/named magic weapon, firearm, armor, wondrous
item, or other deliberately published campaign magic item. Exclude mundane
firearms, ordinary +1 merchant stock, ammunition, crafting supplies, Oleg kits,
and Bokken ammunition. Preserve Issue 7's Border Sentinel result.

Maintain a complete acquisition inventory with item symbol/GUID/name, type,
power/price, current target GUID/name/type/chapter/area, fixed/random/shared/
unique status, existing contents, nearby project uniques, campaign/DLC,
proposed target/theme, selection/rejection evidence, and old-save behavior.
Spread items across chapter-appropriate deterministic base-campaign sources.
Prefer one project unique per exact target, unique containers then named-unit
fixed loot. Avoid clustering. Preserve Rare Firearms progression: earlier
lower-power before Pitax, distinct thematic Pitax sources, deterministic late
capstones, and separately discoverable final-act uniques.

Reject generic/random/artisan/DLC/broad/dialogue/shared/obscure targets. Resolve
exact GUID/type/name/references/chapter/contents for every target. Preserve
native/foreign contents/order; append each exact item once; validate one;
remain idempotent and exactly rollback-safe. Do not retroactively mutate saves.
Add acquisition observers and development-panel audit controls plus shortest
human inspection routes.

Suggested commit: fix(loot): distribute project magic items across organic campaign locations

## Cross-cutting engineering contract

Match existing architecture. Keep Harmony/engine adapters thin and gameplay
policy in narrow testable services. Use real blueprint construction, installed
assemblies, guarded runtime, and transactional fixtures; prefer behavior tests
and mock only irreducible external boundaries. Add no library unless it replaces
substantial code and complies with dependency policy. Preserve every stable
GUID/save identity and append-only manifest rules. Never globally mutate a
native donor. Preserve standalone and optional-mod compatibility. Never reduce
test counts or weaken validators. Classify inherited failures honestly.

Protect KMG_AUTOMATION_BASELINE and all protected saves. Use only authorized
disposable fixtures and guarded Steam App ID 640820 launches. Never launch
Kingmaker.exe directly. Verify deployed DLL/package/bundle/bank hashes before
runtime interpretation. Do not treat builds, structural prefabs, blueprints,
event routing, or screenshots as proof of visible/audible/live behavior.

## Final integration and handoff

After all issues are attempted, audit the matrix for skips; run the complete
final suite, clean Release/output/package/SoundBank/AssetBundle/compatibility
and guarded runtime gates. Choose the next unused patch version from repository
reality; never replace human-tested bytes under an existing version. Update
version, changelog, README, installation/compatibility, architecture/manifests,
reports, qualification, and human acceptance. Produce package/DLL/bundle/bank
hashes and a separate release commit, publish, remote-verify, and leave the
branch clean and unmerged.

The final response contains exact branch/SHA/version/remote/clean status,
artifact paths/hashes, one row per issue with commits/tests/runtime/blockers,
honest unresolved evidence and next bounded action, exact safe installation
instructions, and one consolidated human sequence covering every specified
mechanical, vendor, audio, icon, spear, long-gun, and acquisition check. Do not
ask for human testing earlier.

## Genuine hard stops

Only these stop the whole mission: helper refusal for the exact branch; no safe
baseline/worktree without destroying unrelated work; active same-branch
mutation with no isolation; corruption/divergent stable-ID history/manifest
collision; required credentials/license/prohibited installation/policy bypass/
destructive Git/protected-save mutation/proprietary redistribution; or every
remaining task reduced solely to irreducible human observation after all safe
engineering is complete. A blocked issue is recorded and skipped while the
mission continues.
