# Base Gunslinger fidelity matrix

Overnight bug-fix note (2026-08-19): the current work order preserves all
accepted tabletop/Kingmaker adaptations while reopening only the twelve
human-observed implementation and presentation failures. No balance adaptation
changes are authorized by the repair mission.

Optional-mod compatibility note (0.0.72): compatibility work adds no tabletop
adaptation or balance rule. Existing base and Mysterious Stranger fidelity
dispositions remain authoritative and regression-sensitive under every profile.

Sixth-playtest note: Dodge and Deadeye now pay immediately; Gunsmithing has the
documented 20/20 once-per-rest Kingmaker adaptation; BTSL testing vendors use
exact optional native tables. Presentation/projectile/audio structural repairs
await the consolidated human verdict and are not visually/audibly accepted.

The authoritative local class text is
`C:\Dev\KingmakerGunslingerLab\private\rules\GUNSLINGER_PFSRD.md`. Alternative
deeds and archetype replacements are excluded from the base-class matrix.
Classifications are provisional until the implementation checkpoint records
and qualifies its exact Kingmaker mapping.

Sprint 60 runtime-qualified the cross-cutting presentation graph on `adcb030`:
all 75 visible project-owned facts reachable from the progression have stable
localized names, descriptions, and icons; one hidden implementation fact stays
excluded; and the unchanged 20-level progression is organized into six native
UI groups. ADR-0007's inherited Early Pistol/Light Crossbow icon is the
fail-closed fallback because installed Fighter class/progression icons are null.

Sprint 61 runtime-qualified the supporting acquisition adaptation on `c2fd27b`:
the exact capital Jhod shared-vendor table appends one each of the four
player-fireable firearms and 99 each of powder, balls, and repair kits using
observed native quantity precedents. Native entries and prices are preserved;
Blunderbuss Scatter Shot is runtime-qualified on `d47fa60` and is player-fireable; vendor publication is the next gate.

Sprint 75 runtime-qualified the base class chassis on `e35de17`. Two fresh
save-free native `CharGen`/`LevelUp` observations reproduced d10 player-class
base HP `0 -> 11 -> 18`, class skill base 4, Intelligence-10 evaluated skill
points `4/4`, class level 2, and exact detached-unit isolation. Together with
the Sprint 72 level-20 BAB, saves, and fact graph, the chassis row is complete;
creation and broad player-respec integration remain separately tracked.

