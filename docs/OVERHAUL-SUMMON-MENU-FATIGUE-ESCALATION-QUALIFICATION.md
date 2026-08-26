# Overhaul, summon-menu, and fatigue escalation qualification

## Candidate identity

- Branch: `codex/overhaul-summon-menu-fatigue-escalation`
- Authoritative baseline:
  `970aeb7972dcb155d5789a636ba156e68d1c0d0a`
- Runtime-qualified code commit:
  `4d60cc4b8f968072f7becfacf1657f5b44a1bb20`
- Version: `0.0.103`
- Source-state SHA-256:
  `7D04CB53CAFCBE7D719AF021DBBC0AC1C26A8202D226DECD92A0F1D04D9F1F29`
- Deterministic package SHA-256:
  `7D104DD2454E9158FF8DC39DFC2731A5619CFDC55192194CBA22A03F2B1F51A9`
- DLL SHA-256:
  `7C5AAC8EBCF040F9937878150204DAA1ABC5F5C18D788579939ACF86EC0A01B6`
- DLL MVID: `87208f20-6452-4018-ad2d-b0612e3b144b`

Generated packages, deployment files, saves, compatibility snapshots, and raw
runtime logs are excluded from the source commit.

## Automated source and package gates

- Focused overhaul, summon-menu, fatigue, Acadamae, Cord, Expanded Summoning,
  maintenance, persistence, compatibility, and runtime-safety tests: PASS as
  part of the complete suite.
- Version-aware repository validation: PASS.
- Complete dependency-free domain/reflection suite: PASS, 1,278/1,278
  (derived deterministic test count `1278`).
- Clean exact-reference Release, build-output, firearm asset, SoundBank,
  deterministic package, and strict standalone-package validation: PASS with
  zero compile warnings or errors.
- Runtime-scenario preflight safety suite: PASS, 147 checks.
- Identical-input package reconstruction: PASS; both archives produced the
  package hash recorded above.

## Guarded runtime

Every real launch used the repository's guarded `-kmgRuntimeTestRequest`
mechanism through Steam App ID 640820. Every final run reverified the exact
`4d60cc4b8f968072f7becfacf1657f5b44a1bb20` deployment, package hash, DLL
hash, MVID, and version before launch. Save-backed runs named only
`KMG_AUTOMATION_WORKING`; none targeted `KMG_AUTOMATION_BASELINE`.

| Scenario or configuration | Guarded run ID | Assertions | Result |
| --- | --- | ---: | --- |
| `disposable-overhaul-maintenance` | `20260826T0707415039849Z-b371696252aa420685c394c5440ae3c4` | 12 | PASS |
| `disposable-fatigue-escalation` | `20260826T0710113137823Z-ca644ff4389b46c7b7e698ea0dc9dcad` | 11 | PASS |
| `disposable-acadamae-graduate` | `20260826T0712222091136Z-3097f8ec4c864df9ac6edaeddca7e9de` | 20 | PASS |
| `disposable-cord-of-stubborn-resolve` | `20260826T0714556893117Z-3f749c4f35894eaeae3424ca1a5f3ff2` | 12 | PASS |
| `observe-expanded-summoning-inventory` | `20260826T0721073348067Z-c24d0cfd7da944ceb89443d07f786789` | 38 | PASS |
| `disposable-expanded-summoning` | `20260826T0725027650723Z-f68df60a65e3420f9f758eecf404009f` | 12 | PASS |
| `disposable-expanded-summoning-player-path` | `20260826T0728018151151Z-3828b2f5758440b2926794a4f88b6cbd` | 10 | PASS |
| `disposable-expanded-summoning-visual-contracts` | `20260826T0738032502508Z-6c50f1be0a6b449795e02495da565681` | 13 | PASS |
| `working-save-expanded-summoning-prepare` | `20260826T0740481657597Z-86a305d98efd4691a9ab8c592e87b92d` | 9 | PASS |
| `working-save-expanded-summoning-verify-cleanup` | `20260826T0743288809663Z-ff29cd504c26421e8136e46dd573395e` | 9 | PASS |
| `working-save-expanded-summoning-verify-absent` | `20260826T0746077068028Z-e06e6ed351e7476e9ff157150b5f97fb` | 9 | PASS |
| `working-save-fatigue-prepare` | `20260826T0748458413126Z-0fe17a8af7f64089b4bbc25f70e0c2f8` | 7 | PASS |
| `working-save-fatigue-verify-cleanup` | `20260826T0751167060504Z-06b7a5b9ad644c9c8d198adcfdd15356` | 7 | PASS |
| `working-save-fatigue-verify-absent` | `20260826T0753485399175Z-4e42697af04a4d5592a08497daebc1b2` | 7 | PASS |
| `working-save-smoke` | `20260826T0756232268147Z-c075a85e75124737a706085e78a61075` | 11 | PASS |
| feature modules all enabled | `20260826T0759125719847Z-eff0d0e0fe054f05a58fbe70557468fe` | 13 | PASS |
| Gunslinger, Acadamae/Cord, and Expanded Summoning disabled | `20260826T0801230569689Z-b653985794264bd199a436288451d208` | 13 | PASS |
| Call of the Wild present | `20260826T0804232568857Z-fd1453b37a2947f3a1da29f9c473eb52` | 17 | PASS |
| Call of the Wild absent (`gunslinger-only`) | `20260826T0806514821546Z-362c2e0aa1d84c868f2564d7186dea16` | 16 | PASS |

