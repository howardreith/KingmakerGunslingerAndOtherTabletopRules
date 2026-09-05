# Elemental Races 0.0.116 qualification

Status: **LOCAL PASS** on branch `codex/elemental-races-expansion`.
The required checkpoint push is blocked by the external branch allowlist.
Nothing was merged, tagged, or publicly released.

## Qualified artifact

- Source commit: `c4b9f8fbc40a21ecc9775deab66a40b2ec9b24f3`
- Source-state SHA-256:
  `f64ecc1f280ce7ef4a8dd74228d27e0e669c85e5a5fa05be579fce564e042dde`
- Package:
  `artifacts/local-runtime/0.0.116/KingmakerGunslinger-0.0.116-local-runtime.zip`
- Package size / entries: 23,145,172 bytes / 135
- Package SHA-256:
  `e5b8f77e77fe9d6bf56c43a2371304b631b8fd65e410c7a931abe27adf8ba032`
- DLL size: 5,958,144 bytes
- DLL SHA-256:
  `3c9af3692f2f4dd58ceeb0a54cd607f410c32e43f186824d17c6bc3d80f528d4`
- DLL MVID: `8c8af472-93b4-4d83-a944-0f466be5457a`
- Deployment manifest:
  `20260905T1447593309541Z/deployment.json`
- Deployment-manifest SHA-256:
  `83aaa400ae89aa4ccd7fa2730142b936a7b948cdeb6f6bb05250120bde829ba2`

Repository validation, all 1,408/1,408 deterministic domain/reflection cases,
the clean exact-reference Release build, production firearm manifest and
SoundBank checks, and strict standalone package validation all passed.

## Final integrated mechanics regression

One isolated KMG-only transaction reran all five Release B mechanics scenarios
against the exact artifact above. All five processes and 73/73 assertions
passed with zero runtime-result warnings and save-free exact cleanup.

| Scenario | Assertions | Runtime run ID | Runtime-result SHA-256 | Mechanics SHA-256 | Runtime-evidence SHA-256 |
| --- | ---: | --- | --- | --- | --- |
| Shared Elemental Strike / Wings | 16 | `20260905T1606404056913Z-9169bea234b0448c945f52b36c7c2e10` | `125653dc2789691e4522c78f15ecd9be7280ff63156ef44e9f9c2dde3738fbe5` | `aef0cf2db12fb5a324f64beb9acabebec9fe09e879ea6dc3c29ae7b7a5482845` | `3817460e96af13753cb4992ec0a5f4d1cc4fd52d7be0d6385e37a3fe1e5e4788` |
| Scorching Weapons / Inner Flame | 12 | `20260905T1608008549767Z-ac6c72358ae249328eccaac726f07489` | `ba333fb23f9f714a9127be62fe6f50186df557e0f75fca8d85f0c2901858382d` | `be70c9b853d1fc2975ee9cd5e9f940da892ee42599d0312430400bf8ee7bb451` | `a067b81477f9a4a68418ee7e3ffa5abd65dd2a9362ab5624558d9eeb36b6eb1d` |
| Blazing Aura / Firesight | 14 | `20260905T1609096841169Z-61e56c4eec694a8b94d92a975f95e6a6` | `a588223519207b17bcfc186d2f3cfd0e68afbacfdaf3a3c4b5717b135f233bd8` | `4e679ae26a5eb48cf783b65206a5090db7cb18c8745d7e7308b50bb409f4a8f3` | `98b3c96fa2b0a162fdc9710d03342ab4c6f28a0a34e46b7f836671e4bb406fa7` |
| Sylph feats | 18 | `20260905T1610179428979Z-1a744355bc1c4078921b5c87d2af455a` | `d77803a64aa1e8d0a5be83ffc14688ba2ecf9711106ca4c37edd1fe83709eccc` | `9780e37b9da8104912f98bdf9521c4168c575892ef94a3e1d46a778dbe1ac475` | `2b687bdd6c4ce3fda303a7eb2ecbbcd1272688396dac5acc54ab7484f9cba05a` |
| Undine feats | 13 | `20260905T1611267732821Z-41486d361bc2409ab076ade14a43fbe9` | `d8ccc7d167aadd77fd4b6b7370c4026b1c9df74ecb39a4dd6024d2579d170372` | `6797701ef2d806136807661c70130075bb5eb06270dcaa29eb98815d861114c4` | `50ea390730fb2cc3ef27dd0a21a4224b63d49fc49805409554db875e3b9473dc` |

