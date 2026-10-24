# Cleanup iteration 2: SNA publication and selection labels

Source base: `a35a3277a9c5e2c111475523b868c2cf08422e05` (accepted 0.0.163).
Lane: MENU-01 and LABEL-01. MENU-02 is integrated separately.

| ID | Implementation / technical verification | Native evidence | Owner acceptance |
| --- | --- | --- | --- |
| MENU-01 | Complete coverage guard, repeat-publication reference handling, and transactional rollback ownership implemented. Focused behavior tests authored; integrated execution pending. | Not yet observed on this candidate. Existing source already removes every catalogued generic SNA leaf with the complete current roster. The current installation was reported by the integrator as 0.0.140, so it cannot establish a defect in 0.0.163. | Pending |
| LABEL-01 | Explicit presentation map and metadata implemented; generated and retained-native builders share it. Domain/production compilation and focused tests pending integration. | Menus, quantity branches, conversion, adapters, clipping and old-save casts remain unverified. | Pending |

## Findings and changes

Canonical publication already reconciled all 24 SNA native selectable sources.
Tier I's preserved native clone resolves to Mite; it is not an unexplained
additional animal. Higher tiers resolve to the creatures listed below. No
source-only claim is made that the owner's observed generic paw option has been
reproduced or eliminated in native menus. Native UI inventory must distinguish
an obsolete child, the normal parent, an alternate caller and stale installed
bytes.

The prior publisher rejected repeated publication because additions also existed
among current choices, and Tier I's direct-parent guard rejected the previously
published variants component. The new policy recognizes exact owned references,
preserves foreign reference order, rejects same-GUID foreign objects and returns
without changing components when the resulting sequence is unchanged. It does
not select entries by name, icon or native origin.

Removal now requires every creature behavior represented by an exact legacy
source. An additional Frost Giant carved from the Mastodon action does not
replace Mastodon. The native SM Redcap umbrellas also require the generated
Bralani alignment branch; the SM IX single umbrella requires Ghaele alongside
Thanadaemon. Missing coverage retains the source; ambiguous coverage fails
before publication. These guards fix concrete unsafe conditions when a required
replacement is absent; they do not change today's complete publication roster.

Rollback now validates all owned parent component arrays and child arrays before
restoring any parent. A foreign replacement or in-place child mutation causes a
refusal rather than discarding another mod's choices or partly restoring the
transaction. Legacy blueprints remain registered and resolvable.

## Exact SNA source coverage

`one`, `1d3`, `1d4+1` retain the existing quantity bands and placement order.
Generated creature names below are the stable creature keys. Capitalized native
names refer to their unchanged `KMG.Summoning.NativeOption.SNA` symbols.

| Tier | Quantity | Obsolete selectable source GUID | Required replacement | Additional choice derived from source |
| --- | --- | --- | --- | --- |
| I | one | `b5b7cf07ffeb4533a1320fbd06072cc5` | Mite wrapper | — |
| II | one | `848bd9df8b2643143a7020be7cde8800` | `giant-frog` | — |
| II | 1d3 | `b8ac9c653789b2a46ad85a075734c0e2` | Mite wrapper | — |
| III | one | `6db23a29c0c55c546a0feaef0c8d33d6` | `leopard` | — |
| III | 1d3 | `06d11dfa15e63bd41b01e09d5464ee8f` | `giant-frog` | — |
| III | 1d4+1 | `bb1bac85be6b1e44eafdc54a3b757c3e` | Mite wrapper | — |
| IV | one | `71dfb899a04db614e9db458ed4e43f56` | `dire-boar` | — |
| IV | 1d3 | `eb259941d7c2c5144844a31e72810642` | `leopard` | — |
| IV | 1d4+1 | `3050599c1ca9a9b40940a9426d4f861b` | `giant-frog` | — |
| V | one | `28ea1b2e0c4a9094da208b4c186f5e4f` | Manticore wrapper | — |
| V | 1d3 | `03e8e9605925b7140bdd331232b78d25` | `dire-boar` | — |
| V | 1d4+1 | `87c64591b0e6f7542807429d14bb1723` | `leopard` | — |
| VI | one | `060afb9e13d8a3547ad0dd20c407c0a5` | `dire-tiger` (Smilodon) | — |
| VI | 1d3 | `2aab2a0c280ed3e408a09967ec6bb281` | Manticore wrapper | — |
| VI | 1d4+1 | `7aefdbd7e0933b744b9c85566d16e504` | `dire-boar` | — |
| VII | one | `6d8d59aa38713be4fa3be76c19107cc0` | `mastodon` | FrostGiant wrapper |
| VII | 1d3 | `533f8cee65aa2fc448d6d2a7e5d28bb6` | `dire-tiger` (Smilodon) | — |
| VII | 1d4+1 | `b81bb947975c4e34395ab4e09a036a16` | Manticore wrapper | — |
| VIII | one | `8d3d5b62878d5b24391c1d7834d0d706` | Nereid wrapper | — |
| VIII | 1d3 | `256739c1e61e3f64eaf71734d271f4be` | `mastodon` | FrostGiant wrapper |
| VIII | 1d4+1 | `86f4287572bef49449b9d06c66adf456` | `dire-tiger` (Smilodon) | — |
| IX | one | `f6751c3b22dbd884093e350a37420368` | Hamadryad wrapper | — |
| IX | 1d3 | `780cbc629e74c1049b041b2a2f979863` | Nereid wrapper | — |
| IX | 1d4+1 | `9bd8cb6180842f44e9302c58e47b91f0` | `mastodon` | FrostGiant wrapper |

