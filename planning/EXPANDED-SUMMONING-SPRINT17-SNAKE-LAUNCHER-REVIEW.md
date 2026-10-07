# Sprint 17 snake outer-launcher routing correction

Status: SOURCE PASS; RUNTIME NOT RUN. Full Sprint 17 NOT QUALIFIED.
Classification: orchestration integration defect, caught before deployment.
No gameplay defect or owner-authority blocker is inferred.

## Reproduced defect

Exact candidate d70070a5632bd391d0e5fdf7b267e33b2d8fd1e2 passed all2066
domain tests83.0s, complete source/build/package178.2s and strict320.
Its inner request builder accepted persistenceScope=snakes and the two snake
crowd keys. The real outer Invoke-KingmakerRuntimeTest.ps1 still had its older
crocodilian persistence scope and pre-Sprint17 crowd allowlists.

Actual WhatIf calls rejected both new routes before a runtime lease,
installation observation, snapshot, deployment, game launch or save access.
The new actual-launcher test first failed on this old guard, then passed after
the correction. This is a prelaunch failure, not a failed gameplay batch.

Preserved exact artifact:

- Source fingerprint49097f8030ba9f8d9607479c6825b119ddc37f80e4b0e39d70511846da5d7184
- DLL SHA256b44be2aa14dd7f194062495fb82078339ba77e77d2ef3355a974c2d8e7a4f3b1
- MVID35ad52c5-1430-4814-ab8d-883e2bb188b3
- ZIP SHA25655694f850929cd79439989875b8a26deea814c5cfd46486d243ff0b9f166496e
- Archive artifacts/sprint17-snake-prelaunch-rejected-d70070a5/
- Red/green logs artifacts/sprint17-snake-launcher-routing-{red,green}.log

## Bounded correction and verification

The launcher now permits exactly crocodilians or snakes for the existing
working-save trio and preserves the selected scope when constructing parameters.
The crowd list gains only viper and constrictor-snake. Existing guards for the
typed working save, automatic exit, quantity, parameter count and actual
request preflight remain. No launch/deployment/restore/save sentinel changes.

Test-ExpandedSummoningLauncherRouting.ps1 invokes the actual launcher with
WhatIf:13 valid routes,10 rejected routes. This includes historical default
and crocodilian persistence, both direct/crowd pairs, protected-save rejection,
case/type/scope/quantity/extra-parameter and non-exiting rejection.
AllowDirtyGit applies only to these source-only WhatIf calls, never runtime.
No new evidence directory, staging, lease or Kingmaker process is allowed.
This lab-specific test complements portable request-builder tests; it is not
a game qualification.

Corrected source checks:264 focused,2066 full84.0s,complete180.4s,
repository/static/icon/manifest,clean14-reference Release,strict320 PASS.
Standalone preflight508 PASS. New actual-launcher13/10 PASS.
The assembly/package bytes match d700 before the replacement commit because
only launcher/test/docs changed; exact-source attestation must still be rebuilt
on the new committed head. Do not deploy the superseded candidate.

## Next boundary

Commit/policy-push the coherent NOT QUALIFIED correction, repeat complete
exact-head prelaunch gates including actual-launcher routing, then use one
immutable package for smoke,direct14,crowd18,prepare13,cleanup12,absence5.
The persistence counts are source expectations, not runtime results.
Fresh absence requires a successful native cleanup save.

All prior evidence remains preserved. Latest actual restoration remains
2026-10-06T23:11:14.0412304Z (136 files/Info0.0.117/exact snapshot tree);
these source checks made no later live-install or save mutation.
Sprints14-16 complete;32 snake roots withheld; separate Salamander and full
Sprint17/Phase2B closure still open. PR26 draft/unmerged; DATA ZERO PORTS.
Phase2C authorized but deferred. HumanReview: NOT_PERFORMED_NONBLOCKING.
