# Elemental Races 0.0.116 qualification

Status: **LOCAL PASS** on branch `codex/elemental-races-expansion`.
The required checkpoint push is blocked by the external branch allowlist.
Nothing was merged, tagged, or publicly released.

## Qualified artifact

- Source commit: `d8ad16f7c10670ed5b200214738322195213c392`
- Source-state SHA-256:
  `13fa83a7c0b2bd1314f78fa68739f1c4c7fdb908252b79bad085c7569687c682`
- Proposed release package:
  `artifacts/packages/KingmakerGunslinger-0.0.116-elemental-feats.zip`
- Guarded runtime package (byte-identical):
  `artifacts/local-runtime/0.0.116/KingmakerGunslinger-0.0.116-local-runtime.zip`
- Package size / entries: 23,145,631 bytes / 135
- Package SHA-256:
  `3efab34b80cfcd1d2e9fabb4cf1a33375dbff6e6c53406b6298d3590aa5c22f4`
- DLL size: 5,958,144 bytes
- DLL SHA-256:
  `063ac40ea33e4e01b197e05ea9a44ca724448e4ce21e6e727778fa2448423da3`
- DLL MVID: `adf3a0e2-c09b-4290-9137-cc5b49226eba`
- Deployment manifest:
  `20260905T1705407969279Z/deployment.json`
- Deployment-manifest SHA-256:
  `ba96a746511927bc17cb0bdedae5a7fce0da28cbae4d53d2b80cfe10d4393b5e`

Repository validation, all 1,408/1,408 deterministic domain/reflection cases,
the clean exact-reference Release build, production firearm manifest and
SoundBank checks, and strict standalone package validation all passed.

## Final integrated mechanics regression

One isolated KMG-only transaction reran all five Release B mechanics scenarios
against the exact artifact above. All five processes and 73/73 assertions
passed with zero runtime-result warnings and save-free exact cleanup.

| Scenario | Assertions | Runtime run ID | Runtime-result SHA-256 | Mechanics SHA-256 | Runtime-evidence SHA-256 |
| --- | ---: | --- | --- | --- | --- |
| Shared Elemental Strike / Wings | 16 | `20260905T1707251239808Z-f9db7c4fdd8941a7be473af69d8de047` | `aa613a9b00044fb27b150cb23113a44d50eb9cb74e443a9ac5b336c7d318fe76` | `aef0cf2db12fb5a324f64beb9acabebec9fe09e879ea6dc3c29ae7b7a5482845` | `623dfc93621b1e6732584f90a0b10deac26871e9a8e5ab946e53bf440714c018` |
| Scorching Weapons / Inner Flame | 12 | `20260905T1708373222055Z-36a14f78c62147efac35cb223d7de70a` | `85a070ed89383d8c338fbe8c2c8b50d1bb0bb5cdf3799c2207d9b62e99a7970f` | `0b6bffed286270909cfe296f1b504936f4a0e27c2ae151729fa5397cb8a3fca8` | `3f47d97fa8d7d9825321ddb43b0ac4c4109ea4e491a0c44ba9e6c16c684a7acb` |
| Blazing Aura / Firesight | 14 | `20260905T1709466213275Z-7d9a1ffe449d4719b6a428473f1cf3b4` | `b94cc6d84dc32173b34cf75b7fef55cb99608cbcd402adaf8f3692e567378584` | `adb7df76d1edfe463c40b16b8b2df415506eb3051a4b3fe585867339ecaa21bb` | `aaec1151b879cecebf23c9e7e486886ce7d83b4376c701b5956108e4b87b43f5` |
| Sylph feats | 18 | `20260905T1710550559739Z-a89cff54d5d8429f9c574f80546591d4` | `ddc3f5ddb0d1e59c7ece7d83cf9712e0cdd41edff5fa89b73b633660019bf6a4` | `9780e37b9da8104912f98bdf9521c4168c575892ef94a3e1d46a778dbe1ac475` | `b87be530d95886f5da0da955251c03c9ce11dc8b71d2d211278eab7a22bf6129` |
| Undine feats | 13 | `20260905T1712038695533Z-c26a3ebdb0194c769cdaf78b2e9e51f1` | `9aa7ffb4283f2a253066e96af5353cd0be00035cf42cf37ae5552dfaaacfed0f` | `6797701ef2d806136807661c70130075bb5eb06270dcaa29eb98815d861114c4` | `625fe5b9a5e358f92751084e09c8e075399d22f713934fe614c196eae1fc2e71` |

