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

## Review 6 correction: fitting popups have no scroll component

Retained failed native run:
`20261022T0133007801027Z-6f141a62f48f40cb818923dea1825000`
(`20261022T0133007631017Z-disposable-expanded-summoning-cleanup2-native-review`).
It passed the corrected no-narrative guard and all ten exact SNA I child
identities, then refused `popup-native-scroll-sna1` with `surfaces=[]` before
capturing that popup. This is not menu or tooltip visual acceptance.

Installed `ActionBarSpellsGroup.FillSlots` token `0600451D` obtains each native
`ActionBarSpontaneousConvertedSlot`, parents it directly to the group's own
transform, initializes it, sets its actual AbilityData and appends it to
`m_Slots`. It creates no ScrollRect. The inspected Fill token is `0600451C`;
the audit file's earlier query for nonexistent `ActionBarBaseGroup` failed and
does not establish a base-class contract.

`ExpandedSummoningVariantMenuRuntime.RecordSuccess` provides the live exact
parent/source identity and measured layout snapshot. Its fitting branch does
not install scrolling; overflow installs the existing one owned ScrollRect,
viewport marker, mask and content. A disabled owned scaffold can remain after
an earlier pooled opening. The observer does not create or enable one.

Both menu and targeted tooltip observers now use the same exact admission
helper. It requires the current snapshot object, parent GUID and variants,
exact source slot identity/path and its actual source AbilityData, that source's
exact SubGroup, the same native `m_Slots` collection and row references, all
published child identities once, stable converted-source/spellbook references
and sibling indices, and live root/source geometry matching the snapshot.

For overflow, only the snapshot-proven owned root ScrollRect and exact marked,
masked viewport/content hierarchy can supply scrolling. Active axes must match
the snapshot. A foreign scroll component or arbitrary action-bar ancestor is
not adopted. For fitting popups, an explicit stationary mode uses the exact
native popup RectTransform as both content and boundary. Every native row must
remain directly parented to that root and already fit both its actual bounds
and the measured safe rectangle. A retained scaffold must remain disabled and
inactive. No normalized coordinate or velocity exists in stationary evidence;
those fields are null, and accidental scroll API reads/writes throw.

Stationary reveal only waits for the actual native geometry to settle. It
never writes scroll position, velocity, content position, axes, layout or row
geometry. All existing applicable mask and physical-screen checks remain in
force. The menu observer must still cover every fully visible row in actual
framebuffer captures; the tooltip observer must still render the exact owned
native card. Stationary cleanup verifies unchanged native geometry without
repairing it. Overflow cleanup retains exact position/velocity restoration,
uses enabled axes only, and refuses writes after surface ownership changes.

Admission and later ownership refusals record bounded diagnostics in the
existing observations file: snapshot summary; actual source/popup/slot object
identities; root scroll/marker counts; up to 12 hierarchy levels and 64 row
rectangles; applicable masks with their rectangles; screen corners; and live
scroll content/viewport/axis metadata. Tooltip visibility refusal includes its
exact target even when it falls beyond the initial 64-row diagnostic bound.

Only source inspection and read-only evidence review were performed for this
correction. No build, native run, Git action, frozen worktree change, fixture
change or active-state update was performed. Root's batched compilation and
affected new menu/tooltip native observations remain required.
