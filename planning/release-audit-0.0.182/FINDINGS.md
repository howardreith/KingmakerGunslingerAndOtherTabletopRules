# Audit findings — coordinator-maintained register

Campaign KMG-AUD-20261111T170737Z. Frozen candidate aa91f46571eb239df36e0233082153f2cba01899,
version 0.0.182. IDs are assigned only by the coordinator. Every material status change is
appended to AUDIT-OUTCOMES.md. Severity is a provisional impact judgement, not proof.

## Ranked index

| ID | Kind | Severity | Evidence strength | Feature | Title | Disposition | Evidence |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GS-AUD-0004 | CONTRACT-QUESTION (was PRODUCT-DEFECT) | **S3, downgraded from S1** | omission source-demonstrated; runtime-null premise **NATIVELY DISCONFIRMED** (EV-000014) | Shared buff/area lifecycle (A15); Gunslinger deeds, Eastern weapons, Elven spears | 21 KMG buff identities omit `FxOnStart`/`FxOnRemove`, breaking the project's own stated invariant | OPEN — **CORRECTED EV-000012** | EV-000004, EV-000012 |
| GS-AUD-0001 | ENVIRONMENT-BLOCKER | S2 | native-observed | Whole product / release readiness | The installed mod on OLD LAPTOP is 0.0.117, not the 0.0.182 release candidate | OPEN | EV-000002 |
| GS-AUD-0003 | VERIFICATION-GAP | UNASSESSED | observed-once (external), local evidence absent | A15 incident reconciliation | The supplied 2026-11-11 incident log does not exist on this host; last local game run was 2026-11-07 | OPEN | EV-000004 |
| GS-AUD-0006 | PRODUCT-DEFECT | S2 | source-demonstrated | Feature-module settings (A19) | A failed settings-migration write misclassifies a valid older file as malformed and resets all 13 modules to ON | OPEN | EV-000007 |
| GS-AUD-0007 | CONTRACT-QUESTION | S3 | source-demonstrated | Feature-module settings (A19) | A future settings schema makes the whole mod fail to load, not just the settings file | OPEN | EV-000007 |
| GS-AUD-0005 | HARNESS-DEFECT | S3 | native-observed (executed gate) | Release validation gate | The broad gate depends on an untracked ~99 MB local IL dump; a fresh checkout fails 23 tests with an opaque path error | OPEN | EV-000005 |
| GS-AUD-0015 | PRODUCT-DEFECT | S2 | source-demonstrated | Brown-Fur Transmuter (A11) | Persisted Powerful Change bonus silently degrades to +4 when the caster is not in the loaded scene | OPEN | EV-000009 |
| GS-AUD-0013 | CONTRACT-QUESTION | S2/S3 | source-demonstrated, coordinator-verified | Gunslinger firing (A06) | Misfire suppression is released only by player input; an AI companion stays locked out after repair | OPEN | EV-000009 |
| GS-AUD-0021 | VERIFICATION-GAP | UNASSESSED (S1 if it fails) | source-demonstrated absence | Firearm persistence (A04) | Cold-load firearm token survival is unproven and fails silently | OPEN | EV-000009 |
| GS-AUD-0008 | DOCUMENTATION-DRIFT | S3 | source-demonstrated | Firearm maintenance (A04) | Stale Broken-or-Wrecked field-repair prose in README and seven other places | OPEN | EV-000009 |
| GS-AUD-0016 | CONTRACT-QUESTION | S3 | engine behaviour proven from IL; reachability not demonstrated | Save/load robustness (A11/A13) | Two UnitParts validate by throwing inside an engine callback that has no catch handler | OPEN | EV-000009 |
| GS-AUD-0018 | HARNESS-DEFECT | S3 | source-demonstrated | Cord / Urban Barbarian (A13/A14) | Thirteen tests pass against two dead policy classes the shipped mod never executes | OPEN | EV-000009 |
| GS-AUD-0009 | PRODUCT-DEFECT (latent) | S3 | source-demonstrated; reachability LOW | Firearm state (A04) | Capacity-6 Advanced Revolver cannot persist state past one round; reload silently rolls back | OPEN | EV-000009 |
| GS-AUD-0012 | PRODUCT-DEFECT (diagnostics) | S3 | source-demonstrated | Gunslinger's Dodge (A06) | GunslingerDodgeRuntime is dead code, so its pre-AC evidence can only read zero | OPEN | EV-000009 |
| GS-AUD-0017 | DOCUMENTATION-DRIFT | S3 | source-demonstrated | Cord of Stubborn Resolve (A14) | Combat log says "nonlethal damage" where the approved contract says nonlethal-EQUIVALENT | OPEN | EV-000009 |
| GS-AUD-0019 | DOCUMENTATION-DRIFT | S3 | source-demonstrated | Brown-Fur Transmuter (A11) | Runtime matrix asserts save/reload PASS while the later startup record says cold reconstruction NOT_RUN | OPEN | EV-000009 |
| GS-AUD-0010 | CONTRACT-QUESTION | S3 | source-demonstrated | Firearm maintenance (A04) | Broken-only field repair is enforced only in callers, not at the state transition | OPEN | EV-000009 |
| GS-AUD-0011 | CONTRACT-QUESTION | S3 | source-demonstrated | Deadeye (A06) | Deadeye charges a flat 1 Grit and discards the per-increment cost it computes | OPEN | EV-000009 |
| GS-AUD-0014 | VERIFICATION-GAP | UNASSESSED | source-demonstrated absence | Firearm maintenance / action economy (A04/A06) | Three firearm claims are not settleable from source (rest/autosave order, reload action borrowing, token survival) | OPEN | EV-000009 |
| GS-AUD-0020 | VERIFICATION-GAP | UNASSESSED | source-demonstrated | Urban Barbarian / Elemental races (A12/A13) | Acceptance checklists carry 38 unchecked items and two explicit native waivers that nothing closes | OPEN | EV-000009 |
| GS-AUD-0025 | PRODUCT-DEFECT | S2 | source-demonstrated, coordinator-verified | Expanded summoning (A08) | Untemplated alternate-caller summons keep the donor's inherited celestial/fiendish template | OPEN | EV-000013 |
| GS-AUD-0027 | PRODUCT-DEFECT | S2 | source-demonstrated | Spider Summoner / RU (A08) | The Races Unleashed version pin fails open instead of disabling the integration | OPEN | EV-000013 |
| GS-AUD-0030 | CONTRACT-QUESTION + DOC-DRIFT | S2 | source-demonstrated, coordinator-verified | Gunslinger progression (A05) | Nimble granted at 2/6/10/14/18 (tabletop 3/7/11/15/19) and the fidelity matrix calls it EXACT | OPEN | EV-000013 |
| GS-AUD-0031 | DOCUMENTATION-DRIFT | S2 | source-demonstrated | Vendors / acquisition (A17) | Better Vendors doc describes four vendor rows the candidate does not publish; a second doc contradicts it | OPEN | EV-000013 |
| GS-AUD-0024 | CONTRACT-QUESTION / lead | UNASSESSED | supported-risk — hypothesis, no reproduction | A15 incident | HYPOTHESIS: the incident fits a null `Blueprint` on a deserialized buff better than a null `FxOnStart` | OPEN | EV-000012 |
| GS-AUD-0038 | HARNESS-DEFECT | S3 | native-observed, intermittent (FAIL in batch, PASS alone) | Pistolero deeds (A06) | `up-close-critical` fails only when a Targeting scenario runs first in the same process | OPEN | EV-000014 |
| GS-AUD-0023 | VERIFICATION-GAP | S3 | native-observed on 0.0.182 | Gunslinger's Dodge (A06) | Dodge still has no working native acceptance route; known fixture limitation confirmed present | OPEN | EV-000012 |
| GS-AUD-0028 | VERIFICATION-GAP | UNASSESSED (S1/S2 if confirmed) | source-demonstrated absence | Summon lifecycle (A10) | Grapple/hold release on death, dismissal and scene change is unverified | OPEN | EV-000013 |
| GS-AUD-0026 | HARNESS-DEFECT | S3 | source-demonstrated | Expanded summoning (A08) | The composed-action contract classifies the template-removal action as Other and never compares it | OPEN | EV-000013 |
| GS-AUD-0029 | DOCUMENTATION-DRIFT | S3 | source-demonstrated | Alternate summoning (A08) | ALTERNATE-SUMMONING-SOURCES.md still states the superseded name-first menu order as current | OPEN | EV-000013 |
| GS-AUD-0032 | PRODUCT-DEFECT | S3 | source-demonstrated | Gunslinger progression (A05) | A base Gunslinger's 4th Gun Training pick (level 17) has no selectable option and grants nothing | OPEN | EV-000013 |
| GS-AUD-0033 | CONTRACT-QUESTION | S3 | source-demonstrated | True Grit deeds (A05) | "Select two different deeds" is enforced only by test-only code | OPEN | EV-000013 |
| GS-AUD-0034 | PRODUCT-DEFECT (compatibility) | S3 | source-demonstrated | Feature selection (A17) | Both KMG ExtractSelectionItems postfixes silently discard third-party selection items | OPEN | EV-000013 |
| GS-AUD-0035 | PRODUCT-DEFECT | S3 | source-demonstrated | Teleportation UI (A16) | A destination row with no bindable reader is offered and does nothing but log | OPEN | EV-000013 |
| GS-AUD-0036 | PRODUCT-DEFECT | S3 | source-demonstrated | Firearm crafting (A17) | Crafting rollback loses the original exception and cannot undo a detected over-refund | OPEN | EV-000013 |
| GS-AUD-0037 | VERIFICATION-GAP | UNASSESSED | historical-only | Progression + teleport UI (A05/A16) | Native evidence for these areas is pinned to earlier commits and was not re-run on the candidate | OPEN | EV-000013 |
| GS-AUD-0022 | HARNESS-DEFECT (this audit's own) | S3 | native-observed | Audit adapter | The adapter's `umm-completion-fails` fixture is not offline: it launched a real native transaction | CLOSED-MITIGATED | EV-000010 |
| GS-AUD-0002 | ENVIRONMENT-BLOCKER | S3 | native-observed AND mechanism natively demonstrated (EV-000010) | Host environment / UMM | Live UMM `Params.xml` was left in the 2026-11-07 test-profile state, listing only 2 of 11 installed mods | OPEN | EV-000002 |

---

## GS-AUD-0004 — Twenty KMG buff factories still omit the empty FX links the engine dereferences without a null guard

> ### ⚠ CORRECTED 2026-11-11T18:10Z (EV-000012) — READ THIS FIRST
>
> This finding was originally filed as **PRODUCT-DEFECT / S1** and asserted that the omission
> below explains the 2026-11-11 area-load crash. **That causal claim is WITHDRAWN and the
> severity is downgraded to S3.**
>
> **Why.** Continued disassembly established that
> `Kingmaker.UnitLogic.Buffs.Buff::Activate()` calls `SpawnParticleEffect()` directly, and that
> `IsParticleEffectValid()` returns **false** for a buff that has never spawned an effect — so
> the `FxOnStart` dereference is reached on the **first ordinary application** of such a buff,
> not only during area load. If `FxOnStart` were genuinely null for these blueprints, then
> Gunslinger's Dodge, Deadeye and Bleeding Wound would raise `NullReferenceException` the first
> time a player used them, every time. They are shipped features with seven documented
> playtests behind them. **The runtime-null premise is therefore not established, and is argued
> against by the product working.**
>
> **What survives (verified, unchanged):** the enumeration of 21 identities that omit the
> fields, the absence of any downstream normalisation, and the inconsistency with the project's
> own written invariant and its two prior point fixes. That is a real latent consistency defect
> worth fixing — it is simply not a demonstrated crash cause.
>
> A native attempt to settle it (T2, `disposable-gunslinger-dodge`) was **inconclusive**: the
> command interrupted before delivery, so the buff never applied and the code path was never
> entered. See GS-AUD-0023. The replacement hypothesis for the incident is **GS-AUD-0024**.
>
> **UPDATE — NATIVELY DISCONFIRMED 2026-11-11T18:23Z (EV-000014).** A second native attempt
> settled it. `disposable-pistolero-deeds` arms and consumes `KMG_UpCloseAndDeadly_Armed`
> (`Blueprints/PistoleroBlueprints.cs:146`) — **one of the 21 identities listed below** — and it
> was applied to a live unit and consumed across six separate stages, in two independent runs on
> the frozen 0.0.182 candidate, **with no `NullReferenceException` at any point**. Because
> `Buff::Activate()` calls `SpawnParticleEffect()` directly and `IsParticleEffectValid()` returns
> false for a never-spawned effect, every one of those applications reached the
> `ldfld FxOnStart` / `callvirt Load(bool)` dereference. **`FxOnStart` is therefore not null at
> runtime for a `CreateInstance`'d `BlueprintBuff` on this engine.** The S3 downgrade now rests
> on native evidence rather than inference. The omission below is still a real deviation from the
> project's own invariant and is still worth repairing — it is **not** a crash cause.


- **Kind:** CONTRACT-QUESTION / latent consistency defect (originally filed PRODUCT-DEFECT)
- **Severity:** **S3** (downgraded from S1 — see the correction banner). Rationale: the
  deviation from the project's own invariant is real and verified, but no runtime failure is
  demonstrated, and the shipped features that use these buffs work.
- **Confidence/evidence strength:** the *omission* is source-demonstrated and hand-verified at
  every site. The *consequence* is **not established**: the engine rule (no null guard) is
  proven from IL, but that `FxOnStart` is actually null at runtime for a
  `CreateInstance`'d `BlueprintBuff` is unproven and is argued against by `Buff.Activate()`
  reaching the same dereference on ordinary application.
- **Disposition:** OPEN. Authority for the expected rule is the project's own stated contract
  (below) plus the native IL of the qualified reference assembly.
- **Affected feature, commit, version, exact package/DLL/profile:**
  commit aa91f46571eb239df36e0233082153f2cba01899, version 0.0.182. Source-level; applies to
  any package built from this commit. Not profile-specific — it is independent of optional mods.

### Supported expected behavior and authority

1. **The project's own written contract**, verbatim, at
   `src/KingmakerGunslinger/Blueprints/PaperCartridgeModeBlueprints.cs:56-60`:
   > "Buff.SpawnParticleEffect and Buff.OnRemove call Load(false) on these links **without null
   > guards** when a persistent buff's unit view is rebuilt. Empty links are the engine-safe
   > representation of an intentionally effect-free buff; they spawn nothing but remain safe
   > across save/load and **area-transition view reconstruction**."
2. **The engine, measured directly.** `ildasm` over the qualified private reference assembly
   (`Managed/Assembly-CSharp.dll`, SHA256 `3B6450FF…5AFB`, byte-identical to the installed
   game) shows `Kingmaker.UnitLogic.Buffs.Buff::SpawnParticleEffect` executing, with no
   preceding null test:

   ```
   IL_003a:  ldarg.0
   IL_003b:  call    instance BlueprintBuff Buff::get_Blueprint()
   IL_0040:  ldfld   class PrefabLink BlueprintBuff::FxOnStart
   IL_0045:  ldc.i4.0
   IL_0046:  callvirt instance !0 WeakResourceLink`1<GameObject>::Load(bool)
   ```
   The method returns early only for a null `View`, a null `View.Data`, or an already-valid
   particle effect. If `Blueprint.FxOnStart` is null, `callvirt … Load(bool)` raises
   `NullReferenceException` **inside** `SpawnParticleEffect`.
3. **The project previously reproduced this exact failure and root-caused it.**
   `docs/GUNSLINGER-SAVE-LOAD-FORENSICS.md:56-58`:
   > "The first material failure was reproduced **twice**: `Buff.SpawnParticleEffect_Patch1`
   > dereferenced a **null project buff FX link** during `UnitEntityData.SpawnBuffsFxs` and
   > `AttachToViewOnLoad`. … The save contains two references to Focused Aim marker
   > `ac72998da0a146cf9ca0cff3ea161303`. Its 0.0.88 blueprint factory **omitted `FxOnStart`,
   > `FxOnRemove`, and `ResourceAssetIds`**, unlike the repository's save-safe buff factories.
   > The stable Focused Aim identity now initializes both FX links to empty `PrefabLink`
   > instances…"
   `CHANGELOG.md` 0.0.74 records the same repair for the Paper Cartridge marker:
   "Initialized the persistent Use Paper Cartridges marker's no-FX resource links so existing
   marker facts reconstruct safely during save load and area changes."

   Therefore: on this engine, an omitted `FxOnStart` **is** null at runtime and **does** throw
   from this exact call site. That is settled empirically by this project's own prior evidence.

### Observed behavior and immutable evidence paths/hashes

The repairs in 0.0.74 and 0.0.88 were applied **only to the two buffs that had crashed**. No
central sweep, registration-time normalization, or build-time guard was ever added. Verified:

- `src/KingmakerGunslinger/Blueprints/BlueprintRegistry.cs` — `Register<T>(symbol, factory)`
  contains no reference to `FxOnStart`, `FxOnRemove`, `PrefabLink` or `ResourceAssetIds`.
- `src/KingmakerGunslinger/Blueprints/BlueprintUnitFactAccess.cs:34-65` — `Configure(...)`
  sets only `m_DisplayName`, `m_Description` and `m_Icon`. It does not touch the FX links.
- The only `FxOnStart == null` assertions in the product are three local `Validate` methods
  (`PaperCartridgeModeBlueprints.cs:107`, `AcadamaeGraduateModeBlueprints.cs:104`,
  `BodyguardModeBlueprints.cs:200`), each covering only its own marker.
- The only test guarding the invariant is `FocusedAimSaveCompatibilityTests.cs:9-17`, and it is
  a **source-text substring assertion against one file**
  (`MysteriousStrangerBlueprints.cs`). It cannot detect the omission anywhere else.

Scan + hand verification of every `ScriptableObject.CreateInstance<BlueprintBuff>()` site in
`src/` (41 sites; analyzer `tools-fxscan.py`, raw output `evidence/A15/fxscan.json`) found
**20 sites that never assign `FxOnStart` or `FxOnRemove`**. Each was then read by hand:

**Player-facing, registered with a stable GUID (serialized into saves):**

| # | Buff `name` | Factory site | Lifetime / who carries it |
|---|---|---|---|
| 1 | `KMG_WayfarersOath_Equipped` | `EasternWeaponNamedBlueprints.cs:409` (shared `Buff(...)` factory), registered at :185 | **While the weapon is in the active equipment set** — long-lived, party member |
| 2 | `KMG_MoonlitCrossing_Equipped` | same factory, registered at :200 | **While equipped** — long-lived, party member |
| 3 | `KMG_FallingPetal_AC_Buff` | same factory, registered at :195 | 1 round, party member |
| 4 | `KMG_MountainSunder_Round_Marker` | same factory, registered at :210 | 1 round, party member |
| 5 | `KMG_Boughkeeper_AC_Buff` | `ElvenBranchedSpearNamedBlueprints.cs:278` (shared `Buff(...)` factory), registered at :120 | until start of next turn, party member |
| 6 | `KMG_Thornstep_Speed_Penalty` | same factory via `StatBuff`, registered at :130 | 1 round, **enemy** |
| 7 | named-spear round markers | same factory via `Marker` (:267) | 1 round, party member |
| 8 | `KMG_BleedingWound_<kind>_Buff` | `BleedingWoundBlueprints.cs:112` | **"until the bleeding is removed"** — long-lived, **enemy** |
| 9 | `KMG_BleedingWound_<kind>_Armed` | `BleedingWoundBlueprints.cs:102` | armed marker, party member |
| 10 | `KMG_Deadeye_Armed_Buff` | `DeadeyeBlueprints.cs:71` | 1 round / until next shot, party member |
| 11 | `KMG_GunslingerDodge_ArmorClass_Buff` | `GunslingerDodgeBlueprints.cs:74` | 1 round, party member |
| 12 | `KMG_DeathsShot_DeathEffect` | `DeathsShotBlueprints.cs:54` | applied to **enemy** |
| 13 | `KMG_DeathsShot_Armed` | `DeathsShotBlueprints.cs:68` | armed marker, party member |
| 14 | `KMG_StartlingShot_FlatFooted_Buff` | `StartlingShotBlueprints.cs:54` | until start of target's next turn, **enemy** |
| 15 | `KMG_StunningShot_Armed` | `StunningShotBlueprints.cs:71` | armed marker, party member |
| 16 | `KMG_TargetingHead_Confusion_Buff` | `TargetingHeadBlueprints.cs:49` | 1 round, **enemy** |
| 17 | `KMG_ExpertLoading_Armed` | `ExpertLoadingBlueprints.cs:50` | armed marker, party member |
| 18 | `KMG_LightningReload_Used` | `LightningReloadBlueprints.cs:53` | per-round marker, party member |
| 19 | `KMG_SteadyAim_Armed` | `MusketMasterBlueprints.cs:90` | per-turn marker, party member |
| 20 | `KMG_UpCloseAndDeadly_Armed` | `PistoleroBlueprints.cs:146` | per-turn marker, party member |
| 21 | `KMG_SlingersLuck_<suffix>_Armed` | `SlingersLuckBlueprints.cs:93` | armed marker, party member |

**Lower exposure (not ordinary player content), listed for completeness:**
`ExpandedSummoningSpecialBuilder.Suggestion32.cs:142` (`stagedBuff`, staged/optional-provider
path); `RuntimeTesting/BodyguardCombatFixture.cs:655`;
`RuntimeTesting/RuntimeTestRunner.ExpandedSummoningAstral31Dispel.cs:1201`;
`RuntimeTesting/RuntimeTestRunner.cs:35204`. These are scenario-fixture buffs; they are still
built into the shipped DLL, so they are reported rather than dismissed.

### Minimal reproduction / concrete static causal path

1. A gunslinger uses **Bleeding Wound** on an enemy (or equips **Wayfarer's Oath** /
   **Moonlit Crossing**). The corresponding `BlueprintBuff` has `FxOnStart == null`.
2. The buff is still attached when the player saves, or when the party changes area.
3. On load / area transition the engine calls
   `LoadingProcess.TickLoading` → `Game.OnAreaLoaded` → `Player.OnAreaLoaded` →
   `UnitEntityData.SpawnBuffsFxs`, which iterates the unit's buffs and calls
   `Buff.SpawnParticleEffect` on each.
4. The unit's `View` and `View.Data` are non-null (the view has just been rebuilt) and
   `IsParticleEffectValid()` is false (the particle effect did not survive the rebuild), so
   execution reaches `IL_0040: ldfld FxOnStart` / `IL_0046: callvirt Load(bool)`.
5. `FxOnStart` is null → `NullReferenceException` inside `SpawnParticleEffect`, which
   `LoadingProcess.Update` rethrows as `LoadGameException`.

That path is identical, frame for frame, to the stack preserved in
`audit48h/reference/INCIDENT_2026-11-11.txt`:
`Buff.SpawnParticleEffect_Patch4` → `UnitEntityData.SpawnBuffsFxs` → `Player.OnAreaLoaded_Patch2`
→ `Game.OnAreaLoaded_Patch2` → `LoadingProcess.TickLoading_Patch0` → rethrow `LoadGameException`.

### Preconditions, exact fixture, assistance and native-input limitations

Requires an affected buff to be live on a unit at an area transition or save load. The
long-lived ones (Bleeding Wound on a surviving enemy; Wayfarer's Oath / Moonlit Crossing while
equipped) make that ordinary rather than exotic. **No live reproduction was performed in this
campaign** — see "Remaining unknowns".

### Scope/impact and repeatability

Affects any save in which an affected buff is serialized, or any area transition while one is
active. The historical 0.0.88 instance was reproduced **twice** on demand once the right save
existed, so the mechanism is deterministic rather than racy; the user's report of
*intermittent* failure is consistent with "intermittent presence of the buff", not with an
intermittent code path.

### Negative controls, disconfirmation and competing explanations

Considered and addressed:

- **"KMG's own `Buff.SpawnParticleEffect` Harmony patches threw."** Ruled out as the direct
  thrower. KMG patches that method three times
  (`Summoning/ExpandedSummoningSummonGlowPatch.cs:60`,
  `Summoning/GiantSummonPresentationPatch.cs:297`,
  `Summoning/RedcapScytheAnimationR6.cs:122`). All three are **postfixes whose entire bodies
  are inside `try { … } catch (Exception) { … }`**, so none can propagate an exception out of
  the patched wrapper. The `_Patch4` frame is the *generated wrapper*, which also contains the
  original engine body — so the suffix does not implicate any patch owner. The mission's
  warning against inferring ownership from a Harmony suffix is respected: this finding rests on
  the engine IL and the project's own prior reproduction, not on the suffix.
- **"The crash is a third-party mod."** Not excluded, and deliberately not claimed.
  Six installed peers (BagOfTricks, CheatMenu, CraftMagicItems, KingmakerBuffPlanner,
  KingmakerDiceRoller, KingmakerLastAzlantiPreserver) are in no compatibility profile, and
  `KingmakerBuffPlanner` in particular is buff-adjacent. The incident alone cannot attribute
  ownership. **This finding does not claim the 2026-11-11 incident was caused by KMG.** It
  claims something narrower and independently sufficient: KMG ships 20 buff identities that
  violate the project's own engine-safety contract and can raise this exact exception.
- **"Unity initialises serialised reference fields, so `FxOnStart` is never null."** Ruled out
  empirically: the project reproduced a null FX-link dereference twice on this engine from
  exactly this omission (forensics doc above), and wrote two separate repairs for it.
- **"A `Validate` method would catch it."** Ruled out: only three markers validate their own
  FX links, and none of the 20 sites above is covered.

### Probable location/cause (hypothesis clearly labeled)

**Hypothesis:** the 0.0.74 and 0.0.88 repairs were applied point-wise to the two buffs that had
already crashed, and the invariant was never generalised. The newer factories
(ElementalRaces, Summoning, Acadamae, Bodyguard, MysteriousStranger) do set the links, which
suggests the convention was adopted going forward but never backfilled into the older
Gunslinger deed and named-weapon factories.

### Earlier evidence, related/duplicate finding IDs and status changes

Supersedes nothing. Directly extends `docs/GUNSLINGER-SAVE-LOAD-FORENSICS.md` (2026-08-20),
whose disposition was `BLOCKED` and which closed only the Focused Aim identity. Related to
GS-AUD-0003 (the incident's own local evidence is missing).

### Demonstrated workaround

None demonstrated. (Not re-equipping the named weapons, and resting off bleed before a
transition, would avoid the affected states, but neither was tested.)

### Suggested repair direction for later triage (no product fix implemented in this campaign)

A central, enforced invariant rather than another point fix. For later owner decision:
initialise `FxOnStart`/`FxOnRemove`/`ResourceAssetIds` inside the one shared creation helper
every buff factory already routes through, or normalise them in `BlueprintRegistry.Register`
for `BlueprintBuff`; then replace the single-file source-substring test with a reflective test
over every registered `BlueprintBuff` identity. **Stable GUIDs must be preserved** — this is a
field-initialisation change, not an identity change.

### Regression verification idea

A domain/reflection test that builds the full registry and asserts, for **every** registered
`BlueprintBuff`, that `FxOnStart != null`, `FxOnRemove != null` and `ResourceAssetIds != null`.
That test would fail today on all 21 identities above and cannot be satisfied by a point fix.

### Remaining unknowns and precise next prerequisite

- **Not natively reproduced on 0.0.182 in this campaign.** The precise next step is a guarded
  disposable native case: apply `KMG_BleedingWound_*` (or equip Wayfarer's Oath) on a
  disposable unit, force an area transition, and capture whether `SpawnBuffsFxs` throws. That
  requires an admitted native transaction and a disposable fixture.
- Whether Unity's serialisation backend leaves these fields null on **every** code path is
  taken from this project's prior reproduction rather than re-measured here.
- The exact buff in the 2026-11-11 incident remains **unidentified** (GS-AUD-0003).

---

## GS-AUD-0001 — The installed mod on OLD LAPTOP is 0.0.117, not the 0.0.182 release candidate

- **Kind:** ENVIRONMENT-BLOCKER
- **Severity:** S2; rationale: it does not make the product wrong, but it invalidates any
  assumption that the machine's live installation represents the release being audited, and it
  means the user's own recent play experience is not 0.0.182's behaviour.
- **Confidence/evidence strength:** native-observed (direct hashing of the installed tree).
- **Disposition:** OPEN.
- **Affected feature, commit, version, exact package/DLL/profile:** host installation only.
  Installed DLL SHA256 `FD2FC61C250B13857D81ACC197A896450F5B242EE392FA7192B01201E908F35F`,
  AssemblyVersion 0.0.117.0, ProductVersion `0.0.117-elemental-char-gen-stabilization`.
  Repository candidate is 0.0.182.
- **Supported expected behavior and authority:** mission section 5 step 6 requires the actual
  intake to be recorded and kept distinct from "an old accepted release version".
- **Observed behavior and immutable evidence:** `Mods\KingmakerGunslinger\Info.json` declares
  `"Version": "0.0.117"`; the DLL carries 0.0.117 identity. `FeatureModules.json` there is
  schemaVersion 10 with **11** modules, missing `teleportation-spells` and
  `magic-circle-spells`, which 0.0.182 ships. Evidence: `evidence/A00/host-intake.txt`.
- **Scope/impact:** the live installation cannot be used as evidence about 0.0.182 behaviour.
  Any native work in this campaign must deploy the frozen candidate inside a transaction and
  restore **0.0.117** afterwards — restoring "the current release" would be wrong.
- **Negative controls / competing explanations:** the newest compatibility transaction
  (`compat-20261106T235909Z-50fe08b7735f`) reports verified exact restoration, and the mod
  directory's own file timestamps are the packaging-normalised `2026-01-01T05:00:00Z` while the
  directory mtime is `2026-11-06T21:35:41Z`. That is consistent with 0.0.117 being the user's
  genuine pre-existing installation, restored intact after testing — not with damage.
- **Remaining unknowns:** why the user's last local session (2026-11-07, automated) loaded
  **0.0.178** while the incident log declares **0.0.182** and the resident install is 0.0.117.
  See GS-AUD-0003.
- **Suggested direction:** owner decision only — this is the user's machine state, not a
  product defect. Record it; do not "fix" it during the audit.

---

## GS-AUD-0003 — The 2026-11-11 incident log is not present on this host; the last local game run was 2026-11-07

- **Kind:** VERIFICATION-GAP
- **Severity:** UNASSESSED; the underlying crash is real external evidence, but its causal
  module, buff, unit and DLL identity remain unknown and cannot be recovered locally.
- **Confidence/evidence strength:** native-observed (local file comparison). The crash itself is
  observed-once, externally, and remains **unattributed**.
- **Disposition:** OPEN / BLOCKED-VERIFICATION.
- **Observed behavior and immutable evidence:**
  - The incident reference records a user-uploaded `output_log.txt` of **2,680,892 bytes**
    declaring KMG `0.0.182` / `0.0.182-summon-menu-order-spider-summoner`.
  - The live `…\LocalLow\Owlcat Games\Pathfinder Kingmaker\output_log.txt` is **2,162,816 bytes**,
    LastWriteUtc **2026-11-07T00:06:03Z**, SHA256 `4131CB06…DF51`. Preserved at intake to
    `private/preserved-logs/output_log.AT-INTAKE.txt` (same hash).
  - That file is **byte-identical** (same SHA256) to
    `runtime-evidence\20261107T0000370256951Z-observe-expanded-summoning-inventory\output_log-gunslinger-call-of-the-wild-observe-expanded-summoning-inventory.txt`.
    So the last Kingmaker launch on this host was that automated compatibility run, not a user
    play session.
  - That log's own version line reads `[KingmakerGunslinger] Version '0.0.178'.` — neither the
    0.0.117 on disk nor the 0.0.182 in the incident.
  - **Negative control:** the live log contains exactly one `NullReferenceException`, and it is
    `Kingmaker.UI.BugReportCanvas.OnEnable` — an unrelated UI exception, **not** the incident
    stack. `SpawnParticleEffect`, `SpawnBuffsFxs` and `LoadGameException` do not appear.
  - No other copy of a 2,680,892-byte `output_log.txt` exists under the LocalLow directory, the
    lab, Downloads, Desktop or Documents.
- **Scope/impact:** the A15 lead cannot be closed from local original evidence. The exact buff,
  owning unit, blueprint, patch owner and installed DLL hash of the incident stay unknown.
- **Remaining unknowns and precise next prerequisite:** the original full
  `output_log.txt` (2,680,892 bytes) — and, if it exists, the companion
  `output_log_prev.txt` / crash-report folder — from whichever machine produced it. Without
  that file no attribution is possible, and a different exception or a load delay must **not**
  be labelled a reproduction of this incident.

---

## GS-AUD-0002 — Live UMM `Params.xml` left in the 2026-11-07 test-profile state

- **Kind:** ENVIRONMENT-BLOCKER
- **Severity:** S3; rationale: recoverable, affects only the host's mod-enable state.
- **Confidence/evidence strength:** native-observed.
- **Disposition:** OPEN. **Pre-existing at intake — NOT caused by this campaign, and
  deliberately not repaired by it.**
- **Observed behavior and immutable evidence:**
  `…\Kingmaker_Data\Managed\UnityModManager\Params.xml`, 845 bytes, SHA256
  `5F7B96B4F1BC1313EB169F06951E3C98448D2A1E0F3358CC84E351E4FC4B3747`,
  LastWriteUtc `2026-11-07T00:06:03.3428980Z` — the same second the 2026-11-06/07
  compatibility-profile run ended. Its `<ModParams>` lists only `CallOfTheWild` and
  `KingmakerGunslinger`, although **eleven** mods are installed.
- **Supported expected behavior and authority:** `GUNSLINGER_RUNTIME_REFERENCE.txt` section 3
  states the compatibility-profile wrapper "protects Mods and the external SoundBank" but has
  **"No generic UMM Params protection"**, and section 4 exists precisely because that gap
  requires a separate composed transaction. The observed state is the documented consequence
  of running that wrapper without the UMM composition.
- **Scope/impact:** this is the baseline that any native transaction in this campaign must
  restore to. It is recorded in AUDIT-RESTORATION.md as the campaign's intake value.
- **Suggested direction:** owner decision. Separately, it is concrete evidence that the
  profile-wrapper + UMM composition described in the runtime reference is necessary in
  practice, not merely theoretical.

---

## GS-AUD-0005 — The broad release gate silently depends on an untracked ~99 MB local IL dump

- **Kind:** HARNESS-DEFECT
- **Severity:** S3; rationale: it does not affect shipped gameplay. It is reported because it
  makes the project's own release gate non-reproducible from a clean checkout and because its
  failure mode is misleading rather than explanatory.
- **Confidence/evidence strength:** native-observed — an actually executed gate on this host.
- **Disposition:** OPEN. Worked around for this campaign by the project's own documented
  procedure; **not** repaired.
- **Affected feature, commit, version:** `scripts/Invoke-KmgGate.ps1 -Level Sprint|Tranche`
  (via `scripts/test-domain.ps1`) at commit aa91f4657 / 0.0.182.
- **Supported expected behavior and authority:** `docs/VALIDATION-POLICY.md` presents
  `-Level Sprint` / `-Level Tranche` as *the* candidate-boundary gate, and
  `GUNSLINGER_RUNTIME_REFERENCE.txt` section 2 presents it as the one broad offline pipeline.
  A gate with that role should either provide its own inputs or refuse with a named
  prerequisite.
- **Observed behavior and immutable evidence:**
  A fresh `git worktree` of the exact release commit fails the complete domain suite:
  `Completed 2942 tests; failures=23`. All 23 failures are the same
  `System.IO.DirectoryNotFoundException` for
  `<worktree>rtifacts\inspectionodyguard-native\Assembly-CSharp.il`, across the
  `fiery-foundation` (4), `aerial-foundation` (3), `whiteout-native-contract` (14) and
  `bodyguard-runtime` (2) suites. Evidence:
  `evidence/A02/tranche-gate-attempt1-FAILED-missing-il-dump.log`.
  No assertion failed — those 23 tests validated nothing in either direction.
- **Negative controls and competing explanations:** this is **not** a regression in 0.0.182.
  The repository's own records describe the identical event with the identical count:
  `planning/SUMMON-DRUID-DUPLICATES-GIANT-SIZES-STATE.md:115` — *"The first attempt failed 23
  tests only because the fresh worktree lacked the local
  `artifacts/inspection/bodyguard-native/Assembly-CSharp.il` dump; it was copied in unchanged
  … as for v0.0.165."* See also `planning/SPRINT39-STATUS.md:86`,
  `EXPANDED-SUMMONING-PHASE0-JOURNAL.md:58`, `docs/SPRINT30-31-REVIEW-FOLLOWUP.md:250`.
  So the defect is the undeclared dependency and its opaque failure, not the test content.
- **Scope/impact:** anyone validating a release from a clean checkout — including a future
  maintainer, a CI runner, or this audit — gets 23 red tests that look like product
  regressions and name no prerequisite. The required input is ~99 MB and untracked, so it
  cannot be obtained from the repository.
- **Demonstrated workaround:** yes, and it was used. The dump was copied in unchanged from the
  user's primary checkout (`Assembly-CSharp.il`, 103,589,724 bytes, SHA256
  `2AA7DC0BF64D791FE5DD75F9C7A2C3224B3BFBC0B21AB233FD81E461EE5FE50B`, plus its 676-byte
  `.res`). **Provenance was proven, not assumed:** the dump's header records
  `// MVID: {07FA1E4D-8618-41B3-9B8D-FAA17D3B26F7}`, equal to the MVID of the qualified
  reference assembly and of the installed `Assembly-CSharp.dll`. The frozen worktree's
  `git status --porcelain` stayed empty (the `artifacts/` tree is gitignored).
- **Suggested repair direction for later triage:** have the affected tests detect the missing
  dump and fail (or skip) with one explicit named prerequisite instead of a raw path
  exception; and generate the dump on demand from the already-required reference bundle with
  the `ildasm` that is present in the Windows SDK, rather than depending on a hand-copied
  artifact. Owner decision; no change made.
- **Remaining unknowns:** whether any CI path other than a local developer checkout runs this
  gate.

---

## GS-AUD-0006 — A failed settings migration write turns a *valid* older settings file into "malformed", silently resetting every module to ON

- **Kind:** PRODUCT-DEFECT
- **Severity:** S2; rationale: wrong behaviour with real user impact (all module choices lost,
  and lost again on every launch) but with a clear workaround once understood (make the file
  writable). Not S1: it does not crash, corrupt a save, or block progression.
- **Confidence/evidence strength:** source-demonstrated. The control flow is unambiguous and
  was read in full. Not natively reproduced.
- **Disposition:** OPEN.
- **Affected feature, commit, version:** feature-module settings, all 13 modules.
  `src/KingmakerGunslinger/FeatureModules/FeatureModuleSettingsStore.cs:23-97` at commit
  aa91f4657 / 0.0.182.
- **Supported expected behavior and authority:** mission card A19 requires these four states to
  be *distinguished*: "missing/default, supported older schema **preserving values**, malformed
  quarantined/default behavior and future-schema rejection". A well-formed older-schema file is
  the "supported older schema preserving values" case; its values must be preserved.
- **Observed behavior and immutable evidence (`FeatureModuleSettingsStore.cs`):**

  ```csharp
  try {
      JObject root = JObject.Parse(File.ReadAllText(path));      // :25
      ...                                                        // :35-60  all 13 keys read
      var state = new FeatureModuleSettingsState(...);           // :61  values PARSED CORRECTLY
      if (schema < CurrentSchemaVersion) Save(state);            // :70  <-- INSIDE the try
      return state;                                              // :71
  }
  catch (Exception exception) {                                  // :73
      if (exception is UnsupportedFeatureModuleSchemaException) throw;
      ... File.Copy(path, quarantine, false) ...                 // :82
      warning("Malformed feature-module settings at " + path +
              "; recovered defaults (all modules ON); " ...);    // :90-93
      return new FeatureModuleSettingsState(
          FeatureModuleConfiguration.Defaults, path,
          "malformed-recovery", true);                           // :94-96
  }
  ```
  The migration write at line 70 is inside the same `try` as the parse. Any I/O failure in
  `Save` is therefore caught by the generic `catch (Exception)` at line 73, which discards the
  already-correctly-parsed configuration and returns `FeatureModuleConfiguration.Defaults`
  with `Recovered = true` and the message **"Malformed feature-module settings … recovered
  defaults (all modules ON)"**. The file was not malformed; it parsed successfully.
- **Minimal reproduction / concrete static causal path:**
  1. A user upgrades from any earlier release to 0.0.182. Their `FeatureModules.json` carries
     `schemaVersion < 12` — so the `Save` at line 70 **will** run. (This host is a live
     example: the installed `FeatureModules.json` is `schemaVersion 10`.)
  2. The write fails. Reachable causes: the file carries the read-only attribute (a common way
     users pin settings); the `Mods` tree is under `C:\Program Files (x86)\…` and the process
     is a standard user without write permission there; or an antivirus / backup agent holds a
     transient lock. `Save` uses `File.Replace` (`:137`), which throws `UnauthorizedAccessException`
     on a read-only destination.
  3. Control reaches `catch` at line 73 → quarantine `File.Copy` is attempted (and in the
     read-only / no-permission case usually also fails, yielding `quarantine-failed=…`) →
     defaults returned.
  4. The user's module selections are replaced by all-ON for that session, and the log says
     their file was malformed. Because the write still fails next launch, this repeats forever.
- **Scope/impact and repeatability:** every existing player upgrading to 0.0.182 passes through
  the migration write, so the precondition is universal at upgrade; the failure then depends on
  file writability. Deterministic once the write fails.
- **Negative controls, disconfirmation and competing explanations:**
  - **"Save can't realistically fail."** Not relied upon either way. On *this* host the file is
    writable (`FeatureModules.json` mtime 2026-09-08 differs from the packaged
    2026-01-01 timestamp), so the happy path demonstrably works here. The finding is about the
    error path, which is unreached on this host and untested in the suite.
  - **"The test suite covers this."** Checked. `FeatureModuleSettingsTests.cs` covers
    defaults/legacy (`:11`), schemas 0-10 preserving values (`:129`), an 8192-combination round
    trip (`:164`), malformed quarantine (`:210`), future-schema rejection (`:239`), and
    Teleportation/MagicCircle migration (`:396`, `:639`). **No test makes the migration write
    fail.** This is a genuine coverage gap, not a known accepted limitation.
  - **"`Save` might write the wrong values."** Ruled out: `FeatureModuleSettingsState`'s
    constructor copies `active` into `Pending` field-for-field (`FeatureModuleSettingsState.cs:11-17`),
    and `Save` serialises `state.Pending`, so a *successful* migration preserves values correctly.
- **Probable location/cause (hypothesis):** the `Save` call was placed inside the parse `try`
  for brevity; the generic `catch (Exception)` was written for parse failures and happens to
  swallow write failures too.
- **Demonstrated workaround:** none tested. Clearing the read-only attribute / granting write
  permission on `Mods\KingmakerGunslinger\FeatureModules.json` would avoid it.
- **Suggested repair direction for later triage (no fix implemented):** move the migration
  `Save` outside the parse `try`, or wrap it in its own `try/catch` that logs a *write* failure
  and still returns the correctly parsed `state`. A failure to persist a migration should never
  change the configuration that session.
- **Regression verification idea:** a domain test that loads a valid `schemaVersion: 10` file
  whose destination is made unwritable (read-only attribute, or an open exclusive handle) and
  asserts the returned state has `Recovered == false`, `Source` starting `migrated-schema-`,
  and the file's own values — not `Defaults`.
- **Remaining unknowns:** the real-world frequency of unwritable mod directories on Nexus
  users' machines. Not measurable from here.

---

## GS-AUD-0007 — A future settings schema disables the entire mod rather than the settings file

- **Kind:** CONTRACT-QUESTION
- **Severity:** S3 / UNASSESSED — depends on an owner decision about intended downgrade behaviour.
- **Confidence/evidence strength:** source-demonstrated.
- **Disposition:** OPEN — needs an owner decision, not a repair recommendation from the audit.
- **Affected feature, commit, version:** `FeatureModuleSettingsStore.Load` and
  `Bootstrap/ModContext.cs:114` at aa91f4657 / 0.0.182.
- **Supported expected behavior and authority:** mission card A19 names "future-schema
  rejection" as a state to distinguish, but does not say what *rejection* should do to the mod.
  No repository contract found that settles it.
- **Observed behavior:** `FeatureModuleSettingsStore.cs:29-32` throws
  `UnsupportedFeatureModuleSchemaException` for `schema > 12`, and `:75` deliberately rethrows it
  past the recovery `catch`. `ModContext.Create` calls `Load` at `Bootstrap/ModContext.cs:114`
  with no surrounding handler, so the exception propagates out of the UMM entry point and the
  **whole mod fails to load** — not just the settings file.
- **Concrete path:** a player who installs 0.0.182, runs it once (settings rewritten at
  schema 12), then rolls back to an older build whose `CurrentSchemaVersion` is lower, gets a
  mod that refuses to load. Deleting `FeatureModules.json` would recover it, but nothing tells
  the player that.
- **Negative controls / competing explanations:** this is plainly deliberate — the rethrow at
  `:75` exists only to bypass the recovery path, and `FutureSchemaIsRejectedAndSerializationIsOrdered`
  (`FeatureModuleSettingsTests.cs:239`) tests it. So it is **not** a bug by neglect. The open
  question is whether "refuse to load the mod" is the intended player-visible consequence,
  versus quarantining the file and starting from defaults as the malformed path does.
- **Why it matters now:** the audit's purpose is a Nexus release. Rollback after a bad update is
  an ordinary thing Nexus users do, and a hard load failure with no in-game explanation is a
  predictable support burden.
- **Suggested direction:** owner decision only. If the hard refusal is intended, say so in the
  release notes / README with the recovery step (delete `FeatureModules.json`). No code change
  is recommended by this audit.
- **Remaining unknowns:** whether any shipped documentation already states this. Not found in
  `README.md` or `INSTALLATION-COMPATIBILITY.md` during this pass; that search was not exhaustive.

---

# Findings imported from workers W1 and W2 (EV-000009)

Each entry states the coordinator's adjudication. Where the coordinator changed a worker's
severity or classification, the change and its reason are recorded explicitly. The full
traces remain in `workers/W1/W1-result.md` and `workers/W2/W2-result.md`.
**Every item below is source-demonstrated only. None is natively verified.**

## GS-AUD-0008 — Stale "Broken or Wrecked" field-repair prose in eight places

- **Kind:** DOCUMENTATION-DRIFT · **Severity:** S3 · **Evidence:** source-demonstrated, high confidence
- **Disposition:** OPEN — CONFIRMED and EXTENDED. The discovery pack already established this
  conflict in principle; this audit enumerates every surviving instance.
- **Authority:** `docs/RELEASE-NOTES-0.0.127.md` / `0.0.128.md` — field repair is **Broken-only**,
  outside party combat, rechecked at delivery; Wrecked field repair fails; a completed full rest
  repairs both and leaves Wrecked unloaded.
- **Observed stale wording:** `README.md:869` (and surrounding prose 871-878);
  `tests/KingmakerGunslinger.DomainTests/UnifiedFirearmRepairTests.cs:9` (file header);
  `docs/ARCHITECTURE.md:119,421,427,468,604`; `docs/BLUEPRINT-MANIFEST.md:233`;
  `docs/FIREARM-ITEM-LIFECYCLE-AND-RECOVERY.md:5,93`; `docs/FIREARM-STATE-MACHINE.md:64`;
  `docs/ITEM-DESCRIPTION-AUDIT.md:14`.
- **IMPORTANT NEGATIVE CONTROL — do not "fix" these:**
  `docs/FIREARM-MAINTENANCE-CONTRACT.md:74-77` is **not** drift. It sits under an explicit
  `[BASELINE] Current behavior (verified P0 trace…)` historical heading, and the `[CONTRACT]`
  section above it correctly states Wrecked is ineligible. The shipped in-game kit text is also
  correct. Only the eight locations listed above are stale.
- **Why CI missed it:** `FieldRepairRestrictionTests.cs:204-231` scans only 7 source files plus
  `blueprints.json`; none of the eight stale locations is in that set.
- **Impact:** this is the first thing a Nexus reader sees — `README.md` is the mod's public
  description. It tells players Wrecked firearms can be field-repaired. They cannot.
- **Repair direction (not implemented):** correct the eight locations; widen the CI scan set.

## GS-AUD-0009 — The capacity-6 Advanced Revolver cannot persist state past one loaded round

- **Kind:** PRODUCT-DEFECT (latent) · **Severity:** S3 · **Evidence:** source-demonstrated;
  **reachability LOW and explicitly unproven**
- **Disposition:** OPEN
- **Observed:** `FirearmDefinitions.cs:54-58` gives the Advanced Revolver capacity 6 and no equip
  restriction, but the registered state-token catalog is capacity-ONE
  (`FirearmStateTokenBlueprints.cs:44-45`). Loading a second round makes
  `FirearmStateTokenCatalog.Encode` throw, which rolls the reload back silently — permanently.
- **Why severity is only S3:** W1 searched for an in-game acquisition route and found none —
  the weapon is `LegacyRecognitionOnly` and is never vendored, crafted or granted. The
  coordinator accepted this at the worker's stated limit and did **not** independently re-verify
  it; that limit is recorded rather than hidden.
- **Why it still matters for a Nexus release:** if any future change, or any third-party mod,
  puts that weapon in a player's hands, the failure is silent.
- **Smallest confirming check:** grant the Advanced Revolver via console/BagOfTricks in a
  disposable save, load two rounds, observe whether the second reload silently reverts.

## GS-AUD-0010 — Broken-only field repair is enforced only in callers, not at the transition

- **Kind:** CONTRACT-QUESTION · **Severity:** S3 · **Evidence:** source-demonstrated
- **Observed:** `FirearmStateMachine.Repair` deliberately accepts Wrecked; the Broken-only rule
  is enforced by three callers. There is no invariant at the transition layer.
- **Assessment:** not currently a defect — W1 verified all production callers enforce the gate.
  It is a defence-in-depth question: a future fourth caller would silently violate the contract.
- **Repair direction (not implemented):** owner decision whether to push the gate down.

## GS-AUD-0011 — Deadeye charges a flat 1 Grit and discards the per-increment cost it computes

- **Kind:** CONTRACT-QUESTION · **Severity:** S3 · **Evidence:** source-demonstrated
- **Observed:** `DeadeyeRuntime` passes `int.MaxValue` as current grit, so the per-increment cost
  `DeadeyeService` computes is dead; a flat 1 Grit is charged.
- **Assessment:** code, blueprint text and tooltip all agree on 1 Grit, so this looks deliberate
  rather than broken. Tabletop Deadeye charges per range increment, so the owner should confirm
  this is an intended simplification. **Pending PR #129 revises Deadeye's description text and is
  NOT merged**, so its wording is not the contract here.
- **Repair direction:** owner confirmation only. Either delete the dead cost path or wire it up.

## GS-AUD-0012 — `GunslingerDodgeRuntime` is dead code, so its evidence can only ever read zero

- **Kind:** PRODUCT-DEFECT (diagnostics only) · **Severity:** S3 · **Evidence:** source-demonstrated
- **Observed:** `GunslingerDodgeRuntime` has no caller anywhere in `src/`, and its `authorized`
  flag is never set.
- **Scope:** **player-visible behaviour is unaffected** — the AC buff does the real work. The
  consequence is that any "Dodge applied before the AC calculation" assertion can only measure
  zero, so that acceptance claim is unfalsifiable.
- **Repair direction:** remove the dead runtime, or wire it in so the evidence means something.

## GS-AUD-0013 — Misfire suppression can only be released by a player-issued order; an AI-driven companion stays locked out after repair

- **Kind:** CONTRACT-QUESTION (possible PRODUCT-DEFECT depending on intent) · **Severity:** S2
  if AI-controlled party members are in scope, else S3 · **Evidence:** source-demonstrated;
  mechanism **independently verified by the coordinator**
- **Authority:** release notes 0.0.127 #3 — a committed misfire break "stops the remainder of
  that sequence and its automatic continuations… A later deliberate player attack with the
  Broken gun remains possible." 0.0.128 — "Recovery alone grants no new order."
- **Observed (coordinator-verified):** across all of `src/`, `Suppressed` is set false in exactly
  one place — `Firing/FirearmAttackOrderLedger.cs:87`, inside `Accept` — and `RequiresOrder` in
  exactly one — `:125`, inside `Complete`. `Accept` is reachable only from proposals created in
  `NativeFirearmAttackOrder.CreateAttack/CreateControllerAttack/CreateAutoUse`, all of which are
  entered only through the two transpiled **player-input** handlers
  (`Firing/NativeFirearmAttackOrderPatch.cs:23-29`). The ledger type is referenced by only three
  production files, so **no repair, Quick Clear or rest path can clear suppression**.
- **Failure scenario:** a companion under real-time AI control misfires to Broken. The player
  Quick-Clears the firearm back to Normal. That companion's AI never fires it again for the rest
  of the session unless the player manually clicks an attack order with them. A save/load lifts
  it incidentally, because the ledger is a process-lifetime `ConditionalWeakTable` and is never
  serialized (`FirearmAttackOrderLedger.cs:9,13-14,28-29`).
- **Competing explanations ruled out:** `ReconcileOwnership`/`InterruptedAll` only call
  `Ledger.Cancel`, and `Cancel` (:128-137) *re-sets* `Suppressed` when `RequiresOrder` is true;
  `Epoch` is monotonic and incremented only by `Break`.
- **Why it is a question and not a plain defect:** `BrokenSequenceInterruptionPolicy`'s own doc
  comment (`:128-133`) describes roughly this behaviour, so "needs a fresh deliberate order" may
  be intended. What looks unintended is that it survives a *successful repair*.
- **Smallest confirming check:** real time, companion on AI control with a pistol — force a
  misfire to Broken, Quick Clear to Normal, then issue no orders and watch whether it ever fires.

## GS-AUD-0014 — Three firearm verification gaps not settleable from source

- **Kind:** VERIFICATION-GAP · **Severity:** UNASSESSED · **Evidence:** source-demonstrated absence
- **A04-F3 — cross-process firearm token survival.** Nothing in the candidate establishes that
  item-owned state tokens survive a cold load; the reconciliation patch is purely in-process, and
  a lost token decodes **silently** to Normal/empty rather than failing loudly. This is the
  single most important unproven firearm claim. *Smallest check:* load a firearm, save, exit to
  desktop, relaunch, reload, inspect loaded rounds and degradation.
- **A04-F5 — rest-repair ordering relative to the native autosave.** The requirement that a
  completed full rest repairs carried firearms *before* the autosave is asserted in a source
  comment and cannot be proven statically. *Smallest check:* rest with a Broken firearm, then
  load the rest autosave and inspect its state.
- **A06-F4 — "Standard/FullRound reloads cannot borrow an action" (0.0.128).** No enforcement
  exists in mod source; `ResumeAttack` queues an attack gated only on
  `!isTurnBased || isActorTurn` and defers to the native cooldown. Either the native engine
  enforces it or the claim is unsupported. *Smallest check:* turn-based, attempt a
  standard-action reload after the actor's action is spent.

## GS-AUD-0015 — A persisted Powerful Change bonus silently degrades when its caster is not in the loaded scene

- **Kind:** PRODUCT-DEFECT · **Severity:** S2 · **Evidence:** source-demonstrated; key line
  spot-verified by the coordinator
- **Observed:** the persisted bonus is reconstructed only through
  `BrownFur/BrownFurModifierAdjustmentRuntime.cs:243`, which returns early when
  `sourceContext.MaybeCaster` is null, and keys the lookup on the **live** caster at `:254`.
  There is no fallback to the serialized caster reference — even though this repository reads
  `MechanicsContext.m_CasterReference` reflectively for exactly that purpose elsewhere
  (`RuntimeTesting/RuntimeTestRunner.MagicCircleLifetime.cs:114-118`).
- **Failure scenario:** a Share Transmutation ally is reloaded in a scene that does not contain
  the Brown-Fur caster. The ally silently shows **+4 instead of +6**. The persistence record
  itself is intact, and nothing is logged — so the player sees a wrong number with no diagnostic.
- **Why it matters for this release:** it is exactly the "saved-owner cold reconstruction" gap
  the mission flags as the Brown-Fur priority (A11), and the 0.0.157 startup repair explicitly
  did **not** close it (its working save lacked the fact).
- **Smallest confirming check:** cast Share Transmutation on a companion, split the party so the
  caster is in another area, save, reload, inspect the ally's stat bonus.
- **Repair direction (not implemented):** fall back to the serialized caster reference, using the
  reflective pattern this repository already uses.

## GS-AUD-0016 — Two UnitParts validate by throwing inside an engine callback that provably has no catch handler

- **Kind:** CONTRACT-QUESTION (latent robustness) · **Severity:** S3
  · **Evidence:** engine behaviour **proven from IL**; **reachability NOT demonstrated**
- **Disposition:** OPEN. **DOWNGRADED by the coordinator from the worker's provisional S1-S2**,
  because the coordinator tested reachability and found no trigger. Recorded rather than dropped,
  because the mechanism is real and one future schema change would arm it.
- **The engine half — PROVEN.** Disassembly of the qualified reference assembly (MVID
  `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`) shows:
  * `Kingmaker.UnitLogic.UnitPartsManager::PostLoad` iterates parts inside `.try { … } finally { … }`
    — **a finally only, with no catch handler** — calling
    `callvirt instance void Kingmaker.UnitLogic.UnitPart::PostLoad()`.
  * `UnitPartsManager::PreSave` has the identical shape.
  * `UnitEntityData::PostLoad` contains **zero** catch handlers.
  So a throwing `UnitPart.PostLoad` reaches `LoadingProcess.Update`, which converts it to
  `LoadGameException` — **the save does not load**. A `PreSave` throw would block *writing* a save.
- **The product half:** two KMG UnitParts validate by throwing inside exactly those callbacks —
  `BrownFur/UnitPartBrownFurModifierPersistence.cs:75-85`
  (→ `NotSupportedException` / `InvalidOperationException` via
  `BrownFurPersistedModifierRecord.cs:69-88`) and
  `UrbanBarbarian/UnitPartControlledRageSelection.cs:33-43`
  (→ `FormatException` via `ControlledRageSelectionState.cs:96,102,106`). They are the only two
  such places in the product.
- **Reachability — tested and NOT found:**
  * `BrownFurPersistedModifierRecord.CurrentSchemaVersion` has been **1 since the feature was
    introduced** (`git log -L` → commit 805481e58) and has never been bumped, so no shipped save
    can carry a mismatched schema.
  * `BrownFurModifierAdjustmentPolicy.SupportedCarriers` (`:48-52`) is a **fixed static HashSet**,
    not runtime-dependent — so removing Call of the Wild cannot invalidate a saved carrier family.
  * `ControlledRageSelectionState`'s constructor unconditionally `Unlock`s the Ordinary tier
    (`:13-16`), and `Serialize` (`:65-71`) emits whatever the dictionary holds, so every string
    the mod can produce satisfies `Parse`'s Ordinary requirement (`:105-106`). `git log` shows the
    serialization format has never changed (commits e6d4d475d, 96e7a4ca7), so no older save can
    carry a stale format either.
- **Why it is still reported:** the repository already has the correct pattern —
  `ElementalRaces/ElementalFeatTransientRuntime.cs:123-148` **quarantines** bad data instead of
  throwing. These two parts are inconsistent with it, and the cost of being wrong is an
  unloadable save. The next schema bump or serialization change arms this with no warning.
- **Repair direction (not implemented):** adopt the quarantine pattern in both parts, and add a
  migration path before any future schema bump. Owner decision.
- **Smallest confirming check:** hand-craft a disposable save whose Controlled Rage `_state`
  string omits the Ordinary tier, and confirm the load fails with `LoadGameException`.

## GS-AUD-0017 — The Cord's combat-log line says "nonlethal damage" where the approved contract says "nonlethal-equivalent"

- **Kind:** DOCUMENTATION-DRIFT (player-visible string) · **Severity:** S3
  · **Evidence:** source-demonstrated
- **Disposition:** OPEN. **RE-CLASSIFIED DOWNWARD by the coordinator** from the worker's S2
  PRODUCT-DEFECT. The worker reported that no nonlethal flag is set anywhere; that is true, but
  it is the documented, owner-approved adaptation, which the worker did not weigh.
- **Authority that disposes of the larger claim — `README.md:844`, verbatim:** *"Kingmaker 2.1.7b
  has no usable native nonlethal damage path. The adaptation is untyped self-damage capped so the
  Cord cannot reduce its wearer below 1 HP."* `CHANGELOG.md:2123` (0.0.75) calls it "one capped
  1d6 nonlethal-**equivalent** result".
- **What the code does (verified):** `Cord/CordConditionPatches.cs:103-123` rolls 1d6, caps the
  amount at `HPLeft - 1`, and applies it through `RuleDealDamage` with `DirectDamage`. It is real
  HP damage that provably cannot reduce the wearer below 1 HP — exactly the documented adaptation.
- **What survives:** `Cord/CordConditionPatches.cs:132` logs "…becomes {N} **nonlethal damage**".
  That single player-visible string drops the "-equivalent" qualifier that every owner-approved
  text carries, so a player may believe the damage is true nonlethal (healed separately, distinct
  DR/immunity interactions) when it is capped lethal damage.
- **Repair direction:** reword the one log string. No mechanical change.

## GS-AUD-0018 — Two dead policy classes are covered by passing tests, giving false assurance

- **Kind:** HARNESS-DEFECT · **Severity:** S3 · **Evidence:** source-demonstrated; both
  coordinator-verified by reference count
- **Observed:**
  * `Cord/CordSubstitutionPolicy.cs` has **zero** production callers
    (`grep -rn CordSubstitutionPolicy src/ tests/` → 1 hit in its own file, **7** in
    `tests/KingmakerGunslinger.DomainTests/AcadamaeCordPolicyTests.cs`). The live path
    `Cord/CordConditionPatches.cs:103-124` re-implements the rule inline.
  * `ControlledRageHitPointPolicy` is likewise dead, with six passing tests — and the matching
    acceptance item at `URBAN-BARBARIAN-ACCEPTANCE-CHECKLIST.md:216` is **unchecked**.
- **Why it matters:** thirteen tests pass against logic the shipped mod never executes. Any
  divergence between the dead policy and the live inline re-implementation is invisible to CI —
  which is precisely the situation that lets a rule drift unnoticed.
- **Repair direction (not implemented):** either route production through the policy classes
  (preferred — that is evidently what they were for) or delete both classes and their tests.

## GS-AUD-0019 — Brown-Fur historical records contradict each other on cold reconstruction

- **Kind:** DOCUMENTATION-DRIFT · **Severity:** S3 · **Evidence:** source-demonstrated
- **Observed:** `planning/BROWN-FUR-RUNTIME-MATRIX.md:43` asserts save/reload **PASS**, while
  `docs/BROWN-FUR-STARTUP-ORDER-REPAIR.md:235` records cold reconstruction **NOT_RUN**. The
  matrix also still cites **19** registered identities against today's pin of **25**.
- **Adjudication:** per the mission, the newest explicit accepted decision governs — the startup
  repair record is later and records the gap, so **cold reconstruction is NOT_RUN**. The matrix's
  PASS must not be read as closing it, and GS-AUD-0015 is consistent with the gap being real.
- **Repair direction:** mark the matrix superseded. No code change.

## GS-AUD-0020 — Acceptance checklists carry unclosed items that no current evidence closes

- **Kind:** VERIFICATION-GAP · **Severity:** UNASSESSED · **Evidence:** source-demonstrated
- **Observed:** `URBAN-BARBARIAN-ACCEPTANCE-CHECKLIST.md` is **38 items unchecked, 0 checked**.
  `ELEMENTAL-RACES-COMPLETION.md:44-45` records explicit **waivers** for final Treacherous Earth
  native creation/respec/save integration and final Nereid Player/save/profile repeats.
- **Adjudication:** per the mission, *"A waiver records a gap; it does not disappear into PASS."*
  These remain open native-evidence gaps on the 0.0.182 candidate. They are listed here so the
  release conversation cannot mistake an old "completion" document for coverage.
- **Prerequisite to close:** admitted native transactions with the relevant fixtures — none of
  which this campaign has been able to open (see the restoration/native-admission record).

## GS-AUD-0021 — Firearm cold-load token survival is the single highest-value unproven claim

- **Kind:** VERIFICATION-GAP · **Severity:** UNASSESSED (would be S1 if it fails)
  · **Evidence:** source-demonstrated absence
- **Note:** this is GS-AUD-0014's A04-F3 promoted to its own row because of its weight. Firearm
  state (loaded rounds, ammunition kind, Broken/Wrecked) is the Gunslinger module's core
  persistent state. Nothing in the candidate establishes that it survives a process restart, and
  the failure mode is **silent** — a lost token decodes to Normal/empty rather than erroring.
- **Smallest confirming check:** load a firearm and damage it to Broken, save, exit to desktop,
  relaunch, reload, and inspect both loaded rounds and degradation. One disposable save, one
  cold restart.

---

## GS-AUD-0022 — The audit adapter's own failure-path fixture was not offline and launched a real native transaction

- **Kind:** HARNESS-DEFECT — **in this audit's own tooling, not in the product**
- **Severity:** S3 · **Evidence:** native-observed · **Disposition:** CLOSED-MITIGATED
- **Recorded because the mission requires harness and observer failures to be as visible as
  product failures, and because it changes how the A03 evidence should be read.**
- **What happened:** `adapter/Invoke-AuditProfileWithUmmProtection.ps1` takes a `-FixtureFault`
  parameter intended to exercise the composition's failure paths offline. The
  `profile-throws` case is genuinely offline. The `umm-completion-fails` case is **not**: it
  faults only the UMM-completion step and leaves the profile invocation live. Running it
  therefore performed a real deployment of the 0.0.182 candidate to the live installation and
  launched Kingmaker, with no admission record written beforehand.
- **Why the run was not aborted:** force-terminating a live owning transaction is forbidden by
  the mission and by `docs/VALIDATION-POLICY.md` section 6, and would have left the Mods tree
  quarantined mid-transaction. The correct response was to let the owner complete its own
  `finally`/restoration and then discharge the resulting obligation by the documented recovery
  route. That is what was done (EV-000010, EV-000011).
- **What was NOT compromised:** every protection in the path was real, not simulated — the
  pre-flight refusal on a running game, the runtime lease, the byte-verified UMM backup receipt
  written before any launch, and the profile wrapper's own snapshot/restore transaction. The
  profile and scenario were both inside the wrapper's own `ValidateSet`. **No save was involved
  at any point** (`mod-load-smoke` loads none), and all surfaces were later verified byte-exact
  against the campaign intake baseline.
- **How it affects the evidence:** it does not invalidate the A03 result. The native PASS is
  recorded with the real receipts. It does mean the A03 admission record in
  `AUDIT-RESTORATION.md` (T1) was written **retrospectively from receipts** rather than
  prospectively from intent, and it is labelled as such.
- **Corrective action, already taken:** the fixture parameter must not be used again as written.
  Any further native work in this campaign gets its written admission record **before** the
  command is issued, and an offline fixture must fault the profile invocation as well.
- **Repair direction for the adapter (if it is ever reused):** make `umm-completion-fails` imply
  the `profile-throws` short-circuit, or split the parameter into an explicit
  `-Offline` switch that gates the profile call independently of which step is faulted.

---

# Findings 0023–0024 (coordinator, A15) and 0025–0037 (imported from workers W3 and W4)

Full traces remain in `workers/W3/W3-result.md` and `workers/W4/W4-result.md`.
**Everything below is source-demonstrated unless a native receipt is named. Nothing is a PASS.**

## GS-AUD-0023 — Gunslinger's Dodge still has no working native acceptance route on the candidate

- **Kind:** VERIFICATION-GAP · **Severity:** S3 · **Evidence:** **native-observed on 0.0.182**
  (run `20261111T1806397941256Z-89d8f03ce2a64d29a3eaa003d75131cc`, EV-000012)
- **Disposition:** OPEN — a **confirmed-still-present** accepted limitation, not a new defect.
- **Observed:** `disposable-gunslinger-dodge` under `gunslinger-only` returned **ERROR** at stage
  `activate-immediate-dodge`: *"The native Dodge command completed without its timed Buff fact."*
- **Why this is NOT filed as a product regression:** `KNOWN-ISSUES.md:122-124` already records it —
  *"Gunslinger's Dodge command interrupts before `Start`. This is a test-fixture limitation…"* —
  and `AUTONOMOUS-GUNSLINGER-JOURNAL.md:4206-4212` records the identical ERROR at the identical
  stage on 2026-08-05, concluding *"the actual granted `UnitUseAbility` on the detached live-view
  fixture finished `Interrupt` before custom delivery. This is not live timing evidence and
  proves no Grit, buff, AC, icon, or duration behavior."*
- **What IS newly established:** the limitation is **still present on the 0.0.182 release
  candidate**. Combined with GS-AUD-0012 (`GunslingerDodgeRuntime` is dead code, so its
  diagnostic can only read zero), the consequence is that **Gunslinger's Dodge — a level-1
  class deed — has no automated native acceptance path at all.** For a Nexus release that is a
  real coverage hole in a core feature, even though no defect is demonstrated.
- **Prerequisite to close:** a fixture whose granted `UnitUseAbility` is not interrupted before
  delivery (an attached, non-detached live unit), or a human playtest observation.

## GS-AUD-0024 — HYPOTHESIS: the 2026-11-11 incident is more consistent with a null `Blueprint` than a null `FxOnStart`

- **Kind:** CONTRACT-QUESTION / investigation lead · **Severity:** UNASSESSED
  · **Evidence:** **supported-risk — explicitly a hypothesis, with no reproduction**
- **Disposition:** OPEN. Replaces the causal claim withdrawn from GS-AUD-0004 (EV-000012).
- **Reasoning:** inside `Buff::SpawnParticleEffect` there are two unguarded dereferences on the
  blueprint path — `IL_003b call get_Blueprint` followed by `IL_0040 ldfld FxOnStart`. A **null
  `Blueprint`** faults at the `ldfld`, producing exactly the observed frame. Unlike a null
  `FxOnStart`, a null `Blueprint` is **unreachable on fresh application** (you cannot apply a
  buff that has no blueprint) and **is** reachable during `UnitEntityData.SpawnBuffsFxs` on a
  save/area load, for a deserialized buff whose blueprint GUID no longer resolves.
- **Why it fits this incident better than GS-AUD-0004:** it explains why the crash is load-only
  and intermittent, and it does not require the shipped deeds to be broken in ordinary play.
  It also fits this user's history — this host has carried KMG **0.0.117**, **0.0.178** and
  **0.0.182** at different times (EV-000002, GS-AUD-0003), and content was withdrawn/retired
  across those versions (Wall of Ice, Magma Form, summon Sunder, Bebelith Dismantle Armor).
- **What it does NOT claim:** it does not attribute the crash to KMG. A buff blueprint from any
  of the eleven installed mods — six of which are in no compatibility profile — could fail to
  resolve the same way.
- **Smallest check that would settle it:** obtain the original 2,680,892-byte `output_log.txt`
  (GS-AUD-0003). Failing that, a guarded disposable load of a save containing a buff whose
  blueprint has been withdrawn, with the exception captured.

---

## GS-AUD-0025 — Untemplated alternate-caller summons keep the donor's inherited celestial/fiendish template

- **Kind:** PRODUCT-DEFECT · **Severity:** S2 · **Evidence:** source-demonstrated;
  **structurally re-verified by the coordinator**
- **Observed:** `ContextActionRemoveInheritedSummonTemplates` has exactly **one** production
  construction site — `Blueprints/ExpandedSummoningAbilityBuilder.cs:202`, inside the canonical
  ordinary-spell builder (12 named creatures). The alternate-caller adapter's **untemplated**
  branch (`Blueprints/AlternateSummoningAdapters.cs:648-663`) calls `ReplaceSpawnUnits` and
  optionally `AppendAlignmentAction`, but **never appends the removal action** — while the
  *templated* branch twelve lines above (`:630-647`) does handle templates. Coordinator check:
  `grep -rn ContextActionRemoveInheritedSummonTemplates src/ --include=*.cs` outside
  `RuntimeTesting/` returns only the class itself and that single construction site.
- **Failure scenario:** a Monster Tactician / Summoner / Master Summoner / Occultist /
  Naturalist / Feral Hunter / Preservationist summons an untemplated creature whose native
  carrier carries an inherited celestial or fiendish template buff. The summon keeps a template
  the ordinary Summon Monster spell would have stripped — extra resistances, smite, and an
  alignment descriptor the player did not choose.
- **Why it is an oversight rather than a design choice:** the asymmetry is inside a single
  method, and the sibling branch does the right thing.
- **GS-AUD-0026 explains why CI never caught it** (below).
- **Smallest confirming check:** summon one untemplated creature through an alternate caller
  and inspect the spawned unit's fact list for the donor template buff.

## GS-AUD-0026 — The composed-action contract classifies the template-removal action as `Other` and never compares it

- **Kind:** HARNESS-DEFECT · **Severity:** S3 · **Evidence:** source-demonstrated
- **Observed:** the composed-action comparison that would have caught GS-AUD-0025 classifies
  `ContextActionRemoveInheritedSummonTemplates` as `Other` and excludes it from comparison.
- **Why it matters:** the canonical and adapter paths are supposed to be contract-compared; the
  one action whose absence changes gameplay is the one the comparison ignores.
- **Repair direction:** classify it explicitly and compare it. Would fail today on GS-AUD-0025.

## GS-AUD-0027 — The Races Unleashed version pin fails open

- **Kind:** PRODUCT-DEFECT · **Severity:** S2 · **Evidence:** source-demonstrated
- **Observed:** the Spider Summoner integration's RU version check **fails open** — an
  unrecognised or unreadable provider version proceeds rather than disabling the integration.
- **Why it matters for a Nexus release:** Races Unleashed will update independently of KMG. A
  future RU whose spider blueprints changed would be integrated against anyway, silently. The
  project's own stated posture elsewhere (`docs/FAVORED-CLASS-COMPATIBILITY.md`,
  `docs/BETTER-VENDORS-COMPATIBILITY.md`) is exact-binary gating, which this does not match.
