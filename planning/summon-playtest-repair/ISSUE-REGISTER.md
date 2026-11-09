# Manual Playtest Issue Register

Owner: Howie. Intake: November 7, 2026. Environment: DATA.

All entries below start as planned work, not completed fixes. The report and images do not identify the loaded DLL. Record that identity during implementation intake. Evidence numbers refer to EVIDENCE-INDEX.md. A screenshot establishes visible presentation, not an underlying code diagnosis.

## Status (updated with each sprint)

| ID | Status | Change / evidence | PR |
| --- | --- | --- | --- |
| CUT-01 Stirge | Intentionally removed (R1 checkpoint) | Withheld from all public routes; identities retained. Focused tests + native `r1-stirge-withheld-everywhere`. [R1-STATUS](R1-STATUS.md) | #111 (merged) |
| CUT-02 Wall of Ice | Intentionally removed (R1 checkpoint) | Bone/Ice Devil grant, parameters, AI removed; cleanup identities retained; load fixer. Native fresh + induced-legacy PASS. Pre-existing wall retirement not re-run natively (no dependency changed). | #111 (merged) |
| CUT-03 Magma Form | Intentionally removed (R1 checkpoint) | Grant, resource, AI removed; breath/Pyrotechnics retained; load fixer. Native PASS. | #111 (merged) |
| CUT-04 Bardic Suggestion | Intentionally removed (R1 checkpoint) | Grant, parameters, noncombat route removed; load fixer. Suggestion spell not natively observed (optional provider inactive on DATA). | #111 (merged) |
| CUT-05 Fire Giant Sunder | Intentionally removed (R1 checkpoint) | Native Improved Sunder withheld; `SunderAction`, ProperFlanking2 `SunderActionToggleAbility` and its `SunderActionToggleBuff` absent; Storm Giant keeps them with the buff active. Native PASS on the corrected observer (candidate `d46726d46ddc`), including induced-legacy buff removal; no attack executed, no cold load. | #111 (merged) |
| LIFE-02 Repeated Frightened saves | Repaired (R2, closed) | Cause: the shared Bone/Ice Devil fear aura's per-source memory marker inherited the Frightened context's descriptors, so the Nalfeshnee's Unholy Aura (`BuffDescriptorImmunity` MindAffecting) refused it and the aura re-evaluated on every tick. Marker now built from its own blueprint. Native PASS 8/8 on candidate `c370ec234b70`: stationary pair one save, hostile control frightened once, exit/re-entry, pre-R2 marker refusal recorded on the same bytes, Bone Devil regression. [R2-STATUS](R2-STATUS.md) | #112 |
| LIFE-01 Acadamae readiness | Closed (R2, owner-accepted) | Ordinary Acadamae readiness verified for the requested cases; the reported creature-specific delay was not reproduced. Surprise-round delay was observed and explained: an arriving summon is marked Surprised and first acts in round 1. Native arrival behavior is retained for this release. Acceptance of the current adaptation, not a claim that every tabletop reading requires it. Final surprise PASS: candidate `8a3176bdc312`; ordinary cases: `9c13837dd4ce`, `4a51fdfecc`. Original failures and qualifications kept in [R2-STATUS](R2-STATUS.md). | #112 |
| UI-01 Initiative portraits | Complete for review; data PASS; **rendered PASS** (`57a8e659b1e6` tracker screenshot inspected; the unrelated Auto End Turn prompt was not detected on that candidate); owner approval pending | Own portrait for all 134 registered summon units via the KMG unit's `m_Portrait`. Consumer: tracker view → `InitiativeTrackerPortraitZone.SetUnit` → `unit.Portrait.SmallPortrait`. The captured tracker shows all seven representatives' own art with native overlays. [R3-STATUS](R3-STATUS.md) | #113 |
| UI-02 Lantern ray icons | Complete for review; data PASS; **rendered PASS** (`31e61aa73a01`); owner approval pending | Searing Light / Scorching Ray art, distinct, shown in the native ability description windows. | #113 |
| UI-03 Unnamed blast | Repaired; data PASS; **rendered PASS** (`31e61aa73a01`); owner approval pending | Owner: native `AzataGhaelLightRay` `33e89979…`, granted through the KMG Ghaele's own `AddAbilityToCharacterComponent.Abilities`. Redirected to the owned `c3224dec…`, whose window shows "Light Ray", a description and an icon. The live brain has one AI action referencing the owned ray and none referencing the native ray (no cast executed). Donor unchanged; crystalline classification kept. | #113 |
| UI-04 Unholy Nimbus icon | Complete for review; data PASS; **rendered PASS** (`31e61aa73a01`); owner approval pending | Native Unholy Aura art (sprite `ProtectionFromGood`), visible. | #113 |
| UI-05 Overrun icon | Repaired; data PASS; **rendered PASS** (`31e61aa73a01`); owner approval pending | Overrun shows brachiosaurus creature art and no Duration/Saving Throw rows. Trample keeps the protected `Longstrider` art and shows its Reflex / attack-of-opportunity Saving Throw. No inherited "Trample" or native maneuver text. | #113 |
| UI-06 Null tooltip rows | Repaired; data PASS; **rendered PASS** (`31e61aa73a01`) | Sprint "1 round"/"None", Nimbus, Trumpet Blast "1d4 rounds"/"Fortitude negates"; Overrun/Trample as above. No `<null>` in the eight captured windows. | #113 |
| MEPH-01 Breath damage audit | Complete for review (R4); ten-row matrix | Air electricity → slashing, Earth acid → bludgeoning, Water electricity → acid, through owned breaths on the four native-backed mephits (grant/brain redirected; donors untouched). Six variants: no damage defect. Evidence: Air loaded profile, grant and AI reference only (no Air cast); Earth executed bludgeoning on the target (stray absence not asserted); Water executed acid with the acid cone and no stray events. Controlled failed saves; loaded 1d12 preserved. Charter ally-safety repaired: the enemies-only gate on all ten; native case shows an ally in the cone untouched. [R4-STATUS](R4-STATUS.md) | #114 |
| MEPH-02 Water acid | Complete for review (R4) | Executed acid-only packet, native acid cone launched, inspected green impact (not the cone in flight). | #114 |
| MEPH-03 Breath icons | Complete for review (R4); owner approval pending | Ten distinct existing-art icons inspected in native windows. Salt now uses Metal Blast, chosen after inspecting extracted native art. | #114 |
| MEPH-04 Pyrotechnics flash | Complete for review (R4); owner approval pending | One native blinding burst per executed cast on the caster, captured while alive (requested frame 1500, alive), retires by itself; a cancelled command spawns nothing. | #114 |
| SPELL-01 Cloud Giant Mist | Complete for review (R5; review repair `06d9bc4d0e`: binding is atomic and stays hidden and guarded on any failure; focused tests only); owner approval pending | One owned at-will SLA wrapping Call of the Wild's `ObscuringMistAbility` (point, medium range, 20 ft, CL 16); both custom Fog Cloud grant and AI route removed; saved giants retire them via the fixer. Native (commanded, `366fad0ac41b`): two casts each spawn the CotW mist at the chosen point, available after each; live giant has no fog route. Provider-absent omission covered by tests only (not observed natively). [R5-STATUS](R5-STATUS.md) | R5 (stacked on #114) |
| SPELL-02 Pixie at-will spells | Complete for review (R5) | Owner adaptation: at will and full Confusion (printed: daily uses, Lesser Confusion). Owned SLAs from native Entangle, Confusion, Dispel Magic (+2 variants), Shield; native AI actions copied where they exist. Native: each executed twice, Standard, resource-free, still available (controlled failed saves); Dispel area variant not executed; no autonomous AI cast observed. | R5 (stacked on #114) |
| SPELL-03 Pixie Dance | Complete for review (R5; review repair `06d9bc4d0e`: atomic binding, one-use cost prepared before publication, no unrestricted path on failure; focused tests only) | Existing Dance identity now wraps CotW `IrresistibleDanceAbilityCast` (sticky touch to the CotW touch spell), keeping one use per summoning. Native: buff landed on the hostile from the Pixie (controlled natural-20 touch, controlled failed Will DC 19); 1 → 0 uses, then unavailable. Omitted without the provider (tests only). | R5 (stacked on #114) |
| MECH-01 Griffon two charges | Repaired (R5) | Confirmed duplicate: the engine grants native Charge to every unit and the Griffon granted the same blueprint again. Explicit grant removed with a build guard; Pounce/attacks/reach unchanged. Native: live Griffon has exactly one Charge. Saved Griffons may keep the extra copy until they expire. | R5 (stacked on #114) |
| MECH-02 Mastodon trample | Added (R5) | Shared ungulate carrier with the Mastodon's own 2d8+18, Reflex DC 29 (14 HD, Str 34); Elephant and Aurochs unchanged. Native commanded trample: one contact, DC 29, 2d8+18 (28 dealt), no AoO. Shared Trample window wording ("0 ft. feet", school "None") pre-existing, recorded for the owner. | R5 (stacked on #114) |
| UMM Params.xml (runner) | Protected (R5; review repairs `06d9bc4d0e`, ownership repairs `307cf89356`) | R4 qualification unchanged (intake not restored; exact restoration not established). R5: the guarded wrapper backs up, verifies and restores Params.xml bytes with a durable receipt; both R5 runs were restored and verified at the R5 intake. Review repairs: the receipt is tied to the owner's lease and process identity; recovery is serialized through the coordination lock and refused for a live owner, a foreign lease or a running game; installation and UMM cleanup are independent both ways, with PASS refused on either failure. Ownership repairs: automatic owner restoration now holds a lease in the receipt's coordination root (its own or a validated inherited one) through recheck, restore and receipt completion, and a foreign transaction is refused with no write; a game running at entry is refused before any lease, leaving no lock or recovery debt. 85 assertions pass on temporary files and leases; no build, launch or live change. | R5 (stacked on #114) |
| All others | Planned | Not started. | — |

## R1 — Explicit release cuts

| ID | Owner observation / decision | Required disposition and boundary | Acceptance |
| --- | --- | --- | --- |
| CUT-01 | Stirge attachment animation jitters; drain appears to work. Omit it this release and retain code. E02. | Withhold Stirge from all affected KMG public summon choices, including higher tiers and supported alternate menus. Preserve identities, assets, registration, and legacy implementation. | No public Stirge route remains; unrelated choices stay. Do not repair grapple animation in this mission. |
| CUT-02 | Wall is functional but unattractive and screams; the owner's later decision removes it from both creatures. E10–E11. | Remove Bone Devil and Ice Devil Wall of Ice grants and automatic use. Retain safe cleanup and serialized identities for existing walls. This supersedes ice material and audio polish. | Fresh affected devils cannot cast Wall; old owned state retires safely. No new Wall feature campaign. |
| CUT-03 | Magma Form has no visible effect; cut it. | Remove Magma Mephit's Magma Form grant, dedicated resource exposure, and automatic route. Retain Magma Mephit, breath, and Pyrotechnics. | Removed command and AI route; retained abilities still granted. Legacy states remain resolvable. |
| CUT-04 | Lillend has Suggestion and Bardic Suggestion; cut the ability. | Remove the separate Bardic Suggestion performance/automatic route. Preserve Suggestion spell and other performances. | Exactly the requested duplicate capability withdrawn; spell remains usable under its existing provider contract. |
| CUT-05 | Fire Giant should never use Sunder; retain implementation. | Remove Fire Giant's Sunder capability, including its Improved Sunder grant and any owned automatic route. Keep shared/native code and Storm Giant unchanged. | Ordinary Fire Giant attacks are not replaced with Sunder under the installed ProperFlanking2 configuration. |

## R2 — Combat lifecycle

| ID | Observation | Required investigation / repair | Acceptance |
| --- | --- | --- | --- |
| LIFE-01 | Acadamae Graduate summons sometimes wait. Initial observation may be surprise-round related; Pteranodon/mephits seem slower than Lion/Hell Hound. | Separate ordinary-round readiness, surprise action restrictions, spawn/appearance lock, initiative enrollment, and AI delay. Use matched controls and the existing activation path. | Native first legal action occurs correctly without an extra turn, an artificial initiative reset, or an unconditional full action grant. Surprise behavior has a separate documented result. |
| LIFE-02 | Nalfeshnee repeatedly succeeds on Frightened saves near Ice Devil. E10–E12. | Identify exact source, target, aura, relationship filter, cadence, and entry/exit behavior. DC 22 suggests Ice Devil as a lead only. Fix mechanics rather than just hiding logs. | No unintended rapid repeated saves for a stationary pair; legitimate fear still applies according to the verified contract. Do not invent a universal successful-save immunity. |

## R3 — Portraits, icons, and tooltip text

| ID | Observation | Required repair | Acceptance |
| --- | --- | --- | --- |
| UI-01 | Dire Rat uses a wolf initiative image; Stirge a bird; other summons use inappropriate donor/placeholder portraits. E01–E02, E04–E05, E09. | Audit all published summon portrait bindings. Reuse existing approved creature artwork on the actual turn-based portrait surface. Exclude withheld Stirge from release-blocking native checks. | Roster mapping is correct and representative actual initiative screens show the right creature. Preserve native status overlays and foreign donor assets. |
| UI-02 | Lantern Archon Light Ray and Twin Light Ray have no icons. E05. | Resolve both actual action blueprints and bind appropriate existing art. | Both actions have meaningful icons in their real visible surfaces. |
| UI-03 | Ghaele reportedly has an unnamed, iconless blast. E07. | Trace actual selected owner/action GUID before editing: screenshot hover and combat log differ. Provide an accurate name, description, and icon. | No affected unnamed/blank action remains; the fixed owner is identified in evidence. |
| UI-04 | Unholy Nimbus has no icon. E10. | Assign an appropriate existing icon to its visible action. | Correct visible icon and preserved mechanics. |
| UI-05 | Brachiosaurus Overrun has no icon. | Assign an appropriate existing maneuver icon. | Icon visible where the action is presented; behavior unchanged. |
| UI-06 | Cheetah Sprint shows null fields; Nimbus screenshot does too. E03, E10. | Provide correct Duration/Saving Throw metadata or supported omission of inapplicable rows. Inspect affected sibling metadata only. | No literal <null> fields on these actions. Do not change working mechanics to fit text. |

## R4 — Mephit repair

| ID | Observation | Required repair | Acceptance |
| --- | --- | --- | --- |
| MEPH-01 | Water breath appears electrical; audit all mephit damage types. | Audit all ten live/effective damage types against PF1 and documented project adaptations: air, dust, earth, fire, ice, magma, ooze, salt, steam, water. Include base-donor types. Dice, save, recharge, and sickness are reference context; preserve accepted behavior unless a concrete related defect requires a local repair. | Ten-row actual/expected damage-type matrix. Repair demonstrated defects without reopening all mephit rules. |
| MEPH-02 | Water breath should look acidic. | Use an acid visual; correct damage if its actual packet is wrong. Do not infer damage type solely from particles. | A native cast looks acidic and its damage packet is acid. |
| MEPH-03 | Air uses Burning Hands; choose breath icons that match elements. | Map every breath to appropriate existing spell/class/dragon artwork. Air/earth/salt/dust can be physical, not energy. | No misleading generic fire icon across unrelated breath types. |
| MEPH-04 | Pyrotechnics is visually silent; at least a quick flash. | Add a brief suitable native effect on successful execution, with ownership/cleanup intact. | Visible flash without duplicate execution, lingering particles, or changed mechanics. |

Dust's ice-like cone is explicitly tolerated for this release. Magma Form removal is CUT-03, not a new visual implementation. Positive observations about other mephits protect their working behavior while the requested damage audit still covers all ten.

## R5 — Abilities and actual spell integrations

| ID | Observation / decision | Required repair | Acceptance |
| --- | --- | --- | --- |
| SPELL-01 | Cloud Giant fog seems ineffective. Replace both bespoke fog actions with the actual Obscuring Mist spell at will. E06. | Resolve the installed spell/provider; create one owned SLA using its ordinary target-point behavior. Remove both existing custom fog grants/AI routes. Omit if optional provider is absent. | One working at-will targetable Mist SLA with donor present; no custom fallback, duplicates, or missing-reference faults without it. |
| SPELL-02 | Pixie should have at-will Entangle, Confusion, Dispel Magic, Shield. | Use actual spells with appropriate SLA context. Document the owner's intentional change from printed daily uses/Lesser Confusion. | All four named spells can be used repeatedly with correct native costs and effects. Confusion is full Confusion as requested. |
| SPELL-03 | Pixie Dance should use the actual installed Irresistible Dance spell. E13. | Resolve exact donor/provider; replace the active custom substitute. Conditional omission without provider. Preserve intended Dance frequency, which was not included in the four at-will additions. | Real donor behavior when available; clean omission when unavailable. |
| MECH-01 | Why does Griffon have two charges? | Determine whether the report means duplicate buttons, resource uses, or distinct abilities. Remove confirmed duplication; otherwise label/explain distinct behavior. | Evidence-based disposition without guessing or changing pounce/attacks/resources unnecessarily. |
| MECH-02 | Mastodon lacks Elephant-like Trample. | Add/repair Trample using the proven mechanism and Mastodon's own damage/DC/stat scaling. | Real trample; one target is not repeatedly damaged improperly in the same round; Elephant/Aurochs preserved. |

## R6 — Motion, effects, and audio

| ID | Observation | Required repair | Acceptance |
| --- | --- | --- | --- |
| PRES-01 | Hound Archon deals sword damage without swinging. | Connect compatible sword animation to actual native attack/contact events. | Visible attack, contact, and recovery; hit count and damage unchanged. |
| PRES-02 | Hound Archon Aid lacks a casting action. | Add appropriate native casting gesture/effect through the existing action. | Visible cast tied to the real Aid execution, without a second cast or cost. |
| PRES-03 | Redcap should use its actual scythe; native Redcaps may supply it. | Inspect and reuse compatible native Redcap weapon/presentation/animation. Keep owned summoned unit statistics and AI. | Visible scythe and actual attack agree; no imported encounter-only behavior. |
| PRES-04 | Lantern Archon has an intermittent blue protrusion. E04–E05. | Inspect renderer/donor/attachment behavior, then correct the offending owned visual. | Coherent luminous orb in idle/movement/cast sequence. Preserve intrinsic light and rays. |
| PRES-05 | Vrock Stunning Screech works but is silent. | Add a suitable screech cue at successful execution. | Audible single cue at ordinary game volume; no repeated loop or mechanical change. |
| PRES-06 | Trumpet Archon blast should have a trumpet/horn sound. | Prefer a fitting native cue; otherwise appropriately licensed audio with provenance. | Audible blast cue synchronized to the actual action. |
| PRES-07 | Persistent shiny summon effect is inconsistent. | Chosen style: remove the generic persistent sheen consistently from KMG summons after identifying it. Preserve brief arrival effects, inherent glow/transparency/invisibility, selection, and real buffs. | Small mixed group shows consistent generic treatment without erasing creature identity or actual spell effects. |
| PRES-08 | Ankylosaurus knockback would benefit from an impact sound. | Optional low-cost reuse of a suitable existing cue; preserve the owner-approved bull rush. | Optional polish only; may be explicitly deferred without blocking completion. |

## R7 — Model revisions

| ID | Observation | Required revision | Acceptance |
| --- | --- | --- | --- |
| ART-01 | Glabrezu looks silly; middle arms are not fully attached. E08. | Repair arm-root geometry/attachment, then refine anatomy and material transitions. | Continuous small-arm roots in idle/walk/attack, four-arm identity retained, mechanics unchanged. |
| ART-02 | Nalfeshnee's current 3D model is absurd. E09, E12. | Substantial face/body/limb/wing-root/material refinement. A brighter or larger version alone is insufficient. | Readable, credible creature at ordinary zoom and close-up, shown in actual before/after frames and relevant motion. Owner aesthetic review remains separate from a technical pass. |
| ART-03 | Ice Devil model is OK but merits another revision. E09–E12. | Focus on head/mandibles, carapace-to-torso coherence, joints, and attachment transitions. Preserve accepted silhouette, size, spear, tail, and attacks. | Clear improvement shown against the new DATA baseline. Do not demand inaccessible historical laptop frames. |

## Explicitly retained or deferred

- Dust Mephit's ice-like cone: tolerated for now; no mandatory new effect.
- Dinosaur awkwardness: accepted/charming; no roster-wide dinosaur rework.
- Fire/Frost Giant similarity: recorded observation, not a required redesign.
- Ankylosaurus sound: optional PRES-08.
- Mounted Combat learning from Aurochs: a future project idea, not an authorized cross-repository task.
- Wall ice-material and scream improvements: superseded by CUT-02, not an outstanding repair.
- Stirge grapple animation: deferred with CUT-01; retained implementation is intentionally unpublished.
- Historical reach-balance questions and old Wall diagnosis: outside this new manual-playtest mission unless a changed path directly regresses them.

## Tracking fields for implementation

For each required issue, record status, changed files/IDs, candidate identity, focused evidence, remaining limitation, and PR. Use fixed, intentionally removed, or specifically blocked. An investigative item may be correct-as-observed with an explanation; that outcome does not cancel an explicit requested cut, addition, replacement, or model refinement. Do not turn lack of observation into a pass. Optional PRES-08 may be deferred explicitly. Keep the owner's broad positive observations in PRESERVATION.md.
