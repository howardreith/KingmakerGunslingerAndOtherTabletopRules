# Autonomous Gunslinger resume handoff

## Durable objective

Execute `AUTONOMOUS-GUNSLINGER-MISSION.md` continuously until its complete
definition of done or a listed genuine human-input hard stop.

## Repository state

- Branch: `codex/complete-gunslinger`
- Audited HEAD: `fb4fd51` (runtime-qualified Quick Clear source checkpoint).
- Qualified baseline contained: `4f28dcf` runtime implementation and `5c92012`
  documentation.
- Current checkpoint: Sprint 37 Nimble is source-qualified and awaits
  exact-commit guarded runtime qualification.
- Version: `0.0.37`.
- User-supplied worktree inputs `AGENTS.md` and
  `AUTONOMOUS-GUNSLINGER-MISSION.md` must be preserved.

## Last runtime evidence

- Quick Clear commit `fb4fd51` passed mod load at
  `20260801T2119244427175Z-mod-load-smoke`. Independent PASS runs
  `20260801T2120422933376Z-disposable-gunslinger-quick-clear` and
  `20260801T2122029596275Z-disposable-gunslinger-quick-clear` proved standard
  no-spend repair, move one-grit repair, zero-grit atomic rejection, zero
  faults, and exact cleanup. Package/DLL SHA-256 are
  `0443b50c7af34857f5ae6eaf7fa491eaff52bae2b38fc133bec7d36d6f557ce4` /
  `e1c3d9273c73b0e1722922896128c60d21f035558b53cbb9f6fb43e9c792f746`.

- Gunslinger's Dodge source commit `f79b4a2` passed mod load at
  `20260801T2059139120474Z-mod-load-smoke`. Independent PASS runs
  `20260801T2102104686115Z-disposable-gunslinger-dodge` and
  `20260801T2103264904832Z-disposable-gunslinger-dodge` proved grit two to one,
  native prone, AC 20 to 24, duplicate stability, atomic insufficient rejection,
  zero faults, and exact cleanup. The movement alternative remains pending.
  Package/DLL SHA-256 are
  `40d738a160929a4c611aaa0263a53fe0d48ccca6fab2ecc93d8fc400b7dd9b4a` /
  `798e1fe7f96cc083de8493e164e5e640ccad2592481ea97251a4fe6cd5815677`.

- Deadeye commit `8ef8854` passed mod load at
  `20260801T2039304720861Z-mod-load-smoke`. Independent PASS runs
  `20260801T2040462109967Z-disposable-gunslinger-deadeye` and
  `20260801T2042028613784Z-disposable-gunslinger-deadeye` proved native armed
  fact consumption, grit two to one at the second increment, duplicate spend
  protection, atomic insufficient rejection, zero faults, and exact cleanup.
  Package/DLL SHA-256 are
  `a59e090b2d14911f77f64ba57d26762d2692c1b25ae7e3b6bd38f25b48d7e8f8` /
  `ec58e53a0c9747a34fa55db2290219cf868f13c171978171b1622f9d45ea2426`.

- Firearm grit recovery repair commit `b4c4874` passed mod load at
  `20260801T2010547590872Z-mod-load-smoke`. Independent PASS runs
  `20260801T2012145241329Z-disposable-gunslinger-grit-recovery` and
  `20260801T2013336952646Z-disposable-gunslinger-grit-recovery` proved exact
  critical `0 -> 1`, killing blow `1 -> 2`, duplicate stability, unaware-target
  rejection, zero faults, and exact cleanup. Package/deployed-DLL SHA-256 are
  `cbeeb299e4e10843cd01079b48ee8c702bd71cf4b469f99c294ac2fd4692bc93` /
  `1596f0b56a60144212E7680AA1DD1421DECB90D507B1725C424B3AE3C3759138`.