- **Smallest confirming check:** stage a modified RU `Info.json` version string and observe
  whether the integration still publishes.

## GS-AUD-0028 — Two session-scoped grapple/hold release paths cannot be settled from source

- **Kind:** VERIFICATION-GAP · **Severity:** UNASSESSED (would be S1/S2 if confirmed)
  · **Evidence:** source-demonstrated absence
- **Observed:** W3's A10-6 and A10-7 concern whether grab/hold/swallow/engulf link state releases
  cleanly in every path. Session-scoped release on reload is **by design** and must not be
  reported as a bug; what is unverified is the non-reload paths (death, dismissal, scene change).
- **Smallest check:** a disposable summon that grapples a target, then kill the summon, then
  change scene, inspecting the target's link state after each.

## GS-AUD-0029 — The alternate-summoning authority document still states the superseded name-first menu order

- **Kind:** DOCUMENTATION-DRIFT · **Severity:** S3 · **Evidence:** source-demonstrated
- **Observed:** `docs/ALTERNATE-SUMMONING-SOURCES.md:86-87` states the old alphabetical/name-first
  ordering rule as current. 0.0.182 orders by **quantity group first, then alphabetically within
  each group**.
- **Why this one is corrosive rather than cosmetic:** that document is the nominated authority on
  intent for the seven alternate caller families. A future maintainer reconciling code against it
  would "fix" correct code backwards.

