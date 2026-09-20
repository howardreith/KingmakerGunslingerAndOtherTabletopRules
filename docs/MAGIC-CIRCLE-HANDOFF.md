# Magic Circle review candidate

The owner subsequently accepted PR #19 and explicitly authorized integration,
merge, push and release 0.0.134. See [the release notes](RELEASE-NOTES-0.0.134.md)
and [release acceptance](../planning/MAGIC-CIRCLE-RELEASE-134.md). Version 0.0.134
is now published; [public release evidence](../reports/magic-circle/RELEASE-134-PUBLIC.md)
records the exact tested/downloaded artifact and normal-play restoration. The
earlier review-only authorization and artifact identities below are historical.

The PR 19 review follow-up supersedes the candidate and source identities below.
See [the current hardening handoff](../reports/magic-circle/PR19-REVIEW-HARDENING.md)
for R1/R2/C1, current native evidence, package hashes and restoration. The remainder
of this file preserves the original feature qualification and its limits.


Four Magic Circle spells are implemented and qualified within the profiles and
scopes below. The owner approved the four exact artwork exports and the adaptation
that bearer death ends the circle. This is an installable review candidate; no
merge, release tag, public release or campaign installation is authorized by this
handoff.

Repository: `howardreith/KingmakerGunslingerAndOtherTabletopRules`.
Base: `21a1c4b25a61f481f76b8fb0d57bd7fc4a5698dc`.
Branch: `codex/magic-circle-alignment-spells` in its dedicated worktree.
Qualified product source: `1c3f5acca108408a81a0ea02d9551b2569b1c2b4`.
The branch is published through the required guarded helper.
[Draft PR #19](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/19)
targets `master` (`525625df443d0d2f575e5f984c0091a838889b63` at publication).
GitHub reported MERGEABLE/CLEAN with no status checks reported. It remains open
and draft; no merge was performed. Subsequent handoff-only commits preserve the
qualified product tree.

## Behavior and decisions

Each separate level-3 spell is a standard-action touch abjuration. The touched
creature carries a native moving ten-foot emanation for ten minutes per original
caster level. Bearer, allies, enemies, pets and summons are covered. Native
alignment-conditioned +2 deflection AC and +2 resistance saves use normal typed
stacking. Opposing spell descriptors are Good, Evil, Chaotic and Lawful.

`magic-circle-spells` controls content publication and new casts. The existing
`protection-from-alignment-control-immunity` setting remains the sole authority
for added control protection. Circles reuse the Protection component, catalog,
source resolution, alignment predicate and unresolved-source policy. Only new
qualifying control from a matching controller is blocked. Existing control stays
active. Enhancement OFF retains ordinary defenses and changes descriptions.
Both settings follow the existing startup/restart model.

The caster supplies duration, level, metamagic and dispel context; the bearer
supplies area position; the incoming controller supplies its alignment. Overlaps
retain exact per-area ownership. Expiration, bearer death, permanent bearer
removal or successful carrier/area dispel removes only that cast. Unconsciousness
and original caster death do not end a living bearer's circle. Recipient buffs
are proximity effects and are not independently dispellable. Temporary unloading
reconstructs from the carrier without refreshing its deadline. Native geometry
uses radius 3.048 metres, creature body radius and native obstruction checks.

Cleric, Sorcerer/Wizard and Inquisitor receive all four variants; Paladin receives
Evil and Chaos. Verified optional entitlements use actual inherited lists or
narrow adapters. Prepared, spontaneous, specialist and alignment gates remain
native; learning one variant does not grant four. Each scroll is level 3, CL5,
375 gp and one charge, with exact spell association and native scribing. Existing
arcane/priest suppliers offer finite batches of five. Actual casts debit one
slot or charge; recipient refresh costs nothing.

Content OFF preserves stable blueprint identities and active original lifetimes
for save hydration, while disabling new casts and list publication. Finite vendor
definitions remain to preserve purchase memory; inert scroll stock may remain
visible, including at unvisited suppliers. This does not establish uninstall or
downgrade safety for saves containing feature content.

Deferred mechanics remain absent: new saves or morale against existing control;
removal, suppression, pause or resumption of control; summoned-creature exclusion,
contact/SR/breach barriers; inward circles, binding and diagrams; blanket descriptor
immunity; silver-dust inventory bookkeeping. Individual and communal Protection
retain their levels, targeting, durations and established shared control behavior.
See [rules and optional class details](MAGIC-CIRCLE-SPELLS.md).

## Exact candidate and checks

Package, relative to the feature worktree:
`artifacts/local-runtime/0.0.132/KingmakerGunslinger-0.0.132-local-runtime.zip`
(28,158,734 bytes).

| Identity | Value |
| --- | --- |
| UMM / informational version | `0.0.132` / `0.0.132-icon-art-overhaul` |
| ZIP SHA-256 | `fc440dbfb38d5496f6afeaa8cc97fd61fc85d27b928fc85a5c5b181707781d75` |
| DLL SHA-256 | `a5f8f6a759c672d367ae00a794dd013bb8075b1595e4dd9c3c2519209cac6057` |
| DLL MVID | `ea9ac240-4984-421b-b2e1-4336fa5770c6` |
| Embedded Git commit | `75faf15ace983a225af1e0bc7d0faa605e77237f` |
| Validated build source-state SHA-256 | `f3b6db20350d823283082130eae5a415698fb1ec8f3637bb5cc69fb0bfb2329b` |

The clean Release compilation used the validated working tree at the embedded
commit. That embedded commit alone does not identify the final source. The
qualification record fingerprints all 1,387 product inputs using both raw bytes
and Git-filtered blobs, so the committed product tree can be matched to the exact
tested build. [Committed-source verification](../reports/magic-circle/COMMITTED-SOURCE-CORRESPONDENCE.json)
confirms every frozen path/blob at the qualified source commit and records the
guarded publication. Later documentation and curated evidence do not alter those inputs.
The frozen ZIP retains its original packaged documentation; this handoff supplies
the completed qualification status without rebuilding the tested binary.

| Check | Result |
| --- | --- |
| Repository validation | PASS |
| Complete clean domain/reflection suite | 1,657 PASS, zero failures; all 8,192 settings combinations |
| Focused request guards | PASS |
| Compatibility parameter checks | 8,227 PASS |
| Clean Release compilation | PASS |
| Strict standalone package validation | PASS |
| Native saved-world mechanics, control ON / OFF | 268 / 268 assertions PASS |
| Native desktop UI, control ON / OFF | 62 / 62 assertions PASS |
| Optional absent / CotW native profiles | 67 / 67 assertions PASS within save-free scope |
| Saved preparation / four startup verifications | 13 / 10 each PASS |
| Actual area reload and world-map roundtrip | 22 PASS |
| Cleanup / fresh absence | 12 / 3 PASS |

The [final qualification record](../reports/magic-circle/FINAL-CANDIDATE-QUALIFICATION.json)
maps nineteen acceptance groups to structured production-path assertions, exact
artifact identities and hashed evidence. It covers actual positive and negative
control outcomes, source alignment, native geometry and typed rules, acquisition,
overlap ownership, lifecycle, settings, publication and cleanup. The
[acceptance record](../planning/MAGIC-CIRCLE-ACCEPTANCE.md) is the compact index.
Raw evidence is retained locally under
`C:/Dev/KingmakerGunslingerLab/runtime-evidence`, keyed by the run directory names
in the curated reports. Raw saves, screenshots, logs, packages and proprietary
assemblies are not committed.

All launches used guarded Steam App 640820 requests on Kingmaker 2.1.7b. The exact
`KMG_AUTOMATION_WORKING` save descriptor and loaded DLL were verified. The final
twelve-run saved-world sequence made exactly two authorized Working writes,
preparing and cleaning the disposable fixture, with zero unexpected writes. A
fresh load proved fixture absence and restoration of inventory, gold and grant
state. `KMG_AUTOMATION_BASELINE` and campaign saves were never written.

Saved-world qualification used the original eleven-mod stack: BagOfTricks 1.16.4,
CallOfTheWild 1.14.4c-2.1, CheatMenu 1.2.3, CraftMagicItems 2.1.0,
KingmakerBuffPlanner 0.0.16, KingmakerDiceRoller 0.1.2, KingmakerGunslinger 0.0.132,
KingmakerLastAzlantiPreserver 0.1.0, RacesUnleashed 1.0.11, TweakOrTreat 1.1.0 and
ZFavoredClass 1.3.1. Exact mod DLL hashes/MVIDs are in the run inventories.
`gunslinger-only` and `gunslinger-call-of-the-wild` passed native save-free
mechanics and entitlement checks with exact host/profile restoration.

## Artwork and remaining limits

[Owner approval](../reports/magic-circle/OWNER-DECISIONS.json) identifies the exact
four 128px exports. Each alignment intentionally shares its painting across its
spell, held touch, carrier, recipient and scroll. All twenty visible consumers
passed native desktop checks in both control configurations.
[Direct native visual review](../reports/magic-circle/FINAL-NATIVE-UI-REVIEW.json)
inspected all 42 ON screenshots and all eight OFF carrier/recipient tooltips.
Native target rings were visible and matched the mechanical radius. Native
recipient rows retain the engine's Permanent label and target summaries say
burst; explicit Within Circle names and authored proximity/moving-emanation text
explain the behavior. Gamepad UI is not qualified. Technical and native review
results do not impersonate owner approval.

Two environment/observer limits remain separate from feature PASS:

- Reduced stacks cannot load the existing Working save: native Player.PostLoad
  fails before the Circle fixture, as previously documented for other content.
  The named three-mod `gunslinger-qualified-combined` profile is also affected.
  [Recorded failures](../reports/magic-circle/REDUCED-PROFILE-SAVE-PREREQUISITE.json)
  remain TIMEOUT. Full saved-world coverage for those profiles would require a
  compatible, explicitly authorized disposable fixture. No save repair or guard
  relaxation was attempted. [Bounded native profile evidence](../reports/magic-circle/OPTIONAL-NATIVE-PROFILES.json)
  makes no terrain, navigation, native UI, scene travel or disk-hydration claim.
- The broad settings observer retains three FAIL assertions requiring native
  donor-icon reference equality for unrelated Teleportation scrolls. Those
  scrolls intentionally use unchanged protected composite artwork. Exact base
  comparison and the observer's limits are retained in
  [separate attribution](../reports/magic-circle/UNRELATED-SETTINGS-OBSERVER.json).
  No unrelated code was changed and no validator was weakened.

No remaining observed Magic Circle regression blocks owner review within the
qualified scope. Broader mod compatibility, uninstall safety and a public release
remain unclaimed. The next safe action is owner review of the branch and candidate.

## Installation and restoration

The candidate is **not left installed**. The existing guarded restore mechanism
restored the original normal-play 0.0.117 backup from
`C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20260919T2203354648740Z`.
All 136 files and their SHA-256 values exactly match that backup, including the
original `FeatureModules.json`; the existing global firearm bank is unchanged.
Kingmaker is closed. [Restoration evidence](../reports/magic-circle/NORMAL-PLAY-RESTORATION.json)
records the checks and hashes.

For a later owner-authorized installation, close Kingmaker, back up the current
KMG folder and preserve `FeatureModules.json`, verify the exact ZIP hash above,
then install that ZIP through the established Unity Mod Manager workflow. Start
with disposable saves and review startup settings. Do not identify candidates by
version alone because this feature intentionally keeps the current version.
Rollback restores the explicit pre-installation folder/settings backup while
Kingmaker is closed; use a save from before the candidate for play on an older
build. Removing blueprint definitions from a save with known circles or active
effects is not qualified.
