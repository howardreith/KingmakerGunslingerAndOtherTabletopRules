# Acadamae Playtest Repair Qualification

Status: PASS AND LOCALLY DEPLOYED

Baseline: `c80615e917d1994daad679e8a78af11ae2c7e115`, version 0.0.75, 967 deterministic tests, 250 active blueprints.

## Deterministic and package gates

- Repository validation PASS for 0.0.76.
- Complete dependency-free/domain/reflection suite PASS 970/970.
- Clean exact-reference Release build, blueprint/asset/provenance/SoundBank audits, deterministic package creation, and strict 44-file UMM package validation PASS.
- Active identity count 252 and ledger count 253; existing Acadamae and Cord GUIDs unchanged.

## Guarded runtime evidence

- Standalone Acadamae lifecycle PASS 13/13: evidence `20260810T0025495339458Z-disposable-acadamae-graduate`, runtime `20260810T0025495652087Z-cf68ca7dc54f46888ed1b19f4af2a381`.
- Cord mechanics/icon PASS 8/8: evidence `20260810T0028061943603Z-disposable-cord-of-stubborn-resolve`, runtime `20260810T0028062099814Z-4d661eeab3234d6fbeb2f5a4a4f31559`.
- Later standalone integrated PASS 13/13: evidence `20260810T0057193245457Z-disposable-acadamae-graduate`, runtime `20260810T0057193557972Z-ea604c31ea864f74b5208bbd04bbf682`.
- Four-combination matrix PASS with 252 identities: ON/ON `20260810T0030378293295Z-92d7638006ad407591e69653ac1de6ed`; ON/OFF `20260810T0032442695172Z-b3aba843a05c4110a1ee80124c931edf`; OFF/ON `20260810T0034494700421Z-e5234d444a854e3d9382d24a97f87755`; OFF/OFF `20260810T0036544950657Z-453c089bf845435facf86f18be3bbcb0`. Original settings SHA-256 `8aa8233b19e69af001d28dc9db51748baf3abb9ffff37ce96754c4addfac7470` restored.
- Working-save smoke PASS 11/11 twice using only `KMG_AUTOMATION_WORKING`: `20260810T0059425523070Z-8d4ce89f4e124fd8a5528427ff6b6075` and `20260810T0102171809010Z-518fa9cbfd2d4b49ab8c4b4903ec40ae`.

The Acadamae scenario explicitly proves mode OFF native Full-Round with no
save, mode ON Standard with one save, command snapshot behavior, canonical
permanent fatigue, root/distinct context, survival after spell-context cleanup,
actual native rest removal, attached-view no-FX lifecycle, and the integrated
Cord one-d6/no-fatigue chain.

## Exact compatibility transactions

- Call of the Wild: `compat-20260810T003846Z-8ef116748a84` PASS and restored.
- Arms & Armor: `compat-20260810T004325Z-48b05aca6317` PASS and restored.
- Toggle Custom Soundpacks: `compat-20260810T004510Z-57bdf436e6d3` PASS and restored.
- Qualified combined: `compat-20260810T004700Z-01cf993fd775` PASS and restored.
- High-risk combined consecutive runs: `compat-20260810T004845Z-b479c645228a` and `compat-20260810T005115Z-8d43f465f597`, both PASS and restored.

Historical broad Call of the Wild selector limitations remain historical; this
qualification proves the targeted mode/action/fatigue/publication composition
and does not inflate that claim.

## Final release-source and deployment proof

- Release-code commit `eab12bdbef962398fa7ab9d6fb6b7eace67bea76` standalone PASS twice: `20260810T0132091376104Z-40e05d48a6824f789c845f33b71d37b5` and `20260810T0134251364252Z-7b4eaacad5934c56936705b5e51653d8`.
- Same commit high-risk combined PASS twice with exact restoration: `compat-20260810T013613Z-4e3cfaddf7a0` / runtime `20260810T0136505786359Z-e1eedfb8ac8340728f6efa8cd3381556`, and `compat-20260810T013843Z-b93fe0ebb174` / runtime `20260810T0139202505538Z-2735a4a634ee4abbaa29c878c637e1fa`.
- Installed package SHA-256 `86bcd1dc738223b2eb4e10c99f57731d2006ffe54f2445b9c6b1ad06df3ec92f`; installed/built DLL `3c9b20738d848e70e08e92f66a7a7a009a44fbb208c9208facf951d4d3b4ef7e`.
- Backup `C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260810T0141375127164Z`; deployment manifest `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260810T0141392337217Z\deployment.json`.
- Installed Cord icon hash matches production: `cf3f040eb22691b1e526eb32cc31d1151eafef7113cb0ebe55d0c2637d5d9928`. Feature settings remained byte-exact at SHA-256 `8aa8233b19e69af001d28dc9db51748baf3abb9ffff37ce96754c4addfac7470`. Kingmaker process count after deployment: zero.
