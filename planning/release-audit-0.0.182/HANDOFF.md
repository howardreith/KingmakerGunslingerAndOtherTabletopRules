# Audit handoff — later triage and repair

Campaign KMG-AUD-20261111T170737Z · host HOWARD-LAPTOP · candidate `aa91f4657` / v0.0.182.
Everything below was actually executed in this campaign unless marked **not run**.

## 1. Where everything is

```
C:\Dev\KingmakerGunslingerLab\audits\KMG-AUD-20261111T170737Z\
  AUDIT-SUMMARY.md        read this first — standalone
  AUDIT-FINDINGS.md       47 records, GS-AUD-0001..0047 (0027 and 0030 WITHDRAWN).
                          NOT 47 defects - 6 active S2; read each entry's evidence strength
  AUDIT-OUTCOMES.md       19 append-only events, EV-000001..EV-000019
  AUDIT-COVERAGE.tsv      all 57 rows dispositioned
  AUDIT-RESTORATION.md    intake baseline + every transaction
  AUDIT-STATE.json        durable checkpoint, immutable deadline
  AUDIT-EVIDENCE-INDEX.tsv 23 retained artifacts with SHA-256
  workers\W1..W6\         full source traces, six read-only workers
  adapter\                the one bounded audit adapter
  evidence\A00 A02 A03 A15 A18 A25-review\
  private\preserved-logs\ intake output_log.txt, preserved before any launch
  tools-fxscan.py, tools-coverage.py, tools-state.py, tools-queue.py, tools-amend-finding.py
```
Native run artifacts live outside the campaign dir, under
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\` and `...\runtime-backups\live-mod\`.

## 2. Reproducing what was run

**Frozen checkout** (still present; remove at your convenience):
```powershell
# worktree already exists at C:\Dev\KingmakerGunslingerLab\wt\audit-candidate (detached aa91f4657)
git -C C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger worktree list
```

**The broad offline gate** — reproduces the 2,942/0 result and the exact package:
```powershell
cd C:\Dev\KingmakerGunslingerLab\wt\audit-candidate
.\scripts\Invoke-KmgGate.ps1 -Level Tranche `
  -ReferenceBundleDir 'C:\Dev\KingmakerGunslingerLab\private\extracted-references\KingmakerGunslinger-private-build-references' `
  -KingmakerInstallDir 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker' `
  -MSBuildPath 'C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\MSBuild.exe'
```
> **Prerequisite (GS-AUD-0005):** a fresh checkout **fails 23 tests** until you copy in
> `artifacts\inspection\bodyguard-native\Assembly-CSharp.il` (+ `.res`). Verified provenance:
> SHA-256 `2AA7DC0BF64D791FE5DD75F9C7A2C3224B3BFBC0B21AB233FD81E461EE5FE50B`, header MVID
> `{07FA1E4D-8618-41B3-9B8D-FAA17D3B26F7}` = the qualified reference assembly.
> Also note: do **not** pipe the gate through `Tee-Object` — PowerShell 5.1 turns the Python
> runner's stderr into a terminating `NativeCommandError`. Redirect at the process level.

**A native profile run** (the composed UMM + profile transaction this campaign used):
```powershell
C:\Dev\KingmakerGunslingerLab\audits\KMG-AUD-20261111T170737Z\adapter\Invoke-AuditProfileWithUmmProtection.ps1 `
  -RepoRoot 'C:\Dev\KingmakerGunslingerLab\wt\audit-candidate' `
  -ProfileId 'gunslinger-only' -Scenario 'mod-load-smoke' `
  -PackagePath 'C:\Dev\KingmakerGunslingerLab\wt\audit-candidate\artifacts\local-runtime\0.0.182\aa91f46571eb\KingmakerGunslinger-0.0.182-local-runtime.zip' `
  -ResultPath '<out>.json' -RuntimeTimeoutSeconds 300
