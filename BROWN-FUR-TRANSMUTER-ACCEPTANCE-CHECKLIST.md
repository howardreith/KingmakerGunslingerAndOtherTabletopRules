# Brown-Fur Transmuter human acceptance checklist

## 0.0.115 Share Transmutation direct-cast addendum

Status: **AUTOMATED CANDIDATE; LIVE ACCEPTANCE NOT RUN**.

- [x] Contract version 1 binds the exact ability, recipient, transaction, and
  effect-producing rule without constructing a `UnitUseAbility`.
- [x] Provider coordinator tests cover delayed effect completion, exact
  reservoir ownership, four sequential recipients, and safe AbilityData reuse.
- [x] Rejection cannot fall through to an unenhanced cast or leave a later
  spend-suppression marker.
- [x] Version-aware source validation and the full domain suite pass 1393/1393;
  the exact compiled API passes the consumer's 87/87 binary contract checks.
- [x] Exact-reference Release build output and the strict standalone package
  validate; two deterministic packages have SHA-256
  `2490193efc17e6a27b07beaddba71fd149bfe62ef29c4ca167698e52032316f6`.
- [ ] In a guarded live run, cast Resinous Skin from Felix on four willing
  party members in a compatible planner's Instant mode and verify effects,
  spell uses, reservoir uses, and no ordinary animation delay.
- [ ] Repeat in explicit Animated mode and with an ordinary manual cast to
  confirm both remain native and animated.

This addendum does not alter the immutable 0.0.82 acceptance record below.

Status: **HUMAN ACCEPTED / FINAL QUALIFICATION COMPLETE**.

This checklist applies only to the immutable `0.0.82` repair candidate below.
The rejected `0.0.81` candidate remains superseded and was never accepted.
Human presentation and play review accepted this exact source, package, and
installed DLL identity on 2026-08-16. The runtime policy revised at acceptance
makes the completed 16-state boundary authoritative; no exhaustive game-launch
matrix is required.

## Candidate identity

- Branch: `codex/brown-fur-transmuter-cotw-extension`
- Engineering base: `a8b19fe39285da44ac443b7bcbd217870ec6ffb6`
- Cleanup human acceptance: **PENDING / intentionally deferred**
- Brown-Fur base authority: explicit user override permitting development from
  the pre-human cleanup candidate
- Previous rejected source: `2ef6e933ff521dff2330a948336a38083e741082`
- Repair source commit: `0940c282237826adfd6ef44f5bf864c2fdf0c588`
- Accepted documentation handoff: `1ddd357d20043a6d547d8fa80f899721b42713fe`
- Version: `0.0.82-brown-fur-human-review-repair`
- Package: `artifacts/packages/KingmakerGunslinger-0.0.82-brown-fur-human-review-repair.zip`
- Package SHA-256: `969F89E8744592FFB2D7009F881781498AA14C36932C2920A0ACD608D191B48D`
- DLL and installed-DLL SHA-256: `0D13E393FCD2E3F90D8882583C87915C0DA09EC5778867B5E69B53ABFA6BB265`
- DLL MVID: `754d4c45-dbf6-41b9-a5de-67b6e279c8ea`
- Deployment identity: `20260816T0435061213533Z`
- Deployment manifest SHA-256: `06028B8A74685A0FBF294E1CEC651C0733A8E70B63C3B492F3189973836338BC`
- Deployment backup identity: `20260816T0435018654582Z`
- Domain tests: **1,138 passed; 0 failed**
- Focused runtime qualification: **40 launches / 435 assertions passed**
- Seven-module boundary: **16/16 passed**
- Human acceptance: **GRANTED 2026-08-16**
- Exhaustive runtime matrix: **NOT RUN / NOT REQUIRED UNDER REVISED POLICY**

## Icon provenance

All repaired Brown-Fur icons reuse native Kingmaker sprites by blueprint GUID;
no external or extracted artwork is packaged. Powerful Change's feature icon
comes from CotW's resolved exploit selection. Strength, Dexterity,
Constitution, Intelligence, Wisdom, and Charisma use native donors
`4c3d08935262b6544ae97599b3a9556d`,
`de7a025d48ad5da4991e7d3c682cf69d`,
`a900628aea19aa74aad0ece0e65d091a`,
`ae4d3ad6a8fda1542acf2e9bbc13d113`,
`f0455c9295b53904f9e02fc571dd2ce1`, and
`446f7bf201dc1934f96ac0a26e324803`. Share uses native Beast Shape parent
`5d4028eb28a106d4691ed1b92bbb1915`; Transmutation Supremacy uses CotW's
resolved Magical Supremacy icon.

