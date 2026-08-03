# Fourth playtest repair report

## Live baseline identity

- Starting branch/commit: `codex/third-playtest-feats-reload-grit-dodge-assets`
  at `6a0117d39695c5dd20b84f244f59d28a586b4111`; clean tree.
- Repair branch: `codex/fourth-playtest-runtime-ux-repair`.
- Baseline version: `0.0.63`.
- Baseline package SHA-256: `80010458f3ffe461f5487da93eef9ac67f799ccc39603c7b977bb360024621f0`.
- Baseline package/live/cache DLL SHA-256:
  `f4855c22d8ada1ec2a209b4f2e60edfb9f60477f2873e32adbc72e572d5ca0f9`.
- Firearm AssetBundle SHA-256:
  `5418a9bc008b80e92f10a52e653728612a892bb398aa0a9664d603b21c26d324`.
- Fresh guarded Steam mod-load PASS:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260803T2225523236661Z-mod-load-smoke`.
- Loaded identity: version `0.0.63`, commit `6a0117d...`, MVID
  `80b76899-d9b8-464b-b926-7719a3f9b50d`, and the exact cache hash above.

The fresh process proves that the fourth-playtest screenshots show integration
defects in the qualified build, not a stale canonical DLL or stale UMM cache.
Earlier bundle/audio assertions proved prefab instantiation and callback counts,
but did not prove the player-visible doll, projectile, or native UI surfaces.

## Native firearm feat menu checkpoint

Root cause: firearm `FeatureUIData` rows were appended after Kingmaker's cached
native alphabetic list, and Rifle was displayed as `Advanced Rifle`. The prior
runtime assertion checked only that five entries existed somewhere.

Repair:

- merge native and firearm rows by displayed name using the current-culture,
  case-insensitive order;
- expose the exact labels `Blunderbuss`, `Musket`, `Pistol`, `Revolver`, and
  `Rifle`;
- retain one native top-level feat, native icons, hidden legacy wrappers,
  parameter-specific effects, and native prerequisite shapes;
- strengthen the guarded runtime assertion to inspect the full order and exact
  firearm subsequence in Weapon Focus and all four dependent native families.

Evidence:

- repository validation: PASS;
- dependency-free domain/reflection suite: 878/878 PASS;
- clean Release compile and strict standalone package validation: PASS;
- checkpoint package SHA-256:
  `496ce8acc36e04fbce055411ca204f9f1c79d1ee7a5d82b086a93c8f4d32bb09`;
- checkpoint DLL SHA-256:
  `c5789a755f324780f806c431fab87fe2b7430c98328f868e401f0f2f556243cc`;
- guarded save-free runtime PASS:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260803T2230567143233Z-disposable-firearm-dependent-feats`.

This mechanically qualifies the live native menu data and ordering. Final Phase
12 still requires a player-visible UI observation after the full `0.0.64`
repair package is coherent.

## Remaining mission scope

Rapid Reload and semantic icon art, visible finite Grit UI, real equipped firearm
models/projectile/audio presentation, native reload auto-use, condition and
maintenance UX, Winchester attribution, comprehensive regression/runtime runs,
and final player-visible acceptance remain incomplete.
