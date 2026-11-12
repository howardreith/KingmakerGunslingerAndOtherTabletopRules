# Gunslinger pre-release audit — final summary

**Campaign** KMG-AUD-20261111T170737Z · **host** HOWARD-LAPTOP (verified OLD LAPTOP)
**Started** 2026-11-11T17:07:37Z · **revision 4** after three owner reviews · **elapsed ≈ 7 h 20 min**
of the 48-hour allowance. Deadline 2026-11-13T17:07:37Z, never extended or restarted.

**Disposition: `IN_PROGRESS_CHECKPOINT`.** This is **not** a complete audit and **not** a release
approval. Revision 2 claimed `COMPLETE_WITH_GAPS`; that was wrong, because eligible work remained
unattempted. Every coverage row has a disposition, but **assigning `NOT_RUN` to work that was
never scheduled does not make the audit complete**. The explicit remaining queue is §6a.

> **How to read "48 findings".** It is 48 numbered *records*, not 48 demonstrated product
> defects. It mixes confirmed observations, **2 withdrawn false positives**, several
> reclassified entries, contract questions, conditional risks whose preconditions are
> unestablished, harness defects and verification gaps. **Active S2 count: 6.** Do not use the
> record count as a defect count, or the ranked list as a repair backlog, without reading each
> entry's evidence strength.

> ### Revision 4 — what the third review changed
>
> - **`GS-AUD-0044` (Dead Shot audio) — RECLASSIFIED S2 → S3 harness.** **Eleven notifications
>   are correct.** The counter brackets the *entire* nested scenario, which runs eleven
>   discharge-producing volleys, and production posts one per non-misfire volley. The assertion's
>   `+1` is stale. The counter also measures **attempted** notifications (incremented before the
>   emitter check and before `TryPost`), so it could not have shown audio duplication anyway.
>   Its "second-highest repair priority" ranking is withdrawn.
> - **`GS-AUD-0045` — over-exclusion retracted.** I wrote that every item resolves correctly.
>   `items.Count == 14 &&` **short-circuits**, so the binding check never ran, and the diagnostic
>   prints the *catalog's expectation*, not a comparison result. Bindings are **UNVERIFIED**.
> - **`GS-AUD-0046` — count corrected.** The predicate already expects **12** enchantments and 12
>   were observed; only the weapon (55→57) and item (56→58) counts are stale. `custom.Contains(policy)`
>   is skipped after the weapon mismatch, so policy presence stays unverified.
> - **COV-027 attribution fixed.** Revision 3 classified the *expanded-summoning* row using
>   *firearm* results. COV-027 is restored to `NOT_RUN`; firearm evidence moved to new rows
>   **COV-058/COV-059**.
> - **Superseded instructions removed** from the summary and handoff bodies — not just banners.
> - **`GS-AUD-0048` added**, found by the review: Dead Shot's probes carry the same sign error as
>   Scatter. A complete sweep of production `RuleAttackRoll`/`RuleAttackWithWeapon` constructions
>   now bounds it: **exactly two** sites pass a negative penalty, and no third exists.
>
> ### Revision 3 — what the second review changed
>
> - **`GS-AUD-0040` (Shield Other) — RECLASSIFIED.** I presented a three-way conflict and asked
>   for an owner decision. **The decision already existed in the candidate and I had not found
>   it:** `planning/EXPANDED-SUMMONING-MISSION.md:71` approves a distance-independent established
>   bond, and the journal records the current behaviour as a *deliberate repair*. The code and the
>   regression test are correct; only the shipped description is stale. "Reintroduce the distance
>   check" is removed as a repair option. **Same root cause as the Nimble error: incomplete
>   contract discovery before escalating.**
> - **Coverage ledger reconciled.** The rev-2 corrections had not propagated: COV-006 still
>   asserted the withdrawn Nimble defect, COV-020 the withdrawn RU pin, COV-030 the unverified
>   +4 degradation, and COV-004/COV-057 carried stale totals. All fixed.
> - **Magic Circle VERIFIED downgraded.** I had marked COV-044/045 verified on the strength of
>   `observe-magic-circle-native-contracts`, which is a **blueprint inventory dump** that casts,
>   moves, dispels and unloads nothing. Overall `VERIFIED` rows fell 6 → 3.
> - **Scatter trace finished.** The clamping caveat is closed: no clamp exists anywhere on the
>   penalty path, so the +2 / 4-point swing is established rather than inferred.
> - **A20 was not human-only.** Three machine-observable presentation/audio scenarios were
>   admissible all along. Running them produced three native FAILs. Revision 4 reclassified
>   all three as **stale assertions** rather than product defects — but **GS-AUD-0047**, the
>   `pistol-authored-marker-contract` failure inside the visual-rig scenario, remains an
>   **unresolved structural failure**, not a stale pin.
>
> ### Revision 2 — what the first owner review changed
>
> The first revision of this audit **claimed the eligible queue was complete. It was not**, and
> the justification given for stopping was false for at least five source rows and five
> save-free native scenarios. The review was correct and the work was resumed.
>
> **Two findings were withdrawn as false positives**, one of which would have caused real harm:
>
> - **GS-AUD-0030 (Nimble) — WITHDRAWN.** I claimed tabletop Nimble is 3/7/11/15/19 and the mod's
>   2/6/10/14/18 was wrong. **The mod is correct.** The project's own rules copy at
>   `private/rules/GUNSLINGER_PFSRD.md` reads "Starting at **2nd level** … increases by +1 for
>   every four levels beyond 2nd (to a maximum of +5 at 18th)". Acting on my finding would have
>   **introduced** a rules defect. It was ranked #1.
> - **GS-AUD-0027 (RU version pin) — WITHDRAWN.** The integration gates **structurally on
>   blueprint shape**: unsupported input is refused before mutation and generated rows are
>   withheld (`SpiderSummonerIntegration.cs:143-176`, `:102-107`). I mistook "no version string
>   check" for "no check".
> - **GS-AUD-0038's mechanism — WITHDRAWN.** I said the two scenarios shared a game process. My
>   own log shows **two separate launches** (`T3-run.log` L25/L26 and L38/L39). Cause unresolved.
>
> **The resumed work then found the campaign's most important defect, GS-AUD-0039.** It would not
> exist if the early-exit claim had stood.

