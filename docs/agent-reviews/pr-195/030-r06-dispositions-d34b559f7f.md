<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=r06-dispositions-d34b559f7f part=1/1 sha256=d34b559f7fbdfb86ff8c3c4894f8b69f35a9070ab506f1b39a681178216c05ec -->
### Claude Opus — finding dispositions — round 6

Candidate after corrections: `7f44ae6e74155cc012670812a65507e0b25a8d8d`

- **R6-F1** — `fixed` — commits: `dfc86851a8`, `fe837b5007`, `7dc8979540`
  X6-1 (harness-result 30a5953c…): native StandardLongspear BaseDamage 2xD6 with override True after registration; foundation (6edc216d…) and named Boughkeeper (4a084b02…) override False, 1xD8; startup complete, 'item damage is invalid' x0.
- **R6-F2** — `fixed` — commits: `dfc86851a8`, `fe837b5007`
  X6-2 (a79f5b22…): an empty live Enchantments for the owned spear only (field intact) failed startup once with `movement-enchantment [effective-getter] … getter expected=KMG_ElvenBranchedSpear_MovementOpportunityAccuracy actual=<none> donor=<none>; getterPatches=KmgLoaderCompatHarness`; selectors restored.
- **R6-F3** — `fixed` — commits: `dfc86851a8`, `fe837b5007`
  X6-3 (18be2a83…): Simple appended only to the spear's GetSubCategories list failed startup once with `subcategory-list [effective-getter] … actual=Melee,Finessable,TwoHanded,Exotic,Metal,Simple … getterPatches=KingmakerGunslinger,KmgLoaderCompatHarness`. 'subcategory-policy [effective-getter]' x0 (membership still rejects Simple); selectors restored. Domain case returned-subcategory-list passed.
- **R6-F4** — `fixed` — commits: `0a926ce1d8`, `7f44ae6e74`
  The record's opening lists the current product dfc86851a8 and package 0a926ce1d8bd first, then the round-5 (195d17ce19/18361607eda8) and round-2 (cc8da0e71f/8ee7eabaeda0) products and packages, labelled by round, with the hashes recorded in §4.2.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/versions/34fb852974476cfbd48374fc676c822b4e2f97911cee6706a79161c7e9d9d43c/agent_review.py verify --enrollment kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 --gate focused -- -Filter elven-branched-spear | yes | pass | 1.8 s | Candidate 7f44ae6e7415: exit 0, worktree_unchanged true. Production compile plus 29 elven-branched-spear domain tests; supporting evidence only. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1 -DevelopmentPurpose spear-bootstrap-tooltip-repair -DevelopmentTestFilter elven-branched-spear -ReferenceBundleDir … -LoaderReferenceDir … | yes | pass | about 6 min | Candidate 0a926ce1d8bd: ZIP 83c936d5…, DLL 5e7d69d1…; strict UMM package validation passed. |
| Invoke-LoaderCompatSession.ps1 (item-enchantment-list, simulated, subcategory, reporter-stack on candidate 0a926ce1d8bd) + -Report for each | yes | pass | about 25 min total | Disposable copy, UMM 0.31.1. The reporter stack uses four of the reported mods, not the reporter's full installation. Restoration fields true in every session. |

_Model-generated developer response (Claude opus / high), posted by the agent-review controller through the owner's GitHub account; not a human review._