- One-time native grit initialization commit `4e543fd` passed mod load at
  `20260801T1945287324618Z-mod-load-smoke`. Independent persistence PASS runs
  `20260801T1946581078281Z-disposable-gunslinger-grit-persistence` and
  `20260801T1948157306155Z-disposable-gunslinger-grit-persistence` observed
  maximum two, spent/reconstructed current one, and current one after a later
  exact feature reapply. Package/deployed-DLL SHA-256 are
  `3f32686c52b0c6b21082a5966eb006032e616d15674d6eb73c2d14bf5f078421` /
  `ca3c62f4b48abb2baba8217b78276282474c3ab5deeace7af9704fd42ce319a7`.

- Native grit persistence repair commit `1949d80` passed mod load at
  `20260801T1935464087387Z-mod-load-smoke`. Independent PASS runs
  `20260801T1937081415049Z-disposable-gunslinger-grit-persistence` and
  `20260801T1938272020280Z-disposable-gunslinger-grit-persistence` proved
  maximum two/current one, distinct exact-blueprint JSON reconstruction at
  current one, and exact cleanup without save APIs. Package/DLL SHA-256 are
  `519c5f99a430808895d1ea787f832088f51b2b861b19974409d6bb2a02715429` /
  `0b37c7748cdca07c0015a730da3626b8a0a6a8d132c96e9ef72f64c325d60866`.

- Native daily grit rest commit `b0ca3f3` passed mod load at
  `20260801T1916580082273Z-mod-load-smoke`. Independent save-free PASS runs
  `20260801T1918185563653Z-disposable-gunslinger-grit-rest` and
  `20260801T1919385521658Z-disposable-gunslinger-grit-rest` proved
  `maximum=1;initial=1;spent=0;rested=1` with exact cleanup. Package/DLL
  SHA-256 are
  `9211a4cd8dfb0e9b2dc9c2092673f9b4fb947cf3849aa176685e13d5a4608694` /
  `997ad369b55856321a3c1ce8593dc219864a0a38857df086e46c5a8902f8e8d6`.

- Native grit repair commit `cd22f3d` passed mod load at
  `20260801T1907337714075Z-mod-load-smoke`. Independent save-free PASS runs
  `20260801T1908491510715Z-disposable-gunslinger-grit-resource` and
  `20260801T1910149815825Z-disposable-gunslinger-grit-resource` proved initial
  1/1, spend to zero, no level-up refill at Gunslinger 2, capped restore to one,
  and exact cleanup. Package/DLL SHA-256 are
  `4ddea7d37d08cd1255562cc8d21678ea686c01d1dc3a48ecaade6630d18c8fbd` /
  `da0d1e20a51dc288daa3383fbd0fff628b79b76194b7946c1e60a774d6d1543b`.

- Exact respec preview commit `3d4ba8f` passed mod load at
  `20260801T1836154433116Z-mod-load-smoke`, then two independent save-free PASS
  runs `20260801T1837314150470Z-disposable-gunslinger-respec-preview` and
  `20260801T1838472989503Z-disposable-gunslinger-respec-preview`. Both proved a
  fresh detached replacement at Fighter 0/Gunslinger 0 reached Gunslinger 1,
  while the original disposable source remained Fighter 1/Gunslinger 0; both
  detached entities were cleaned up. Package/DLL SHA-256 are
  `fffd41f772c7b8b3668c7b8c4d2e8364a16a19cb118dccba112a67d826721e05` /
  `5616dfd5da3eb32431c28501fa53289d6866364e818fa37a9568f235a9e6f36e`.

- Exact multiclass preview commit `c1eb9b7` passed mod-load at
  `20260801T1748568385594Z-mod-load-smoke`, then two save-free PASS runs:
  `20260801T1750132878481Z-0665958b379b4f8ca6067083a9ee9708` and
  `20260801T1751280423920Z-703b0d97c03843a28fedffe8c4392214`.
  Both proved source Fighter 1/Gunslinger 0 and preview Fighter 1/Gunslinger 1,
  with two actions and exact cleanup. Package/DLL SHA-256 are
  `14e5a1746638f1f1d48c4a9ccd79c92cd6307e1d2e96680355a7f8f873e9eedf` /
  `66265d13b598477701674ec05cf50dec009bf2809bca0a3cbd63533ec9cffd86`.

