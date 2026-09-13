# Recall learning, direct casting, and automatic scroll readers

Owner mission, 2026-09-13. The implementation and guarded mechanical acceptance
are complete with the explicit limits below. The prior
[Oracle scroll repair](Z-WORD-OF-RECALL-ORACLE-STATE.md) is preserved. No merge or
public release is authorized or performed.

## Configuration and provenance

- Branch: `codex/z-recall-and-smart-scroll-ui`; master remains
  `9dc2b6301d97bc83540845240544d34b6fad4b48` (0.0.128). The older 0.0.126 anchor
  was not restored. No pre-existing local changes were discarded.
- Installed profile: CotW 1.14.4c-2.1, balance fixes enabled, plus BagOfTricks,
  BetterVendors, CheatMenu, CraftMagicItems, EddicKingmakerRespec,
  KingmakerBuffPlanner, KingmakerBugfixes, KingmakerDiceRoller,
  KingmakerLastAzlantiPreserver, ProperFlanking2, RacesUnleashed, SkipIntro,
  TweakOrTreat and ZFavoredClass. All twelve KMG modules enabled normally.
- Native Assembly-CSharp SHA-256:
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
- Owner FeatureModules.json retained byte-for-byte, SHA-256:
  `a3fb0a2136547c5467d65469a782570b7e61ff9e3a83314197789b4095ea4749`.
- Original installed DLL:
  `ce65373e73f40573fc9f715d30ef181da0d5b07db4be38d9be8261b6ce59b8b8`;
  original backup `runtime-backups/live-mod/20260913T1328415126859Z`.
- Checkpoints already published through the approved push script: `59e97eef`
  (Oracle candidate diagnostic), `a11b35b6` (direct/quiet Recall), `39983390`
  (real Oracle learning), `40682f64` (automatic reader/grouped UI). The commit
  containing this record completes the mission with the evidence limits below.

## Oracle learning: demonstrated cause and native proof

Installed ordinary sixth-level known-spell allowances at Oracle levels
11/12/13/14 are **0/1/2/2**. Thus 11→12 grants one normal choice; **13→14 grants
zero**. The owner reports Sayan advanced 13→14 and obtained Recall through the
favored-class bonus. This explains the ordinary-choice entitlement distinction;
it does not purport to reproduce his unavailable archetype/FCB selection.

Native `ApplySpellbook.Apply` uses the effective ClassData spellbook and the
SpellsKnown difference. `CharBSelectorLayer.FillSpellLevel` reads that selection's
`SpellList.GetSpells`/filtered list, then applies normal known/selected and view
exclusions. ZFavoredClass's specific-level parametrized selector also reads the
filtered list, excludes known spells and requires one higher available spell
level. Neither path requires corresponding Oracle SpellListComponent metadata.
The preserved final-list reconciler already makes the canonical normal candidate
available; no additional production learning repair was justified or introduced.
Cleric 6 / Druid 8 / optional Oracle 6 and canonical spell/scroll identities remain.

Equivalent configuration actually tested: Human male, True Neutral, Charisma 20
at Oracle 11, no archetype, Brass Dragon mystery, Lich curse, Lamashtu deity;
the native favored-class selection is Druid, so it supplies no additional Oracle
spell choice. The owned unit starts with no Recall. Oracle class
`32c02466b2364c8a906e6e4761175099`, effective book
`3587fa91b34341e49b3a22cfb5450e0d`, list
`f305174b73f64783a8379238a14c3283`, canonical Recall
`596d85a666204d6ea5c0188e53f4b4de`. Exact native feature IDs and selections are
recorded in the final `teleportation-level-up.json`.

The fixture progresses its owned character natively to Oracle 11, then opens the
actual 11→12 level-up UI. Recall appears exactly once among level-6 candidates.
Selecting and canceling leaves it unknown at level 11; selecting again and using
the native completion button learns it once in the correct Oracle book and
exhausts one normal choice. A direct cast from the newly learned spell spends one
normal spontaneous sixth-level slot and arrives exactly at Oleg's. No Recall
AddKnown call, candidate-list injection, extra choice/slot or campaign edit is
used. Native class processing grants other spells separately, so acceptance checks
the actual choice exhaustion rather than confusing total known count with choices.
The zero-UMD Oracle who does not know Recall is a separate native scroll control.

## Direct casting, automatic readers, and resource rules