## GS-AUD-0030 — Nimble is granted one level early at all five tiers, and the fidelity matrix calls that "EXACT"

- **Kind:** CONTRACT-QUESTION (gameplay fidelity) + DOCUMENTATION-DRIFT · **Severity:** S2
  · **Evidence:** source-demonstrated; **independently verified by the coordinator**
- **Observed (coordinator-verified):** `Blueprints/GunslingerClassBlueprints.cs:486-489` computes
  `nimbleIndex = level < 2 || (level - 2) % 4 != 0 ? -1 : (level - 2) / 4`, granting Nimble at
  levels **2, 6, 10, 14, 18**. `Classes/NimbleService.cs:14-18` agrees:
  `gunslingerLevel < 2 → 0`, else `Math.Min(5, 1 + ((gunslingerLevel - 2) / 4))`.
  The Pathfinder tabletop Gunslinger gains Nimble +1 at **3rd** level and +1 every four levels
  thereafter (**7, 11, 15, 19**).
- **The documentation conflict:** `planning/GUNSLINGER-FIDELITY-MATRIX.md:66` records the row as
  `| 2/6/10/14/18 | Nimble +1..+5 | EXACT |`. Every **other** row in that same matrix sits on its
  correct tabletop level (Deadeye/Dodge/Quick Clear at 1; Gunslinger Initiative and Pistol-Whip
  at 3), and deliberate divergences elsewhere are labelled `ADAPTED` or
  `OMITTED-NO-MEANINGFUL-INTERACTION`. So the off-by-one is anomalous within the project's own
  scheme.