---

## 1. What was audited, exactly

| | |
|---|---|
| Frozen candidate | `aa91f46571eb239df36e0233082153f2cba01899`, tree `0e666734…`, **v0.0.182** |
| Remote freshness | VERIFIED — `origin/master` equals that commit |
| PR #129 | open **draft**, NOT merged, excluded; its contents are a pending-change risk |
| Package | `KingmakerGunslinger-0.0.182-local-runtime.zip`, SHA-256 `8B9DE7BD…C84E` |
| DLL | SHA-256 `98C14F80…F3E8` · **release candidate** |
| Built against | `Assembly-CSharp.dll` SHA-256 `3B6450FF…F5AFB`, MVID `07fa1e4d-…`, byte-identical to the installed game (2.1.7b) |
| Profiles exercised | `gunslinger-only`, CotW, RU, CotW+RU, ToT, **ArmsArmor**, **Soundpacks** |
| Native transactions | **15**, 19 scenario executions |
| Saves used | **NONE.** No `.zks` opened, copied or written. `KMG_AUTOMATION_BASELINE` never touched |

## 2. Method, and what it is worth

- **One broad offline gate**: repository validation, the **complete domain suite (2,942 tests,
  0 failures)**, exact-reference Release compilation, packaging and strict package validation.
- **Fifteen native transactions**, each with a deployment receipt, live-mod backup, byte-verified
  UMM receipt and an independent post-run hash of every protected surface.
- **Six read-only source workers** across A04–A19 plus the resumed rows. Coordinator
  re-verification **changed or withdrew five of their conclusions** — including one that was
  wrong in the mod's favour and one that was wrong against it.
- **Engine behaviour measured by `ildasm`**, not assumed. This settled four questions: the buff
  FX dereference, the `UnitPart.PostLoad` catch behaviour, the `Buff::Activate()` call path, and
  — decisively — the sign semantics of `AttackBonusPenalty`.