The overhaul run used the production ability and proved availability out of
combat, one prompt delivery with no pending iterator, unchanged `GameTime`,
the same runtime firearm changing Wrecked to Broken, one kit consumed, no
ammunition change, changed-context failure, idempotence, combat rejection, and
one concise combat-log result.

The canonical fatigue, Acadamae, and Cord runs proved fresh Fatigued, repeated
Fatigued to one Exhausted fact, already-Exhausted stability, same-frame
determinism, immunity and cancellation boundaries, duration preservation,
independent/rest-removable Acadamae context, one Cord substitution and damage
event, no recursion, no double penalties, and the 1-HP floor. The three-launch
fatigue sequence then proved exact save/load persistence, native rest cleanup,
and absence on a final no-write reload.

Expanded Summoning runs proved all 153 production commands, converted-ability
and spell-slot behavior, exact option publication/order, alignment/template
contracts, actual creatures and effects, and native summon duration. Installed
2.1.7b adds a six-second lifecycle grace to the requested 120-second duration;
the final harness captures `RuleSummonUnit.Duration` and `BonusDuration`, then
verifies that exact canonical `SummonedUnitBuff` rather than an appearance
buff. The three-launch sequence proved the exact canonical fact persists,
cleans up, and remains absent.

The module matrix restored the original `FeatureModules.json` bytes and SHA-256
`28B9589DB49EF977D2A033AA563052930A1D0E37E920689DB746BD0AF9108B59`
after each restart. Both optional-mod profiles restored the installed mod set,
Call of the Wild settings, and feature settings exactly.

## Diagnostic outcomes not counted as qualification

The first final inventory observation used the older default 120-second
orchestration timeout. The game remained responsive and committed a late PASS
after 206.7 seconds, but the orchestration result was ERROR and is not counted.
The evidence-supported rerun used a 300-second bound and passed normally under
run `20260826T0721073348067Z-c24d0cfd7da944ceb89443d07f786789`.

An untouched-baseline run
`20260826T0640593621603Z-3a3148e71d9e49f2b33681d594a68f87`
reproduced the stale runtime assertion that expected the summoned-unit fact to
expire within 121 seconds. Exact installed IL and the feature run showed that
native `RuleSummonUnit.OnTrigger` adds six seconds to the canonical lifecycle;
the assertion, not production summon mechanics, was corrected. A pre-commit
proof run `20260826T0657543481748Z-e86f1110a88f4162bb3d582dcc7ea155`
and the final immutable run both passed the corrected exact-fact contract.

## Evidence boundary

