# Urban Barbarian mission

## Authority and baseline

- Repository remote: `git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git`.
- Requested source branch: `codex/brown-fur-transmuter-cotw-extension`.
- Fetched remote base: `08ebf14d2ff84db74f8be861bcc58ed9740b79eb`.
- Verified handoff SHA: `08ebf14d2ff84db74f8be861bcc58ed9740b79eb`.
- Feature branch: `codex/urban-barbarian`.
- Version authority before the mission: `Info.json` and
  `Directory.Build.props` both identify package version `0.0.82`.
- Reserved next development version: `0.0.83`.

The on-disk checkout is named `KingmakerGunslinger`, but its configured origin
is the requested combined-package repository. The requested sibling directory
`KingmakerGunslingerAndOtherTabletopRules` did not exist; no alternate clone or
baseline was invented.

## Mission boundary

Implement Urban Barbarian as the eighth, default-enabled native Kingmaker
feature module, ID `urban-barbarian`, attached to the native Barbarian class.
It is independent of Call of the Wild. CotW is an optional interoperability
profile only; absent, unknown, or ambiguous CotW state must not suppress the
Urban core or another package module.

The mission includes investigation, tracked design, stable blueprint identity,
schema-7 settings, transactional publication, exact Controlled Rage mechanics,
Crowd Control, focused and exhaustive fast tests, clean Release build, package
validation, one immutable candidate, focused guarded runtime qualification,
CotW absent/normal/balance profiles, existing-owner OFF persistence, the exact
18-state eight-module runtime boundary, guarded installation, and the final
human presentation/play-acceptance checklist.

Stop only at that human boundary or at a documented hard stop after safe,
reversible, evidence-supported alternatives and narrower instrumentation have
been exhausted. Build or source-test success is not runtime qualification.

## Non-negotiable design contract

- Parent: native Barbarian class and progression.
- Proficiencies: simple, martial, light armor, and non-tower shields; no medium
  armor.
- Archetype-only class skills: retain the native applicable Athletics,
  Mobility, Perception, and Persuasion; remove Lore (Nature); add Knowledge
  (World). No Profession substitute and no global Persuasion modifier.
- Level 1: replace Fast Movement with Crowd Control and Controlled Rage.
- Crowd Control: owner-scoped attack/AC rule-event evaluation, native hostility
  and active-life-state filtering, edge-to-edge five-foot adjacency including
  corpulence, +1 untyped attack and +1 dodge AC at two or more adjacent enemies.
- Controlled Rage pools: +4, +6, and +8 morale ability-score bonuses, allocated
  among Strength, Dexterity, and Constitution in +2 increments.
- Selector: one compact nested current-tier selector, with exactly 6/10/15
  deterministic allocations. It costs no Rage rounds, persists, cannot change
  while raging, defaults each newly unlocked tier to full Strength, and grants
  no benefit outside Rage.
- No ordinary native Rage attack, damage, temporary HP, Will, or AC effects.
- Preserve the native activation/resource/per-round/fatigue/Tireless/end-rage
  lifecycle, spell/concentration restriction, rage powers, and Rage-inspecting
  prerequisites/items.
- Constitution is a real morale ability modifier and must not enable toggle
  healing, duplicate HP, immortal negative HP, or persistence discrepancies.
- Never mutate native Barbarian class skills, proficiency feature, Rage feature,
  Rage buff, or progression arrays globally.
- Register every Urban identity whether the module is ON or OFF. The module
  gates only additive publication into the native Barbarian archetype array.

The crowd-movement and crowd-influence tabletop clauses may be implemented only
if exact native subsystems exist. Broad movement, terrain, attack-of-opportunity,
freedom, or Persuasion substitutes are forbidden.

## Architecture gate

Production Rage implementation is blocked until a guarded read-only inventory
records the final no-CotW and CotW-present graphs for:

