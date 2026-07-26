# Kingmaker Gunslinger 0.0.19 smoke test

This build is for a disposable campaign only. It fixes the Sprint 18 Firearm Proficiency development control and replaces the disproved `ItemEntityWeapon.UniqueId` persistence candidate with item-owned state-token enchantments.

## What the Sprint 18 test established

- Unity Mod Manager loaded the DLL.
- Harmony and the blueprint lifecycle ran.
- All eight expected custom blueprints registered exactly once.
- Kingmaker 2.1.7b does **not** expose the assumed inherited `ItemEntityWeapon.UniqueId` member.

The old identity-vault persistence path is therefore disabled in this build.

## Install

1. Close Pathfinder: Kingmaker.
2. Install `KingmakerGunslinger-0.0.19-token-smoke-test.zip` through Unity Mod Manager. It may replace version 0.0.18.
3. Confirm that Unity Mod Manager shows version `0.0.19`, enabled and active.
4. Use a disposable campaign and disposable saves.

## Test A — proficiency and equipment

1. Load a disposable campaign.
2. Select exactly one party member.
3. Open the Kingmaker Gunslinger UMM options panel.
4. Click **Grant Firearm Proficiency to selected unit**.
5. Read **Last result** near the top of the panel. It must begin with `SUCCESS` and report a verified rank of at least 1.
6. Click **Add one Test Musket to shared inventory**.
7. The item still uses the Heavy Crossbow name, icon, model and animation as a placeholder. This is expected in 0.0.19.
8. Equip that newly added item.
9. Click **Print selected unit's equipped-firearm state diagnostics**. The result should report `firearmProficiency=True` and one equipped firearm.

A `FAILED` result is useful evidence. Capture the full result and the `[KMG]` entries from `UnityModManager.log.txt`.

## Test B — item-token state creation

1. Unequip Test Muskets so four copies can remain in shared inventory.
2. Click **Create/normalize A-D item-token persistence fixture**.
3. The result must begin with `SUCCESS` and describe:
   - A: Loaded / Normal, one token.
   - B: Empty / Broken, one token.
   - C: Loaded / Broken, one token.
   - D: Empty / Normal, no token.
4. Click **Print visible firearm states** and save the displayed result or the corresponding `[KMG][development]` log line.

## Test C — save, full restart and reload

1. Save to a new disposable slot.
2. Exit Kingmaker completely to the desktop.
3. Start Kingmaker again and load the disposable save.
4. Open the mod panel.
5. Click **Print visible firearm states**.
6. The visible Test Muskets must still include exactly:
   - one Loaded / Normal;
   - one Empty / Broken;
   - one Loaded / Broken;
   - one Empty / Normal.

Repository entry numbers may change after restart. The item-owned states and token multiplicities are what matter.

## Useful evidence to return

- A screenshot showing the `Last result` after the proficiency grant.
- A screenshot or copied log line showing the A-D fixture result before saving.
- The corresponding visible-state result after a full process restart.
- `UnityModManager.log.txt` entries containing `[KMG]` if any step fails.
- `output_log.txt` only if the game throws or crashes.

Do not send the private Kingmaker reference bundle again; the exact reference hashes are already recorded for this build.
