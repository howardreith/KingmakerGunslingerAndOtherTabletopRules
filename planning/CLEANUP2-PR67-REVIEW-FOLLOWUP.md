# PR 67 review follow-up

Development checkpoint, 2026-10-22. PR stays draft. The mission, production
attack qualification, giant differentiation and owner art approval remain open.
The reviewed input was aea5a8ae5553650602b23aa6d5284f957ac1a525.

## Confirmed tooling defects

The original PowerShell wrapper now owns the live save handles and transaction
lease until a precise process query confirms Kingmaker has exited. Evidence
write or process-enumeration errors cannot unwind that owner. Restoration runs
after confirmed exit; the original failure still exits nonzero.

Disposable process-boundary tests actually start the standalone wrapper and a
separate stand-in process. They prove write/delete/rename denial and retained
transaction ownership before exit, then exact restoration and original wrapper
exit. The three cases cover ordinary waiting, waiting-evidence IO failure and
process-query failure continuing beyond stand-in exit until the query recovers.
Final results: 70/70 PASS on Windows PowerShell 5.1 (13.065s) and PowerShell 7
(12.330s). No real installation, registry, game or save is used.

Frost is rejected anywhere in a requested batch before package/install/save
entry, with the existing retained-native giant-lineup route identified. The
14 advertised generated targets use the actual shared catalog resolver.
CurrentStack behavior: 124/124 on PS5.1 (15.227s), 124/124 on PS7 (11.900s).
Named resolver cases: 2/2; exact production compile 16.1s. Safety commit:
7fb9ac54a251621e3e7ae8d4ba88754d7ee8d45e.

## Review10 artifact and native evidence

Source: 3f782d4cb8b69b9d43247c0ac972c3454fa60d6a.
Source-state SHA256: 5e72ccc6b8dc785b687cec63e4ca4626876cd7019efb985bf14eed57acda1b95.
Package SHA256: d67912b70ad96715809da3ee523497d5a74d339d56691c13a2ccd7e3a0bce981.
DLL SHA256: 5118f52b819bfff1886f4fa50e1c06ca76c5c26e5639adc1e509981da263f45e.
MVID: 3ff81984-b0a0-4422-b783-446f76bcf5e4.
Purpose/version: summon-cleanup-iteration2 / 0.0.163, development only.
Exact production compile, strict package and 83/83 selected domain cases PASS
(4.3s runner). Final phase preflight 828/828 PASS.

| Phase | Run | Result |
| --- | --- | --- |
| Production attacks | 20261022T1231203057818Z-6abe5596ec7c4720a1acdeaf9a1b7bb2 | FAIL, 11 PASS / 2 FAIL, 109.411s |
| Mastodon and canonical Triceratops shared controls | 20261022T1236053125583Z-1ac4b7c8506648b4a02a4471ea4c3d6f | PASS, 24/24, 74.828s |
| Ettin lifecycle | 20261022T1241398095815Z-0d0db554918c445dacedb457a4ef01dc | FAIL, 8 PASS / 2 FAIL, 89.776s at initial head/material assertion |

Raw evidence remains in the correspondingly named save-free Cleanup2 directories
under C:/Dev/KingmakerGunslingerLab/runtime-evidence. All three guarded
transactions completed restoration, confirmed native exit and verified all
95 original saves unchanged. No Working save was loaded or written.

The attack failure exposed an incorrect minimum-delay assumption in the new
driver/observer. Kingmaker's HasAnimation=false branch legitimately acts on its
first eligible Tick despite storing ScheduleAct(1). Exact source/collider and
zero-offset skin proofs passed, but the old association gate refused the real
act. Brachiosaurus was not reached. The failed record is retained.
Correction 532e9391ee43ff02ef385c604b634fc67c0f3d4d reads the native branch,
handled transition and exact first-tick/delta association without delaying or
forcing combat. It captures ready-before-command, actual act and recovery.
Its new native reproduction is still required; no production animation PASS.

