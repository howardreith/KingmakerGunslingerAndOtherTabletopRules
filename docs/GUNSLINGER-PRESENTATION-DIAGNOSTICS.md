# Firearm visual rigs and item presentation: every identity evaluated, every result reported (INT-04, INT-05)

Status: **development change on a review branch. Not merged, not released.** Branch
`codex/int04-int05-presentation-diagnostics`, stacked on draft PR #171 at its accepted head `7c4cb8ad44` (which is
stacked on #170 and #169). The published v0.0.191 does not contain it. Nothing here is a release qualification.

This is one bounded cut of the Gunslinger remediation epic of 2026-11-15: stories S3 T4 and S3 T5, inventory records
INT-04 (audit finding GS-AUD-0045) and INT-05 (GS-AUD-0046). Current status is kept in
`planning/GUNSLINGER-REMEDIATION-STATUS.md`. It repairs two diagnostics. Gameplay, released identities and authored
assets are unchanged.

## What was wrong

**`disposable-firearm-visual-rigs`** judged the firearm items' visuals with

```csharp
bool exactItems = items.Count == 14 && items.All(value =>
    FirearmWeaponPresentation.HasExactItemVariant(value.Item3, value.Item1, value.Item2));
```

The roster has had sixteen items since the two midgame firearms were added, so the count was false and `&&` never
ran the binding check. Had the count been right, `All` would have stopped at the first bad binding, and the check
itself threw on a symbol with no mapping. The text printed beside the verdict was the catalog's expected variant for
each symbol, not what the item was bound to.

**`observe-gunslinger-presentation`** ended its item check in one conjunction:

```csharp
exact = weapons.Length == 55 && items.Length == 56 &&
    custom.Length == 12 && custom.Contains(policy) && issues.Count == 0;
```

57 weapons and 58 items were there, so the first comparison was false and `custom.Contains(policy)` was never
evaluated. The text checks did run (they come before the conjunction), but one opaque verdict covered five
different things. The expectation of twelve custom enchantments was correct and is unchanged.

## The expected identity sets

Expectations now come from `ProjectItemRosters`, which is built from the declarations that publish each identity.
Nothing a running game reports supplies its own expectation.

| Set | Count | Made of |
|---|---|---|
| Firearm item visuals | **16** | the development Test Musket (1); the production firearms (5: pistol, musket, blunderbuss, rifle, revolver); the magic firearms the magic builder declares (8: Pistol +1, Musket +1, Blunderbuss +1, Duelist's Rebuttal, The River King's Measure, Irovetti's Ovation, The Last Word, Watch at the World's End); the midgame firearms it appends from `MidgameFirearmCatalog` (2: Roadwarden, Dead Reckoning) |
| Presentation weapons | **57** | production firearms (5); magic firearms (10: the eight and the two midgame); `EasternWeaponCatalog` generic items (12); `EasternWeaponNamedCatalog` (18); `ElvenBranchedSpearCatalog` (6); `ElvenBranchedSpearNamedCatalog` (6) |
| Presentation items | **58** | the 57 weapons and the Cord of Stubborn Resolve |
| Custom weapon enchantments | **12** | Reliable; Seeking; the Falling Petal, Mountain-Sunder and Unfixed Form effect enchantments; the Eastern proficiency policy enchantment (policy-only, blank text); the spear's movement-opportunity accuracy; the Boughkeeper, Thornstep, Viper's Reach, Briar-Crowned and First Branch enchantments |

Why sixteen and not fourteen: the firearm roster is the items whose builders apply an exact item visual variant.
Those builders are the Test Musket's, the production firearms' and the magic firearms'. The magic builder gained the
two midgame firearms in 0.0.116, and the pin was not moved. For the same reason the presentation counts are 57 and 58
and not 55 and 56. The Test Musket is development-only; it is in the visual roster by design and is not a
presentation weapon.

What is not in the firearm roster: the 27 generic progression firearms (+2 to +5 and Reliable +1 to +5 for pistol,
musket and blunderbuss). They have visual-catalog mappings too, but they are built by the progression builder, which
applies no item variant of its own, and this scenario has never read them. A domain test pins the arithmetic: the
visual catalog's firearm mappings are exactly the sixteen and those twenty-seven.

Each declared symbol is an active entry of its type in `blueprints/blueprints.json`, the manifest the game deploys
(checked by a domain test: 71 distinct GUIDs for the 58 items, the Test Musket and the 12 enchantments). In the game,
the presentation scenario resolves each expected symbol to its manifest GUID and matches observed blueprints on
GUID; the firearm roster is matched on the symbol each runtime entry carries.

## What changed

**Shared: `IdentityCensus`.** Expected identities against observed ones. It reports the two counts, and separately
which expected identities are missing, which observed ones are unexpected, which were observed or declared more than
once, which expected ones could not be resolved and which observed entries had no identity. A wrong count does not
stop the sets being compared, and a matching count does not excuse a replacement.

