# Supervised native UI checklist — owner-operated (rev. 2, catalog-verified)

Why this exists: the fixture qualifications prove exact native-widget binding
(racial actions) and real selection-ladder data (higher firearm feats). They
do **not** prove ordinary in-game menu flow or actual screen rendering for
every context. These owner-operated steps close that gap. Each step names the
exact expected art identity from the canonical catalog — verified against it
on 2026-09-18, not guessed.

**Setup (smallest practical):** one supervised session with the verified
candidate artifact installed (see the review packet for its exact package
hash). A human-initiated load of `KMG_AUTOMATION_WORKING` (it contains
elemental-race characters) covers steps 1–4; a human-driven level-up of a
disposable mercenary covers step 5. No new save write, campaign access or
automated input is required or authorized by this checklist. Where the exact
UI gesture is machine-verified it is stated; where it is not, the step is
marked **[supervised discovery]** — open the ability's variant list by the
game's own control (the same path the Expanded Summoning integration uses:
the action-bar group slot's toggle, `ActionBarGroupSlot.OnToggleGroupClick`).

**State-changing steps are marked STATE-CHANGING.** Casting, toggling and
level-up commits change live game state even when no save is written. Perform
them on disposable characters only.

## 1. Racial parent action — original art vs deliberate native reuse

| Context | Expected |
|---|---|
| Undine Acid Breath parent ability (action bar/ability list) | **owned-v2 painting** (`owned-v2-painting` disposition), exact approved art |
| Ifrit Burning Hands parent ability | **the native Burning Hands spell icon — deliberately NOT a painting** (native-reuse disposition `native-BurningHands`) |

Pass = Acid Breath shows the approved painting with no clipping; Burning
Hands shows its native spell icon (a painting there would be a defect).

## 2. Parent → variant navigation

Hydraulic Maneuver parent: open its variant list through the game's own
control **[supervised discovery if the gesture differs from the action-bar
group toggle]**. Expected children, each its own **owned-v2 painting**, in the
native variant-list container: Bull Rush, Dirty Trick (Blind), Disarm, Trip.
Pass = all four children present, each with its approved art, correctly
spaced, no clipped icons or labels.

## 3. Activatable surface

Oread Crystalline Form "Deflect Next Ray" toggle: expected **owned-v2
painting** in both off and on states with the native toggle-state indicators.
Toggling is **STATE-CHANGING** — perform it once on a disposable character;
the native off/on visuals must switch without artifacts.

## 4. Held-touch presentation — two different expected-art cases

| Context | Expected |
|---|---|
| Undine Rimesoul Chill Touch + its held-touch delivery | **owned-v2 paintings** (both parent and delivery are original art) |
| Sylph Stormsoul Shocking Grasp + its held-touch delivery | **native Shocking Grasp / native delivery identities — deliberately NOT paintings** |

Casting, holding the charge and discharging are **STATE-CHANGING** — perform
on a disposable character in a safe encounter. Pass = Chill Touch shows the
original art through the delivery state and returns after discharge;
Shocking Grasp shows its native identity throughout.

## 5. Higher firearm feat screens (four roots × three weapons)

On a human-driven mercenary level-up with firearm proficiency (the same legal
progression the qualified scenario performs):

- **Selection/preview:** open each higher root's native parametrized menu —
  Greater Weapon Focus, Weapon Specialization, Greater Weapon Specialization,
  Improved Critical. Each must show the three firearm entries **Pistol,
  Musket, Blunderbuss** with native P/M/B monogram lettering (null sprite +
  acronym), ordered, beside the native weapon entries. Representative
  sampling: capture all four root menus once each with the three entries
  visible; the weapon dimension is identical UI machinery across roots, so
  4 menus (not 12 screens) close this surface — say so in the record; do not
  imply 12 individual captures.
- **Cancellation/back navigation:** enter a root menu, back out without
  selecting, enter a different (non-firearm) feature list, and confirm the
  native list layout returns to normal (this observes the
  `FirearmNativeFactSlotMonogram` adapter's cleanup: border/mask sequence,
  scroll extent, no residual lettering).
- **Total/final review + character sheet:** complete one actual selection
  (STATE-CHANGING, disposable character) of e.g. Greater Weapon Focus
  (Pistol); the final review and the character sheet must show the selected
  fact with the native P/M/B monogram. The other root/weapon combinations are
  covered by the equivalent-path note above unless a defect is seen.

## 6. Wakizashi and P/M/B in the actual weapon selector

The original complaint concerns the **weapon-category monogram inside the
parametrized weapon selector** (the list where Weapon Focus / Exotic Weapon
Proficiency choices appear), not the equipped-weapon illustration or the
inventory art. At the owner's **verified normal resolution/UI scale**
(historical 1920×1200 is the starting reference — do not silently change
current display settings; if a temporary change is wanted it needs explicit
authorization and exact restoration), open that selector and check the
Wakizashi monogram beside the native Weapon Focus entries and the Katana /
Nodachi eastern entries, plus the KMG P/M/B entries. Pass = no clipped
monogram letters or labels at that scale; a genuine art defect found here is
reported with the exact image/hash for separate permission — not fixed here.

## Statuses (kept separate and current)

| Status | Value |
|---|---|
| `HIGHER_FEAT_SELECTION_AND_DATA` | PASS (guarded scenario, corrected fixture) |
| `HIGHER_FEAT_CANCELLATION_AND_CLEANUP` | PASS after re-qualification on the new artifact (snapshot-proven) |
| `HIGHER_FEAT_NATIVE_SCREEN_RENDERING` | PENDING — step 5 above |
| `RACIAL_WIDGET_BINDING` | PASS (guarded fixture; widget binding, not menu flow) |
| `ORDINARY_NATIVE_ACTION_FLOW` | PENDING — steps 1–4 above |
| `SAVED_PARAMETER_ROUND_TRIP` | NOT RUN — authorization/input required (request rev. 3) |
| `OWNER_FINAL_NATIVE_UI_ACCEPTANCE` | PENDING — review packet |

Prior Weapon Focus/Rapid Reload screen captures remain evidence for those
contexts only; they are not captures of the higher roots, and a FeatureUIData
null sprite + acronym is the data contract, not the rendered Total/sheet.
