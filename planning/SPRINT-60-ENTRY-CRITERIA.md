# Sprint 60 player-facing presentation entry criteria

## Authority and scope

The autonomous mission requires player-facing class names, descriptions,
icons or approved fallbacks, tooltips, localization, and progression display.
ADR-0007 authorizes native fallback assets while keeping custom art
replaceable. This sprint changes presentation metadata only.

## Observable contract

- The Gunslinger class and progression retain nonblank localized names and
  descriptions and the approved native class fallback icon.
- Every project-owned, non-hidden feature or ability reachable from the
  20-level progression has a nonblank localized name and description and a
  nonnull approved fallback icon.
- Existing nonnull native or firearm-specific icons are preserved.
- Hidden markers, transient facts, and diagnostic implementation details stay
  hidden and are not promoted into progression UI.
- Progression UI groups organize existing level entries without adding,
  removing, moving, or duplicating mechanics.
- Localization keys remain stable and unique; English fallback text remains
  available when no external localization pack is installed.

## Qualification

- Source validation enumerates the presentation normalizer and its exclusions.
- Focused tests cover graph traversal, preservation, hidden-fact exclusion,
  and duplicate/reference handling where dependency-free modeling is useful.
- A guarded save-free runtime scenario inspects the registered class,
  progression, all visible reachable project facts, and UI groups.
- Mod-load smoke and two fresh-process feature PASS runs are required.

## Non-goals and failure behavior

No custom art, asset bundle, model, sound, animation controller, mechanical
feature, deed rule, level entry, or localization language is added. Missing
required presentation metadata fails bootstrap/qualification closed; it is not
silently hidden or replaced with internal blueprint names.
