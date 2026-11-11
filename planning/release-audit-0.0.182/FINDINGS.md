# Audit findings — coordinator-maintained register

Campaign KMG-AUD-20261111T170737Z. Frozen candidate aa91f46571eb239df36e0233082153f2cba01899,
version 0.0.182. IDs are assigned only by the coordinator. Every material status change is
appended to AUDIT-OUTCOMES.md. Severity is a provisional impact judgement, not proof.

## Ranked index

| ID | Kind | Severity | Evidence strength | Feature | Title | Disposition | Evidence |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GS-AUD-0004 | CONTRACT-QUESTION (was PRODUCT-DEFECT) | **S3, was S1** | omission source-demonstrated; consequence **natively DISCONFIRMED** | Buff invariant (A15) | 21 buff identities omit `FxOnStart`/`FxOnRemove`, deviating from the project's own stated invariant | OPEN — **CORRECTED** | EV-000004, EV-000012, EV-000014 |
| GS-AUD-0001 | machine-intake provenance | S3 (was S2) | native-observed | Host environment | Installed mod is 0.0.117, not the candidate — the "recent play experience" inference is **withdrawn** | OPEN | EV-000002, EV-000017 |
| GS-AUD-0003 | VERIFICATION-GAP | UNASSESSED | observed-once (external), local evidence absent | A15 incident reconciliation | The supplied 2026-11-11 incident log does not exist on this host; last local game run was 2026-11-07 | OPEN | EV-000004 |
| GS-AUD-0006 | PRODUCT-DEFECT | S2 | source-demonstrated | Feature-module settings (A19) | A failed settings-migration write misclassifies a valid older file as malformed and resets all 13 modules to ON | OPEN | EV-000007 |
| GS-AUD-0007 | CONTRACT-QUESTION | S3 | source-demonstrated | Feature-module settings (A19) | A future settings schema makes the whole mod fail to load, not just the settings file | OPEN | EV-000007 |
| GS-AUD-0005 | HARNESS-DEFECT | S3 | native-observed (executed gate) | Release validation gate | The broad gate depends on an untracked ~99 MB local IL dump; a fresh checkout fails 23 tests with an opaque path error | OPEN | EV-000005 |
| **GS-AUD-0039** | **PRODUCT-DEFECT** | **S2** | source-demonstrated + **engine IL measured** | Scatter Shot (A06) | **Scatter's documented −2 attack penalty is applied as a +2 bonus** — a 4-point swing on every scatter attack | OPEN | EV-000018 |
| **GS-AUD-0044** | OBSERVED-FAILURE (candidate defect) | **S2** | **native-reproduced once** | Dead Shot audio (A20) | Dead Shot appears to commit ~11 audio events where its contract requires exactly 1 | OPEN | EV-000019 |
| GS-AUD-0045 | HARNESS-DEFECT (stale pin) | S3 | native-observed + source | Firearm visual rigs (A20) | The rig assertion is hard-pinned to `items.Count == 14`; the roster is now 16 | OPEN | EV-000019 |
| GS-AUD-0046 | HARNESS-DEFECT (stale pin) | S3 | native-observed | Item presentation (A20) | Presentation counts are stale (57/12 vs 55/11); `issues=<none>` — the text itself is clean | OPEN | EV-000019 |
| GS-AUD-0047 | OBSERVED-FAILURE-UNATTRIBUTED | UNASSESSED | native-observed once | Firearm visual rigs (A20) | `pistol-authored-marker-contract` reports `exact=False`; which sub-condition failed is unrecoverable | OPEN | EV-000019 |
| GS-AUD-0040 | DOCUMENTATION-DRIFT (was CONTRACT-QUESTION) | S3 (was S2) | source-demonstrated | Shield Other (A15) | The shipped ability description contradicts the **approved** distance-independent bond contract; code and test are correct | OPEN — **RECLASSIFIED** | EV-000018, EV-000019 |
| GS-AUD-0041 | DOCUMENTATION-DRIFT | S3 | source-demonstrated | Protection from Alignment (A15) | Catalog has 23 entries, authority table lists 21; the Suggestion entry may violate the doc's own exclusion list | OPEN | EV-000018 |
| GS-AUD-0042 | DOCUMENTATION-DRIFT | S3 | source-demonstrated; consequence UNVERIFIED | Scatter Shot (A06) | ADR-0037 requires Vital Strike exclusion; only precision damage is suppressed | OPEN | EV-000018 |
| GS-AUD-0043 | PRODUCT-DEFECT (presentation) | S3 | source-demonstrated; gated on unverified engine behaviour | Magic Circle (A15) | Decal reuse does not match `AreaId`/`Alignment` | OPEN | EV-000018 |
| GS-AUD-0015 | CONTRACT-QUESTION / conditional risk | UNASSESSED (was S2) | guard source-demonstrated; **consequence UNVERIFIED** | Brown-Fur Transmuter (A11) | Persisted Powerful Change restoration needs a resolvable live caster; the cross-area +4 result is NOT established | OPEN | EV-000009, EV-000017 |
| GS-AUD-0013 | CONTRACT-QUESTION | S2/S3 | source-demonstrated, coordinator-verified | Gunslinger firing (A06) | Misfire suppression is released only by player input; an AI companion stays locked out after repair | OPEN | EV-000009 |
| GS-AUD-0021 | VERIFICATION-GAP | UNASSESSED (S1 if it fails) | source-demonstrated absence | Firearm persistence (A04) | Cold-load firearm token survival is unproven and fails silently | OPEN | EV-000009 |
| GS-AUD-0008 | DOCUMENTATION-DRIFT | S3 | source-demonstrated | Firearm maintenance (A04) | Stale Broken-or-Wrecked field-repair prose in README and seven other places | OPEN | EV-000009 |
| GS-AUD-0016 | CONTRACT-QUESTION | S3 | engine behaviour proven from IL; reachability not demonstrated | Save/load robustness (A11/A13) | Two UnitParts validate by throwing inside an engine callback that has no catch handler | OPEN | EV-000009 |
| GS-AUD-0018 | HARNESS-DEFECT | S3 | source-demonstrated | Cord / Urban Barbarian (A13/A14) | Thirteen tests pass against two dead policy classes the shipped mod never executes | OPEN | EV-000009 |
| GS-AUD-0009 | PRODUCT-DEFECT (latent) | S3 | source-demonstrated; reachability LOW | Firearm state (A04) | Capacity-6 Advanced Revolver cannot persist state past one round; reload silently rolls back | OPEN | EV-000009 |
| GS-AUD-0012 | PRODUCT-DEFECT (diagnostics) | S3 | source-demonstrated | Gunslinger's Dodge (A06) | GunslingerDodgeRuntime is dead code, so its pre-AC evidence can only read zero | OPEN | EV-000009 |
| GS-AUD-0017 | DOCUMENTATION-DRIFT | S3 | source-demonstrated | Cord of Stubborn Resolve (A14) | Combat log says "nonlethal damage" where the approved contract says nonlethal-EQUIVALENT | OPEN | EV-000009 |
| GS-AUD-0019 | DOCUMENTATION-DRIFT | S3 | source-demonstrated | Brown-Fur Transmuter (A11) | Brown-Fur historical records need explicit scope notes; they are not inherently contradictory | OPEN | EV-000009, EV-000017 |
| GS-AUD-0010 | CONTRACT-QUESTION | S3 | source-demonstrated | Firearm maintenance (A04) | Broken-only field repair is enforced only in callers, not at the state transition | OPEN | EV-000009 |
| GS-AUD-0011 | CONTRACT-QUESTION | S3 | source-demonstrated | Deadeye (A06) | Deadeye charges a flat 1 Grit and discards the per-increment cost it computes | OPEN | EV-000009 |
| GS-AUD-0014 | VERIFICATION-GAP | UNASSESSED | source-demonstrated absence | Firearm maintenance / action economy (A04/A06) | Three firearm claims are not settleable from source (rest/autosave order, reload action borrowing, token survival) | OPEN | EV-000009 |
| GS-AUD-0020 | VERIFICATION-GAP | UNASSESSED | source-demonstrated | Urban Barbarian / Elemental races (A12/A13) | Acceptance checklists carry 38 unchecked items and two explicit native waivers that nothing closes | OPEN | EV-000009 |
| GS-AUD-0025 | PRODUCT-DEFECT | S2 | source-demonstrated, coordinator-verified | Expanded summoning (A08) | Untemplated **Summon Monster** alternate-caller children keep the donor's inherited template (scope narrowed from "all seven families") | OPEN | EV-000013, EV-000017 |
| ~~GS-AUD-0027~~ | **WITHDRAWN — FALSE POSITIVE** | ~~S2~~ none | source verified: structural gate exists | Spider Summoner / RU (A08) | ~~RU version pin fails open~~ — unsupported input is refused before mutation and generated rows are withheld | **WITHDRAWN** | EV-000017 |
| ~~GS-AUD-0030~~ | **WITHDRAWN — FALSE POSITIVE** | ~~S2~~ none | rules text verified: mod is CORRECT | Gunslinger progression (A05) | ~~Nimble granted one level early~~ — tabletop Nimble IS 2/6/10/14/18; the mod and its `EXACT` label are right | **WITHDRAWN** | EV-000017 |
| GS-AUD-0031 | DOCUMENTATION-DRIFT | S2 | source-demonstrated | Vendors / acquisition (A17) | Better Vendors doc misdescribes the "+1" acquisition path; items ARE available via Dire Narlmarches/Pitax/BTSL plus BV Military rank I | OPEN | EV-000013, EV-000018 |
| GS-AUD-0024 | CONTRACT-QUESTION / lead | UNASSESSED | supported-risk — hypothesis, no reproduction | A15 incident | HYPOTHESIS: the incident fits a null `Blueprint` on a deserialized buff better than a null `FxOnStart` | OPEN | EV-000012 |
| GS-AUD-0038 | OBSERVED-FAILURE-UNATTRIBUTED | S3 | native-observed, non-reproducing; **mechanism withdrawn** | Pistolero deeds (A06) | Same assertion failed once and passed once on identical inputs — cause unresolved (the two ran in SEPARATE game processes) | OPEN | EV-000014, EV-000017 |
| GS-AUD-0023 | VERIFICATION-GAP | S3 | native-observed on 0.0.182 | Gunslinger's Dodge (A06) | Dodge still has no working native acceptance route; known fixture limitation confirmed present | OPEN | EV-000012 |
| GS-AUD-0028 | VERIFICATION-GAP | UNASSESSED (S1/S2 if confirmed) | source-demonstrated absence | Summon lifecycle (A10) | Grapple/hold release on death, dismissal and scene change is unverified | OPEN | EV-000013 |
| GS-AUD-0026 | HARNESS-DEFECT | S3 | source-demonstrated | Expanded summoning (A08) | The composed-action contract classifies the template-removal action as Other and never compares it | OPEN | EV-000013 |
| GS-AUD-0029 | DOCUMENTATION-DRIFT | S3 | source-demonstrated | Alternate summoning (A08) | ALTERNATE-SUMMONING-SOURCES.md still states the superseded name-first menu order as current | OPEN | EV-000013 |
| GS-AUD-0032 | PRODUCT-DEFECT | S3 | source-demonstrated | Gunslinger progression (A05) | A base Gunslinger's 4th Gun Training pick (level 17) has no selectable option and grants nothing | OPEN | EV-000013 |
| GS-AUD-0033 | CONTRACT-QUESTION | S3 | source-demonstrated | True Grit deeds (A05) | "Select two different deeds" is enforced only by test-only code | OPEN | EV-000013 |
| GS-AUD-0034 | CONTRACT-QUESTION / conditional risk | S3 | source-demonstrated; no affected mod identified | Feature selection (A17) | `ExtractSelectionItems` postfixes narrow to `FeatureUIData`, which would discard other interface implementations | OPEN | EV-000013, EV-000017 |
| GS-AUD-0035 | PRODUCT-DEFECT | S3 | source-demonstrated | Teleportation UI (A16) | A destination row with no bindable reader is offered and does nothing but log | OPEN | EV-000013 |
| GS-AUD-0036 | PRODUCT-DEFECT | S3 | source-demonstrated | Firearm crafting (A17) | Crafting rollback loses the original exception and cannot undo a detected over-refund | OPEN | EV-000013 |
| GS-AUD-0037 | VERIFICATION-GAP | UNASSESSED | historical-only | Progression + teleport UI (A05/A16) | Native evidence for these areas is pinned to earlier commits and was not re-run on the candidate | OPEN | EV-000013 |
| GS-AUD-0022 | HARNESS-DEFECT (this audit's own) | S3 | native-observed | Audit adapter | The adapter's `umm-completion-fails` fixture is not offline: it launched a real native transaction | CLOSED-MITIGATED | EV-000010 |
| GS-AUD-0002 | ENVIRONMENT-BLOCKER | S3 | native-observed AND mechanism natively demonstrated (EV-000010) | Host environment / UMM | Live UMM `Params.xml` was left in the 2026-11-07 test-profile state, listing only 2 of 11 installed mods | OPEN | EV-000002 |

---

## GS-AUD-0004 — 21 KMG buff identities omit `FxOnStart`/`FxOnRemove`, deviating from the project's own stated invariant

- **Kind:** CONTRACT-QUESTION / latent consistency deviation · **Severity:** **S3**
- **Evidence:** the **omission** is source-demonstrated and hand-verified at every site.
  The originally claimed **consequence** was natively **disconfirmed** by this campaign.
- **Disposition:** OPEN. Originally filed PRODUCT-DEFECT / S1 with a crash-cause claim; that
  claim is **withdrawn**. See the clearly-marked historical section at the end.

---

## CURRENT ADJUDICATION

The review was right that the earlier text asserted the withdrawn reasoning as current fact.
The three questions are separated here deliberately, because only the first is settled.

### 1. Source sites that omit the explicit assignment — **ESTABLISHED**

Every `ScriptableObject.CreateInstance<BlueprintBuff>()` site in `src/` was enumerated by
analyzer (`tools-fxscan.py`, raw output `evidence/A15/fxscan.json`) and then **read by hand**:
41 sites, of which **20 never assign `FxOnStart`/`FxOnRemove`**, covering **21 buff identities**.

Nothing downstream fills them in. Verified:
`Blueprints/BlueprintRegistry.cs` — `Register<T>` contains no reference to `FxOnStart`,
`FxOnRemove`, `PrefabLink` or `ResourceAssetIds`; `Blueprints/BlueprintUnitFactAccess.cs:34-65` —
`Configure(...)` sets only `m_DisplayName`, `m_Description` and `m_Icon`.

Player-facing identities registered with stable GUIDs:
`KMG_WayfarersOath_Equipped`, `KMG_MoonlitCrossing_Equipped`, `KMG_FallingPetal_AC_Buff`,
`KMG_MountainSunder_Round_Marker` (`EasternWeaponNamedBlueprints.cs:409`, registered :185/:195/:200/:210);
`KMG_Boughkeeper_AC_Buff`, `KMG_Thornstep_Speed_Penalty`, named-spear round markers
(`ElvenBranchedSpearNamedBlueprints.cs:278`, :120/:130/:267);
`KMG_BleedingWound_<kind>_Buff` and `_Armed` (`BleedingWoundBlueprints.cs:112`, `:102`);
`KMG_Deadeye_Armed_Buff` (`DeadeyeBlueprints.cs:71`);
`KMG_GunslingerDodge_ArmorClass_Buff` (`GunslingerDodgeBlueprints.cs:74`);
`KMG_DeathsShot_DeathEffect`/`_Armed` (`DeathsShotBlueprints.cs:54`, `:68`);
`KMG_StartlingShot_FlatFooted_Buff` (`StartlingShotBlueprints.cs:54`);
`KMG_StunningShot_Armed` (`StunningShotBlueprints.cs:71`);
`KMG_TargetingHead_Confusion_Buff` (`TargetingHeadBlueprints.cs:49`);
`KMG_ExpertLoading_Armed` (`ExpertLoadingBlueprints.cs:50`);
`KMG_LightningReload_Used` (`LightningReloadBlueprints.cs:53`);
`KMG_SteadyAim_Armed` (`MusketMasterBlueprints.cs:90`);
`KMG_UpCloseAndDeadly_Armed` (`PistoleroBlueprints.cs:146`);
`KMG_SlingersLuck_<suffix>_Armed` (`SlingersLuckBlueprints.cs:93`).
Lower exposure: `ExpandedSummoningSpecialBuilder.Suggestion32.cs:142` and three
`RuntimeTesting/` fixture buffs.

**The invariant they deviate from is the project's own**, stated verbatim at
`Blueprints/PaperCartridgeModeBlueprints.cs:56-60`:
> "Buff.SpawnParticleEffect and Buff.OnRemove call Load(false) on these links without null
> guards when a persistent buff's unit view is rebuilt. Empty links are the engine-safe
> representation of an intentionally effect-free buff..."

The project applied that invariant twice as point fixes — 0.0.74 (Paper Cartridge marker) and
0.0.88 (Focused Aim) — but never generalised it. The only guard is a **source-text substring
assertion over one unrelated file** (`FocusedAimSaveCompatibilityTests.cs:9-17`).

### 2. The behaviour actually observed natively — **NARROW, and it CONTRADICTS the original claim**

`disposable-pistolero-deeds` arms and consumes **`KMG_UpCloseAndDeadly_Armed`** — one of the 21.
On the frozen 0.0.182 candidate it was applied to a live unit and consumed across six stages, in
**two independent runs**, with **no `NullReferenceException`**.

That matters because the dereference really is reached on ordinary application:
`Buff::Activate()` calls `SpawnParticleEffect()` directly, and `IsParticleEffectValid()` returns
**false** for a never-spawned effect, so there is no short-circuit.

**Therefore `FxOnStart` is not null at runtime for that blueprint.** The "omitted ⇒ null ⇒
throws" premise is false as a general rule.

**What this does NOT establish** (the review's point, and it is correct): one buff behaving
correctly does **not** prove the runtime state of the other 20 identities, nor every
initialization path, nor any cold-reconstruction path. The native evidence is narrow.

### 3. Paths that remain unverified — **OPEN**

- The runtime `FxOnStart` state of the other **20** identities (only one was exercised).
- Any **save-load / cold-reconstruction** path for any of them. **No save was loaded in this
  campaign at all**, so `UnitEntityData.SpawnBuffsFxs` was never reached.
- `FxOnRemove` and `ResourceAssetIds`, which were never exercised in any observed path.

### Why this is still worth fixing at S3

21 shipped identities deviate from an invariant the project wrote down for itself and has already
had to repair twice after real incidents. The cost of compliance is three field assignments; the
cost of being wrong is, on the project's own prior experience, an unloadable save. That is a
reasonable thing to tidy — it is **not** a demonstrated defect and **not** a release blocker.

### Suggested repair direction (not implemented)

Initialise `FxOnStart`/`FxOnRemove`/`ResourceAssetIds` in the one shared creation helper, or
normalise them in `BlueprintRegistry.Register` for `BlueprintBuff`. **Stable GUIDs must be
preserved** — this is field initialisation, not identity change.

### Regression idea — corrected

A reflective domain test asserting non-null `FxOnStart`/`FxOnRemove`/`ResourceAssetIds` for every
registered `BlueprintBuff`. **Correction:** the original entry claimed such a test "would fail
today on all 21 identities". That is now known to be wrong — at least one of the 21 has a
non-null link at runtime, and the test would run against *registered blueprint objects*, not
source text. What the test would actually do is **pin the invariant going forward**; what it
would report today is unknown until it is written.

---

## HISTORICAL — ORIGINAL ANALYSIS, WITHDRAWN 2026-11-11 (EV-000012, EV-000014)

> Retained for the record under the append-only principle. **Everything in this section is
> superseded by the adjudication above. Do not act on it.**

The original finding was PRODUCT-DEFECT / S1 and claimed this omission explained the
2026-11-11 area-load crash. Its reasoning was:

1. `Kingmaker.UnitLogic.Buffs.Buff::SpawnParticleEffect` executes
   `IL_0040 ldfld FxOnStart` / `IL_0046 callvirt Load(bool)` with **no null guard** — measured
   from the qualified reference assembly. *(This part remains true and is still the reason the
   invariant exists.)*
2. 21 identities omit the field. *(Still true — see §1 above.)*
3. **Therefore** those links are null at runtime and the listed buffs produce a deterministic
   path to `NullReferenceException` during `SpawnBuffsFxs` on area load. ***This inference was
   wrong.***
4. It was supported by `docs/GUNSLINGER-SAVE-LOAD-FORENSICS.md:56-58`, which records a null
   project buff FX link being dereferenced and **reproduced twice** in 0.0.88 after a factory
   omitted exactly these three fields. *(That history is real; it does not generalise to every
   omitting factory, which is the step I got wrong.)*

**How it was withdrawn.** Further disassembly showed `Buff::Activate()` reaches the same
dereference on *ordinary application*, which would make the shipped deeds crash on first use —
a disconfirming argument. A first native attempt (`disposable-gunslinger-dodge`, T2) was
**inconclusive** because the command interrupted before delivery (GS-AUD-0023). A second
(`disposable-pistolero-deeds`, T3/T4) applied an affected buff six times with no exception,
settling it.

**Replacement hypothesis for the incident:** a null `Blueprint` rather than a null `FxOnStart` —
reachable only on a deserialized buff whose GUID no longer resolves, i.e. load-only, which fits
both the stack and the reported intermittency. Filed **untested** as GS-AUD-0024. The incident
itself remains **unreproduced and unattributed**.
## GS-AUD-0001 — The installed mod on OLD LAPTOP is 0.0.117, not the 0.0.182 release candidate

> **CORRECTED 2026-11-11 (EV-000017).** The original entry inferred that "the user's recent play
> experience is therefore not this release". **That inference is withdrawn** — it contradicted
> this audit's own GS-AUD-0003, which establishes that the incident log is *not* from this host.
> OLD LAPTOP was never shown to be the incident machine. Severity reduced S2 → S3: since 0.0.182
> was successfully staged and tested five times, the intake version is not a blocker to auditing
> the candidate.

- **Kind:** ENVIRONMENT-BLOCKER → **machine-intake provenance** · **Severity:** S3 (was S2)
- **Evidence:** native-observed · **Disposition:** OPEN (informational)
- **Observed:** `Mods\KingmakerGunslinger\Info.json` declares `"Version": "0.0.117"`; the DLL
  carries 0.0.117 identity (SHA-256 `FD2FC61C…8F35F`). Its `FeatureModules.json` is
  schemaVersion 10 with **11** modules, missing `teleportation-spells` and
  `magic-circle-spells`, consistent with that older artifact.
- **Why it still matters:** it is the baseline every native transaction must restore to.
  Restoring "the current release" would have been wrong. All five transactions restored 0.0.117
  exactly.
- **What it does NOT establish:** which version produced any particular play session, and in
  particular nothing about the machine on which the 2026-11-11 incident occurred (GS-AUD-0003).
- **Negative control:** the prior compatibility transaction reports verified exact restoration,
  and the mod tree's own file timestamps are the packaging-normalised `2026-01-01T05:00:00Z`.
  Consistent with 0.0.117 being a genuine pre-existing installation, not damage.

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

## GS-AUD-0006 — A failed settings-migration write turns a *valid* older settings file into "malformed", resetting every module to ON

> **NARROWED 2026-11-11 (EV-000017).** The original entry said "every upgrading player passes
> through this write". Corrected: the write runs for **any player whose existing settings schema
> is older than 12** — which is every upgrade *from an older schema*, but not an upgrade between
> two builds that both use schema 12.

- **Kind:** PRODUCT-DEFECT · **Severity:** S2 · **Evidence:** source-demonstrated
- **Disposition:** OPEN
- **Affected:** `src/KingmakerGunslinger/FeatureModules/FeatureModuleSettingsStore.cs:23-97`
- **Supported expected behavior and authority:** mission card A19 requires four states to be
  *distinguished*: missing/default, **supported older schema preserving values**, malformed
  quarantined/default, and future-schema rejection. A well-formed older-schema file is the second
  case and its values must be preserved.
- **Observed:**

  ```csharp
  try {
      JObject root = JObject.Parse(File.ReadAllText(path));      // :25
      ...                                                        // :35-60  all 13 keys read
      var state = new FeatureModuleSettingsState(...);           // :61  values PARSED CORRECTLY
      if (schema < CurrentSchemaVersion) Save(state);            // :70  <-- INSIDE the try
      return state;                                              // :71
  }
  catch (Exception exception) {                                  // :73
      ...quarantine copy...                                      // :82
      warning("Malformed feature-module settings at " + path +
              "; recovered defaults (all modules ON); " ...);    // :90-93
      return new FeatureModuleSettingsState(
          FeatureModuleConfiguration.Defaults, path,
          "malformed-recovery", true);                           // :94-96
  }
  ```
  The migration write at `:70` is inside the same `try` as the parse, so **any I/O failure in
  `Save` is caught by the generic `catch (Exception)`**, which discards the already-correctly-parsed
  configuration and returns all-ON defaults with a message saying the file was malformed. It was not.
- **Reachability — precondition stated:** the file must have `schemaVersion < 12` (so the write
  runs) **and** the write must fail. Reachable write failures: the file carries the read-only
  attribute (a common way users pin settings); the `Mods` tree is under
  `C:\Program Files (x86)\…` and the process lacks write permission; or an antivirus/backup agent
  holds a transient lock. `Save` uses `File.Replace` (`:137`), which throws
  `UnauthorizedAccessException` on a read-only destination.
  **This host is a live example of the first half**: its installed `FeatureModules.json` is
  `schemaVersion 10`.
- **Negative control:** on this host the write demonstrably *succeeds* — the file's mtime
  (2026-09-08) differs from the packaged `2026-01-01T05:00:00Z`. The finding concerns the
  unreached error path.
- **Coverage gap:** `FeatureModuleSettingsTests.cs` covers defaults/legacy (`:11`), schemas 0-10
  value preservation (`:129`), an 8192-combination round trip (`:164`), malformed quarantine
  (`:210`), future-schema rejection (`:239`) and per-module migration isolation (`:396`, `:639`).
  **No test makes the migration write fail.**
- **Repair direction (not implemented):** move the migration `Save` outside the parse `try`, or
  give it its own `try/catch` that logs a *write* failure and still returns the parsed `state`.
  A failure to persist a migration should never change the configuration for that session.
- **Regression idea:** a domain test loading a valid `schemaVersion: 10` file whose destination is
  unwritable, asserting `Recovered == false`, `Source` starting `migrated-schema-`, and the file's
  own values rather than `Defaults`. A focused offline fixture should **allow the read and fail
  the write specifically**.
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

## GS-AUD-0015 — Persisted Powerful Change restoration requires a resolvable live caster

> **RECLASSIFIED 2026-11-11 (EV-000017).** The original entry asserted that an ally reloaded in
> a scene without its caster "silently shows +4 instead of +6". **That consequence is
> UNVERIFIED** and is no longer asserted. The code observation stands; the player-visible result
> does not, because I never established that `MaybeCaster` is actually unresolved in that
> ordinary cross-area case. Severity S2 → **UNASSESSED**.

- **Kind:** CONTRACT-QUESTION / conditional risk (was PRODUCT-DEFECT) · **Severity:** UNASSESSED
- **Evidence:** source-demonstrated for the guard; **the triggering condition is not established**
- **Disposition:** OPEN, consequence UNVERIFIED

### What IS established (source, spot-verified)

`BrownFur/BrownFurModifierAdjustmentRuntime.cs:243` returns early when
`sourceContext.MaybeCaster` is null, and `:254` keys the persisted record's `CasterId` on that
**live** caster. There is no fallback to a serialized caster reference.

### The explicit PRECONDITION, which is NOT established

The risk only materialises **if `MaybeCaster` is actually unresolved at the point restoration
runs.** I have not shown that an ordinary companion in another area produces that state. Pathfinder
party members generally remain resolvable entities across areas, so the ordinary cross-area case
may well resolve fine.

The supporting reference I originally cited —
`RuntimeTesting/RuntimeTestRunner.MagicCircleLifetime.cs:114-118` reading
`MechanicsContext.m_CasterReference` reflectively — concerns a **permanently destroyed source**
in its own scenario. That does not establish ordinary companion resolution across areas.

### Kept separate from the .157 gap

These are two different questions and the original entry conflated them:

- The **0.0.157 startup** issue concerned *registration of a previously missing blueprint* before
  a saved owner is loaded.
- This entry concerns *restoring a modifier when its caster cannot be resolved*.

### Smallest check that would settle it

A disposable native case that reaches the restoration path with the caster absent, logging
whether `MaybeCaster` is null there. Until then this is a guard worth a defensive fallback, not a
demonstrated defect.

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

## GS-AUD-0019 — Brown-Fur historical records need their scopes stated, not treated as contradictory

> **SOFTENED 2026-11-11 (EV-000017).** The original entry called the two records a contradiction.
> They are not inherently contradictory: a PASS on an **older candidate** and a NOT_RUN for a
> **later, narrower** case can both be true.

- **Kind:** DOCUMENTATION-DRIFT · **Severity:** S3 · **Evidence:** source-demonstrated
- **Observed:** `planning/BROWN-FUR-RUNTIME-MATRIX.md:43` records save/reload **PASS**;
  `docs/BROWN-FUR-STARTUP-ORDER-REPAIR.md:235` records cold reconstruction **NOT_RUN**. The
  matrix also cites **19** registered identities against today's **25**.
- **Correct reading:** the matrix's PASS is valid **for its own candidate and its own scope**.
  The later record introduces a narrower question — genuine cold reconstruction of a
  *previously missing* saved fact owner — that the earlier run did not and could not cover.
  Neither record is wrong; what is missing is an explicit scope note.
- **What should change:** annotate the matrix with the candidate it covers and what it does not
  establish for later candidates, and reconcile the 19 vs 25 identity count. Do **not** mark the
  earlier result invalid.
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

## GS-AUD-0025 — Untemplated alternate-caller Summon Monster children keep the donor's inherited template

> **SCOPE NARROWED 2026-11-11 (EV-000017).** The original entry said this "affects all seven
> alternate caller families". It does not: **Naturalist, Feral Hunter and Preservationist are
> Nature's Ally routes**, and the inherited celestial/fiendish template is a Summon Monster
> concern. Scope corrected to the Summon Monster callers.

- **Kind:** PRODUCT-DEFECT · **Severity:** S2 · **Evidence:** source-demonstrated,
  coordinator-verified · **Disposition:** OPEN
- **Observed:** `ContextActionRemoveInheritedSummonTemplates` has exactly **one** production
  construction site — `Blueprints/ExpandedSummoningAbilityBuilder.cs:202`, in the canonical
  ordinary-spell builder. The alternate-caller adapter's **untemplated** branch
  (`Blueprints/AlternateSummoningAdapters.cs:648-663`) calls `ReplaceSpawnUnits` and optionally
  `AppendAlignmentAction` but **never appends the removal action**, while the *templated* branch
  twelve lines above (`:630-647`) does call `AppendDynamicTemplate`. Coordinator check:
  `grep -rn ContextActionRemoveInheritedSummonTemplates src/ --include=*.cs` outside
  `RuntimeTesting/` returns only the class and that single site.
- **Failure scenario, with its precondition:** *if* a Summon Monster alternate caller summons an
  untemplated creature **whose native carrier actually carries an inherited celestial/fiendish
  template buff**, that summon keeps a template the ordinary spell would have stripped.
  The reproduction needs a **specific affected SM caller, carrier, tier and creature** — which
  this audit has not enumerated.
- **Why it reads as an oversight rather than a design choice:** the asymmetry sits inside one
  method and the sibling branch handles templates.
- **Smallest confirming check:** summon one untemplated creature through a Summon Monster
  alternate caller and inspect the spawned unit's fact list for the donor template buff.
- **Related:** GS-AUD-0026 explains why no automated check catches it.

## GS-AUD-0026 — The composed-action contract classifies the template-removal action as `Other` and never compares it

- **Kind:** HARNESS-DEFECT · **Severity:** S3 · **Evidence:** source-demonstrated
- **Observed:** the composed-action comparison that would have caught GS-AUD-0025 classifies
  `ContextActionRemoveInheritedSummonTemplates` as `Other` and excludes it from comparison.
- **Why it matters:** the canonical and adapter paths are supposed to be contract-compared; the
  one action whose absence changes gameplay is the one the comparison ignores.
- **Repair direction:** classify it explicitly and compare it. Would fail today on GS-AUD-0025.

## GS-AUD-0027 — ~~The Races Unleashed version pin fails open~~ **WITHDRAWN — FALSE POSITIVE**

> ### ❌ WITHDRAWN 2026-11-11 (EV-000017) — THIS FINDING WAS WRONG
>
> The integration is gated **structurally on blueprint shape**, not on a version string. An
> unrecognised future RU is refused before mutation and its generated rows are withheld.

- **Kind:** ~~PRODUCT-DEFECT~~ → **FALSE-POSITIVE (withdrawn)** · **Severity:** ~~S2~~ → **none**
- **Disposition:** WITHDRAWN.

### What the finding claimed

That because the Spider Summoner integration carries no RU version/binary pin, a future Races
Unleashed with changed spider blueprints "would be integrated against anyway, silently".

### Why it is wrong — `Blueprints/SpiderSummonerIntegration.cs`

The production path refuses unsupported input **before any mutation**, and restores the original
on a failed repair:

- `:143-144` reads the loaded original row and **classifies** it.
- `:146-147` `LegacyInputProblem(reading)` returns `"unsupported-input:" + input` and
  **returns false before touching anything**.
- `:169-176` re-classifies the repaired result; if rejected it **restores the original
  components** (`row.ComponentsArray = components`) and returns false.
- `:102-107` — the caller, on that false result, records the problem and calls `Placeholder(...)`
  for every placement, so **generated rows are withheld rather than published**.
- The comment at `:162-165` states explicitly that the input check already refuses a row carrying
  any other variants component.

That is the accepted **RU-18** contract: unknown contracts remain unsupported, generated rows are
withheld, unrelated behaviour is preserved.

### The reasoning error

Changing RU's `Info.json` while leaving its blueprints unchanged would only show that the
integration does not require a particular *version string*. It would not show acceptance of an
unsupported blueprint *structure*. The finding also argued from "other integrations gate on exact
binaries, therefore this one is defective" — exactly the inference this audit's own method rules
forbid: *the absence of a check another component happens to have does not establish a defect
here.*

### What could still be proposed (not a defect)

An **additional** version or MVID restriction, matching `FAVORED-CLASS-COMPATIBILITY.md` and
`BETTER-VENDORS-COMPATIBILITY.md`, is a legitimate *design* proposal for defence in depth. It is
the owner's call, and nothing in the current behaviour requires it.

### Process note

This finding was imported from worker W3 at W3's own calibration **without independent
verification** — EV-000013 records that limit explicitly. The review found it anyway. Worker
findings that were not re-derived by the coordinator should be read as having exactly that
weight.

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

## GS-AUD-0030 — ~~Nimble is granted one level early~~ **WITHDRAWN — FALSE POSITIVE**

> ### ❌ WITHDRAWN 2026-11-11 (EV-000017) — THIS FINDING WAS WRONG
>
> **There is no Nimble defect and no owner decision here. Acting on the original finding would
> have introduced a rules defect into correct code.**

- **Kind:** ~~CONTRACT-QUESTION + DOCUMENTATION-DRIFT~~ → **FALSE-POSITIVE (withdrawn)**
- **Severity:** ~~S2~~ → **none**
- **Disposition:** WITHDRAWN. Retained here, not deleted, because the error is part of the record.

### What the finding claimed

That the Pathfinder Gunslinger gains Nimble at levels **3/7/11/15/19**, that the mod's
**2/6/10/14/18** was therefore one level early at all five tiers, and that
`planning/GUNSLINGER-FIDELITY-MATRIX.md:66` was wrong to classify that placement `EXACT`.

### Why it is wrong

The authoritative rules text was in the lab the whole time, at
`C:\Dev\KingmakerGunslingerLab\private\rules\GUNSLINGER_PFSRD.md`. Its class table reads:

| Level | 2nd | 6th | 10th | 14th | 18th |
|---|---|---|---|---|---|
| | Nimble +1 | Nimble +2 | Nimble +3 | Nimble +4 | Nimble +5 |

and its **Nimble (Ex)** entry reads, verbatim:

> "Starting at **2nd level**, a gunslinger gains a +1 dodge bonus to AC while wearing light or no
> armor. Anything that causes the gunslinger to lose her Dexterity bonus to AC also causes the
> gunslinger to lose this dodge bonus. This bonus increases by +1 **for every four levels beyond
> 2nd level** (to a **maximum of +5 at 18th level**)."

The implementation matches that rule **exactly**, including both qualifying clauses —
`Classes/NimbleService.cs:14-18`:

```csharp
if (!retainsDexterityBonus ||
    (armor != NimbleArmor.None && armor != NimbleArmor.Light) ||
    gunslingerLevel < 2)
    return 0;
return Math.Min(5, 1 + ((gunslingerLevel - 2) / 4));
```

`Math.Min(5, …)` reproduces "maximum of +5 at 18th level"; `retainsDexterityBonus` reproduces
the lose-Dex clause; the armor test reproduces "light or no armor". The progression wiring at
`Blueprints/GunslingerClassBlueprints.cs:486-489` grants at the same levels.
**The code is correct and the `EXACT` label is correct.**

### How the error was made — the part worth keeping

Worker W4 reported two things: the code's levels (checkable in the repository) and the tabletop
table (not in the repository). I verified the first, did not verify the second, and then recorded
in EV-000013 that I had "VERIFIED INDEPENDENTLY" — language that covered both. I also reinforced
the false conclusion with a plausible-sounding argument ("every other row in the matrix sits on
its correct tabletop level, so this one is anomalous"), which made a wrong finding look stronger.

**The rule I broke is the one I gave my own workers:** *a search hit is navigation, not
inspection* — and an external fact asserted by a subagent is navigation too. One `grep -i nimble`
over `private/rules/` would have caught it.

### Consequence if it had been actioned

This was ranked **#1** in `SUMMARY.md` and in the PR body. Moving Nimble to 3/7/11/15/19, or
relabelling a correct `EXACT` row, would have broken conformance with the tabletop rule.

## GS-AUD-0031 — `BETTER-VENDORS-COMPATIBILITY.md` misdescribes the acquisition path for the reused "+1" items

> **CORRECTED AND SHARPENED 2026-11-11 (EV-000017, EV-000018).** The original entry concluded the
> four melee "+1" items are **unavailable**. **That conclusion was wrong** — absence of fixed
> *capital* rows does not establish unavailability. Dedicated re-inspection (worker W6) found the
> actual acquisition path, and the documentation defect is real but different from what I claimed.

- **Kind:** DOCUMENTATION-DRIFT · **Severity:** S2 · **Evidence:** source-demonstrated
- **Disposition:** OPEN — a conflict for the owner to resolve, not resolved here.

### The actual acquisition path (this is what the docs should say)

- One-copy **fixed rows at Dire Narlmarches, Pitax and both BTSL "Honest Guy" tables** while the
  module is on (`PlusOne` appears in both `AllGenericKinds()` and `AllFoundationKinds()`).
- **Plus** 5-copy batches at the capital blacksmith via **Better Vendors Military rank I** when
  that optional integration is active.

The items are **not** unavailable.

### The documentation defect

`docs/BETTER-VENDORS-COMPATIBILITY.md:319-324` claims KMG publishes fixed `SmithVendorTable`
rows for all seven reused "+1" items. The source says otherwise: the capital `SmithVendorTable`
specs for Eastern and Spear are literally **empty arrays**, labelled *"Model D capital retired
generic stock"* (`Blueprints/EasternWeaponCampaignBlueprints.cs:21-24`,
`Blueprints/ElvenBranchedSpearCampaignBlueprints.cs:23-25`), and the capital publication actively
**strips** such rows. `planning/PROJECT-MAGIC-ITEM-ACQUISITION-INVENTORY.md:159-161` agrees with
the code, not with the compatibility doc. The doc's *limitation 3* is consequently wrong for four
of the seven items.

### Why CI did not catch it

The domain case that supposedly pins this models only the Pistol and **never asserts a
destination table** — code-and-test agreement establishes nothing here.

### Remaining native gap, unchanged

Real merchant behaviour still needs an authorized kingdom-stage fixture that does not exist
(COV-051, BLOCKED).

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

## GS-AUD-0034 — Both KMG `ExtractSelectionItems` postfixes narrow to `FeatureUIData`, discarding other implementations

> **NARROWED 2026-11-11 (EV-000017).** The original entry implied a realised compatibility
> failure. **No actually affected installed mod has been identified.** This is a conditional risk.

- **Kind:** CONTRACT-QUESTION / conditional compatibility risk (was PRODUCT-DEFECT)
- **Severity:** S3 · **Evidence:** source-demonstrated · **Disposition:** OPEN
- **Observed:** both KMG postfixes on `BlueprintParametrizedFeature.ExtractSelectionItems` accept
  `IEnumerable<IFeatureSelectionItem>` and narrow it with `.OfType<FeatureUIData>()`, assigning
  the narrowed result back. Any selection item supplied by another mod that implements the
  interface **without** being a `FeatureUIData` would be silently dropped.
- **PRECONDITION, not established:** that some installed mod actually supplies such an item. The
  eleven mods on this host were not inspected for it.
- **Why it is still worth recording for a Nexus release:** the failure mode is silent and
  presents to a user as "KMG broke mod X". The defensive fix (preserve non-`FeatureUIData`
  entries rather than narrowing) is cheap.
- **Smallest confirming check:** enumerate `IFeatureSelectionItem` implementations across the
  installed mod set.

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

## GS-AUD-0038 — The same assertion failed once and passed once, on identical inputs — cause unresolved

> ### ⚠ MECHANISM WITHDRAWN 2026-11-11 (EV-000017)
>
> The original explanation — "fails only when another scenario runs first **in the same game
> process**" — is **factually wrong** and is withdrawn, along with the broader conclusion it
> supported about batched native results. The *observation* stands; the *cause does not*.

- **Kind:** OBSERVED-FAILURE-UNATTRIBUTED (was HARNESS-DEFECT) · **Severity:** S3
- **Evidence:** native-observed, non-reproducing · **Disposition:** OPEN, cause unresolved

### What is actually established

| Run | Position | Result |
|---|---|---|
| `20261111T1816557789702Z-disposable-pistolero-deeds` | 2nd scenario of transaction `compat-20261111T181418Z-9d1b82c04345` | **FAIL** — `up-close-critical` FAIL, other 9 PASS |
| `20261111T1821405012055Z-disposable-pistolero-deeds` | only scenario of transaction `compat-20261111T182024Z-636f7b658091` | **PASS** — all 10 |

Same package, same DLL, same profile, same parameters, ~5 minutes apart.

### Why the original explanation was wrong

My own retained log contradicts it. `evidence/A15/T3-run.log` shows **two separate launch
sequences** inside the single profile transaction:

```
L25  Stage: steam-launch-requested      L26  Stage: kingmaker-process-discovered   -> targeting-arms  PASS
L38  Stage: steam-launch-requested      L39  Stage: kingmaker-process-discovered   -> pistolero-deeds FAIL
```

and the wrapper sets `ExitAfterCompletion = $true`
(`scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1:512`) and asserts the game is
not running between scenarios (`Assert-KmgNotRunning`, `:685`). **The two scenarios ran in
different game processes.** A shared *PowerShell/profile transaction* is not a shared *game
process*, and I conflated them.

My supporting citation was also a stretch: `KNOWN-ISSUES.md:126-129` describes order-sensitive
forced-roll fixtures inside the **broad aggregate** scenario, not two separate scenarios in
separate processes.

### What remains, stated without mechanism

The only difference I can name between the two runs is that the failing one was the **second
game launch within one profile transaction** and the passing one was the **first launch of its
own transaction**. Whether that is causal is **not established**.

The assertion itself conjoins two conditions —
`criticalUnmultiplied = hp - target.HPLeft == hitDamage && !attacker.Descriptor.HasFact(pistolero.UpCloseArmed);`
(`RuntimeTesting/RuntimeTestRunner.cs:33966-33967`) — and the retained record does not say which
half failed, so even the failure's shape is unresolved. The stage forces a die roll via
`UnityEngine.Random.InitState(FindNativeD6Seed(6))` (`:33962`), which is one candidate for
nondeterminism, but that is a hypothesis and is labelled as one.

### Competing explanations, none excluded

1. Nondeterminism in the forced-roll seeding or the surrounding RNG consumption.
2. Residual state in the live installation across launches within one transaction.
3. A genuine ordering-dependent defect in the Up Close and Deadly critical path.

### Withdrawn claim

The statement that *"batched multi-scenario native results from this harness are weaker evidence
than the same scenarios run one per process"* rested entirely on the false shared-process premise
and is **withdrawn**. Prior batched PASS records are not impeached by this campaign.

### Smallest check that would settle it

Re-run `disposable-pistolero-deeds` as the second scenario of a two-scenario transaction several
times. If it fails only in that position, position matters; if it fails sporadically in any
position, it is nondeterminism. Separately, split the conjoined assertion so the record says
which half failed.

---

# Findings 0039–0043 — from the investigations the PR #132 review identified as unfinished (EV-000018)

These five come from source work that the first pass left undone and wrongly justified as
blocked. **GS-AUD-0039 is the most significant product finding of the entire campaign, and it
only exists because the review refused the early-exit claim.**

## GS-AUD-0039 — Scatter Shot's documented −2 attack penalty is applied as a **+2 bonus**

- **Kind:** PRODUCT-DEFECT · **Severity:** **S2** (worker W5 proposed S1; see rationale)
- **Evidence:** source-demonstrated **and confirmed by direct engine IL measurement**
- **Confidence:** high on the sign inversion; the exact final number is subject to the caveat below
- **Disposition:** OPEN

### Expected rule and authority

ADR-0037:40-41 and `SPRINT-32-ENTRY-CRITERIA.md:13,27` specify a **−2 attack penalty** on scatter
attacks, and the shipped blueprint text says "attack at **-2**".

### The engine semantics, measured — not inferred

`ildasm` over the qualified reference assembly (`Assembly-CSharp.dll`, SHA-256 `3B6450FF…F5AFB`,
MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`), method
`Kingmaker.RuleSystem.Rules.RuleCalculateAttackBonus::OnTrigger` — the **real calculation**, not a
UI breakdown:

```
IL_019a:  ldfld     ModifiableValue CharacterStats::BaseAttackBonus
IL_019f:  call      int32 ModifiableValue::op_Implicit(...)
IL_01a4:  ldarg.0
IL_01a5:  call      instance int32 RuleCalculateAttackBonus::get_AttackBonusPenalty()
IL_01aa:  sub
```

`AttackBonusPenalty` is **subtracted**. A **positive** value is a penalty; a **negative** value is
a **bonus**. The same `sub` appears in `RuleCalculateAttackBonusWithoutTarget`.

### The defect

`Scatter/ScatterAttackVolleyDecision.cs:8` declares `internal const int AttackPenalty = -2;` and
`Scatter/ScatterShotRuntime.cs:159-161` passes it straight into the `RuleAttackWithWeapon`
constructor's penalty argument. Under the measured semantics that is **BAB − (−2) = BAB + 2** —
a **+2 bonus** where a −2 penalty was intended, a **4-point swing in the player's favour** on
every scatter attack.

### The repository's own convention agrees — six independent production call sites

| Site | Intent | Written as |
|---|---|---|
| `Feats/FirearmWeaponFocusAttackBonus.cs:25` | **+1 bonus** | `AttackBonusPenalty - 1` |
| `Feats/FirearmWeaponFeatBonus.cs:24` | bonus | `AttackBonusPenalty - decision.AttackBonus` |
| `Summoning/ExpandedSummoningSmiteComponent.cs:24` | bonus | `AttackBonusPenalty - bonus` |
| `EasternWeapons/EasternWeaponProficiencyRuntime.cs:89` | **−4 penalty** | `AttackBonusPenalty + 4` |
| `ElvenBranchedSpear/MovementOpportunityAccuracyComponent.cs:99` | **−4 penalty** | `AttackBonusPenalty + 4` |
| `Summoning/GiantHumanoidAttackComponents.cs:400` | penalty | `AttackBonusPenalty + 5` |

And the project states the rule explicitly in its own words at
`RuntimeTesting/AidAnotherCompatibilityObserver.cs:477-481`:
*"`RuleAttackWithWeapon.AttackBonusPenalty` is a positive subtraction in Kingmaker 2.1.7b"* —
where it passes **1000** through the identical constructor to **force a miss**.
**Scatter is the only production site that passes a negative.**

### Why the existing coverage never caught it

- The domain test asserts only that the constant **equals −2** — the exact code-and-test-agree
  trap this audit's method rules warn about.
- The native scenario `disposable-gunslinger-scatter-shot` **was run on 0.0.182 in this campaign
  and PASSED** (run `20261111T...-disposable-gunslinger-scatter-shot`). Its four assertions cover
  the command contract, the combat-log line, the Thundering fallback and external isolation —
  **none asserts the attack penalty**. Its PASS is therefore fully consistent with the inversion
  being present and unnoticed.

### The clamping question — TRACED AND CLOSED (EV-000019)

The first revision left open whether something clamps negatives. The full chain is now
disassembled and there is **no clamp anywhere on the penalty path**:

1. `RuleAttackWithWeapon..ctor` stores the 4th argument **verbatim** —
   `IL_0010 ldarg.s attackBonusPenalty` / `IL_0012 stfld AttackBonusPenalty`.
2. It forwards that field **verbatim** into `RuleAttackRoll..ctor` —
   `IL_0020 ldfld AttackBonusPenalty` / `IL_0025 newobj RuleAttackRoll::.ctor(...)`.
3. `RuleAttackRoll::set_AttackBonusPenalty` is a plain auto-property setter —
   `IL_0001 ldarg.1` / `IL_0002 stfld '<AttackBonusPenalty>k__BackingField'`.
4. `RuleCalculateAttackBonus::OnTrigger` does a plain `sub`.

The only `Math::Min` in `RuleAttackRoll` is in `IncreaseMissChance`, clamping **miss chance** to
75 — unrelated to the attack bonus. So passing `-2` does yield **+2**, and the "4-point swing"
figure is supported rather than inferred.

**Severity remains S2** (not W5's proposed S1): it is wrong gameplay behaviour confined to one
deed, with no crash, data loss or progression impact. The magnitude is now established; the
severity rubric is what caps it.

### Smallest confirming check

One disposable scenario asserting `Attacks[0].AttackRoll.AttackBonusPenalty == 2` (positive two)
after a scatter volley.

### Suggested repair direction (not implemented)

Change the constant to `+2`, matching every other penalty site, and add the assertion above so the
sign is pinned by behaviour rather than by a constant's literal value.

## GS-AUD-0040 — Shield Other's shipped ability description contradicts the approved range contract

> ### ⚠ RECLASSIFIED 2026-11-12 (EV-000019)
>
> The original entry presented this as a **three-way conflict requiring an owner decision**
> between enforcing a post-cast range limit and correcting the description. **That was wrong:
> the owner decision already exists in the audited candidate and I had not found it.** This is
> documentation drift only, and "reintroduce the distance check" has been **removed** as a repair
> option — doing so would undo a deliberately approved repair.

- **Kind:** DOCUMENTATION-DRIFT (was CONTRACT-QUESTION) · **Severity:** S3 (was S2)
- **Evidence:** source-demonstrated · **Disposition:** OPEN

### The approved contract

`planning/EXPANDED-SUMMONING-MISSION.md:71`, owner-authorized, verbatim:

> "Before summoning work, inspect `ShieldOtherLinkValidityPolicy` and its tests.
> **An established bond must not depend on distance; close range remains on the casting ability.**
> Preserve expiration, removal/dispel, dead/missing endpoints, and area separation."

`EXPANDED-SUMMONING-JOURNAL.md:77` records the *pre-repair* state as the defect:
> "Mandatory inspection proved `ShieldOtherLinkValidityPolicy` **still removes** an established
> bond outside close range. Separate prerequisite repair is next."

followed by a "Shield Other prerequisite qualified" entry. So distance-independence is the
**deliberate outcome of an approved repair**, not an oversight.

### Therefore

| Element | Status |
|---|---|
| `ShieldOtherLinkValidityPolicy.cs:13-19` computes distance/bound and never compares them | **CORRECT** — matches the approved contract |
| `RuntimeTestRunner.cs:25080-25101` ("post-cast-range-preservation") asserts the split still happens at ~65 ft with a 35 ft bound | **CORRECT** — it is a regression guard protecting the approved behaviour |
| The shipped ability description saying the subject must remain "within close range" | **STALE** — the only thing that is wrong |

The unread fields `DistanceFeet`/`MaximumRangeFeet` are a residue of the pre-repair implementation
(same inert-property pattern as GS-AUD-0018/0042), not evidence of a missing check.

### Repair direction

Correct the ability description so it states that **close range applies at casting** and the
established bond then persists regardless of distance. **Do not** reintroduce the distance check
and **do not** change the regression test.

### Process note — the same error as Nimble

I found an apparent conflict among code, test and description and escalated it as an owner
decision, when the contract was already in the repository. Both failures share one cause:
**incomplete contract discovery before asking the owner to choose.** For the rest of this
register, any "owner decision" is only raised after `planning/*MISSION*.md`, `docs/`, the
journals and `KNOWN-ISSUES.md` have been searched for an existing instruction.
## GS-AUD-0041 — The Protection-from-Alignment control-immunity catalog has 23 entries; its authority document lists 21

- **Kind:** DOCUMENTATION-DRIFT (+ one embedded contract question) · **Severity:** S3
- **Evidence:** source-demonstrated · **Disposition:** OPEN
- **Observed:** the shipped catalog carries **23** entries against the authority table's **21**.
  The two extras are `FeyCelestial.SuggestionBuff` and `FeyCelestial.CharmBuff`.
- **Embedded contract question:** the Suggestion entry's own stated reason — *"imposes the
  provider's fixed avoid-combat instruction"* — reads as the **one-command compulsion** that the
  authority document's exclusion list explicitly rejects. Either the entry or the exclusion list
  is wrong.
- **Repair direction:** reconcile the table with the catalog, and decide whether Suggestion
  belongs under the control-immunity rule at all.

## GS-AUD-0042 — ADR-0037 requires Vital Strike-style increases excluded from scatter; only precision damage is suppressed

- **Kind:** DOCUMENTATION-DRIFT / unimplemented stated rule · **Severity:** S3
- **Evidence:** source-demonstrated; **gameplay consequence UNVERIFIED**
- **Observed:** ADR-0037 requires Vital Strike-style damage increases to be excluded from scatter
  attacks. Precision damage **is** suppressed; Vital Strike is **not**.
  `AllowsVitalStrikeDamage` is an inert property referenced only by a test.
- **Precondition for any player impact:** a gunslinger must actually have Vital Strike and use it
  with a scatter attack. Whether that combination is reachable in this installation was **not
  established**, so the consequence is marked UNVERIFIED rather than asserted.
- **Related:** this is the same inert-property/test-only-reference pattern as GS-AUD-0018.

## GS-AUD-0043 — `MagicCircleRadiusVisual.Attach` reuses an existing decal without matching `AreaId`/`Alignment`

- **Kind:** PRODUCT-DEFECT (presentation) · **Severity:** S3 · **Confidence:** low
- **Evidence:** source-demonstrated; **gated on an unverified engine behaviour**
- **Observed:** `Attach` reuses an existing decal without checking that its `AreaId` and
  `Alignment` match — the fields are written for exactly that purpose but read only by test
  oracles.
- **PRECONDITION, not established:** that Kingmaker actually recycles area views in a way that
  presents a stale decal. If it does not, this is inert.
- **Smallest check:** create two Magic Circles of different alignments in sequence in one scene
  and observe the rendered decal colour/identity.

---

# Findings 0044–0047 — from the A20 scenarios I had wrongly implied were human-only (EV-000019)

The rev-2 review's point 4 noted the disposition said complete while eligible work remained.
Re-checking the wrapper's own `ValidateSet` showed **three machine-observable
presentation/audio scenarios were admissible all along**. Running them produced three native
FAILs. Each is classified below on the evidence, **not** assumed to be a product defect —
two are stale assertion pins and one is a credible defect.

## GS-AUD-0044 — Dead Shot appears to commit far more audio events than its contract allows

- **Kind:** OBSERVED-FAILURE (candidate PRODUCT-DEFECT) · **Severity:** S2
- **Evidence:** **native-reproduced once** on the frozen 0.0.182 candidate · **Disposition:** OPEN
- **Expected rule and authority:** the scenario's own contract at
  `RuntimeTesting/RuntimeTestRunner.cs:8780` requires
  `deadAfter == deadBefore + 1 && startlingAfter == startlingBefore + 1 &&
  menacingAfter == menacingBefore + 2 && bleedingAfter == bleedingBefore + 2`
  — i.e. **Dead Shot must commit exactly one** source-specific audio discharge.
  Mission card A20 independently requires "no duplicate sound event/state after repeated use".
- **Observed:** `disposable-firearm-wwise-audio` **FAIL** on `deed-audio-commit-boundaries`.
  Route diagnostics: `dead=PASS:11;scatter=PASS:1;startling=PASS:1;menacing=PASS:2;stopBleeding=PASS:2`.
  Startling (+1), Menacing (+2) and Stop Bleeding (+2) all match their required deltas.
  **Only Dead Shot is anomalous, at 11.**
- **Reading of the evidence, stated as a reading:** the `:N` suffix is the committed-discharge
  counter the assertion compares, so Dead Shot committed ~11 where 1 is required. I am inferring
  that from the diagnostic format rather than from a labelled delta field, so the exact number
  carries that caveat. What is **not** in doubt is that the assertion failed while the other
  three deeds matched, which localises the failure to Dead Shot.
- **Why 11 is anomalous on any reading:** Dead Shot is a multi-attack deed, so a number above 1
  is conceivable — but it is not 2 or 3 either, and the contract says 1.
- **Competing explanations:** (a) duplicate audio commits per constituent attack — the A20
  concern; (b) a stale expectation that never accounted for Dead Shot's multi-attack structure.
  **Neither is excluded.** Note that GS-AUD-0014's A06 work found Dead Shot's *delivery* correctly
  spends once despite duplicate callbacks, which makes (a) specifically interesting: the
  mechanical path dedupes and the audio path may not.
- **Smallest confirming check:** re-run with the counter logged as an explicit before/after pair,
  and listen for repeated reports on one Dead Shot.

## GS-AUD-0045 — The firearm visual-rig assertion is hard-pinned to 14 items; the roster is now 16

- **Kind:** HARNESS-DEFECT (stale assertion pin) · **Severity:** S3
- **Evidence:** native-observed + source-demonstrated · **Disposition:** OPEN
- **Observed:** `disposable-firearm-visual-rigs` **FAIL 2 of 66**. The first failure,
  `firearm-all-14-item-visual-identities`, is computed at
  `RuntimeTesting/RuntimeTestRunner.cs:13928` as:
  ```csharp
  bool exactItems = items.Count == 14 && items.All(value =>
      FirearmWeaponPresentation.HasExactItemVariant(value.Item3, value.Item1, value.Item2));
  ```
  The count is **hard-pinned to 14**; the observed roster is **16**. The firearm set has grown
  (`RoadwardenItem`, `DeadReckoningItem`, `WatchAtTheWorldsEndItem` and others now appear).
- **This is NOT a presentation defect.** The assertion can fail on the count alone, and every
  observed item does resolve to a named variant (`Pistol.Duelist`, `Pistol.LastWord`) or a
  `<Kind>.Service` variant that `WeaponVisualVariantCatalog` explicitly maps. `issues` were not
  reported for any item.
- **Explicitly NOT a finding, having been checked:** `KMG.Test.TestMusketItem` appears in the
  observed list. It is a **development-only** item (`Blueprints/TestMusketBlueprints.cs`,
  consumed by `Development/DevelopmentControls.cs:313,395`) and is in
  `Assets/WeaponVisualVariantCatalog.cs:76` by design to satisfy this very rig check. It is
  **not** a player-facing leak, and I am recording that so nobody "fixes" it.
- **Repair direction:** derive the expected count from the catalogue instead of pinning 14, or
  update the pin deliberately. Per this project's convention, a count pin should be moved
  **knowingly**, not to make a run green.

## GS-AUD-0046 — The player-facing presentation assertion's counts are stale; the text itself is clean

- **Kind:** HARNESS-DEFECT (stale assertion pin) · **Severity:** S3
- **Evidence:** native-observed · **Disposition:** OPEN
- **Observed:** `observe-gunslinger-presentation` **FAIL 1 of 9** on
  `project-item-player-facing-presentation`.
  Expected: "55 weapons and the Cord have complete clean player text; eleven visible custom
  enchantments are clean and the policy-only enchantment is blank."
  Observed: `weapons=57;items=58;customEnchantments=12;issues=<none>`.
- **The decisive detail is `issues=<none>`:** the observer found **no text-quality problems at
  all**. The failure is purely that content has grown past the pinned counts (57 vs 55 weapons,
  12 vs 11 enchantments).
- **Scope limit:** this is a *structured text* check. It does **not** establish rendered
  appearance, icon quality or layout — those remain human-only gaps regardless.
- **Repair direction:** same as GS-AUD-0045 — derive the counts or move the pins knowingly.

## GS-AUD-0047 — `pistol-authored-marker-contract` fails and the cause is unresolved

- **Kind:** OBSERVED-FAILURE-UNATTRIBUTED · **Severity:** UNASSESSED
- **Evidence:** native-observed once · **Disposition:** OPEN, cause unresolved
- **Observed:** the second failure in `disposable-firearm-visual-rigs`:
  `pistol-authored-marker-contract`, expected "Duelist and LastWord retain exactly four authored
  markers, +Z muzzle, and no support IK", observed **`exact=False`**.
- Unlike GS-AUD-0045 this is a **structural** assertion, not a count pin, so a stale-expectation
  explanation does not obviously apply. The retained diagnostic reports only the boolean, so
  **which** of the three conditions failed (marker count, muzzle axis, or support IK) is not
  recoverable from the evidence.
- **Why it is UNASSESSED rather than S2/S3:** I have not established whether this is cosmetic
  (a muzzle-flash origin) or functionally visible, and the sibling assertion
  `pistol-three-variant-separation` **passed**, so the three prefab references are distinct.
- **Smallest confirming check:** re-run with the three sub-conditions reported separately, then
  inspect the two named pistols' authored markers against the contract.
