# Native UI review packet — icon overhaul candidate (2026-09-18)

One compact review page for the owner's final native-UI acceptance. It concerns
**final UI integration on the candidate below**, not a second vote on the
approved paintings (those remain closed). Read with
[ICON-EVIDENCE-LEDGER.json](ICON-EVIDENCE-LEDGER.json).

## Candidate

See [ICON-EVIDENCE-LEDGER.json](ICON-EVIDENCE-LEDGER.json) `finalCandidate`
for the exact package/DLL/MVID of this review round. All captures referenced
below were taken by guarded runs on that artifact unless explicitly marked
historical.

## What to review (highest-value contexts)

1. **Racial action rows (39 consumers).** Curated captures per race from the
   corrected action runs (Ifrit/Oread/Sylph/Undine directories under
   `runtime-evidence/`, `native-racial-action-row:*` / `native-action-control:*`
   stages): wide portrait-strip shots plus per-row captures showing the exact
   painted sprite in the native converted-slot widget. The pinned native Fight
   Defensively control row appears alongside (`native-action-control`).
   *What this proves:* native widget binding/rendering. *What it does not:*
   ordinary menu flow — see item 4.
2. **Higher firearm feat roots (12 combinations).** FeatureUIData monogram
   identities (P/M/B, null icon) for every committed root/weapon fact, and the
   legal Fighter 1–19 ladder with refused-below-prerequisite controls:
   run `20260918T1350433937364Z-disposable-firearm-higher-feat-roots`
   (`selectionLog` in the assertion payload shows each level's slot, offered
   route and refusal). In-game screen: open a character sheet / level-up Total
   for a firearm-fact character (the supervised checklist covers this in one
   step).
3. **Wakizashi clipping + P/M/B beside native/eastern controls.** This session
   produced no new 1920×1200 captures of these specific contexts; per the
   continuation instruction the earlier lower-resolution evidence is **not**
   presented as final. Owner step: at your normal 1920×1200 settings, view an
   equipped Wakizashi and the P/M/B monogram entries next to native Weapon
   Focus and Katana/Nodachi entries; the captures you take are the acceptance
   evidence (two or three screenshots suffice).
4. **Ordinary racial action flow.** Perform the four short steps in
   [SUPERVISED-NATIVE-FLOW-CHECKLIST.md](SUPERVISED-NATIVE-FLOW-CHECKLIST.md)
   (parent menu, parent→variant navigation, Crystalline Form activatable,
   held-touch delivery). Passing these closes `ORDINARY_NATIVE_ACTION_FLOW`.

## Statuses (kept separate)

| Milestone | Status |
|---|---|
| Approved art (90) | CLOSED (owner; hashes re-verified no-write 2026-09-18) |
| Exact loaded bindings | CLOSED (census, painted-integration artifact) |
| Native-widget rendering (39 + control) | PASS on the corrected candidate |
| Higher firearm roots (12) | PASS (real selection ladder; screen step in checklist) |
| Ordinary native action flow | PENDING — supervised checklist |
| Saved-parameter disk round trip | NOT RUN — authorization/input required |
| Owner final native UI acceptance | PENDING — this packet |

## Historical captures

The 379 captures from the September 17/18 action runs and earlier qualified
runs remain evidence for their recorded artifacts and scopes only; none are
re-labeled as belonging to the current candidate. Where this packet reuses a
context (e.g. the v0.0.129 scroll/Oracle records), the ledger entry names the
original artifact.