**Evidence ceiling.** Most findings are *source-demonstrated*. The gate proves nothing about
gameplay. **No save was loaded**, so save migration, cold reconstruction and the area-load path
are unproven on this candidate.

## 3. Ranked discovered problems

| ID | Sev | Kind | Problem | Evidence |
|---|---|---|---|---|
| **GS-AUD-0039** | **S2** | product | **Scatter Shot's documented −2 attack penalty is applied as a +2 bonus** — a 4-point swing on every scatter attack. `AttackBonusPenalty` is *subtracted* by the engine, so the `-2` constant is a bonus. Six other KMG sites write penalties as `+n`; Scatter and Dead Shot (GS-AUD-0048) are the only two passing a negative. | source + **engine IL measured** |
| **GS-AUD-0006** | **S2** | product | A **failed settings-migration write** misclassifies a valid older `FeatureModules.json` as malformed and **resets all 13 modules to ON**, repeating every launch. Applies to any player whose schema is older than 12. | source |
| **GS-AUD-0048** | **S2** | product | **Dead Shot's iterative probes receive +5/+10 bonuses instead of −5/−10 penalties** — the same sign error as GS-AUD-0039. A complete sweep shows these are the **only two** negative-penalty production sites. *(Found by the rev-3 review.)* | source + **engine IL measured** |
| GS-AUD-0040 | S3 | doc | Shield Other's shipped **description** is stale. The distance-independent bond is the **approved** contract (`EXPANDED-SUMMONING-MISSION.md:71`); the code and the regression test are correct. *(Reclassified in rev 3 — not an owner decision.)* | source |
| **GS-AUD-0025** | **S2** | product | Untemplated **Summon Monster** alternate-caller children keep the donor's inherited celestial/fiendish template. (Scope corrected — the Nature's Ally families are not affected.) | source |
| **GS-AUD-0031** | **S2** | doc | `BETTER-VENDORS-COMPATIBILITY.md` misdescribes the "+1" acquisition path. (Corrected — the items **are** available, via Dire Narlmarches/Pitax/BTSL plus BV Military rank I.) | source |
| GS-AUD-0013 | S2/S3 | contract | Misfire suppression is released **only by player input**; repair, Quick Clear and rest do not clear it, so an AI companion stays locked out for the session. | source, verified |
| GS-AUD-0044 | S3 | harness | The deed-audio aggregate expectation is stale — **eleven notifications are correct** for the nested scenario, and the counter measures *attempted* not audible events. *(Reclassified from S2 in rev 4.)* | native, re-explained |
| GS-AUD-0008 | S3 | doc | Stale "Broken **or Wrecked**" field-repair prose in **eight** places including **`README.md:869`** — the Nexus front page. | source |
| GS-AUD-0041 | S3 | doc | PFA control-immunity catalog has 23 entries; its authority table lists 21. The Suggestion entry may violate the doc's own exclusion list. | source |
| GS-AUD-0032 | S3 | product | A base Gunslinger's level-17 Gun Training pick has no option and grants nothing. | source |
| GS-AUD-0004 | S3 | contract | 21 buff identities omit `FxOnStart`/`FxOnRemove`, deviating from the project's own invariant. **Originally S1 with a crash claim — withdrawn and natively disconfirmed.** | source + native |
| GS-AUD-0016 | S3 | contract | Two `UnitPart`s validate by **throwing inside an engine callback proven to have no catch handler**. No reachable trigger found today. | IL-proven |
| GS-AUD-0018 | S3 | harness | **Thirteen tests pass against two dead policy classes** the shipped mod never executes. | source |
| GS-AUD-0042 | S3 | doc | ADR-0037 requires Vital Strike exclusion from scatter; only precision damage is suppressed. Consequence UNVERIFIED. | source |
| GS-AUD-0005 | S3 | harness | The release gate depends on an **untracked ~99 MB IL dump**; a fresh checkout fails 23 tests with an opaque path error. | native |
| GS-AUD-0038 | S3 | unattributed | One assertion failed once and passed once on identical inputs. **Cause unresolved** — the shared-process explanation was wrong. | native |
| GS-AUD-0022 | S3 | harness (ours) | This audit's own adapter fixture was not offline and launched a real transaction. | native |

