# Brown-Fur startup-order repair — development checkpoint

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
be used. Baseline and ordinary saves remain protected. The actual human KMG
installation is not the supplied 0.0.157 package; snapshot and restore it, never
replace it permanently.

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