Transaction `compat-20260905T170705Z-db94dda1ab19` restored exactly.
Its transaction SHA-256 is
`6c2fb7c962d93cdd2e4c2e8f230199538d5913b8eece9d5889dad1f83d8a08f9`.

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
| KMG / ON | mod load | 3 | `20260905T1716056386880Z-6e0455908fe24a8781d5a9044f0cc1ee` | `c1c01e1bf0c5dbe255bd0b9ef33da9914d9108f16811a6a3e29f92e63df143b4` | `2b9a8acaa1882d0aa4ffed5204ce19c8821af1cb8ff7895498e624e8f711d5d9` |
| KMG / ON | optional contracts | 16 | `20260905T1717136957530Z-b4e602c962504e99b28052cedaf4d2e0` | `f149f5fa19518874b576cc318ef9ecbb5bbe89f79344fcbefd0e691367d9cec7` | `939ad81c3338e19bb00c1d2d63f70edfbd2fee20ee525f058893649fec5debc7` |
| KMG / ON | race/feat publication | 13 | `20260905T1718211136674Z-00fe825217194e4dbf78050413e0fd00` | `79b08902e21b6000f39faaa6254c8e73ddf9cff40f337f7beb10677d5687805e` | `f8a407328082f25f4c1a9fb9425bdf81c831e2ade8f6c0e758f2fbf71b0d5d17` |
| KMG / ON | native feat contracts | 10 | `20260905T1719279129790Z-2a81a5daedb6442eafc1b1eb456f7901` | `3a56ec674c8ecfd4c21201b22687581de3e7622742936c86ffe49c464e142fcd` | `655ed075381b64df3b73399f45b02c4abcf4ed1e57ac91b1ffd544ce2564076f` |
| KMG / OFF | identity / no publication | 13 | `20260905T1720572082592Z-8a04f48551744defa24851a97f85c65a` | `013e6e625d451dc6feb16bb8c75ce70b7e5b28f3be2fc18715f9757bf467c5d0` | `693dbf90acefa01afb75373e0c9a454582beadba292742a653f65bd46f19807b` |
| Call of the Wild / ON | mod load | 3 | `20260905T1722306630327Z-e07f97021524477486e621b7bd0e5781` | `edf24da3a68b05710620ea63a987e03d868188f40d369da2f35eabce8fdbb18a` | `0a291ae1fe2226d2f39280ab0b40319cccd1667de61c9ddb4925aa56dcc34b25` |
| Call of the Wild / ON | optional contracts | 23 | `20260905T1724135638434Z-1a5fb71f13df458bbcc9440c57c3ab77` | `271c6b54ee711797b6d70c2009ce6fcedb7603b63869d42f3ea780ebe7588dd0` | `f8934785919b779f24dc7c356d10bf2066690b5d49498f55e110ec5b22f17a5c` |
| Call of the Wild / ON | race/feat publication | 13 | `20260905T1725535641349Z-589138ff99bc44a1a62ffabb6be5fc0e` | `ea67150023239d44a97ae7a299f09e3622f6e2eb58afd25471a69ab8665912d6` | `57a200f308321b8365ee6898dedb5de968f73df7c151a54b337d3fe3d80d374e` |
| Call of the Wild / ON | native feat contracts | 10 | `20260905T1727333354196Z-563244f8d2204daa8090adfbe69c047f` | `905e8bef4caa699f624dbe9945d6d32cbc7a13673a3ac6bfc79182e768d5aef4` | `cd25ad828d96ab30d06f2b3ec5fc6c6852bf4cbb8a6376429179d5e6dd0014e2` |
| Call of the Wild / OFF | identity / no publication | 13 | `20260905T1729426484319Z-b62706433c8f40968d157c0c884cb77f` | `adea159ba58ce38c454938c46a86203732ce78c811790728dc3723e31a16b63b` | `867237c75e67b8af26a75e75538f2b92ba2ea1dfbe8d3967c5430f3a6ae354f7` |
| Races Unleashed / ON | mod load | 3 | `20260905T1731492670841Z-2f077705ebe144f8bb06e45bcf2bcbef` | `a39a2e921d9444bad57d6ac9c72f8634898eab45ae8a0c1684e3e8be260a06f6` | `0e309fd17f8f2447df14242f41de15103d8d631ed479193428effccb1db6ae2e` |
| Races Unleashed / ON | optional contracts | 16 | `20260905T1732572085409Z-4c7fe2e53311432cadd509a4d669779e` | `0dccd8d9443fb0c6e43a66396fcc44c7448c8fcb674ad7bcb6606668fcc159f7` | `97b74499eb2dabf6538e3f91c6c67a29eac0e8236ba6c45596d6f3110288f17f` |
| Races Unleashed / ON | race/feat publication | 13 | `20260905T1734062887979Z-fcadb1c081b044f3b84d744aaadb826f` | `50e9360b7f8e92d05511e20b2e43c22134f321e3a6760f46d3e818032c352ed7` | `fe7409fd8de3cdf73eb61ded624d385684636f1c2cbf299029b44d448850e6b5` |
| Races Unleashed / ON | native feat contracts | 10 | `20260905T1735139539849Z-031e881ca4d541c6b6ae32f67ebf2b0a` | `ae11f770867cf1ae67f0779401d984df6c5ef456dcb2c7c5b4ff441d005a0829` | `04fdeb069c7440beedfefc8a3737be915d54f2cb9e70fe97c69496d445b39bfd` |
| Races Unleashed / OFF | identity / no publication | 13 | `20260905T1736488837017Z-06675315f8794bac8ec401499eba02a5` | `1d4fe1e5790f778602c7be7b248ac08369e26faa01440889f5b24d465d0d6da1` | `344c98902767982ad30efe9370ed13d38d3bfc2277e211a141e6a50bf60ee21a` |
| Favored Class / ON | mod load | 3 | `20260905T1738234019576Z-9343c22054b344a294e1a8975e90cf7d` | `0e1cffdbc9e9be9c41b2f7272ba02f03284419139c8a79f63c4dd5570bfdef6d` | `78c74d2e488c95805c2fe3c2cedfe4a29bc0e6827fe8062797e21db5b525e78f` |
| Favored Class / ON | Aid Another interop | 12 | `20260905T1740061508024Z-17cd24de4748449cb4f959d081ad8679` | `4550a27293f68fd2ffc1a22a11c9de6c4b977324372d6cf0f7572b8fad497dc2` | `83f9af57c49a75d0b7aaefb76195b11645c7b8cfea8a2471d434f49c5dd4b56f` |
| Favored Class / ON | race/feat publication | 13 | `20260905T1741481210245Z-5eb321dde47843caadb8541a186ac783` | `d19b7fd182991b313f95fdbb7433e0724cf9ed56447c13657796fd3110c6bf34` | `284a3020558c54575662732912e1ab6527c318cbd59ca83f1e314c5c25809cb5` |
| Favored Class / ON | native feat contracts | 10 | `20260905T1743307770681Z-754fe9e1fa584ea5978ed2421c399319` | `08b5820091d08bb4dbd195684439ec3fd851838462c007f84fa9c5b00cf7c5d7` | `34a8bdc9f0095f385fe18d734ada95c76c03b5acd30ab3174aba6759f84b8f8c` |
| Favored Class / OFF | identity / no publication | 13 | `20260905T1745422681013Z-75ac9e8d3f2f4e97a6e787965028f540` | `e2516823b6b70a0bf5431c1e3675e1cab864da8d1284fb189675e895db9e24ce` | `584bdcfa75a48a7c51e1eef0846d0c3a9a22f4f256766ef7190e1d88299891df` |
| Tweak or Treat / ON | mod load | 3 | `20260905T1747551100687Z-fa0bbec1480442dfa2b9b7659299a1eb` | `338c4024f2b4efe7aadc67178161703a5f50f65f2fb9f3518fa8b0e0114553a9` | `ecf8d13e0b77e8d30a44a6e5680b5425883a0b6544ef7b43586791896df91d7b` |
| Tweak or Treat / ON | optional contracts | 23 | `20260905T1749381659874Z-a76d664412164e5cac9be11ab7dc99c2` | `8cb8098c4fc2948d7090bac5afd4c471402ee9690273aa9f245445473bbe42e5` | `fd29ae7f85c68b95689da349988bcbab0b0ca21dd98fe4ff6b2de73f4488f117` |
| Tweak or Treat / ON | race/feat publication | 13 | `20260905T1751193716282Z-046521770fe147d380e579d842957cd7` | `25a19e6223f0e299b9b8a819b3f1269d3c8e0a5d4e24a4508b918d27aedae3ee` | `7081c9c658e056a283a790544b5ac73270b4daf54459ea03f0a37461a331de65` |
| Tweak or Treat / ON | native feat contracts | 10 | `20260905T1753010172259Z-daf4a62ecef0434896803154d6c1e46a` | `ee3e95f684f14a22e4965c8d98c35a2a15d87ca9038ca8694e230df1ad73e0b8` | `460ea52b9d345b80f49d455336c5827cda8dd4afaeb9643ce67992ecdfdee5c7` |
| Tweak or Treat / OFF | identity / no publication | 13 | `20260905T1755190666728Z-b9bb3d8d8d164265a91b99d60745a624` | `ad3f1728bbac7b5f64fb7abce07d76ad5927b22d9905eeb41b7ff65156d68d8a` | `05e7ec4d2cc873d8ae1e3c10f0c89dc17bcb524df269592f8068ead1c7e9320f` |
| High risk / ON | mod load | 3 | `20260905T1757324334719Z-366d7a0f46db4add87995c2d923684f9` | `52e5831d4dccd40a957d79b37c4dc0354c8fbf029e3f8138cbf9cfb1374e7f5a` | `d557194c8a2a321f158371f7c636cbbc20092804eee82c961ecc3dccffefaa23` |
| High risk / ON | optional contracts | 23 | `20260905T1759171385299Z-817730869fe140fdb23ae9874ff02719` | `26213fcf624b2333e2f1d078f455f6da579578699e26791019bdd123d9c7274f` | `8ecc2067475091e812977ff3b40c56d53f14d15346a47690dca3a5267436163c` |
| High risk / ON | Aid Another interop | 12 | `20260905T1801016223745Z-f8c31c899cb64a9d9ec3c8320977303c` | `df4fb665cd6597731cbe9434f50b6c6d807891d25f4659efc8baa890a170bf78` | `1517949bf53c459be45ee8e44f0915aca5ccf2725593a4ecfe06e513f0207347` |
| High risk / ON | race/feat publication | 13 | `20260905T1802451679300Z-6c923581d3be44448d35ac948e773f6b` | `86a9685b70edf0aed190d9b09df1b8c60ad8827b522fc221cd55804a33e254c9` | `cb8723f30af60084aefa5dfac5121fbf5e2f665d4ca5d05500c6cd0897f777ce` |
| High risk / ON | native feat contracts | 10 | `20260905T1804281067034Z-040b161e47f44f9796c1b1913d4a943b` | `6a01e150f8507eaf0d1d12dc0237b624443c488b18f40f6ff645cc036e953852` | `344dca3cf09d1825aa815f92b72b210480dbc9083b9b2b9b55f94e8ddb820377` |
| High risk / OFF | identity / no publication | 13 | `20260905T1806504935311Z-d3343552975b42c7bdeffb85771a4e6c` | `e8ad0d0520a9a0245775ae785e8bbaa74c721aad221d4fcbe326b7a284fd9d1f` | `630170155d8271dbd131e02aabe4677d7bd7cd4c99f131f4834ac8db6178f555` |