- **Scope:** every Gunslinger character gets a +1 AC dodge bonus one level earlier than tabletop,
  at five separate tiers.
- **Not adjudicated:** whether the levels or the label should change is the owner's call. But
  **one of the two is wrong** — if the placement is deliberate, the row must not say `EXACT`.

## GS-AUD-0031 — `BETTER-VENDORS-COMPATIBILITY.md` describes vendor rows the candidate does not publish, and a second document says the opposite

- **Kind:** DOCUMENTATION-DRIFT · **Severity:** S2 (two docs in direct conflict on shipped
  acquisition behaviour) · **Evidence:** source-demonstrated
- **Observed:** `docs/BETTER-VENDORS-COMPATIBILITY.md:319-327` states KMG publishes fixed
  `SmithVendorTable` rows for the four melee "+1" items. Both campaign publications pass an
  **empty kind array** for that exact table GUID
  (`Blueprints/EasternWeaponCampaignBlueprints.cs:21-24`,
  `Blueprints/ElvenBranchedSpearCampaignBlueprints.cs:23-25`), and
  `Blueprints/CapitalVendorBlueprints.cs:85-87` puts those items in the *owned* set so
  `NormalizeOwned` removes them. `docs/ELVEN-BRANCHED-SPEAR-PLACEMENT-MANIFEST.md:24` states the
  opposite of the Better Vendors doc and **agrees with the code**.
