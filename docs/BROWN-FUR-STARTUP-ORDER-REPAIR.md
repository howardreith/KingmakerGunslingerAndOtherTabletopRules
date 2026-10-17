# Brown-Fur startup-order repair — tested development candidate

Current outcome: missed startup retry repaired; focused source checks and one
guarded native load/cast probe PASS. Reconstruction of previously serialized
Brown-Fur owners, the affected character's load and controller-mode loading
remain unqualified. The sections below retain checkpoint chronology; this
outcome and the final evidence section supersede earlier NOT_RUN entries.

## Scope and attribution (2026-10-17)

Branch `codex/brown-fur-startup-order-20261017`, isolated worktree
`C:/Dev/KingmakerGunslingerLab/worktrees/brown-fur-startup-order-20261017`,
base `2eed3944f6eab657f9030d33acae6d81eb5f10ae` (remote master matched at intake).
No default-branch merge, publication, permanent installation or save editing is
authorized. Buff Planner and other mods are outside the repair scope.

The supplied `C:/Dev/KingmakerBuffPlannerLab/handoff/output_log.txt` is preserved
privately under `artifacts/brown-fur-startup/input/`. Its SHA-256 is
`4035f106b1ef30fb167517911abb3e8b2c9e99c80b8d78185ba83112784da6e6`.
The supplied `KingmakerGunslinger-0.0.157-alternate-summoning-sources.zip`
hash `d2e6489277b56fb8a2b1a949fde6ddec8ddb1c40e1fcda145b7a2296db0d32a5`
matches the released archive, product
`8726ec737a73f9a0956f3ae9cbab2aceeb7ffc7c`.
Its DLL hash is `b5ac181083843268075503481f8a82589a4efb7496c563616ca631b6ffc67562`,
released MVID `0105e3cb-ac91-45aa-8137-ac74c3239390`.
This attributes the supplied package, not an independently hashed installation
on the affected computer. The affected log reports Kingmaker 2.1.7b, UMM
0.33.0.0, CotW 1.14.4c-2.1, KMG 0.0.157, and Buff Planner
0.5.4-controller.1 / product `9aec44e`.

## Demonstrated defect and repair

The log records early first-update contract rejection (line 302), then CotW
creation deferred for the unavailable KMG library (1235), KMG library commit
(5250), and no subsequent Brown-Fur registration. At 9679 the Strength
activatable GUID `16c06d016437be9e9e6dac6211ff30a5` cannot resolve; at 9682
`Player.PostLoad_Patch2` throws. Missing identity is confirmed. Its causal
relationship to the PostLoad exception is **not yet verified**. No evidence
attributes the scheduling change to Buff Planner or UMM.

Readiness now stays pending without running the compatibility resolver too
early. A synchronous notification after `BlueprintBootstrap` commits its real
library retries pending work before `TryInitializePending` returns. The other
boundary remains CotW's exact `createArcanistClass` postfix. Thus whichever
dependency finishes last performs reconciliation, not an arbitrary later frame.
The one-shot UMM fallback remains a fallback, not the only retry opportunity.
Completed/rejected/reentrant states do not register twice or create retry loops.
After an actual completed CotW creation, missing Arcanist still reaches the
unchanged structural rejection rather than waiting forever.

Existing manifest registration and rollback, contract fingerprinting, all 25
stable identities and module-OFF registration versus selector publication are
preserved. Before committing the optional set, validation checks every frozen
GUID against the actual library dictionary, its expected object and type, and
exactly one all-blueprints entry. No placeholders, exception suppression,
deserialization patch or GUID changes are introduced.

## Evidence and limits

- `Invoke-KmgGate.ps1 -Filter 'brown-fur.startup'`: 6/6 PASS, production
  compilation included; 9.672 s, `artifacts/brown-fur-startup/initial-focused.log`.
- `Invoke-KmgGate.ps1 -Filter 'brown-fur,feature-settings.brown-fur-status'`:
  83/83 PASS of 2,652 registered, production compilation included; 2.111 s,
  `artifacts/brown-fur-startup/neighbors.log`.
