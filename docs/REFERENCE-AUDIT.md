# Reference Mod Audit

## Purpose

The reference mods solve different halves of the problem:

- **Call of the Wild** is the authoritative inspiration for Kingmaker startup, blueprint registration, class construction, local assembly references, and stable-ID discipline.
- **Cowboys and Demons** is the authoritative inspiration for decomposing firearm mechanics into rule-event components, but it targets Wrath and contains prototype shortcuts that must not become our state model.

No source code from either project is included in this package.

# Call of the Wild

## Adopt

### Unity Mod Manager and Harmony bootstrap

Its entry point captures the UMM logger, creates a `Harmony12.HarmonyInstance`, and patches the executing assembly. This matches Kingmaker's established mod ecosystem.

### Blueprint lifecycle

It patches `LibraryScriptableObject.LoadDictionary()` and performs one-time initialization after the native blueprint dictionary exists. This becomes our selected initialization boundary.

### Release GUID discipline

Its bootstrap permits GUID generation in debug and disallows it in release. Our policy is stricter: no runtime generation in any normal game execution; development tooling writes reservations into a checked-in manifest.

### Local proprietary references

Its project references assemblies from a local Kingmaker installation and marks them non-private. This becomes our redistribution policy.

### New-class construction

Its Brawler implementation demonstrates creation of a `BlueprintCharacterClass`, assignment of hit die/BAB/saves/skills/presentation, registration in the library, and construction of a progression. The Gunslinger class can follow that broad lifecycle.

## Adapt

| Pattern | Adaptation |
|---|---|
| Large static initialization sequence | Split into named, idempotent registration phases |
| GUID helper embedded deeply in helpers | Use a small validated manifest service |
| Direct blueprint literals throughout code | Centralize vanilla references with names, expected types, and signatures |
| Broad monolithic helper surface | Keep narrowly scoped factories and services |
| Global library field | Wrap library access behind bootstrap/context boundaries |

## Do not copy blindly

- Hard-coded vanilla GUIDs must be revalidated against the target final build.
- Call of the Wild's README names older expected game versions; it is precedent, not proof of final-patch compatibility.
- Its scale and static architecture are not a reason to build the new mod as one monolith.
- No runtime dependency on Call of the Wild is introduced.

# Cowboys and Demons

## Adopt

### Firearms are real weapons

The mod keeps firearm attacks in the ordinary weapon pipeline. This preserves ordinary attacks, criticals, feats, and weapon damage.

### Rule-event decomposition

Its firearm directory separates armor penetration, clip/load checking, misfire behavior, and weapon construction. The conceptual separation is sound.

### Touch-AC event technique

Its armor-piercing component listens to AC calculation and adjusts the attack to the target's touch AC rather than converting the attack into a spell. We will use an equivalent Kingmaker rule boundary after instrumenting it.

### Crossbow presentation fallback

It borrows crossbow categories, visuals, and animation style to obtain functional ranged-weapon presentation. That is a sensible fallback for the first Kingmaker release.

### CRPG deed adaptations

It converts some tabletop post-hit free actions into abilities selected before the shot and omits deeds without meaningful game interactions. We will publish the same kind of fidelity matrix.

## Adapt or replace

| Cowboys and Demons behavior | Kingmaker Gunslinger decision |
|---|---|
| Uses Wrath `BlueprintsCache.Init` | Use Kingmaker `LibraryScriptableObject.LoadDictionary` |
| Uses BlueprintCore and Wrath publicized assemblies | Use native Kingmaker blueprint helpers and local game references |
| Uses `HandCrossbow` as the firearm category | Verify category availability/use in Kingmaker; firearm marker remains authoritative |
| Touch AC appears unconditional once the component applies | Add explicit distance and early/advanced firearm rules |
| Rounds are tracked as a buff on the initiator | Replace with per-item loaded state |
| Damaged firearm is a timed buff on the initiator | Replace with per-item condition |
| Empty gun is converted into a miss inside attack processing | Prefer command/action validation plus a defensive event guard |
| Advanced/Rapid Reload paths can effectively ignore loaded state | Every projectile must trace to a loaded chamber and a completed ammo transaction |
| Explosion does not destroy the weapon | Use a recoverable `wrecked` state initially, with explicit repair rules |
| Capacity is effectively one | Design state for capacity N even though the first musket uses one |
| Assets require separate Wrath template package | Keep Kingmaker custom assets optional and investigate later |

## Specific prototype risks observed

The load check reads and removes a `Rounds` buff from the attacking unit. The misfire handler reads a damaged-firearm buff from the unit and adds another timed buff to that unit. Those choices make one character's guns share state and cannot represent two differently loaded or damaged firearms.

The Wrath project targets .NET 4.8.1, Harmony 2, BlueprintCore, publicized Wrath/Owlcat assemblies, and an IL merge step. None of those are assumed portable to Kingmaker.

The mod's own README documents projectile problems, imperfect pistol/revolver posing, floating offhand firearms, capacity-one limitations, and several deed adaptations. Those are useful scope warnings rather than defects to hide.

# Resulting architectural split

```text
Call of the Wild:
  bootstrap
  Kingmaker assembly/toolchain baseline
  blueprint registration
  class/progression precedent
  stable save-ID discipline

Cowboys and Demons:
  firearm subsystem boundaries
  weapon-attack integration
  AC/misfire/reload event candidates
  crossbow presentation fallback
  deed adaptation precedent

New implementation:
  per-item persistent state
  inventory-backed ammunition transactions
  distance-aware touch AC
  schema migrations
  data-driven firearm/ammunition definitions
  isolated services and diagnostics
```