| Level | Feature | Classification | Current state | Required disposition / adaptation question |
|---|---|---|---|---|
| 1 | Weapon/armor/firearm proficiencies | EXACT | Native level-one preview contains exactly one production aggregate plus exact simple, martial, light-armor, and firearm proficiency facts; source isolation and cleanup reproduced twice | Preserve through creation commit, level-up, multiclass, and respec |
| 1 | Gunsmith and battered starting firearm | ADAPTED | Option 1 is authorized. Visible Gunsmithing exclusively grants kit-backed Repair/Overhaul; an inert item-owned enchantment carries the originating unit through its serialized parent context; firing, reload, misfire, and deed use gates apply nonowner effective condition without mutating actual state; an exact player-to-vendor instance hook returns fixed 22 gp only for marked items. Exact `6b1e413` PASS pair `20260802T1441506873456Z` / `20260802T1445126858809Z` runtime-qualifies native grant, ownership, 22 gp value, ordinary-item isolation, rollback, and no-save behavior | Crafting/rest selection remain unavailable Kingmaker interactions; no unresolved mandatory mechanical choice remains in the authorized adaptation |
| 1 | Grit: Wisdom-based pool, daily reset, critical/killing-blow recovery | EXACT | Runtime-qualified pool/rest/persistence and recovery. Exact confirmed critical and weapon-damage zero crossing restore separately; helpless/unaware/below-half-level targets are excluded; attack/target references weakly dedupe | Preserve through deed integration |
| 1 | Deadeye | ADAPTED | Runtime-qualified personal free action arms a native persisted fact; next successfully discharged exact firearm spends one grit per increment beyond first and extends touch AC without changing native range penalties; insufficient grit rejects atomically | Preserve through save/respec and final integrated combat regression |
| 1 | Gunslinger's Dodge | ADAPTED | Runtime-qualified drop-prone reaction: personal free action arms a persisted fact; the next ranged weapon attack spends one grit in light/medium armor at light load, applies native prone, and adds +4 AC exactly once; insufficient grit fails atomically | Implement the 5-foot movement / +2 AC alternative only after safe deterministic destination selection is established; Kingmaker has no Immediate action type |
| 1 | Quick Clear | ADAPTED | Runtime-qualified exact single-equipped-firearm actions: standard action requires positive grit without spending; move action spends one grit; both atomically change the item-owned misfire-origin Broken state to Normal without a repair kit | Preserve through save/respec and final integrated combat regression |
| 2/6/10/14/18 | Nimble +1..+5 | EXACT | Runtime-qualified five cumulative native Dodge-descriptor facts at exact levels; two fresh runs proved +5 in light/no armor, zero in medium armor, and native flat-footed exclusion | Preserve through level-up/respec and final integrated regression |
| 3 | Gunslinger Initiative | ADAPTED | Runtime-qualified +2: level-three feature adds +2 to the owning unit's exact native initiative roll while current grit is positive, with no spend and rule-object duplicate protection. The conditional firearm draw is OMITTED-NO-MEANINGFUL-INTERACTION because installed Kingmaker 2.1.7b has no player-facing Quick Draw feat; forcing a weapon-set switch would also bypass native action accounting | Revisit the draw clause only if the separately scoped feat work introduces Quick Draw with an exact free-hands/visible-firearm contract |
| 3 | Pistol-Whip | ADAPTED | Runtime-qualified standard-action ability requires exactly one equipped non-Wrecked firearm and one grit, then uses an unowned transient native melee surrogate: 1d6 one-handed or 1d10 two-handed, bludgeoning, 20/x2, firearm enhancement copied to attack/damage, and a free native Trip event on hit. Grit is spent on delivery even on a miss; ammunition, misfire condition, equipment, and persistence are unchanged | Preserve through level-up/respec and final integrated regression |
| 3 | Utility Shot: blast lock | OMITTED-NO-MEANINGFUL-INTERACTION | Exact installed metadata exposes locks through map interaction, Pick Lock, and Disable Device contracts, not firearm attack targets with lock AC, quality, jammed state, or destructible unlocking semantics | Revisit only if a supported firearm-targetable lock system is introduced |
| 3 | Utility Shot: scoot unattended object | OMITTED-NO-MEANINGFUL-INTERACTION | Exact installed metadata exposes no general Tiny-or-smaller unattended-object combat target with suppress-damage and 15-foot movement semantics | Revisit only if supported movable unattended combat objects are introduced |
| 3 | Utility Shot: stop bleeding | ADAPTED | Runtime-qualified standard-action self/adjacent ability requires positive grit without spending it, consumes exactly one loaded chamber without an attack roll or damage, and removes exactly one native `SpellDescriptor.Bleed` fact with rollback on delivery faults | Preserve through level-up/respec and final integrated regression |
| 4/8/12/16/20 | Bonus feats | EXACT | Runtime-qualified by reusing the exact installed Fighter combat-feat `BlueprintFeatureSelection` once at each required level, retaining its 108 aggregate candidates and prerequisite enforcement without cloning or a project-owned ID. Kingmaker has no native grit-feat category; no deeds are mislabeled as feats | Qualified on `4604717` with exact mod load and two guarded native-selection PASS runs; append independently authorized grit feats later if introduced |
| 5/9/13/17 | Gun Training selections | ADAPTED | Runtime-qualified as cumulative distinct selections by immutable `FirearmDefinition.Kind`; matching choices add the exact Dexterity modifier through native weapon-stat damage once and reduce the Broken misfire increase from +4 to +2. Borrowed weapon categories remain excluded as identity | Qualified on `76ae9f9` by exact mod load and two guarded production damage/misfire PASS runs |
| 7 | Dead Shot | ADAPTED | Runtime-qualified full-round ability at level 7: BAB supplies 1-4 native iterative probe rolls, one item-owned chamber and grit point are spent, hits aggregate into one native firearm damage delivery whose base dice alone multiply, threats request one native confirmation at the adjusted penalty, and condition changes only when every probe misfires | Qualified on `fdd5d7c` by exact mod load and two guarded mixed/all-misfire PASS runs; preserve through final integrated regression |
| 7 | Startling Shot | ADAPTED | Runtime-qualified standard-action weapon ability: positive grit is required but not spent, one item-owned chamber is consumed, the intentional miss emits no attack/damage event, and a one-round native `LoseDexterityToAC` buff makes the enemy flat-footed until its next turn; exact applied-fact reconciliation preserves atomic rollback | Exact `8609ebd` fresh-process PASS pair `20260802T1451324004693Z` / `20260802T1452540172188Z` proved chamber `1->0`, grit `3->3`, damage `0->0`, one exact six-second native flat-footed fact, isolation, and cleanup |
| 7 | Targeting: arms | ADAPTED | Runtime-qualified full-round one-grit no-damage firearm attack; an eligible hit applies the exact installed `DisarmMainHandBuff` for one six-second round, while sneak-attack immunity suppresses the rider | Exact `98a2a59` mod load plus PASS pair `20260802T1934063083002Z` / `20260802T1935297321491Z` proved chamber `1->0`, grit `2->1`, damage `0->0`, six-second native Disarm, isolation, and cleanup |
| 7 | Targeting: head | EXACT | Runtime-qualified full-round one-grit ordinary firearm attack; exact applied-fact reconciliation gives an eligible hit a one-round mind-affecting native Confusion fact only when not sneak-attack-immune, and authoritative target damage delta proves native damage | Exact `8609ebd` fresh-process PASS pair `20260802T1454137455208Z` / `20260802T1455368086818Z` proved hit, grit `3->2`, chamber `1->0`, native damage `3/4`, exact six-second nonpermanent Confusion, isolation, and cleanup |
| 7 | Targeting: legs | ADAPTED | Runtime-qualified full-round one-grit ordinary firearm attack; after a hit it dispatches normal native firearm damage, then requests one guaranteed-strength native Trip event only when the target is neither sneak-attack-immune nor natively immune to combat maneuvers; successful Trip ensures native prone aftermath | Qualified on `1a2f29c` with exact mod load and two guarded damage/Trip/native-immunity PASS runs; Kingmaker exposes no reliable general body-location or leg-count contract, so native maneuver immunity is authoritative and anatomy is never inferred from names |
| 7 | Targeting: torso | EXACT | Runtime-qualified full-round one-grit ordinary firearm attack; only its reference-marked non-sneak-immune attack lowers the per-rule native critical edge to 19, retaining native confirmation, multiplier, damage, misfire, and chamber behavior | Exact mod load and two guarded natural-18/natural-19 PASS runs retained |
| 7 | Targeting: wings | OMITTED-NO-MEANINGFUL-INTERACTION | Confirmed by exact installed 2.1.7b metadata inspection: only a visual `UnitAnimationSpecialAttackType.Wing` value and an `ActivatableAbilityGroup.Wings` UI grouping exist; there is no general flying/airborne/grounded state, altitude, falling, or flight-loss rule contract | Revisit only if Kingmaker gains a supported general flight-state and flight-loss interaction; never infer anatomy or flight from blueprint names |
| 11 | Bleeding Wound | ADAPTED | Runtime-qualified four-choice free-action pre-shot arming; the next exact firearm attack consumes the marker, an eligible hit spends one grit for persistent Dex-modifier HP bleed or two grit for persistent 1-point Strength, Dexterity, or Constitution bleed, and native per-round damage rules deliver the effect | Exact mod load and two fresh-process guarded PASS runs retained |
| 11 | Expert Loading | ADAPTED | Runtime-qualified free-action pre-shot arming; the next exact firearm attack consumes the marker, and only a Broken early-firearm misfire that would wreck/explode spends 1 grit to retain empty/Broken and suppress its burst | Exact `79731d1` mod load and two fresh-process guarded PASS runs retained |
| 11 | Lightning Reload | ADAPTED | Runtime-qualified production swift-action ability requires positive grit without spending it, atomically loads one equipped non-Wrecked firearm chamber from shared basic ammunition, preserves Broken condition, and uses a unit-local marker cleared by the next native round callback | Exact `df60f59` mod load and two fresh-process guarded PASS runs retained; free-action route remains fail-closed until Rapid Reload or alchemical cartridges exist |
| 11 | Lightning Reload with Paper Cartridges | ADAPTED | The same once-per-round, positive-grit/True Grit deed is Swift with loose ammunition and no matching Rapid Reload, Free with matching Rapid Reload, and Free with one compatible Paper Cartridge. Paper mode has no loose fallback; inline full attacks may consume this free branch at most once when normal reload is not Free. | Qualified by the 954-test matrix, dedicated guarded Lightning/full-attack scenarios, and final 0.0.74 comprehensive PASS pair. |
| equipment | Paper Cartridges | ADAPTED | One prepared powder-and-projectile bundle replaces loose powder and ball for canonical early Pistol/Musket/Blunderbuss families, reduces reload one step, and adds +1 misfire before exact-item Reliable. Native Reload right-click remains the sole auto-use control. | Final 0.0.74 comprehensive/compatibility/working-save evidence passes; Bokken publication alone is evidence-deferred while Smith, BTSL, and crafting are qualified. |
| 15 | Evasive | EXACT | Runtime-qualified level-fifteen wrapper conditionally grants project-owned clones of the exact installed Evasion, Uncanny Dodge, and Improved Uncanny Dodge components while grit is positive; exact Spend/Restore refresh preserves unrelated native facts | Exact `28b8b83` mod load and two guarded runs observed level 15, grit `4->0->1`, benefits `True,False,True`, other-unit isolation, and cleanup; Kingmaker's native CannotBeFlanked has no attacker-level comparison to adapt |
| 15 | Menacing Shot | ADAPTED | Runtime-qualified self-centered 30-foot living-creature burst; atomically spends one grit and one loaded chamber, then uses the exact native Fear action/save/descriptors with deed DC `10 + floor(level/2) + Wisdom` | Exact isolated `24a735e` PASS pair retained; `58baf84` additionally fixes observer-only native d20 determinism and two comprehensive runs reproduced DC 21/caster level 15, self inclusion, grit `4->3`, chamber `1->0`, native Frightened failure, native Shaken success, and cleanup |
| 15 | Slinger's Luck | ADAPTED | Runtime-qualified separate pre-roll arming abilities reroll the next owned saving throw for fixed 2 grit or skill check for fixed 1 grit; exact natural-d20 access retains the native second roll even when lower, consumes one marker, and isolates units | Exact `a67a930` mod load plus two fresh-launch guarded runs observed saving and skill rolls `17->10`, grit `4->2->1`, both markers consumed, other grit `4->4`, and cleanup |
| 19 | Cheat Death | EXACT | Runtime-qualified completed native-damage target handler spends all remaining grit (minimum 1) and leaves exactly 1 HP | Exact `10a4274` mod load plus two fresh-launch PASS runs observed max HP 137, grit `1->0`, final HP 1, independent zero-grit control HP -10, progression, and cleanup |
| 19 | Death's Shot | ADAPTED | Runtime-qualified free-action pre-shot marker at level 19; the next confirmed critical with an exact firearm consumes the marker and one grit, requests native Fortitude at `10 + floor(level/2) + Dexterity`, and on failure performs the exact terminal `MarkedForDeath` transition used by installed `ContextActionKillTarget`; critical immunity suppresses the deed | Exact `612105f` fresh-process PASS pair `20260802T2009410114711Z` / `20260802T2011048524145Z` proved natural-1 death marking, natural-20 survival, grit `1->0->0`, progression, isolation, and guaranteed fixture cleanup |
| 19 | Stunning Shot | ADAPTED | Runtime-qualified free-action pre-shot marker at level 19; the next owned exact firearm attack consumes it, eligible hits spend exactly 2 grit and request native Fortitude at `10 + floor(level/2) + Wisdom`, natural-1 failure applies an exact native Stunned clone for one round, natural-20 success applies nothing, and native critical immunity spends nothing | Exact `f5dc6bb` mod load plus two fresh-process PASS runs observed chamber `1->0`, native damage 3, grit `4->2->0->2`, six-second Stunned, all markers consumed, immunity, and cleanup |
| 20 | True Grit | ADAPTED | Repeated obligatory selection with 24 deed-ownership-gated choices, including Targeting Arms, Death's Shot, Focused Aim, Twin Shot Knockdown, Steady Aim, and Fast Musket. Each unit-owned selected deed costs one less grit to minimum zero; a positive cost reduced to zero still requires one current grit, selected positive-grit/no-spend deeds work at zero, variable costs reduce centrally, and Up Close and Deadly, Clipping Shot, Stranger's Fortune, and Slinger's Luck remain excluded. | Earlier exact native cost/gate evidence is retained; the expanded archetype-aware catalog is source-qualified and awaits its guarded runtime slice. |
## Human-review correction fidelity amendments - 2026-08-20