- **Impact:** a player following the compatibility doc looks for four items at a vendor that
  does not stock them.
- **Repair direction:** correct the Better Vendors document to match the code and the placement
  manifest, or publish the rows. Owner decision.

## GS-AUD-0032 — A base Gunslinger's 4th Gun Training pick grants nothing

- **Kind:** PRODUCT-DEFECT · **Severity:** S3 · **Evidence:** source-demonstrated
- **Observed:** a base Gunslinger receives four Gun Training selections (levels 5/9/13/17) but
  only three official firearm types exist, so the level-17 pick has no selectable option and the
  level grants nothing. Disclosed **only in a source comment** — absent from `KNOWN-ISSUES.md`
  and from the fidelity matrix.
- **Impact:** a dead level at 17 for every base Gunslinger, with no player-facing explanation.
- **Repair direction:** owner decision (add a fourth option, or collapse to three and document).
  At minimum it belongs in `KNOWN-ISSUES.md`.

## GS-AUD-0033 — "Select two different deeds" has no production enforcement

- **Kind:** CONTRACT-QUESTION · **Severity:** S3 · **Evidence:** source-demonstrated
- **Observed:** `TrueGritCatalog.IsValidPair` (the `first != second` rule) is referenced **only by
  a domain test**. The only live guard is `Ranks = 1` plus native filtering. The codebase
  demonstrably knows how to write an explicit self-guard
  (`Blueprints/HeirloomNodachiBlueprints.cs:89-95`), and the Sprint-59 evidence only ever picked
  two *different* deeds — so the duplicate case was never exercised.
