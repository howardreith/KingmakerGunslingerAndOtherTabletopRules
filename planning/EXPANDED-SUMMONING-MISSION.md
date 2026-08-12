# CODEX MISSION: Expanded Summon Monster and Summon Nature's Ally

Status: active. Authority received 2026-08-11 from the repository owner.

This is the durable mission and stopping contract for the Expanded Summoning
branch. It preserves the complete user-authorized scope so the work can resume
after context compaction or process restart.

## Mission status and authority

Codex is authorized to independently design, implement, test, qualify,
document, commit, push, and open a draft pull request for a complete Expanded
Summoning feature in
`howardreith/KingmakerGunslingerAndOtherTabletopRules`.

Do not stop for permission, routine design decisions, checkpoints, commits,
failed tests, missing preferred donor assets, implementation uncertainty,
context compaction, or ordinary engineering setbacks. Continue until the
definition of done is satisfied or a catastrophic condition in section 21 is
reached. This authority supersedes stale repository text that prohibits later
sprints or tabletop-to-Kingmaker adaptations. All other safety, evidence, Git,
save-protection, and runtime rules in `AGENTS.md` remain binding.

Decision priority:

1. Save and repository safety.
2. Correct, deterministic game behavior.
3. Preservation of vanilla and third-party content.
4. Pathfinder 1e tabletop fidelity.
5. Consistency with native Kingmaker mechanics and presentation.
6. Visual plausibility using existing assets.
7. Simplicity and maintainability.

When an exact tabletop mechanic cannot be represented safely, use the closest
conservative, non-overpowered behavior, document and test the deviation, and
continue. Do not add unapproved creatures for symmetry.

Network access is limited to this GitHub repository and normal publication,
official/primary Pathfinder 1e rules (especially Archives of Nethys/Paizo), and
documentation for the installed toolchain. Do not access unrelated accounts,
browsers, mail, personal files, password managers, repositories, or software.

## 1. Establish the correct baseline before editing

The repository default branch is not automatically authoritative. The known
minimum integrated baseline is branch `origin/codex/shield-other-spell`, commit
`e4d560f8dd2909518614e3a20e77ba4d70dadeb8`, release line 0.0.77, including
qualified Acadamae Graduate commit
`7ba84439caa1fc92b8c8148ce95ea79fd59bdc57`.

Run the required preflight: repository root, remotes, short status, fetch all
and prune, remote branches containing the minimum SHA, and remote `codex/*`
refs sorted by committer date. Confirm the intended repository; preserve all
uncommitted/unrelated work; use a separate worktree if the current one is dirty
or belongs to another task. Resume coherent `origin/codex/expanded-summoning`
work only when it descends from the qualified baseline. Otherwise branch from
the newest qualified, non-experimental descendant that contains the Shield
Other repair. Never choose backup/forensic/experimental work merely by date.
Never merge to a release branch, force-push, rewrite history, destructively
reset, or destructively clean.

Before summoning work, inspect `ShieldOtherLinkValidityPolicy` and its tests.
An established bond must not depend on distance; close range remains on the
casting ability. Preserve expiration, removal/dispel, dead/missing endpoints,
and area separation. Reuse a qualified descendant containing the repair or
implement and qualify the repair in a separate coherent commit, then continue.

