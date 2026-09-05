# Elemental Races 0.0.117 replacement-framework qualification

## Scope and outcome

Incremental framework qualification PASS; Release C remains IN PROGRESS.
All 21 alternate-trait mechanics and the final release-wide gates are still
required. This report does not turn historical 0.0.114/115/116 results into
0.0.117 evidence.

Authoritative starting master:
`6874dc15a27ded132456dbdd480f47c794543a05`.
Branch: `codex/elemental-races-expansion`.
Both candidate builds below embed preceding commit
`e2986654246bf0081ba6965ae5fc90318635d059` and identify the exact uncommitted
source by fingerprint. The eventual framework commit does not retroactively
become their embedded commit.

## Exact artifacts and local gates

Both candidates use assembly/package version `0.0.117`, informational
version `0.0.117-elemental-traits`, and the local-only package filename
`KingmakerGunslinger-0.0.117-local-runtime.zip`. Neither is published or
committed.

| Candidate | Focused framework/compatibility | Final persistence fixture |
| --- | --- | --- |
| Package bytes / entries | 23,168,117 / 135 | 23,169,506 / 135 |
| Package SHA-256 | `a43038698d95dfbbeaa5293c4d2548ee3ded0fa4b6916710bdbd765a555b7917` | `f99ac332e0396ea7d537a2cad943060b5ece62365fa3acee55187e83de66d480` |
| DLL bytes | 6,020,608 | 6,024,192 |
| DLL SHA-256 | `2c693905d8f35b490540002400ea5b4a4f994ace1d64ea41dcc98bee981796f5` | `97568a921aaa9bbcb0a88256eaa6e1caf4c6f507ad9ac87548ef8a7b0d7e48aa` |
| DLL MVID | `a5bced63-dfc2-480e-a128-ce146c31e6ca` | `aa93be71-da6e-4ff9-adf4-e137c0290625` |
| Source-state SHA-256 | `b81efb52a5e8aaab2519fa9cc4646811b07445f0a1a081d250281fd27fe54a25` | `bc75a7eb42945390728168a2234f00d9d334e2c2ebaed6ba5f339a3672663b23` |
| Deployment ID | `20260905T2212482258255Z` | `20260905T2246116296072Z` |

Repository validation, complete 1,413/1,413 domain/reflection tests, clean
exact-installed-reference Release build, supply-icon validation, and strict
standalone package validation passed. The later candidate changes only the
persistence fixture and checkpoint journal after the focused ten-process
run; production framework mechanics are unchanged. Runtime preflight passed
202/202 twice after one unexplained artifact-snapshot mismatch; no assertion
was relaxed. The mismatch remains historical failed evidence, not a diagnosed
production defect.

Manifest: 1,846 total, 1,844 active, two reserved. Elemental Races: 209 total,
208 active, one reserved; 180 blueprints plus 28 visual-resource proxies.
The unconditional package-core blueprint count is 1,784; the optional
Brown-Fur identities and visual resources are counted separately.

Exactly 62 appended identities occupy
`e117e1e0a17a4acec001000000000001` through
`e117e1e0a17a4acec001000000000062`, using decimal-string suffixes:
ten selections, ten retain markers, 21 trait markers, and 21 hidden providers.
The symbol-to-GUID authority is the append-only
[manifest](../blueprints/blueprints.json).

## Behavior established

The pure replacement policy exhausts all legal subsets and illegal overlaps:
Ifrit 21, Oread 16, Sylph 28, Undine four, totaling 69 sets and 207
heritage-combination rows. Live native-unit reconciliation executes 414
forward/reverse application-order rows, all 21 marker-first cases, resource
removal and re-addition, live Hydraulic Push prerequisite outcomes,
idempotence, and exact cleanup of 33 disposable units. Each complete
framework run passes 4,333 assertions, including its 38 graph assertions.
Foreign facts survive; consumed providers disappear; changing a heritage
while its SLA slot is consumed cannot reintroduce an SLA.