**INT-04: `FirearmVisualRigAudit`.** Two assertions replace `firearm-all-14-item-visual-identities`:

- `firearm-item-visual-roster`: the census of the sixteen.
- `firearm-item-visual-bindings`: every item inspected, each reported as `symbol=variant:bound` or
  `symbol=variant:FAIL(reason)`. The reasons are: no approved variant mapped for the symbol; the mapped variant is
  outside the item's firearm family; the item could not be read (with the fault); the game has no item for the
  symbol; the variant's prefab is not loaded; the item has no visual parameters; the item's model is X, not the
  variant prefab Y. An expected identity that was not observed is a binding failure of its own.

The game's items are read source by source. A source that cannot be read becomes a malformed observation in the
census and the other sources are still read. `FirearmWeaponPresentation.HasExactItemVariant` now goes through the
same inspection and no longer throws. `pistol-three-variant-separation` reports an unreadable input instead of
ending the scenario.

**INT-05: `ProjectItemPresentationAudit`.** Five assertions, then the aggregate under the name the check has always
had:

| Assertion | What it judges |
|---|---|
| `project-item-weapon-census` | the 57 declared weapons against the six catalogs' entries (not de-duplicated first, so a repeat is reported) |
| `project-item-census` | the 58 declared items |
| `project-custom-enchantment-census` | the 12 declared custom enchantments against those found on the weapons and their types |
| `project-policy-enchantment` | the policy-only enchantment exists, is among those enchantments, and has a blank name and description |
| `project-item-player-facing-text` | the existing text requirements for every item and every visible custom enchantment |
| `project-item-player-facing-presentation` | all five hold; its text names each failed part |

The text loops are the same checks as before. The policy enchantment's blank-text rule moved from the text issues to
its own assertion.