Plus GS-AUD-0009, 0010, 0011, 0012, 0017, 0019, 0023, 0026, 0029, 0033, 0034, 0035, 0036, 0043
(S3 and below), and UNASSESSED gaps 0003, 0014, 0015, 0020, 0021, 0024, 0028, 0037.

**Withdrawn:** GS-AUD-0027, GS-AUD-0030 — retained in the register, struck through, with the
reasoning error documented.

## 4. The crash lead — and the correction

The supplied incident (`Buff.SpawnParticleEffect_Patch4` → `SpawnBuffsFxs` → `OnAreaLoaded` →
`LoadGameException`) was the top-priority lead. I filed GS-AUD-0004 at **S1** claiming it
explained the crash, then **disconfirmed my own finding**: `Buff::Activate()` reaches the same
dereference on ordinary application, and a native run applied an affected buff six times with no
exception. **Downgraded to S3, crash claim withdrawn.** Replacement hypothesis (a null
`Blueprint` on a buff whose GUID no longer resolves — load-only) filed untested as GS-AUD-0024.

**The incident remains unreproduced and unattributed.** Its log is not on this machine, and six
of the eleven installed mods appear in no compatibility profile.

## 5. What the audit found CORRECT

Worth stating, because several areas were genuinely clean: twelve scatter checks, ten Shield
Other checks, eight Protection-from-Alignment checks, Magic Circle area lifetime (no
source-demonstrated defect; caster/bearer/recipient roles correctly separated), MagicCircleScroll
vendor stock and migration guards, Better Vendors exactly-once catch-up with no duplication path
across seven traced triggers, and 36 further rows — all with file:line.

## 6a. THE EXPLICIT REMAINING QUEUE — why this is a checkpoint, not a conclusion

**Eligible and simply not attempted:**

| Card | Status | Note |
|---|---|---|
| **A09** autonomous summon AI | NOT_RUN | admissibility never assessed |
| **A21** performance / repeated use | NOT_RUN | core candidate and restoration are valid, so it is admissible |
| **27 of 28 module boundary states** | NOT_RUN | one state (all 13 ON) was observed, PASS 47/47 |
| **COV-040** elemental appearance/equipment/motion | NOT_RUN | admissibility never assessed — and the *firearm* equivalents proved admissible |
| **GS-AUD-0047** | needs one re-run | with the `pistol-authored-marker-contract` sub-conditions reported separately, so the structural failure can be attributed |
| **GS-AUD-0045 / 0046 residue** | need one re-run | with the item-binding predicate and the policy-enchantment condition evaluated **independently of the counts**, so a count drift cannot suppress them |

**Genuinely blocked, with the prerequisite named:**

| Card | Blocker |
|---|---|
| **A22** upgrade/persistence | a legitimate older save fixture |
| Better Vendors real merchants | an authorized kingdom-stage fixture |
| Controller / human aesthetic | unreachable unattended |

## 6. Highest-value unclosed gaps

| Gap | Why it matters | Prerequisite |
|---|---|---|
| **Any save-bearing behaviour at all** | **No save was loaded.** Migration, cold reconstruction and the area-load path are entirely unproven | An admitted Working-save transaction |
| **Firearm cold-load token survival** (GS-AUD-0021) | Core persistent state; a lost token decodes **silently** | One disposable save, one cold restart |
| **The incident** (GS-AUD-0003/0024) | Attribution impossible without it | The original `output_log.txt` |
| **Summon roster presentation / audio / performance** (A09, A21, COV-027) | **Never attempted** — not blocked. The *firearm* presentation and audio scenarios were run (COV-058/059); the *summoning* ones were never assessed | Dedicated native sessions |
| **A22** upgrade/persistence | genuinely **blocked** | a legitimate older save fixture |
| **27 of the 28 module boundary configurations** | `observe-feature-module-settings` has **already been run** (PASS 47/47, all 13 modules ON). The remaining 27 states and the coupled OFF/ON pairs are what is unrun | Generate them from `Get-KmgFeatureModuleConfigurations -Boundary` and run selected states |
| **Better Vendors real merchants** (COV-051) | Declared BLOCKED | A kingdom-stage fixture |
| **Nereid / Treacherous Earth** (GS-AUD-0020) | Owner-**waived** — a waiver is a gap | Admitted native runs |

