# Overnight Gunslinger Bug-Fix Implementation Report

## Issue 7 - Border Sentinel later organic placement

The exact item is the stable +1 cold-iron nodachi `c1c7a6746916504ebfdcb2b650a7145b`, price 4,420 gp. The prior Eastern campaign spec deliberately placed it at Oleg. A save-free live inventory of 437 bounded fixed targets selected `PoorHuman_treasure_chest_03` (`c8b8159fb695be64883b609a7e77e75d`) in base-campaign `StagLordFort`: a later late-Act-I fortification source with zero registered direct references, fixed native horseshoe/gold contents, and no project unique.

The existing Eastern transaction now desires no named item at Oleg and desires Border Sentinel once at that separate chest. Its established owner-wide normalization removes only project-owned Eastern rows from wrong publication targets, preserves all native/foreign entries and order, validates five distinct loot targets/twelve fixed rows/all eighteen singular named items, and restores exact pre-publication arrays on failure. Live observation scans every shared vendor and every registered `BlueprintLoot`, not only declared targets. A development-only read-only action reports exact item, target, area, contents, count, and current-area match without gameplay mutation.

Status: MISSION CHECKPOINT IN PROGRESS

## Baseline

The mission starts from fetched published origin/master
d13268d3abe9ffe89c8195b213c1eee194328672 on the isolated branch
codex/gunslinger-overnight-bugfixes. Version is 0.0.87 and the unchanged source
passes repository validation, 1,150 deterministic tests, clean exact-reference
Release, build-output, SoundBank, deterministic package, and strict package
validation.

No production behavior has changed at this checkpoint. Fresh human reports
reopen the twelve matrix rows and supersede contradictory historical automated
acceptance. Each subsequent section and commit will remain issue-scoped and
will state implementation, tests, runtime evidence, uncertainty, and any
remaining human gate.

## Issue status

Issues 1 through 12 are pending in the controlling order. The exact current
state is maintained in planning/OVERNIGHT-GUNSLINGER-BUGFIX-MATRIX.md.

## Issue 1 - Acadamae Graduate

Source-qualified candidate in progress. The prior tracker recognized a
completed cast only while its command was the thread-local active `OnAction`
command. The repair attaches the concrete `RuleCastSpell` created by that
eligible command to the tracker entry and consumes the rule exactly once at
the terminal callback, even when that callback is delayed. Failed casts and
canceled commands consume their entries without a save.

The action policy, toggle, prepared-arcane eligibility, exact Summoning marker,
DC, canonical Fatigued blueprint, null MechanicsContext, permanence, and Cord
contract are unchanged. New bounded diagnostics expose the actual d20,
Fortitude modifier/total, DC, outcome, and fatigue disposition for eligible
completed casts only. The guarded scenario now includes actual native command
success and failure paths instead of proving those paths through manual tracker
calls. All source/build/package gates pass; guarded runtime remains pending the
immutable issue commit.

The first immutable runtime attempt exposed a fixture limitation before any
assertion: a detached `ChargenUnit` cannot advance a queued command through its
animation controller in one synchronous tick. The corrected save-free fixture
now invokes the exact protected `UnitUseAbility.OnAction()` boundary. It does
not manually call the production tracker or construct `RuleCastSpell`, so the
native action and all repaired Harmony correlation points remain exercised.

That direct native action also proved unavailable on the detached fixture: an
installed composed `OnAction_Patch3` dereferenced loaded-area state. The final
safe automated mode therefore targets the production defect itself. It creates
the native `RuleCastSpell` while the exact command scope is active, ends that
scope, and only then triggers the rule. This requires the new constructor-time
rule identity retention to work and continues through native Rulebook save and
fatigue handling. It does not claim animation-driven execution or area change;
those remain human-gated.

The first completed delayed-terminal run proved correlation and consequences,
but found the new diagnostic labels were inverted by the old forced-roll
fallback. Installed getter IL proves `BaseRollResult` is d20 plus `StatValue`,
and `RollResult` adds a conditional success bonus. The test postfix no longer
writes `BaseRollResult`; natural rolls are controlled only at native
`RuleRollD20.PreRollDice`. Production diagnostics now report the native d20,
Fortitude `StatValue`, conditional bonus, final total, DC, outcome, and fatigue
disposition separately.

Final automated disposition: guarded run
`20260820T0428503321600Z-d97a49371e1949c89f3de25aac1c6eff` passed all 14
assertions on published commit `f807eb1cc3dabf9dc66acaa2b773c029a72dc942`.
The installed saving-throw path did not call `RuleRollD20.PreRollDice`, so the
guarded-only completion control now sets `BaseRollResult` to the requested
natural plus the actual native `StatValue`. Production diagnostics and
uncontrolled saves remain native. Loaded-area animation execution, area-change
persistence, and visible UI/log presentation remain consolidated human checks.

