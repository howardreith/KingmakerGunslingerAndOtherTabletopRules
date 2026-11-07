# Summoned Creature Manual Playtest Repair Mission

## 1. Outcome

Deliver an enjoyable summon roster that preserves the extensive gameplay the owner has now approved, removes the capabilities the owner no longer wants, corrects visible interface problems, and repairs the few reported mechanical and presentation defects.

This mission has seven bounded sprints, R1 through R7. These labels are separate from the repository's historical creature sprints. Each sprint has a player-visible outcome, affected evidence, and a stopping point. Avoid another sequence dominated by repairing an increasingly broad test harness.

The current request is authoritative. Earlier automated judgments that a model was adequate do not veto the owner's new request to revise Nalfeshnee, Glabrezu, or Ice Devil.

### Baseline and environment

- Source inspected: master 7ed6e247471d346cfd4b190844e3891639e16f24. Fetch and inspect the actual current branch before starting; preserve concurrent work.
- Current master includes the v0.0.179 publication records. The user's tested/installed binary is not identified by these screenshots.
- Record the actual installed version and available DLL/package identity, mod profile, relevant settings, and screenshot provenance during intake. Never restore an old hard-coded version.
- Environment: DATA. No HOWARD-LAPTOP access is assumed.
- The owner supplied thirteen new screenshots, including Ice Devil. Use them as the visual baseline. Their existence does not authorize bypassing a host-restricted automation phase.
- This package authorizes implementation planning and supplies the R1 implementation prompt. It does not itself run game tests, merge code, install a build permanently, or publish a release.

## 2. Settled owner decisions

| Decision | Required result |
| --- | --- |
| Omit Stirge for now | Withdraw all KMG public summon choices for Stirge, including higher-tier and supported alternate routes. Retain its code, assets, stable identities, and legacy behavior needed for saves. Do not spend this release repairing its grapple animation. |
| Remove Wall of Ice | Remove the ability from Bone Devil and Ice Devil, including automatic use. Keep safe legacy state cleanup and stable identities. The earlier ice-texture and scream requests are superseded. |
| Remove Magma Form | Magma Mephit remains, with breath and Pyrotechnics. Its Magma Form command and automatic use are removed. |
| Remove Bardic Suggestion | Remove Lillend's separate performance ability and automatic route. Preserve its Suggestion spell and other approved abilities. |
| Remove Fire Giant Sunder | Remove the Fire Giant's ability to initiate Sunder, including feat-driven automatic swaps. Preserve shared implementation and other creatures, especially the approved Storm Giant. |
| Cloud Giant mist | Retire both existing custom Cloud Giant fog implementations. Grant one at-will SLA using the actual working Obscuring Mist spell, with ordinary target-point behavior. If its optional provider is unavailable, omit this ability. |
| Pixie SLAs | Add at-will Entangle, Confusion, Dispel Magic, and Shield. Use the real installed Irresistible Dance spell for Dance; omit Dance without its provider. The at-will instruction applies to the four named additions, not automatically to Dance. |
| Consistent summon sheen | Preferred implementation: remove the persistent generic summon sheen consistently from KMG summons. Preserve a brief arrival effect, intrinsic creature glow, invisibility, transparency, selection state, and actual spell/buff effects. If the observed sheen is a legitimate active buff, identify it before changing anything. |
| Focused model work | Refine Glabrezu, substantially improve Nalfeshnee's current presentation, and refine Ice Devil. Other models remain protected by the owner's acceptance unless a shared change introduces a concrete regression. |

Removing a player-facing feature is not permission to delete registered GUIDs or strand serialized objects. Conversely, retaining a definition for compatibility is not a reason to leave its public grant or AI action enabled.

## 3. Sprint sequence

