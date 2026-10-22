# Cleanup 2 palette readback and restoration review

## Review scope and engine finding

The review inspected `RuntimeTestRunner.SummonCleanup2PaletteReview.cs`,
`GiantSummonPaletteRuntime.cs`, their scale-prototype caller and the installed
Kingmaker 2.1.7b material-controller IL. No build, game launch or Git operation
was performed by this lane. Only the palette runtime and these notes were
changed after the integrator ceded that correction.

The original release predicate reported `restored=True` from ownership before
the restore/reinitialization operation. It did not verify the resulting source
material reference or protected material values. The lease accepted that string
as restoration proof.

Native `StandardMaterialController.ReinitMaterials()` first calls five
`RevertToDefaults` controllers, then `Awake()`. The revert methods include actual
SetTexture, UV, color, shader-keyword and scalar writes across the controller's
previous materials. `Awake()` reads and writes `Renderer.materials` on every
active child renderer, rebuilds material settings, and updates blood state.
This operation is broader than replacing one body albedo slot.

The managed IL establishes the auto-instancing getter call; it does not expose
Unity's native already-instance optimization. It therefore cannot establish
that restoring a previously instanced material always clones it, or always
retains its exact reference. The post-operation source state must be observed.

## Focused correction

Before any palette allocation or renderer swap, the owner now captures every
source renderer's full material array and source mesh, plus unique original
materials' shader, sorted shader keywords, render queue, protected
`_MainTex`/`_BumpMap`/`_Parameters` texture references and UV scale/offset,
present `_TintColor`/`_Color`/`_BaseColor`/`_MainColor` values, and `_Dissolve`
where present. Protected materials with non-finite or unsettled dissolve are
refused before any source mutation.

After Apply, all recorded source arrays/material fields must remain exact,
with only the exact owned body slot and its private albedo allowed to differ.
Detached original materials remain protected: the native controller may still
visit them while rebuilding its cache.

Release checks protected current state before calling the global native
reinitialization, so detected foreign material changes do not become authority
to reset them. Only the exact still-owned body slot is restored. Reinit must
return `reinitialized`, and a second protected-surface check must observe the
original source slot, complete original material arrays and recorded properties
before `restored=True` is emitted. This code does not write original material
properties to force a comparison to pass.

Failed attach/restoration keeps its first diagnostic, current verification
reason and ownership record. A failed native rebuild can retain cached owned
materials after the renderer swap, so failed restoration conservatively retains
its owned resources until explicit release or owned-view teardown. No failure
is relabelled as a successful restore. The integrator separately owns the
palette lease's view-clearing correction.

The snapshot checks the named properties above; it is not a claim that every
arbitrary shader property or active visual-effect channel has been qualified.
Actual protected-surface and native appearance evidence remains required.

## GPU readback and scale integration limits

The Gamma-only readback saves/restores both `RenderTexture.active` and
`GL.sRGBWrite` in `finally`, destroys its temporary readable texture and
releases its temporary render target. The source texture is read only. Its
RGBA test is an actual graphics-context copy/channel/alpha/orientation check;
it does not by itself establish DXT1 content identity, native palette appearance
or owner visual approval. Native DXT1 inputs and resulting actor appearance
still require their own recorded observations. No new native PASS is claimed.

The current integration applies palettes before the scale helper snapshots its
material references, and disposes scale helpers before palette leases. This
ordering is compatible with the helper's exact reference guards. The protected
snapshot verifies the captured source renderers, rather than claiming the owned
proxy renderers as source renderers; inactive proxies can still await Unity's
deferred destruction during the release frame.

Compilation and the affected bounded native experiment are pending the root
integrator. An exact material-reference refusal remains a failure/unknown;
there is no alternate restoration or tint waiver.

## Menu fixture advice supplied separately

The first-level illustrated book uses native dialog/book/interchapter lifecycle
state. `StopDialog` runs the dialog's FinishActions before clearing its public
Dialog field; presentation hiding is not dialog completion. The recommendation
was to reuse the existing exact Oleg preset fixture and fail closed on active
dialog/book/interchapter state. The integrator owns those source changes.