Transaction `compat-20260905T160603Z-4d73fae7b1d2` restored exactly.
Its transaction SHA-256 is
`4842490cf6339fded043e09515a0da8cef0742059396de16989dae2b5ffb1f32`.

## Compatibility matrix

The matrix used the same immutable artifact. It ran 31/31 guarded Steam
processes and 359/359 assertions with zero runtime-result warnings. Every ON
profile checked mod loading, its relevant optional-mod contract, exact
Elemental race/feat selector publication, and native feat contracts. Every OFF
profile proved complete project identity registration with zero Elemental race
or feat publication.

The “evidence” hash below is the scenario-specific evidence file where one
exists and otherwise the common `runtime-evidence.json`.

| Profile / module | Scenario | Assertions | Runtime run ID | Runtime-result SHA-256 | Evidence SHA-256 |
| --- | --- | ---: | --- | --- | --- |
| KMG / ON | mod load | 3 | `20260905T1449082307030Z-e89a69f8bb0841aca598c2ac51c38e16` | `d8bf389a35ea6df3fd16ecb65808cc2daf6d6ee8eb9c578919e768b70bf541dd` | `73bc0b95634047cb74089c8716887aeafefec21c58995a6dba683cd6cbb680e9` |
| KMG / ON | optional contracts | 16 | `20260905T1450175128989Z-81543a70ea024aaba63bf4418b32679e` | `5de090465163bcac0c6ad2e199916eff8a47e02665a76771f029e249dbabc8ab` | `b13355817e3b1bcf999152cfd36753aeca36c6b4cf189f167c48af887702a82a` |
| KMG / ON | race/feat publication | 13 | `20260905T1451250798275Z-3925b537c35f4293ba69a2084678733d` | `4e515858ccb263bab86862a5668b1b18871872b87ffad50e8f58cc66c4bff052` | `f8a407328082f25f4c1a9fb9425bdf81c831e2ade8f6c0e758f2fbf71b0d5d17` |
| KMG / ON | native feat contracts | 10 | `20260905T1452319708446Z-a14f1672cf40415cb48d152e535fcafc` | `b61bd574ebf1b1552cb4d1be279e221e7a91989e8152abaf873cd1c4c37ca9f9` | `655ed075381b64df3b73399f45b02c4abcf4ed1e57ac91b1ffd544ce2564076f` |
| KMG / OFF | identity / no publication | 13 | `20260905T1454433677992Z-23b4ffb734104aacaf05fc715d13cb9f` | `57684cbc2bb7eb6fb26b05c86ec1be885025098ee5f66f2ccaef20337b038d25` | `693dbf90acefa01afb75373e0c9a454582beadba292742a653f65bd46f19807b` |
| Call of the Wild / ON | mod load | 3 | `20260905T1457034520594Z-1734c44d07a8467b9b21aaf89787ddf0` | `b69ab4f1eb4921fefb8ace38ba9efc2a532a08a6d3dbcc891ebe559781b4e839` | `b44537d96c97af81108cec6d46696a4d9c8eee11de2c86d2fdd4fd1f990a717d` |
| Call of the Wild / ON | optional contracts | 23 | `20260905T1458476154834Z-1ce18fbc61dc462ca384916265c83aeb` | `cb9207442f0d5d0492057125e87304d5dbf1030b0d5e73b84bf7d734a5cd0e4a` | `932ee751c8ca13bf85aed1f78a9e147128a08093ce6fb4c7a40d104d570714be` |
| Call of the Wild / ON | race/feat publication | 13 | `20260905T1500282616680Z-83bfe1c18e414f6caab3ee561aecf639` | `f1cc17674667fe7a83c791df4bfa99253379fd96f24d966ad605041e596ac02f` | `57a200f308321b8365ee6898dedb5de968f73df7c151a54b337d3fe3d80d374e` |
| Call of the Wild / ON | native feat contracts | 10 | `20260905T1502085058001Z-cbab8f5be89d4ffc94d08fc90a2d75e0` | `1ba3e3beb330cfba429c608f3e37d46cb16c0af885cd604f84355355f078c283` | `cd25ad828d96ab30d06f2b3ec5fc6c6852bf4cbb8a6376429179d5e6dd0014e2` |
| Call of the Wild / OFF | identity / no publication | 13 | `20260905T1505051542927Z-4d01e6dd7f154c478551994456342aed` | `e2f0cdae276370e693f837ee8da1732317d0fc6f123ea0b5e1db18a7c2593bd7` | `867237c75e67b8af26a75e75538f2b92ba2ea1dfbe8d3967c5430f3a6ae354f7` |
| Races Unleashed / ON | mod load | 3 | `20260905T1508016999385Z-b58069cda9964e20ae98874607a708ec` | `3b1c1d96ef34a6edde71134be042d6798f424d26b84c5a257c171e22a5bcff1d` | `8970f6ee8d9d1e208ec1e890ad348971cd339713c0014deec94c9c5039bba6e3` |
| Races Unleashed / ON | optional contracts | 16 | `20260905T1509111237296Z-b95a6c94f21a431086b3c64842f83ced` | `c9a569c984b6f6a74e24eed069c582152119a63096ff40a6e2f57ce8e2d80b83` | `06c879a88beed858ccb4c1ab6f0ff7c813b56e8057e3f48488093005509bab81` |
| Races Unleashed / ON | race/feat publication | 13 | `20260905T1510210393710Z-576aea221a1f484683913f91253ff738` | `53c174f64fe606cacad55af39405141b5cc26a32464e577e516502b8abb70374` | `fe7409fd8de3cdf73eb61ded624d385684636f1c2cbf299029b44d448850e6b5` |
| Races Unleashed / ON | native feat contracts | 10 | `20260905T1511298088081Z-ef040ff2431a4f68803aceef756181a7` | `67e05fee25806ee08b796180f18bade113bc04e5926623545d920f3100843ef4` | `04fdeb069c7440beedfefc8a3737be915d54f2cb9e70fe97c69496d445b39bfd` |
| Races Unleashed / OFF | identity / no publication | 13 | `20260905T1513419346598Z-d612bcbea3c649518fada4080c6fcea6` | `073304d7b12697bdfad10a056c0d38fbdbae2cd0645be3897e41f43a92e8afed` | `344c98902767982ad30efe9370ed13d38d3bfc2277e211a141e6a50bf60ee21a` |
| Favored Class / ON | mod load | 3 | `20260905T1516007751149Z-34fca77fbbe74c67be00930e00efd783` | `076a2db7d67332107f8334306d12f442c90386f3357d4704ebf3a77d1a280774` | `c926559e417bedad772063922b4113f883aed4d408350d7af0e359312d883e90` |
| Favored Class / ON | Aid Another interop | 12 | `20260905T1517460419794Z-49dc975bb55b4134ba45fd9f78af4c57` | `ce5697199b6d7e9f09a17074e1170ac9f2222ce169f6f431143c76dad370b5dd` | `83f9af57c49a75d0b7aaefb76195b11645c7b8cfea8a2471d434f49c5dd4b56f` |
| Favored Class / ON | race/feat publication | 13 | `20260905T1519275471678Z-684ab7e4c2824694999bf2e305f07138` | `f0459802c4078553145ffee6cc784fd8a1e18b4ae943bc41211b6392c26dc49c` | `284a3020558c54575662732912e1ab6527c318cbd59ca83f1e314c5c25809cb5` |
| Favored Class / ON | native feat contracts | 10 | `20260905T1521087154212Z-662fb4afd5c54d35b8bd45fd35b20e93` | `1a51499331b70c7192b117e8314d46423f5d063cbdc48e27564075a849040cf1` | `34a8bdc9f0095f385fe18d734ada95c76c03b5acd30ab3174aba6759f84b8f8c` |
| Favored Class / OFF | identity / no publication | 13 | `20260905T1523586497014Z-63fe19d12356426b94712d8fb0e13bec` | `de51014a142d460122148bcfd550de2c671b9d05c600439180ac72b6db4bf9b9` | `584bdcfa75a48a7c51e1eef0846d0c3a9a22f4f256766ef7190e1d88299891df` |
| Tweak or Treat / ON | mod load | 3 | `20260905T1527079869858Z-645050a497f6427e86d20c2e9d5dfec5` | `74619218f8da5455ba417de34edfa0310dfe2bd68780b65e6890ec296784bdb6` | `15527cdaf21e94a57b9fe82e9ae05b613043b99788785ff21f9a5c928cad99dd` |
| Tweak or Treat / ON | optional contracts | 23 | `20260905T1528532072736Z-75ad3a9b834b4dad8b5ff11ce265b112` | `14f1b6460ef1d7c56f5af5fdf88f56fe4a9334473feb4e03a1d2819c5ee16955` | `df5e0d7a0db65d0dd8ad95133e42a2abccb6cc95c5e0add6379dcd8dff95ced2` |
| Tweak or Treat / ON | race/feat publication | 13 | `20260905T1530352786663Z-cb77f6899ecb456d86cdc94702504127` | `bd9ccd1ed5ef25041c08d367c14100fef2e666ff67abd3f0c5eb50a8c4ef48a1` | `7081c9c658e056a283a790544b5ac73270b4daf54459ea03f0a37461a331de65` |
| Tweak or Treat / ON | native feat contracts | 10 | `20260905T1532164801884Z-48483ed6a39c4364b2a24f0fabc1332e` | `bc28eb1338b0bd896a939dd67365dd5d406999a398b7d7f066d199998d03b433` | `460ea52b9d345b80f49d455336c5827cda8dd4afaeb9643ce67992ecdfdee5c7` |
| Tweak or Treat / OFF | identity / no publication | 13 | `20260905T1535119207449Z-216dc1e3a702463d8c7e564c149b0def` | `16f79b0ae61a52b5e974cbf2acada4bdaa2fbf28e5315ea9016a261a7859623a` | `05e7ec4d2cc873d8ae1e3c10f0c89dc17bcb524df269592f8068ead1c7e9320f` |
| High risk / ON | mod load | 3 | `20260905T1538085903619Z-d976bab72016462db9ae1c359f6e011f` | `5f5fa627b47963956e161589c8f7eefebed694a4aee4798ed6279e1dffee4f7c` | `8043d467906da03c60769ecb6475370464a0ebda579fcd1980ec55b1a6d6070c` |
| High risk / ON | optional contracts | 23 | `20260905T1539540505766Z-582f15d8b1a9462cb1c62c53fdb5bd8d` | `ed7e4791bb07b5889ea0b3a2d04b65e58d55267405d150ea583aecdd992591ab` | `b688a718cb014c865790b05ee44a7328c7e718fdbdca3b6eb55b1d25249bf996` |
| High risk / ON | Aid Another interop | 12 | `20260905T1541376860742Z-11e9ece2520b4346936dacf5ae33fd23` | `3b3c7b0b02072d61ea71385f092ccdf5403e1e0d76a23eec84fe8cc839464c26` | `1517949bf53c459be45ee8e44f0915aca5ccf2725593a4ecfe06e513f0207347` |
| High risk / ON | race/feat publication | 13 | `20260905T1543205945574Z-5ca36e2172ec434ab171ba464c59c621` | `007e9f1eb21ae16887190c9cf08325c23380935cd50a56ac94eaf8374e835ea7` | `cb8723f30af60084aefa5dfac5121fbf5e2f665d4ca5d05500c6cd0897f777ce` |
| High risk / ON | native feat contracts | 10 | `20260905T1545154919164Z-7baa056299d6479d8cea5d2d2344c4be` | `07e0cd965e90d4408b6bebf4051e1851b37f73899599b7aa6e7f9dfe67d407ee` | `344dca3cf09d1825aa815f92b72b210480dbc9083b9b2b9b55f94e8ddb820377` |
| High risk / OFF | identity / no publication | 13 | `20260905T1548149858593Z-6859679955b2477897e6c046ad26bebd` | `5f00746767583f1ab199db60deb9461514e49e46a425463873c5ddc890a5f5a5` | `630170155d8271dbd131e02aabe4677d7bd7cd4c99f131f4834ac8db6178f555` |

