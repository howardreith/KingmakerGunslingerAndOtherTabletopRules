# Sprint 16 laptop native-faction candidate, 2026-10-05

**NOT QUALIFIED.** Candidate `bc36f97612020d6d680144bcb6ebc01bebe11216` completed all six guarded
scenarios. The native-faction manual-control repair worked, but the main
scenario failed 188/208. Dire remains withheld; Sprint 17 has not started.

## Ownership and preserved history

The latest owner directive designates HOWARD-LAPTOP and
`codex/expanded-summoning-phase2b-sprints14-17` / draft PR #26 as the sole
development line. PR #27 and `codex/expanded-summoning-phase2b-finalize-20261005`
are frozen salvage-only evidence. No source was imported from that branch.
No branch was merged, rebased, reset, force-pushed or deleted.

The clean active worktree was created directly from fetched `bc36f97612020d6d680144bcb6ebc01bebe11216`
at `.worktrees/expanded-summoning-phase2b-laptop-20261005`. Remote identity
was checked again before launch and after restoration and remained exact.
The prior local `c5058203f51633ed11d15ad35100dc01c53e250c` tip was preserved by
renaming its branch to `codex/local-safety/phase2b-c505-prior-local-laptop-20261005`;
its worktree and earlier recovery bundle remain intact. Additional safety refs:
`codex/local-safety/phase2b-laptop-bc36-intake-20261005` and
`codex/local-safety/phase2b-salvage-8d12-owner-order-20261005`.
All earlier local-safety refs, failed candidates, and `2d334375` remain.

Session source receipt: ignored `artifacts/laptop-source-owner-20261005.json`;
owner PID 31796, start `2026-10-05T19:29:25.9127956Z`, thread
`01a10bcd-8582-7551-826f-0f128807eb99`; exclusive holder PID 15084, start
`2026-10-05T20:34:17.3075720Z`. No other Phase 2B lock owner or game process
was found. Unlabelled terminal processes were not claimed to have known working
directories. Old September 23-27 / October 3 runtime journals retain stale
Active/RecoveryRequired labels, but their recorded owners are gone, they have
no settings/profile obligation, and there was no shared compatibility lock.
They were preserved, not silently rewritten or deleted.

The preceding side-branch transaction was finished and exactly restored before
this branch was acquired; its source receipt was released. That evidence remains
on PR #27 and does not qualify these different bytes.

## Exact prelaunch candidate

| Identity | Value |
| --- | --- |
| Source SHA | `bc36f97612020d6d680144bcb6ebc01bebe11216` |
| Source fingerprint | `ad6825c2648391eed8befa182e2245a89c356a1f502629d75c711e45177f9d36` |
| DLL SHA-256 | `92dc8a4bc941dd5a4f41fbcb749f9459870149c9095fe752a5c4b20d8ec21b52` |
| DLL MVID | `e5c68a9a-dfbc-4e23-a3a2-5327f5aab7c0` |
| ZIP SHA-256 | `416b4c6d8406479fd84cdb509c1ff8ac0fd6eae672b58cdab835475b92b40feb` |
| Version / package members | 0.0.141 / 312 |

Focused 216/216 PASS; unfiltered 2018/2018 PASS (88.8 seconds); repository,
static, roster/manifest and icon validation PASS; clean exact-reference Release,
deterministic package and strict standalone validation PASS. The Sprint gate
completed at `2026-10-05T20:39:27.0213972Z` in 209.8 seconds.
Runtime preflight 480 PASS; orchestration 68 PASS; persistence wiring 11,
three scoped round trips and 19 rejection cases PASS; crowd five round trips
and seven rejection cases PASS. No test filter narrowed the complete suite.
Source checks do not qualify gameplay.

## Six fresh Steam 640820 processes

Every request named only `KMG_AUTOMATION_WORKING`. No direct executable launch,
manual save-file access, baseline load/write, UI input automation, or force
termination was used. Prepare and cleanup each made one authorized native
working-save write; the final process loaded that cleanup save.