## Issue 2 - Focused Aim

Source-qualified candidate in progress. The prior ability manually debited the
shared Grit resource before activating its timed damage marker. The candidate
reverses that vulnerable ordering: it first establishes the marker, then
rechecks the live True Grit decision and authoritative shared pool, commits the
effective debit, and verifies the exact post-spend amount. Any unavailable or
inconsistent debit removes the marker, so damage cannot survive a failed spend.

Exact ability-fact ownership is required, armed duplicates remain unavailable,
ordinary uses spend one, and a legally selected True Grit Focused Aim remains a
zero-cost activation requiring positive Grit. The native `AbilityResourceLogic`
UI component still points at the same shared resource and remains presentation
only. A new guarded scenario exercises the real resource collection and native
weapon-stat rule for firearm damage and crossbow isolation. Runtime is pending
publication of the immutable candidate.

## Issue 2 - Focused Aim transactional Grit spend

The verified live defect was not fact-activation reconciliation. Kingmaker materialized the exact Focused Aim buff in the owner's `RawFacts` collection while returning null from `Buffs.AddBuff`. The previous code treated that return value as an activation failure and restored the resource snapshot, but the surviving marker continued to grant the firearm Charisma damage bonus.

The repair resolves only the exact project-owned marker blueprint from `RawFacts` when the native return is null. It then evaluates the established True Grit policy against the live owner, spends the shared Grit resource once, verifies the exact before/after delta, and removes the marker while restoring the snapshot if the commit cannot be proven. Ability ownership, positive-Grit True Grit semantics, firearm-only damage, wrong-owner rejection, duplicate rejection, and feature lifecycle reconciliation are retained.

Commit chain: `24c2bae`, `c8f58e0`, `75d7bc0`, `de46051`, `24ffb78`. Guarded run `20260820T0458550047640Z-b20727d24cc24d3297e0e9d23d385235` passed all seven assertions against `24ffb78ac6821d4aec173213df1f046940be683b`.

## Issue 3 - Firearm Touch AC range and feedback

The existing runtime architecture was retained: the attack-roll frame captures exact firearm identity and per-attack range modifiers, while `RuleCalculateAC` re-reads authoritative participant distance and target AC values. The decision service now applies the repository's completed era policy rather than the Sprint 9 advanced-firearm deferral: one effective increment for early firearms and five for advanced firearms. The contextual ordinary-to-touch delta remains additive, so cover and other native event adjustments are preserved.

Production and magic firearm descriptions plus the dynamic Qualities surface now state exact base penetration distances. Blunderbuss wording limits this to ordinary direct fire and preserves Scatter Shot's cone rules. A new exception-contained player log adapter reuses the existing native warning/battle-log event and publishes one line after a resolvable exact-firearm branch commits. Duplicate callbacks are stamped, and adapter/log failures do not alter native attack processing.

No established safe pre-attack hover seam was found after bounded searches of tooltip, cursor, target-preview, and combat-log adapters. The battle-log fallback is therefore the production candidate; pre-attack hover remains human/UI follow-up rather than a new framework.

## Issue 4 - Acadamae Graduate prerequisite presentation

The selected feat contained one correct `PrerequisiteAcadamaeGraduate` component, but its localized rules description also began with a manual prerequisite sentence. Kingmaker therefore presented the same specialist-Wizard and Conjuration eligibility contract through both the native prerequisite renderer and description body.

The repair removes only the manually embedded sentence. The exact prerequisite component, its `Check` implementation and `GetUIText()` output, feature GUID/identity, feat group, publication order, module toggle, and `AddFacts` mode grant are unchanged. A focused semantic test isolates the description localization span rather than snapshotting the full description, and the guarded runtime scenario inspects the live registered feature for one prerequisite component, retained native prerequisite text, and absence of the duplicate prose.

Commit: `78bc46d21b71dbfb35d430d00755228348afb751`. Guarded run `20260820T0524095518410Z-ea7adae339fc4fa4a98bfe7bd52b4222` passed all 15 assertions. Final visual confirmation in the actual feat-selection UI remains human-gated.
## Issue 5 - Oleg maintenance kits

Published code `1586c5e7abd9c8d1b18bac483df88e86700677b0` adds a dedicated exact-table publication transaction for `C11_OlegVendorTable` (`f720440559fc00949900bfa1575196ac`). The transaction owns only `KMG_FirearmRepairKit_Item` and `KMG_OverhaulKit_Item`, publishes counts 5 and 2, retains all native/foreign component references in relative order, is idempotent at the exact state, normalizes only project-owned stale rows, and participates in reverse-order bootstrap rollback.

