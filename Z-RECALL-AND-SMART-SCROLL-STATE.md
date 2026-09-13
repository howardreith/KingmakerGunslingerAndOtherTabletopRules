# Recall learning, direct casting, and automatic scroll readers

Active mission: owner's 2026-09-13 four-part instruction. This extends the
completed [Oracle scroll repair](Z-WORD-OF-RECALL-ORACLE-STATE.md); that repair
and its historical evidence remain intact. No release or merge is authorized.

## Baseline and scope

- Branch: `codex/z-recall-and-smart-scroll-ui`, created from clean master
  `9dc2b6301d97bc83540845240544d34b6fad4b48` (0.0.128). The 0.0.126 review
  anchor is older and was not restored. No pre-existing local changes.
- Installed version: 0.0.128; DLL SHA-256
  `ce65373e73f40573fc9f715d30ef181da0d5b07db4be38d9be8261b6ce59b8b8`.
- Owner FeatureModules.json: all twelve modules enabled; SHA-256
  `a3fb0a2136547c5467d65469a782570b7e61ff9e3a83314197789b4095ea4749`.
- Installed Assembly-CSharp.dll SHA-256
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
- Call of the Wild 1.14.4c-2.1 enabled, including balance fixes. Actual UMM
  profile also enables BagOfTricks, BetterVendors, CheatMenu, CraftMagicItems,
  EddicKingmakerRespec, KingmakerBuffPlanner, KingmakerBugfixes,
  KingmakerDiceRoller, KingmakerLastAzlantiPreserver, ProperFlanking2,
  RacesUnleashed, SkipIntro, TweakOrTreat, and ZFavoredClass. This is the
  owner's installed profile, not an assumed isolated CotW profile.

## Work and evidence

The first native diagnostic confirms the installed ordinary Oracle path
already offers Recall. `ApplySpellbook.Apply` creates choices from the effective
class book's SpellsKnown difference. `CharBSelectorLayer.FillSpellLevel` reads
that selection's `SpellList.GetSpells(level)` (filtered list); row refresh
excludes known/selected spells and applies the normal view filter. None of these
paths requires matching SpellListComponent metadata. No production Oracle
repair is justified by this evidence yet; Sayan's configuration remains open.

Direct Recall now uses the existing Greater Teleport dispatcher and shared
transaction guard. Desktop/controller original action callbacks are consumed
before synchronous settlement; unrelated modals still block both direct spells.
Successful Recall and Greater Teleport are quiet at the native warning boundary.
Scroll failures now format the actual reader and spell with independently
observed expenditure (none / exactly one / uncertain), without `{0}` leakage.
Scroll actions still multiply by reader; consolidation/ranking is next.

Owner clarified Sayan advanced 13→14 and obtained the spell through the favored
class bonus; equivalent earlier natural availability is acceptable. Installed
ZFavoredClass uses its Oracle book's list and a specific-level native parametrized
selector, with a prerequisite for one higher spell level. That native iterator
also reads SpellsFiltered and excludes known spells without SpellListComponent
metadata. The installed normal 13→14 sixth-level choice count still needs a live
table capture before attributing the report to choice entitlement.

Required acceptance remains pending: native Oracle candidate/selection,
commit/cancel and spontaneous cast; direct Recall from all resource kinds;
quiet direct successes and truthful named failures at production notification
boundary; one stable scroll row per material variant with read-only native
chance ranking and exactly one real activation. Preserve ordinary Teleport
confirmation/outcomes, transaction safeguards, sanctuary restrictions,
desktop/controller layout and native movement/travel behavior.

## Qualification status

Diagnostic artifact: repository validation PASS; complete domain suite
1,622/1,622 PASS; clean exact-reference Release build and strict package PASS.
ZIP SHA-256 `b75e63257b3f553690b56e69d496d23f5061c10eb35392474c46a62ce7e942c5`;
DLL SHA-256 `1b8d246e06126793cca447f5f2946e239157bad64012a06d5594903b729bcf0e`;
MVID `2588fe77-ad81-4477-90d0-9a26433b286f`; source fingerprint
`c8cb25e0cefe4e82f19235480cefea5e05459431e7de828a5b78d0d07bfa5fa5` on
base commit 9dc2b630 (uncommitted fixture only).