All 12 ON/OFF transactions restored the same 968-entry original mod-tree
manifest exactly. The compact canonical manifest SHA-256 is
`cc8d634150133b0eed87bcb5933cbe432ad2f1c5153c182e9ce44c8e3099893e`.

| Profile / module | Transaction ID | Transaction SHA-256 |
| --- | --- | --- |
| KMG / ON | `compat-20260905T144848Z-e598a1e854e7` | `81e98e2e36f74e43179a9ca9478385ddf69373ac5d135678d788ee148b2924fd` |
| KMG / OFF | `compat-20260905T145424Z-6e1e5014bbe1` | `923a0ceb6d5bc1fcf2b5c8559edf2028ab4f0beb010bebc7091918ec036571a5` |
| Call of the Wild / ON | `compat-20260905T145637Z-9d3e45623d6a` | `47b1d5a5341d4481c7dcd3a46a511d1cd9f8540fba0dd44a7ed2eb50bd5ebb16` |
| Call of the Wild / OFF | `compat-20260905T150441Z-e6f0519d2fda` | `22c9f3be81a2d772a24c54b746122dc5eaa21401bfb92c6b81f5a3efe7620619` |
| Races Unleashed / ON | `compat-20260905T150735Z-84abe9964897` | `8876b039f9017818518ebbc263f447def181db772c4d159ff63b5e3c94fa8bd5` |
| Races Unleashed / OFF | `compat-20260905T151319Z-5decdb5e77bd` | `0fcb4f96468f47e62323c8c2a2ab9627520263ab1a377bf90044465a387c03b7` |
| Favored Class / ON | `compat-20260905T151535Z-60172c160e04` | `f8991341310921002b80ebf092f48e4f55be874cea7b0bf2b89114e94fb031da` |
| Favored Class / OFF | `compat-20260905T152334Z-1c7094d6cd1b` | `2d92a30c148315ca39d762f3835223edd36d1eaffd64064bc00aafa89d928a90` |
| Tweak or Treat / ON | `compat-20260905T152637Z-87825d77afe6` | `9c83de411d2e930895dd0f1b99b46a8ec5ada7e289e96aa390eb3e9523e05bb2` |
| Tweak or Treat / OFF | `compat-20260905T153443Z-c274856d98ef` | `2f121301c9cc539314d336328da04777f0e35d04300371d967c35443aab1e3f8` |
| High risk / ON | `compat-20260905T153738Z-f79791a23ce0` | `4ad3274b8345b5f00e944ad7d5bcb2d48b323831a3f81583aa05f376d413e350` |
| High risk / OFF | `compat-20260905T154743Z-ab8bc903c8be` | `31785535e05707246edeaa7471049bbb35e0b396ffc5d3c40ba3abeb4d8db4d6` |

