# Sprint 5 closure checkpoint (S5 T1 to S5 T6)

Status: **Sprint 5 is accepted and formally closed for its reviewed development scope** (principal review,
2026-11-20). Acceptance is not merge or release. At the time of this record nothing had been merged, released,
tagged, published, installed into the human game or sent for review, and every pull request below was an open draft
with no reviewers. Every limitation recorded below is unchanged by the acceptance.

Each story keeps its own product and evidence identities. No earlier session ran against the final Mastodon package,
and the records-only heads were not rebuilt or rerun.

## Accepted stories

| Story | PR | Accepted head | Disposition |
|---|---|---|---|
| S5 T1 / DEF-05 | #186 | `f59e3ed811a772c2d82ee33cc0b5eac28c6a96d8` | Inherited-template cleanup derived from the owned summon graph |
| S5 T2 / DEF-06 | #187 | `bbc8174f056eacd3c1957edd0b17db197a91ccd2` | Controller Elementals launcher consolidation; aggregate FAIL sessions retained |
| S5 T3 / POL-04 | #188 | `ada0ff2650d49db9c3bf5ddba0d8979d8b966417` | Brachiosaurus A-records scope decision |
| S5 T4 / POL-05 | #189 | `acd50a042a2c3494c4503470f6ce14f37f5352d2` | Twelve approved native-wrapper exclusions |
| S5 T5 / POL-06 | #190 | `c2867a3aa1216c1cd842b7acf644421208e234e9` | Prepared-caster Call of the Wild conversion projection |
| S5 T6 / RISK-06 | #191 | `9d2f5e7c4f2f23aed47d01cc9bdb33ae7551b920` | Autonomous Mastodon approach Trample |

The Sprint 5 stack sits on the closed Sprint 4 head `5c662d50b0` (PR #185). This closure record is a records-only
commit on PR #191's branch, after the accepted S5 T6 head.

## Product and evidence identities per story

All native sessions ran in the disposable loader-compat game copy (UMM 0.33.0 owner profile, game 2.1.7b) with
harness `c47e7f14d48edac88740b4c9bf1841e805c45a88043dc162789b4c86dbc1b089`.

| Story | Product-build commit | Package SHA-256 | DLL SHA-256 | MVID | Accepted sessions |
|---|---|---|---|---|---|
| S5 T1 / DEF-05 | `8ceb08ccef29f1029618ee86e4797f40480ec8d8` | `3ef1bce7027b0c168dddd9f3042cfc65e7a107ad7382b508075c5d08b0427ccb` | `e0dadbc9c854768bd83711324403fc9f5d61f82c8572b9b844249e5a0f14cc35` | `3806eb49-4581-4784-a01d-99c7cfec0659` | `20261119T201715Z-owner-0.33.0-inherited-template-census` PASS; `20261119T201938Z-owner-0.33.0-inherited-template-casts` PASS (15 of 15) |
| S5 T2 / DEF-06 | `f91a80144707fa9e81486bbcea05145e0d1ed9b1` | `6e23d2eb5776d97d5f5bf14aaacf5cda567f309b1dfbf77cfaced36454045481` | `76e0aaad6ef362358ff571bf42a302212eb0faa9aa205f23bd7325af0f931307` | `23a5bfc6-76d3-4e57-85fc-436397b97684` | `20261119T213730Z-owner-0.33.0-def06-console`: every S5 T2 observation passed, **aggregate FAIL** (pre-existing console-learning assertion); matched S5 T1 baseline `20261119T213956Z-owner-0.33.0-def06-console` fails it identically |
| S5 T3 / POL-04 | none (tooling and records; no product built) | — | — | — | — |
| S5 T4 / POL-05 | none (tooling and records; no product built) | — | — | — | — |
| S5 T5 / POL-06 | `c5fdcb1431c25acfac48dfc73b617ef94bc1af51` | `2fd49eaadb1ca6d4a1b873a563b9e3684707d4e2f1cff684cc782c8b24bebb72` | `bff80052bfb76d7d29731946a85c1141ea604d014220564431e7e2b429e97645` | `6d4e9a1b-2ae4-441f-b739-924dc535d0bb` | `20261120T054533Z-owner-0.33.0-pol06-lane` (the `summon-pol06-cotw` lane), original in-game verdict PASS, 77 of 77 |
| S5 T6 / RISK-06 | `203b605c108c490a9df57d3d5f3eff739c7d49bc` | `7391dc81475df8a01bd4d0ec929d4006f72d6c08f926280e7e17bf14458e1c48` | `d900c900ed8a04d2a4150e2716d8e2c7882719fdf61c6cf764d189ad95a97fc7` | `5b0eb1b8-17f5-4062-bbd0-c99aada6b56d` | `20261120T162705Z-owner-0.33.0-risk06-trample-ai`, PASS 28 of 28 |

Story records: `docs/EXPANDED-SUMMONING-INHERITED-TEMPLATES.md` (S5 T1),
`docs/EXPANDED-SUMMONING-CONTROLLER-LAUNCHER-CONSOLIDATION.md` (S5 T2), `docs/EXPANDED-SUMMONING-BRACHIOSAURUS-SCOPE.md`
(S5 T3 and S5 T4), `docs/EXPANDED-SUMMONING-COTW-CONVERSION-SEMANTICS.md` (S5 T5),
`docs/EXPANDED-SUMMONING-MASTODON-TRAMPLE-ROUTE.md` (S5 T6).

### Preserved failed and superseded candidates and sessions

Every one stays under its own identity, as recorded in its story record, including:
- S5 T2: `20261119T212359Z` (MAX_PATH fixture failure; no lane ran) and `20261119T212635Z` (observer failure on
  candidate `779ee6620ed7`), plus the two aggregate-FAIL sessions above;
- S5 T5: lane sessions `20261120T050759Z`, `20261120T051940Z` and `20261120T053352Z` (FAIL; observer and lane defects)
  and the full-phase sessions of the first observer, including `74d01069713a`'s in-game FAIL beside its re-read PASS;
- S5 T6: investigation sessions `20261120T135147Z` and `20261120T135607Z` on `bc92642e1907` (FAIL, observer teardown),
  `20261120T140731Z` on `921a952978ce` (investigation PASS, 12 of 12), and candidate `09c1f5925efb` (superseded,
  never run).

## Reviewed scope, with the principal review's qualifications

- **S5 T1:** the inherited-template removal list is derived from the owned summon graph. The quantity is proved
  through one controlled d4 result; the retained baseline lacks historical donor identities.
- **S5 T2:** the controller Elementals launcher follows an actual row consolidation. The console-learning assertion
  (the hidden-option seam still lists one original Summon Elemental) is **unclassified**, kept separate and not
  investigated.
- **S5 T3 and S5 T4:** Brachiosaurus stays Nature's Ally only (Summon Monster VII single, VIII 1d3 and IX 1d4+1
  approved out of scope); Redcap, Axiomite, Soul Eater, Manticore and Mite keep base-plus-two ladders (twelve higher
  1d4+1 cells approved out of scope). Placement ledger: **1,208 admitted, 9 withheld, 0 in-scope missing and 15
  approved exclusions against the original 1,232-placement ideal**, which stays incomplete.