A failing live marker-first test exposed a late inherited General SLA
resource orphan. Exact native resource removal now retires only that inactive
project-owned resource and preserves its previously remembered spent amount.
The regression injects the orphan and proves that restoring the previously
spent heritage yields zero uses. No Unity object destruction or global
fact-reconciliation patch was introduced.

The ten focused guarded processes pass 17,483 assertions with zero
runtime-result warnings. They cover base-race mechanics (27), native
identity/movement (17), heritage mechanics (68), the full framework matrix,
and elemental publication in KMG-only and highest-risk combined profiles,
including module ON/OFF.

## Compatibility restoration

All four transactions report `Restored` and
`restorationVerified=true`. Their identical 968-entry original mod-folder
manifest hashes to
`861094eb2ac176143ab2792a22b2d6907592751e224ad4fb23db2577be174d62`
using SHA-256 of UTF-8 PowerShell
`ConvertTo-Json -Depth 6 -Compress` serialization of `originalManifest`.

| Profile / state | Transaction ID | Transaction JSON SHA-256 |
| --- | --- | --- |
| KMG-only ON | `compat-20260905T221311Z-1813ab6be7ac` | `fb5d85d0546104c6a9cd12234338a3fd3423a129406bcd093b67b03352e33d4d` |
| KMG-only OFF | `compat-20260905T221857Z-dd81c8953cc2` | `65c70e08275398dd592472bf05fc64c17baa9029179df4999fcc7cc88225f0ad` |
| Highest-risk combined ON | `compat-20260905T222140Z-6b6b1e9d0e41` | `bbe0a56f9275d1f338420eb83a76a72277ee937ca15f9c1058cae91cbce7a63f` |
| Highest-risk combined OFF | `compat-20260905T222554Z-04c56c4a3058` | `21076f62d39cdba99ddb4427af775df848be1e11fd1d91a1ace9f7ed7bcad5d2` |

Original/restored FeatureModules.json SHA-256:
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
Call of the Wild settings:
`24cc3f80269992a53ebbfd1f5986e5aab056841d6b2f43d8e22e764cdb73f6e8`.
Combined-profile Favored Class staged settings before/after:
`bdceed77d2bf4a31dd9e4eeb64ef9d55a42ef59d23f46abcb1ddbcc6ef66754b`.
No FCB feature, resource, selection, or behavior was added.

## Four-process persistence

The final candidate passes 43/43 assertions across prepare (13), module-OFF
load/level/rest/re-spend (12), module-ON respec/cleanup (11), and independent
fresh-process absence (seven). All launches used the guarded request through
Steam App ID 640820 on the approved Windows 10 environment.

Only `KMG_AUTOMATION_WORKING` was loaded or written. The protected baseline
was excluded. The 24 fixtures cover four races, both sexes, and all twelve
heritages; this framework transaction deliberately retains every base trait.
The native LevelUpController executed 180 retain-choice selections in 72
creation/respec records. All 144 persisted/current retain-state observations
are exact. Existing heritage stats, providers, spent SLAs, Release B feat
facts, buffs, item enchantments, doll data, rest, and level-up checks remain
active. Cleanup removes all fixtures and retained feat items. A fourth
process loads the same exact working save, proves the original three-character
party and zero fixtures, and performs zero save writes.

The final working file `Manual_299_KMG_AUTOMATION_WORKING.zks` is
3,566,381 bytes, SHA-256
`262f8eb83008b81908c7625a997f7788a884188317f7b7c135b3261f2a8ebe02`.
Settings restore to the original hash above; temporary ON/OFF settings were
`333e5a3cdb7196ac6c58c9959ad24c81b2b0c98a06804b94e601743685d0fa8e`
and
`fd8420daa53de98aeb7b81538ab4eff4ae8a0eaeadc54342ee53581913de0195`.