| Sprint | Deliverable | Issue IDs |
| --- | --- | --- |
| R1 — Release cuts | Five explicit withdrawals, with public/automatic routes removed and compatibility retained | CUT-01 through CUT-05 |
| R2 — Combat entry and fear | Consistent first eligible action; investigate surprise rounds separately; fix the source of repeated fear evaluations | LIFE-01, LIFE-02 |
| R3 — Portraits and interface | Correct initiative portraits across the published roster; named, visible icons and clean tooltips | UI-01 through UI-06 |
| R4 — Mephits | Correct breath damage profiles, matching breath icons, Water acid presentation, Pyrotechnics flash | MEPH-01 through MEPH-04 |
| R5 — Abilities and spell reuse | Cloud/Pixie donor integration, four Pixie SLAs, Mastodon Trample, Griffon Charge disposition | SPELL-01 through SPELL-03; MECH-01, MECH-02 |
| R6 — Motion, effects, and audio | Hound/Redcap presentation, clean Lantern orb, Vrock/Trumpet sounds, consistent summon sheen | PRES-01 through PRES-08 |
| R7 — Three models and final review | Glabrezu/Nalfeshnee/Ice Devil revisions, focused final visual and integration review | ART-01 through ART-03 |

R3 may be implemented independently after R1 if branches remain isolated. Merge ordering must preserve coherent tested candidates. Do not turn parallel work into several simultaneous live game transactions.

## 4. R1 — Remove the five unwanted features

### Work

1. Use the existing SummonVisibilityCatalog withholding mechanism for Stirge. Keep identity registration and assets intact. Inspect canonical and supported alternate publication paths.
2. Detach Wall of Ice grants, parameters/resources that are no longer needed by fresh grants, and AI use from Bone Devil and Ice Devil. Retain required legacy configuration and cleanup.
3. Remove Magma Form's grant, dedicated public resource exposure, and AI use from Magma Mephit. Keep the registered ability/state configured where serialization requires it.
4. Remove Lillend Bardic Suggestion's grant and noncombat automatic route. Preserve the separate Suggestion spell and shared provider configuration.
5. Remove Fire Giant's Sunder source. Current source grants NativeImprovedSunder; ProperFlanking2 can derive an automatic attack replacement from that feat. Hiding an icon is insufficient. Leave Storm Giant's separate grant unchanged.

### Compatibility contract

Stable GUIDs and existing saved references must still resolve. For a summon loaded from an older save, inspect whether persisted facts preserve a withdrawn action; if so, reconcile only the exact KMG-owned grant. Avoid global removals of native feats, spells, donors, or other mods' blueprints.

A pre-existing Wall must still retire cleanly through the retained lifetime/navigation machinery. Retain the wall prototype, section/area identities, and InfernalWallSourceLifetime35 where needed. This is a removal and compatibility task; no new Wall material, sound, breach, steam, or broad lifecycle campaign is in scope.

### Acceptance

- Stirge is absent from every affected public route; unrelated summon choices remain available.
- Fresh affected summons have no withdrawn actions or automatic routes.
- Lillend Suggestion, Magma breath/Pyrotechnics, ordinary Fire Giant attacks, and other approved summon capabilities remain.
- The active mod stack's Fire Giant does not substitute Sunder for ordinary attacks.
- Saved identities resolve and one targeted legacy-wall cleanup case covers any changed cleanup dependency.
- Focused tests and one coherent build support the changes. Use minimal permitted DATA observation where necessary; accurately record any native gap.
- Push one reviewable PR and stop. Do not begin timing, portraits, or model work in R1.

## 5. R2 — First-turn activity and fear spam

### LIFE-01: Acadamae Graduate and summon readiness

The report contains two observations: apparent surprise-round delay and inconsistent readiness across creature types. Treat them separately.

Compare Pteranodon and a representative mephit with Lion and Hell Hound under matched conditions. Begin with an ordinary turn and one summon per cast. Add the surprise-round case separately. Cover multiple-summon enrollment only if the shared path changes, and retain an ordinary non-accelerated cast as a control.