- **Same shape as GS-AUD-0018:** a rule that exists only as test-only code.
- **Smallest check:** attempt to select the same deed twice at level-up.

## GS-AUD-0034 — Both KMG postfixes on `ExtractSelectionItems` silently discard third-party selection items

- **Kind:** PRODUCT-DEFECT (compatibility) · **Severity:** S3 · **Evidence:** source-demonstrated
- **Observed:** both KMG postfixes on `BlueprintParametrizedFeature.ExtractSelectionItems`
  narrow the result with `.OfType<FeatureUIData>()` and assign it back, so any selection item
  contributed by another mod that is not a `FeatureUIData` is dropped.
- **Impact:** a silent, hard-to-diagnose compatibility failure for an unrelated mod's feature
  selection — exactly the class of problem a Nexus audience reports as "KMG broke mod X".
- **Repair direction:** preserve non-`FeatureUIData` entries instead of narrowing.

## GS-AUD-0035 — A teleport destination row with no bindable reader is still offered and does nothing

- **Kind:** PRODUCT-DEFECT · **Severity:** S3 · **Evidence:** source-demonstrated
- **Observed:** a destination row is offered for a scroll group that has no bindable reader;
  activating it closes the destination panel and only writes a log line.
- **Why it matters:** the mission names "no offers without an executable action" as an explicit
  A16 requirement. This is a direct instance.
