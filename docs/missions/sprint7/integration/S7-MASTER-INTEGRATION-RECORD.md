# Sprint 7 master integration record (development checkpoint)

**Status: dated pre-submission snapshot, 2026-11-28.** This page states the integration as committed and the checks
selected for it. It does not predict a review outcome. The focused gate results are recorded in §5 for the head named
there, and the controller's receipts on the submitted head are cited in the developer report.

Mission: [`S7-MASTER-INTEGRATION-MISSION.md`](S7-MASTER-INTEGRATION-MISSION.md) (the owner's authorization of 2026-11-28,
verbatim). This is a separate enrollment with at most four formal reviews. It does not resume, amend or extend the
completed parent enrollment of PR #219.

## 1. Identities

| Item | Identity |
|---|---|
| Starting `master` | `52755b617c79328a972ae5b74bc05784422e332d` (fetched 2026-11-28; equal to the owner's reviewed target, so `master` had not advanced) |
| Accepted parent, reviewed H | `3ebdda17dac096156974a118474649070b68521c` (PR #219), the integration input |
| Accepted parent, archive A | [`d682b6bd4e443e685257a12eb24b00878f9c4114`](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/d682b6bd4e443e685257a12eb24b00878f9c4114/docs/agent-reviews/pr-219), the immutable audit record. Its 43 `pr-219` files are **not** imported |
| Merge base of H and `master` | `35763430acc9eefc306b861045526a444690bc77` |
| Integration branch | `codex/s7-master-integration` |
| Integration merge | `f5adee695a8d7a448a145a169ebe201dfff11ba0`, parents `52755b617c` (first) and `3ebdda17da` |
| Reconciliation commits | none. The merge was textually clean and needed no source edit. The commits after it add only this record, the mission and the status-page pointer |
| Merge base of the integration head and `master` | `52755b617c` |

Accepted child archives, unchanged and cited by immutable links:
- C1 #221: [`2c67a049ee`](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/2c67a049ee33e0b29621fd29a8b25608ed31e956/docs/agent-reviews/pr-221).
- C2 #222: [`eab90a5ac8`](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/eab90a5ac82f661ecb903c5456d951a57a085eb9/docs/agent-reviews/pr-222).
- C3 #223: [`3bfc570090`](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/3bfc570090b9772cac4b92e79983d31455510bca/docs/agent-reviews/pr-223).
- The C3 manifests:
  - [recovery manifest](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/3bfc570090b9772cac4b92e79983d31455510bca/docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md);
  - [evidence manifest](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/3bfc570090b9772cac4b92e79983d31455510bca/docs/evidence/sprint7/recovery-c3/MANIFEST.json).
- The parent's dated record, cited and not rewritten:
  [`docs/GUNSLINGER-SPRINT7-RECORD.md` at H](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/3ebdda17dac096156974a118474649070b68521c/docs/GUNSLINGER-SPRINT7-RECORD.md).
  It stays as the parent committed it, including its own pre-submission status lines.

## 2. Protected transcript-path preflight

Checked with `git diff --name-status <from> <to> -- docs/agent-reviews` before any gate ran:
- `35763430ac..3ebdda17da` (the Sprint 7 side): empty. The 51 `pr-195` files were already at the merge base and are
  unchanged; the parent's `pr-219` records exist only at A.
- `35763430ac..52755b617c` (the `master` side): adds the 48 `pr-220` files.
- `52755b617c..f5adee695a` (what this integration adds to `master`): empty. The new enrollment inherits no foreign
  transcript addition and does not modify any of `master`'s archive records.

## 3. Overlap and how both contracts are kept

The merged tree differs from `master` in exactly the 302 paths that the Sprint 7 side changed (`35763430ac..3ebdda17da`).
Each of these carries H's bytes, except the four shared paths. Each of `master`'s 166 changed paths keeps `master`'s bytes,
except the same four. The four shared paths received both sides' hunks, which do not touch the same lines:

| Path | `master` (#220 and the 0.0.196–0.0.197 work) | Sprint 7 |
|---|---|---|
| `scripts/validate-package.ps1` | `-PassThru`; the package path resolved once; strict archive-entry extraction (`Expand-KmgPackageEntries`); the packaged-DLL identity returned by `Assert-KmgPackagedLoaderContract` | the repaired asset pins: Nalfeshnee mesh `6ff85493…` and albedo `1e9f5303…`, Salamander human mesh `0837e796…` |
| `src/KingmakerGunslinger/KingmakerGunslinger.csproj` | `UnityModManagerDir` set to the pinned cache `artifacts\references\loader\UnityModManager`; the `KMGPIN` missing-reference errors; the `VerifyPinnedLoaderReferences` and `AssertPinnedLoaderReferencesResolved` targets; four Elemental compile items | four compile items: `SummonMovementTargetTooltipPatch.cs`, `DustMephitBreathPresentation.cs`, `RuntimeTestRunner.Sprint7Presentation.cs`, `Sprint7ObserverPolicy.cs` |
| `tests/.../KingmakerGunslinger.DomainTests.csproj` | `ElementalPersistenceTraitCoverageTests.cs`, `LoaderReferenceContractTests.cs` | `Sprint7VisualRepairTests.cs`, and links to `Sprint7ObserverPolicy.cs`, `TrumpetCueVerdict.cs` and `GhaeleAttribution.cs` |
| `tests/.../Program.cs` | four `elemental-traits.persistence-coverage-*` cases; `loader.production-dll-references-pinned-loader`; the `NOT APPLICABLE` outcome and `RunSelection.Focused` in the runner | the 25 `sprint7-visual.*` cases |

- **Sprint 7 content kept.** All product code, assets, compile items and registered tests from H are present: 25
  `sprint7-visual` cases are registered in the merged `Program.cs`. No gameplay behaviour, provisional audio choice,
  visual asset, blueprint identity or save contract was changed.
- **Version metadata.** `Info.json`, `Directory.Build.props` and `Properties/AssemblyInfo.cs` are byte-identical to
  `master`: version 0.0.197. Sprint 7 changed none of them. There is no downgrade to the parent's historical 0.0.195 and
  no new release version.
- **#220 contracts.** These files are byte-identical to `master`: `scripts/loader-reference-pins.json`,
  `scripts/Assert-PinnedLoaderReferences.ps1`, `build.ps1`, `Build-Local.ps1`, `common.ps1`, `package.ps1`,
  `ReferenceProvenance.Common.ps1`, `DevelopmentCandidate.Common.ps1`, `Test-CandidateAdmission.ps1` (which hold the
  embedded source-stamp checks, `AssemblyMetadata` `GitCommit`), and the loader-compat runner and admission scripts. The pinned-loader compiler targets and
  the strict archive-entry and packaged-DLL identity checks are present in the merged shared files above.
- **Consequence for Sprint 7 code.** At the merge base, `Build-Local.ps1` already compiled candidates against the pinned
  loader files, but the focused gate's `build.ps1` compile used the project's default `UnityModManagerDir`, which was
  the installed loader folder. The parent's focused gates therefore did not compile Sprint 7 code against the pins.
  In the merged tree every compile, the focused gate included, uses the pinned Unity Mod Manager 0.31.1 files under
  #220's verification and compiler reference targets. Both selected gates exercise this (§4).

## 4. Integration risks and gate selection

These selections run through the controller's `verify` on the committed head, with production compilation:

1. **`verify --gate focused -- -Filter sprint7-visual`.** The merged production DLL compiles against the pinned loader
   with #220's reference targets. The merged domain-test project compiles, and the 25 Sprint 7 cases run through
   `master`'s changed runner. Those cases include the linked loader-compat harness files (`TrumpetCueVerdict.cs`,
   `GhaeleAttribution.cs`) and the observer policy.
2. **`verify --gate focused -- -Filter loader.production-dll-references-pinned-loader`.** The merged production DLL,
   bound by path and SHA-256 to the gate's own compile, must reference exactly the pinned loader assemblies.

Evaluated and **not run**, because the merge changed none of their inputs:
- **`native-profile`** (9 `expanded-summoning.native-profile-*` cases). `ExpandedSummoningNativeProfilePolicy.cs`,
  `Sprint7ObserverPolicy.cs` and `ExpandedSummoningNativeProfileTests.cs` are byte-identical to H, and `master` changed
  none of them. The changes they share with the merged tree are the domain-test project, `Program.cs` and
  `Assertions.cs`, which both selected gates compile and run. The native profile runner itself is a runtime path, and no
  native session is authorized.
- **`loader-compat-probe`** (5 cases). `tools/LoaderCompatHarness/ProbeLifecycle.cs` and
  `LoaderCompatProbeLifecycleTests.cs` are unchanged on both sides. `master`'s loader-compat changes are PowerShell runner
  and admission scripts, which are not compiled into the domain tests. The Sprint 7 harness files linked into the tests
  run under `sprint7-visual`.
- **`summon-playtest-r6`** (11 cases). `SummonPlaytestR6Tests.cs` is unchanged on both sides. The production
  `RuntimeTestRunner.SummonPlaytestR6Presentation.cs` is byte-identical to H and is compiled by gate 1. `master` touched
  no summoning source.

Other existing tests read the merged `validate-package.ps1` (the Sprint 13, 14, 23 and 25 summoning tests,
`FirearmAudioTests`, `FirearmFeatIconTests`). They check text that neither side's hunk changed, so they are not
selected.

**A runtime interaction this checkpoint cannot test.** `master`'s loader-compat session runner now admits a candidate
first, with `-CandidateDevelopmentPurpose` or a historical release control. The ten Sprint 7 session files
(`scripts/compatibility/loader-compat/sessions/s7-*.json`) have not run under that runner. This belongs to the
package/native boundary (§8).

## 5. Focused validation ledger (this integration only)

The ordinary 15-minute planning target and 30-minute ceiling apply, and full outer command time is charged. The parent's
final ledger (71 min 40.3 s of 85 min) and C3's ledger are separate, unchanged, and are not this task's budget. No
runtime transaction was in progress at the start.

The controller ran both selected gates on head `28f2408e6fd17e4382994354213f9a327622f965`. That head is the merge plus the
first records commit, and it differs from the submitted head only in this section. Receipts are in the enrollment's
private `evidence/` folder.

| Gate on `28f2408e6f` | Selected | Result | Receipt duration | Outer duration | Receipt (SHA-256) |
|---|---|---|---|---|---|
| `-Filter sprint7-visual` | 25 of 3,218 | 25 PASS, 0 failures; production compiled with `Pinned loader references verified: UnityModManager.dll 0.31.1.0, 0Harmony12.dll 1.2.0.1` | 21.9 s | 23.2 s | `gate-focused-28f2408e6fd1-d88e49a1250-a10f9d5aef1-20261128T132208333466Z.json` (`77d8fa17ad8bf027d461bec71a004248cf0c8655192400597acfd5a1212822f6`) |
| `-Filter loader.production-dll-references-pinned-loader` | 1 of 3,218 | 1 PASS, 0 failures, 0 not applicable | 3.2 s | 4.4 s | `gate-focused-28f2408e6fd1-d88e49a1250-a32501bde64-20261128T132228600439Z.json` (`4bca55da9cea1873dab845edd7bdee04e07d0b9bd02ffbba558a799c6f6e0bbd`) |

- **Charged so far:** 27.6 s of outer time, from 13:21:45Z to 13:22:28Z.
- **What the loader case inspected.** The production DLL compiled by the first gate, SHA-256
  `6d4a643e41ed01dc72d7890777967fdbd37115ec05f98ab60b06f022a6652187`. The second gate's incremental compile left it
  unchanged. This is a development compile output that is checkout-specific, not a candidate or package identity.
- **Gates on the submitted head.** The same two selections run again there, and the developer report cites their
  receipts and adds their outer time to this ledger. The results above are not relabelled as execution on the submitted
  head.

## 6. Inherited evidence: original identities and scope

Nothing below was re-executed on the merged tree, and nothing here is relabelled as execution on it.
- **Recovery candidate 6** is the accepted historical mechanics package:
  - build source `962bf27694a797e7a06d674599dfdbba341b00a7`;
  - package `6c71d928ea4293945af43a4b33f81238047176b07ed10fc88edee342649cd55a`;
  - DLL `6af754eb06f72a261916d2dcd6e5fea125b62b13dcc9eeae53ad000e94341411`.
  - Its qualified session `20261127T133020Z-owner-0.33.0-s7-c3-mechanics` passed 43/43.
  - It **does not qualify the merged tree.** The merged DLL also contains `master`'s code since the merge base and is
    compiled under #220's reference targets, so its bytes are not candidate 6's.
  - Candidate 6's seven mechanics cells are distinct from the seven parent stories.
- **Presentation evidence** stays on its producing candidates (parent record §3): sessions `…050957Z`, `…061437Z`,
  `…162110Z`, `…170720Z` and `…180635Z`, with the per-session scope stated there.
- **C2's Ghaele lifecycle evidence** stays on candidate 3.
- **Scopes carried forward:**
  - Constructed callback overlap, source-derived cancellation guarantees and focused-only transitions keep their stated
    scope.
  - The October Ghaele incident remains unattributed.
  - The accepted controller-rendering and other unexecuted dispositions are unchanged.
- **R9-F1 (parent review 9, P3).** Accepted by the owner as a non-blocking handoff clarification. It is not marked fixed
  or closed. The owner's clarification:

  > The restriction in the parent record’s §3 concerns the model, tooltip and Dust dependency comparison. Session
  > 20261124T050957Z also contains supplementary T1 Ghaele gh/ghd observations and six indexed stills. Those remain
  > evidence on their original candidate. They do not establish the missing released outdoor/bright observation or
  > qualify the later Dust spawn-ownership repair.

## 7. Owner decisions still pending

The provisional cues are unchanged. Existing material is reused, not regenerated.
- **T2 / VIS-01, Nalfeshnee (visual):** [`evidence/sprint7-review/T2-VIS-01/` at H](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/3ebdda17dac096156974a118474649070b68521c/evidence/sprint7-review/T2-VIS-01).
- **T3 / VIS-02, Salamander (visual):** [`evidence/sprint7-review/T3-VIS-02/` at H](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/3ebdda17dac096156974a118474649070b68521c/evidence/sprint7-review/T3-VIS-02).
- **T6 / VIS-05, Dust Mephit breath (visual):** [`evidence/sprint7-review/T6-VIS-05/` at H](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/3ebdda17dac096156974a118474649070b68521c/evidence/sprint7-review/T6-VIS-05).
  These stills predate the round-4 call-site binding (parent record §4.6).
- **T5 / VIS-04, Ankylosaurus (listening):** provisional `Sweetener_Low_06_Quake`.
- **T7 / VIS-06, Trumpet (listening):** provisional `InspireCourage`.
- **Listening packet for T5 and T7:** private, never committed or published. Folder
  `C:\Dev\KingmakerGunslingerLab\private\s7-listening-packet-r5\` and zip
  `C:\Dev\KingmakerGunslingerLab\private\s7-listening-packet-r5.zip`, SHA-256
  `2b5f461050807c6593de62a397e0ac16606f6d952729d999e425c88ddfed2565`. Nobody has listened to it yet.

## 8. The later package/native qualification boundary

Not run at this checkpoint: package build, deployment, game sessions, loader-compat sessions, screenshot or audio capture,
listening and compatibility matrices. Compilation and focused tests here are not native qualification.

Before a packaged/native integration qualification:
- Build one fresh development candidate from the actual merged source, through #220's strict validation and admission
  (`Build-Local.ps1 -DevelopmentPurpose …`). Reuse that unchanged candidate afterwards under its verified identity.
- Run the native checks that boundary selects on that candidate. Loader-compat sessions now go through candidate admission
  (§4).

Before a release, in addition:
- the owner's T2/T3/T6 visual and T5/T7 listening decisions;
- the owner's merge into `master`;
- a separately authorized release with its own version decision.