## Powerful Change

- [x] The six score icons are immediately distinguishable at action-bar size.
- [x] Strength reads as physical might rather than an abstract blue circle.
- [x] Every score icon shows the same live Arcane Reservoir number.
- [x] Activating a score changes no reservoir amount.
- [x] The selected score receives Kingmaker's native green active treatment.
- [x] Clicking the selected score again visibly turns it off.
- [x] Selecting another score moves the active treatment and leaves one score ON.
- [x] No confusing permanent pending effect appears in Effects and Conditions.
- [x] Bull's Strength gives +4 enhancement while Strength is OFF and costs 0.
- [x] Bull's Strength gives +6 enhancement while Strength is ON, costs 1, and turns Strength OFF.
- [x] Cat's Grace gives +6 enhancement while Dexterity is ON, costs 1, and turns Dexterity OFF.
- [x] An ineligible spell spends nothing and leaves the selected score armed.
- [x] Canceling before commitment spends nothing and leaves the selected score armed.
- [x] The tooltip explains Arcanist-spellbook eligibility, typed-bonus preservation, cost, level-20 +4 increase, ineligible casts, and manual cancellation.

## Share Transmutation

- [x] Share has a distinct icon and shows the live Arcane Reservoir number.
- [x] Share's native green treatment clearly communicates ON versus OFF.
- [x] Its tooltip explains Personal Transmutation, willing targets, Touch, exact 30 feet at level 20, one-point cost, cancellation, exclusions, and combined cost 2.
- [x] Beast Shape II enters creature-target selection rather than self-casting.
- [x] Undead Anatomy I enters creature-target selection rather than self-casting.
- [x] Resinous Skin enters creature-target selection rather than self-casting.
- [x] Selecting a willing ally applies each supported spell to that ally only.
- [x] Canceling targeting spends no slot or reservoir point and leaves Share ON.
- [x] Turning Share OFF restores ordinary Personal self-cast behavior.
- [x] Successful use spends 1, updates the counter, and turns Share OFF.
- [x] Touch delivery and movement feel native before level 20.
- [x] The level-20 maximum is visibly and behaviorally exactly 30 feet.

## Combined and package presentation

- [x] Powerful Change and Share can both be visibly armed.
- [x] A supported shared Arcanist spell costs exactly 2 and one spell slot.
- [x] The selected ally receives the spell and the chosen stat receives the enhanced original typed bonus.
- [x] Both toggles and pending facts clear after successful combined use.
- [x] The UMM switch says `Brown-Fur Transmuter  requires Call of the Wild`.
- [x] Available, Unavailable, and Blocked dependency states are distinct.
- [x] Active-this-process, saved-next-restart, effective publication, and restart-required states are distinct.
- [x] Brown-Fur appears under CotW Arcanist when ON and not when OFF.
- [x] All six pre-existing CotW Arcanist archetypes remain visible and ordered.
- [x] Progression shows Powerful Change at 3, Share Transmutation at 9, and Transmutation Supremacy at 20.
- [x] No archetype, feature, buff, grant, toggle, or resource counter is duplicated.

## Acceptance record

- Reviewer: user-authoritative human reviewer
- Review date and timezone: 2026-08-16, America/New_York
- Decision: **ACCEPTED**
- Accepted repair source commit: `0940c282237826adfd6ef44f5bf864c2fdf0c588`
- Accepted package SHA-256: `969F89E8744592FFB2D7009F881781498AA14C36932C2920A0ACD608D191B48D`
- Accepted installed DLL SHA-256: `0D13E393FCD2E3F90D8882583C87915C0DA09EC5778867B5E69B53ABFA6BB265`
- Accepted DLL MVID: `754d4c45-dbf6-41b9-a5de-67b6e279c8ea`
- Notes: all presentation and play-review criteria satisfactory

Any gameplay source or packaged-artifact change invalidates this acceptance
identity and requires a new immutable candidate. Documentation, policy, test,
and controller changes recorded after acceptance do not alter the accepted DLL
or package. The completed focused scenarios and 16-state boundary constitute
the final runtime seal under the revised policy.