- **Smallest check:** open the world-map destination panel with a scroll but no valid reader and
  click the row.

## GS-AUD-0036 — Firearm crafting rollback loses the original exception and cannot undo an over-refund it detects

- **Kind:** PRODUCT-DEFECT · **Severity:** S3 · **Evidence:** source-demonstrated
- **Observed:** the firearm crafting rollback path discards the original exception (so the real
  cause is unrecoverable from logs) and, where it detects an over-refund, has no way to undo it.
- **Impact:** a currency/inventory discrepancy that cannot be diagnosed after the fact.
- **Repair direction:** preserve the inner exception; make the refund path transactional.

## GS-AUD-0037 — Class/archetype/progression and world-map UI native evidence is pinned to earlier commits and was not re-run

- **Kind:** VERIFICATION-GAP · **Severity:** UNASSESSED · **Evidence:** source-demonstrated
- **Observed:** W4 A05-5 — all class/archetype/progression native evidence is attributed to
  earlier commits; W4 A16-4 — the teleport UI-coexistence evidence (reopen without duplicates,
  native callback restoration) is pinned to **0.0.119**. Neither was re-run on this candidate.
- **Adjudication:** per the mission, a historical PASS on a different artifact is
  `HISTORICAL-ONLY`, not current coverage. Recorded so release discussion does not treat these
  as covered.