| Surface | Prior claim | Authoritative amendment | State |
|---|---|---|---|
| Acadamae Graduate | synthetic runtime-qualified | human failure reproduced as null outer slot on selected variant; exact converted-root prepared-slot rebinding now passes two fresh real-command runs | automated-qualified; prior synthetic conclusion superseded; final visible UI check human-gated |
| Focused Aim | transactional mechanics qualified | damage, one Grit spend, and kill recovery accepted; save correlation unproven | accepted/frozen mechanics; P0 serialization audit |
| Magic acquisition | 30 distinct targets qualified | structural distinctness does not prove organic pacing/lootability | reported; prior pacing conclusion superseded |
| Firearm feat icons | semantic/native-style candidate | dark badges rejected; Nodachi is exact accepted parameter template | reported; human-gated |
| Rapid Reload icon | native-style candidate | dark field rejected; pale native feat grammar required | reported; human-gated |
| Long-gun visuals | structural rig qualified | improved overall, but both yaw too far left | reported; preserve accepted work |
| Elven Branched Spear | length/axis qualified | length accepted; active direction and back carry rejected | reported; preserve length |
# Human-review correction override - 2026-08-20

Acquisition pacing amendment: the former 30-distinct-target result is superseded by a fixed map whose normalized named-area density is at most two. Runtime run `20260820T1425182231173Z-observe-rare-firearm-acquisition` qualified exact publication, uniqueness, and vendor absence; ordinary accessibility and thematic fit remain human-gated. The Last Word remains a Pistol and Watch at the World's End remains a Musket.

