# Native racial action qualification — correction and superseding scope record

Recorded: 2026-09-18 (Z executor, review-driven continuation).
Supersedes the *scope claims* of
[NATIVE-RACIAL-ACTION-QUALIFICATION.json](NATIVE-RACIAL-ACTION-QUALIFICATION.json)
for runs on artifact `f0c234df…`/DLL `7d5191df…`. The underlying observations
(39/39 exact widget bindings, capture sequences, restoration measurements) from
those runs remain valid evidence of what they measured; this record corrects
what they establish overall and documents four source-review findings that were
fixed afterward.

## What the qualified runs did and did not establish

| Milestone | Status on `f0c234df…` (historical) | Status on the corrected artifact |
|---|---|---|
| Approved art (90 images) | CLOSED (owner approval, unchanged) | CLOSED (re-verified no-write) |
| Exact loaded bindings (census) | CLOSED (separate painted-integration runs) | CARRIED OVER unchanged |
| Native-widget row binding/rendering of the 39 consumers | **PASS** — exact native `ActionBarSpontaneousConvertedSlot`/activatable widgets, native `Initialize`/`Set`/`InitSlot` icon path, exact sprite references, catalog-order captures | Re-qualified with strengthened assertions |
| Rendered pinned control (Fight Defensively) observation | **NOT PROVEN** — finding A1: the control widget was created but never captured/asserted; final PASS did not require it | **Required** by the shared evidence evaluation (identity, rendered sprite, active row, preserved initial state, capture record) |
| Fixture restoration (facts/selection/world/pause/widgets) | Partially proven — findings A2/A3: modifier-cache `Reject` unenforced; `restored` computed before pause release; pause-restoration failures not folded into the result; disposal check covered only converted rows; one comparison used an always-empty placeholder list; bar/popup "not borrowed" was asserted unconditionally | Enforced: `Reject` fails before mutation; preserved caches compared by instance and ordered entry identities; final `restored` evaluated after all cleanup including pause equality; every owned widget tracked at acquisition and checked released; bar owner + popup active/slot-count compared before/after |
| Ordinary native action-menu flow (opening, parent→variant navigation, action-bar lifecycle, held-touch transitions, placement in usual containers) | **NOT ESTABLISHED** and not claimed after this correction | **PENDING** — see below |
| Owner final native UI acceptance | PENDING | PENDING |

## Why ordinary menu flow is pending, precisely

The qualification route bound native row widgets through the conversion-popup's
own `FillSlots` path and reparented them to the portrait strip. That proves
native widget rendering and icon binding. It does not prove the ordinary
in-game ability-menu lifecycle. Machine tracing (temporary request-armed
Harmony prefix on `ActionBarSpellsGroup.Hide`, installed only during diagnosis
and removed from the final source) established that
`ActionBarManager.Update → ActionBarSlots.Set → ActionBarIndexSlot.Set →
ActionBarGroupSlot.SetSpontaneousControls → Hide` reconciles every conversion
popup each frame and closes any popup whose list is not the anchor slot's own
spellbook conversion. That is evidence about the attempted foreign-list route
only; it is **not** proof that the legitimate racial ability UI has no usable
path. Per the continuation instruction, no patching of `ActionBarManager.Update`,
no Hide suppression, no faked conversion anchor and no global UI override were
used to force an artificial popup open. The bounded representative native
interactions (parent/variant navigation in a real bar, the real activatable
surface, applicable touch-delivery presentation) remain a separate, honestly
pending gate; a supervised owner-operated checklist is provided alongside this
record.

## Separated statuses going forward

1. `EXACT_BINDINGS_AND_WIDGET_RENDERING` — per-consumer native-widget binding
   and rendered sprite identity.
2. `RENDERED_CONTROL_AND_RESTORATION` — pinned control observation plus every
   fixture-owned object's verified cleanup.
3. `ORDINARY_NATIVE_ACTION_FLOW` — real menu opening/navigation/lifecycle;
   pending, never inferred from detached `AbilityData` or reparented widgets.
4. `OWNER_FINAL_UI_ACCEPTANCE` — owner decision on the final artifact; pending.

Historical runs on `f0c234df…` are retained with their original artifact
identity and limits; their PASS is not retroactively re-labeled as satisfying
assertions that did not exist at the time. Corrected results, where required,
are attributed to the corrected artifact qualified after this record.