Unknown third-party GUIDs are preserved. The normal SNA I–IX parent abilities,
legacy definitions, spell lists, conversion sources and saved references are
not removed. Alternate adapters already copy canonical display names and natural
descriptions through `AlternateSummoningAdapterBuilder.Finish`; no adapter file
was changed by this lane.

## Selection-label contract

The explicit mapping is in
`src/KingmakerGunslinger/Summoning/SummonChoicePresentationPolicy.cs`. It covers
all 117 creatures currently registered in the accepted catalog and the 11
additional retained-native species. Coverage is checked against catalogs rather
than enforced by a frozen aggregate count. No new species is published.

Family/subtype/elemental size/template/quantity remain separate metadata.
Elemental size ranks are Small, Medium, Large, Huge, Greater, Elder. Rendering
uses `Elemental, Fire, Small`, `Mephit, Air`, `Beetle, Stag, Giant`, `Giant,
Storm`, `Wasp, Giant`, `Archon, Lantern`, and analogous explicit entries.
Individual dinosaurs retain their own species names. Smilodon retains its
released display identity despite the historical `dire-tiger` key. Meaningful
compound species such as Hell Hound, Invisible Stalker, Shambling Mound and Soul
Eater intentionally retain their complete names. Ant caste distinctions remain
visible, for example `Ant, Giant (Worker)`.

Template presentation follows the family, for example `Rat, Dire (Celestial)
(1d3)`; quantity suffix spelling remains unchanged. Full sorting is deferred:
`SummonDisplayOrderPolicy` is unchanged. Builders write the new rendered value
under the existing localization key. Natural descriptions still use
`SummonCreatureSpec.DisplayName` or `SummonNativeExpansionSpec.DisplayName`.
Stable keys, symbols, GUID allocation, identity-ledger inputs, asset paths and
creature identity strings are unchanged by these edits.

## Checks and remaining evidence

Focused test file: `Cleanup2SummonPresentationTests.cs`, with four registered
methods for map coverage/natural names, label composition/metadata, complete
legacy-source replacement coverage (including every missing-required-choice
negative case), and repeated publication/foreign identity conflicts. Integration
owns project registration, compilation and supervised execution. These domain
checks do not run the native publisher or prove native menu behavior.

Read-only consistency check (2026-10-21, approximately 1.5 s): parsed the accepted
catalog keys and explicit map, reporting 117 catalog keys, 128 mappings, zero
missing keys and zero duplicate mappings; `git diff --check` passed. This is a
source consistency check, not runtime acceptance.

Attempted read-only pilot diff inspection failed: the remote-only pilot ref
requires promisor object `6747cae90e6dee1d58a42afb1a990f6611d428ae`; restricted
network prevented lazy fetch. No unfinished pilot work was copied. The
integrator must check overlapping files against the current pilot before push.

Still required: actual all-nine-tier spellbook/action-bar scrolling and quantity
menus, spontaneous conversion and affected alternate callers, current/old-save
casts, module-off cold start, and native repeated-publication/rollback checks.
Actual observed GUIDs/build identity are needed before attributing any surviving
generic leaf. LABEL-01 also requires native icon/tooltip inspection for clipping.
No native or owner visual PASS is asserted here.

## Build prerequisite correction: installed push-wrapper audit

At the integrator's request, updated `scripts/Test-PushPolicyWrapper.ps1` to run
behavioral fixtures for the actual current external push wrapper. The prior
test assumed a historical `$AllowedBranches` variable and refused before any
fixture ran. The current wrapper instead checks exact worktree root, review
branch namespace, Git operation state, clean status, exact origin, tracked
content and remote ancestry. Contract detection selects fixtures; it does not
skip their execution or supply a PASS. Historical allowlist fixtures remain.

Command: `powershell.exe -NoProfile -ExecutionPolicy Bypass -File
scripts/Test-PushPolicyWrapper.ps1`. Result: exit 0, 37/37 isolated cases PASS,
including exact push argv, missing/failed guard reads, refusal before mutation,
and the deliberately incorrect push-before-refusal negative control. Only fake
Git ran; no network or real push occurred. Tested external wrapper SHA-256:
`e40cdfc06986a46814007d92db1328c39ef82bf39986d4515aaf7684eb9a7393`.
Exact elapsed wall time was not instrumented on this standalone run; the
integrator's supervised gate captures timing when reusing the prerequisite
correction on the clean baseline. An older installed allowlist wrapper was not
available and its retained fixture mode was not run here.
