# v0.0.182 pre-release audit — findings record

Independent pre-release audit of **v0.0.182** (`aa91f46571eb239df36e0233082153f2cba01899`),
carried out on HOWARD-LAPTOP on 2026-11-11 ahead of a Nexus Mods release.

**This directory is a triage input, not a release approval, and contains no product change.**
No fix was implemented, nothing was merged or published, and no save was opened at any point.

## Read in this order

| File | What it is |
|---|---|
| [`SUMMARY.md`](SUMMARY.md) | Standalone executive summary: what was audited, ranked problems, gaps, machine state, suggested triage order |
| [`FINDINGS.md`](FINDINGS.md) | The register — 38 findings, `GS-AUD-0001`..`GS-AUD-0038`, each with kind, severity, evidence strength and disposition held separately |
| [`COVERAGE.tsv`](COVERAGE.tsv) | All 57 planned coverage rows with a final disposition each, separating source / domain / native evidence |
| [`HANDOFF.md`](HANDOFF.md) | Reproduction commands, the smallest next investigations, and the decisions only the owner can make |

## Headline

The candidate **passes its complete offline gate** — repository validation, 2,942 domain tests
with 0 failures, exact-reference Release compilation, packaging and strict package validation —
and **loads cleanly on five optional-mod profiles** (standalone, CotW, RU, CotW+RU, ToT).

It also carries **7 findings at S2**, the most actionable being:

- `README.md:869` (and seven other places) still describes **Broken-or-Wrecked field repair**.
  The shipped contract is Broken-only. That text is the Nexus front page.
- **Nimble is granted at levels 2/6/10/14/18**; the tabletop table is 3/7/11/15/19 — and
  `planning/GUNSLINGER-FIDELITY-MATRIX.md:66` classifies that placement `EXACT`, while every
  other row in the same matrix sits on its correct tabletop level.
- A **failed settings-migration write** misclassifies a valid older `FeatureModules.json` as
  malformed and silently resets all 13 modules to ON — and every upgrading player passes
  through that write.
- **Untemplated alternate-caller summons keep the donor's inherited celestial/fiendish
  template**; the canonical spell strips it, the adapter's untemplated branch does not.

## How to read the evidence strength

Most findings are **source-demonstrated**. The offline gate proves nothing about gameplay, and
the native runs covered **startup and two deed scenarios only**. **No save was loaded**, so save
migration, cold reconstruction and the area-load path are unproven on this candidate.

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

- the append-only outcomes journal (16 events, ~105 KB of machine-local receipts, run IDs and
  deployment manifests)
- the evidence index, with hashes and paths of machine-local artifacts
- raw `output_log.txt` captures, runtime result JSON, worker reports, the campaign's bounded
  audit adapter, and campaign control state

Those live in the local campaign directory named in [`HANDOFF.md`](HANDOFF.md). Nothing
referencing a save file's contents, a private reference assembly, or a raw log is published here.

## Machine state after the audit

Five native transactions were opened and closed. Every protected surface — installed mod tree,
`FeatureModules.json`, the external `KMG_Firearms.bnk` SoundBank, UMM `Params.xml` and the
game's own `Assembly-CSharp.dll` — was re-hashed against the intake baseline afterwards and
**all match**. No restoration debt remains. Details in `HANDOFF.md` §3 and §7.