BTSL responsibility amendment: Honest Guy is the permanent-equipment merchant and Xelliren is the firearm-support merchant in both exact modes. This changes no item mechanics, acquisition price, stable identity, or balance rule. Run `20260820T1444126864934Z-observe-rare-firearm-acquisition` qualified exact live table ownership; merchant materialization remains human-gated.

| Contract | Status | Notes |
|---|---|---|
| Focused Aim persistence | source-qualified, release-blocking runtime gate open | Existing 1-Grit, Charisma damage, and kill recovery behavior is frozen. Compatibility repair initializes only empty native buff presentation fields. |

## 2026-08-20 firearm feat icon correction checkpoint

- Status: implemented; source qualification and guarded UI observation pending.
- Human rejection: the 0.0.88 dark circular firearm monograms and Rapid Reload medallion are superseded.
- Root boundary: the accepted Nodachi parameter appearance is produced by CustomWeaponSelectorRuntime through FeatureUIData with a null sprite plus the NO monogram; nodachi.png is item art, not the parameter template.
- Repair: retained every stable firearm choice blueprint and exact publication mapping, replaced only the six project-owned rendered assets with a deterministic reconstruction of the native selector grammar, and added a separate pale-field oxblood reload glyph.
- Source/provenance: JSON palette and monograms plus PowerShell vector/source generator; Segoe Script and Georgia system fonts are rendered but not packaged; no native pixels or proprietary fonts are included.
- Automated evidence: deterministic 64/32 contact sheet generated; focused/full/build/package/runtime gates pending.
- Human gate: compare P/M/B/Ri/Rv and Rapid Reload beside native choices at actual UI scale.
- Next action: run focused icon test, repository validator, complete suite, clean Release/package gates, then the packaged disposable firearm-dependent-feats observer.
## 2026-08-20 firearm feat icon automated qualification

