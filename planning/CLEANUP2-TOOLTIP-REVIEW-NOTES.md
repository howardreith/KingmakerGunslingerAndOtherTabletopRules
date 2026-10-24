# Cleanup 2 exact native tooltip correction

## Retained failure

Run `20261022T0302309996047Z-ba15b9c5ed5440f98d49dae83ff03fce`
in `20261022T0302309816023Z-disposable-expanded-summoning-cleanup2-native-review`
failed on SM II Horse. Structured observations prove the exact row AbilityData,
owned hover and visible, unclipped `Horse` title; the source row icon matched.
The only PNG is the initial popup (`native-ui-000.png`), which was inspected.
It contains no tooltip. This failed run does not qualify actual hover pixels.

## Installed template contract

Actual Kingmaker 2.1.7b Assembly-CSharp MVID:
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`.

`TooltipConstructor.SetContent` calls `DescriptionBuilder.Build(Data, Body,
true)`. `DescriptionTemplatesAbility.AbilityDataHeader` passes the actual
child Name/Icon to `DescriptionTemplatesBase.IconNameHeaderWithBorder`.
That method adds the exact `TooltipsController.Bricks.HeaderItem` template.
For `isTooltip=true` it **skips art SetIcon(index 0)** and clears border index
1. `DescriptionBrick.Init` deactivates its Images. An expanded description uses
the separate `isTooltip=false` path. Requiring a child art sprite among hover
HeaderBox descendants was an invalid observer assumption.

The correction reads the actual HeaderBox `m_ShowList`. Its required HeaderItem
must match the native prefab's type and nonempty BlockID, remain directly under
HeaderBox.Content, and belong to the exact CurrentTooltipData. This is the same
BlockID relationship used by native AddToShowList. Its actual TextFields[0]
must render the exact expected parsed title without clipping or overflow.
Its exact Images list must match the text-only template and remain inactive.
No unrelated descendant, decorative image or fabricated art earns a pass.

The actual child source-row icon must separately match and be fully visible
in the same full-frame native hover capture. Evidence explicitly records
`tooltipArtDisplayed=false`, `nativeTextOnlyHeaderContract=true`, the HeaderItem
identity and source-row sprite. This observes native hover titles and menu art;
it does not claim an art-bearing tooltip or expanded-description coverage.

## Installed close lifecycle and correction

`TooltipTrigger.ShowTooltipManual(false)` uses the same native ShowTooltip path
as pointer exit. StandartHide clears ContextTooltipData and disposes the trigger
data. It does not synchronously reset the controller's shown flag.

`TooltipsController.Update`, after native MoveDelay, calls SetupTooltip.
With Context=null, SetupTooltip invokes `TooltipBase.Show(null)` and clears
`m_IsShowed`. Show(null) clears `m_NeedTooltip` and calls OnHide, which starts
FadeAnimator.DisappearAnimation. Its native completion sets CanvasGroup alpha
to zero and either deactivates the object or retains the configured always-active
object. Cached constructor Data, LastTooltipData and TooltipBase.Owner can remain
after closure; the native hide path does not clear them.

The old observer's 30-frame wait followed a finally block and was skipped when
the header predicate threw. Its outer restoration therefore sampled immediately
after native hide and reported hoverClosed=false. A bounded wrapper now catches
the target iterator failure, permits native Update/fade settlement for at most
600 frames / eight wall seconds, and only then rethrows the retained original
failure. It requires exact controller/constructor identity, Context=null,
m_IsShowed=false, m_NeedTooltip=false, and inactive or exact-zero inherited
render alpha. Retained cached identities and actual renderer alpha are recorded.
No cached field, fade, controller or shared tooltip is manually reset. Foreign
context stops settlement and refuses writes. Failure is not converted to PASS.

## Scope and validation

Changed only `RuntimeTestRunner.SummonCleanup2TooltipReview.cs` and this note.
NativeIconScreenEvidence, product icons, labels, palette, source8/frozen bytes,
phase lists and dispatch were untouched. Exact installed public field contracts
and private lifecycle IL were read. No build, native execution, Git action,
artifact rewrite or broad campaign was performed. Root compilation and one
affected new tooltip observation remain required; all old failures are retained.
