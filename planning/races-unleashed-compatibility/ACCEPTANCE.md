# Spider Summoner acceptance contract

This file defines the final behavior for the complete mission. It includes the original compatibility repairs and the template, quantity, SNA, and DC enhancements. No acceptance item is closed merely because the original two unresolved contracts become recognized.

## 1. Quantity matrix

Apply this base-spell table independently to Summon Monster and Summon Nature's Ally. An em dash means no choice. Every added placement requires the real Spider Summoner feature.

| Cast tier | Giant Crab Spider, base I | Giant Black Widow, base IV | Ogre Spider, base V | Giant Tarantula, base VII |
| --- | --- | --- | --- | --- |
| I | 1 | — | — | — |
| II | 1d3 | — | — | — |
| III | 1d4+1 | — | — | — |
| IV | 1d4+1 | 1 | — | — |
| V | 1d4+1 | 1d3 | 1 | — |
| VI | 1d4+1 | 1d4+1 | 1d3 | — |
| VII | 1d4+1 | 1d4+1 | 1d4+1 | 1 |
| VIII | 1d4+1 | 1d4+1 | 1d4+1 | 1d3 |
| IX | 1d4+1 | 1d4+1 | 1d4+1 | 1d4+1 |

Coverage is 9 + 6 + 5 + 3 = 23 species/tier placements per family, 46 total. Eight are original own-tier placements; 38 are additional higher-tier placements. These totals describe the gameplay matrix, not blueprint allocation. Existing applicable count modifiers operate once on the base result. Every resulting eligible spider receives its template, normal summon buffs, and the feat benefit once.

## 2. Required outcomes

Record each ID in `STATUS.md` with implementation state, verification state, candidate identity, and evidence reference. A focused test and a native observation are different evidence types.

