# Cleanup 2 targeted native tooltip observer

## Confirmed coverage gap

The original menu observer captures real icon grids, but
`NativeIconScreenEvidence.Capture` repeatedly invokes the native hover cooldown.
Installed `TooltipsController.SetTemporaryCooldown()` sets a 0.3-second counter,
calls `TooltipConstructor.Show(null)` and clears `m_IsShowed`. Such images cannot
qualify actual hover titles, tooltip art or family-first hover presentation.
Row TextMeshPro children can be counts rather than creature labels.

## Additive implementation

New entry:

```text
ReviewSummonCleanup2Tooltips(ExpandedSummoningCorrectionFixture fixture, Spellbook book)
```

It requires the exact automatic-exit Cleanup 2 scenario and the sole parameter
`reviewPhase=tooltips`. The root integrator must add the new source Compile
entry, parser/phase admission, Oleg fixture branch and top dispatch. The existing
narrative-overlay guard, native popup reveal/viewport checks and fixture spell
preparation methods are reused. No existing nine-tier menu traversal changes
are needed.

The allowlist contains five actual child rows across four canonical popups:

| Parent | Stable creature key | Required rendered title |
| --- | --- | --- |
| SM II | `horse` | `Horse` |
| SNA II | `horse` | `Horse` |
| SNA II | `small-fire-elemental` | `Elemental, Fire, Small` |
| SNA IV | `air-mephit` | `Mephit, Air` |
| SNA IX | `storm-giant` | `Giant, Storm` |

Children resolve from exact generated variant specs, stable identity symbols,
and the request fixture's live blueprint references. Each must occur once in
its real canonical parent's variant array, retain the fixture spellbook and
ConvertedFrom parent, and carry a nonnull live icon. Display text is not used
to find a row. Parent access is deliberately fixture granted/prepared in its
disposable native Wizard spellbook; this is not natural class-learning proof.

For each target, the real native spell group and level tab supply the actual
canonical parent row and popup. Existing native scroll APIs reveal the exact
row without reparenting or painting it. A normal grid image retains that menu
context. The existing row trigger then opens its native hover, a second image
retains that source-owned card, and the observer closes only its owned hover
and restores the original scroll state.

## Verified installed engine API

The inspected Kingmaker 2.1.7b Assembly-CSharp module is
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`.

* Public `TooltipTrigger.ShowTooltipManual(bool)` is an eight-byte wrapper over
  the same protected ShowTooltip method called by native pointer enter/exit.
  No SetObject, fabricated TooltipData, label write or icon assignment is used.
* Native tooltip collection searches the existing ActionBarSlot and its
  MechanicSlot. For converted spell rows it returns that slot's stored Spell
  AbilityData. The observer requires exact reference identity to this data.
* Native `DescriptionTemplatesAbility.AbilityDataHeader` reads the child's
  Blueprint.Name and Blueprint.Icon. This is the rendered identity under test.
* Public TooltipsController fields `ContextTooltipData`, `LastTooltipData` and
  `TooltipConstructor`, public trigger Data and static TooltipBase.Owner expose
  native source identity. Exact read-only reflection of the installed fields
  `TooltipsController.m_IsShowed`, `TooltipBase.m_NeedTooltip`, and protected
  `TooltipBase.Data` confirms the constructor is displaying that same data.
  Missing fields fail closed.
* ActionBarSlot.Hover is a separate mechanic/prediction hover path.
  ActionBarSlot.OnRightClick toggles auto-use. Neither is called here.

## Capture and restoration contract

`NativeIconScreenEvidence.CaptureOwnedHover` is restricted to this exact phase.
It uses the existing native framebuffer writer and UMM ownership handling, but
preserves the hover only while its native ownership predicate remains true
through settlement and PNG completion. Default Capture still suppresses
incidental hover. Every image records its actual hover policy.

The observer waits boundedly for native tooltip delay, layout and fade. It
reads active native HeaderBox TextMeshPro titles and Images, requires the exact
expected parsed title without truncation/overflow and an exact child-icon
sprite, and checks native alpha/culling, clip rectangles, positive size and
screen bounds. The framebuffer still requires visual inspection and owner art
approval; these structured checks do not grant either.

Source row/AbilityData references, ConvertedFrom, spellbook and fully visible
scroll geometry remain guarded. Caster and original party memorized slot
references, spell references, availability, resource identities/amounts,
empty commands and hotbar snapshots are compared without issuing a click,
cast, pin or auto-use toggle. Original party/selection/group states are restored
on the owned normal path. An empty original party/slot set is not old-save
compatibility proof.

Native trigger hiding and OnDisable clear the shared tooltip context. A foreign
replacement therefore causes explicit failure: the observer does not call
trigger hide, popup hide or selection/group/party UI restoration over that
replacement. It records incomplete restoration and leaves mandatory exact-owned
fixture teardown to the enclosing guarded save-free scenario. No foreign card
is adopted as the requested target, and no failed capture is labelled PASS.

## Validation status

This lane inspected actual installed API/IL and implemented the bounded
observer and hover-preserving overload. No build, game launch, Git mutation,
project edit or capture was performed. Root integration, production compilation
and the affected native phase remain pending. It is not current native Horse
or LABEL-01 acceptance evidence.
