# v0.0.182 pre-release audit — findings record

Independent pre-release audit of **v0.0.182** (`aa91f46571eb239df36e0233082153f2cba01899`),
carried out on HOWARD-LAPTOP on 2026-11-11 ahead of a Nexus Mods release.

**This directory is a triage input, not a release approval, and contains no product change.**
No fix was implemented, nothing was merged or published, and no save was opened at any point.

## Read in this order

| File | What it is |
|---|---|
| [`SUMMARY.md`](SUMMARY.md) | Standalone executive summary: what was audited, ranked problems, gaps, machine state, suggested triage order |
| [`FINDINGS.md`](FINDINGS.md) | 48 numbered records, `GS-AUD-0001`..`GS-AUD-0048` (0027 and 0030 WITHDRAWN) — **not** 48 defects, each with kind, severity, evidence strength and disposition held separately |
| [`COVERAGE.tsv`](COVERAGE.tsv) | All 59 coverage rows (57 planned + 2 added) with a final disposition each, separating source / domain / native evidence |
| [`HANDOFF.md`](HANDOFF.md) | Reproduction commands, the smallest next investigations, and the decisions only the owner can make |

## Revision 4 — corrected after a third owner review

**This is an incomplete audit checkpoint, not a finished audit** (`IN_PROGRESS_CHECKPOINT`).
`SUMMARY.md` §6a carries the explicit remaining queue.

> **"48 findings" is 48 numbered records, not 48 demonstrated product defects.** It mixes
> confirmed observations, **2 withdrawn false positives**, several reclassified entries, contract
> questions, conditional risks with unestablished preconditions, harness defects and verification
> gaps. **Active S2 count: 6.** Do not use it as a repair backlog without reading each entry's
> evidence strength.

Corrected in revision 4:

- **`GS-AUD-0044` (Dead Shot audio) — RECLASSIFIED S2 → S3 harness.** **Eleven notifications are
  correct.** The counter brackets the *entire* nested scenario, which runs eleven
  discharge-producing volleys, and production posts one per non-misfire volley; the assertion's
  `+1` is stale. The counter also measures **attempted** notifications (incremented before the
  emitter check and before `TryPost`), so it could not have evidenced audio duplication. Its
  "second-highest repair priority" ranking is withdrawn.
- **`GS-AUD-0045` — over-exclusion retracted.** `items.Count == 14 &&` **short-circuits**, so the
  binding comparison never ran, and the diagnostic prints the *catalog's expectation* rather than
  a comparison result. The visual bindings are **UNVERIFIED**, not confirmed correct.
- **`GS-AUD-0046` — count corrected.** The predicate already expects **12** enchantments and 12
  were observed. Only the weapon (55→57) and item (56→58) counts are stale, and
  `custom.Contains(policy)` is skipped after the weapon mismatch, so policy presence is unverified.
- **COV-027 attribution fixed.** Revision 3 classified the *expanded-summoning* row using
  *firearm* results. COV-027 is restored to `NOT_RUN`; the firearm evidence moved to new rows
  **COV-058** and **COV-059**.
- **Superseded instructions removed** from the summary and handoff bodies — including a triage
  item still telling the owner to decide Shield Other's range contract, which is already settled.
- **`GS-AUD-0048` added, found by the reviewer:** Dead Shot's iterative probes carry the same sign
  error as Scatter. A complete sweep now bounds it — **exactly two** production sites pass a
  negative penalty, and there is no third.

Earlier revisions withdrew **GS-AUD-0030 (Nimble)** and **GS-AUD-0027 (RU version pin)** as false
positives — the Nimble one would have *introduced* a rules defect into correct code — withdrew
GS-AUD-0038's mechanism, reclassified GS-AUD-0040 (Shield Other) once an existing owner contract
was found, downgraded the Magic Circle coverage that had rested on an inventory-only observer, and
replaced the claim that the audit was complete.

## Headline

The candidate **passes its complete offline gate** — 2,942 domain tests, 0 failures,
exact-reference Release compilation, packaging and strict package validation — and **loads
cleanly on seven optional-mod profiles** across **15 native transactions**.

The **6 active S2 findings**:

- **GS-AUD-0039 — Scatter Shot's documented −2 attack penalty is applied as a +2 bonus**, and
  **GS-AUD-0048 — Dead Shot's iterative probes get +5/+10 instead of −5/−10.** Same root cause:
  the engine *subtracts* `AttackBonusPenalty`, and the whole constructor-to-trigger chain carries
  no clamp. A complete sweep shows these are the **only two** negative-penalty production sites.
  Both survived because the tests pin intended numbers rather than the realised modifier.
- **GS-AUD-0006** — a failed settings-migration write misclassifies a valid older file as
  malformed and silently resets all 13 modules to ON.
- **GS-AUD-0025** — untemplated Summon Monster alternate-caller children keep the donor's
  inherited template.
- **GS-AUD-0031** — the Better Vendors doc misdescribes the "+1" acquisition path.
- **GS-AUD-0013** (S2/S3) — misfire suppression is released only by player input.

Plus `README.md:869` and seven other places still describe Broken-**or-Wrecked** field repair
(GS-AUD-0008). The shipped contract is Broken-only, and that text is the Nexus front page.

## How to read the evidence strength

Most findings are **source-demonstrated**. The offline gate proves nothing about gameplay, and
the native runs covered **startup across seven profiles plus ten disposable/observe scenarios**.
**No save was loaded at any point**, so save migration, cold reconstruction and the area-load
path are unproven on this candidate.

The supplied 2026-11-11 area-load crash was **not reproduced and remains unattributed**.

## A correction that is part of the record

`GS-AUD-0004` was originally filed **S1** with a claim that it explained that crash. The audit
then **disconfirmed its own finding** — first by disassembly (`Buff::Activate()` reaches the
same dereference on ordinary buff application), then natively (an affected buff was applied and
consumed six times on 0.0.182 with no exception). It is now **S3**, the crash claim is
withdrawn, and a replacement hypothesis is filed untested as `GS-AUD-0024`.

The correction is kept visible rather than edited away: see the banner at the top of
`GS-AUD-0004` in [`FINDINGS.md`](FINDINGS.md).

## What is deliberately NOT in this directory

Kept local as private campaign evidence, per the audit's own evidence rules:

- the append-only outcomes journal (20 events, ~130 KB of machine-local receipts, run IDs and
  deployment manifests)
- the evidence index, with hashes and paths of machine-local artifacts
- raw `output_log.txt` captures, runtime result JSON, worker reports, the campaign's bounded
  audit adapter, and campaign control state

Those live in the local campaign directory named in [`HANDOFF.md`](HANDOFF.md). Nothing
referencing a save file's contents, a private reference assembly, or a raw log is published here.

## Machine state after the audit

Fifteen native transactions were opened and closed. Every protected surface — installed mod tree,
`FeatureModules.json`, the external `KMG_Firearms.bnk` SoundBank, UMM `Params.xml` and the
game's own `Assembly-CSharp.dll` — was re-hashed against the intake baseline afterwards and
**all match**. No restoration debt remains. Details in `HANDOFF.md` §3 and §7.