Source11 focused qualification completed: exact installed-reference production
compilation PASS (11.8s), named filter expanded-summoning.cleanup2, 27/27 cases
PASS (domain build/run3.0s; runner0.5s). DLL SHA256
157738fd6ce53c71d0fef00dae15818f6d507b8540bb044d4ee22a2e9434489f,
MVID30f36765-6678-4150-8119-454e47911d05. Frozen worktree:
artifacts/worktrees/summon-cleanup2-review11. Log:
artifacts/cleanup2-review11-focused.log in that checkout.
No source11 package or native run exists. Its attempted candidate pipeline
passed repository validation, then failed domain compilation because this new
checkout lacked ignored GamePath.props (Newtonsoft reference unresolved).
After copying the existing local configuration, the focused gate above passed.
The failed candidate log is retained as cleanup2-review11-build.log. Do not
describe this as a successful source11 immutable candidate build.

The shared controls now pass their exact source/view/material/mesh/action/scale
preservation predicates. This closes the prior failed-before-spawn technical
gap for Mastodon and Triceratops. Native pixels still need owner review.

Ettin's exact two-driver/four-material attachment and original texture ownership
were present. The live dissolve matched the donor at the initial sample
(0.008244276, maximum error zero), but the full cached fade-baseline predicate
was false. Execution stopped there. Neither subsequent changing expiry fades
nor cold reconstruction is qualified. Do not turn this partial match into PASS.
All four cached dissolve values were also zero, matching the captured native
baseline. The differing field is not recorded by this run. The next narrow
observer change must expose field-level cached baseline differences and texture
instance IDs, including attach-time versus current identities; forcing fades or
dropping the full comparison is not justified.

The Human pre-upload CPU reference contract reader initially failed on its
expected-negative exception wrapper. Reader-only correction retained the same
candidate DLL and passed 34/34 in 0.297s. This proves the no-game contract,
not native giant measurement. Giant calibration remains empty and palette
application remains an experiment.

## Visible work and remaining checks

Review10 adds warmer Ettin skin paint, controlled material adoption, original
render-only Crab/Brachiosaurus gestures, weights-only Brachiosaurus tail repair,
and darker Gorgon plate paint. Geometry/resource identities and deterministic
generation are documented in the dedicated material, attack and Gorgon notes.
No new native Gorgon paint or giant scale-prototype run was selected before the
bounded checkpoint ended. Ettin material baseline needs a narrow diagnosis and
correction before retry. Cold harness work remains a local unpublished draft;
legacy cold entrypoints still refuse this current-stack mission.

Owner acceptance remains required for Griffon intersections, Stag Beetle contact,
Girallon far fingers, Ettin face/neck, Elephant/Stegosaurus proportions/materials,
Gorgon appearance and surrounding-icon comparison. No blanket art approval.

## Local gallery and elemental presentation

Owner requested local review only; no upload or remote reviewer is needed.
Open artifacts/cleanup2-portable-review-gallery-v2/index.html in the integration
worktree, or extract artifacts/cleanup2-portable-review-gallery-v2.zip.
188 original PNGs, seven identity/result groups, 182,743,270 bytes; ZIP SHA256
eb112333d06becfc9662a8766f2071dbefe29b3d5735f4817a4ebb3d577a17c4.
This packet covers the recorded review5–9 evidence, not the new review10 work.

[Option B screen design](CLEANUP2-OPTION-B-SCREEN-DESIGN.md) specifies one
desktop Spells-tab elemental chooser with explicit original casting-source
and resource choices. Original abilities, spellbooks, preparations, items and
metamagic contexts execute. Domain/item/legacy exceptions retain access.
This is a design proposal; all six legacy ranks remain visible.

## Validation boundary

Ordinary focused window: 12:23:34–12:53:34 UTC, 2026-10-22, following
docs/VALIDATION-POLICY.md. No new live task is selected at its ceiling.
The final focused gate was started before the ceiling; its recorded completion
was12:53:39.662 UTC, six seconds beyond the planned ceiling. No further checks
or native task were selected. Restoration always completes. Release, broad current-stack gameplay, alternate
callers, old-save surfaces and final art acceptance are deferred explicitly.
