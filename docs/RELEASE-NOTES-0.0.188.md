# Tabletop Added Rules v0.0.188

This stable release lets the mod load on **Unity Mod Manager 0.31.1** and newer. Nothing in play changes: every identity
from v0.0.187 is unchanged and none is appended, so existing saves keep their references, and the summon roster and its
1,208 public choices are untouched.

## Unity Mod Manager 0.31.1 or newer

Earlier releases declared Unity Mod Manager 0.32.4 as their minimum, so 0.31.1 refused to load them. This release
declares 0.31.1. The mod is now built against the Unity Mod Manager 0.31.1 programming interface, and every part of it
the mod uses was checked to exist in 0.31.1, 0.32.4 and 0.33.0.

The same development build, with identical product code, was tested in the game on Unity Mod Manager 0.31.1, 0.32.4 and
0.33.0. It was tested alone, with Craft Magic Items 2.1.0 and Kingmaker Bugfixes 1.3.3, and, on 0.31.1 and 0.33.0, with
Call of the Wild and Favored Class. The tests covered:

- loading and the settings panel;
- a module switched off staying off after a restart;
- Gunslinger character creation and level-up;
- firearm reloads and attacks;
- casting expanded summons;
- saves reloaded in a new game session;
- upgrading a firearm with Craft Magic Items.

The 0.31.1 and 0.32.4 loaders used in testing were assembled from verified upstream files rather than taken from their
original Nexus installers. The 0.33.0 loader was a normal Nexus installation.

If your newer Unity Mod Manager already works, keep it; there is no need to downgrade. Install over the existing mod as
usual; your module settings carry over. The mod still includes no Unity Mod Manager, Harmony or game files.

## Existing saves

Nothing in this release changes, adds or removes any identity, so saves load exactly as before.

## Disclosed open items

- **Untested versions.** Unity Mod Manager 0.27.x, other 0.31.x releases, 0.32.0-0.32.3 and anything after 0.33.0 were
  not tested. Neither were the original Nexus installers for 0.31.1 and 0.32.4, or Call of the Wild and Favored Class
  on 0.32.4.
- **Not part of the test matrix.** Combat with a firearm upgraded through Craft Magic Items, and very large mod
  collections.
- **This exact build.** The in-game tests ran on a development build with the same product code. This release's DLL
  differs from it only in its version number and build stamp.
- **Earlier items.** The disclosures in the v0.0.187 notes still stand, unchanged: the hand-maintained inherited summon
  template list and the two feature-module self-checks that fail on a heavily modded installation.

Details: `docs/UMM-LOADER-COMPATIBILITY.md` in the repository.