- New behavior tests cover update→CotW→library, CotW→library, library→CotW,
  duplicate notifications, reentrancy, deferred commit, genuine contract
  rejection, the missing stable identity/type, and pending presentation.
  These execute the production readiness gate and existing pure contract
  policy, **not Unity blueprint creation or native loading**.
- Exact local Assembly-CSharp hash
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`,
  MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7` (also recorded by the supplied
  log): inspected native `LibraryScriptableObject.LoadDictionary` populates
  its dictionary synchronously. The existing lifecycle postfix calls
  `Observe`→`TryInitializePending`; the new notification runs after package
  commit inside that call, before the postfix returns. This is assembly/source
  contract evidence, not a demonstrated native load on the candidate.
- No repeated discovery/frame retry, PostLoad patch or global input changes.

## Native plan and acceptance boundary

Owner authorized DATA for an exclusive guarded disposable-fixture test on
2026-10-17. Only existing repository orchestration, leases, current-version
development-candidate identity, transactional staging and exact restoration may
be used. Baseline and ordinary saves remain protected. Independently snapshot
and restore the actual human installation; never infer its bytes from a version
string or replace it permanently.

Before running, inspect the selected launcher and nested staging/request/restore
contracts and `-WhatIf`. Prefer the Brown-Fur working-save prepare/cold-verify
scenarios only if their development-candidate handling and safety prerequisites
are satisfied. Record the exact native Load Game receiver route and input mode;
do not call a main-menu startup a save-load success. Check all 25 actual types
before dependent deserialization, then inspect retained Brown-Fur facts,
activatables and resource ownership after cold load. If PostLoad still fails,
capture actual Harmony owners and narrower provenance without swallowing it.

At this checkpoint native order, native 25-type resolution, cold fixture load,
affected-character load, input-mode equivalence and restoration are NOT_RUN.
Packaging still needs the documented development validator repin from released
0.0.156 to released 0.0.157, with corruption fixtures. Keep that tooling change
separate from the source repair. A development ZIP must retain distinct purpose,
commit, receipt and hashes; it is not a new public release.

## Candidate/probe preparation follow-on

Source repair is preserved at `6912c86503`; development validator repin at
`ab68d92c08` (14 corruption tests and 16 unchanged release-validator tests PASS,
6,895 released identities, no appends). The first build refused an incomplete
shared reference bundle; a separate branch-local export from the installed
game supplies all 14 exact references, without changing the shared bundle.
A second attempted invocation stopped on redirected Python unittest stderr;
the corrected invocation retains normal child-process error/exit semantics.
The next build correctly refused the unlisted bootstrap delta in the protected
icon catalog. Its existing exact-hunk feature-extension mechanism now declares
only the synchronous callback; baseline/art/mapping pins remain unchanged.
All 30 icon corruption tests PASS, including removal/substitution of that
exact notification and continued rejection of unrelated/mapping mutations.

The historical eleven-mod installed-stack wrapper cannot represent current
DATA. The new `Invoke-BrownFurStartupProbe.ps1` preserves all sixteen current
installed mod directories and their exact files through the existing locked
compatibility transaction, rather than choosing a subset or replacing other
mods from examples. It accepts only the attributed CotW version, explicit
working-save name and exact development purpose. All pre-existing saves,
including WORKING, receive OS read leases. It uses the existing guarded native
Load Game route and disposable Brown-Fur cast scenario, preserves native
registry settings, and restores the original Mods/SoundBank transaction.
It does not set controller mode or alter UMM, bindings, drivers or display.
Observer evidence records `Game.IsControllerGamepad` and the 25 actual resolved
types. That post-load dictionary is supporting evidence; the synchronous
registration validation/log is the earlier pre-load boundary.

Probe protocol fixtures: 8/8; reused installed-stack restoration fixtures:
30/30; protected-save catalog fixtures: 6/6. These do not launch the game.
The read-only probe cannot prove reconstruction of persisted Brown-Fur owners
on cold load: that requires the separate prepare/cold-verify protocol or an
authorized affected-state fixture. Keep that gap explicit even if the probe
passes. Current DATA intake now reports KMG Info 0.0.157; do not reuse the
historical 0.0.140 banner as current identity.

