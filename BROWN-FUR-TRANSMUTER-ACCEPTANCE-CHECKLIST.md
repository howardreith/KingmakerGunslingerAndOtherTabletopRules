# Brown-Fur Transmuter human acceptance checklist

Status: **REPAIRED CANDIDATE INSTALLED / HUMAN ACCEPTANCE PENDING**.

This checklist applies only to the immutable `0.0.82` repair candidate below.
The rejected `0.0.81` candidate remains superseded and was never accepted.
Do not run the final 128-state matrix until a reviewer explicitly accepts this
exact source, package, and installed DLL identity.

## Candidate identity

- Branch: `codex/brown-fur-transmuter-cotw-extension`
- Engineering base: `a8b19fe39285da44ac443b7bcbd217870ec6ffb6`
- Cleanup human acceptance: **PENDING / intentionally deferred**
- Brown-Fur base authority: explicit user override permitting development from
  the pre-human cleanup candidate
- Previous rejected source: `2ef6e933ff521dff2330a948336a38083e741082`
- Repair source commit: `0940c282237826adfd6ef44f5bf864c2fdf0c588`
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
- Final exhaustive matrix: **PENDING HUMAN ACCEPTANCE / NOT RUN**

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

- [ ] The six score icons are immediately distinguishable at action-bar size.
- [ ] Strength reads as physical might rather than an abstract blue circle.
- [ ] Every score icon shows the same live Arcane Reservoir number.
- [ ] Activating a score changes no reservoir amount.
- [ ] The selected score receives Kingmaker's native green active treatment.
- [ ] Clicking the selected score again visibly turns it off.
- [ ] Selecting another score moves the active treatment and leaves one score ON.
- [ ] No confusing permanent pending effect appears in Effects and Conditions.
- [ ] Bull's Strength gives +4 enhancement while Strength is OFF and costs 0.
- [ ] Bull's Strength gives +6 enhancement while Strength is ON, costs 1, and turns Strength OFF.
- [ ] Cat's Grace gives +6 enhancement while Dexterity is ON, costs 1, and turns Dexterity OFF.
- [ ] An ineligible spell spends nothing and leaves the selected score armed.
- [ ] Canceling before commitment spends nothing and leaves the selected score armed.
- [ ] The tooltip explains Arcanist-spellbook eligibility, typed-bonus preservation, cost, level-20 +4 increase, ineligible casts, and manual cancellation.

## Share Transmutation

- [ ] Share has a distinct icon and shows the live Arcane Reservoir number.
- [ ] Share's native green treatment clearly communicates ON versus OFF.
- [ ] Its tooltip explains Personal Transmutation, willing targets, Touch, exact 30 feet at level 20, one-point cost, cancellation, exclusions, and combined cost 2.
- [ ] Beast Shape II enters creature-target selection rather than self-casting.
- [ ] Undead Anatomy I enters creature-target selection rather than self-casting.
- [ ] Resinous Skin enters creature-target selection rather than self-casting.
- [ ] Selecting a willing ally applies each supported spell to that ally only.
- [ ] Canceling targeting spends no slot or reservoir point and leaves Share ON.
- [ ] Turning Share OFF restores ordinary Personal self-cast behavior.
- [ ] Successful use spends 1, updates the counter, and turns Share OFF.
- [ ] Touch delivery and movement feel native before level 20.
- [ ] The level-20 maximum is visibly and behaviorally exactly 30 feet.

## Combined and package presentation

- [ ] Powerful Change and Share can both be visibly armed.
- [ ] A supported shared Arcanist spell costs exactly 2 and one spell slot.
- [ ] The selected ally receives the spell and the chosen stat receives the enhanced original typed bonus.
- [ ] Both toggles and pending facts clear after successful combined use.
- [ ] The UMM switch says `Brown-Fur Transmuter  requires Call of the Wild`.
- [ ] Available, Unavailable, and Blocked dependency states are distinct.
- [ ] Active-this-process, saved-next-restart, effective publication, and restart-required states are distinct.
- [ ] Brown-Fur appears under CotW Arcanist when ON and not when OFF.
- [ ] All six pre-existing CotW Arcanist archetypes remain visible and ordered.
- [ ] Progression shows Powerful Change at 3, Share Transmutation at 9, and Transmutation Supremacy at 20.
- [ ] No archetype, feature, buff, grant, toggle, or resource counter is duplicated.

## Acceptance record

- Reviewer: ______________________________
- Review date/time and timezone: ______________________________
- Decision: **PENDING**
- Accepted repair source commit: ______________________________
- Accepted package SHA-256: ______________________________
- Accepted installed DLL SHA-256: ______________________________
- Notes: ______________________________

If review requires any source change, this acceptance identity is invalid.
Build and qualify a new immutable candidate. If this exact candidate is
accepted without source changes, reuse its installed artifact for the single
post-acceptance 128-state matrix and final release seal.