Record the cast's actual action type, caster/command ID, combat controller, round and surprise state, spawn/registration time, appearance lock, lifecycle grace, native initiative enrollment, AI readiness, and first legal action. A visible delay, initiative delay, and actual action prohibition are different observations.

Use the existing SummonSameTurnActivationPolicy/Runtime and Acadamae invocation tracking. Do not install a second global turn scheduler. Do not reset initiative, manufacture an extra full turn, erase legitimate action costs, or mask the defect by forcing a target into range.

Rules baseline: Summon Monster says a creature acts immediately when it appears on the caster's turn. Acadamae Graduate changes casting time and adds the caster's fatigue save. Surprise rounds have their own participation/action limits; the general text alone does not settle every engine-specific arrival case. Preserve the established adaptation unless a concrete mismatch is demonstrated.

Acceptance: reported slow cases and fast controls have the same correct eligibility under equivalent ordinary-round conditions; the surprise-round result is explained and, if defective, repaired without extra actions or stalled turns. Record a native action, not merely a populated initiative list.

### LIFE-02: repeated Frightened saves

The screenshot names Nalfeshnee as the saving target, not necessarily the source. DC 22 is consistent with the nearby Ice Devil's printed fear aura, but that is an investigation lead only.

Observe one Ice Devil, then the reported Ice Devil/Nalfeshnee pair. Record exact source/target identities, aura and effect GUIDs, faction/relationship filtering, entry/exit, save evaluations, buff refreshes, and any existing immunity policy.

Repair the incorrect evaluation or targeting path. Throttling log lines alone does not establish a mechanics fix. Do not add a blanket 24-hour successful-save immunity by importing a different edition's Ice Devil rule or conflating fear aura with frightful presence.

Acceptance: a stationary pair does not produce an unintended rapid stream of repeated evaluations; valid fear still works on eligible targets under the documented contract; leaving/re-entering, distinct source ownership, and cleanup behave correctly where changed. Use focused scenarios rather than the removed Wall campaign.

## 6. R3 — Portraits, icons, and text

### Initiative portraits across the published roster

Audit the actual portrait consumer used by turn-based initiative. The selection icon being correct does not prove the unit portrait is correct.

Map each published summon key to its existing approved creature artwork, unit portrait/small-portrait fields, and actual initiative presentation. Start with Dire Rat's wolf image, Stirge's bird as historical evidence, Lantern's helmet, and donor-looking portraits for high-tier outsiders.

Reuse the approved creature-icon family where suitable. Adapt it to the native portrait surface using existing APIs/caches; avoid repainting every creature, mutating shared donor portraits, or installing a global icon getter. Preserve overlays for initiative state, health, selection, delay, and conditions.

A cheap catalog-level assignment audit can cover the roster. Native screenshots should sample distinct presentation paths; spawning every creature again is unnecessary unless a specific shared problem requires it. Stirge's retained art need not block release once it is withheld.

### Individual interface repairs

- Give Lantern Archon's Light Ray and Twin Light Ray appropriate icons.
- Identify the actual owner and GUID of the unnamed blast reported on Ghaele, then give it an accurate name, description, and icon. The screenshot's hover and combat-log context differ, so do not patch the wrong creature.
- Give Unholy Nimbus an appropriate icon.
- Give Brachiosaurus Overrun an appropriate icon.
- Remove literal null fields from Cheetah Sprint and Unholy Nimbus. Supply correct metadata or omit inapplicable rows through supported scoped behavior.
- Inspect the same fields for affected sibling abilities while already in the relevant code. This is not a new whole-game localization audit.
- Assign icons to new/replaced abilities in later sprints as part of those changes.

Read the repository icon guide, reference index, and catalog. The owner permits semantically appropriate native/class/dragon art reuse; there is no need to create a new painted family for these repairs. Preserve the parent summon spell identity.

Acceptance: correct species in initiative; no blank white or nameless affected actions; no literal null tooltip fields; actual screens at normal UI scale. Treat player-visible action/AI-cast icons as real surfaces even when they are not usually manually controlled.

