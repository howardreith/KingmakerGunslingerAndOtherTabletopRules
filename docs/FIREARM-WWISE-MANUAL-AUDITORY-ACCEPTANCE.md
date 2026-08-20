# Firearm Wwise manual auditory acceptance

Run only after automated bank/runtime qualification. Record the exact commit,
bank hash, game version, audio device, and observer.

- [ ] All five firearm kinds are audible and mapped to the intended recording.
- [ ] Master and SFX/effects sliders control the reports.
- [ ] Pause, camera distance, and spatial position behave sensibly.
- [ ] Rapid reports overlap without unwanted cutoff.
- [ ] Scatter produces one report per volley.
- [ ] Misfires produce no normal firearm report.
- [ ] No inherited crossbow release/twang layers over a firearm report.
- [ ] Blunderbuss baked-in reverb is acceptable.
- [ ] Equip, unequip, and inventory sounds remain intact.

## 2026-08-06 observed results before auditory-polish fixes

- [x] Pistol custom report is correct and sounds good.
- [x] Musket custom report is correct and sounds excellent.
- [x] Blunderbuss custom report exists and maps to the shotgun recording.
- [ ] Blunderbuss timing failed: the approved processed source's blast began
  around 2.20 seconds. A deterministic 2.180-second trim is now implemented
  and requires fresh listening.
- [ ] Blunderbuss misfire exposed an inherited crossbow release. Firearm
  presentation now severs prototype fallback after resolving protected fields
  and clears only the release/whoosh Event; requires fresh listening.
- [x] Scatter posts the custom Blunderbuss report.
- [ ] Scatter also played borrowed Burning Hands audio. Its cone geometry now
  uses the firearm projectile rather than the spell projectile; requires fresh
  listening and visual confirmation.

A nonzero Wwise playing ID is not evidence that any checklist item was heard.

## 2026-08-20 integrated regression status

Fresh human playtest evidence reports Pistol and possibly other firearm sounds absent. Treat every checked historical listening item above as stale for the current integrated candidate. Automated run `20260820T0635323959656Z-88cfa04a0deb4595bfbc2ee8d4284e31` proves the exact bank loaded and all five Events returned nonzero playing IDs; it does not resolve the audible failure. Repeat the complete checklist on the final overnight candidate and record the exact commit, package, bank hash, audio device, and observed weapon/action for any silent or layered result.
