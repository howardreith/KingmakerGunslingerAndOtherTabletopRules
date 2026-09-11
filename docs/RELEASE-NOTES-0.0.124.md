# Release notes — 0.0.124-teleport-polish-specialist

This release is superseded by the Word of Recall Oracle eligibility
repair: install `KingmakerGunslinger-0.0.126-word-of-recall-oracle.zip`
(0.0.126-word-of-recall-oracle) so an optional Call of the Wild Oracle can
use an existing Word of Recall scroll; it carries the settlement-button and
teleport polish fixes forward.

Owner follow-up mission: teleport UI polish and the genuine Conjuration
specialist-slot repair. Eight requirements from
`Z-TELEPORT-POLISH-AND-SPECIALIST-MISSION.md`.

## What changed

1. **Specialist favorite-slot root cause and repair.** The owner's existing
   Conjuration specialist keeps a serialized per-book special-spell cache
   (`m_SpecialSpells`) that is derived only at school-feature activation
   (`AddSpecialList`, from then-known spells) and at learn time (`AddKnown`,
   when the spell is already in an attached special list). `Spellbook.PostLoad`
   rebuilds only the known-level index, and fact components do not re-run
   `OnFactActivate` on load — so a book whose Teleport knowledge predates the
   Conjuration-list publication keeps rejecting the spell in the favorite slot
   forever, even though the published list contains it. The repair is an
   additive, idempotent load-time reconciliation on `Spellbook.PostLoad`
   (module-gated, fail-soft) restoring, for exactly the published Teleport and
   Greater Teleport spells, the membership native would have cached. No slots
   are granted, no spell is auto-learned, and other schools', lists', and
   books' boundaries are untouched. AllSpellsKnown books self-heal natively
   (`TryRestoreKnownSpells`) and are left alone.
2. **Direct Greater Teleport.** Selecting Cast Greater Teleport (prepared,
   spontaneous, or supported scroll rows, desktop and gamepad) closes the
   destination popup and settles the cast immediately through the same
   transaction/execution machinery — no second confirmation dialog, one
   in-flight guard across direct and confirmed casts, and no dependence on the
   confirmation surface. Ordinary Teleport keeps its risk confirmation; Word of
   Recall and native settlement teleport are unchanged.
3. **Clear confirmation sections.** The retained ordinary-Teleport risk
   confirmation separates caster/destination/uses, familiarity, probabilities,
   and cost with restrained hairline rules measured from the rendered text —
   no text mutation and no shared-prefab change; console keeps its plain
   presentation. Destination-action groups (spell vs scroll families) get the
   same modest separation.
4. **Rows inside the parchment.** Rows are sized from the settled native
   action button's world extent (never the wider dialog canvas group), and
   rendered containment is verified after layout settles.
5. **Arrival copy.** Verified successful Greater Teleport arrivals announce
   nothing (failures and uncertain expenditure still do). Ordinary Teleport
   uses complete sentences: unnamed on-target arrivals read "the party arrived
   at the target location"; unnamed off-target/similar arrivals read "the
   party arrived somewhere else"; useful destination names are retained.

## Verification summary

- 1,574 deterministic domain tests PASS (seven new: arrival templates,
  success suppression, settled-extent width and containment, specialist-cache
  invariant, modal-versus-presenter offer policy, separator-aware viewport
  height).
- Clean Release build and repository validation PASS at the release identity.
- Guarded runtime scenarios updated for the direct cast, the settled-extent
  width containment, the duplicate-input and unrelated-modal boundaries, and
  the divider structural checks; a new
  `disposable-teleportation-specialist-cache` scenario reproduces the
  stale-cache rejection through native seams, proves the load-seam repair for
  both spell levels independently, and casts from both repaired favorite-only
  preparations on the world map. A disposable-save deserialization round-trip
  was not run; visual acceptance at the owner's geometry remains pending
  owner review.

Released as **KingmakerGunslinger 0.0.124** by owner approval on 2026-09-11
after the merged review-corrected candidate passed the full guarded runtime
battery. The save-round-trip lifecycle and manual visual acceptance notes
above remain the honest boundaries of what was verified.

## Upgrade

Install **KingmakerGunslinger-0.0.124-teleport-polish-specialist.zip**
through Unity Mod Manager and restart the game. Existing module settings and
saves are preserved. Affected specialist books re-derive their special-spell
cache on their next load — no save migration is involved.

## Retained baselines

This Kingmaker Gunslinger 0.0.124 release carries the retained qualification
counts forward: the inherited Gunslinger-fixes baseline of 1,288 tests, the
fatigue-authority baseline of 1,325 tests, and the current deterministic suite
of 1,574 tests all pass.

The installable archive is
`KingmakerGunslinger-0.0.124-teleport-polish-specialist.zip`. The qualified
firearm SoundBank is retained unchanged: `KMG_Firearms.bnk` SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Foreign assemblies — `CraftMagicItems.dll` remains an externally installed
optional mod and is never bundled.

