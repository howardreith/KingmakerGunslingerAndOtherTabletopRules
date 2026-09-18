# Supervised native action-flow checklist — owner-operated

Why this exists: the corrected action qualification
([NATIVE-RACIAL-ACTION-CORRECTION.md](NATIVE-RACIAL-ACTION-CORRECTION.md))
proves exact native-widget row bindings for all 39 consumers, but not the
ordinary in-game ability-menu lifecycle. Machine tracing established that
`ActionBarManager.Update → SetSpontaneousControls → Hide` closes conversion
popups holding foreign lists each frame, so a fixture cannot keep an arbitrary
racial action list open in that specific popup route. Per the continuation
instruction no native patch, Hide suppression, faked anchor or global override
was used. The remaining ordinary-flow observations are therefore owner-operated
steps on a build that has the icon overhaul installed. Each step is read-only
gameplay; no save write is required beyond the owner's own normal play.

Environment: any installed qualified candidate with Elemental Races enabled;
the party contains at least one elemental-race character (a mercenary with the
race works). Perform at your normal display settings.

## 1. Parent ability menu (any racial SLA parent)

1. Load any save where an elemental-race character with a racial spell-like
   ability (e.g. an Ifrit with Fire Affinity's Burning Hands) is in party.
2. Select that character; open the ability/action bar and locate the parent
   ability icon.
3. Confirm: the icon is the approved painting (not a fallback/letter),
   the tooltip name is exact, the icon is not clipped by the slot frame.

Record: character, ability, screenshot (optional), pass/fail per item.

## 2. Parent → variant navigation (multi-variant parents)

1. On a character owning a variant-bearing parent (e.g. Hydraulic Maneuver or
   Breeze-Kissed Gust), click and hold / right-click the parent ability in the
   action bar so the variant sub-menu opens.
2. Confirm: the sub-menu lists the exact variants (Bull Rush / Dirty Trick
   (Blind) / Disarm / Trip, or Bull Rush / Trip), each with its approved
   painting, correctly spaced, no clipped icons or labels.

Record: parent, variants listed, screenshot, pass/fail.

## 3. Activatable surface (Oread Crystalline Form)

1. On an Oread with Crystalline Form, locate the Deflect Next Ray activatable
   in the bar/toggles area.
2. Confirm: the approved icon is used in both off and on states, and the
   toggle switches without visual artifacts.

## 4. Held-touch delivery (Sylph Stormsoul Shocking Grasp; Undine Rimesoul
   Chill Touch)

1. Cast Shocking Grasp (or Chill Touch) from the bar in a safe encounter.
2. Confirm: the delivery/hold icon shown while the touch charge is held is the
   approved delivery painting, and the parent icon returns after discharge.

These four checks close `ORDINARY_NATIVE_ACTION_FLOW` for the action surface.
Until they are performed, that status remains **pending** and nothing in the
qualification records may describe it as proven. Screenshots can be attached to
the review packet alongside the fixture captures; they are supporting evidence
for exactly these interactions and do not retroactively certify the fixture.
