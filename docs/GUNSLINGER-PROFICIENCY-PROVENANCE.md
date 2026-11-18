# Firearm proficiency provenance and the scope reconciler (RISK-05 / S4 T7)

Status: **boundary record and rule extraction on a review branch, awaiting review. Not merged, not released.** Branch
`codex/risk05-proficiency-provenance`, stacked on the preceding stacked checkpoint `codex/risk04-true-grit-distinct`
(draft PR #183).

## Contract

Epic S4 T7, inventory RISK-05: "The proficiency reconciler may remove an independently granted scoped firearm
proficiency. No current affected provider is identified." Trace what `FirearmProficiencyScopeReconciler` knows about
ownership and find any real independent grant. If one exists, preserve it and let Rapid Reload see it. If none does,
document the extension boundary and the requirement for future providers, and add focused tests. No parallel global
proficiency system, and no invented player failure.

## Every source of the firearm proficiency facts

| Fact | Granted by | Kind |
|---|---|---|
| `KMG.Firearms.FirearmProficiency` (full) | The Gunslinger proficiencies presentation (`GunslingerClassBlueprints.CreateProficiencies`, `AddFacts`) | class-owned |
| | The legacy Exotic Weapon Proficiency (Firearms) wrapper `KMG_ExoticWeaponProficiency_Firearms` (`AddFacts`). Removed from every selection catalog; kept for existing owners. | obsolete owned, honoured |
| | The diagnostic test musket fixture | development only |
| `KMG.Firearms.OneHandedFirearmProficiency` (hidden) | The Pistolero proficiencies presentation (`AddFacts`) | archetype-owned |
| `KMG.Firearms.TwoHandedFirearmProficiency` (hidden) | The Musket Master proficiencies presentation (`AddFacts`) | archetype-owned |
| Any of the three | The Rapid Reload gate scenario's `CreateIndependentProficiencySource` fixtures: request-local, never registered, destroyed afterwards | hypothetical (test-only) |

No other class, archetype, feat, item or integration grants them. The scoped facts are hidden features this mod
created, and no other installed or supported mod references their identities. **No supported independent provider
exists**, so there is no real provider to exercise.

## What the reconciler knows

Each presentation carries one `FirearmProficiencyScopeReconciler`. When it turns on, it removes the other two scopes by
blueprint identity (`Owner.RemoveFact(blueprint)`). The exception is the full proficiency while the legacy wrapper is
owned. Kingmaker 2.1.7b facts carry no general provenance the reconciler could consult. Native `AddFacts` keeps its own
applied-fact list, but there is no cross-component ownership query. So if a future provider granted one of these
blueprints directly, a Gunslinger's or archetype's reconciler could remove it. That is the extension boundary.

The reconciler's job is still needed for this mod's own grants. A pending archetype change rebuilds the preview, and
older saves may hold a stale scope. Removing the mod's obsolete scope there is correct.

## What changed

- `Firearms/FirearmProficiencyScopeRules.cs` (new, engine-free) holds the scope rules:
  - each presentation's expected scope: Gunslinger full, Pistolero one-handed, Musket Master two-handed;
  - the two scopes it removes, in the registered order;
  - the legacy-wrapper exemption, which covers only full proficiency.

  Its documentation states the boundary and the requirement for any future provider: grant a provider-owned fact that
  the firearm proficiency policy is extended to recognize, or bring a recorded provenance the reconciler honours, as
  the legacy wrapper is honoured. Never grant these three blueprints directly and expect them to survive on a
  Gunslinger.
- `FirearmScopedProficiencyBlueprints.AttachScopeReconciliation` builds each reconciler's expected and incompatible
  blueprints from these rules. The registered blueprints, components, order and behaviour are unchanged.
- No identity, text or save change. No new proficiency system.

## Tests

| Command | Result | Time |
|---|---|---|
| `scripts/Invoke-KmgGate.ps1 -Filter 'firearm-proficiency;rapid-reload;archetype;pistolero;musket-master'` | production compiled; 55 of 55 (the first compile failed on a missing `using`, fixed) | 16 s, then 24 s |

New `firearm-proficiency-scope.*` tests:

- Each presentation keeps its own scope and removes exactly the other two.
- The mod's obsolete scopes are reconciled in the registered order, and only the mod's three scopes are ever removed.
- The legacy wrapper spares full proficiency only, not a stale scope.

`tools/validate_icon_catalog.py` passed.

## Native evidence and limits

- There is no real independent provider, so none was exercised. The contract's "supported external grant survives
  and Rapid Reload sees it" stays unverified by design. It is recorded as a future requirement, not as compatibility.
- Rapid Reload's parent gate reads the registered proficiency features, so a provider-owned fact recognized by the
  policy would need that policy extended. The Rapid Reload transaction scenario run for S4 T8 includes the
  request-local independent-source fixtures on non-Gunslinger characters (final-candidate session
  `20261118T014708Z`, cases `F.independent-source-full` and `F.independent-source-two-handed`, both acquiring a child). Those rows show native prerequisites
  accepting a separately granted fact. They are not a provider surviving a Gunslinger's reconciler.
