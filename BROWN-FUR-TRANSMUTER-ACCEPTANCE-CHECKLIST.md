# Brown-Fur Transmuter human acceptance checklist

Status: **HUMAN REVIEW FAILED / SUPERSEDED**.

This checklist records the rejected `0.0.81` candidate. It does not authorize
the final 128-state matrix. A replacement checklist will be prepared only after
the `0.0.82` repair passes automated and focused runtime qualification.

## Candidate identity

- Engineering base: `a8b19fe39285da44ac443b7bcbd217870ec6ffb6`
- Cleanup human acceptance: **PENDING / intentionally deferred**
- Brown-Fur base authority: explicit user override permitting development from
  the pre-human cleanup candidate
- Source commit: `2ef6e933ff521dff2330a948336a38083e741082`
- Version: `0.0.81-brown-fur-transmuter`
- Package: `KingmakerGunslinger-0.0.81-local-runtime.zip`
- Package SHA-256:
  `883751EDD4AF3427CCE712C2C875EEA0E4B6CCB955E9A2778176E5CE1C2BA7BE`
- DLL and installed-DLL SHA-256:
  `9F0EFEE718F58F2B87292D993A6EC4973810510DF7193612DC5BD220EE04A8CF`
- DLL MVID: `30b587a9-39f6-42c5-a5df-1ce32d73974f`
- Deployment identity: `20260815T2354477268935Z`
- Deployment manifest SHA-256:
  `8ACF004E570BC3319F5A10C8E076DFB473268C3772782E2DC5467CA7908B85B6`
- Deployment backup identity: `20260815T2354434393118Z`
- CotW: `1.14.4c-2.1`; DLL SHA-256
  `4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`;
  MVID `8caab254-aacf-4811-8093-44b9184e6e53`

## Human findings

- Powerful Change score actions did not expose a sufficiently legible native
  armed state; Strength was unintentionally disarmed and Bull's Strength cast
  normally at +4 without reservoir cost.
- Cat's Grace later reached +6 Enhancement while Dexterity was actually armed,
  confirming that the descriptor-preserving bonus path worked but the player
  interaction did not.
- The six score icons were too similar, action descriptions omitted operational
  rules, and consuming actions did not show the live Arcane Reservoir count.
- With Share armed, Beast Shape II, Undead Anatomy, and Resinous Skin
  immediately self-cast instead of entering creature-target selection.

## Superseded presentation checklist

- [ ] The UMM switch visibly says `Brown-Fur Transmuter  requires Call of the Wild`.
- [ ] Available, Unavailable, and Blocked dependency states are distinct and understandable.
- [ ] Active-this-process and saved-next-restart values are distinct.
- [ ] Effective publication and restart-required states are distinct and understandable.
- [ ] Brown-Fur appears under the CotW Arcanist when the module is ON.
- [ ] Brown-Fur does not appear for new selection after restarting with the module OFF.
- [ ] Existing CotW Arcanist archetypes remain visible and in their prior order.
- [ ] Progression shows Powerful Change at 3, Share Transmutation at 9, and Transmutation Supremacy at 20.
- [ ] The replaced exploit opportunities and removed Magical Supremacy are clear in progression presentation.
- [ ] The Powerful Change parent action and six ability-score choices are understandable and native-feeling.
- [ ] Share Transmutation's Touch delivery feels native at level 9.
- [ ] Share Transmutation's exact 30-foot delivery feels native at level 20.
- [ ] Tooltips accurately explain eligibility, one-point costs, combined two-point cost, Arcanist-slot restriction for Powerful Change, and willing-creature restriction for Share.
- [ ] No archetype, feature, buff, grant, or action-bar choice appears duplicated.
- [ ] A representative single-stat spell visibly receives the correct enhanced descriptor bonus.
- [ ] A representative multi-stat spell visibly enhances only the chosen stat.
- [ ] Representative combined Powerful Change plus Share behavior agrees with the structured evidence.
- [ ] Transmutation Supremacy's duration behavior agrees with the structured evidence and does not double an already Extended spell.

## Acceptance record

- Reviewer: ______________________________
- Review date/time and timezone: ______________________________
- Decision: **CHANGES REQUIRED / NOT ACCEPTED**
- Accepted source commit: ______________________________
- Accepted package SHA-256: ______________________________
- Accepted installed DLL SHA-256: ______________________________
- Notes: ______________________________

Do not run the final exhaustive matrix from this checklist. Create and qualify
a new immutable candidate, then conduct a new human review.
