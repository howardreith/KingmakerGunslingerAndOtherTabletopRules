# Expanded Summoning Phase 0 owner review checklist

Nothing below has been accepted. Technical PASS is not owner approval, and no
item here may be ticked by the implementation thread.

This checklist covers Sprints 0-2 only. **Sprint 3 is not started.**

## What to expect before you look

Phase 0 deliberately changes nothing a player can see. If you notice any
difference in the summon menus or in any creature's appearance, that is a
defect, not the feature.

- Visible summon choices are still **693** (371 Summon Monster, 322 Nature's
  Ally).
- No creature was added, removed, hidden, or unhidden.
- No GUID was allocated. No model, icon, or animation changed.
- Pteranodon still uses its accepted borrowed body.

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

## 2. Pteranodon - confirm it is untouched

- [ ] Cast Summon Monster IV and Summon Nature's Ally IV for a single
      Pteranodon. It looks exactly as it did before, with the same scale.
- [ ] Its bite still lands at its usual reach and timing.
- [ ] Higher-tier 1d3 and 1d4+1 Pteranodon casts behave as before.
- [ ] Celestial/fiendish templating still applies as before for your caster's
      alignment.
- [ ] Save, reload, dismiss, kill and let one expire - all as before.

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
- [ ] **Sprint 2 is an audit, not an implementation.** No pterosaur asset
      exists. The audit answers what the donor is and what must not be touched;
      the remaining work is live donor inspection, mesh and texture authoring,
      a Unity 2018.4.10f1 bundle, the runtime loader with fallback, and live
      acceptance.

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