- **S5 T5:** prepared-caster scope only; spontaneous casters are outside it. Pinned-slot debit is exact only for the
  unique-copy fixture; equal preparations follow the native spend; the cursor explanation for the full-phase guard
  aborts remains an unattributed hypothesis. The projection, raw fallbacks, popup size, the same creature at two
  tiers and the duplicate-looking rows are retained presentation limitations.
- **S5 T6:** the summoned Mastodon's own AI uses its existing Trample as a conservative approach maneuver. The review's
  three qualifications:
  1. **Stale plans.** Revalidation at command creation is implemented and source-reviewed. Policy and segment
     decisions are domain-tested. Stale dispatch and queued-command behaviour were not separately observed. When the
     plan no longer holds at creation, the fallback is an ordinary attack against the previously selected target; it
     does not request fresh native target selection, and no reselection path is claimed.
  2. **Death.** The Mastodon was killed after its Trample started and before its first contact. That shows
     interruption without contact and without a claim being created. It does not show removal of a previously
     established claim after death.
  3. **Inventory.** All 32 expected Mastodon outputs resolved and none was unreadable in this frozen session. The
     observer checks that expected set only: it does not scan for unexpected extra outputs, and an unreadable
     alternate graph is reported without failing the verdict on its own. It is not an exhaustive bidirectional census.

  Not newly qualified: summon expiry; a stopping attack of opportunity during an AI-issued Trample; the threatened
  case in turn-based combat; stale dispatch or queued commands; ordinary menu selection, slot debit or summoning
  action cost; existing saved summons acquiring the new brain; other tramplers; the owner's full installation. The
  unstarted attack-command end event after turn-based Tramples is an unexplained log observation, not an extra
  attack.

## Checks

- **Complete domain suite (inherited result): 3,147 of 3,148.** The single failure,
  `model-d.ItemIdentityEconomicsEnchantmentsAndCraftingUnchanged`, pins
  `src/KingmakerGunslinger/Blueprints/ProductionFirearmBlueprints.cs` to its 0.0.142 hash; RISK-01 (`0c0a211e62`,
  S4 T5) changed that file. Both the file and the pinning test are unchanged from the accepted S5 T5 base (same Git
  blobs). It is not relabelled PASS.
- Closure checks (records only): text, path, link, commit-identity and consistency checks and `git diff --check`. No
  product rebuild, broad domain rerun, compatibility matrix or native rerun was performed for this closure.

## Carried obligations

Release-validator and release-notes obligations, unchanged:
- account for the approved Gunsmith's Kit manifest note (INT-07, row 206);
- the next release notes must state that v0.0.128's "Recovery alone grants no new order" is superseded (POL-01);
- **DEF-05:** the next release validator must replace the v0.0.191 validator's obsolete inherited-template
  creature-list expectation.

Every native, presentation, save and compatibility limitation recorded in the story records stays in force.

## Next

S6 T1 / REP-05, the retained Tenebrous loading incidents (`docs/GUNSLINGER-SAVE-LOAD-FORENSICS.md`). Sprint 6 had not
started at this closure.