The read-only live observer also freezes the two direct owners: `OTP_Oleg` (`5db389e0409ef534d81358555e6ab99d`) and `OTP_Oleg_FirstVisit` (`67db4b8bacc69e643880f0a4ed6dff6f`). Guarded run `20260820T0535269245782Z-7edf2d2c158a4085893931e91b14db1d` passed 20/20 and observed exactly one row at each required count. No player inventory or save-owned shop state is mutated. Future/unmaterialized static-table behavior is qualified; refresh of an already-materialized merchant remains unclaimed.
## Issue 6 - Bokken ammunition supplies

The earlier shared-vendor investigation was incomplete because its unit loop continued before examining `AddVendorItems` when a unit had no `AddSharedVendor`. Published forensic commit `3b29451f24cc163f48f03150cce0e7563165beaa` added bounded localized/dialog/unit metadata and outgoing/reverse-reference evidence. It resolved `OTP_Bokken` and `OTP_Bokken_ZeroState` as the only direct owners of `C11_BokkenVendorTable` (`4778ecb5df5d48742b9be5a204ed4657`), an exact `BlueprintUnitLoot` rather than a shared vendor table.

Published fix `73e776a91167bc02024e7be794a822fa63fec48e` adds a dedicated transaction for that exact type/GUID/name. It owns only the existing canonical Black Powder, Lead Ball, and Paper Cartridge item references, publishes each once at 100, preserves all native/foreign component references and relative order, removes only owned stale rows, is idempotent at the exact state, and performs guarded exact-snapshot rollback. No Jhod, capital, BTSL, player inventory, or save-owned shop state is mutated.

Guarded run `20260820T0555507894600Z-0772504de3254a64986e6ea2da172a02` passed 23/23 with all three exact rows at `1/100`, both exact owners, and all 21 existing Bokken stock rows retained. Merchant UI materialization remains human-gated.

## Issue 8 - Firearm Wwise regression audit

The fresh human report that Pistol and possibly other firearm sounds are absent supersedes the earlier listening record. The bounded investigation did not find an evidence-supported replacement for the existing native-Wwise path: `KMG_Firearms.bnk` remains byte-identical to the qualified post-polish bank, all five approved source WAVs contain nonzero PCM, and the bank contains five unique nonempty in-bounds embedded media entries. The staging path now rejects a bank unless its BKHD, DIDX, DATA, and HIRC chunks and exact five-media cardinality are structurally valid.

The guarded preview boundary now accepts an exact `FirearmKind` and the save-free scenario posts all five exact Event families before exercising a live Blunderbuss preview, ordinary committed discharge, and forced misfire. Commit `d8fd4ad1836f3ad4d9b54dec908d7818725c64d1` preserves the existing bank, emitter, and mechanical transaction architecture while strengthening bank validation and event-family coverage.

Run `20260820T0635323959656Z-88cfa04a0deb4595bfbc2ee8d4284e31` passed 6/6: state Ready, one bank load, Pistol/Musket/Blunderbuss/Revolver/Rifle playing IDs 2-6, live Blunderbuss preview ID 7, ordinary committed Blunderbuss ID 8, and no additional post for forced misfire. Repository, authoring, deterministic-source, SoundBank, 1,158-test, clean Release, output, package, strict-package, and diff gates passed. No bank, source recording, or AssetBundle bytes changed.

This is automated event-routing qualification, not an audible-fix claim. Kingmaker/Wwise can return a nonzero playing ID without establishing speaker output, mix, or absence of inherited layers. Issue 8 therefore remains human-gated for fresh five-family listening, empty/rejected/misfire silence, Scatter once-per-volley, and crossbow release/bolt suppression.

## Issue 9 - Native-style firearm feat icons

The generic presentation came from one shared parameter sprite: all five `KMG_WeaponFocus_*` parameter blueprints resolved to the target icon, and those same parameters feed Weapon Focus, Greater Weapon Focus, both Weapon Specializations, and Improved Critical through native `FeatureUIData`. Rapid Reload likewise painted its top-level feat and all five children with one bright square icon.

Published commit `15fff80bef6d6319c5281343264891ce13aa7b4a` adds original deterministic P/M/B/Ri/Rv circular monograms, a separate muted salmon reload-arrow/ramrod icon, editable JSON/drawing source, a 64/32-pixel map, and exact hashes. `ProjectAssetIcons` assigns each kind's monogram to the shared native parameter, Rapid Reload child, and every legacy dependent child while leaving native top-level feats and non-firearm choices untouched. Rapid Reload retains its own semantic top-level sprite.

The exporter reproduced all seven output hashes byte-for-byte. Repository, 1,159-test, clean exact-reference Release, output, SoundBank, deterministic 137-file package, strict-package, and diff gates passed. Guarded run `20260820T0659308177934Z-65bd0924b3dc455ebb04097f00172d90` passed 13/13 against the published SHA, including exact full/unit-aware native-menu icon maps, original native top-level sprite names, Rapid Reload/dependent choice mappings, selection commit, prerequisites, effects, isolation, and legacy compatibility. Final aesthetic judgment at actual UI scale remains human-gated.