Persistence emits 14/15/15/two warnings respectively: 13 low-foreground
optional-image framing diagnostics per rendered phase, native class-clothes
reconstruction guidance, and subjective image-review guidance in later
phases. These are recorded, not silently promoted to visual PASS.
Mechanical proof comes from structured native state and command assertions,
not screenshots.

The first prepare attempt failed before creating fixtures or writing a save
because the inherited explicit registration inventory omitted the 62 new
identities. The feature-specific fixture now enumerates those identities,
selects retain markers through the native command path, and verifies them
through load/level/rest/respec. It does not force markers onto legacy 0.0.114
characters.

## Exact runtime evidence ledger

All hashes below are SHA-256. Raw evidence remains machine-local beneath the
documented runtime-evidence root; only this curated ledger is committed.

### Focused framework and compatibility

- `20260905T2213324958502Z-3692ce79e6b846ff929ddda6e38a0f33` — `disposable-elemental-race-mechanics`; PASS, 27 assertions, 0 warnings.
  - `elemental-race-mechanics.json`: `6b2019ae3a9b7eb57fb617bae90ce8349f9686f49939fe620a2b56dfbae13968`
  - `runtime-evidence.json`: `3c7bea63aebd66386cb8c0b1e337fafa8ae8a65e86e7036bbc11854fb67d7a00`
  - `runtime-result.json`: `3d2e631cff161e05cbdf66fc687ffa238959dcec1864263553ec6438601a7cbb`
- `20260905T2214429243031Z-06c1bdaac0a54b0996d077708699f6a3` — `disposable-elemental-race-native-identity`; PASS, 17 assertions, 0 warnings.
  - `elemental-race-native-identity.json`: `76c462f30fa3d98d6e774a724b4704b4bff740db87e93b01ae859f108926612b`
  - `runtime-evidence.json`: `2692c3a30c35f06faa1d5fc65c2c8786665aa27c70275c850d7f611e687d9fe9`
  - `runtime-result.json`: `132b12f06e43dd2868d7c7c3c76e51c0298166bbabebacbfe98a9d4887a5309b`
- `20260905T2215516747021Z-7f0f79a5d5374ccb8d4420e29f6a1b52` — `disposable-elemental-heritage-mechanics`; PASS, 68 assertions, 0 warnings.
  - `elemental-heritage-mechanics.json`: `56936c6e173b7ed68ae37e6d2e94c3cc13bd6b2d533b9e9e26f1240fd8da5eeb`
  - `runtime-evidence.json`: `aee775ac7dbcb131680c428dfafa5c200015f540bfd207f5c6fc9b09f772cdc7`
  - `runtime-result.json`: `adcb6680c13c904894c33e05be8cfe0e63ab75b3ae340520c2e6b3ec203ae6ac`
- `20260905T2217033596005Z-17bb47a9157a44da91f10c83f1882bd5` — `observe-elemental-alternate-trait-framework`; PASS, 4333 assertions, 0 warnings.
  - `elemental-alternate-trait-framework.json`: `115e1318137a84099dae058798051f61f884ab63571ced3a04a647dccf65f018`
  - `elemental-alternate-trait-reconciliation.json`: `a96504beb3e1ae9c638b762c4560d78e0a5b0314b8cc9f7c42dfddc37425be82`
  - `runtime-evidence.json`: `dd8be625d889080f49dfc47baae0442cd7b87c73a797dd5774276f0d30a84d19`
  - `runtime-result.json`: `7b19bfccd394a6f187e903f82c5feab02d5c1070417a0cd0e48df2985e935f89`
- `20260905T2219176361212Z-77032c285d664f99954aa3a7dec7a746` — `elemental-races-races-unleashed-compatibility`; PASS, 13 assertions, 0 warnings.
  - `elemental-races-races-unleashed-compatibility.json`: `693dbf90acefa01afb75373e0c9a454582beadba292742a653f65bd46f19807b`
  - `runtime-evidence.json`: `450c37ed4cd803ea5bda62111c60b53326ff5b5780f19cc69392e60735a22eee`
  - `runtime-result.json`: `6694a8d3d286c16b8b164377cc489aad89cafcb99656c428f394781754f20079`