## 7. R4 — Mephit breath audit and presentation

Audit all ten profiles, including the four native-donor types as well as the six project variants. Inspect the effective damage packet separately from projectile/cone effects and icons.

### PF1 breath baseline

| Type | Base damage | Additional effect on failed save |
| --- | --- | --- |
| Air | 1d8 slashing | None |
| Dust | 1d4 slashing | Living targets sickened 3 rounds |
| Earth | 1d8 bludgeoning | None |
| Fire | 1d8 fire | None |
| Ice | 1d4 cold | Living targets sickened 3 rounds |
| Magma | 1d8 fire | None |
| Ooze | 1d4 acid | Living targets sickened 3 rounds |
| Salt | 1d4 slashing | Living targets sickened 3 rounds |
| Steam | 1d4 fire | Living targets sickened 3 rounds |
| Water | 1d8 acid | None |

Shared printed baseline: 15-foot cone; standard action; every four rounds; Reflex halves damage and, where applicable, avoids sickness. If a required correction touches save scaling, verify it against the creature's live Constitution and established project adaptation, not a permanently hard-coded displayed DC.

The required mechanical audit concerns damage types. Dice, save, recharge, and sickness values above are reference context, not authority for a new full rules implementation. Preserve accepted behavior unless a concrete related defect is demonstrated or the required correction necessarily touches it. Record discrepancies and make only justified local repairs.

Water must deal acid if its current packet is wrong and should use a recognizably acid visual. A lightning-looking effect alone does not establish the current damage type. Air should not automatically become electricity simply because it is elemental air.

Choose coherent icons for every breath using existing art. Physical grit/stone/salt, acid, ice, fire, and steam need appropriate meanings. A generic Burning Hands icon is not an adequate identity for every type.

Add one brief, appropriate visible flash to successful Pyrotechnics use. Reuse a suitable native effect with correct ownership and cleanup. Preserve its action/resource/effect behavior. Magma Form remains removed after R1.

Dust's resemblance to the ice cone is explicitly tolerated for this release; improve it only if an easy existing donor fits without expanding the sprint.

Acceptance: a ten-row actual-vs-required matrix; focused behavior tests for the corrected profiles; native Water acid damage and visual proof; representative physical, energy, and sickening breath coverage for any changed shared path; visible Pyrotechnics flash and coherent icons. Do not requalify all unrelated mephit mechanics.

## 8. R5 — Reuse actual spells and complete missing abilities

### Cloud Giant: one at-will Obscuring Mist SLA

Resolve the exact spell that the owner's character can cast, including provider, GUID, type, initialization order, targeting, effects, duration, icon, and description. The supplied screenshot shows a target-point spell; it does not identify the provider.

Replace the two custom Cloud Giant fog grants and AI routes with one owned SLA wrapper around that real spell implementation. Preserve its ordinary target-point behavior and rules while using the giant's appropriate caster context and an at-will grant. At will does not mean free actions or continuous AI recasting.

If the spell belongs to an optional mod, grant the SLA only when that provider and the exact usable blueprint are present. Without it, the giant remains usable and receives no substitute fog ability. Do not mutate the provider's shared spell. Update saved owned grants narrowly as necessary.

### Pixie: four at-will SLAs

Grant Entangle, Confusion, Dispel Magic, and Shield through actual spell implementations with appropriate spell-like wrappers. This is an explicit owner adaptation: the printed Pixie lists daily uses and Lesser Confusion. Implement full Confusion and at-will use as requested and document the override.

Use the real installed Irresistible Dance spell for the existing Dance capability. Resolve its provider, preserve the intended existing Dance frequency, and omit it if its provider is absent. Retain legacy identities as needed; the custom substitute must no longer be the active public route.

### Mastodon and Griffon

Add/repair Mastodon Trample using the proven existing trample mechanism. Preserve Mastodon-specific damage, save scaling, movement, targeting, and once-per-target-per-round behavior; do not copy Elephant's numbers blindly.

