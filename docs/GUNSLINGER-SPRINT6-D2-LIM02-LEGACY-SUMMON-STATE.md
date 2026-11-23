# Sprint 6 D2 / LIM-02: legacy summon state (Earth Elemental preference, Griffon duplicate Charge, Bebelith marker)

Status: **D2 accepted by manual review (2026-11-23)** at head `189b68a9fc`, as two explicit until-expiry retention
dispositions and a current-build native diagnostic for the inert Bebelith marker. Authentic pre-fix-save qualification
remains **unexecuted**; LIM-02 is not fully natively qualified. Section 8 records the decision and governs where it
differs. Nothing is merged or released.

| | Identity |
|---|---|
| Branch | `codex/s6-d2-lim02-legacy-summon-state`, stacked on D1 acceptance `5fdc422265` (PR #213) |
| Product | **unchanged**: accepted D1 `634a6efdd3`, package `862027f189cf19656aa288264531fd81acbe91f18910336560eb7352da0fdc37`, DLL `65a0b60f43da49e7217414ad8258e7f02e0546916060a1d4b8bdb7ce175c43ef` |
| Harness | `f63d4d5ae3` (`kmgLegacyMarker`, `tools/LoaderCompatHarness/LegacyMarkerProbes.cs`), build `df6bca9000ceed5afd71f3be24240101e4b3b71fa258eccfcabb803eb88de0b4` |
| Fixture | `KMCS_AFTER_NORMAL_012_MODONLY.zks` (diagnostic control only, see section 2) |
| Native session | `20261123T182444Z-owner-0.33.0-d2-lim02-marker` |

## 1. Outcome per owner

| Owner | Disposition | Player-visible result |
|---|---|---|
| Greater/Elder Earth Elemental attack preference | **Retained until summon expiry** (explicit retention, no code change) | A KMG Greater or Elder Earth Elemental saved before v0.0.182 still prefers lightly armored or sundered targets until it expires. Its Sunder was already removed on load. Fresh summons use the ordinary-attack brain. |
| Griffon duplicate Charge | **Retained until summon expiry** (explicit retention, no code change) | A KMG Griffon saved before v0.0.180 may still show two identical Charge actions until it expires. Fresh Griffons show one. |
| Bebelith `DismantledArmor` marker | **Existing behavior kept** (no code change): identity registered, component deserializable, zero AC | A character marked in an older save keeps a "Dismantled Armor" marker described as a retired legacy effect. It has no scheduled duration and the withdrawal does not remove it; it changes no AC (current-build diagnostic). |

No attributable in-scope defect was found that a narrow repair could correct within the stated constraints, so no product change was made and no candidate was built.

## 2. Fixture intake (one bounded search)

**Search.** 353 candidate `.zks` paths under `C:\Dev\KingmakerGunslingerLab` (game copies, the frozen fixture set, runtime evidence, repository artifacts) were opened in place, without extraction. 14 are known non-archive stubs (the teleportation save-protection test files) and could not be searched; the remaining **339 archives** were searched for the five LIM-02 GUIDs:

- KMG Greater Earth Elemental unit `17c4fcc106df43af980fcea7a0877e04`;
- KMG Elder Earth Elemental unit `2bdfdec402384e689351bad9a0d2d8af`;
- KMG Griffon unit `4b55d1f8da5e4a3ea23f9a95191f8737`;
- KMG Bebelith unit `9d7d36e71ea141258509b8a32557577e`;
- marker buff `KMG.Summoning.Special.Bebelith.DismantledArmor` `736a349933f24cc2b11bc284b6e559cb`.

**Result:** none of the 339 searched archives contains any of them.

The pre-fix packages exist, but nothing was built from them:

- the v0.0.181 release asset `KingmakerGunslinger-0.0.181-cleric-summon-alignment.zip`;
- several local `0.0.181` development candidates.

Producing a historical fixture was **not admitted and not attempted**. That is the fixture-admission boundary of the current isolated workflow, not a new owner-approval requirement:

- No guard was identified that needs owner action. The loader-compat runner hashes the candidate package and refuses pre-existing state (a running game, locks, a pending journal); it has no version gate on the candidate.
- Untested: whether the current harness probes, which reflect into current product internals, would operate against a historical package.
- The v0.0.181 packages **cannot** reproduce the Griffon duplicate: the explicit grant was removed before v0.0.180. A v0.0.161–v0.0.179 package would be needed; its local availability was not checked.

No version guard was bypassed. The search is not repeated.

**Precise missing inputs** (any one makes the matching owner qualifiable):

1. **Earth Elementals.**
   - The save is written by a released KMG build from v0.0.101 to v0.0.181 (the Sunder withdrawal and brain change shipped in v0.0.182).
   - It holds a living KMG Greater (`17c4fcc1…`) or Elder (`2bdfdec4…`) Earth Elemental summon with time left on its native `SummonedUnitBuff`.
   - Its saved `UnitBrain.Blueprint` is the native `GreaterEarthElementalBrain` `d8342bca1ce38294bbf47f9566e6e2aa`.
   - Its `Actions` are the native `SunderArmorAiAction` `23bcb18f…` and `GreaterEarthElementalAttackAiAction`, and it carries the native Improved Sunder `9719015e…`.
2. **Griffon.**
   - The save is written by a released KMG build from v0.0.161 to v0.0.179 (the explicit grant was removed in R5 `057ec10f1a`, released in v0.0.180).
   - It holds a living KMG Griffon (`4b55d1f8…`) with two `ChargeAbility` `c78506dd0e14f7c45a599990e4e65038` ability facts.
3. **Bebelith marker.** The save is written by a build before v0.0.182, in which a Bebelith's Dismantle Armor fired and left the marker `736a3499…` on a character.

**Requirements common to all three:**

- installed Kingmaker 2.1.7b, Steam App ID 640820;
- UMM owner profile 0.33.0, through the loader-compat game copy;
- the KMG mod-only profile;
- provenance: the writing build's package hash and commit;
- the save's own hash;
- the actor's `UniqueId`.

## 3. Ownership analysis

### 3.1 Earth Elementals: the retained attack action is not removable without replacing the brain

**Saved representation** (installed `Kingmaker.UnitLogic.UnitBrain`):

- The brain persists `Blueprint` (the brain blueprint), `Actions` (a list of `AiAction`, each referencing its blueprint) and `AvailableActions`.
- The constructor builds `Actions` from `unit.Blueprint.Brain` once, at unit creation.
- `UnitDescriptor` has no load-time re-synchronization with the current unit blueprint's brain.

So a pre-v0.0.182 KMG elemental keeps the native `GreaterEarthElementalBrain` reference and its action instances. After the v0.0.182 fixer (`$KMG_R1_WithdrawnCapabilities_GreaterEarthElemental` / `…_ElderEarthElemental`), it holds exactly `GreaterEarthElementalAttackAiAction`.

That end state is recorded in `planning/summon-menu-order-and-sunder/STATUS.md`. It comes from an induced legacy state, with a direct `OnPostLoad` call, no cold load, and the `sunder-legacy-earth-elemental` outcome `Proven`.

**Distinguishability.** The action is the native blueprint shared by native Greater/Elder Earth Elementals. On an exact KMG owner, it can be attributed to the former KMG brain reference by three facts together:

- the unit blueprint is exactly the KMG unit;
- the saved brain blueprint is the native Greater Earth Elemental brain;
- the current KMG unit blueprint's brain is the Dumb Monster Brain.

This is source inference. Another mod that changed the native brain's actions before the unit was created would have put its contributions into the same list, indistinguishably.

**Why it is not repaired.**

- The installed `AiBrainController.SelectAction` chooses only from `unit.Brain.AvailableActions`. It has no fallback attack.
- For the **unmodified historical brain** (native `GreaterEarthElementalBrain` = [`SunderArmorAiAction`, `GreaterEarthElementalAttackAiAction`]), the retained action is the old elemental's only autonomous action after the Sunder withdrawal. Removing it alone would leave such a summon idle in combat, which is worse for the player than a target preference. If another mod changed that brain before the unit was created, the saved list may differ.
- A targeted substitution (replacing that exact action instance with the native `AttackAiAction`) is conceivable. It is **unimplemented and unqualified**; retention is the selected policy, not a categorical prohibition.
- The preference is bounded: the action still attacks with the elemental's own natural attacks and only weights its target choice (AttackTargetsPriority, LightArmorConsideration, HasBuffSunder). It ends when the summon expires.

**Disposition:** retain until summon expiry, disclosed.

- **No reapplication** (source): a fresh summon's brain is built from the current unit blueprint (Dumb Monster Brain).
- The earlier native `sunder-earth-elemental-fresh` observation found the fresh brain = [`AttackAiAction`].
- No autonomous target selection with the retained action has been observed. A commanded attack would not demonstrate it.

### 3.2 Griffon: the two Charge instances are indistinguishable

**Saved representation.**

- `UnitDescriptor.Initialize` (installed 2.1.7b) first calls `AddFact(BlueprintRoot.Instance.SystemMechanics.ChargeAbility)` for every unit, then adds each `Blueprint.AddFacts` entry.
- Before R5 (`057ec10f1a`, released in v0.0.180), the KMG Griffon's `AddFacts` held the same native `ChargeAbility` `c78506dd…`. That produced a second `Ability` fact of the same blueprint.
- Both instances were added with no parent context, source item or source cutscene.
- Their persisted fields are `Blueprint`, component data, `Initialized`, `Active`, `m_Hidden`, `Data` and `StickyTouch`, so the representations are identical.
- The only difference is position in the fact list, which reflects creation order, not provenance. A foreign mod that grants Charge to the unit would also add an identical instance.

**Why it is not repaired.**

- The R1 withdrawal fixer removes by blueprint, which would remove both instances, including the engine's valid Charge.
- Removing "the later one" would invent provenance from ordering.
- The duplicate is mechanically equivalent: same ability, same implementation. Its only effect is a second identical action. Pounce, attacks and the Griffon's other grants are separate facts and are unaffected.

**Disposition:** retain the duplicate until summon expiry, disclosed. No fresh Griffon receives it: the build guard refuses an explicit Charge grant, and R5 `griffon-one-charge` observed one Charge on a live fresh Griffon.

### 3.3 Bebelith marker: already inert; identity kept

**Owner.** The marker's owner is the affected character, not the Bebelith. The marker was applied by v0.0.181 as `AddBuff(DismantledArmor, <Bebelith combat-traits context>, null)`. A null duration gives it no scheduled end, so it does not expire with the summon, and the withdrawal does not remove it. Other buff-removal routes were not examined.

**Representation.** `BebelithDismantledArmorComponent` has no serialized fields, at v0.0.181 and now (the same type and namespace). So a marker serialized by the current build has the same component data as a historical one. This is source inference: no historical instance was loaded.

**Current behavior** (since `ec33e2b355`, released in v0.0.182):

- The component's `RuleCalculateAC` handler takes its penalty from `BebelithDismantledArmorPenalty(withdrawn, …)`. While withdrawn, that returns 0 without reading the armor piece.
- The blueprint, its GUID and its component stay registered.
- The description reads as a retired legacy effect.

**Disposition:** keep. The withdrawal does not delete the marker, and it is not described as expiring.

## 4. Native diagnostic control: the Bebelith marker

Session `20261123T182444Z-owner-0.33.0-d2-lim02-marker`: **PASS**, 2/2 launches.

- **mark:** 80 s.
- **reload:** 75 s in a fresh process.
- **Total:** 168 s.
- **Restoration:** transaction completed, game settings applied, owner files verified.
- **Candidate:** the accepted D1 package. No product build.

**Scope: a diagnostic control, not a qualification.**

- The marker is constructed by the current build on a disposable `DefaultPlayerCharacter`, not cold-loaded from a pre-fix save.
- The character wears `ScalemailStandard`.
- A real Bebelith is summoned through the product execution route, and its combat-traits buff context is used as the marker's parent context, as v0.0.181 did.

**Predeclared expectations:**

- AC with the marker equals AC without it;
- the marker component runs during the real native `RuleCalculateAC`;
- the component adds no modifier;
- armor and its AC contribution are unchanged;
- the marker is present exactly once, survives the Bebelith's native expiry, and deserializes with its component after a fresh load.

The observer counts the component's `OnEventAboutToTrigger` calls, and the marker-sourced AC modifiers present immediately after it. That is the point where a temporary rule modifier would still be on the stat.

| Launch / point | Rule TargetAC | Stat AC | Armor modifiers | Marker modifiers during rule | Component calls | Markers |
|---|---|---|---|---|---|---|
| mark: without marker | 15 | 15 | 5 | 0 | 0 | 0 |
| mark: with marker | 15 | 15 | 5 | 0 | 1 | 1 |
| mark: after the Bebelith expired natively | 15 | 15 | 5 | 0 | 1 | 1 |
| reload: first calculation | 15 | 15 | 5 | 0 | 1 | 1 |
| reload: repeat after four wall-clock seconds unpaused | 15 | 15 | 5 | 0 | 1 | 1 |

**Asserted in the mark launch** (against the without-marker row):

- equal rule AC and stat AC with the marker, and after the Bebelith expired;
- unchanged armor name and armor-modifier total;
- exactly one marker, zero marker-sourced modifiers, the component ran;
- the Bebelith was destroyed by native expiry;
- the save completed by the enforced predicate: the save callback fired and no save commit was in progress. (The wait loop also waited for loading to end, but that is not part of the asserted predicate.)

**Asserted in the reload launch:**

- the marker deserialized exactly once with `KingmakerGunslinger.Summoning.BebelithDismantledArmorComponent`;
- equal rule AC between the two repeated calculations;
- exactly one marker, zero marker-sourced modifiers, the component ran, in each calculation;
- armor present.

Stronger reload equalities (stat AC, armor-modifier total, armor name, and AC equal to the mark launch) are **captured comparisons** of recorded outputs, not reload assertions.

**Captured:**

- **The marker:**
  - its caster is the Bebelith (`6db5c76d…`), and after reload no live caster is present;
  - its end time is the permanent maximum (`10675199.02:48:05`) before and after reload;
  - its GUID `736a3499…` is unchanged across the save.
- **The reload comparison:**
  - the reload AC (15), stat AC and armor values equal the mark launch's; this is a comparison of the two launches' recorded outputs;
  - the character was found by its custom name;
  - its `UniqueId` (mark launch `4aba619c…`) was not compared in the reload launch.

**Disclosed:**

- The Bebelith's deadline was moved to now, so expiry was observed without waiting out the summon duration.
- The withdrawn policy has no native positive control here: no penalty-enabled build was run. The penalty seam's control is the focused test `summon-sunder.bebelith-legacy-penalty-inert`, a model test.

**Unexecuted:**

- a pre-fix cold load;
- AC against attack types other than melee;
- full item-instance identity.

## 5. Evidence classes

| Claim | Class |
|---|---|
| No admitted lab save carries any LIM-02 owner | Captured (one bounded read-only scan) |
| Brain, fact and marker persistence shapes; Charge added for every unit; AI selects only from available actions | Source inference from the installed 2.1.7b decompile |
| Earth Elemental post-fixer brain = [`GreaterEarthElementalAttackAiAction`] | Earlier induced native control (`sunder-legacy-earth-elemental`), not a cold load |
| Earth Elemental autonomous targeting with the retained action | **Unexecuted** |
| Griffon duplicate representation identical; fixer would remove both | Source inference; R5 record |
| Griffon real Charge and expiry with a saved duplicate | **Unexecuted** (no fixture) |
| Marker adds no AC, survives expiry and fresh load, deserializes | Native diagnostic control (asserted), not a pre-fix load |
| Marker component data identical to the historical shape | Source inference (v0.0.181 vs current type) |

## 6. Validation and time

- **Product:** none changed. No focused gate, production compilation or candidate build was needed; the accepted D1 candidate is reused.
- **Harness:** compiled in about 1 s, with one fix: a missing `Kingmaker.Enums` namespace.
- **Native:** 168 s, one session.
- **Total:** well inside the 15-minute target.

## 7. Recommendation

1. Accept the two explicit until-expiry retentions (Earth Elemental preference, Griffon duplicate Charge).
2. Accept the Bebelith marker's existing inert behavior.
3. LIM-02 qualification against authentic saved state stays open until one of the named pre-fix saves is admitted.

Possible follow-up for the owner, not done here: if a pre-v0.0.182 save becomes available, the Earth Elemental case could be qualified for autonomous targeting. A brain substitution would need an explicit owner decision.

G5 and LIM-03 were not started. REP-05/REP-06 originals were not searched for.

## 8. Review decision and corrections (2026-11-23)

**D2 is accepted** at head `189b68a9fc` as:

- an explicit until-expiry retention disposition for old Earth Elemental targeting;
- an explicit until-expiry retention disposition for the old Griffon duplicate Charge;
- a current-build native diagnostic supporting the retained Bebelith marker's inert behavior.

Authentic pre-fix-save qualification remains **unexecuted**. These legacy paths are not marked PASS, and LIM-02 is not
fully natively qualified. No D2 product migration or historical fixture production is required before D3. The product
remains the accepted D1 candidate (`634a6efdd3`, package `862027f1…0fdc37`, DLL `65a0b60f…5c43ef`).

Corrections (applied in place above):

1. The sole-action argument is scoped to the unmodified historical brain. A targeted substitution of one exact action
   is unimplemented and unqualified; it was not categorically prohibited. Retention is the selected policy.
2. Historical-fixture production was not admitted or attempted. No guard needing owner action was identified. The
   v0.0.181 packages cannot reproduce the Griffon duplicate.
3. Scan census: 353 candidate paths, 14 known non-archive stubs, 339 archives searched.
4. Armor and stat preservation assertions are scoped to the mark launch. The reload enforces repeated rule AC, marker
   and component presence, no marker-sourced modifier, and armor presence; stronger equalities are captured.
5. The reload wait is four wall-clock seconds unpaused. Save completion is stated by the enforced predicate.
6. The marker has no scheduled duration and is not removed by the withdrawal; it is not universally irremovable.
7. The limits stand: no native positive penalty control, no historical load, and no actor `UniqueId` comparison
   across the reload.

**Harness cleanup correction (harness only).** `LegacyMarkerProbe.Mark` took the marked character out of its cleanup
list before saving and only put it back after a successful save, so a save refusal or an exception left it unowned.
It now stays owned throughout; the save operation is unchanged. The existing PASS stays attached to harness build
`df6bca90…88de0b4`. The corrected harness was compiled only (build `0970dc9c…53d51d`); D2 was not rerun, and no new native qualification is
implied.
