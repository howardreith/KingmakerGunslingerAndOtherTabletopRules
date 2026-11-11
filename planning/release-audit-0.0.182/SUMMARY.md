# Gunslinger pre-release audit — final summary

**Campaign** KMG-AUD-20261111T170737Z · **host** HOWARD-LAPTOP (verified OLD LAPTOP)
**Started** 2026-11-11T17:07:37Z · **work completed** 2026-11-11T18:42Z · **elapsed ≈ 1 h 35 min**
of the 48-hour allowance. Work finished early because the planned eligible queue was complete;
the deadline was never extended and the remaining budget was not consumed for its own sake.

**Disposition: `COMPLETE_WITH_GAPS`.** This is **not** a release approval. It means the planned
audit and its cleanup are accounted for, every coverage row has a disposition, and the gaps are
stated. Important behaviour remains unverified — see §5.

---

## 1. What was audited, exactly

| | |
|---|---|
| Frozen candidate | commit `aa91f46571eb239df36e0233082153f2cba01899`, tree `0e666734a4bf8ee8b8a051d5e286675f04898cc2`, **v0.0.182** |
| Remote freshness | **VERIFIED** — `origin/master` equals that commit; nothing landed during the campaign |
| PR #129 | still an **open draft** (head moved to `121623b5…`); **NOT merged, excluded from this candidate**. Its contents are a pending-change risk, not findings |
| Package built | `KingmakerGunslinger-0.0.182-local-runtime.zip`, 73,673,750 bytes, SHA-256 `8B9DE7BDB090266790058FB336C66EA0468803E22D48710D45E08B7CCE65C84E` |
| DLL | SHA-256 `98C14F80F6D027FCD22110A70BE371D2550778F3226A2BD009DDA410B565F3E8` · artifact kind: **release candidate** |
| Built against | `Assembly-CSharp.dll` SHA-256 `3B6450FF…F5AFB`, MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`, byte-identical to the installed game (2.1.7b) |
| Profiles actually exercised | `gunslinger-only`, `gunslinger-call-of-the-wild`, `gunslinger-races-unleashed`, `gunslinger-call-of-the-wild-races-unleashed`, `gunslinger-tweak-or-treat` |
| Saves used | **NONE.** No `.zks` was opened, copied or written at any point. `KMG_AUTOMATION_BASELINE` was never selected or touched |

**One fact that reframes everything else:** the mod installed on this machine is **0.0.117**, not
0.0.182 (GS-AUD-0001). The user's recent play experience is therefore *not* this release, and
every native transaction in this campaign had to restore 0.0.117 — not "the current release".

## 2. Method, and what it is worth

- **One broad offline gate**, run once on the frozen candidate: repository validation, the
  **complete domain suite (2,942 tests, 0 failures)**, exact-reference Release compilation,
  packaging and strict package validation. 643.5 s. PASS.
- **Five native transactions** on the real installation, each with a deployment receipt, a
  live-mod backup, a byte-verified UMM receipt, and an independent post-run hash check of every
  protected surface.
- **Four parallel read-only source workers** covering A04–A17 and A19, whose claims were
  **re-verified by the coordinator** before promotion — two were materially re-framed as a result.
- **Engine behaviour measured, not assumed:** `ildasm` over the qualified reference assembly
  settled three questions that source reading alone could not.

**Evidence ceiling.** Most findings are *source-demonstrated*. The gate proves nothing about
gameplay. The native runs cover **startup and two deed scenarios only**. No save was loaded, so
nothing here speaks to save migration, cold reconstruction, or the area-load path.

## 3. Ranked discovered problems

Severity is provisional impact, not proof. Full detail in `AUDIT-FINDINGS.md`.

| ID | Sev | Kind | Problem | Evidence |
|---|---|---|---|---|
| **GS-AUD-0030** | **S2** | fidelity + doc conflict | **Nimble is granted at levels 2/6/10/14/18; tabletop is 3/7/11/15/19 — and the project's own fidelity matrix calls that placement "EXACT".** Every other row in that matrix sits on its correct tabletop level. One of the two is wrong. | source, coordinator-verified |
| **GS-AUD-0015** | **S2** | product | A persisted **Powerful Change** bonus is reconstructed only via the *live* caster, so a Share Transmutation ally reloaded without its caster in scene silently shows **+4 instead of +6**, with nothing logged. | source |
| **GS-AUD-0025** | **S2** | product | **Untemplated alternate-caller summons keep the donor's inherited celestial/fiendish template.** The canonical spell strips it; the adapter's untemplated branch does not. Affects all seven alternate caller families. | source, coordinator-verified |
| **GS-AUD-0027** | **S2** | product | The **Races Unleashed version pin fails open** — an unrecognised RU version is integrated against anyway. RU updates independently of KMG. | source |
| **GS-AUD-0031** | **S2** | doc conflict | `BETTER-VENDORS-COMPATIBILITY.md` says KMG stocks four melee "+1" items the candidate **does not publish**; a second document says the opposite and matches the code. | source |
| **GS-AUD-0006** | **S2** | product | A **failed settings-migration write** misclassifies a *valid* older settings file as malformed and **silently resets all 13 modules to ON** — and repeats every launch. Every upgrading player passes through this write. | source |
| **GS-AUD-0001** | S2 | environment | The installed mod is **0.0.117**, not the 0.0.182 release candidate. | native |
| **GS-AUD-0013** | S2/S3 | contract | Misfire suppression is cleared **only by player input** — repair, Quick Clear and rest do not clear it, so an AI-driven companion stays locked out of its firearm for the session. | source, coordinator-verified |
| **GS-AUD-0008** | S3 | doc drift | Stale "Broken **or Wrecked**" field-repair prose in **eight** places including **`README.md:869`** — the Nexus-facing text. Wrecked field repair fails. | source |
| GS-AUD-0032 | S3 | product | A base Gunslinger's level-17 Gun Training pick has no option and grants nothing; disclosed only in a source comment. | source |
| GS-AUD-0034 | S3 | compatibility | Both KMG `ExtractSelectionItems` postfixes **silently discard third-party selection items** — the shape of "KMG broke mod X" reports. | source |
| GS-AUD-0004 | **S3** ⚠ | contract | 21 buff identities omit `FxOnStart`/`FxOnRemove`, breaking the project's own invariant. **Originally filed S1 with a crash-cause claim — that claim was WITHDRAWN and natively disconfirmed by this audit.** See §4. | source + native |
| GS-AUD-0016 | S3 | contract | Two `UnitPart`s validate by **throwing inside an engine callback proven to have no catch handler** — an unloadable save if ever triggered. No reachable trigger found today. | IL-proven mechanism |
| GS-AUD-0018 | S3 | harness | **Thirteen tests pass against two dead policy classes** the shipped mod never executes. | source |
| GS-AUD-0038 | S3 | harness | `up-close-critical` fails **only when another scenario runs first in the same process**. Implies batched native results are weaker than per-process ones. | native, intermittent |
| GS-AUD-0005 | S3 | harness | The broad release gate depends on an **untracked ~99 MB IL dump**; a fresh checkout fails 23 tests with an opaque path error. | native |
| GS-AUD-0002 | S3 | environment | Live UMM `Params.xml` was already left in the 2026-11-07 test-profile state. This campaign **demonstrated the mechanism** and protected against it. | native |
| GS-AUD-0022 | S3 | harness (ours) | This audit's own adapter fixture was not offline and launched a real transaction. Recorded, not hidden. | native |

Also: GS-AUD-0009, 0010, 0011, 0012, 0017, 0019, 0026, 0029, 0033, 0035, 0036 (S3 and below).

## 4. The crash lead — and a correction I owe the record

The supplied incident (`Buff.SpawnParticleEffect_Patch4` → `SpawnBuffsFxs` → `OnAreaLoaded` →
`LoadGameException`) was the campaign's highest-priority lead. What happened:

1. The engine rule was **measured from IL**: `SpawnParticleEffect` dereferences
   `this.Blueprint.FxOnStart` with **no null guard**.
2. 21 KMG buff identities were found omitting that field — and the project had **already
   root-caused and twice reproduced** a null FX-link crash from exactly that omission in 0.0.88.
3. I filed this as **S1, claiming it explained the crash.**
4. **I then disconfirmed my own finding.** Further disassembly showed `Buff::Activate()` calls
   `SpawnParticleEffect()` directly, so the dereference is reached on **ordinary buff
   application**, not just area load — meaning these shipped deeds would crash on first use if
   the field were really null.
5. A native run settled it: `disposable-pistolero-deeds` applied and consumed
   `KMG_UpCloseAndDeadly_Armed` — **one of the 21** — six times across two runs on 0.0.182,
   **with no exception**. The premise is false.
6. **Severity downgraded S1 → S3; the crash-cause claim withdrawn.** A replacement hypothesis
   (a null `Blueprint` on a buff whose GUID no longer resolves — reachable only on load, which
   fits both the stack and the intermittency) is filed as **GS-AUD-0024, untested**.

**The incident remains UNATTRIBUTED.** It cannot be attributed to KMG, and it was not
reproduced. Critically, **the incident log does not exist on this machine** (GS-AUD-0003): the
last local game run was the automated 2026-11-07 session, and six of the eleven installed mods
appear in no compatibility profile at all.

## 5. Highest-value unclosed gaps, and what would close each

| Gap | Why it matters | Prerequisite |
|---|---|---|
| **Firearm cold-load token survival** (GS-AUD-0021) | The Gunslinger module's core persistent state; a lost token decodes **silently** to Normal/empty | One disposable save, one cold restart |
| **Any save-bearing behaviour at all** | **No save was loaded in this campaign.** Migration, cold reconstruction and the area-load path are entirely unproven on 0.0.182 | An admitted Working-save transaction |
| **The incident itself** (GS-AUD-0003/0024) | Attribution is impossible without it | The original 2,680,892-byte `output_log.txt` |
| **Brown-Fur saved-owner reconstruction** (GS-AUD-0015) | A real wrong-number bug with no diagnostic | A legitimate older save with the fact |
| **Gunslinger's Dodge** (GS-AUD-0012/0023) | A level-1 deed with **no working native acceptance route at all** | A non-detached fixture, or human playtest |
| **Better Vendors real merchants** (COV-051) | Declared BLOCKED; no kingdom-stage fixture exists | An authorized kingdom-stage fixture |
| **Autonomous summon AI, presentation, audio, performance** (A09/A20/A21) | Never scheduled | Dedicated native sessions |
| **Elemental Nereid / Treacherous Earth** (GS-AUD-0020) | Owner-**waived** — a waiver is a gap, not a pass | Admitted native runs |

## 6. Coverage across all 57 planned rows

`VERIFIED 4 · PARTIAL 34 · FAIL 5 · NOT_RUN 9 · HISTORICAL_ONLY 2 · BLOCKED 2 · NOT_APPLICABLE 1`

All 13 modules received source breadth review. **PARTIAL** almost always means *source and domain
evidence exist, native does not*. The single `NOT_APPLICABLE` (ArmsArmor/Soundpacks profiles) is
justified by host scope — neither mod is installed — not by lack of time.

Native evidence exists for exactly three things: **startup on five profiles**, **two deed
scenarios**, and **restoration**. Everything else native is historical or absent.

## 7. Machine state

**Fully restored and independently verified.** After every one of the five transactions, each
protected surface was re-hashed against the intake baseline:

`KMG DLL (0.0.117) · FeatureModules.json · KMG_Firearms.bnk · UMM Params.xml · Assembly-CSharp.dll — ALL MATCH`

11 mods · 0 leftover `Mods.*` directories · 0 Kingmaker processes · 0 pending UMM receipts ·
coordination lock free · 118 saves, none opened ·
`KMG_AUTOMATION_BASELINE` LastWriteUtc still `2026-07-28T19:26:46.0491301Z`.

**One stated limit:** `Params.xml`'s *timestamp* is now the restore time, because the UMM
contract restores **bytes/absence, not timestamps**. Its bytes are exact. The SoundBank's
contract *does* include its timestamp, and that was preserved exactly. **No restoration debt
remains.** Campaign-created scaffolding to remove is listed in `AUDIT-RESTORATION.md`.

## 8. Suggested triage order

1. **GS-AUD-0008** — fix `README.md`. It is the Nexus front page and it currently tells players
   something false about a core mechanic. Cheapest possible win.
2. **GS-AUD-0030** — decide Nimble: correct the levels, or relabel the matrix row. Do not ship
   with the matrix asserting "EXACT" for a placement that is not.
3. **GS-AUD-0006** — move the migration `Save` out of the parse `try`. Every upgrading player
   hits that write.
4. **GS-AUD-0025 / 0026** — the template leak and the contract gap that hides it.
5. **GS-AUD-0015**, **GS-AUD-0027**, **GS-AUD-0031**, **GS-AUD-0013**.
6. **Close the save-bearing gap before release** — GS-AUD-0021 above all. It is the single
   largest hole in the evidence.
7. Harness debt (**GS-AUD-0005, 0018, 0038**) — currently producing false assurance.

**No product repair was implemented, no fix applied, nothing merged, published or uploaded.**

## 9. Honest statement of what this audit is not

It did not reproduce the user's crash. It did not load a single save. Its native coverage is
startup plus two deeds. Four S2 product findings rest on source reading, not gameplay. And its
own headline finding was wrong and had to be withdrawn — which is recorded in full rather than
quietly deleted (EV-000012, EV-000014).

**A clean domain suite and a clean startup smoke are not a release qualification.** On this
evidence, release readiness for 0.0.182 is **not established**.