Identify what the owner's Griffon “two charges” means: duplicated action entries, two different actions, or resource-use counts. Remove a duplicate if confirmed. If two actions are intentional and mechanically different, explain and label them clearly. Do not change pounce, attacks, reach, or daily resources based on ambiguous wording.

### Acceptance

- Cloud Giant has one working targetable at-will Mist SLA when its provider is present, and neither old custom fog action.
- Missing/disabled optional providers cause clean omission, without missing-reference faults or fallback homebrew.
- Pixie can use each of the four named SLAs repeatedly with native action costs; Dance uses the resolved donor and correct frequency.
- Mastodon performs a real trample with its own values; Elephant/Aurochs remain unaffected.
- Griffon's actual ambiguity has a documented, player-visible resolution.
- Verify only provider-present/absent behavior relevant to these links, not every historical compatibility profile.

## 9. R6 — Animation, effects, and sound

### Motion and equipped weapons

- Hound Archon: show a real sword attack animation tied to the actual native attack/contact, and an appropriate casting gesture/effect when Aid is cast. Keep hit count, action economy, damage, and Aid mechanics unchanged.
- Redcap: inspect Kingmaker's actual Redcap prefab, weapon setup, and animations. Prefer its native scythe presentation if compatible. Reuse the presentation without importing unrelated campaign AI, statistics, or special encounter facts. Verify the weapon it visibly swings matches its actual attack.

### Effects

- Lantern Archon: identify the blue/teal protrusion on the golden orb. Inspect the donor/render hierarchy or a short sequence, then remove or integrate the offending owned visual. The current code retains the native donor renderers, making that one plausible cause, not a proven diagnosis. Keep intrinsic light and working ray attacks.
- Summon sheen: identify the generic persistent overlay and remove it consistently from KMG summons as the chosen style. Preserve brief appearance cues, inherent creature effects, invisibility, Shadow Demon transparency, and actual active buff effects. Do not remove all particles/renderers indiscriminately.

### Audio

- Vrock Stunning Screech: add a sharp, appropriate screech cue at ability execution.
- Trumpet Archon blast: add a trumpet/horn cue, preferably a suitable native sound. Reuse bardic performance only if it fits.
- Ankylosaurus impact audio: optional low-cost polish using an existing cue. It must not delay required fixes.

Prefer existing assets and event paths. If new audio is necessary, use an appropriately licensed/public-domain source and record provenance. Do not buy assets, add libraries, or extract WotR assets. Avoid duplicate/looping playback, excessive level, or sound continuing after interruption/despawn.

Acceptance: short actual motion clips for sword, casting, and scythe; a clean Lantern idle/move sequence; audible Vrock/Trumpet clips at normal game volume; consistent generic sheen treatment in a small mixed group. Screenshots cannot prove audible sound or animation timing.

## 10. R7 — Refine three models and close the repair package

### Glabrezu

Fix the smaller middle arms' attachment first. They must join credible chest/shoulder roots without visible gaps in idle, movement, and attack. Improve the torso/face/material transitions enough to make the anatomy coherent. Preserve four-arm identity, native attack behavior, size category, reach, and collision.

### Nalfeshnee

The new owner review explicitly rejects the current model. Improve facial and boar/ape-like anatomical definition, torso/limb transitions, tusks, wing roots, and material variation. The current rounded orange figure and slab-like wings are the starting defect, not an accepted finish.

Retain imposing presence without relying on another scale increase or brightness change alone. Match the established game art and use the original WotR inspiration as reference, with original/reusable authorized assets.

### Ice Devil

The owner calls the current model acceptable but requests refinement. Improve the integration of pale carapace/joints with the dark torso, head/mandible definition, and shoulder/limb/tail transitions. Preserve its cold identity, spear attachment, and working mechanics. The new DATA screenshots are the baseline; do not resume the obsolete laptop-image retrieval task.

