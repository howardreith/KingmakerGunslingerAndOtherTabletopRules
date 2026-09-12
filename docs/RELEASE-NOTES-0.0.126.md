# Release notes — 0.0.126-word-of-recall-oracle

Owner reported mission: Sayan, the owner's Call of the Wild Oracle, could not
use a **Word of Recall** scroll. Word of Recall belongs on the cleric/oracle
list at spell level 6. This release repairs that eligibility only; the
working teleportation system, scroll restrictions, destinations, and
economics are untouched.

## Root cause

The teleportation module publishes the canonical Word of Recall ability into
the native Cleric 6 and Druid 8 spell lists while the blueprint library
loads. Call of the Wild (installed 1.14.4c-2.1) keeps the Oracle class on its
own separate `OracleSpellList` (a distinct blueprint derived from the cleric
list), so that publication never reached an Oracle. The native scroll reader
predicate (`BlueprintAbility.IsInSpellListOfUnit`) walks
`Progression.Classes -> ClassData.Spellbook.SpellList` with reference-equality
membership, so a zero-UMD Oracle was refused and no Word of Recall scroll row
was ever composed for one.

## What changed

1. **Final-live Oracle reconciliation.** At the first idle update after every
   optional mod has loaded — the same qualified pattern Shield Other uses —
   the optional Oracle class is resolved through validated blueprint identity
   and structure (the known Call of the Wild Oracle GUID, a self-owned
   spontaneous divine Charisma spellbook reaching level 6), and the canonical
   Word of Recall ability is merged exactly once at level 6 of that class's
   actual final spell list, twice idempotently, preserving every foreign
   entry and clearing the native filtered-spells cache the level-up spell
   selection reads. This is additive and idempotent: repeated reconciliation
   produces no duplicate entries.
2. **Fail-closed optionality.** Oracle absence, a disabled Teleportation
   Spells module, a failed or absent base publication, or a foreign duplicate
   Word of Recall ability refuses the reconciliation and logs it, without
   disabling any other module and without touching the native Cleric 6 /
   Druid 8 registration or its rollback behavior. Call of the Wild is not a
   dependency; vanilla initialization is unaffected.
3. **Unchanged on purpose.** Canonical spell and scroll GUIDs are preserved,
   so already-owned scrolls benefit without replacement or repurchase, and
   the production lifecycle repairs an existing character's eligibility, not
   only newly created Oracles. Scroll readers are never hard-coded; UMD
   rules, inventory blocking of strategic scrolls, destination policies,
   sanctuary identity, spell level, scroll economics, and the teleport UI are
   untouched. An Oracle still must know the spell and spend a slot to cast it
   from spellbooks; scroll use and learning remain separate native concerns,
   and a level-6 Oracle spell selection can now simply see the spell.

Released as **KingmakerGunslinger 0.0.126** by owner approval on 2026-09-11
after PR #16 merged (master 73b16a95). Tag v0.0.126 published
KingmakerGunslinger-0.0.126-word-of-recall-oracle.zip
(sha256 ee9c00eef064b2b93727051bd55311b622a5c7f8a0d4c695ce725bb370682edf;
release DLL 1ed5dec089b9d6973342d6e9b425f5fa09afcd57bf8641b3c688dfd4e923756e,
Publish-Release default MSBuild pipeline — same source as the runtime-qualified
exact-reference builds of 1e4f4be2, carried into master unchanged by the merge).

## Evidence

- Deterministic domain suite: 1,581 tests PASS (five new Oracle regression
  tests: level-6 merge idempotency, fail-closed malformed lists, reconciler
  source contract, base-publication preservation, scroll identity
  preservation).
- Repository validation PASS at 0.0.126 through the full validator chain;
  clean exact-reference Release build and strict package validation PASS.
- Guarded runtime qualification on the exact release source through the
  Steam App 640820 workflow and the `KMG_AUTOMATION_WORKING` disposable
  working save with Call of the Wild active: before the repair the final
  Oracle list contained no Word of Recall, the native class-list predicate
  refused a genuine zero-UMD Oracle, and no scroll row was composed; after
  the repair the production reconciliation alone places the canonical spell
  exactly once at Oracle level 6, the same Oracle (who still does not know
  the spell) is offered the row at the sanctuary destination, and one
  confirmed cast relocates the party to Oleg's Trading Post, consumes
  exactly one scroll, and spends no spell-slot resource. Cleric and Druid
  readers, UMD behavior, and cancellation controls are unchanged.

## Upgrade

Install **KingmakerGunslinger-0.0.126-word-of-recall-oracle.zip**
through Unity Mod Manager and restart the game. Existing module settings,
saves, and already-owned Word of Recall scrolls are preserved; the repair
changes no persisted identity.

## Retained baselines

This Kingmaker Gunslinger 0.0.126 release carries the retained qualification
counts forward: the inherited Gunslinger-fixes baseline of 1,288 tests, the
fatigue-authority baseline of 1,325 tests, and the current deterministic suite
of 1,581 tests all pass.

The installable archive is
`KingmakerGunslinger-0.0.126-word-of-recall-oracle.zip`. The qualified
firearm SoundBank is retained unchanged: `KMG_Firearms.bnk` SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Foreign assemblies — `CraftMagicItems.dll` remains an externally installed
optional mod and is never bundled.