- Status: automated-qualified; final aesthetic judgment remains human-gated.
- Determinism: a second tools/New-FirearmFeatIcons.ps1 run reproduced all six PNG and 64/32 contact-sheet SHA-256 values exactly.
- Repository/source: PASS.
- Complete dependency-free suite: 1,162/1,162 PASS, including firearm-feat-icons.semantic-publication.
- Clean Release/package: PASS; output validation PASS; firearm AssetBundle manifest/output validation PASS; SoundBank validation PASS; strict standalone package validation PASS.
- Guarded runtime scenario: disposable-firearm-dependent-feats, run 20260820T1505344745363Z-71ef2e5f35aa45ce9c929d0dc5369f47, 13/13 PASS.
- Runtime publication: distinct exact P/M/B/Ri/Rv sprites resolved under Weapon Focus, Greater Weapon Focus, Weapon Specialization, Greater Weapon Specialization, Improved Critical, and Rapid Reload children; the separate Rapid Reload top sprite resolved; all native top-level icons were preserved.
- Local-runtime package SHA-256: f256f59f65587d7475672eb415ed0e648cc60c7c85e4e388f60fa35021630b70.
- DLL SHA-256: c6060a14968fe0227b601fd0fe5c2c2f736241d4044b24036717576071900ecf.
- Firearm AssetBundle SHA-256: 1aa75fa1230abfb60cd5148ca90b99d604dbece7d80d98d85cb7d7c0a885a8ff.
- SoundBank SHA-256: 0e9f88c562f4f937a8941ace0f241bb31a7ed56b46fbca549c98f764392edf18.
- Runtime deployment backup: C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20260820T1505311020164Z; exact only-target restore verified.
- Human gate: inspect the five calligraphic parameter fields and Rapid Reload beside native feats at real 32/64 UI scale.