Recall shares Greater Teleport's direct dispatcher and transaction. Explicit
caster/book selection remains for spell slots. Both direct spells work without a
confirmation presenter; unrelated native modals block all spells. Ordinary
Teleport retains confirmation and outcome messages. Recall revalidates the
current capital or pre-capital Oleg sanctuary before spending; no local-area
entry, inventory activation, ordinary travel or arbitrary destination is added.

One stable action represents each material scroll group. Shared inventory is
counted once; normal buttons and Teleport scroll confirmation omit reader/book
names. Different CL, SL, charges, renewal/UMD contracts, unidentified state, and
unknown item components/enchantments remain separate. Only multiple variants add
a compact qualifier. Both native desktop and controller paths retain real rows,
focus and parchment containment; spellbook rows and settlement width are preserved.

The read-only adapter follows the inspected installed native/CotW path:

- Native suitability, traveling-party membership, life/consciousness, available
  charges, prohibitions, caster/target/parameter restrictions, and patched
  `IsUnitNeedUMDForUse` (including CotW FamiliarFreeItemUse) determine eligibility.
- Required UMD uses actual modified UMD plus supported native/CotW deterministic
  check bonuses/stacking, DC `20 + item spell level + difficulty adjustment`,
  applicable take-ten and conditional success bonus. Native ordinary d20 has no
  automatic 1/20 rule here. Multiply the UMD success probability by
  `(100 - maximum applicable native/CotW item/spell failure percent) / 100`. No CL or ability-score check or natural-1
  UMD cooldown was present in this installed activation path.
- Highest supported probability wins; a proven no-check route wins ties, then
  party order/stable unit ID. Selection is per spell/variant. No equipment,
  ability, optional resource, RNG, rule event or character mutation improves rank.
- Unknown applicable listeners, active UMD rerolls/replacements, random/resource
  bonuses, unsupported reason-dependent modifiers, or active skill/dice cheats
  are unscored. One eligible reader needs no comparison; a proven no-check maximum
  may still win. Otherwise the group reports that no best reader can be safely
  resolved, without guessing. This is not a general future-mod probability engine.

Activation resolves a current real reader and specific physical item. Teleport
confirmation binds reader/item/count/familiarity/risk and cancels before spending
if they change. No variant or spell-slot substitution, automatic retry, second
reader, or destination-roll cherry-picking occurs. The original desktop/controller
callback is consumed before synchronous execution, including same-frame replays.

Native item activation remains authoritative. Exact bound-item stock/charge
changes now cover consumable, charged, renewable and reusable scrolls. Native
HandOfMagusDan preservation is accepted only with an attributed successful native
attempt and its observed top-level preservation roll. Compensation restores only
an exactly proven pre-outcome debit using the captured item, preserving metadata;
legitimate native outcomes are never refunded. No missing spending evidence is
mistaken for a free spellbook cast.

Production warning-boundary observers see zero mod success announcements for
Greater/Recall and retained ordinary Teleport on/off/similar/mishap outcomes.
Native failure probes name the actual reader and spell once and prove no scroll
consumed, one scroll consumed, and one charge consumed. Uncertain-expenditure
wording is covered by domain tests, not a manufactured native inventory ambiguity. A vanished
reader produces no attempt/consumption and an honest notice without an invented
reader. Technical/stale-resource notifications remain.

## Optional-CotW registration repair

An actual guarded absent-CotW startup exposed a separate inherited dependency:
the preferred Greater/Recall scroll templates are supplied by CotW, although
they had been documented as native. Their absence aborted publication of all
three strategic spells. Preferred templates remain unchanged when present.
Only absence permits a verified base-game fallback with identical approved
cost and caster level: Greater uses `013c0f5972c1b794b869b284ba426542`
(Summon Greater Earth Elemental, 2,275 gp / CL13); Recall uses
`0437d7a2ea4b01542907c4d5fb12c4da` (Elemental Body III Fire, 1,650 gp / CL11).
Canonical identities, isolated CopyScroll teaching, activation/charge contracts,
Cleric/Druid/Oracle publication and module gating remain intact. Present but
incompatible templates still fail exact validation rather than being replaced.
Actual final native assertions verify economics, icons/weight, isolated teaching
and charge/UMD contracts in both profiles. The absent profile passes 22/22 in
`20260913T1727419794936Z-observe-teleportation-native-contracts`; transaction
`compat-20260913T172737Z-bd8937fe1ee1` restores the original profile exactly.