`FeatureModules.json` restored byte-for-byte after every transaction to
SHA-256
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
Call of the Wild settings restored to
`24cc3f80269992a53ebbfd1f5986e5aab056841d6b2f43d8e22e764cdb73f6e8`;
Favored Class settings remained
`bdceed77d2bf4a31dd9e4eeb64ef9d55a42ef59d23f46abcb1ddbcc6ef66754b`.
No favored-class bonus or publication behavior was added. Visual Adjustments
was not installed and is **NOT-RUN**, not PASS.

## Persistence and migration

The Release B 24-fixture prepare/module-OFF/module-ON transaction and final
fresh-process absence pass are recorded in
`ELEMENTAL-RACES-EXPANSION-STATE.json`. They preserve feat facts, granted
abilities, resources, Wings, an active Elemental Strike buff, and Scorching
Weapons on two exact item references without restoring uses or retargeting
equipment. The named disposable save is absent after cleanup.

Release A's exact 0.0.114 migration remains the authoritative migration proof:
the Release B addition does not change any 0.0.114 race, General provider, SLA,
resource, or visual identity.

## Limitations

- Visual Adjustments: **NOT-RUN**, because it was absent.
- Dirty Trick (dazzle): omitted because Kingmaker exposes no genuine native
  player-facing path; Dirty Trick (blind) is implemented.
- Falling damage, grapple, unrestricted three-dimensional flight, and
  non-native aquatic summon options retain their documented engine
  adaptations or no-ops.
- The branch push is blocked by external policy, not by a source, package, or
  runtime failure.