| Scenario | Result | Passed / total | Fresh PID / start UTC | Evidence directory |
| --- | --- | --- | --- | --- |
| working-save-smoke | PASS | 11/11 | 21500 / 2026-10-05T20:40:51.4364219Z | `20261005T2040502944415Z-working-save-smoke` |
| disposable-expanded-summoning-crocodilians | FAIL | 188/208 | 21316 / 2026-10-05T20:43:49.4706529Z | `20261005T2043482347104Z-disposable-expanded-summoning-crocodilians` |
| working-save-expanded-summoning-creature-review | PASS | 20/20 | 31612 / 2026-10-05T20:56:01.6359256Z | `20261005T2056007310692Z-working-save-expanded-summoning-creature-review` |
| working-save-expanded-summoning-prepare | PASS | 9/9 | 16504 / 2026-10-05T21:03:37.2678616Z | `20261005T2103363332950Z-working-save-expanded-summoning-prepare` |
| working-save-expanded-summoning-verify-cleanup | PASS | 9/9 | 31184 / 2026-10-05T21:07:02.7082970Z | `20261005T2107017121702Z-working-save-expanded-summoning-verify-cleanup` |
| working-save-expanded-summoning-verify-absent | PASS | 5/5 | 28136 / 2026-10-05T21:10:33.3940394Z | `20261005T2110323476981Z-working-save-expanded-summoning-verify-absent` |

Exact request/result bindings:

| Scenario | Request ID | Request SHA-256 | Result SHA-256 | Orchestration SHA-256 |
| --- | --- | --- | --- | --- |
| working-save-smoke | `20261005T2040503323697Z-5b27818f660441c8b71c933d999759a1` | `664c6d0be6e748f93df4b258d264e4725a9c0c4ab0cb08292fa86a5d5148c15b` | `bb17d410f3cc1ca5255206965f2013417780ffc4a3bd3bbe2dda716b1fe21c29` | `bbe6867631587cced28ac0238c71ccb324c949b08ebd3a551a4f071091b33aae` |
| disposable-expanded-summoning-crocodilians | `20261005T2043482347104Z-7fb6bb7177d143eb8ebfb5ea9e7acf8b` | `efd8bab2c3254f7eada50627c5f6c51bdb5c61642175f78792a83564a64c9df9` | `6e3b03bbc15c2bb1ab5a60098e88d7a6834b2fa9709de693b983c094d22b1632` | `7a6d9bde30eb7cf82cf86c16ee3f53b1ce0ec01be41a96b8266801a9996261f3` |
| working-save-expanded-summoning-creature-review | `20261005T2056007376499Z-8c95581704cc43f7b7efd02386d7d563` | `9e900b76b096583e3e4d7fd37322712da337c20b51278e915756aa55848525b3` | `40893938cfdaeef09df6537f5b37fd1e69a5f3b84ff2cf46e12f9fef4a3eb12f` | `a4af3506ecb118bf5e4ab888d055f1066a7cc8abac5101fb029a0ea154a983ff` |
| working-save-expanded-summoning-prepare | `20261005T2103363332950Z-72a9b171047040eeb2f4ce2bda564633` | `94b9407b002fed9078961ebcbea47161becf414d20d99791cfd429cf694b9d6d` | `9d2513f27ffd7043b7615823f528ed837a3e6adc7c801fd3fa1fe288e275961e` | `161ebea1a189dd720122313bfe752af4fbb6ae35fe0127c02925e7108906d724` |
| working-save-expanded-summoning-verify-cleanup | `20261005T2107017278146Z-e10b0b26473743e7a6b7e0e8fa07824c` | `cffdc3674f7c829e60de7d7838fd07287cd67b918bed5f28915b285e82dbcca0` | `c01895e871edc88d4466d302bceabcfd7d1295c8cc3f55f7d7e4bdfb9ea6d30b` | `f55ddc67bb5a58f2caacabfc0c8c1b289649479322df13a57ea40bc96f626e11` |
| working-save-expanded-summoning-verify-absent | `20261005T2110323476981Z-28fadbd62ee546c1abf591d5c2e8f97a` | `ffd3be231ef0162cfc1db55036d333589e84e694005835ff110571be3c7e3aff` | `ef715a4ecb75df1e682c97d24dbf28b86dcb8fd7101c7c4a2f80ba4d7b130d59` | `c115414a824efee077fedbc80d4238123154cc4bf87baa23b2877e4a3c47b6c8` |

## Failure classification and next engineering boundary

