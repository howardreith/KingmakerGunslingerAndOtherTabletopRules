# Z Weekend Mission Handoff — Traits, Content Cleanup, and Research (2026-10-03)

**Mission:** Z_Weekend_Gunslinger_Mission_2026-10-03 (finite weekend mission;
this file is the evidence ledger and final handoff it requires).
**Repository / origin:** `howardreith/KingmakerGunslingerAndOtherTabletopRules`
(`https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules.git`).
**Starting `origin/master`:** `2ce70e4e7e9d3c97ca1008ab05a341e758f5cf5a`
(HEAD equaled `origin/master` at branch creation; fetch during preflight moved
no master commits — new remote branches/tag `v0.0.141` appeared only).
**Mission branch:** `codex/z-weekend-traits-content-2026-10-03`
(created fresh; no collision).
**Clean-tree evidence:** `git status --porcelain` at start showed only the
untracked workspace-local `.zcodeignore`; tracked tree clean.
**Mod identity at start:** `Info.json` Version `0.0.140`, manifest source
artifact, UMM `KingmakerGunslinger.dll`, entry `KingmakerGunslinger.Main.Load`.

## Discovered validation commands (current docs)

- Domain suite: `.\scripts\test-domain.ps1 -Configuration Release`
- Clean build + strict package: `.\scripts\build.ps1 -Configuration Release -Clean -Package`
- Icon catalog: `python tools/validate_icon_catalog.py`, `python tools/test_icon_catalog.py`
- Push wrapper: `powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1`

## Slice matrix (living status — final results in the closing sections)

| Slice | Status |
|---|---|
| 0 Preflight/ledger | IN-PROGRESS |
| 1 Nodachi Heirloom Weapon | SKIPPED-ALREADY-COMPLETE (evidence below) |
| 2 Firearm description normalization | NOT-STARTED |
| 3 Fiery Glare (Ifrit) | IN-PROGRESS (investigation) |
| 4 Stoic Dignity (Oread) | IN-PROGRESS (investigation) |
| 5 Earthsense (Oread) | IN-PROGRESS (investigation) |
| 6 Whiteout/weather research | NOT-STARTED |
| 7 Item/vendor availability research | NOT-STARTED |

## Slice 1 — SKIPPED-ALREADY-COMPLETE evidence

Nodachi already exists as an Heirloom Weapon choice on the qualified baseline:

- `src/KingmakerGunslinger/Blueprints/HeirloomNodachiBlueprints.cs` (in tree
  since commit `431bef614`, "fix(compat): defer Nodachi martial publication",
  an ancestor of HEAD; shipped in `0.0.93-eastern-favored-compatibility` per
  `CHANGELOG.md`): save-stable five-identity set reproducing Favored Class
  1.3.1's three-way Heirloom Weapon structure (proficiency / +1 AOO trait
  bonus / +2 CMB trait bonus) for the KMG-owned martial Nodachi category,
  with `PrerequisiteNoFeature` duplicate prevention.
- Publication is late-bound, atomic, idempotent, conflict-rejecting:
  `ElementalWeaponLatePublicationCoordinator` + `HelpfulPublicationTransaction`
  append exactly once into the installed Favored Class Equipment Traits
  selection; exact-count validation; martial classification is asserted
  before registration (`EasternWeaponMartialPublicationPolicy.NodachiCategoryValue`)
  — Nodachi is martial, no exotic is added, no global category behavior changes.
- Tests: `tests/KingmakerGunslinger.DomainTests/EasternFavoredCompatibilityTests.cs`
  (`HeirloomNodachiIdentityAndMechanicsAreExact`,
  `HeirloomForeignPublicationIsAtomic`, `HeirloomPartialFailureRestoresEveryArray`,
  plus identity-count/blueprint-manifest assertions; 19 heirloom references).
- The final integrated domain run of this mission re-verifies those tests
  against the current tree.

## Key investigation findings (2026-10-03)

Recorded so the blocked slices' evidence is durable; full findings land in
`docs/research/` with each slice.

- **Fiery Glare (mechanics):** Kingmaker represents Intimidate separately from
  Persuasion as `StatType.CheckIntimidate` (0x67; dialog checks and combat
  Demoralize both use it). The engine has a native take-10 fact component,
  `Kingmaker.Designers.Mechanics.Facts.Take10ForSuccess` (fields `Skill`,
  `MagicDeviceType`), an `IInitiatorRulebookHandler<RuleSkillCheck>` that sets
  `RuleSkillCheck.Take10ForSuccess`; `RuleSkillCheck.OnTrigger` then uses a d20
  of exactly 10 when `10 + StatValue + Bonus >= DC`, else rolls normally.
  Optionality can be preserved with the repository's established no-action
  toggle pattern (`BlueprintActivatableAbility` off-by-default, immediately,
  free — `BodyguardModeBlueprints.cs`). Offering mechanism: Favored Class
  `racial_traits` late publication (Heirloom Weapon/Helpful precedent) gated by
  native `PrerequisiteRace` on KMG's Ifrit `BlueprintRace`.
- **Stoic Dignity (mechanics):** fully expressible with established patterns —
  `AddAreaEffect` implements `OnFactActivate`/`OnFactDeactivate` (works on
  features, verified in game IL), `BlueprintAbilityAreaEffect` Cylinder
  `10.Feets()` with ally-only delivery (`AbilityAreaEffectBuff` +
  `ContextConditionIsAlly`, Magic Circle precedent), descriptor-walk
  mind-affecting detection (`SpellDescriptor.MindAffecting` 0x10; pattern in
  `ElementalAlternateTraitSaveBonus`), `UnitState.IsConscious` for the
  consciousness gate, `ModifierDescriptor.Trait`/`Morale` for correct stacking.