The initial absent-profile run `20260913T1642215687215Z` is a real production
registration failure. The next `20260913T1654051508879Z` proves repaired production
bootstrap but errors in an old fixture's unconditional optional-donor lookup.
That fixture now follows the same presence-based selection and asserts actual
donor isolation; neither earlier run is counted as acceptance.

## Final candidate qualification

Repository validation, **1,632/1,632 complete domain tests**, clean exact-reference
Release build, icon checks and strict standalone package validation all PASS via
`scripts/Build-Local.ps1`. Version remains 0.0.128.

- ZIP SHA-256: `7d36c6669ed07ddfcc3f08d96b1b87286ababeeb15704057a2db175d47049fa1`
- DLL SHA-256: `731638e8f72d79b760c651b084560e89bd57b88bb401564f1faa4ceeaddd0309`
- MVID: `2fd3fdc4-6590-4a23-8ef6-8e3cf10f3401`
- Tested base: `40682f640d89124126f7f9c09ed5435a797527cc`; exact precommit source
  fingerprint `698ea359c83ffa4a59caac10794bc339f4b5c63eea7edf5fc8beb5f50131eb89`.
  This record was updated after qualification; production/fixture code is unchanged.
- Deployment: `deployments/20260913T1726022570374Z/deployment.json`.
- Installable ZIP: `artifacts/local-runtime/0.0.128/KingmakerGunslinger-0.0.128-local-runtime.zip`.

All final native runs below use that exact artifact, Windows PowerShell, guarded
requests and Steam App 640820, with automatic exit and no save writes. Save-backed
cases use only KMG_AUTOMATION_WORKING; registration cases are save-free.
Evidence is under `C:/Dev/KingmakerGunslingerLab/runtime-evidence/`.

**PASS: 520/520 assertions across 17 final runs**, plus the 22/22 absent-profile
assertions above. Final batch: `teleportation-hardening-native-20260913T1729349319709Z`.

| Guarded scenario | Module | Assertions | Evidence directory |
| --- | --- | ---: | --- |
| native-contracts | ON | 22 PASS | `20260913T1729367819073Z-observe-teleportation-native-contracts` |
| interaction | ON | 35 PASS | `20260913T1730163657107Z-disposable-teleportation-interaction` |
| scrolls | ON | 65 PASS | `20260913T1731199917493Z-disposable-teleportation-scrolls` |
| casting | ON | 54 PASS | `20260913T1732261042869Z-disposable-teleportation-casting` |
| gamepad | ON | 51 PASS | `20260913T1733266310209Z-disposable-teleportation-gamepad` |
| familiarity | ON | 9 PASS | `20260913T1736353322578Z-disposable-teleportation-familiarity` |
| destinations | ON | 68 PASS | `20260913T1737367157010Z-disposable-teleportation-destinations` |
| travelers | ON | 15 PASS | `20260913T1739002484298Z-disposable-teleportation-travelers` |
| resources | ON | 19 PASS | `20260913T1740067752977Z-disposable-teleportation-resources` |
| spellbook-ui | ON | 31 PASS | `20260913T1741100411532Z-disposable-teleportation-spellbook-ui` |
| level-up | ON | 47 PASS | `20260913T1742148751909Z-disposable-teleportation-level-up` |
| disabled | OFF | 8 PASS | `20260913T1743413156588Z-disposable-teleportation-disabled` |
| coexistence | ON | 28 PASS | `20260913T1744390895844Z-disposable-teleportation-coexistence` |
| coexistence-gamepad | ON | 25 PASS | `20260913T1745503665021Z-disposable-teleportation-coexistence-gamepad` |
| coexistence | OFF | 15 PASS | `20260913T1747082424027Z-disposable-teleportation-coexistence` |
| coexistence-gamepad | OFF | 17 PASS | `20260913T1748073182454Z-disposable-teleportation-coexistence-gamepad` |
| working-save-smoke | ON | 11 PASS | `20260913T1749081611061Z-working-save-smoke` |

Scroll native proof includes guaranteed-versus-fallible readers, UMD +7/DC25
(15%) versus +11 (35%), stable ties, changed/unavailable readers, shared counts,
material variants, and unchanged RNG/rulebook/resources across repeated ranking
and rendering. Owned native failure features exercise actual UMD and non-UMD
activation failures; one actual selected-reader native event is observed with no
second attempt, slot use or false arrival. Charged/renewable/reusable contracts
and both real preservation outcomes pass. The preservation branch alone uses a
guarded exact native d100 override of 1/100; it is not simulated activation or
ranking. The additional comparison reader's exact ClassData references are
isolated only synchronously in the disposable control and restored before yield.