- Exact Sprint 34 level-up preview commit `84bb692` passed mod-load at
  `20260801T1741332784385Z-mod-load-smoke`, then two independent save-free
  `disposable-gunslinger-levelup-preview` runs:
  `20260801T1742575116740Z-8a6cca94fc1c4d97bda6a25e01dad80a` and
  `20260801T1744173560342Z-aae78c49ec4849d19bedbde1f12446fb`.
  Both proved isolated Gunslinger 1 -> 2 preview, unchanged source at level 1,
  two exact actions, and external cleanup. Package/DLL SHA-256 are
  `a1a2e199df996427eef7ca7f123fbdac9da37709c51f7ada5d2960a08455d63e` /
  `386772a6ab12125a40d7647e1d1049ed64a5c2bead7f81ade123d17b735e2472`.

- Exact Sprint 34 starting-item commit `cc2f77d` passed `mod-load-smoke` at
  `20260801T1731148707849Z-mod-load-smoke`, then two independent fresh-process
  `gunslinger-starting-items` runs:
  `20260801T1732334037249Z-ccbfd7861d04442782c358cf7d236dc9` and
  `20260801T1734133597055Z-4fa5d631d72f4b999751ceeb7d119fd5`.
  Both proved one Pistol, one powder, one ball, exact instance/quantity rollback,
  restored class identity/gold/money, stable working-save identity, and no save
  write. Package/DLL SHA-256 are
  `6a41ed815d910983e0067184fdfb40ca16629b8e70aab83f4d1a24fa4ff57153` /
  `2f4d2a0f2772b5923349ce467bbebf7426bb80348c25548d16f0238af23d0fb4`.

- Exact Sprint 31 catalog commit `1539ae9` passed `mod-load-smoke`, run ID
  `20260801T1334059331758Z-9736bc0a7d7844bd83bc9d26b5a30676`.
- Two fresh-process `production-firearm-catalog` PASS runs:
  `20260801T1335276981327Z-5145ec8fbc864500889d489fb4c23fad` and
  `20260801T1336546357107Z-1986affde5794aad8eb0710a31932eb0`.
- Exact deployed package SHA-256:
  `0ca093bd05eaa19a6dc3e3577b618fea2b3db018b29e61965fc0742815e2c342`;
  DLL SHA-256:
  `c0e59abe94e89ec478a55c43327c8ce7763851dc1d50f4a141c39e7ad0767473`.
  Both feature runs proved the three concrete catalog entries, marker/native
  isolation, special-range fail-closed behavior, stable working-save identity,
  and no save write.

- Exact Sprint 31 entry commit `67d7779` passed `mod-load-smoke`, run ID
  `20260801T1309524765214Z-f92ce74df2bc4c6695e6cdd3a6bbeeed`, and canonical
  `working-save-smoke`, run ID
  `20260801T1311109692839Z-c700cd91ca9c45e5aa9082adbc3ec263`.
- Exact deployed DLL SHA-256:
  `f133220a212d0cd6b7af21a58fc42af4f04f00c693329e3eb95185593eed6eaa`.
  The working save was uniquely correlated, the baseline was distinct and not
  loaded, the fingerprint was stable, and no save-writing API was observed.
- Exact commit `47fb861` passed `mod-load-smoke`, run ID
  `20260801T0435526657821Z-4ba4ea84718947f1a8cfc3de1d6ad76a`.
- Exact commit `47fb861` passed canonical `working-save-smoke`, run ID
  `20260801T0437220565711Z-d8664d7f634542f58d8d95126e90fe51`.