## Final candidate and native evidence (2026-10-17)

Source repair checkpoint: `6912c86503`. Frozen product/harness commit:
`913dfd6e89386ff233849f06f902ca8ea2de239e`. Current documentation is a subsequent
record-only checkpoint, not a rebuild or new native observation.

Local package (also copied byte-for-byte to
`C:/Dev/KingmakerBuffPlannerLab/handoff/`):

`artifacts/development-candidates/brown-fur-startup-order/913dfd6e8938/KingmakerGunslinger-0.0.157-development-brown-fur-startup-order-913dfd6e8938.zip`

- Version metadata remains `0.0.157-alternate-summoning-sources` under the
  existing development-only mechanism; **not a new release**.
- ZIP SHA-256 `d7cf7aa3cbae1a3ff76a73d01ad302448e907be6da92fbe5b0a94569ca91f33e`.
- DLL SHA-256 `f878c8c3e36264b9f34d0441443a26c50da5ea939490d03e264f30164731afbb`.
- DLL MVID `88974741-ddbe-4fcb-8f1c-145305534eaa`.
- Adjacent `.development-receipt.json` records exact references, clean source
  fingerprint, validation, purpose and selected tests; `.sha256` also copied.
- Final `Build-Local.ps1 -DevelopmentPurpose brown-fur-startup-order
  -DevelopmentTestFilter 'brown-fur,feature-settings.brown-fur-status'
  -ReferenceBundleDir <branch-local export>`: PASS in **61.341 s**. Mandatory
  repository wrapper, exact .NET 4.7 / C# 7.3 build, selected **83/83** tests,
  37 assembly-backed companion checks, output/audio and strict package checks.
  Log `artifacts/brown-fur-startup/candidate-qualified-harness-build.log`.
- Earlier successful candidates `e3bb1b6e88bf` (64.470 s) and `f394120170f0`
  (66.549 s) were not runtime-qualified. The latter's handoff copy is archived
  under `handoff/brown-fur-superseded-f394120170f0/`; use **913dfd6e8938**.
- Final mutation-free `-WhatIf` PASS after fixing provider-preference leakage
  and sharing all phase bounds between real preflight/launch. Probe protocol
  **11/11**, unchanged transaction fixtures **30/30**, protected-save fixtures
  **6/6**, Steam source tests **47/47**. Protected-baseline/wrong-purpose,
  unfamiliar mod, mismatched CotW, ambiguous identity and out-of-range timeout
  refusals are covered by the focused probe fixtures.

The owner-authorized command was:

```powershell
./scripts/Invoke-BrownFurStartupProbe.ps1 -PackagePath <exact package above> `
  -DevelopmentPurpose brown-fur-startup-order `
  -SaveName KMG_AUTOMATION_WORKING -TimeoutSeconds 300 -Confirm:$false
```