### Visual acceptance and finish

Use the same gameplay camera, ordinary zoom, and comparable lighting for before/after views. Include a clear view of the specific corrected geometry and short relevant motion. Neutral/bright and dim scenes should both remain readable, but avoid building an elaborate new visual harness.

Technical checks cannot approve aesthetics. Supply a compact review the owner can inspect. Do not rewrite already accepted creature models or reopen dinosaur styling.

At the end, reconcile the issue register against all merged changes. Perform a focused integration check on the final candidate covering shared paths actually changed: publication, turn enrollment, affected optional-provider links, and selected presentation. Preserve earlier evidence for unchanged inputs and distinguish exact-candidate results.

A public release or permanent installation is a separate selected action. Prepare a reviewable candidate and concise manual checklist; do not turn this into an automatic full release campaign.

## 11. Operating rules that keep the work bounded

1. Read AGENTS.md and docs/VALIDATION-POLICY.md before selecting checks. Ordinary work uses a named focused gate and production compilation when needed.
2. Plan validation before building. Batch coherent changes into one candidate; build unchanged inputs once. Reuse that artifact until inputs change.
3. The repository's ordinary target is 15 validation minutes and ceiling 30, excluding safe completion/restoration. Keep this repair ledger separate from archived campaigns while retaining historical totals. Do not silently grant repeated extensions or consume time on unrelated checks.
4. Inspect each failed run before changing code or observer. Preserve failed evidence. Distinguish product failures, test setup failures, and lack of access.
5. Add behavioral tests for concrete risks. Cosmetic remaps need real UI inspection and affected binding checks, not dozens of tests restating assignments. Reuse existing infrastructure.
6. Screenshots support presentation; live command/damage/source observations support mechanics; audio clips support sound. A fixture PASS does not replace the owner's aesthetic judgment.
7. Restore the exact current intake installation, settings, and protected saves. KMG_AUTOMATION_BASELINE is immutable. Never write a campaign save to obtain evidence.
8. Respect DATA's admitted workflows and host restrictions. Do not rename/recreate a laptop-only phase to evade its restriction. Complete permitted source/art work and report one precise remaining native observation if a host boundary applies.
9. Keep changes scoped to owned summons. Do not disable ProperFlanking2, rewrite foreign spells, globally replace portraits, or alter baseline difficulty to make an observation pass.
10. Preserve approved gameplay from PRESERVATION.md. Regression testing follows changed dependencies, not the number of creatures in the catalog.
11. Each sprint ends with its actual product changes, evidence limits, a reviewable PR, and a copyable continuation/next-sprint prompt. Stop when its acceptance is sufficiently established.
12. No new “just one more” diagnostic sprint without a concrete unresolved player-facing problem. Do not use historical assertion counts as targets.

## 12. Final completion contract

Every required issue in ISSUE-REGISTER.md must reach its stated outcome. A demonstrated no-defect finding is valid for investigative questions such as timing, Griffon's apparent duplicate, or a damage-type audit; it does not cancel an explicit requested cut, addition, spell replacement, or model refinement. The three requested model revisions must produce actual improvements for inspection, not another unchanged-assets declaration of adequacy. Optional polish may be explicitly deferred.

The final review must show:

- The five requested cuts, including automatic paths, are effective.
- First-turn behavior and fear evaluation have a clear, correct disposition.
- Initiative portraits and affected action metadata are usable.
- Mephit breath mechanics and presentation agree.
- Cloud/Pixie spell reuse respects optional-provider availability.
- Mastodon and Griffon issues are resolved.
- Retained attacks/casts have appropriate animation and audible cues.
- The three requested model revisions are available for owner inspection.
- Approved unrelated gameplay remains protected.
- The exact current installation and saves are restored after any validation.
- No unreviewed aesthetic or unexecuted native acceptance is claimed.

See SOURCES.md for source and rules references. See ISSUE-REGISTER.md for the complete mapping of owner observations.