- `20260905T2220265423856Z-371e85c523684fd9abc7bbdc690bee18` — `observe-elemental-alternate-trait-framework`; PASS, 4333 assertions, 0 warnings.
  - `elemental-alternate-trait-framework.json`: `00cf6d27c5b71be57e1745deb921caa4f6eb3bc74e9f383802673cafb4b0e17c`
  - `elemental-alternate-trait-reconciliation.json`: `a96504beb3e1ae9c638b762c4560d78e0a5b0314b8cc9f7c42dfddc37425be82`
  - `runtime-evidence.json`: `c2f6c0815e65a4ca3eb7d080a721d8129a2a570562b1dc4a9b09a21574f9e71c`
  - `runtime-result.json`: `a32c4ced886062e515d84a16ff49c5572e0bd9b731bd8f5b56c86924b32980ff`
- `20260905T2222162418330Z-b7f32e38ac334366ad6d50dff3361b46` — `elemental-races-races-unleashed-compatibility`; PASS, 13 assertions, 0 warnings.
  - `elemental-races-races-unleashed-compatibility.json`: `cb8723f30af60084aefa5dfac5121fbf5e2f665d4ca5d05500c6cd0897f777ce`
  - `runtime-evidence.json`: `158e4cfde0c6ded8def70e4d5b7745f3ff13ae86ad90bef9df0262c2d0ae21a3`
  - `runtime-result.json`: `2f09ba6dd2fad6bef5955469aeba857fd40c07a4674370ce0b4931e5904f7ac7`
- `20260905T2224039359273Z-d19b0bc0e6864bed959638da52655acd` — `observe-elemental-alternate-trait-framework`; PASS, 4333 assertions, 0 warnings.
  - `elemental-alternate-trait-framework.json`: `115e1318137a84099dae058798051f61f884ab63571ced3a04a647dccf65f018`
  - `elemental-alternate-trait-reconciliation.json`: `a96504beb3e1ae9c638b762c4560d78e0a5b0314b8cc9f7c42dfddc37425be82`
  - `runtime-evidence.json`: `6f7ee7f5ac345e3e4739f740a609f2f5fd32b05fefc6a5e34334c63704ce30a3`
  - `runtime-result.json`: `65aad4f5a67e9656ad67e4f6b575bb01c4c150d691326a8cefef01eaeecb6dbd`
- `20260905T2226223995572Z-7d163b8144d14908801524a6a932abf4` — `elemental-races-races-unleashed-compatibility`; PASS, 13 assertions, 0 warnings.
  - `elemental-races-races-unleashed-compatibility.json`: `630170155d8271dbd131e02aabe4677d7bd7cd4c99f131f4834ac8db6178f555`
  - `runtime-evidence.json`: `3385259627fc86b61c273b61068cfd0539415afe3247246fae3debda3abfa9a8`
  - `runtime-result.json`: `202362b9b039f0756ba84a2d4a7ec75dc24633fdc094c8a5d5b9e35dc7f5aadf`
- `20260905T2228076038208Z-6fab37057c7647b0a5fb9253fd545030` — `observe-elemental-alternate-trait-framework`; PASS, 4333 assertions, 0 warnings.
  - `elemental-alternate-trait-framework.json`: `00cf6d27c5b71be57e1745deb921caa4f6eb3bc74e9f383802673cafb4b0e17c`
  - `elemental-alternate-trait-reconciliation.json`: `a96504beb3e1ae9c638b762c4560d78e0a5b0314b8cc9f7c42dfddc37425be82`
  - `runtime-evidence.json`: `5290ffd36e5cb92b81e2c426a9f6dc5c9b5e28281e509e4fa35dbe10860ccc4a`
  - `runtime-result.json`: `9a543c66812440777827ffbc0a147a8be4c4776f2bc7f13a172c0ead76f1b9f0`

### Persistence and fresh-process absence