| Class | Failed assertions | Observed evidence and bounded next action |
| --- | ---: | --- |
| Production visual | 8 Dire AI bite/tail contact rows, both modes and AI drivers | Native attacks/AI pass, but weighted-world surface gaps remain 1.04-2.58 m. The pose reports a maximum 0.25 m root approach; bite pre-adjustment gaps reach 2.828 m. Review the creature-owned pose against its Gargantuan footprint/reach, preserving mechanical reach, unit position, target pose and the unchanged 0.25 m contact criterion. Do not widen the assertion. |
| Fixture command sequencing | 1 Dire RTWP manual-swallow row | Swallow and its contact occurred, but the deliberate rejected Sprint command remained None/unacted and manualNativeAttackAttempts was zero. An attack not issued by the manual drill satisfied the current early-completion predicate. Require the actual queued manual attack and completed cooldown rejection; instrument attack provenance rather than assuming an AoO or increasing waits. |
| In-process fixture restoration | 1 native-ui-restoration row | Tabs, sheet, character, section, group and pause restored; selection did not, and the clock advanced 0.040 s. Snapshot/restore native selection at the combat/UI boundaries and restore the exact captured clock; retain the strict equality checks. |
| Fixture scheduling (diagnosis to confirm explicitly) | 10 lifecycle rows | Both active-expiry rows failed to reach native expiry; all eight cooldown boundary rows failed their initial speed-state check, with expiry also unreached. The same helper passes in fresh persistence. Native BuffCollection.Tick uses TurnStartTime and skips off-turn owners/casters in TB combat. The current final fixture does not record that context: record clocks/current turn, establish a deliberate scoped mode, and prove native expiry without direct buff removal or waived assertions. |

Environment failures: none. Live-install restoration failures: none. The UI
restoration failure above is not installation restoration success and remains
a failed gate. No failure is owner-waived.

Bounded positive evidence on this exact artifact: damage/maintain 37/37;
speed/timeline 19/19; icon bindings 9/9; combat/contacts 46/55; final review
69/80; all fourteen existing Crocodile and six private Dire routes passed.
The manual faction observations prove the noncapital native faction predicate
changed from false to true only for the disposable actor, with its summon
identity retained. Both-mode ordinary attacks and AI command rows executed.
These partial passes are not complete Sprint 16 acceptance.

## Exact restoration

Lease `runtime-20261005T204023Z-8c4952571cc0480eb940d6ea31f9fbea`
(owner PID 33592, start `2026-10-05T20:40:14.5552479Z`) was acquired **before**
observing or snapshotting the installation.
Snapshot: `C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20261005T2040235864744Z`.
Deployment: `C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20261005T2040488947683Z/deployment.json`;
deployment receipt SHA-256 `d0f98eff8935883d4f750e96381f8e66772b99ce167962eec8cfd417abcf3192`.

Before and restored after: **136 files, Info version 0.0.117**, tree
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`; Info SHA-256
`1040695b0ae4339b2a935be3fe06828be80a451b4ea86bae11a7c16d6dce5cff`.
The actual snapshot was restored, not a forced historical tree.
Restoration completed `2026-10-05T21:13:38.1407714Z`; lease is Completed with
recoveryRequired=false; no Kingmaker process, compatibility lock or deployment
staging remains. Generated extraction is preserved under this batch's artifact
directory. The temporary candidate installation was removed by restoration;
its exact package and all evidence remain recoverable.

Ignored complete batch journal:
`artifacts/laptop-runtime-20261005T2040228007144Z/batch.json`,
SHA-256 `387FE4AECCB70BC56EABB2271637673111ACE001A9073CB0A9F088606033FC47`.
Prelaunch timing/console evidence stays in `artifacts/gate-timings.jsonl` and
`artifacts/laptop-prelaunch-bc36-captured-output.txt`; request/scenario artifacts
remain in the recorded machine-local evidence directories.

## Continuation

Commit/push this truthful NOT QUALIFIED checkpoint through the policy wrapper.
Then perform the owner's separate read-only range-diff/stat salvage audit.
No side-branch authority text, ownership receipt or qualification claim may be
ported. Continue bounded corrections and a new exact-artifact gate on PR #26;
Sprint 17 waits for complete Sprint 16 qualification and publication.
Phase 2C is authorized in principle but deferred until Phase 2B owner acceptance;
Sprints 18-22 are not started under this mission.

Unchanged: 976 registered / 970 published / 6 Dire withheld / 29 wrappers /
999 visible choices; Sprints 14-15 complete. Preserve all four accepted
limitations/adaptation, including the interior AC/HP omission; do not reopen
or substitute a subsystem for them. `HumanReview: NOT_PERFORMED_NONBLOCKING`.