| ID | Required behavior | Minimum meaningful verification |
| --- | --- | --- |
| RU-01 | Supported installed feature, eight original rows, intended units, acquisition prerequisites, and DC ownership are identified accurately. | Inspect current metadata/hash and actual loaded graphs or installed DLL. Reconcile the 1.0.11/1.1.1 labels; preserve original evidence. No name-based discovery masquerading as exact identification. |
| RU-02 | Additional spiders require the native feat through every offered route. Ordinary Giant Spider keeps normal access. | Naturally acquire Spider Summoner on a legitimate Drow fixture. Without the feat, check menus and held/saved execution attempts. Remove the feat after selection and prove no slot/resource/spawn effect. Verify ordinary Giant Spider remains selectable without it. |
| RU-03 | All four named spiders actually appear in SM and SNA. Original IDs remain resolvable. | Eight original own-tier casts, one per species/family, through real spell routes. Capture spawned unit identity, count, source, and duration/lifecycle. Exercise both formerly broken Ogre branches across evil and non-evil casters; no Bralani/Redcap in either corrected effect. |
| RU-04 | Correct base quantities at every eligible tier, no premature or duplicate choices. | Behavioral coverage of every matrix cell and published contract. Native representative casts of 1, 1d3, and 1d4+1 across both families. Verify dice expression and actual count; do not infer distribution from one roll. |
| RU-05 | Existing summon modifiers operate once. | Native representative multi-summon with Superior Summoning, plus Augment Summoning creature stats and their DC interaction. Compare base expression, final count, and buffs; no wrapper-based doubling. |
| RU-06 | All four SM species and their higher-tier rows use standard template selection and correct HD bands. | Exact payload inspection for every row; focused good/evil/LN/N/CN selection and HD boundary cases. Native celestial, fiendish, and neutral-toggle cases; confirm final unit buffs/stat effects and absence of leftover opposite/native template. Exercise all four species through their distinct template configurations. |
| RU-07 | Template choice is current at execution, while SNA remains free of SM templates. | Change neutral mode after opening a menu, cast, and inspect the result. Open/cast SNA with both toggle positions and verify no SM template is added. Check established SNA alignment behavior without inventing a neutral exemption. |
| RU-08 | Common cleric policy governs menus and final execution using corrected effects. | Focused full alignment-axis cases plus a native lawful-good cleric, an evil cleric, morally neutral behavior, and a deity-derived restriction using an appropriate fixed-alignment control. Refused held selection spends nothing. Corrected Ogre is classified as its spider effect, not the prior Bralani. |
| RU-09 | Noncleric, multiclass, and ordinary spontaneous spellbooks remain correct. | A real noncleric feat holder keeps eligible choices. A real multiclass cleric uses the proper book and retains the established SM restriction. Observe an ordinary spontaneous SM cast and, where a valid installed spellbook offers it, an ordinary spontaneous SNA cast: exactly the correct level pool/book loses one use, other books/resources remain unchanged, and a forbidden/stale choice spends nothing. Prepared conversions and class SLAs do not satisfy that check. Record a genuinely absent provider narrowly. Cover existing unknown-deity behavior and unrelated item/SLA handling with affected focused controls. |
| RU-10 | Herald Caller conversions include all eligible SM spider quantities and spend the actual converted spell. | A naturally obtained Herald route at least once. Cast from an unrelated prepared source at a base tier and a higher quantity tier; inspect source chain, exact debit, book, caster level, duration and representative metamagic. Check newly restored Crab I/Widow IV admission. |
| RU-11 | Druid conversion and prepared SNA routes expose the intended SNA matrix. | Compare prepared SNA against conversion from an unrelated prepared spell. Cast original and higher-tier spider choices through conversion, with correct original-slot debit and no duplicates. |
| RU-12 | Poison DC benefit is exactly +2 on eligible summoned spiders and actual ongoing effects. | Trace each distinct supported poison implementation. Native first and continuing save observations; compare actual resolving DC against matched base parameters. Cover ordinary Giant Spider and representative RU poison paths; repeat a comparison with Augment Summoning active. No cumulative +2 per save. |
| RU-13 | Web DC benefit reaches the actual applicable held/escape effect without changing attack mechanics. | Native web hit and break-free/effect DC observation on ordinary Giant Spider and distinct RU web implementations that exist. No newly introduced initial save, attack bonus, extra uses, duration change, or global webbed-buff modification. Tooltips alone do not close this item. |
| RU-14 | Feat bonus is species/source-specific and applied once. | Matched normal Giant Spider casts with and without the feat; no bonus for ineligible sources. Focused exact-identity controls for Bebelith, Giant Scorpion, ordinary crabs, shared-donor ants/beetles, unrelated poisons/Web spells, and nonsummoned spiders. Repeated initialization/casts and affected reload do not duplicate benefits. |
| RU-15 | Menus are stable, alphabetized, correctly labeled, and free of duplicate template/quantity choices. | Structural manifest for all tiers; inspect representative early, mixed, and high-tier rendered menus in both families and conversion routes. Show real feature-enabled rows and a second caster without the feature. No blank icons or stale entry after changes. |
| RU-16 | Original and new saved references survive supported reload/module behavior. | Fresh process plus save/reload of an original RU shortcut and a new higher-tier shortcut, with RU still installed. Verify casting and one active summoned spider/effect. Repeat affected disabled-module behavior using the established lifecycle; no dangling new GUID or leaked KMG choice. |
| RU-17 | Optional dependencies, late initialization, and mutation ownership are safe. | Valid RU-absent profile loads without integration errors. Valid Expanded-Summoning-inactive/RU-present control preserves native behavior. Focused delayed init, repeated reconciliation, cache invalidation, and later foreign mutation/compare-before-restore cases. Never remove another mod's rows. |
| RU-18 | Unknown contracts remain honestly unsupported; normal spells are unaffected. | Negative cases for wrong feature, unit, tier/count, condition, buff recipient, hidden spawn, and unexpected effect. Diagnostic stays explicit, generated rows are withheld from an unverified source, and unrelated native/foreign behavior is preserved. |
| RU-19 | The observer measures the new intended behavior and preserves historical evidence. | Replace old Bralani spawn and 3-inherent/1-governed success assumptions with per-row semantics. Zero unresolved supported final rows; explicit unsupported records for unknown shapes. Evidence is tied to actual tested source and bytes. |
| RU-20 | Candidate delivery and restoration are complete. | Reviewable draft PR, relevant docs and status, exact candidate identities, focused/native scope and results, and successful exact restoration. No merge/public release/permanent installation. If a parent selection graph changed, include a focused unaffected NPC selection/AI control. |