## 7. Coverage across all 59 rows (57 planned + 2 added)

`VERIFIED 3 · PARTIAL 36 · FAIL 9 · NOT_RUN 8 · HISTORICAL_ONLY 2 · BLOCKED 1` (59 rows)

`VERIFIED` fell from 6 to 3 in revision 3 — the Magic Circle rows had been verified on an
inventory-only observer. Revision 4 added **COV-058** (firearm visual rigs) and **COV-059**
(firearm/deed audio) because revision 3 had wrongly recorded firearm results against the
expanded-summoning row COV-027, which is restored to `NOT_RUN`. No row is `NOT_APPLICABLE`.
**PARTIAL** almost always means source and domain evidence exist and native does not.

## 8. Machine state

**Fully restored and independently verified after every one of the fifteen transactions.**

`KMG DLL (0.0.117) · FeatureModules.json · KMG_Firearms.bnk · UMM Params.xml · Assembly-CSharp.dll — ALL MATCH`

11 mods · 0 leftover `Mods.*` · 0 Kingmaker processes · 0 pending UMM receipts · lock free ·
118 saves, none opened · `KMG_AUTOMATION_BASELINE` LastWriteUtc still `2026-07-28T19:26:46Z`.

One stated limit: `Params.xml`'s *timestamp* is the restore time — the UMM contract restores
bytes, not timestamps. Bytes are exact. **No restoration debt remains.**

## 9. Suggested triage order

1. **GS-AUD-0039** — the scatter sign inversion. A one-constant fix plus an assertion that pins
   the sign by behaviour instead of by the constant's literal value.
2. **GS-AUD-0008** — fix `README.md`. Nexus front page, currently false about a core mechanic.
3. **GS-AUD-0006** — move the migration `Save` out of the parse `try`.
4. **GS-AUD-0040** — correct Shield Other's shipped **description** only. There is **no design
   decision here**: `planning/EXPANDED-SUMMONING-MISSION.md:71` already approves the
   distance-independent bond, and the code and regression test are correct. **Do not reintroduce
   the distance check.**
4b. **GS-AUD-0048** — Dead Shot's probe sign, alongside GS-AUD-0039. One coherent change.
5. **GS-AUD-0025 / 0026**, then **GS-AUD-0031**, **GS-AUD-0013**, **GS-AUD-0041**.
6. **Close the save-bearing gap before release** — GS-AUD-0021 above all.
7. Harness debt (**GS-AUD-0005, 0018, 0038**) — currently producing false assurance.

**No product repair was implemented. Nothing was merged, published or uploaded.**

## 10. Honest statement of what this audit is not

It did not reproduce the crash. It did not load a save. Native coverage is **startup across
seven profiles plus ten disposable/observe scenarios** — real, but narrow. Several S2 findings
rest on source reading alone. **A09, A21, A22, 27 of the 28 module boundary states and the whole
summon-roster presentation/audio/performance contract were never attempted.**

**And its findings needed three rounds of correction.** Across those reviews it withdrew two
false positives (Nimble, RU), withdrew one mechanism (GS-AUD-0038), reclassified three findings
(GS-AUD-0040 Shield Other, GS-AUD-0044 Dead Shot audio, GS-AUD-0046 enchantment counts),
retracted an over-exclusion (GS-AUD-0045), corrected a coverage attribution error (COV-027), and
changed its own disposition from "complete" to "checkpoint". Four of those errors shared one
cause: **accepting a subagent's claim, or an apparent conflict, without finishing the
verification.** Two of them — Nimble and Shield Other — would have sent a repair agent to undo
correct, deliberately approved code.

Read each entry's evidence strength before acting on it.

**A clean domain suite and clean startup smokes are not a release qualification.** On this
evidence, release readiness for 0.0.182 is **not established**.
