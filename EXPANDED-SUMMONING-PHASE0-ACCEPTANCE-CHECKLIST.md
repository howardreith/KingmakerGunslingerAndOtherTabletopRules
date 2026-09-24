# Expanded Summoning Phase 0 owner review checklist

Nothing below has been accepted. Technical PASS is not owner approval, and no
item here may be ticked by the implementation thread.

This checklist covers Sprints 0-2 only. **Sprint 3 is not started.**

## What to expect before you look

**Exactly one thing should look different: Pteranodon.** Sprint 2 replaces its
borrowed body with an original pterosaur visual. Everything else is unchanged,
and any other visible difference is a defect rather than the feature.

Changed on purpose:

- Pteranodon's model, textures and silhouette.

Unchanged, and a difference here is a defect:

- Visible summon choices are still **693** (371 Summon Monster, 322 Nature's
  Ally).
- No creature was added, removed, hidden, or unhidden. Dire Bat stays hidden.
- Pteranodon's identity, Summon Monster IV / Nature's Ally IV placements,
  higher-tier quantities, statistics, reach, attack cadence, alignment
  templating, AI, duration and persistence.
- Every other creature's appearance, including Eagle and Roc, which share
  Pteranodon's donor prefab and are the negative controls for this work.
- Ordinary menu semantics: names, icons, tooltips, ordering and quantities.

## 1. Menus - confirm nothing moved

Open a caster with a broad summon selection and check each parent spell.

- [ ] Summon Monster I-IX show the same options, in the same order, as before
      this branch.
- [ ] Summon Nature's Ally I-IX likewise.
- [ ] The largest menus (Summon Monster VIII and IX, 69 choices) open, scroll
      and select exactly as they did before.
- [ ] Icons, names, tooltips and quantity labels are unchanged.
- [ ] Dire Bat is still absent from every menu.
- [ ] Any third-party or Call of the Wild summon entries you normally see are
      still present and in their usual position.

## 2. Pteranodon - the finished creature

This is the one thing that should look different. Every image inspected so
far was rendered by the implementation thread; nobody else has seen it in the
game, and only you can accept it.

- [ ] Cast Summon Monster IV and Summon Nature's Ally IV for a single
      Pteranodon. It is a pterosaur, not an eagle: long toothless beak, a
      backswept crest with its tip as the highest point, leather wings with
      no feathers, a short tail stub. The crest reads from the party camera.
- [ ] Colours sit with the game's palette: countershaded brown-and-cream
      body, paler throat, warm-red crest, horn beak, leathery tan wings with
      fibres running across the chord.
- [ ] Idle, walking, turning, biting, being hit and dying all move the whole
      body - wings included - with nothing tearing, lagging or staying behind.
- [ ] Its bite still lands at its usual reach and timing, and its scale is
      unchanged against Eagle and Roc.
- [ ] Higher-tier 1d3 and 1d4+1 Pteranodon casts show every unit with the
      new body.
- [ ] Eagle, Roc and (if you unhide it) Dire Bat look exactly as before.
- [ ] Selection circle, shadow, hit flashes and death all behave as before.
- [ ] Save, reload, dismiss, kill and let one expire - all as before.
- [ ] Celestial/fiendish templating still applies as before for your caster's
      alignment.

## 3. Judgement calls I made that you may want to overrule

These are decisions, not findings. Each is reversible.

- [ ] **No new menu UI was built.** The projected worst single menu is 124
      choices, up from 69 - a factor of 1.8, not the 693-to-1,232 the aggregate
      suggests. The shipped menu policy already clamps to the safe rectangle
      and scrolls, and passed a rubric fixed before measuring at four
      resolutions and every option count from 1 to 200. The charter allows one
      bounded presentation solution *when the existing UI is shown to fail*; it
      did not fail. If you would still prefer paging or grouping at 124
      choices, say so and it becomes a scoped piece of work.
- [ ] **Borrowed-body proxies are counted as 20, not 67.** A view policy naming
      the creature itself (`boar<Boar`) is an exact visual, not a proxy.
      Counting those would have overstated the remaining art work threefold.