**Also, without changing its verdict:** `pistol-authored-marker-contract` now reports each sub-condition it reads
(the four marker counts, whether a support-hand target exists, and the muzzle's position beside the expected one).
See "The remaining failure" below.

## Focused tests

`.\scripts\Invoke-KmgGate.ps1 -Filter 'presentation-audit;firearm-fit;eastern-weapons;elven-branched-spear;weapon-visual'`:
production compiled; 53 of 3,025 registered tests selected, 0 failures (17 s). The candidate build's own selection:
67 of 3,025, 0 failures.

Ten tests are new (`presentation-audit.*`). They call the production evaluation with stand-in observations and
assert on the identities and outcomes it reports.

| Test | What it shows |
|---|---|
| `rosters-are-the-declared-identities` | 16, 57, 58 and 12, each from its declarations; every firearm has an approved variant in its own family; the visual catalog's firearm mappings are the sixteen plus the progression firearms; every symbol is an active manifest entry of its type |
| `firearm-visuals-valid-roster-passes` | the supported roster, fully bound, passes both assertions |
| `firearm-visuals-count-mismatch-and-bad-binding-both-reported` | fifteen items and one of them bound to the wrong model: the census names the missing identity and fails, and the bindings still run, name the wrong item with its expected variant and observed model, and fail |
| `firearm-visuals-inspection-continues-after-a-bad-binding` | three bad bindings, the first on the first item: all three are reported with their own reasons and all sixteen items are inspected |
| `firearm-visuals-missing-unexpected-and-replacement` | a replacement that keeps sixteen fails as one missing and one unexpected identity; a duplicate is reported |
| `firearm-visuals-unreadable-and-unmapped-inputs` | an unreadable item, an absent item, an unmapped symbol, a mapping across the family boundary and an unreadable source are each reported while the other items are still inspected |
| `item-presentation-valid-roster-passes` | 57, 58 and 12 with clean text pass all five assertions and the aggregate |
| `item-presentation-count-mismatch-does-not-hide-policy-or-membership` | 55 weapons and the policy enchantment on no weapon: both censuses fail with the missing weapons named, and the policy assertion and enchantment census fail on their own; visible policy text and an unread policy enchantment fail |
| `item-presentation-text-failures-stay-visible` | a short census and two text issues: both are reported, the unrelated results still pass, the aggregate names each failed part |
| `item-presentation-replacement-duplicates-and-unresolved` | a same-count replacement, a weapon read twice, the Cord missing and an extra enchantment are each reported; an unresolved expectation and a malformed observation do not cancel out |

One existing source check (`firearm-fit`) named the retired assertion and now names the evaluation call.

## Native results

Session `scripts/compatibility/loader-compat/sessions/presentation-diagnostics.json` in the disposable game copy,
loader profile `owner-0.33.0`, no other mods, no save; each scenario in a fresh process at the main menu.

| Scenario | Before: PR #171's candidate `15767a89ad2b` (`20261116T030922Z`) | With this change: candidate `35049ef6fde7` (`20261116T031851Z`) |
|---|---|---|
| `disposable-firearm-visual-rigs` | FAIL, 2 of 66 not passing: `firearm-all-14-item-visual-identities` (the stale pin; no binding inspected) and `pistol-authored-marker-contract` (`exact=False`) | **FAIL, 1 of 67 not passing**: `pistol-authored-marker-contract` only |
| `observe-gunslinger-presentation` | FAIL, 1 of 9 not passing: `project-item-player-facing-presentation` (`weapons=57;items=58;customEnchantments=12;issues=<none>` against 55 and 56) | **PASS, 14 of 14** |

The baseline matches what the audit recorded on 0.0.182.

On the new candidate:

- `firearm-item-visual-roster`: PASS. `expected=16;observed=16;count=match;membership=match`.
- `firearm-item-visual-bindings`: PASS. `inspected=16;bound=16;failed=0`, each item listed with its variant: Musket.Service
  for the Test Musket, the musket, Musket +1, The River King's Measure, Watch at the World's End and Roadwarden;
  Pistol.Service for the pistol, Pistol +1 and Dead Reckoning; Pistol.Duelist for Duelist's Rebuttal; Pistol.LastWord
  for The Last Word; Blunderbuss.Service for the blunderbuss, Blunderbuss +1 and Irovetti's Ovation; Rifle.Service;
  Revolver.Service. **This is the first run in which the bindings were inspected at all.**
- `project-item-weapon-census` 57 of 57, `project-item-census` 58 of 58, `project-custom-enchantment-census` 12 of
  12: each `count=match;membership=match`.
- `project-policy-enchantment`: PASS. `KMG_EasternWeapons_ProficiencyPolicyEnchantment`, among the weapons'
  enchantments, name length 0, description length 0. **This condition had been unverified.**
- `project-item-player-facing-text`: PASS, `issues=<none>`.

### The remaining failure

`pistol-authored-marker-contract` (audit GS-AUD-0047; the epic's REP-03, story S3 T8) still fails, on both builds.
It is a structural assertion on the Duelist and Last Word prefabs and has nothing to do with either census: on the
new candidate the roster and all sixteen bindings pass beside it. With its sub-conditions now reported:

| Sub-condition | Pistol.Duelist | Pistol.LastWord |
|---|---|---|
| exactly one each of `KMG_Grip`, `KMG_Support`, `KMG_Butt`, `KMG_Muzzle` | 1, 1, 1, 1 | 1, 1, 1, 1 |
| no `SupportHandTarget` | none | none |
| `Muzzle` at local (0, 0, 0.264) | **(-0.1483, -0.2357, 0.0521)** | **(-0.1483, -0.2357, 0.0521)** |

So the failing condition is the muzzle's position, the same on both prefabs. Whether the prefabs or the expected
position is wrong is not established: that is S3 T8's question and was not worked here. The assertion, its verdict
and the assets are unchanged.

## Identities

| Item | Identity |
|---|---|
| Product commit | `baad52d579` (the only product change after `7c4cb8ad44`) |
| Candidate | built once at `35049ef6fde7`, 415 s; package SHA-256 `aae60220ea5e8b3348f9bc7d0149baaa80e5a63f5bc7f7b0fedb8959d3ec9141`, DLL `6af1435ab99dfd27e1dae2cea0486426b998c0455fcca9be793bee01f62220c5` |
| Baseline | PR #171's candidate `15767a89ad2b`, package `462696cc2bd30874d89df251a7bdcb289080e6fb7c8eb79c2948a1c9a3e7eedc`, DLL `9519bb86b864480de2d21f3bd0330317d96dc5a879a451da614d3e02ff5fc5a3`, not rebuilt |
| Session file | `presentation-diagnostics.json`, SHA-256 `18a3c6b2a4adf8aca5fbe3301c0ea6d26526bfa48ae40b3b1c47d53036e3327e`, commit `35049ef6fd` |
| Harness | unchanged since `df7f11156a`; DLL `50989f667d8f3fcd4c77b59a3881a5b2d961cae0cfa2f6dc53f337e7aa38cbf5` |
| Fixture | none: no save, no other mods |

Both sessions reported the owner's 95 save files and installation unchanged and the registry restored. The session
verdict is FAIL in both because one scenario fails; that is the marker assertion, kept as it is.

Validation time: about 12 minutes (two gate runs 18 + 17 s, baseline session 136 s, candidate build 415 s, candidate
session 133 s).

## Limits

- Both scenarios ran with no other mods. They were not run in the owner's mod stack or through the live guarded
  launcher.
- No failing census or binding was produced in the game; the failure reports are covered by the domain tests.
- The firearm roster is matched on the symbol each runtime entry carries, not on GUID.
- The 27 progression firearms' visuals are still read by no scenario.
- A structured check of identities, model references and text. It establishes nothing about how anything looks.
- `pistol-authored-marker-contract` still fails; its cause is not established.
- The complete domain suite was not run for this cut.
