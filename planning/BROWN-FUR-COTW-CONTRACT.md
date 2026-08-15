# Brown-Fur / Call of the Wild Contract

Status: implementation investigation; runtime reconciliation not yet qualified.

## Engineering authority

- Verified engineering base: `a8b19fe39285da44ac443b7bcbd217870ec6ffb6`
- Cleanup human acceptance: **PENDING / intentionally deferred**
- Brown-Fur authorization: explicit user override permits development from the
  pre-human cleanup candidate.
- This authority does not accept or complete the deferred cleanup sprint.

## Inspected CotW fingerprint

The installed investigation subject was inspected without adding a compile-time
or package dependency.

| Evidence | Value |
| --- | --- |
| CotW mod ID | `CallOfTheWild` |
| CotW mod version | `1.14.4c-2.1` |
| Assembly full name | `CallOfTheWild, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null` |
| DLL SHA-256 | `4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915` |
| DLL MVID | `8caab254-aacf-4811-8093-44b9184e6e53` |
| Settings SHA-256 | `24CC3F80269992A53EBBFD1F5986E5AAB056841D6B2F43D8E22E764CDB73F6E8` |
| Inspected balance mode | `balance_fixes=true` |
| Blueprint catalog SHA-256 | `F227B1C302DC8DB9773DE483369407ECC4A154B4082D83C97FCFE0C65912A4F4` |

These binary hashes are evidence, not the sole compatibility gate. Runtime
publication requires every structural check below to agree.

## Required blueprint identities

| Surface | Canonical GUID |
| --- | --- |
| Arcanist class | `19c3cf3d51cf4cbf9a136a600c26585a` |
| Arcanist progression | `2d28526efc2e4a9cb6a84c85267fb344` |
| Arcanist casting spellbook | `0c21cfcab6ce4395bd4df330ab3cf715` |
| Arcanist memorization spellbook | `ab76417567444a6cb87d9d53e9752955` |
| Full arcane reservoir resource | `3b775ee982444493b3de8f7bc31bd872` |
| Arcane reservoir feature | `06427aa76d584db6915900623575439e` |
| Magical Supremacy feature | `2d86a417ab1542f98a8444b2b97d4951` |

The resolver must obtain these objects from CotW's live public/static Arcanist
fields, cross-check their GUIDs and graph relationships, and independently
inspect the resolved progression. A familiar GUID alone is insufficient.

## Reflection contract

The inspected `CallOfTheWild.Arcanist` type exposes the required static fields:

`arcanist_class`, `arcanist_progression`, `arcanist_spellbook`,
`memorization_spellbook`, `arcane_reservoir_resource`, `arcane_reservoir`,
`arcane_exploits`, and `magical_supremacy`.

The safe lifecycle seam is the static zero-argument `createArcanistClass()`
method. Its decoded call graph creates the class, spellbooks, progression,
registers the class, creates all CotW Arcanist archetypes, assigns the final
archetype array, and completes related registrations before returning. Brown-Fur
therefore uses a reflection-resolved postfix, plus one bounded first-update
fallback, and never polls every frame.

The inspected `CallOfTheWild.SharedSpells` bridge exposes exactly:

- `static bool canShareSpell(Kingmaker.UnitLogic.Abilities.AbilityData)`
- `static bool isValidShareSpellTarget(Kingmaker.EntitySystem.Entities.UnitEntityData, Kingmaker.UnitLogic.UnitDescriptor)`

Changed, missing, overloaded, or otherwise ambiguous signatures block Brown-Fur
publication. Any reuse remains cast-scoped and Brown-Fur-owner-scoped; it does
not install a global replacement for Kingmaker targeting.

## Progression contract

The actual exploit-bearing `LevelEntry` objects are authoritative. The settings
file is fingerprint evidence only.

| Shape | Resolved exploit levels | Replaced opportunities |
| --- | --- | --- |
| Normal | `1,3,5,7,9,11,13,15,17,19` | `3,9` |
| Balance fixes | `1,4,7,10,13,16,19` | `4,10` |

The pure progression policy rejects null, empty, duplicate, unordered,
out-of-range, partial, and unknown schedules. An unknown future CotW schedule
blocks Brown-Fur only; unrelated package modules remain available.

## Fail-closed checks

Publication requires one active CotW mod entry and assembly, resolved Arcanist
class and progression, both spellbooks, reservoir, exploit selection, Magical
Supremacy, exact Shared Spells signatures, a readable current archetype array,
a recognized exploit schedule, and a nonempty structurally inventoried
Transmutation spell set.

CotW absence produces `Unavailable` rather than a package bootstrap failure.
Any installed-but-failed check produces `Blocked` with the exact failed check.
The runtime coordinator and publication transaction remain pending at this
checkpoint; this document does not claim in-game compatibility yet.
