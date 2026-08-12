# Expanded Summoning implementation report

Status: final audit qualification in progress. The completion audit required
one stronger final-live assertion tying all 681 roots and 364 template
executions to their native spell, Acadamae, metamagic, material-data, and
action-bar contracts. That focused assertion is source-qualified; its guarded
immutable-source rerun and resulting artifact re-freeze are pending. Draft PR
[#2](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/2)
targets `master` and remains unmerged for review.

Selected baseline: `origin/master` at
`2894d9fcce250708e354894ffd8e1be9c7493b9b`, containing required
`e4d560f8dd2909518614e3a20e77ba4d70dadeb8`. Release baseline: 0.0.77.

## Authoritative final qualification

This section supersedes the checkpoint-status language in the chronological
engineering record below. Earlier “pending” statements are retained as an
honest account of what was unproven at those checkpoints, not as the current
release state.

- Frozen catalog: 66 Summon Monster entries, 57 Summon Nature's Ally entries,
  67 shared unique creatures, 361 SM placements, 320 SNA placements, and 681
  same-kind placements total.
- Identities: 1,155 Expanded Summoning active identities; repository ledger
  1,409 active plus one reserved; runtime registration is exactly 1,409 in
  every one of the 16 feature-module states.
- Static gates: repository validation PASS, `1009/1009` domain tests PASS,
  warnings-as-errors Release build PASS, and strict package validation PASS.
  Two clean exact-reference `Build-Local.ps1` executions produced byte-identical
  DLL and deterministic package outputs.
- Structural runtime: guarded run
  `20260812T1149141950160Z-0dfc7143323b4095a01dea690e43c2c0` passed all 30
  assertions, including every registered identity, 18 required parent spells,
  all 681 placements, donor immutability, sanitizer contracts, exact special
  structures, and preservation of preexisting final-live variants.
- Mechanical runtime: guarded run
  `20260812T1303503740041Z-73e7a3e6a825468d91f5a9fd7e970889` passed every
  assertion and all 153 production commands: 123 logical one-creature choices,
  16 `1d3` tier/family cases, and 14 `1d4+1` cases. Counts, same-kind identity,
  CL20 duration, close placement, Augment/Superior Summoning, Acadamae
  eligibility, good/neutral/evil templates, SNA alignment, representative
  combat, special actions, and exact cleanup passed.
- Visual contracts: guarded run
  `20260812T1151394827201Z-add45a04f5de44c1a39e3251f7ff0778` passed all ten
  assertions for 67/67 unique units: attached/renderable views, bounded scale
  and footprint, navigation, locomotion, attack/projectile fallback, hit/death,
  and exact view/unit cleanup.
- Persistence: enabled and disabled prepare/load/cleanup/absence sequences all
  passed on fresh Steam App ID 640820 processes. Frozen summon identities,
  caster context, remaining duration, faction/control/view state, and cleanup
  survived restart. With the module disabled, existing summons remained
  load-safe while new KMG parent publication was exactly zero.
- Module matrix: all 16 restart-bound states passed on the immutable source with
  exact independent surfaces and constant 1,409 registration. Settings were
  restored byte-for-byte.
- Compatibility: standalone twice, Call of the Wild 1.14.4c-2.1 twice, Arms
  and Armor 1.0.10 once, Toggle Custom Soundpacks 1.0.1 once, and the
  highest-risk combined profile twice all passed and restored the Mods
  directory and settings transaction exactly.
- Save safety: restored settings SHA-256
  `424da4573acb5dc9e3c7ca3546da688a1405702858fb3b28aea5cbae28c4ba3e`,
  working-save SHA-256
  `3595a41873f62ef2e28762abb6dd757418b239f2e5c9441f6f027214fc99a997`,
  and protected `KMG_AUTOMATION_BASELINE` SHA-256
  `cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07`.
  The protected baseline was never selected or modified.

### Conservative adaptations and omissions

- Lantern Archon uses a Will-o'-Wisp view and immediate dual ranged-touch ray
  delivery. Aura of Menace is conditionally reused only when the compatible
  optional carrier exists; greater teleport, gestalt, truespeech, and
  separately modeled low-light/darkvision are omitted.
- Mephits retain Owlcat's unconditional Fast Healing 2 because no safe local
  element-environment predicate was proven.
- Natural/proxy creatures omit only the per-row mechanics listed in the
  fidelity matrix, principally unsupported swim/climb/burrow modes, grab,
  trample, sprint, rage, and unproven donor-specific feats or senses. Donor
  visuals do not contribute unrelated mechanics.
- Salamander uses bounded spear/tail, heat, and grab-constrict graphs; cold
  vulnerability is omitted because no exact safe bounded fact was proven.
- Invisible Stalker retains attack-safe permanent invisibility; dedicated
  tracking and scent are omitted.
- Shadow Demon uses bounded incorporeal/cold combat; possession, shadow blend,
  sprint, teleportation, and summoning are omitted.
- Succubus charm and energy drain are bounded to short domination and a
  one-round temporary negative level; profane gift, teleportation, and
  summoning are omitted.
- Bebelith replaces permanent armor destruction with a DC 25, one-round -2 AC
  dismantle effect and retains bounded demon-hunting bonuses; rot and climb are
  omitted.
- Pixie sleep arrows use a zero-damage, resource-backed, non-transferable bow;
  irresistible dance uses a frozen bounded state. No ammunition or persistent
  effect can escape the summon lifecycle.

The manual residual visual checklist is limited to aesthetic judgment of proxy
scale, camera framing, projectile appearance, and animation quality; all
mechanical visual contracts already pass.

### Final release hashes

- Release artifact source commit:
  `193d73cc22fe41fda8546f1d2e1750e185ed8288`.
- Exact-reference `KingmakerGunslinger.dll` SHA-256:
  `64bc093904ea80514b7811ab73ef488c3c7561ab5af049f7ba08e74d8c177966`.
- Deterministic `KingmakerGunslinger-0.0.78-expanded-summoning.zip` SHA-256:
  `2dde3ce858397cf27e86d01b9f69b68ececb05e0127386cd31d3fd22caa739ce`.
- Deterministic `git archive` source ZIP SHA-256:
  `c698e82d38599e06c58a32a9b243c391c9e9a4cb155b6047dee4d5ef936cf784`.

The ordinary `Compress-Archive` output was also strict-validated but embeds
archive metadata and therefore changed byte hash between runs. The canonical
release ZIP above was regenerated from the same validated 45-file staging tree
with the repository's fixed-timestamp deterministic writer; it exactly matched
the separately emitted local-runtime ZIP and passed strict validation.

## Chronological engineering record

The mandatory Shield Other prerequisite is source-qualified: established links
no longer depend on distance, while close initial targeting and all other
lifecycle rules remain intact. Repository validation, 981/981 domain tests,
clean Release build, and strict 0.0.77 package validation pass.

Current implementation checkpoint: the complete frozen 681-placement catalog,
67 summon-unit identities, 1,050 abilities, six HD-banded celestial or
fiendish template buffs, two bounded smite markers, seven custom special
creatures, and the tier I-IV natural/proxy group are registered
deterministically. The exact template
SR threshold is implemented as low (0-4 HD, no SR), mid (5-10 HD, CR+5 SR),
and high (11+ HD, CR+5 SR), with resistance/DR values 5/5/10. The ledger is
1,401 stable IDs: 1,400 active and one reserved. Source qualification passes
1,008 tests, clean Release, and strict package validation.

The six-buff graph subsequently passed guarded fresh-process run
`20260811T1954362756414Z-observe-expanded-summoning-inventory` on committed
source `d384ba06cf76896543a6b23ed480d3f6715bbba2`, including exact low/mid/high
mechanics, all aligned execution counts, registry 1,372, and 681 live parent
placements. The run was save-free.

The exact optional smite inventory finished on
`20260811T2008011506353Z-observe-expanded-summoning-inventory`: the native-like
optional graph uses a fixed one-use resource but applies a permanent non-child
buff to its target. Reusing that graph would allow external target state to
outlive the summon. KMG instead implements smite as a unit-local marker: the
first successful attack against the opposed alignment receives nonnegative
Charisma to attack and HD to damage, consumes the marker, and creates no target
state. This intentionally omits manual swift target selection and persistent
bonuses against one selected target, a conservative non-overpowered lifecycle
adaptation. Its committed-source structural runtime qualification remains
pending. Runtime unit-alignment fidelity also remains open and is not claimed.

That structural qualification subsequently passed on committed source
`a0d8e7752281d4ae1e51ce094c1226f6a30faf16` as guarded run
`20260811T2021406071785Z-observe-expanded-summoning-inventory`: exact registry
1,374, 67 units, 1,045 abilities, 681 placements, 182 executions per template,
six template buffs, two smite markers, and all sanitizer checks passed. The
preceding fresh launch exited during platform initialization after request
acceptance and before blueprint inspection; it was retained as failed evidence
and the evidence-supported retry passed. Neither run accessed a save. Runtime
unit-alignment fidelity remains open and is not claimed.

Spawn-local alignment is now source-qualified. A custom native post-spawn
action sets the new unit descriptor rather than changing a shared blueprint.
Celestial and fiendish summons preserve the unit's law/chaos axis while
replacing its moral axis; all Nature's Ally placements copy the actual caster's
exact alignment from the ability context. Missing or invalid context fails
closed without mutation. Repository validation, 1,005 tests, clean Release,
and strict package validation pass; committed-source structural observation
and later actual-cast/save-load proof remain pending.

The first alignment-aware structural run failed and was retained as
`20260811T2031332882968Z-observe-expanded-summoning-inventory`. It proved that
the generic graph clone had preserved references within native action lists,
allowing post-spawn actions to accumulate across abilities using the same
native quantity template. The initial repair explicitly cloned each
`ActionList`; the following run narrowed the deeper alias to its GameAction
elements. The observer now also rejects any KMG
post-spawn action or buff in a non-KMG ability. The repair is source-qualified;
its committed-source rerun remains pending.

That rerun (`20260811T2037058339804Z-observe-expanded-summoning-inventory`)
proved the ActionList-only repair incomplete: 286 non-KMG abilities still
contained KMG actions. Exact assembly inspection established that every
`GameAction` is itself a `SerializedScriptableObject` and was being returned by
the clone's Unity-object preservation guard. The follow-up now creates and
recursively copies each GameAction while still preserving immutable referenced
Unity assets. This deeper repair is source-qualified; its committed-source
rerun remains pending.

The deeper repair was proved by
`20260811T2043115519772Z-observe-expanded-summoning-inventory`: contamination
of non-KMG abilities fell from 286 to zero. Remaining assertion failures were
limited to the observer's incorrect one-action-per-ability assumption; native
quantity templates may contain multiple spawn nodes. The observer now requires
one matching alignment/template/smite action per actual spawn branch. This
cardinality correction is source-qualified and awaits its committed rerun.

That rerun passed as
`20260811T2048003275107Z-observe-expanded-summoning-inventory` on committed
source `b88e99cffb7464d7354416fba82d1da313e17ae2`. All celestial, fiendish, and
Nature's Ally actions matched exact native spawn-branch cardinality, and
non-KMG action contamination was zero. All registry, placement, template,
smite, and sanitizer assertions remained green. No save was accessed.

The complete donor graph passed guarded save-free run
`20260811T2055502086857Z-observe-expanded-summoning-inventory` on committed
source `9e1d851e75cf413f5d0a576484a9f5a8538b2a2b`. All 54 distinct chosen donor
identities were present, and bounded component, body, and view graphs were
captured for each. All 17 structural assertions passed with zero warnings,
including registry 1,374, 681 placements, and every existing sanitizer and
alignment invariant. The wrapper reached its host timeout only after the game
had flushed the PASS result and exited. This inventory now drives the native
reuse versus reconstruction decisions; it does not itself claim creature
mechanical fidelity.

The first creature-mechanics group is source-qualified. All 24 elemental and
four mephit entries now have an explicit immutable native-dedicated-reuse
classification; KMG clones their proven summon units and applies the existing
XP/loot/inventory/campaign sanitizer. Lantern Archon is reconstructed from a
Will-o'-Wisp view with 2 outsider HD, official ability scores and alignment,
dual bounded 1d6 ranged-touch rays, ray-only AI, archon defenses, and the
native Aura of Menace carrier. Wisp and Ghaele combat/campaign mechanics are
not retained. Greater teleport and gestalt are conservative summon-safety
omissions. Source qualification is 1,006 tests plus clean Release and strict
package PASS; exact final-live structure and actual runtime use remain open.

The next two reconstruction groups are now structurally qualified. Invisible
Stalker and Shadow Demon passed exact final-live assertions in
`20260811T2207541420526Z-observe-expanded-summoning-inventory`. Salamander and
Succubus then passed in
`20260811T2238575798728Z-observe-expanded-summoning-inventory` on committed
source `b0deb04ff9b387b375202c5304a6741c9549ef0a`. That final save-free run passed
all 24 assertions: 67 units, 1,047 abilities, registry 1,386, all 681 parent
placements, exact special structures, exact alignment/template counts, and
zero donor aliases, prohibited references, inherited class spells, or starting
inventory. Actual casts, visual contracts, persistence, feature-state launches,
and complete compatibility-profile qualification remain open and are not
claimed.

Bebelith and Pixie are now structurally qualified on committed source
`f058f4b5060e7eae4de4c7621cbdcbd06cbf08a7`. Guarded save-free run
`20260811T2310424930290Z-observe-expanded-summoning-inventory` passed all 25
assertions: 67 units, 1,048 abilities, registry 1,396, all 681 placements,
exact bounded special structures, exact alignment/template execution counts,
and zero donor aliases, prohibited references, inherited class spells,
starting inventory, or native-action contamination. Call of the Wild was
loaded and its final-live summon surfaces were preserved.

Bebelith's unsafe permanent armor destruction is conservatively represented by
a DC 25 Reflex-gated, one-round -2 AC effect after two same-target claw hits in
one round; no equipped item is mutated. Its demon-hunting benefit is a +2
attack/damage bonus against chaotic-evil outsiders. Rot and climb are omitted.
Pixie uses an actual native arrow rig with zero weapon damage, sixteen
resource-backed sleep arrows (Will DC 15, 50 rounds), and one resource-backed
CL 8 Irresistible Dance using the native touch delivery and dance state.
Neither implementation introduces inventory, transferable ammunition,
teleportation, summoning, planar travel, poison, web, or persistent external
state. Actual casts, special-action execution, visuals, cleanup, persistence,
feature-state launches, and complete compatibility-profile qualification
remain open and are not claimed.

The first low-tier natural group is now structurally qualified on committed
source `c2bee19c6598f559436e5f09af5029dc1da746de`. Dog, Eagle, Poisonous Frog,
Giant Centipede, Giant Spider, Goblin Dog, and Hyena use explicit tabletop
chassis and attacks; their donors no longer supply unrelated enemy mechanics.
Guarded run
`20260812T0010300046437Z-observe-expanded-summoning-inventory` passed all 27
assertions with registry 1,399, all 681 placements, exactly 67 hidden KMG
extraplanar markers, and zero sanitizer, donor-alias, inventory, inherited-spell,
or native-action contamination failures. Static validation, `1007/1007`
domain tests, clean Release, and strict packaging passed. Actual casts, visuals,
cleanup/persistence, module-state launches, and compatibility profiles remain
open and are not claimed.

## Strengthened mechanical qualification

The current 1,155-identity implementation passed the complete guarded
mechanical scenario in run
`20260812T1143070098993Z-bffb856b44d34334be86fa89c15bb6db`. All 153 native
ability commands passed: every 123 logical one-creature SM/SNA entry, 16
family/tier `1d3` cases, and 14 family/tier `1d4+1` cases. Runtime evidence
proves same-kind identity, CL20 duration, close-range approach, exact cleanup,
Augment and Superior Summoning, good/neutral/evil template choice, SNA caster
alignment, and representative natural/proxy/elemental/outsider/special combat.

The selected KMG celestial/fiendish template now replaces all four exact
native Owlcat summon-template buffs on the spawned KMG unit. Runtime cases for
good celestial, neutral celestial, neutral fiendish, and evil fiendish each
contained exactly one KMG template and smite marker and zero native template
buffs. The actual Fire Mephit breath command dealt 18 fire damage through its
native effect graph; Succubus domination, Pixie dance/sleep arrows, bounded
Bebelith dismantling, and permanent-on-attack Invisible Stalker invisibility
also passed. Final immutable-source repetition remains required before release
qualification is claimed.

## Active-summon persistence qualification

Guarded prepare/load/cleanup/absence transactions pass with Expanded Summoning
enabled and disabled. Two actual production summon casts were serialized with
exact frozen unit GUIDs. Fresh processes proved registered identity, caster
context, remaining native duration, control/view/faction state, native cleanup,
and final absence. With the module disabled, the saved summons remained
load-safe while KMG parent publication was exactly zero. The working save and
settings were restored to their original hashes, and
`KMG_AUTOMATION_BASELINE` was never modified. Immutable-commit repetition is
the next release gate; it is not yet claimed here.

## Feature-module runtime matrix

All 16 restart-bound configurations passed on immutable source `5e25656`.
Each fresh process registered exactly 1,403 active identities independent of
module state. Existing feature publication remained isolated, and Expanded
Summoning contributed exactly 681 required-base parent references when enabled
and zero when disabled. The settings transaction restored the original file
byte-for-byte; working and protected-baseline save hashes were unchanged.

## Compatibility qualification

Immutable source `5bce781d25ba6f3efadf693dafef2267fd2003fe` passed the
complete required profile set: standalone twice, Call of the Wild 1.14.4c-2.1
twice, Arms and Armor 1.0.10, Toggle Custom Soundpacks 1.0.1, and the
highest-risk combined profile twice. All eight guarded Steam launches passed
the exact Expanded Summoning structural observer and restored the prior Mods
directory and feature settings transaction exactly. Call of the Wild's
final-live summon surfaces remained additive and intact.

Standalone inventory proved that the Aura of Menace and original
Irresistible Dance carriers are supplied only by the installed Call of the
Wild profile. Lantern Archon therefore conditionally reuses the aura and
documents its standalone omission. Pixie instead owns a frozen, bounded dance
state (`aa8b4284e12e49f0b37f327f665638d1`) and behaves identically in every
profile without a compile-time or runtime dependency on the optional mod.

## Native cast qualification

Committed source `8647ceff29ae45c416f948a979fd25098422910d` passed guarded
scenario `disposable-expanded-summoning`, run
`20260812T0235012741461Z-b7419c9642a445ac9edf4bfc8a2ad825`. The scenario
loaded exactly `KMG_AUTOMATION_WORKING` through the established autonomous
receiver-bound UI workflow and issued 153 native ability commands: every 123
approved one-creature SM/SNA logical entry, 16 `1d3` family/tier cases, and 14
`1d4+1` family/tier cases. All commands reached native execution completion;
the 205 observed summons had legal counts and exact same-kind blueprint
identity. Every created unit was disposed and exact party/global-unit snapshots
were restored. No save-writing API was observed, scenario hooks were removed,
and `KMG_AUTOMATION_BASELINE` was neither selected nor modified.

The first loaded-area attempt exposed a native view-attachment null
dereference for KMG-owned summon buffs. The fix initializes all custom
`FxOnStart` and `FxOnRemove` fields to non-null empty `PrefabLink` objects,
matching Kingmaker's native data-object invariant; domain source contracts now
enforce it. Static validation, `1009/1009` domain tests, clean Release, and
strict package validation pass for the qualified source. Visual animation and
special-action contracts, active-summon persistence, the 16-state fresh-launch
matrix, and compatibility-profile qualification remain open and are not
claimed.

## Visual-contract qualification

Committed source `ee8a5886fdd817e659fe2afdf3f1019501aac064` passed guarded
scenario `disposable-expanded-summoning-visual-contracts`, run
`20260812T0316269056830Z-77c365156f0b47f5bc6a6c1e8501a6c7`. Every 67 unique
one-creature unit was created through its production ability/native summon
path. The run proved live view attachment, renderable nonzero geometry,
bounded scale/world bounds, colliders and navigation members, locomotion
events, attack animation or the exact Lantern ray fallback, hit and death
handling, valid projectile/torso origins for detected ranged weapons, and
exact unit/view cleanup.

The Lantern Archon's Will-o'-Wisp rig has no cast/attack clip, so its two-ray
ability now uses Kingmaker's native `Immediate` animation style and the proven
`CenterTorso` projectile origin. This is a bounded visual-rig compatibility
adaptation; ray count, ranged-touch delivery, damage, defenses, and AI remain
unchanged. Repository validation, `1009/1009` domain tests, clean Release, and
strict packaging pass. No save-writing API was observed and request-local
hooks were removed. Residual aesthetic judgment remains on the final manual
checklist; active-summon persistence, the 16-state fresh-launch matrix, and
compatibility-profile qualification remain open.

The complete tier I-VII natural/proxy catalog is now structurally qualified on
committed source `3c2c5fef82a7d9b032f7da906385013a5699cc8c`. The final group adds
Dire Lion, Ankylosaurus, Dire Bear, Dire Tiger/Smilodon, Elephant, Mastodon,
and Roc with immutable PF1e chassis and proxy-only donor views. Three frozen
KMG weapons provide exact 3d6 tail, 2d8 bite, and 2d6 talon dice without
mutating native weapons. Guarded run
`20260812T0045336396930Z-observe-expanded-summoning-inventory` passed all 29
assertions in 107,891 ms: 67 units, 1,048 abilities, registry 1,403, all 681
placements, exact tier I-VII and special structures, and zero sanitizer,
donor-alias, prohibited-reference, inventory, inherited-spell, or native-action
contamination failures. Call of the Wild final-live parents remained intact;
no save was accessed. Static validation and all 1,009 domain tests, clean
Release, exact-reference local build, and strict packages pass. Actual casts,
quantity rolls, visuals, cleanup/persistence, the 16-state fresh-launch matrix,
and compatibility-profile qualification remain open and are not claimed.

The tier III-IV natural/proxy tranche is now structurally qualified on
committed source `2534f57199cec7a8cd5ef3b5715cdd4ad30d0ac6`. Boar, Leopard,
Monitor Lizard, Cheetah, Crocodile, Dire Bat, Wolverine, Dire Boar, Dire Wolf,
Grizzly Bear, Lion, and Pteranodon are rebuilt from checked-in tabletop
profiles. Their donors supply views only. A frozen KMG secondary 1d12 tail
identity supplies Crocodile's tail without changing the native 1d8 donor
weapon; the ledger is now 1,400 active plus one reserved identity and the
constant registry is 1,400.

The first guarded launch
`20260812T0026276683838Z-observe-expanded-summoning-inventory` timed out after
an exact bootstrap failure: several native facts declared as the base
`BlueprintUnitFact` are concrete `BlueprintFeature` objects. Registration
rolled back. The repair preserves exact concrete lookups per fact instead of
weakening type validation. Fresh guarded run
`20260812T0031212209441Z-observe-expanded-summoning-inventory` then passed all
28 assertions in 106,947 ms: 67 units, 1,048 abilities, registry 1,400, all 681
placements, exact tier I-IV and special structures, and zero sanitizer,
donor-alias, prohibited-reference, inventory, inherited-spell, or native-action
contamination failures. Call of the Wild final-live parents remained intact;
no save was accessed. Repository validation, `1008/1008` domain tests, clean
Release, and strict package validation pass. Actual casts, visuals,
cleanup/persistence, module-state launches, and compatibility profiles remain
open and are not claimed.