```
> **Do NOT pass `-FixtureFault umm-completion-fails`** — that case is not offline and launches a
> real transaction (GS-AUD-0022). Run **one scenario per process** (GS-AUD-0038).

**Recovery, if an owner is ever interrupted** — both proven working in this campaign:
```powershell
.\scripts\Restore-UmmParams.ps1 -ReceiptPath '<exact receipt>' -Confirm:$false
.\scripts\compatibility\Restore-KingmakerCompatibilityProfile.ps1 -RunId '<exact runId>' -Confirm:$false
```

## 3. The intake baseline — restore to THIS, not to "the current release"

| Surface | SHA-256 |
|---|---|
| installed KMG DLL (**0.0.117**) | `FD2FC61C250B13857D81ACC197A896450F5B242EE392FA7192B01201E908F35F` |
| `FeatureModules.json` | `A06601C52F1B98AC54EED309F7415677A3C55FE4C51DAA2556DDE5206C687F17` |
| `KMG_Firearms.bnk` | `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18` |
| UMM `Params.xml` | `5F7B96B4F1BC1313EB169F06951E3C98448D2A1E0F3358CC84E351E4FC4B3747` |

## 3b. Revision 2 — corrections the owner review forced

Read `AUDIT-SUMMARY.md` revision note first. In short:

- **GS-AUD-0030 (Nimble) and GS-AUD-0027 (RU version pin) are WITHDRAWN false positives.**
  Do not action them. Acting on the Nimble one would have broken correct code.
- **GS-AUD-0038's shared-process explanation is withdrawn**; cause unresolved.
- **GS-AUD-0004** is restructured: current adjudication first, the withdrawn causal analysis in a
  clearly marked historical section.
- **GS-AUD-0015, 0025, 0031, 0034, 0019, 0006, 0001** were narrowed or re-grounded.
- The unfinished source rows (scatter, Shield Other, Protection from Alignment, Magic Circle
  lifetime and vendors) were **completed**, producing GS-AUD-0039..0043 and 36+
  correct-with-file:line rows.
- Native follow-up, stated accurately: **four** disposable/observe scenarios plus **two** profile
  startup smokes in T6, then **four** more in T7 (the settings observer plus three A20
  presentation/audio scenarios). The earlier wording "five previously unassessed native
  scenarios" was a miscount.

## 3c. Revision 3 — the second review

- **`GS-AUD-0040` reclassified to documentation drift.** `planning/EXPANDED-SUMMONING-MISSION.md:71`
  already approves a distance-independent established bond. **Do not reintroduce the distance
  check and do not change the regression test** — only the shipped ability description is stale.
- **Coverage ledger reconciled** with the rev-2 withdrawals; `VERIFIED` rows fell 6 → 3 because
  the Magic Circle rows had been verified on an inventory-only observer.
- **Disposition is now `IN_PROGRESS_CHECKPOINT`**, not complete. See `SUMMARY.md` §6a for the
  explicit remaining queue.
- **GS-AUD-0044** (Dead Shot audio commits) is new and is the second-highest repair priority
  after the scatter sign.

## 4. The smallest next investigations, in value order

0. **GS-AUD-0039 — the scatter sign inversion.** No run needed: the full
   constructor-to-`OnTrigger` chain is disassembled and carries **no clamp**, so the +2 result is
   established. The worthwhile check is a disposable scenario asserting
   `Attacks[0].AttackRoll.AttackBonusPenalty == 2` (positive two), which would also pin the sign
   by behaviour instead of by the constant's literal value.
0b. **GS-AUD-0044 — Dead Shot audio commits.** Re-run `disposable-firearm-wwise-audio` with the
   committed-discharge counter logged as an explicit before/after pair, so the delta is recorded
   rather than inferred from the `:N` diagnostic suffix.
1. **Firearm cold-load token survival** (GS-AUD-0021) — load a firearm, damage it to Broken,
   save, **exit to desktop**, relaunch, reload, inspect rounds + degradation. One disposable
   save, one cold restart. This is the largest hole in the evidence.
2. **Brown-Fur unresolved caster** (GS-AUD-0015) — the consequence is **UNVERIFIED**, so the run
   must establish the **precondition first**: instrument or observe whether
   `sourceContext.MaybeCaster` is actually null at the restoration point when the caster is in
   another area. Only if it IS null does the +4-instead-of-+6 result follow. A bonus of +4 alone
   does not confirm this finding — it could have other causes — and a bonus of +6 does not clear
   the guard, only that scenario.
3. **Settings migration write failure** (GS-AUD-0006) — mark `FeatureModules.json` read-only on a
   `schemaVersion < 12` file and launch. If the log says "malformed … all modules ON", confirmed.
4. **Template leak** (GS-AUD-0025) — scope is **Summon Monster callers only** (Naturalist,
   Feral Hunter and Preservationist are Nature's Ally routes and are not affected). The run needs
   a **specific affected SM caller, carrier, tier and creature** whose native carrier actually
   carries an inherited celestial/fiendish template buff — that tuple has not been enumerated.
   Then inspect the spawned unit's facts for the donor template.
5. **Shield Other range** (GS-AUD-0040) — cast it, separate the pair beyond close range, deal
   damage, and observe whether the split still occurs.
6. **Area-load path** (GS-AUD-0024) — only worth running once the original log exists.

## 5. Decisions only the owner can make

- ~~**GS-AUD-0030 — Nimble**~~ **WITHDRAWN.** There is no decision here. Tabletop Nimble *is*
  2/6/10/14/18 (`private/rules/GUNSLINGER_PFSRD.md`); the implementation and the `EXACT` label
  are both correct.
- ~~**GS-AUD-0040 — Shield Other's range contract**~~ **RESOLVED, no decision needed.**
  `planning/EXPANDED-SUMMONING-MISSION.md:71` already settles it: the established bond is
  distance-independent by approved design. Only the shipped description needs correcting.
- **GS-AUD-0011** — is Deadeye's flat 1-Grit cost intended? The per-increment cost it computes is
  dead code.
- **GS-AUD-0013** — should a *successful repair* clear misfire suppression? Today only a player
  click does, which strands AI-driven companions.
- **GS-AUD-0007** — should a future-schema settings file disable the **whole mod**? It does today,
  with no in-game explanation. Rollback after a bad update is ordinary on Nexus.
- **GS-AUD-0032** — the level-17 Gun Training dead pick: add an option, collapse to three, or at
  minimum document it in `KNOWN-ISSUES.md`.
- **GS-AUD-0041 — Protection from Alignment.** Reconcile the 23-entry catalog with the 21-row
  authority table, and decide whether the Suggestion entry belongs under the control-immunity
  rule at all given the doc's own exclusion list.
- **GS-AUD-0004 / 0016 / 0018 / 0033 / 0042** — latent consistency deviations with no
  demonstrated failure. Worth fixing on principle; none is a release blocker on current evidence.

## 6. Prerequisites this campaign could not obtain

- The original **2,680,892-byte `output_log.txt`** from the crash. Nothing can attribute the
  incident without it; it is not on this machine.
- A **legitimate older save** carrying Brown-Fur facts / summon Sunder state / firearm tokens.
- An **authorized kingdom-stage fixture** for Better Vendors (the standard fixture is pre-kingdom).
- A **non-detached fixture** for Gunslinger's Dodge.
- **Human/controller observation** for presentation, audio and input — unreachable unattended.

**Not prerequisites, simply not done:** A09 (autonomous summon AI), A20 (presentation/audio),
A21 (performance) and A22 (upgrade/persistence cross-check) were never scheduled. They are
recorded as NOT_RUN rather than blocked, and `observe-feature-module-settings` remains eligible
once its thirteen Boolean module parameters are built from the catalogue.

## 7. Scaffolding this campaign created

- Worktree `C:\Dev\KingmakerGunslingerLab\wt\audit-candidate` (+ its local `GamePath.props` and
  the copied IL dump). Remove with `git worktree remove` when no longer wanted.
- `C:\Dev\KingmakerGunslingerLab\compatibility-state\audit-umm-params\` — completed receipts.
- The campaign directory itself.

**Untouched throughout:** the user's primary checkout and its uncommitted work (modified
`scripts/Backup-Live-Mod.ps1`, untracked `.scratch-persistence-scope-draft/` and
`scripts/Prune-RuntimeArtifacts.ps1`), the nine other worktrees, and all 118 saves.

## 8. Nothing in this campaign repaired, published or uploaded anything

No product file was modified. No PR was merged. No issue was opened. No log, save or private
reference left this machine.
