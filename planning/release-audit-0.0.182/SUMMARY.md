# Gunslinger pre-release audit — final summary

**Campaign** KMG-AUD-20261111T170737Z · **host** HOWARD-LAPTOP (verified OLD LAPTOP)
**Started** 2026-11-11T17:07:37Z · **revision 2** after owner review · **elapsed ≈ 3 h 55 min**
of the 48-hour allowance. Deadline 2026-11-13T17:07:37Z, never extended or restarted.

**Disposition: `COMPLETE_WITH_GAPS`.** Not a release approval. Every coverage row has a
disposition and the gaps are stated. Important behaviour remains unverified — see §6.

> ### Revision 2 — what the owner review changed
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
| Native transactions | **11**, 15 scenario executions |
| Saves used | **NONE.** No `.zks` opened, copied or written. `KMG_AUTOMATION_BASELINE` never touched |

## 2. Method, and what it is worth

- **One broad offline gate**: repository validation, the **complete domain suite (2,942 tests,
  0 failures)**, exact-reference Release compilation, packaging and strict package validation.
- **Eleven native transactions**, each with a deployment receipt, live-mod backup, byte-verified
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
| **GS-AUD-0039** | **S2** | product | **Scatter Shot's documented −2 attack penalty is applied as a +2 bonus** — a 4-point swing on every scatter attack. `AttackBonusPenalty` is *subtracted* by the engine, so the `-2` constant is a bonus. Six other KMG sites write penalties as `+n`; scatter is the only one passing a negative. | source + **engine IL measured** |
| **GS-AUD-0006** | **S2** | product | A **failed settings-migration write** misclassifies a valid older `FeatureModules.json` as malformed and **resets all 13 modules to ON**, repeating every launch. Applies to any player whose schema is older than 12. | source |
| **GS-AUD-0040** | **S2** | contract | **Shield Other computes a range bound and never compares it.** The published description says "within close range"; the code is unlimited; a test *deliberately* asserts the split still happens at ~65 ft with a 35 ft bound. Three authorities, three answers. | source |
| **GS-AUD-0025** | **S2** | product | Untemplated **Summon Monster** alternate-caller children keep the donor's inherited celestial/fiendish template. (Scope corrected — the Nature's Ally families are not affected.) | source |
| **GS-AUD-0031** | **S2** | doc | `BETTER-VENDORS-COMPATIBILITY.md` misdescribes the "+1" acquisition path. (Corrected — the items **are** available, via Dire Narlmarches/Pitax/BTSL plus BV Military rank I.) | source |
| GS-AUD-0013 | S2/S3 | contract | Misfire suppression is released **only by player input**; repair, Quick Clear and rest do not clear it, so an AI companion stays locked out for the session. | source, verified |
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

## 6. Highest-value unclosed gaps

| Gap | Why it matters | Prerequisite |
|---|---|---|
| **Any save-bearing behaviour at all** | **No save was loaded.** Migration, cold reconstruction and the area-load path are entirely unproven | An admitted Working-save transaction |
| **Firearm cold-load token survival** (GS-AUD-0021) | Core persistent state; a lost token decodes **silently** | One disposable save, one cold restart |
| **The incident** (GS-AUD-0003/0024) | Attribution impossible without it | The original `output_log.txt` |
| **Autonomous summon AI, presentation, audio, performance** (A09/A20/A21/A22) | **Genuinely never attempted** — not blocked, just not done | Dedicated native sessions |
| `observe-feature-module-settings` | Would close the native 13-module boundary | Build the 13 Boolean parameters from the catalogue |
| **Better Vendors real merchants** (COV-051) | Declared BLOCKED | A kingdom-stage fixture |
| **Nereid / Treacherous Earth** (GS-AUD-0020) | Owner-**waived** — a waiver is a gap | Admitted native runs |

## 7. Coverage across all 57 planned rows

`VERIFIED 6 · PARTIAL 31 · FAIL 8 · NOT_RUN 9 · HISTORICAL_ONLY 2 · BLOCKED 1`

No row is `NOT_APPLICABLE` — the one that was has been corrected (see revision note). **PARTIAL**
almost always means source and domain evidence exist, native does not.

## 8. Machine state

**Fully restored and independently verified after every one of the eleven transactions.**

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
4. **GS-AUD-0040** — decide Shield Other's range contract; three authorities disagree.
5. **GS-AUD-0025 / 0026**, then **GS-AUD-0031**, **GS-AUD-0013**, **GS-AUD-0041**.
6. **Close the save-bearing gap before release** — GS-AUD-0021 above all.
7. Harness debt (**GS-AUD-0005, 0018, 0038**) — currently producing false assurance.

**No product repair was implemented. Nothing was merged, published or uploaded.**

## 10. Honest statement of what this audit is not

It did not reproduce the crash. It did not load a save. Native coverage is startup plus six
scenarios. Several S2 findings rest on source reading alone. A09/A20/A21/A22 were never
attempted. **And its first revision over-claimed completeness, shipped two false positives, and
had to be corrected on review** — one of which would have broken correct code.

**A clean domain suite and clean startup smokes are not a release qualification.** On this
evidence, release readiness for 0.0.182 is **not established**.