Use a dedicated branch/worktree. A recovery branch is allowed only when the
existing branch cannot be used without destructive history changes, and the
reason must be recorded. After every coherent commit and before any pause,
compaction, or handoff, run exactly:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1
```

## 2. Persist the mission

Before substantive edits create and maintain:

- `planning/EXPANDED-SUMMONING-MISSION.md`
- `planning/EXPANDED-SUMMONING-ROSTER.md`
- `planning/EXPANDED-SUMMONING-FIDELITY-MATRIX.md`
- `EXPANDED-SUMMONING-JOURNAL.md`
- `EXPANDED-SUMMONING-IMPLEMENTATION-REPORT.md`
- `EXPANDED-SUMMONING-STATE.json`
- `AUTONOMOUS-RESUME.md`
- `AUTONOMOUS-BLOCKERS.md` only for genuine hard stops

State must record the baseline branch/SHA, working branch/current SHA, release
version, completed phase, next action, domain/build/package results, runtime IDs
and result paths, compatibility profiles, unresolved deviations, push status,
and draft PR number or blocker. Update journal/state after meaningful phases. A
report or checkpoint is never permission to stop.

## 3. Feature objective

Create a fourth independent, default-enabled, restart-bound feature:

- machine ID: `expanded-summoning`
- player-facing name: Expanded Summoning

When enabled, additively expand Summon Monster I-IX and Summon Nature's Ally
I-IX with the approved rosters and higher-level quantity choices. When disabled,
publish no new KMG variants into live parents, leave all vanilla/third-party
options untouched, keep every KMG identity registered so old saves and active
summons deserialize/expire safely, and leave the other modules independent.

Frozen catalog invariants:

- 66 Summon Monster family/tier entries
- 57 Summon Nature's Ally family/tier entries
- 67 unique creature keys after cross-family sharing
- 361 Summon Monster placements
- 320 Summon Nature's Ally placements
- 681 total logical placements before native-equivalence deduplication or
  proven safe ability reuse

These are generator/validator invariants, not a demand for 681 distinct IDs.

Explicit non-goals: do not import every supplement creature; add aquatic-only
summons, horses/ponies, unapproved ants, apes, rhinoceroses, giants, or extra
dinosaurs; remove existing such creatures; create companions, pets, inventory,
vendors, encounters, quests, or campaign-spawned forms; import external models
or proprietary copied assets; add a dependency; redesign spellbook UI; or alter
unrelated spell/non-summoning feature balance.

## 4. Pathfinder rules contract

Use official Pathfinder 1e as rules truth and native Kingmaker mechanisms for
adaptation. Common behavior: conjuration (summoning); native one-round casting;
close initial placement; one round/caster level and dismissible through native
duration; legal selected location; acts immediately on the caster's turn;
native faction/control/command; cannot summon/conjure, teleport, plane travel,
or use an expensive-material spell/SLA; grants no XP, loot, quest progress,
dialogue, permanent inventory, corpse persistence, or companion slot; and
cleans up safely on expiration/dismissal, save/load, area transition, rest,
caster death, and process restart.

For family tier N: tier I offers one tier-I creature; tier II offers one tier-II
or 1d3 of one tier-I kind; tier III offers one tier-III, 1d3 of one tier-II
kind, or 1d4+1 of one tier-I kind; tiers IV-IX offer one current-tier, 1d3 of
one immediately preceding-tier kind, or 1d4+1 of one approved kind at least two
tiers lower. Packs are always same-kind. Preserve native quantity options and
add only missing KMG options, exactly once in each final live parent.

Templated natural Summon Monster creatures are celestial for good casters and
fiendish for evil casters; neutral casters can choose where safely exposed.
Alignment and descriptors follow native/tabletop rules. Outsiders/elementals
with their own subtype receive no celestial/fiendish template. Preserve opposed
descriptor restrictions and reuse native template/alignment mechanisms.
Nature's Ally never applies those templates; non-alignment-subtype creatures
match caster alignment and descriptors follow caster/element subtype through
native mechanics.

## 5. Approved Summon Monster roster

- I: Dog (templated); Eagle (templated, Roc visual); Poisonous Frog
  (templated, scaled Giant Poisonous Frog visual).
- II: Wolf, Giant Centipede, Giant Frog, Giant Spider, Goblin Dog (Worg
  visual), and Hyena (Wolf visual), all templated; Small Air, Earth, Fire, and
  Water Elementals, not templated.
- III: Boar, Leopard, Monitor Lizard, Cheetah (Leopard visual), Crocodile
  (Monitor Lizard visual), Dire Bat (Roc visual), and Wolverine (Worg visual),
  all templated; Lantern Archon (Will-o'-Wisp visual), not templated.
- IV: Dire Boar, Dire Wolf, Grizzly Bear, Lion (Leopard visual), and Pteranodon
  (Roc visual), all templated; Hell Hound; Medium Air/Earth/Fire/Water
  Elementals; Air/Earth/Fire/Water Mephits.
- V: Bralani Azata; Large Air/Earth/Fire/Water Elementals; Dire Lion
  (templated, Smilodon visual); Ankylosaurus (templated, Hodag visual);
  Salamander (Lizardfolk visual).
- VI: Dire Bear, Dire Tiger/Smilodon, and Elephant (Mastodon visual), all
  templated; Erinyes Devil; Huge Air/Earth/Fire/Water Elementals; Invisible
  Stalker (Air Elemental visual); Shadow Demon (Soul Eater preferred, Ankou
  fallback); Succubus (Nymph preferred, Tiefling fallback).
- VII: Mastodon and Roc, templated; Greater Air/Earth/Fire/Water Elementals;
  Bebelith (enlarged Doomspider visual, replacing spider poison/web with safe
  armor-rending and demon-hunting behavior).
- VIII: Elder Air/Earth/Fire/Water Elementals.
- IX: Ghaele Azata.

Reuse and validate a correct native option instead of publishing a duplicate.

## 6. Approved Summon Nature's Ally roster

- I: Dog; Eagle (Roc visual); Giant Centipede; Poisonous Frog (scaled Giant
  Poisonous Frog visual).
- II: Small Air/Earth/Fire/Water Elementals; Giant Frog; Giant Spider; Goblin
  Dog (Worg visual); Hyena (Wolf visual); Wolf.
- III: Boar; Cheetah (Leopard visual); Crocodile (Monitor Lizard visual); Dire
  Bat (Roc visual); Leopard; Monitor Lizard; Wolverine (Worg visual).
- IV: Dire Boar; Dire Wolf; Medium Air/Earth/Fire/Water Elementals; Grizzly
  Bear; Lion (Leopard visual); Air/Earth/Fire/Water Mephits; Pteranodon (Roc
  visual).
- V: Ankylosaurus (Hodag visual); Dire Lion (Smilodon visual); Large
  Air/Earth/Fire/Water Elementals.
- VI: Dire Bear; Dire Tiger/Smilodon; Huge Air/Earth/Fire/Water Elementals;
  Elephant (Mastodon visual).
- VII: Greater Air/Earth/Fire/Water Elementals; Mastodon; Roc.
- VIII: Elder Air/Earth/Fire/Water Elementals.
- IX: Pixie with irresistible dance and sleep arrows. Prefer Pixie/Nixie or a
  compatible small fey visual; use Nymph only after scale, rig, attacks,
  casting, death, and navigation validation. Create no permanent ammunition or
  transferable loot.

## 7. Forensic inventory before implementation

Check in an inventory of all native SM/SNA parents with GUID, name, spell
level/lists, variants, components, descriptors, material data, pools, duration,
spawn actions, and caches; shared/cloned/replacement parents; native equivalents;
and every donor/view with GUID/name, prefab/scale, size/footprint/reach,
movement, attacks/equipment, animation paths, AI/faction, abilities, and all
loot/XP/inventory/interaction/quest/dialogue/cutscene/area/companion/persistence
components. Inventory native template/descriptors, multiple summon actions,
Augment/Superior/Sacred Summons, cleanup, and Acadamae recognition; supported
optional-mod structures without compile references; manifest/registry counts;
and all explicitly enumerating project files. The final loaded library and
installed 2.1.7b assemblies are authoritative. Record ambiguous donor evidence
and use approved fallbacks without asking.

## 8. Required architecture

Use immutable, data-driven specifications for family, multiplicity, creatures,
placements, stable symbols, donors, tabletop profile, policies, and adaptations;
a self-validating catalog; pure generation/merge/publication policies; a runtime
builder/registrar; transactional idempotent final-live reconciliation; explicit
summon sanitization; and separate compatibility discovery. Remain on .NET
Framework 4.7/C# 7.3 and explicitly add sources to runtime/domain-test project
files. No new package/test framework.

A development-time tool may expand the roster, allocate random GUIDs once,
emit/validate checked-in data, and detect duplicate symbols/GUIDs, missing
types/variants, and stale generation. Runtime must never derive, generate,
hash, or repair IDs. Share normalized units only when mechanics match; share
abilities only when parent context, level, metamagic, duration, UI,
availability, and save compatibility are proven.

## 9. Blueprint identity and registration

Add stable `KMG.Summoning.*` symbols with lowercase 32-character GUIDs and exact
planned types to `blueprints/blueprints.json`. Never reuse/delete retired IDs;
reserve abandoned ones. Registration must be deterministic and constant in all
16 module combinations; update exact active/reserved ledger totals and bootstrap
expected count, preferably through a feature-local invariant. Register all IDs
even while publication is disabled. Before commit, collision-check repository,
loaded game, feature, and supported optional-mod blueprints.

## 10. Clone and normalize summon units safely

Never summon a live campaign enemy unless proven to be a dedicated safe native
summon. Deep-clone any array/list/component that will change; prove donors remain
structure-equivalent after initialization. Use native summon faction/control,
pools, duration, and cleanup. Strip XP, loot, permanent inventory, dialogue,
interactions, quests/cutscenes/areas/stories, companion/pet progression,
map/kingdom hooks, teleport/planar travel, summon/conjuration, expensive
component spells/SLAs, corpse persistence, and campaign scripts/facts. Preserve
only approved combat profiles. Repeated casts must not leak units, views,
commands, buffs, inventory, or subscriptions. Disabled-module save loads must
remain safe. Units cannot be recruited, looted, spoken to, used as companions,
or retain permanent effects.

Every fidelity row must cover type/subtypes/alignment; size/reach/speed/movement;
abilities, HD/HP, BAB/CMB/CMD; AC/touch/flat-footed; saves; attacks/damage/crit;
feats/maneuvers; DR/resistance/immunity/vulnerability/SR; senses; Ex/Su/SLA;
uses/DCs; templates; and explicit adaptations/omissions. Prefer official values
unless Kingmaker has a global deliberate adaptation; do not inherit unrelated
enemy scaling.

## 11. Required special adaptations

- Lantern Archon: dual safe ranged-touch light rays, archon defenses/aura when
  safely available; no greater teleport. Wisp is visual only.
- Elementals: correct tier size, subtype, movement, attacks, defenses,
  immunities, and specials; prefer dedicated summons and strip enemy scaling.
- Mephits: four distinct variants with correct breath/damage/save scaling,
  traits, resistances/immunities/vulnerability, practical fast healing, and no
  conjure/planar powers.
- Salamander: Lizardfolk visual only; spear/tail, heat, grab/constrict, defense,
  fire subtype where supported; no inventory/drops.
- Invisible Stalker: Air visual only; attack-safe permanent invisibility, slams,
  air defenses, safe tracking where supported; no teleport.
- Shadow Demon: Soul Eater then Ankou visual; incorporeal/shadow defenses and
  core offense. Possession only if duration-bounded and save/load-clean;
  otherwise document omission. No teleport/summoning.
- Succubus: Nymph then Tiefling visual; bounded charm/energy drain; no lasting
  profane gift, teleport, or summoning.
- Bebelith: enlarged Doomspider visual only; remove poison/web; safe bounded
  armor dismantling and demon-hunting; validate scale/footprint/reach/camera.
- Pixie: bounded irresistible dance and sleep arrows with correct uses/saves,
  fey traits/invisibility, no ammunition/loot/permanent effect/teleport/summon;
  validate scale, sockets, animations, navigation, and selection.

All natural proxies named in sections 5-6 use donors only for compatible
visuals/animations. Rebuild intended stats, size, reach, speed, attacks, feats,
senses, and specials; validate scale/footprint/navigation; retain no
inappropriate poison, web, gaze, breath, spellbook, or campaign ability.

## 12. Ability construction and publication

Reuse native parent/child architecture while preserving spell slot/level,
metamagic, caster/duration context, casting time, range and placement,
descriptors/school, material-object invariants, prepared/spontaneous access,
UI rendering, AI/commands, pools/cleanup, Acadamae, and summon feats. Never
null a donor's non-null empty data object.

The pure merge and transaction must start from exact final-live variants;
preserve every existing reference and order; add KMG entries in documented
deterministic order; deduplicate by reference/GUID; be idempotent; prove exactly
one intended KMG reference and no changed/lost prior entry; clear only relevant
caches after success; record originals; roll back all required-base mutations
on failure; refuse unsafe rollback after unrelated mutation; and fail closed
only on an ambiguous optional surface. Never replace a whole parent array or
assume KMG initializes last.

Reconcile final-live optional parents structurally without compile references.
Require exact documented signatures and one unambiguous match, preserve all
third-party variants, skip/record absent or ambiguous optional targets, and
never guess by name.

## 13. Feature-module integration

Extend three modules to four. Expanded Summoning defaults on and requires a
restart; pending UI edits never mutate active process state; settings writes
remain atomic; malformed settings quarantine and recover with all modules on.
Increment the actual schema and migrate every older schema, preserving explicit
Gunslinger/Acadamae/Shield Other values and adding Expanded Summoning on when
absent. Update equality/hash/diagnostics/state/UI/plans/serialization/tests and
enumerate all 16 configurations from an authoritative catalog where possible.
Each module controls only its publication surfaces; registration stays constant;
restore settings exactly after testing.

The Expanded Summoning gate controls only KMG additions to SM/SNA parents and
optional-parent reconciliation. It never gates registration or existing active
summons.

## 14. Existing-feature and feat interaction

Qualify Acadamae recognition of every relevant variant, one acceleration for
eligible casters only, retained failure/save behavior; Augment on every unit
exactly once; Superior only on appropriate quantity options; Sacred Summons or
equivalent optional behavior when structurally available and fail-closed when
ambiguous; metamagic and prepared/spontaneous UI without exceptions/inert
icons. Shield Other, Gunslinger, Cord, firearms, vendors, loot, and settings
remain unchanged except the authorized prerequisite. Avoid broad global summon
patches when a feature-local/native solution exists.

## 15. Domain and static tests

Use the existing dependency-free executable suite. Cover exact SM/SNA rosters
and no duplicate key/tier; unit/ability identities, manifest types/collisions;
matrix rules and same-kind packs; SM template policies, neutral choice, and no
SNA templates; merge preservation/idempotence/rejection; exact rollback and
unsafe-rollback refusal; disabled publication; all 16 settings states; schema
migration/quarantine/restart snapshots; sanitizer and donor immutability;
good/neutral/evil decisions; feat markers; count bounds; duration/cleanup;
special adaptation policies; deterministic generated output; exact bootstrap
count; and all existing regressions. Explicitly enumerate sources/tests and use
real checked-in roster/manifest data with narrow boundary stubs only.

## 16. Runtime test implementation

Extend the guarded allowlisted harness with separate structural, disposable
mechanical, visual-contract, and persistence scenarios (using repository naming
conventions equivalent to `observe-expanded-summoning-blueprints`,
`disposable-expanded-summoning`, visual contracts, prepare, and cleanup).

On fresh process, structural evidence must prove all IDs/types/counts,
family/tier/multiplicity, required parents, enabled/disabled exact publication,
preserved prior variants, components/descriptors/level/range/casting/duration/
pool/unit/count/material invariants, no forbidden unit surfaces or donor
mutation, no duplicates/null hazards, and no startup/action-bar exception storm.

Disposable mechanics must cast every approved one-creature KMG option and, for
every eligible family/tier, at least one 1d3 and 1d4+1 option through actual
ability use; prove count bounds, same-kind units, and cleanup. Exercise animal,
flying proxy, elemental, outsider, incorporeal, invisibility, breath,
ranged/projectile, SLA, large/huge, Bebelith, and Pixie; movement, control/AI,
attack/hit/damage/death/dismiss/expire; RTwP/turn-based where behavior differs;
caster duration; placement/pathing; no XP/loot/interaction/dialogue/companion/
inventory/quest effect; prohibited powers; SM and SNA alignment; Augment,
Superior, Acadamae eligible/ineligible; and leak-free repetition. Test formulas
and RNG boundaries in pure tests without changing production RNG.

Visual-contract evidence must mechanically observe prefab instantiation,
locomotion/combat/cast/hit/death events, projectile origins/fallbacks,
selection/footprint/scale/reach/navigation bounds, no missing mesh/T-pose/
runaway VFX/persistent corpse, and no map/camera-blocking proxy scale.
Screenshots are supporting evidence only; retain a short human aesthetic list.

Persistence uses guarded named saves only. Never overwrite
`KMG_AUTOMATION_BASELINE`. With `KMG_AUTOMATION_WORKING` or an authorized
disposable save: create active summons, save, exit, relaunch via Steam App ID
640820, reload through the guarded workflow, verify identity/context/duration/
control/facts/no duplication, dismiss/expire, save/relaunch, prove absence, and
repeat disabled to prove frozen identities load while no variants publish.

Run all 16 module states on fresh launches, verifying exact active state,
constant registry count, expected-only surfaces, no duplicates/leakage, and
byte/hash restoration of settings. Using current checked-in profiles, qualify
standalone, Call of the Wild, Arms and Armor, Toggle Custom Soundpacks, and the
highest-risk combined profile. Repeat standalone, CotW, and highest-risk twice
on fresh processes after final source freeze. Restore Mods/settings after every
transaction. No optional compile dependency.

## 17. Validation and packaging

Inspect current scripts/reports rather than stale root documents, and update
stale guidance. Required gates: `git diff --check`; roster/manifest validation;
repository static validation; complete domain suite; clean Release against
exact local 2.1.7b references and warnings-as-errors where required;
deterministic repeat build/package; strict package validation; exact source,
version, DLL, and package hashes; all guarded runtime scenarios; 16-state
matrix; profiles; and full regression suite. Build/tests alone are not runtime
proof.

Never commit/publish proprietary assemblies, installed-mod DLLs, saves,
credentials, raw machine-local logs/screenshots, packages, local config, or
private game assets. Commit only curated reports/evidence, manifests, source,
tests, scripts, and permitted project assets.

## 18. Documentation and release metadata

Update current repository conventions including `Info.json`, assembly/file
versions, `Directory.Build.props`, changelog, README, installation/
compatibility, known issues, build info, testing, architecture/manifest/module/
runtime docs, profiles/schema, and package/version validation. Determine the
next version from the fetched qualified baseline; do not assume 0.0.78.

The roster/fidelity record must identify per entry: family/tier/name, template
policy, reuse/clone, donor GUID/name/view, KMG unit GUID, all count-ability GUIDs
or sharing rationale, mechanics removed/adapted, deviations, structural/runtime/
visual/profile status. Document exclusions and never call an omission complete.
Reconcile stale Sprint 29/30 root guidance.

## 19. Commit and publication strategy

Use coherent reviewable commits for prerequisite, mission/inventory, pure model,
manifest tooling/identities, sanitizer, tiered units, special creatures,
one-creature abilities, quantity variants, transaction/reconciliation,
feature/settings matrix, feat integration, runtime instrumentation, profiles,
and final release evidence as dependencies permit. Do not collapse the feature
into one commit. Push each coherent passing commit with the mandated script.

At completion push the branch and open, but do not merge, a draft PR against
the intended integration base. Include baseline/final SHA, version, roster and
ledger totals, tests, runtime IDs, profiles, hashes, deviations, and manual
visual list. If credentials prevent publication, preserve everything and record
the exact blocker.

## 20. Definition of done

Do not report completion until every approved SM/SNA entry is correct without
native duplicates; all quantity coverage is complete and same-kind; all units
are safe normalized summons; adaptations are implemented or honestly
documented; IDs/types and constant registration are exact; the independent
default-on restart-bound module and 16 restored states pass; vanilla and
third-party variants are preserved; optional reconciliation is structural,
additive, idempotent, and ambiguity-safe; every one option and every tier/family
quantity class is cast at runtime; alignment, feats, and Acadamae pass;
save/load/restart/cleanup and all profiles pass; regressions, clean Release, and
strict package pass; metadata/evidence/report are current; branch is pushed;
and a draft PR exists or an exact credential blocker is recorded.

## 21. Catastrophic stop conditions

Stop only after safe recovery is exhausted when: repository or required game
installation is inaccessible/corrupt with no safe recovery; exact assemblies or
assets are absent and unrecoverable without acquiring/redistributing protected
material; destructive upstream conflict prevents preserving both histories;
the roster hits a proven engine hard limit after multiple instrumented safe
architectures and continuing risks saves/parents; runtime is blocked by Steam
credentials/entitlement/update/cloud/save ambiguity after all safe static work;
no safe disposable-save path exists; GitHub credentials prevent push/PR after
all local work; or a required legal asset has no approved in-game proxy.

Compile/test/runtime/package failures, preferred visuals, ambiguous optional
parents, unavailable exact tabletop mechanics, UI crowding, reflection/API
obstacles, donor surprises, instrumentation needs, compaction, and conservative
proxy choices are not catastrophic. Resolve, narrow, instrument, use the
authorized fallback, document, and continue.

## 22. Final response contract

The concise final response must state: complete or exact catastrophic blocker;
repository/branch; baseline and final source SHA; version; draft PR; commit
count/groups; exact roster totals; feature active/reserved and total registry
counts; settings schema and 16-state result; domain/static totals; build/package;
runtime IDs/results; persistence; profiles; DLL/package/source hashes; all
adaptations/omissions; preservation of existing variants; restoration of
settings/Mods/saves and confirmation the protected baseline was untouched;
remaining manual visual checklist; and only the next human action of reviewing
the draft PR and performing those listed aesthetic checks. Do not ask for
permission to continue.