## 3. Scenario selection

Do not run the Cartesian product of every species, tier, alignment, deity, modifier, and UI route. Cover the matrix structurally and through behavior tests, then choose native cases that exercise each distinct implementation and integration risk.

Useful representative cases include:

- Legitimate Drow noncleric with the feat, plus the same relevant access control without it.
- Legitimate Drow Herald Caller with the feat, with original and higher-tier conversion casts.
- Drow Druid with the feat, comparing prepared SNA and spontaneous conversion.
- Lawful-good/Erastil and lawful-evil/Asmodeus cleric cases for corrected spider effects; verify pairing prerequisites instead of forcing invalid natural characters.
- Morally neutral template selections; cover LN, N, and CN in focused tests, with actual neutral-toggle execution observed natively.
- A deity-derived restriction on a fixed-alignment ordinary control, preserving the existing policy rather than making a caster-matched spider artificially illegal.
- One relevant multiclass spellbook and one source carrying representative metamagic.
- An ordinary spontaneous SM spellbook, plus an ordinary spontaneous SNA spellbook if the supported installed classes provide one. Verify pool consumption separately from prepared-spell conversion.
- Actual Giant Spider poison/web observations with the feat off/on, and affected RU effects with their legitimate feat present.

The four feat-exclusive species cannot be legally cast through their feat-only routes after the feat is removed. For their DC controls, compare the observed resolving value with verified unbonused parameters or an explicitly isolated mechanics fixture. Use ordinary Giant Spider for a clean legal before/after-feat native cast comparison. Do not bypass the feature gate and describe the resulting cast as a natural player route.

Observe distinct poison/web implementations; a shared proven implementation can justify avoiding redundant native repetitions, but a shared name, model or donor is not proof. An absent poison/web ability is recorded as inapplicable for that species, not invented. The DC benefit must still be implemented and verified for every applicable in-scope ability.

## 4. Profile and artifact discipline

The main profile is the current valid combined installation. Capture actual dependencies and current enabled settings. Minimal profiles are used only for a concrete attribution or optional-dependency question. A valid RU-absent profile must also omit dependent mods that cannot load without RU. No historical installation is substituted for the current one.

Keep source SHA, package/DLL hash, assembly identity where used, observer/fixture version, mod versions/hashes, profile, save fixture and run IDs together. An unchanged candidate can reuse its artifact. A code/compiled-observer change creates new bytes and needs affected verification.

Inspect real source debits across all books and applicable resources. Record possible action-graph references separately from spawned instances. Screenshots do not establish unit identity, actual DC, or slot consumption. A menu-only success does not establish a cast.

Follow the current validation target and ceiling in MISSION and the repository policy. Do not repeatedly launch unchanged campaigns, erase failures, or use elapsed time as grounds to abandon restoration. Finish safe cleanup even if the ordinary testing ceiling is reached.

## 5. Completion states

- **Complete for review:** the full required scope is implemented, focused checks pass, required native observations pass on the delivered candidate, and restoration succeeds. This is focused compatibility acceptance, not full release qualification.
- **Implemented with verification pending:** the implementation is reviewable, but named required native observations remain unavailable or outside the current authorized validation limit. Keep them open and do not claim mission completion.
- **Blocked or failing:** an unresolved source/engine contract, implementation defect, failed verification, or restoration issue prevents acceptance. State the exact ID, evidence, impact, and smallest next step.

Use a status table with columns `ID`, `Implementation`, `Focused evidence`, `Native evidence`, `Candidate`, and `Remaining work`. Do not initialize the table with passing results. An owner acceptance exception must be explicit and must not alter the original test verdict.
