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

Current code confirms Recall opens confirmation, scroll rows multiply by
reader, and activation-failure text can retain an unresolved `{0}`.

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

NOT RUN: completed Oracle level-up/newly learned spontaneous cast; new direct
Recall, automatic-reader UI/ranking, notification acceptance; final candidate
qualification; module boundary/optional profile repeats. The first fixture
checkpoint does not claim any of those owner goals complete.
Runtime work uses Steam App 640820 and guarded requests with authorized
disposable fixtures / KMG_AUTOMATION_WORKING only. Owner campaign and
KMG_AUTOMATION_BASELINE are protected; installed settings and other mods must
be preserved. Raw artifacts remain ignored/local. Commit only after required
checks pass; publish coherent checkpoints through the approved push script.