- Working-save evidence directory:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260801T0437220409268Z-working-save-smoke`.
- Deployed DLL SHA-256:
  `b1422ae9a2aed50a0ae8a8d2d3f4ff0defc5d03d31f142d7a19c57f5eb973d7b`.
- First active Sprint 33 invariant: exact multi-round state and inventory
  deltas remain atomic across partial top-up, persistence, repeated discharge,
  misfire, and two otherwise identical firearms.

## Current source evidence

- Nimble has five cumulative +1 native Dodge facts at levels 2/6/10/14/18,
  exact light/no-armor gating, equipment refresh, and native flat-footed
  exclusion. The guarded detached scenario covers no/light/medium armor and
  cleanup. Sprint 37 validation, 737/737 tests, exact-reference Release build,
  strict packaging, and 15 dispatch checks pass. Candidate package/DLL SHA-256:
  `3a322ff1b18dcc1e9cf80cbb2d89952b9b01a22ecc4b4a73400446bcd987a1ba` /
  `0188eb4ea037af6e81ed6c15674ee1735eed963302a70b2ccf45e38424bd6e40`.

- Quick Clear now has exact standard and move actions over one equipped
  item-owned misfire-broken firearm. Standard requires positive grit without
  spending; move spends one. Both repair without a kit and fail atomically.
  The guarded detached scenario covers both successes, zero-grit rejection,
  diagnostics, and cleanup. Validation, 732/732 tests, exact-reference Release
  build, and strict packaging pass. Candidate package/DLL SHA-256 are
  `01fd8fe73c53575c08f957aae99cae21fdb262e333949698f670b89fc732dd28` /
  `34fd2cd6acdc105f378bed9ab276acb3b2771a382fbbde3348b05ca239fb6b41`.

- First recovery run `20260801T2006592774427Z` failed safely because a newly
  constructed detached `UnitCombatState` retains `m_InCombat=false`. Exact
  `UnitEntityData.IsInCombat` IL delegates to that flag. The repaired fixture
  sets both exact detached flags and asserts their native getters before event
  evaluation; production recovery code is unchanged. Rebuild and commit before
  the repaired runtime attempt.
  The rebuild now passes 710/710 and strict packaging; candidate package/DLL
  SHA-256 are
  `9b982127b5bebe48d07b7ea4199dcfc0da114a9d9376939a0c402726698f82c2` /
  `36583d9893390380aedb83e3e0e4d952d8303d9a2d60c23159246337fee59b53`.

- Firearm grit recovery is source-qualified. Exact critical and exact native
  weapon-damage zero-crossing paths restore independently, weakly dedupe by
  attack/target reference, and fail closed for invalid combat/target contexts.
  A guarded detached runtime scenario covers both restores, duplicate calls,
  unaware-target rejection, and exact cleanup. Complete suite is 710/710;
  candidate package/DLL SHA-256 are
  `838a3882e5998da9496aaa608a55dfe73ced2433079f8f7ac97aee1b4d25047a` /
  `d589582a7a27d1e146009f7352a62bbd5e0e8c97fd6e96a02fef513dd6122c90`.

- Persistence run `20260801T1927062718434Z` passed JSON identity/reconstruction
  but exposed initial-fill ordering at Wisdom 14: maximum two, current one
  before spend. The active repair adds an owner/class-filtered
  `IUnitGainLevelHandler` restoring only when Gunslinger class level equals one;
  later levels remain non-refilling. Rebuild, commit, and retry next.
  The repair now passes 703/703 tests and strict packaging; candidate
  package/DLL SHA-256 are
  `92b0c3133e81c5cac2b1abb0a8c0fb1f8f952bf28b1120350db54dae703e2686` /
  `400a78b2c5950f45978bacb1e0aaeabfff4afa172c27639818a6f39bb292de2a`.
- Run `20260801T1932396799284Z` reproduced the initial-fill failure. Exact
  `ApplyLevelup` IL proves it does not raise the global gain-level event; it
  explicitly invokes unit-scoped feature reapply after queued actions. The
  repair now uses `IUnitReapplyFeaturesOnLevelUpHandler` with the same exact
  class-level-one guard. It passes 703/703 tests and strict packaging;
  candidate package/DLL SHA-256 are
  `0766e2c3e36dd8d84c8efad04e0e5293eda92bb1d101c898066e3af1f96ff503` /
  `3b9eccf6770898cc89493fc51a1754feda7a7459d32fbcef6672bda82b52f4d2`.
- Multiclass audit found class-level-one alone would refill on later unrelated
  levels. A new stable hidden per-unit initialization marker makes the exact
  reapply restoration one-time; the persistence scenario now requires a later
  reapply to preserve current one. All 703 tests and strict packaging pass;
  candidate package/DLL SHA-256 are
  `1bdc5cdfd0e32d170b16037495441883beff4be96f0bcccaa4b74819f3768efc` /
  `c72d2e3c7bbdb3ebce182a8ab29bdd5a468e4e6fec38e489935ebf205898bdee`.

- Native grit persistence round trip is source-qualified: a non-maximum current
  value uses Kingmaker `DefaultJsonSettings`, deserializes to a distinct record
  with the exact grit blueprint, and rebuilds a fresh detached resource map.
  Complete suite remains 703/703; candidate package/DLL SHA-256 are
  `ccf5facff8d846c8b6ac3598115fe9cebb57df9c6570b1a7aceb7f62b8cf3e2f` /
  `4d519cd6d807d01da2499402599101e5e7c599f951b9e978bb17a6b89ad67c61`.

- Exact IL shows native `RestController.ApplyRest(UnitDescriptor)` restores all
  registered unit resources and the supported-build eligibility helper always
  permits resource restore. A guarded detached `disposable-gunslinger-grit-rest`
  scenario is source-qualified with 703/703 tests and strict packaging.
  Candidate package/DLL SHA-256 are
  `5e4711b5fdfdd4c7ed478fa77e76ad2afddb124c691b2cf498cb5a69b00cd1a9` /
  `c77b482cbe7b9579cd69f88ececc3e551fdc43631e97a4466f4a01b04849f199`.

- First live grit attempt
  `20260801T1904500891309Z-disposable-gunslinger-grit-resource` failed safely
  in native `BlueprintAbilityResource.GetMaxAmount` because its runtime-created
  `Amount.Class` array was null. Commit `cd22f3d` resolved this by initializing
  all four native class/archetype arrays; the two subsequent runs passed.

- The native grit integration is source-qualified: stable resource and owner
  feature blueprints, exact Wisdom-floor subscriber, first-grant restore,
  no level-up refill, and a guarded detached-unit acceptance scenario. Full
  clean validation passes 703/703 tests; candidate package/DLL SHA-256 are
  `6ba5a00008d8d72b14a3db1439e519a02d5b347bdcca6c9db4916954d17ab5bd` /
  `8de23371914342fa66ae5a379c5d156ad874e4e233eaab081c094b068d6d6306`.

- Sprint 32 exact-reference target planning is implemented with 10 focused
  cases. Native cone/volley aggregation adds 10, one-discharge transactions add
  seven, triple-explosion policy adds six, and the fail-closed cone-distance
  boundary adds five; the complete domain suite is 662/662 PASS.
- Package candidate SHA-256:
  `e45b9e2253435a1e5926dccc6c9e00a9cadc96ae25af404a2749e0cc6249a639`;
  DLL candidate SHA-256:
  `33b9cddac997428d945c35e156f04ee004e7109d1a10bb08ed5fa409886c67f7`.
- This slice is source-qualified only. It does not establish cone length,
  native attack delivery or feature runtime acceptance. Native 90-degree
  directional geometry is contract-proven; numeric cone distance is not.
- Sprint 33 exact batch reload and partial top-up transactions add eight
  focused cases. Complete suite is 670/670 PASS. Package candidate SHA-256 is
  `f04448c1cc8de40b9eae5ad781730a4c911a10cd3d9aec83ce6f560d2710dd55`;
  DLL candidate SHA-256 is
  `75788f9ff56f76a3012754802e0c78d847da11c6c1ecd806abada23e065e2b51`.
- Advanced Rifle/Revolver definitions, six-round finite token semantics, and
  capacity-aware early/advanced misfire handling add twelve more focused cases;
  complete suite is 682/682 PASS. Latest package SHA-256 is
  `35527b3cba8d7764b3882e8910c58d43bc600741c1e13bc33ba6ff679afde94a`;
  DLL SHA-256 is
  `9ebbfaaf3dd023441c5e424b777fc04b4609ae2d5185ac8f953ff0fb11ec46ac`.
- Save-owned-vault reconstruction, two-item count isolation, and repeated
  canonical discharge add three cases; complete suite is 685/685 PASS. Latest
  package SHA-256 is
  `ea88730ad1a2aa208de081fb4e8e68000dc53e26be17b24403eeda1cd3d4d26e`;
  DLL SHA-256 is
  `5408f50e47c189115c42d3067250ade2491b2030b8f8b90acb60c7b2a42c82ad`.
- Four stable advanced blueprint IDs now produce exact Rifle/Revolver item/type
  pairs and extend guarded catalog acceptance. Complete suite remains 685/685
  PASS. Latest package SHA-256 is
  `30e5f6b53efcdef9068e2524945bad49b06a78e77ce745c9c29afc79bf0ad957`;
  DLL SHA-256 is
  `5621847b5197518f97e97912eb680b6a024adfde20448ee40fd8c496fa9deae5`.
- Exact commit `41f299a` passed `mod-load-smoke` run
  `20260801T1433034839705Z-56c2363396894593961542057943f189` and two expanded
  catalog runs `20260801T1434411929092Z-b69f2b13cc1f4a03945624f83ff3c5b9`
  and `20260801T1436113008241Z-3c3f36a8807e4ed3869826afd13a5543`.
  Both feature runs observed no save-writing API. Exact deployed package/DLL
  SHA-256 are `6a8386e782f47726c38be60cda52e5e9b335d943a3650426cf6263c5deb51cf2`
  and `ba0f28e9197e1fb6949de9e829a3ccd60b65d865cea2962b6a06528ee87b4a64`.

## Commands already run

- Read mission, roadmap, Sprint 30 report/entry criteria, architecture, source
  and test inventories, version files, and local class/firearm rule headings.
- Verified `codex/complete-gunslinger` descends from the qualified baseline.
- Repository validation passed. Exact Release domain suite passed 611/611.
  Exact private-reference Release build and strict package validation passed.
  Runtime package SHA-256 is
  `b253eaed27bccfd7841ca938032373bb13146984e94c021b1994cbd901397dfd`;
  DLL SHA-256 is
  `5ce1b5bf0d3563648e9fcd9629981c4ee41cf2fb59143df7dedf4f94fbe373de`.

## Sprint 30 closure

Commit `0052dad` passed exact mod load and two fresh-process feature runs. Latest
run ID is `20260801T0448285054152Z-4e5925080ce1422fbcb44c2ee07adcac`;
deployed DLL SHA-256 is
`de9f8507e5180adeb5df8dab4559e901da68022be556ef4fe1ffb874034e3d3f`.
Both feature runs reached `MaintenanceLoopPassed`, proved native Heavy Crossbow
isolation, and observed no save write.

## Next action

Commit the Nimble source checkpoint, then exact mod load and run two independent
`disposable-gunslinger-nimble` fresh-process scenarios. Preserve the Dodge
movement alternative as a documented pending adaptation until deterministic
destination selection is safe. Broad
first-level `Commit` and native replacement callbacks remain deferred until
their global mutations have complete rollback proof; do not invoke them
speculatively.
Full first-level `Commit` remains
deferred until its global rest/entity/remote-companion/view mutations have a
complete rollback proof; do not invoke it speculatively.

## Safety boundaries

Launch only through Steam App ID 640820 and the guarded request mechanism. Use
only `KMG_AUTOMATION_WORKING`; never load or mutate
`KMG_AUTOMATION_BASELINE`. Never save, quicksave, send UI input, or infer a save
from Continue/newest ordering. Stop on ambiguous identity, entitlement, UI,
prerequisite, save-write, or result evidence.
