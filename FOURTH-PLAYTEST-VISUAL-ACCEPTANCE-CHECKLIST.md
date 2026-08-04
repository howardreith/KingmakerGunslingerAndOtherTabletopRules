# Fourth playtest consolidated visual acceptance

This is the single remaining supervised perception pass for the functionally
qualified `0.0.64` candidate. Do not use screenshots, OCR, mouse coordinates,
or visual navigation as mechanical runtime proof. A human must initiate normal
UI interactions. Use only `KMG_AUTOMATION_WORKING`; never select or overwrite
`KMG_AUTOMATION_BASELINE`.

## Launch

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario working-save-smoke `
  -ExpectedVersion 0.0.64 `
  -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$false `
  -Confirm:$false
```

After the guarded PASS leaves Kingmaker running, perform one consolidated
read-only/playtest review. Stop on wrong version, wrong save, DLC Required,
Steam dialogs, ambiguity, or any unexpected save prompt.

## Checklist

- [ ] UMM shows `0.0.64` without a red/broken indicator.
- [ ] Native Weapon Focus has one top-level entry and its submenu visibly adds
  Blunderbuss, Musket, Pistol, Revolver, and Rifle alphabetically.
- [ ] Greater Weapon Focus, Weapon Specialization, Greater Weapon
  Specialization, and Improved Critical retain native icons and visibly enforce
  matching firearm and ordinary level/BAB prerequisites.
- [ ] Rapid Reload has one top-level entry, five firearm choices, and its muted
  oxblood/gold ramrod icon reads clearly at game scale.
- [ ] The progression uses visibly distinct icons for Proficiencies,
  Gunsmithing, Grit, Deeds, Nimble, Gun Training, True Grit, Quick Clear,
  Reload, Repair, and Overhaul; Bonus Combat Feat retains its native icon.
- [ ] Every paid Grit deed shows the same remaining native number; spending and
  recovery refresh it immediately, zero disables paid deeds, and nonspending
  abilities have no counter.
- [ ] Right-clicking the single Reload Firearm ability enables native auto-use;
  an empty attack reloads the exact equipped firearm and follows the documented
  RTwP/turn-based continuation policy.
- [ ] Pistol, Musket, Blunderbuss, Revolver, and Rifle each display their mapped
  firearm mesh in hand at acceptable socket, scale, orientation, and
  handedness; Rifle alone uses the Winchester lever-action model.
- [ ] No crossbow, bolt, arrow, or quiver mesh remains visible beneath any
  firearm.
- [ ] A successful live shot produces one appropriate firearm report and no
  layered crossbow-bolt report; empty rejection is silent; misfire/explosion
  remain distinct.
- [ ] Inventory tooltips have no placeholder/crossbow-visual wording and no
  `<null>, <null>` qualities; qualities and `Condition: Normal/Broken/Wrecked`
  are readable and refresh after transitions.
- [ ] Normal misfire becomes Broken; Quick Clear is available only while Broken
  with Grit and visibly returns Normal; a later qualifying Broken misfire becomes
  Wrecked; Wrecked blocks fire/reload.
- [ ] Overhaul is disabled in combat and reads as one uninterrupted minute of
  out-of-combat maintenance.
- [ ] Existing class alphabetical placement, progression layout, starting
  Pistol/20 powder/20 balls, and one Reload icon remain intact.

Record PASS/FAIL and concise notes for every box in one session. Screenshots may
be retained only as optional supporting evidence.