- `20260905T2246251031426Z-974cc6136d7a4ef5bb70f8526153a7d3` — `elemental-race-persistence-prepare`; PASS, 13 assertions, 14 warnings.
  - `elemental-race-persistence-index.json`: `5885ffc8388da5173cee9b0007c1c6833ec5cf30704cc83e33ae8531a5a8140e`
  - `runtime-evidence.json`: `b818875615894eb446b4042e4f8ef2190e2cab63f9f5a6a3b46036814a93b9ba`
  - `runtime-result.json`: `0f2d5536d85252416cf9a0d45982bda530d29b19b1ed953b754e4ceb1ee880cf`
- `20260905T2250020131649Z-7dfc23ac42c5499cad8e9548a9fcbd5b` — `elemental-race-module-disabled-persistence`; PASS, 12 assertions, 15 warnings.
  - `elemental-race-persistence-index.json`: `90d1cfef22b331bf201b6e6682b458d5162c345445be804a0c3dd526ca0d22fc`
  - `runtime-evidence.json`: `7cabb759e6dbeeb1eadb9d64f42f39b896a6b0050ac7c8c512f0901b95a36637`
  - `runtime-result.json`: `b288adab082fa070e0f7c4505f002e3664dc48ef8aa14f0b8d964dd98a122df2`
- `20260905T2253086654947Z-ff83b8c53ca147279279f2060a35514f` — `elemental-race-module-restored-persistence`; PASS, 11 assertions, 15 warnings.
  - `elemental-race-persistence-index.json`: `d571a4e13b27f4a06d0d4ace72b76e1bde70383b373e6839237bde96f7d8e581`
  - `runtime-evidence.json`: `57d81b6496fdb549550e3f4dfe531911456fbbe96972150a91018554f9297cf1`
  - `runtime-result.json`: `c6221ef35288684336bfd4b7a225eb83e375d482af75150fc7f18516f901af32`
- `20260905T2257250886765Z-a694484b621140259b9d44b3bc926d48` — `elemental-race-persistence-verify-absent`; PASS, 7 assertions, 2 warnings.
  - `elemental-race-persistence-index.json`: `9dcd666f507280ffdfcd51ff5148df579c837141ded14756e4e099781e217200`
  - `runtime-evidence.json`: `8ee7bf680173eacb4b210ed3bf0060e1e68076200bd8e1b62bf394facf3696ee`
  - `runtime-result.json`: `6bee1df9f8ea2c063f09deadb3de469bb3ac08e101fd507da4349df52f00894d`

## Failed evidence and remaining gates

Earlier failures and their narrow corrections remain in the mission journal.
Key runtime-result hashes are:

- Late inherited-resource orphan:
  `ddcf9e88017abfc81307efcf931bd542925edc16e20262b40fa0bdd13a12ab13`.
- Old default-race mechanics fixture:
  `1c6943107efc93abb0b83a620fb4b508448233e1744aab9074bc481f7efe2f36`.
- Already-applied racial modifiers measured against an incorrect fixture baseline:
  `31308bb327c7d103ed6fdbe2abdb073cdd613bb51853fe8e58cf7987b6b1e9ce`.
- Incomplete persistence registration inventory:
  `6910e8565b870eb501c5cb14e2dc48187ec96555c6be1fde35486e93a81b8bfc`.

NOT-RUN for this framework checkpoint: actual trait mechanics, actual
trait-bearing multi-combination persistence and resource capacities, direct
0.0.114-to-0.0.117 migration, the final six-profile release-wide rerun, and the
current generic 24-configuration module boundary. Visual Adjustments is absent
and remains NOT-RUN. Subjective image review is not established by these
mechanical runs. These limits prevent Release C PASS, not the independently
qualified framework checkpoint.

The external required push wrapper still refuses the exact mission branch.
No wrapper bypass, merge, tag, public release, committed ZIP, raw runtime
artifact, save, or proprietary assembly is part of this checkpoint.