The final `transaction-result.json` is PASS: all **85 protected pre-existing save
files unchanged**, original settings and complete Mods inventory restored, no game
process, no error. A separate read-only artifact audit verifies every runtime
result against the loaded DLL SHA-256/MVID, guarded Steam request, scenario and
save identity. The qualified DLL remains installed; KMG_AUTOMATION_BASELINE and
the owner's campaign were never written. Full profile inventory is the batch's
`mods-before.json`, SHA-256
`53c1167d38b31273090ee8c0c9205cc954e626542b24ba9efff8b6929e5412a9`.
Relevant installed DLL hashes: CotW
`4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915`;
ZFavoredClass
`dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c`.

## Diagnostic failures kept distinct from acceptance

An earlier complete native batch had all 345 runtime assertions PASS but its
outer reporter mishandled an empty array; a read-only recovery audit verified
all results, 85 save files, settings and the complete Mods inventory. The final
local runner uses `ConvertTo-Json -InputObject` to serialize empty arrays as `[]`,
retains every guard/restoration check, and explicitly includes the mission's
registration, scroll and four module ON/OFF coexistence cases plus the final
original-profile working-save smoke. Runner SHA-256:
`e748a027ee0ccd508f2425353a0825c82f86e15db8f2d56d7b9a9f4e5cfafe8c`.

The unrelated broad `observe-feature-module-settings` diagnostic at
`20260913T1637028296468Z` failed its old vendor-stock expectation (12/24 versus
actual 10/20). Master has the same production counts and obsolete fixture
expectation; no firearm-shop changes are in this mission. Its other 34 assertions
and four dedicated coexistence cases passed, with exact restoration. This
unrelated observer remains FAIL; it is not substituted for mission acceptance.

The 170313 batch stopped at `20260913T1705579805026Z` interaction: 33/34 PASS,
with the last reopened viewport 453.733917 versus 454 pixels. The fixture's
120-frame cap silently expired while the camera target continued moving.
Explicit projected-point waits then timed out in 171350 and diagnostic 171957;
the latter records continuous native target panning while the map point is fixed.
The final fixture isolates native edge scrolling only during this comparison
through `SettingsEntityBool.m_Cached`, never its persisting setter. It restores
and asserts the exact nullable cache in `finally`, recenters through native
`ScrollTo`, and records projected-point stability before measuring all eight
reopens. No production layout or 0.01-pixel assertion tolerance changed.

Focused final-artifact run `20260913T1726073981324Z` passes 35/35: edge scrolling
was ON; the camera settles after 1,000 actual frames / 1.562 seconds; all eight
heights are exactly 279.510773 pixels; cache, party/resources and native UI cleanup
are restored. The abandoned diagnostic outcomes remain ERROR/FAIL. Both aborted
combined batches restored all 85 saves, settings and the complete Mods tree.

## Evidence limits

- **NOT RUN:** Sayan's exact unavailable archetype and 13→14 favored-class-bonus
  UI. The authorized equivalent ordinary Oracle 11→12 flow above is fully native.
- **NOT RUN:** absent-CotW save-backed gameplay/Oracle learning; the isolated
  absent profile qualifies save-free startup/registration only.
- **NOT QUALIFIED:** final screenshot-based visual acceptance. Desktop 173119
  and controller 173326 captures were black and byte-identical (SHA-256
  `8a843dc4f25ae7ba443b1df93dfa37831f447b28a5b45aefd8739a4a0803ab1a`).
  Native text, geometry, focus, events and resources passed; these are not pixel
  inspection. The earlier actual desktop capture
  `20260913T1615188389544Z-disposable-teleportation-scrolls/desktop-scroll-variants.png`
  was inspected: readable, contained variants without reader names. It is
  supporting evidence from the earlier candidate, not final-artifact pixel proof.
- Arbitrary unknown activation mods and unsupported active roll effects are not
  assigned invented probabilities. The supported/fail-closed contract is above.

Early diagnostic fixture failures and earlier artifact hashes remain in this
record's Git history. They are superseded by the final passes, not reclassified
as acceptance. The capture-only overlay adjustment closes/restores UMM through
its public API inside the guarded fixture and never saves settings. Raw evidence,
images, packages, saves and proprietary assemblies remain local and uncommitted.