- native Barbarian class and progression;
- proficiency, Fast Movement, Rage feature, activatable, resource, and buff;
- Greater Rage, Tireless Rage, Mighty Rage;
- native rage powers;
- representative Rage feats and items;
- CotW Rage marker and representative CotW-added rage powers; and
- every component on the finalized Rage buff, classified as ordinary benefit,
  lifecycle, restriction, native rage-power integration, CotW integration, or
  presentation/metadata.

The gate must also establish whether owner-scoped substitution can preserve the
exact native Rage identity without affecting base Barbarians. If that surface is
invasive or fragile, the approved fallback is an Urban-specific buff cloned from
the finalized native lifecycle plus an explicit Rage-equivalence bridge.

Status after the architecture checkpoint: **GATE PASSED; production Rage
implementation authorized; no production Rage source has yet been modified**.
The save-free
`observe-urban-barbarian-rage-inventory` scenario records the exact native
class/progression graph, class skills, archetype order, component fields and
assemblies, and bounded Rage-bearing contracts in one atomic JSON artifact.

The exact identities are native Barbarian
`f7d7eb166b3dd594fb330d085df41853`, progression
`74ad9fbd930a1d84aaf9a230f29f8f12`, proficiency
`acc15a2d19f13864e8cce3ba133a1979`, Fast Movement
`d294a5dddd0120046aae7d4eb6cbc4fc`, Rage feature
`2479395977cfeeb46b482bc3385f4647`, activatable
`df6a2cce8e3a9bd4592fb1968b83f730`, resource
`24353fcf8096ea54684a72bf58dedbc9`, buff
`da8ce41ac3cd74742b80984ccc3c9613`, Greater Rage
`ce49c579fe0bcc647a32c96929fae982`, Tireless Rage
`ca9343d75a83a2745a22fa11c383153a`, Mighty Rage
`06a7e5b60020ad947aed107d82d1f897`, and Rage Power selection
`28710502f46848d48b3f0d6132817c4e`.

Selected architecture: retain the exact native Rage feature, activatable,
resource, and tier facts. Exact IL/runtime evidence showed that activatable
Rage enters `BuffCollection.AddBuffInternal`; a narrow owner check on that
authoritative overload replaces
only an attempted native Rage-buff application for an Urban owner with a cloned
Urban buff. The clone retains exact lifecycle/actions, rank integration,
shared-value, spell prohibition, Rage descriptor, and presentation components;
it removes only the six classified ordinary benefit components and adds the
selected morale ability modifiers. Base Barbarians and every other Rage owner
continue receiving the native buff. The retained native feature supplies exact
feat/resource/prerequisite equivalence without a global prerequisite rewrite.

Research candidate `8edb2cb57206b359e8b67ee8eacbb4df6e98a67a`
passed both guarded profiles. CotW-present run
`20260816T1328091838409Z-observe-urban-barbarian-rage-inventory` recorded 952
records; CotW-absent run
`20260816T1331159196672Z-observe-urban-barbarian-rage-inventory` recorded 130.
The absent-profile transaction
`urban-rage-absent-20260816T133054Z-14e8246e` restored the Mods tree and
settings exactly. This checkpoint artifact is research evidence, not the final
gameplay candidate.

## Qualification contract

Fast tests enumerate all 256 eight-module configurations. Real Kingmaker
cross-module coverage is exactly 18 states: all ON, all OFF, eight ON-alone, and
eight OFF-with-all-others-ON. CotW profiles and Urban mechanics are focused
profiles, not Cartesian dimensions. Only concrete higher-order architectural
interactions justify additional combined profiles.

Every candidate launch must bind source commit, version, package SHA-256, DLL
SHA-256, DLL MVID, installed DLL SHA-256, package-manifest hash, deployment-
manifest hash, game build, feature-settings identity, CotW identity/settings
where relevant, and disposable fixture identity. Package and CotW settings are
restored byte-for-byte after each profile and after interruption.

The exact immutable accepted artifact remains installed unchanged at handoff.
Any gameplay source or package change requested during human review invalidates
the candidate and requires a new build and qualification cycle.
