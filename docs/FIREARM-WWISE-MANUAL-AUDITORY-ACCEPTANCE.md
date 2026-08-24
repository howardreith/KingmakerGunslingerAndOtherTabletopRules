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

## 2026-08-24 restoration listening gate

The repaired production manifest boundary and focused audio routing passed on
commit `ea51bd3732fd7313e92bcc2edac9560008f6c9ac`. The five global previews
returned Pistol/Musket/Blunderbuss/Revolver/Rifle playing IDs 2/3/4/5/6 and a
live-unit Blunderbuss preview returned 7. These are technical acceptance IDs,
not evidence of audible output. Record the exact final installed commit,
package hash, audio device, observer, and result for every item below.

1. [ ] Fire one ordinary Pistol shot.
2. [ ] Fire one ordinary Musket shot.
3. [ ] Fire one ordinary Blunderbuss shot.
4. [ ] Fire one ordinary Revolver shot.
5. [ ] Fire one ordinary Rifle shot.
6. [ ] Confirm each produces exactly one clearly audible and correctly
   differentiated firearm report.
7. [ ] Confirm a normal miss still produces exactly one report.
8. [ ] Confirm a true misfire does not produce the ordinary shot report.
9. [ ] Confirm an Empty or Wrecked firearm does not produce the ordinary
   report.
10. [ ] Confirm Scatter produces one Blunderbuss report per volley, not per
    target.
11. [ ] Listen for and record any inherited crossbow release/twang or duplicate
    report.
12. [ ] Confirm Master and SFX/effects volume controls affect the firearm
    report.

## 2026-08-24 repository-owner result

The repository owner listened to the exact installed implementation at commit
`d9a51132a39369d6393b7fe90b7a6ffc3ee243bf`, package SHA-256
`DFBEDB0CB3CF7ADDB38E5D794D49555B7CEF141F17139922DC8A08F65B163A51`,
and reported: "Sound effect sounds working to me." The owner then explicitly
approved commit, merge, and creation of a new release.

Status: owner auditory release gate accepted. The response did not separately
enumerate every checklist line, identify the audio device, or provide an
independent mixer-control result, so those individual observations remain
unrecorded rather than inferred. Automated playing IDs were not used as the
audible result.
