# Kingmaker Gunslinger 0.0.122

Informational version: **0.0.122-teleportation-completion**.
The owner reviewed the change on PR #12, accepted it, merged it, and
explicitly authorized this public release.

This release completes and stabilizes the World-Map Teleportation Spells
feature. The owner says teleportation is otherwise working well; the working
mechanics are preserved throughout.

## Post-teleport movement

Relocation now issues the game's own pawn-notification pair, so the direction
arrows around the party token are rebuilt at the actual arrival point —
including off-target arrivals and fresh save reloads. The first click on a
legal arrow moves the party immediately; no extra travel is needed to
"repair" movement.

## Conjuration specialists

Teleport and Greater Teleport are published into the native Conjuration
school list, so a Conjuration specialist can prepare them in the
fifth/seventh-level favorite slot through the normal spellbook UI. Existing
specialists work without a respec. Universalists and other schools are
unchanged.

## Compact destination menu

Spell actions are compact two-line controls ("Cast Teleport" over
"caster · 2 prepared · CL 9") that fit the parchment on narrow layouts.
When the native settlement teleport is also available it is labeled
**Settlement Teleport** to distinguish it from the spells; the label restores
itself when the spell rows close. Long source lists scroll inside the panel.

## Genuine scrolls

Real Teleport (5th, 1,125 gp), Greater Teleport (7th, 2,275 gp) and Word of
Recall (6th, 1,650 gp) scrolls are sold in finite batches — Zarcie stocks
Teleport and Greater Teleport; Arsinoe and the Jhod priests stock Word of
Recall. Wizards can copy them into a book, any character with trained Use
Magic Device can read them from the world-map menu, and using one from
inventory tells the player to select a destination on the world map.

Activation safety: a **request-bound activation gate** authorizes exactly one
reader/item pair for exactly the confirmed contextual request. Ordinary
inventory, equipment, or action-bar use of any supported scroll — including
compatible crafted variants discovered through the canonical ability
association — is refused before any activation roll or consumption. A failed
Use Magic Device check refuses the activation with nothing spent; a failed
cast keeps the native consumption and never teleports. Exactly one component
owns consumption: the native activation path itself. Every activation is
observed and attributed to the exact reader and item; an unrelated rulebook
event is never accepted as this request's result.

Material variant identity is preserved: equivalent copies aggregate, while
variants differing in caster level, item spell level, or charge model stay
distinct rows that display their caster level. Selecting a variant spends
only that variant; a scroll whose teaching target differs from its activated
spell is never offered.

Vendor stock integrity: publication is atomic (a failure on any supplier
restores every changed table), the arcane supplier resolves Zarcie's tier
with the approved Hassuf fallback when her content is genuinely absent, and
the priest supplier resolves independently. The finite rows are published as
save-compatible definitions whenever the mod loads, so module OFF/ON
preserves already-generated stock and the native purchase bookkeeping —
re-enabling restores the shelf to the batch minus remembered purchases with
**no refill**, bought-out shelves stay bought out, and owned items, learned
spells, and spent preparations persist untouched.

## Qualification status

- Repository source validation (0.0.122 chain): PASS.
- Domain test suite: **1,561 tests, 0 failures** (Release, clean).
- Two deterministic qualified Release builds and strict standalone UMM package
  validation (performed by the release script).
- Guarded in-game scenarios on the corrected candidate, all through Steam App
  640820 with disposable fixtures and the authorized working save only:
  arrows **8/8**, specialist **9/9**, casting **44/44**, interaction
  **29/29**, travelers **15/15**, gamepad **39/39**, coexistence **26/26**,
  observer **19/19**, and scrolls **36/36** twice.
- Fresh-process acquisition persistence: phases **15/12/8/4 PASS**
  (transaction `20260910T2328349730573Z_e6f6f244594e420aa9febe777507d087`,
  four distinct processes): real gold purchase → native copy → Conjuration
  favorite preparation → rest → one native activation → actual first-arrow
  movement; the exact lifecycle state verified after fresh ON reload, fresh
  OFF load/save, and fresh ON reload with the no-refill contract.

## Retained compatibility posture

Optional-mod compatibility is unchanged from the 0.0.121
`unified-firearm-maintenance.zip` baseline: Craft Magic Items coexistence
keeps the runtime-loadable profile boundary and this package still ships no
foreign assemblies — `CraftMagicItems.dll` remains an externally installed
dependency of that mod, never a bundled file.

## Disclosed limitations

- In-area genuine Zarcie absence and kingdom auto-management acquisition
  could not be established in the qualified environment; the fallback
  identity, shared supplier decision, and fallback grant behavior are
  verified, the in-area absence condition is not.
- Crafted-scroll variants are covered by a distinct fixture variant; the
  Craft Magic Items creation path itself was not exercised.
- Non-16:10 aspect ratios and optional-mod coverage beyond the qualified
  profiles remain unexercised.

The inherited 1,288-test overhaul and 1,325-test fatigue-authority checkpoints
remain historical evidence, as do the 1,554-test unified-repair records.

## Retained baselines

The installable archive is
`KingmakerGunslinger-0.0.122-teleportation-completion.zip`. The qualified
firearm SoundBank is retained unchanged: `KMG_Firearms.bnk` SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18` (see
`assets/soundbanks/firearm-soundbank-manifest.json`). Optional-mod
compatibility is unchanged from 0.0.121. The manifest contains 1,886
identities: 1,884 active and two reserved.