- **Prerequisite to close:** admitted native runs on the 0.0.182 candidate.

---

## GS-AUD-0038 — `up-close-critical` fails only when another scenario runs first in the same game process

- **Kind:** HARNESS-DEFECT · **Severity:** S3 · **Evidence:** **native-observed, intermittent**
- **Disposition:** OPEN. Attributed to the harness, with a product explanation explicitly retained
  as not excluded.
- **Observed, on the frozen 0.0.182 candidate:**

  | Run | How it ran | Result |
  |---|---|---|
  | `20261111T1816557789702Z-disposable-pistolero-deeds` | 2nd scenario in a shared process, after `disposable-gunslinger-targeting-arms` | **FAIL** — `up-close-critical` FAIL, other 9 PASS |
  | `20261111T1821405012055Z-disposable-pistolero-deeds` | alone, fresh process | **PASS** — including `up-close-critical` |

  Nothing else differed: same package, same DLL, same profile, same parameters.
- **Why the harness is the favoured explanation:** the stage forces a die roll with
  `UnityEngine.Random.InitState(FindNativeD6Seed(6))`
  (`RuntimeTesting/RuntimeTestRunner.cs:33962`), and `KNOWN-ISSUES.md:126-129` already records
  that this aggregate "has **order-sensitive forced-roll** and detached target fixtures for
  **Targeting**/Bleeding Wound" — and the scenario that preceded it was a Targeting scenario.
- **Competing explanation retained (not excluded):** a genuine ordering-dependent defect in the
  Up Close and Deadly critical path. The evidence cannot separate the two, because the assertion
  conjoins two conditions —
  `criticalUnmultiplied = hp - target.HPLeft == hitDamage && !attacker.Descriptor.HasFact(pistolero.UpCloseArmed);`
  (`RuntimeTestRunner.cs:33966-33967`) — and the retained result does not record which half failed.
- **Why it was investigated rather than dismissed:** `PISTOLERO-MUSKET-MASTER-JOURNAL.md:1010-1024`
  records this same assertion PASSING on an earlier artifact with "unmultiplied critical packet"
  explicitly proven, so a fresh FAIL was a credible regression candidate. One justified targeted
  repeat after clean restoration resolved it to intermittent.
- **Implication that matters beyond this one assertion:** **batched multi-scenario native results
  from this harness are weaker evidence than the same scenarios run one per process.** Prior
  batched PASS records in this project should be read with that in mind.
- **Repair direction (not implemented):** make the assertion report which half failed, and either
  isolate the forced-roll stages per process or re-seed deterministically at stage entry.