- [ ] **No creature was marked verified or accepted.** All 66 live creatures
      sit at `Published`; `TechnicallyVerified` and `OwnerAccepted` are empty
      and only you can fill the latter. The charter's Appendix B "Done"
      classification is recorded separately as `CharterBaselineComplete` rather
      than being converted into acceptance.
- [ ] **The charter baseline was superseded, not reset to.** Work is on
      `master` at 0.0.136, not 0.0.114.

## 4. Outstanding - disclosed, not waived

- [ ] **Live menu measurement was not run.** The scalability gate is geometry
      proof. There is no in-game open/reopen timing, scrolling feel, memory
      behaviour, or screenshot at projected scale. If you want that before
      Sprint 3, it needs a development-only stress fixture and a guarded run.
- [ ] **The finished creature has had no human review.** Every acceptance
      claim about it is machine evidence or the implementation thread's own
      look at renders. Section 2 is the review; until it is done the creature
      is internally accepted only.
- [ ] **Two compatibility mechanical runs are still owed** (`gunslinger-only`
      and `gunslinger-high-risk-combined`); both timed out in save load inside
      the compatibility transaction and the same scenario passes outside it.

## 5. Two things worth knowing before Sprint 2 proceeds

- [ ] **The Pteranodon's donor is not a Roc.** It is
      `CR3_GiantEagleStandard`; "Roc" is only the view-policy label the roster
      prints. The charter and the roster both read as though it were a Roc.
- [ ] **That donor is shared by Eagle, Dire Bat, Pteranodon and Roc.** A
      careless mesh swap would change all four. Every Sprint 2 visual change
      must be instance-local, and those three creatures are the negative
      controls its acceptance scenarios will use.

## 6. Sign-off

- [ ] Sprint 0 accepted
- [ ] Sprint 1 accepted
- [ ] Sprint 2 audit accepted, asset work authorised to proceed
- [ ] Draft PR reviewed; **not merged**


---

## Acceptance state after the autonomous completion pass, 2026-09-23

Three things are tracked separately here and must not be conflated.

- **OwnerDelegationGranted** - the autonomous-completion order delegates
  intermediate technical, menu/usability and visual acceptance to internal
  review for Sprints 0-2.
- **InternalAcceptance** - recorded per row below.
- **HumanReview: NOT_PERFORMED_NONBLOCKING** - for every row. Nothing here is
  `OwnerAccepted` and nothing is `HumanVisualReviewPassed`.

| Item | Internal acceptance | Evidence |
|---|---|---|
| Structural inventory | ACCEPTED | 38/38 under all five compatibility profiles |
| Mechanical casting | ACCEPTED | 13/13 on the candidate outside a compatibility transaction |
| Player path | ACCEPTED | 10/10 |
| Persistence trio | ACCEPTED | 9/9 each |
| Visual contracts | ACCEPTED | 13/13 |
| Feature boundary | ACCEPTED | census PASS in both module directions |
| Compatibility coexistence | ACCEPTED | five profiles, mod sets confirmed loaded |
| Compatibility mechanical | NOT ACCEPTED | both runs timed out in save load inside the transaction |
| Rig contract | ACCEPTED | animation ActionSet, clips, events, anchors, 72-bone bind pose |
| Deformation proof | ACCEPTED | zero drift, control wing unmoved, BakeMesh agrees to zero |
| Loader and fallback | ACCEPTED | schema 2 loader publishes mesh + albedo or neither; withdrawn-visual and post-swap-fault paths exercised on live summons in the fault drill |
| Finished creature | ACCEPTED (internal) | tip-apex crest, atlas UVs, painted albedo; attached on every cast Pteranodon; HumanReview: NOT_PERFORMED_NONBLOCKING |
| Projected menu, live | NOT RUN | no anchorable action-bar slot in the disposable save |
| Live acceptance groups 1, 2, 4, 6, 7 | ACCEPTED (internal) | motion binding and presentation on the visual-contracts run; isolation, crowding, failure recovery and repeated lifecycle on the mechanical and fault-drill runs |

### What a human still needs to look at

The finished creature, in party-camera play - section 2 above. No image in
this mission has been reviewed by anyone but the implementation thread. The
renders it judged are the untextured clay and silhouette views of each crest
iteration and the textured, unlit and studio-lit views of the accepted one; the
game's own lighting has been seen by nobody.