Pure geometry tests cover top, middle, bottom, narrow, ultrawide, UI scale,
oversize, short-native, repeat-open, third-party-height, and navigation policy.
The adapter and supervised observer are source- and build-qualified, but no
autonomous test may place an icon and open the actual player popup. Therefore
`observe-expanded-summoning-variant-menu` remains a deliberate supervised,
read-only gate: a human must load `KMG_AUTOMATION_WORKING`, place the largest
list near the top-left sidebar, and open it. The observer never sends input,
selects an option, casts, or writes the save. Actual rendered bounds,
mouse-wheel behavior, and controller focus remain the only uncertainty.

## Human acceptance

1. Equip one empty Wrecked firearm and possess one Firearm Repair Kit.
2. Out of combat, activate **Overhaul Firearm**.
3. Confirm it completes promptly without waiting one game minute.
4. Confirm the same firearm becomes Broken and exactly one kit is consumed.
5. Confirm no campaign or game-time minute was visibly advanced.
6. Enter combat and confirm Overhaul Firearm cannot be used.
7. Place a high-tier Summon Monster spell near the top of the left sidebar.
8. Open its largest variant list.
9. Confirm every option remains on-screen or reachable by scrolling.
10. Confirm the first and last options can both be selected.
11. Confirm a short native variant menu still looks and behaves normally.
12. With Acadamae Graduate active and the caster not Fatigued, fail the save
    and confirm Fatigued.
13. Fail another Acadamae save before resting and confirm Exhausted.
14. Confirm another fatigue application does not duplicate or downgrade
    Exhausted.
15. Repeat the relevant fatigue cases while wearing the Cord of Stubborn
    Resolve and confirm its established substitution occurs exactly once.

## 0.0.103 acceptance-repair cycle

The installed candidate at branch HEAD
`7f7f4fbba27bea03ecccfd0badda1267aec44bfe` failed the first human popup
presentation check: at 1600x900, a largest-list parent in the first left
sidebar slot opened a bounded popup several slots lower around mid-sidebar.
Human testing also found the legacy UMM misfire button no longer reliably
created Broken and then Wrecked state on the visibly equipped production
firearm. These failures do not invalidate the earlier overhaul, summon
mechanics, fatigue, persistence, module, or optional-mod results above; this
section records the additive acceptance repair.

The popup adapter now captures the exact `ActionBarGroupSlot` that invoked the
shared native Toggle, preserves native opening direction, clamps only the
nearest crossed vertical edge, and applies the target by translating measured
rendered bounds rather than assuming a pivot-space origin. Oversized scroll
content is top-aligned and spans the complete safe vertical range. The observer
now records actual source slot, safe area, rendered popup, viewport, first/final
slot, scroll state, and top/bottom clamp state.

The UMM panel now uses separate deterministic Break and Wreck controls. They
require exactly one selected party unit and one current-hand production
firearm, reject combat and ambiguous targeting, invoke canonical transitions,
and verify the same item/repository identity plus the real before/after state.
The updated guarded overhaul fixture calls the exact same development-control
bridge before handing the resulting Wrecked item to the production Overhaul
ability.

Source qualification for this repair is PASS at 1,288/1,288 tests. The
implementation commit is
`ca0eedb5ef79aa81ffcb03026827fbf35f1636aa`; the request-local runtime-fixture
correction and final runtime-qualified code commit is
`a1f9e5e26ce6b8c0bc00625ff337181a25e30fe6`.

The final clean exact-reference Release candidate has source-state SHA-256
`02CC8D8AD9243D6A557020A17054598B5A21D7782AD0297C7463F92CCC64D79C`,
DLL MVID `d7448fb4-023c-4ba3-85de-ddcd0f13fd9a`, deterministic package
SHA-256
`CCE8DB03CDDADAED7BE5E8B34502C0031233CC7C3BF2F61193B22AC7A1609E9B`,
and DLL SHA-256
`888C50711F0605C94BD84CF76B0E2467C0D95A0DD41EACFCE5448348541A5EC9`.
An identical-input reconstruction produced the same package hash and passed
strict validation. Repository validation, clean Release, exact installed
reference compilation, build-output validation, firearm asset and SoundBank
validation, deterministic packaging, and strict standalone-package validation
all pass. Runtime-scenario preflight passes 147/147, and the supervised menu
observer's `-WhatIf` readiness check passes without deployment, launch, input,
or save access.