Probe wall time **99.829 s**, fresh Steam App 640820 process PID 4132.
Native scenario run ID `20261017T2101467133518Z-92560c397d6c4e269ed1ad43e00d7222`.
Runtime result **PASS, 10/10 assertions**, no non-PASS assertions. The result's
duration field is 54,620 ms; its start/end stamps do not describe that full
elapsed interval, so use the external probe wall clock for total duration.
DATA Windows lab, installed Kingmaker 2.1.7b contract, UMM 0.33.0.0,
CotW 1.14.4c-2.1; all
sixteen installed mods retained, including the lab's existing Buff Planner
0.1.1-rc3 (not the affected computer's 0.5.4-controller.1). No input-mode switch
was performed: the actual observer records **`controllerGamepad=false`**.

The action was **Load Game**, not New Game or Continue: guarded native
main-menu button → exact working SaveSlot → MainMenu.LoadGame. The structured
`workingSaveSmoke` evidence records the normal load chain, completion callback
and stable post-load fingerprint. It is programmatic invocation of the native
receiver, **not physical controller or human menu-input evidence**.

| Observation | Status / boundary |
| --- | --- |
| Early-update→CotW→library, reverse order, repeated events and rejection | PASS in focused production-gate/policy tests; other orderings not forced natively |
| CotW defers, then KMG library commits and synchronously registers | PASS natively: `registration.complete checkpoint=package-blueprint-library-ready`, 25 identities/type checks; one registration/publication |
| Exact missing Strength GUID plus remaining expected types | PASS actual registration validation before load; observer independently lists all 25 actual resolved full types after load |
| Native working-save load | PASS callback and stable fingerprint; no missing-blueprint or PostLoad error in this fresh log |
| Native Brown-Fur behavior after load | PASS actual disposable casts, combined reservoir 4→2 / slots 6→5, ally +6 Polymorph Strength, shared spells, ordinary/+6 enhancement cases, interruption with no spend, cleanup |
| Saved Brown-Fur owners / affected character cold reconstruction | NOT_RUN: supplied evidence has no affected save; current working ZIP does not contain the missing GUID. Do not infer PostLoad causation or persisted-state repair from this pass |
| Controller-mode load / physical input / Continue / New Game | NOT_RUN; no equivalence claim |
| Module-OFF native persistence | NOT_RUN; existing source publication/rollback tests PASS |
| Restoration and save protection | PASS: all 95 pre-existing save files unchanged, no new/owned save path, full original Mods and managed SoundBank restored, native registry settings exact, process exited and lock released |

Private evidence:

- `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261017T2101466987536Z-disposable-brown-fur-native-cast/`
  (`runtime-result.json`, `brown-fur-native-cast.json`, loaded build identity).
- `runtime-result.json` SHA-256
  `2c9a1ddfced4d70ac538eac3c826eca6b37e1259d19017cb195209871af2ec4c`.
- `brown-fur-native-cast.json` SHA-256
  `f39f71c24fea9a3c313175a1f127f10e7edf625504c7eb9a818c5754e7b2a9bd`.
- Transaction `C:/Dev/KingmakerGunslingerLab/runtime-evidence/brown-fur-startup-20261017T210120Z-e66c29d8c8e1/`
  (`probe.json`, `preexisting-saves.json`, `save-preservation.json`);
  corresponding compatibility-state transaction is `Restored` / verified.
- `artifacts/brown-fur-startup/native-output_log.private.txt`, SHA-256
  `4a787da6f19ef7dc2e3ade8047b84b7a7b8b7fd3e992f7f4606dcf5e2ecb14bd`.
  Lines 1117 deferred, 5145 exact Strength registration, 5158 completion,
  5159 publication, 5160 compatible. Original supplied log remains unchanged.
- Original DATA KMG DLL restored to
  `00ef67570d30e4ff9c54a2ad2027393660436d5724bcee2bb99e4c5718e966fd`.

Next smallest acceptance step: supply an authorized Brown-Fur-bearing disposable
fixture / authorize the existing prepare→fresh-process verify protocol with
current-stack transactional protection, or test the affected save on its test
computer without saving over it. Record Load Game versus Continue and actual
input mode, then capture the log and inspect retained Brown-Fur ownership,
activatables and reservoir. If PostLoad still fails **after** all 25 resolve,
capture actual Harmony PostLoad patch owners and exception provenance; do not
assume the missing GUID was the sole cause. No native retry, merge, tag, public
release, permanent installation or save edit is performed by this handoff.

## Preserved checkout boundary

The original repair worktree at
`C:/Dev/KingmakerGunslingerLab/worktrees/brown-fur-startup-order-20261017`
is parked on `codex/brown-fur-startup-order-product-913dfd6e89` at the exact
product commit. Its artifact receipts remain usable without bypassing source
identity guards. Review/documentation branch
`codex/brown-fur-startup-order-20261017` is checked out separately at
`C:/Dev/KingmakerGunslingerLab/worktrees/brown-fur-startup-order-review-20261017`.
Continue review or new repairs there; do not alter/relabel the frozen candidate
or inherit its native PASS onto a new binary. No additional runtime run is
selected by creating these checkouts.