Guarded run `20260913T1332151328337Z-disposable-teleportation-level-up`: PASS
through Windows PowerShell / Steam 640820 / KMG_AUTOMATION_WORKING. Native
Oracle row, preview selection, and cancel pass alongside Wizard/Sorcerer
controls; zero exceptions and exact fixture cleanup, no save write. Oracle
book `3587fa91b34341e49b3a22cfb5450e0d`, list
`f305174b73f64783a8379238a14c3283`, canonical Recall
`596d85a666204d6ea5c0188e53f4b4de`; raw/filtered membership each one, one normal
level-6 choice, no archetype, no matching Oracle SpellListComponent. The fixture
supplies an Oracle book at caster level 11 to the disposable save's level-2
owner; the native UI advances that book and chooses the spell. This is actual
candidate/preview/cancel proof, not yet a realistic full Oracle progression,
committed learning, or spontaneous-cast acceptance. No AddKnown/candidate-list
injection is used for the tested Recall choice.

Earlier same-artifact run `20260913T1329565378989Z-disposable-teleportation-level-up`
also produced native PASS and exited, but its PowerShell 7 launcher rejected
returned process metadata. It is diagnostic evidence only; the Windows
PowerShell repetition above has the complete successful orchestration.
Original installed mod backup: `runtime-backups/live-mod/20260913T1328415126859Z`.
Settings retain the original hash. No game process remains after the passing run.

Direct-cast checkpoint: repository validation, 1,623/1,623 complete domain tests,
clean exact-reference Release build, and strict standalone package PASS.
Corrected fixture artifact ZIP SHA-256
`bf421b411a2360d3a32422f6b88c3c9dc5ef5795b8fc70d3dbe5438deb40efb7`;
DLL `fffbbc15cf4387675b4cdf57c94305bd77a522fc09479daf0d59754eeca620e3`;
MVID `6b9456e5-12d6-4e6f-a4b8-505537585ce7`; source fingerprint
`f449a29de7c8f18eb1199d9dcce0301deda47dda282c2fddd31ef5620bbba9e1`
on base 59e97eef (uncommitted direct changes). Deployment
`20260913T1359587761724Z`; settings retain the original hash.

Guarded Steam / working-save evidence:
- `20260913T1351213722683Z-disposable-teleportation-casting`: 52/52 PASS.
  Actual desktop direct prepared Recall reaches both sanctuaries, spends one,
  no dialog; production warning observer sees zero direct success messages and
  one retained message for each ordinary Teleport on/off/similar outcome.
- `20260913T1352503487268Z-disposable-teleportation-scrolls`: 45/45 PASS.
  Native unknown-spell zero-UMD Oracle scroll activation, one scroll spent,
  exact Oleg arrival, quiet warning boundary, same-frame and later original
  event replays do not spend again. This is still explicit-reader code.
  Those two runs used ZIP `b08ffb0fbf6e01379c732fa54b0f9caa2d89b9e920fe29e08e0413d43fd58a1c`,
  DLL `6e4b5a6c2dba2f7f7b19a42a25e0b16de91be4137b86a5cc25294d628312c7cd`.
- `20260913T1354572749461Z-disposable-teleportation-gamepad`: ERROR in the
  new fixture's stale Unity widget invocation after a successful direct cast.
  Replaced widget access with the retained native m_OnConfirmAction callback.
- `20260913T1359596003845Z-disposable-teleportation-gamepad`: 39/39 PASS on
  the corrected artifact; direct Recall at both sanctuaries, native controller
  action replay same frame and later, exact resources/cleanup, zero exceptions.
- `20260913T1402468713473Z-disposable-teleportation-interaction`: 34/34 PASS
  on the corrected artifact; native modal/stale/depletion/cancel/movement and
  Travel controls retained. No save writes in these runs.

NOT RUN: completed Oracle level-up/newly learned spontaneous cast;
automatic-reader UI/ranking; named failure wording observed at production
notification boundary (formatter tests alone pass); final combined candidate
qualification; module boundary/optional profile repeats. The first fixture
checkpoint does not claim any of those owner goals complete.
Runtime work uses Steam App 640820 and guarded requests with authorized
disposable fixtures / KMG_AUTOMATION_WORKING only. Owner campaign and
KMG_AUTOMATION_BASELINE are protected; installed settings and other mods must
be preserved. Raw artifacts remain ignored/local. Commit only after required
checks pass; publish coherent checkpoints through the approved push script.