- **Earthsense (engine):** no tremorsense exists in Kingmaker — zero matches
  for Tremor* in the full Assembly-CSharp IL; the sense model is
  `UnitCondition` flags (Invisible/SeeInvisibility/TrueSeeing/Blindness) plus
  `UnitStealth`. The mission forbids blindsight/blindsense/see-invisibility
  substitutes. BLOCKED.
- **Icons (Slices 3/4):** no authorized image-generation tool exists in this
  session (the prior trait icons' briefs record `"tool": "built-in image_gen"`,
  a tool this session does not have; no local generator installed; procedural
  substitution and donor art are both forbidden by `docs/ICON-ART-GUIDE.md`).
  Mission §8 slices 3/4/5 and §9 require committing only when feature AND icon
  qualify; therefore those slices end as recorded blockers without feature
  commits, with complete implementation designs in the research findings.

## Slice 2 — Firearm item description normalization (PASS)

### Before/after inventory

All firearm item descriptions compose at registration: base sentence (base
items) and/or property sentence (magic/named/progression items), then the
shared penetration sentence (`FirearmPenetrationPresentation.Describe`).
Localization keys and names are unchanged; only description text changed.

| Surface | Previous | New |
|---|---|---|
| Penetration (all firearm items) | "Penetration: Touch AC within the first range increment ({X} ft. base); Normal AC beyond." | "Attacks with this firearm are resolved against touch AC out to {X} ft. (its first range increment), and against normal AC at greater distances." |
| Penetration (advanced Rifle/Revolver) | "…first five range increments ({X} ft. base)…" | "…out to {X} ft. (its first five range increments)…" |
| Penetration (Blunderbuss) | "…beyond. This applies to ordinary direct fire; Scatter Shot retains its separate cone rules." | Scoped to lead ball: "Attacks with a lead ball are resolved against touch AC out to {X} ft. (its first range increment)…" (the Scatter Shot cross-reference is gone; the base sentence already separates ball and pellet attacks) |
| Base items (Pistol/Musket/Rifle/Revolver) | "Uses black powder and lead balls. It can misfire and must be reloaded." | "This firearm fires lead balls driven by black powder. It can misfire, and it must be reloaded to fire again." |
| Base item (Blunderbuss) | "Uses black powder and lead balls. It can fire a lead ball or use Scatter Shot to fire pellets in a 15-foot cone. It can misfire and must be reloaded." | "This firearm fires lead balls driven by black powder, or pellets in a 15-foot cone with Scatter Shot. It can misfire, and it must be reloaded to fire again." |
| Reliable property (Duelist's Rebuttal, The River King's Measure, Irovetti's Ovation, The Last Word, Watch at the World's End, Roadwarden, 15 Better-Vendors reliable variants) | "Reliable reduces this firearm's misfire value by 1 after other increases, to a minimum of 0. A natural 1 still misses." | "This weapon's misfire value is 1 lower than it would otherwise be, to a minimum of 0. A natural 1 still misses." |
| Seeking property (The Last Word, Dead Reckoning) | "Seeking ignores concealment miss chances. It does not reveal unseen creatures, allow targeting a creature you could not otherwise target, or bypass other defenses." (The Last Word's compressed variant) | "This weapon's attacks ignore the miss chance from concealment. It grants no ability to see or target creatures the wielder could not otherwise target, and other defenses still apply." |

Flavor texts were preserved unchanged (already suitable). The sentences now
live in one shared source (`Firearms/FirearmEnchantmentItemText.cs`) used by
the authored items, merchant catalog, and vendor-progression items. Mechanical
accuracy: penetration windows derive from
`FirearmPenetrationRangePolicy.EffectivePenetrationRangeFeet` exactly as
before; the Reliable sentence still states the after-increases/minimum-0
semantics proven by `BetterVendorsProgressionTests` (misfire 2 + broken +4 +
cartridge +1 − 1 = 6); Scatter Shot is a native no-attack-roll cone so
touch-AC text correctly scopes to lead-ball attacks only.

No mechanic/design discrepancies were found: every previous sentence was
mechanically accurate but written in implementation voice; the "({X} ft.
base)" qualifier is dropped per native item-description style (native entries
state base values without modifier qualifiers).

Adjacent surfaces inspected and deliberately unchanged: ammunition item text
(not firearms), firearm condition/qualities tooltip stat lines (UI stat-line
style), enchantment display names, combat-log AC messages. Non-English
localization: the mod registers runtime localization for its own keys only;
no non-English firearm entries exist to become stale.

### Tests and gates

- New `firearm-descriptions.*` domain cases (4): penetration text for all five
  families incl. scatter scoping and parenthesis balance; enchantment sentence
  truthfulness; every item description resolves/composes; source scan proving
  the prohibited internal phrases ("ft. base", "separate cone rules",
  "ordinary direct fire", "after other increases", "Penetration: Touch AC
  within", "Uses black powder and lead balls") no longer appear in any
  description-bearing source.
- Updated: `ac.penetration-presentation` (Program.cs) and
  `midgame-firearms.truthful-properties` to the new copy.
- Deterministic test count pin updated 1918 → 1922
  (`tools/validate_favored_class140.py` +
  `validation/static-validation.json` `favoredClassIntegration140` block —
  the current-release block tracks the live tree, per 0.0.140 merge precedent
  1854 → 1918).
- Gates: `scripts/validate-repository.ps1` PASS;
  `scripts/test-domain.ps1 -Configuration Release` 1922/1922 PASS;
  `scripts/build.ps1 -Configuration Release -Clean -Package` clean Release +
  strict standalone package validation PASS;
  `git diff --check` clean.

## Commit / push ledger

(appended as slices complete)