| Acceptance-repair scenario | Guarded run ID | Assertions | Result |
| --- | --- | ---: | --- |
| `disposable-overhaul-maintenance` | `20260826T1209541091175Z-0913defa04e34613a8e656e80aadcda8` | 16 | PASS |
| `disposable-expanded-summoning` | `20260826T1212352963057Z-aded88f462f14c7ebc545992e18501f4` | 12 | PASS |
| `disposable-expanded-summoning-player-path` | `20260826T1215494490608Z-2b64a72c89dd4410b7185748f85d0f40` | 10 | PASS |
| `disposable-expanded-summoning-visual-contracts` | `20260826T1225404869508Z-011811f4df3747438fd15b3f7aa5447f` | 13 | PASS |
| `observe-expanded-summoning-inventory` | `20260826T1228429573111Z-b98236aae6e54f29a92be0e8aadb0283` | 38 | PASS |
| `working-save-smoke` | `20260826T1232570621893Z-60132354b52f422dadfd8eb6f79dc61f` | 11 | PASS |

Every real launch used the guarded request mechanism through Steam App ID
640820 and reverified the final commit, source state, package, DLL, MVID, and
installed DLL before launch. Save-backed scenarios named only
`KMG_AUTOMATION_WORKING`. The 16-assertion overhaul run proves the real UMM
Break/Wreck bridge, same-item/repository identity, invalid-repeat rejection,
zero ammunition/kit consumption by the diagnostic transitions, production
Overhaul recognition, prompt completion, unchanged `GameTime`, combat
rejection, and exact cleanup. The four Expanded Summoning runs retain the
previous inventory, mechanics, player-path, option-order, and visual-contract
proof on the corrected adapter.

The first attempted final overhaul run
`20260826T1202534929483Z-8394b35448304dacbb21ead16101dbe9` is not counted: it
failed before assertions or mutation because the disposable main-menu fixture
had no native PC `SelectionManager`. The final scenario creates one hidden,
non-saved exact native component request-locally and destroys it after restoring
selection; production selection behavior was not relaxed. An immediate first
147-check preflight invocation also observed artifact-tree drift while the last
runtime cycle was finishing and was not accepted; the stable no-process rerun
passed all 147 checks.

The exact package is installed at
`C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger`.
Its installed DLL SHA-256 is
`888C50711F0605C94BD84CF76B0E2467C0D95A0DD41EACFCE5448348541A5EC9`,
byte-equal to the qualified build. The summon popup is not visually accepted
by automation; renewed human review of this exact installed candidate remains
required.

## Final timeboxed popup acceptance attempt

The renewed human check again displayed native overflow. The corresponding
installed-candidate log proves that source-slot capture and the exact Toggle
patch both ran, but `SummonVariantMenuPlacementPolicy` rejected the rendered
root before translating it because its post-Unity dimensions were not equal to
the requested dimensions within 0.01 canvas units. The exception was repeated
for each open and the intentional failure cleanup restored native layout,
explaining the apparent reversion.

The final adapter uses actual rendered dimensions when calculating the
translation. A top-clamped decision aligns the rendered top edge exactly to the
safe top; a bottom-clamped decision aligns the rendered bottom edge; ordinary
placement remains nearest-edge clamped. The adapter continues to fail closed
if the rendered root is materially larger than the complete safe rectangle,
and it verifies the resulting rendered rectangle remains within that safe
rectangle. The existing focused rendered-bounds test now reproduces
fractional size drift that previously threw. Repository validation and the
complete 1,288-test suite pass, as do clean Release, output, asset, SoundBank,
package, and strict-package validation. Final in-game appearance remains a
human acceptance boundary.