All 12 ON/OFF transactions restored the same 968-entry original mod-tree
manifest exactly. The compact canonical manifest SHA-256 is
`cc8d634150133b0eed87bcb5933cbe432ad2f1c5153c182e9ce44c8e3099893e`.

| Profile / module | Transaction ID | Transaction SHA-256 |
| --- | --- | --- |
| KMG / ON | `compat-20260905T171546Z-c39b7d8680b7` | `ba9bf7d40f874134bb57f90cb9ee2087011f02c7bd085c355ed56de441a95d53` |
| KMG / OFF | `compat-20260905T172039Z-06bc14911e28` | `0a82e8f8664609b6e2d9ca17e72fa2b06adf00aa4d6adc6e8029c6c87eab3f0e` |
| Call of the Wild / ON | `compat-20260905T172206Z-edb7e30188e5` | `2b5167064c9f6fe1c0e6261f5eb9b205b48de86d692d7b7f985a495e2344e5b6` |
| Call of the Wild / OFF | `compat-20260905T172920Z-2c92282ae658` | `b30aeff4aa17da58cd2c236bdc4aa07ef2769c191e40bdb93e3e0f3a3f3ce2a5` |
| Races Unleashed / ON | `compat-20260905T173125Z-4da60514671d` | `27f97e1b6ba95b5b3a659760e4d16a3fade8b116130715d9dee3ec7684375c65` |
| Races Unleashed / OFF | `compat-20260905T173627Z-0fa0cbdc015a` | `66364ff7f01e7db7279a3584b86543fd51a6e199c41a2f9b0cee86cb210ad842` |
| Favored Class / ON | `compat-20260905T173758Z-13797ba412c9` | `fb677458ecc42fc412db850477e0ed0077901be253e346fef1f1ecef0a3669cb` |
| Favored Class / OFF | `compat-20260905T174519Z-055d2a30c850` | `cdd0145b57d9e71802d66b4884f21e2257aa31c8db36984362023f682950a3f9` |
| Tweak or Treat / ON | `compat-20260905T174726Z-c588c50ffe67` | `9c0047141e52fa93523fb8a07fb23a0d0e9870122165f731f6169764f5636500` |
| Tweak or Treat / OFF | `compat-20260905T175450Z-91dfcb0faeca` | `ef02ab5f7ff62042208e2932f6c8fe09874001b282eff4e3d8f1839ea009f4c0` |
| High risk / ON | `compat-20260905T175704Z-87a85e5f6470` | `0602cd52d5f94b7996eb7532a165852d00a1a0a1398e15d468be77f92125d191` |
| High risk / OFF | `compat-20260905T180619Z-d4e4b7bce652` | `d15b34294414b01b7ab535cb173cf6277b41d631f6a149d605f7ea33f5f1590c` |

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
