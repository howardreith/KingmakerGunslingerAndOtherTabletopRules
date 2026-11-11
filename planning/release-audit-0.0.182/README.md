# v0.0.182 pre-release audit — findings record

Independent pre-release audit of **v0.0.182** (`aa91f46571eb239df36e0233082153f2cba01899`),
carried out on HOWARD-LAPTOP on 2026-11-11 ahead of a Nexus Mods release.

**This directory is a triage input, not a release approval, and contains no product change.**
No fix was implemented, nothing was merged or published, and no save was opened at any point.

## Read in this order

| File | What it is |
|---|---|
| [`SUMMARY.md`](SUMMARY.md) | Standalone executive summary: what was audited, ranked problems, gaps, machine state, suggested triage order |
| [`FINDINGS.md`](FINDINGS.md) | The register — 43 findings, `GS-AUD-0001`..`GS-AUD-0043` (0027 and 0030 WITHDRAWN), each with kind, severity, evidence strength and disposition held separately |
| [`COVERAGE.tsv`](COVERAGE.tsv) | All 57 planned coverage rows with a final disposition each, separating source / domain / native evidence |
| [`HANDOFF.md`](HANDOFF.md) | Reproduction commands, the smallest next investigations, and the decisions only the owner can make |

## Revision 2 — corrected after owner review

The first revision claimed the eligible audit queue was complete. **It was not**, and two
prominent findings were wrong. Both were verified and corrected:

- **GS-AUD-0030 (Nimble) — WITHDRAWN false positive.** Tabletop Gunslinger Nimble *is*
  2/6/10/14/18, exactly as implemented. Acting on the original finding would have **introduced**
  a rules defect into correct code.
- **GS-AUD-0027 (RU version pin) — WITHDRAWN false positive.** The integration gates structurally
  on blueprint shape; unsupported input is refused before mutation and generated rows withheld.
- **GS-AUD-0038's mechanism — withdrawn.** The two scenarios ran in separate game processes, not
  a shared one. Cause unresolved.
- **GS-AUD-0004** restructured so the current adjudication is not contradicted by the withdrawn
  analysis sitting beneath it; **0015, 0025, 0031, 0034, 0019, 0006, 0001** narrowed or re-grounded.

The unfinished source rows and five never-assessed save-free native scenarios were then
completed — and that resumed work produced the campaign's most important defect.

## Headline

The candidate **passes its complete offline gate** — 2,942 domain tests, 0 failures, exact-reference
Release compilation, packaging and strict package validation — and **loads cleanly on seven
optional-mod profiles** across **11 native transactions**.

It also carries **5 findings at S2**:

- **GS-AUD-0039 — Scatter Shot's documented −2 attack penalty is applied as a +2 bonus.**
  The engine *subtracts* `AttackBonusPenalty` (measured from `RuleCalculateAttackBonus::OnTrigger`),
  so the `-2` constant is a bonus. Six other KMG sites write penalties as `+n`; scatter is the only
  production site passing a negative. A 4-point swing on every scatter attack. **This finding only
  exists because the review refused the early-exit claim.**
- **GS-AUD-0006** — a failed settings-migration write misclassifies a valid older file as malformed
  and silently resets all 13 modules to ON.
- **GS-AUD-0040** — Shield Other computes a range bound and never compares it; the description, the
  code and a deliberate test assertion say three different things.
- **GS-AUD-0025** — untemplated Summon Monster alternate-caller children keep the donor's inherited
  template.
- **GS-AUD-0031** — the Better Vendors doc misdescribes the "+1" acquisition path.

Plus `README.md:869` and seven other places still describe Broken-**or-Wrecked** field repair
(GS-AUD-0008). The shipped contract is Broken-only, and that text is the Nexus front page.

## How to read the evidence strength

Most findings are **source-demonstrated**. The offline gate proves nothing about gameplay, and
the native runs covered **startup across seven profiles plus six disposable scenarios**.
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

- the append-only outcomes journal (18 events, ~130 KB of machine-local receipts, run IDs and
  deployment manifests)
- the evidence index, with hashes and paths of machine-local artifacts
- raw `output_log.txt` captures, runtime result JSON, worker reports, the campaign's bounded
  audit adapter, and campaign control state

Those live in the local campaign directory named in [`HANDOFF.md`](HANDOFF.md). Nothing
referencing a save file's contents, a private reference assembly, or a raw log is published here.

## Machine state after the audit

Eleven native transactions were opened and closed. Every protected surface — installed mod tree,
`FeatureModules.json`, the external `KMG_Firearms.bnk` SoundBank, UMM `Params.xml` and the
game's own `Assembly-CSharp.dll` — was re-hashed against the intake baseline afterwards and
**all match**. No restoration debt remains. Details in `HANDOFF.md` §3 and §7.
