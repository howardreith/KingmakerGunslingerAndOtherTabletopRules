# Optional-mod compatibility journal

## 2026-08-07 - Mission start and unchanged baseline

- Repository root: `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger`.
- Starting branch/commit/version: clean `master`,
  `d03dfe9eae65f5cd1395df7337f21dfdb4357661`, `0.0.71`.
- Isolated branch/worktree: `codex/postbase-archetypes-compatibility` at
  `.worktrees/postbase-archetypes-compatibility`.
- Repository validation: PASS.
- Complete dependency-free domain/reflection suite: 911/911 PASS. The first
  sandboxed run failed only because the sandbox denied the audio fixture's
  atomic `File.Replace`; the identical approved elevated run passed.
- Exact-reference Release, build-output, SoundBank, package creation, and strict
  package validation: PASS. No deployment occurred.
- Package SHA-256:
  `1815C6A37C935A61223D026E03A8E6D50A0D949066CD41F9D2A17479D9197CC2`.
- DLL SHA-256:
  `F879904D51DDAA0B226375048EF0C7983F44158B8441EC1EC4616C00CB204BEB`.
- AssetBundle SHA-256:
  `F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B`.
- SoundBank SHA-256:
  `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
- Stale-state finding: the prior active resume incorrectly summarized the
  attach-slot A/B result. Per the current work-order authority, attach-slot
  Experiment A still left held long guns invisible and the later isolated
  holster-patch experiment restored them; minor clipping is accepted.
  Historical entries remain untouched, including their conflicting summary.
- Next action: inventory every immediate child of the authorized examples root
  without mutation, then implement the committed catalog/schema and fixtures.

## 2026-08-07 - Reference inventory checkpoint

- Read-only run ID: `20260807T1740349239015Z`; 12 immediate children fully
  inventoried with bounded `.git` metadata/object exclusion recorded.
- Loadable primary roots: ArmsArmor 1.0.10, CallOfTheWild 1.14.4c-2.1, and
  ToggleCustomSoundpacks 1.0.1. CraftMagicItems 1.10.0 is source-only.
- Five `KAZ_*` folders proved to be small loadable UMM equipment mods rather
  than raw assets. They are retained as a grouped extension and not promoted to
  a required primary profile without separate manifest authority.
- CallOfTheWild compiled/source versions differ (1.14.4c-2.1 versus 1.14.5), so
  no exact source-twin claim is made. ArmsArmor and Toggle identities/versions
  align but byte equivalence remains unproven.
- Eddic Respec and Bag of Tricks are absent:
  `UNAVAILABLE-LOCAL-REFERENCE`.
- Inventory fixtures PASS: source-only classification, invalid-loadable
  classification, and canonical approved-root escape rejection.
- Version surfaces advanced together to 0.0.72 with a version-aware validator.
- Full 0.0.72 checkpoint gate: repository validation PASS, 911/911 PASS,
  exact-reference Release PASS, build-output/SoundBank/strict package PASS.
  Package SHA-256
  `43D48259B890F7F600DF6E2FFC1B5D142ED0948FF1A1BD4FE1F0181E9779B006`;
  DLL SHA-256
  `BD1BD66C690A4689A2125CE4D6CC8ED3CFC36962ADB222A131F1BDA856FA0339`.
- Next action: run full 0.0.72 source/build/package gates, commit/publish this
  checkpoint, then implement the static GUID/Harmony/bootstrap audit.
- Commit: `274d4d7` (`chore(compat): establish mission and reference inventory`).
- Required push helper: FAIL before network activity; branch is not on its
  external allowlist. No alternative push was attempted. Safe local work
  continues; publication status is blocked.

## 2026-08-07 - Static audit and profile manifest checkpoint

- Added deterministic standard-library lexical audit plus PowerShell wrapper
  and behavior fixture. Fixture proves exact GUID collision classification and
  shared Harmony-target reporting.
- Real audit: five trees, zero cross-owner project-definition GUID collisions,
  three shared Harmony targets. Curated details are in the forensics report.
- Added eight required stable profiles. Craft Magic Items and its combined
  profile are `STATIC-AUDITED-ONLY`; absent extensions remain explicit. The
  high-risk primary combined profile includes the three compiled primary roots;
  the all-loadable extension additionally names all five proven KAZ UMM IDs.
- No runtime compatibility claim is made. Next engineering phase is exact
  profile resolution followed by transactional staging fixtures.
- Full checkpoint gate: repository validation PASS, 911/911 PASS,
  exact-reference Release/build-output/SoundBank/strict package PASS. Package
  SHA-256 `AC9DB772CDA6C228B1CEEA2AC13CE7DDF73BDD2C85A87D0EFE57D827CB4BECAF`;
  DLL SHA-256
  `8891F0709E96282908BB26898BB51D852F61D8497FD859E4B74E8F0AA6857EFA`.

## 2026-08-07 - Publication resumed and transaction core

- Verified clean required worktree/branch at exact `9da61a4`; the updated
  approved helper published successfully and origin matched the local commit.
- Current user authority supersedes the earlier KAZ staging interpretation:
  `KAZ_*` stays an asset-reference group, `runtimeStagingAllowed=false`, and no
  KAZ key or UMM ID appears in the all-loadable runtime profile.
- All eight dry runs PASS without Mods mutation. Six are runtime-capable;
  Craft Magic Items and its Call-of-the-Wild overlay remain static-only.
- Added a committed-profile-only resolver with exact UMM/version/assembly/MVID,
  whole-tree source manifests, intended destination, load order, warnings, and
  runtime-capability reporting.
- Added transaction enter/restore/recovery primitives. Public entry accepts only
  a committed profile ID and exact lab state root. The original Mods directory
  is renamed, never merged; a fresh staged directory carries a sentinel bound
  to an immutable hashed ownership record.
- Filesystem integration tests PASS for normal restoration, copy/profile
  failure, simulated launch failure, destination collision, unresolved prior
  transaction, interruption recovery, staged extra-file detection, original
  hash mismatch preservation, original Mods absent, managed SoundBank
  present/absent, running-process refusal, duplicate restore, spaces in paths,
  and sentinel hash mismatch.
- No real Kingmaker installation, Mods directory, Steam process, save, or
  SoundBank was touched by these fixture tests.

## 2026-08-07 - Guarded runtime observer source checkpoint

- Added `observe-optional-mod-compatibility` to the existing guarded request
  parser/runner. Only the six committed runtime-capable profile IDs are
  accepted; source-only, unavailable, traversal, extra-parameter, and arbitrary
  values fail closed in the focused fixture.
- Exact installed contracts are used: UMM 0.32.4 private `modEntries` for
  ordered entry identities/state and Harmony12 1.2.0.1
  `GetPatchedMethods`/`GetPatchInfo` for owner, role, priority, before/after,
  and order evidence.
- Runtime assertions cover isolated UMM identity, assembly MVID/SHA-256,
  singular base class and 20-level progression, exact Mysterious Stranger
  registration/replacement rows/Charisma binding, five production firearm
  pairs, non-faulted Wwise state, singular Gunslinger patch installation, and
  the observer's save-free boundary.
- Focused observer fixture PASS. Full gate PASS: repository validation,
  911/911 tests, exact-reference Release, SoundBank, and strict package.
  Candidate package SHA-256
  `08DA0786E37F1AC4EC97A0166DEC0FEDBAEC7FE9B6E0168A8984EEA62D12DB22`;
  DLL SHA-256
  `026B7215DCAC0AA1923B11F5A1E79101D0C70872BD462813FC9703C11918F698`.
- No real Mods mutation or Kingmaker launch occurred at this checkpoint.

## 2026-08-07 - First real transaction, runner timing repair

- Transaction `compat-20260807T191144Z-9ce245d1f232` staged only Gunslinger.
  Guarded `mod-load-smoke` run
  `20260807T1912134251961Z-379f7fd088d945fca5a7e663ed6c1262`
  PASS with embedded commit `3fbd5ae`.
- Exact original Mods and managed SoundBank restoration verified `True` at
  `2026-08-07T19:13:16.1390728Z`. Staged mutation was observed and safely
  discarded only after restoration verification.
- The second requested fresh process was correctly refused because the first
  Kingmaker process had committed its result but had not completed automatic
  exit. Root cause is wrapper sequencing, not product compatibility. The
  wrapper now waits boundedly for process exit between scenarios as well as in
  `finally`. No process was killed.
- After the sequencing repair, transaction
  `compat-20260807T191438Z-3adf77ada3af` again restored exactly. Its mod-load
  control passed; observer run
  `20260807T1916297159780Z-observe-optional-mod-compatibility` ended `ERROR`
  before assertions because `typeof(UnityModManager)` did not expose the
  private field on the live runtime type. Offline exact assembly inspection
  reconfirmed UMM 0.32.4 MVID `97735e89-6c7c-4f6c-a737-187e1328fba3` and field
  `modEntries`. The repair now resolves the manager from the actual
  `context.ModEntry.GetType().DeclaringType`, eliminating compile-reference
  type selection from this exact-runtime query.
- Observer run under transaction
  `compat-20260807T192017Z-17a2340a5202` proved the live declaring type but
  still failed before assertions. Exact field metadata then showed
  `modEntries` is `Public, Static, InitOnly` in this UMM 0.32.4 build, contrary
  to the earlier private-field note. The binding now includes `Public` and
  `NonPublic`; restoration again verified exactly.
- Observer run `20260807T1924016022942Z-fce5fc0272f9417a968fbbb87d3fd868`
  reached all assertions. UMM identity, class/progression, exact Mysterious
  Stranger rows and Charisma binding, five firearm pairs, Wwise, and save-free
  checks PASS. Only Harmony duplicate detection failed: the same Evasive
  postfix legitimately targets two `Restore` overloads, while the identity key
  omitted parameters. Patch target identities now include exact parameter type
  signatures. Transaction `compat-20260807T192334Z-3062c8cc37ab` restored
  exactly; no gameplay defect was observed.
- Two consecutive exact gunslinger-only observers PASS:
  `20260807T1927010278912Z-9f3d6d5e8337497ab1866bfef14247d7` and
  `20260807T1928069055132Z-841e007fa26a441490602a5e7f56901c`, with transactions
  `compat-20260807T192628Z-7568d3bd3e7b` and
  `compat-20260807T192735Z-4e989a3e6edd` restored exactly.
- Initial Call of the Wild profile transaction
  `compat-20260807T192918Z-c9ce6b83ef83` exceeded the basic 120-second result
  timeout during the first guarded process. The harness did not kill it; it
  exited during the wrapper's bounded finally wait and exact restoration
  verified. This is a broad-mod startup observation, not a compatibility
  verdict. Profile orchestration now supplies the already-supported 300-second
  scenario/startup bounds.
- Arms & Armor transaction `compat-20260807T194023Z-0fdb2d8752de` restored
  exactly. Mod-load passed; observer
  `20260807T1942160100478Z-ac890d898de0440b817a78e55484d285` passed every
  product, assembly, Harmony, Wwise, and save-free assertion. Its only failure
  was manifest comparison treating expected membership order as mandated load
  order: actual UMM order was `ArmsArmor,KingmakerGunslinger`. Expected IDs are
  now compared as an exact set while the observed string continues to preserve
  and report actual order.

## 2026-08-07 - Individual compiled-reference qualification

- Call of the Wild 1.14.4c-2.1 transaction
  `compat-20260807T193416Z-b1e18d2b46e4` produced a structured guarded
  `TIMEOUT` after 300025 ms during `mod-load-smoke`; readiness never completed.
  Exact Mods and managed SoundBank restoration verified. Disposition is
  `CONFLICT-OBSERVED`, limited to exact DLL SHA-256
  `4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`.
- Arms & Armor 1.0.10 isolated observer PASS run
  `20260807T1945060542640Z-8e90a80c6ae340b78d4fc3f2320927f3` recorded actual
  UMM order `ArmsArmor,KingmakerGunslinger`; result SHA-256
  `45C30C714A41D4C72321E4298ECCE36EAE075857DF190F109A267FDA20116C95`.
  Visual-rig run `20260807T1951374125273Z-0230e28df636482a885f9ca9e15c7cfb`
  and production-switching run
  `20260807T1953023759407Z-8edf6383be2b41248db68dd728f23107` both PASS.
  Transaction `compat-20260807T195110Z-e0d22a37068f` restored exactly.
- Toggle Custom Soundpacks 1.0.1 load PASS run
  `20260807T1946430623743Z-28f134968e6b41cc93d93e22abf4bb60`, observer PASS
  run `20260807T1948071575023Z-b02d7bbca92841339c5735c7df6b8101`, and Wwise
  PASS run `20260807T1954513766772Z-03339ad39a714dfb8c46407bcf0b0f36` establish
  isolated exact-version audio coexistence. Harmony evidence records the
  third-party `AkBankHandle.LoadBank` and `LoadBankAsync` prefixes while the
  Gunslinger bank remains Ready and the discharge scenario remains singular.
  Transactions `compat-20260807T194616Z-2670aac10cc1` and
  `compat-20260807T195424Z-6f83d019d8ef` restored Mods and the bounded
  `KMG_Firearms.bnk` side effect exactly.
- Current deterministic package SHA-256 is
  `2C85B610CB51247F5C45D7EB1803EA6BBE7BF9584FC253A0B5E73DCE120D965D`.
  Runtime evidence remains machine-local; only exact IDs and hashes are
  curated here.

## 2026-08-07 - Combined readiness diagnosis

- High-risk combined transaction `compat-20260807T195748Z-3af2b4bfabb9` and
  independently named all-loadable transaction
  `compat-20260807T200349Z-4c435e2e7ee6` each reached structured `TIMEOUT` at
  the 300-second bound and each restored exactly.
- Both results report `timeoutStage=request-accepted`: Gunslinger accepts and
  validates the guarded request immediately, but Unity's main thread does not
  enter the scenario runner until the deadline. No Gunslinger assertion or
  runtime exception is emitted. This narrows the observed condition to a long
  blocking combined bootstrap/readiness path shared with the exact Call of the
  Wild profile.
- The wrapper now exposes a bounded 120-900 second timeout, defaulting to 300,
  for one materially different 600-second Call of the Wild attempt. Profile and
  scenario allowlists, Steam launch, automatic exit, and restoration semantics
  are unchanged.
- The one changed-strategy Call of the Wild attempt used 600 seconds under
  transaction `compat-20260807T201206Z-e36dbdadd645`. It again ended structured
  `TIMEOUT` at `request-accepted`; exact restoration verified. This profile and
  both combined profiles remain `CONFLICT-OBSERVED`; no further unchanged
  bootstrap retry is authorized by the evidence.

## 2026-08-07 - Maximum passing combination extension

- Standalone class/blueprint and presentation scenarios PASS in fresh processes
  under transaction `compat-20260807T202419Z-f94e101e3a19`; exact restoration
  verified.
- Added `gunslinger-qualified-combined` as a ninth extension profile containing
  only the independently passing Arms & Armor 1.0.10 and Toggle Custom
  Soundpacks 1.0.1 roots. The original eight required logical profiles remain
  intact; this extension does not weaken their conflict dispositions.
- All nine dry runs, observer/preflight allowlists, transaction fixtures,
  repository validation, 911/911 tests, exact Release build, SoundBank, and
  strict package validation PASS. Pre-runtime package/DLL SHA-256 are
  `072235BA4057153D655A782D7DCC08F109FDE676C9CD701BAB192B6A929178E1` /
  `511EAE870CE9760A527701C424CF644895B060EB531D36772C3C2519C5AF79F4`.
- First runtime transaction `compat-20260807T202946Z-28927c42d2fd` proved
  combined `mod-load-smoke` PASS, then stopped before observer launch because
  the top-level guarded launcher retained its own six-profile `ValidateSet`.
  Restoration verified. The new ID is now added to that final typed allowlist
  and tested; no arbitrary profile input is accepted.

## 2026-08-07 - Qualified-combined comprehensive diagnosis

- Transaction `compat-20260807T203235Z-07bfd9da0b29` passed the exact observer,
  presentation, visual rigs, production switching, Targeting Arms, Wwise,
  Scatter Shot, and reload scenarios. Comprehensive run
  `20260807T2044246940174Z-c8aab2a7b41049a9bc9b218d4b49ab61` failed two
  fixture slices; restoration verified.
- Standalone control run
  `20260807T2046415501631Z-13a6f5a8855e44d099e312eed7348018` reproduced the
  same Grit-recovery and Dodge fixture failures under transaction
  `compat-20260807T204615Z-d788273d6ced`, proving they are not optional-mod
  interaction defects. Restoration again verified.
- The Grit fixture spent its only restored point from 1 to 0 before the
  unaware-target call, yet asserted it remained 1. The assertion now matches
  the actual fail-closed contract: it remains 0 and diagnostics record one
  ignored event. The comprehensive aggregator now preserves the full inner
  exception chain, and the established dedicated Dodge scenario is admitted
  by the typed profile wrapper for exact diagnosis.
- Dedicated standalone Dodge run
  `20260807T2050596684740Z-855d97503f97488086afa4b2c7268038` again reached
  `activate-immediate-dodge`, but the central error formatter also discarded
  inner exceptions and the exact game log contained no nested trace.
  `ExceptionSummary` now preserves `Exception.ToString()` so the next single
  diagnostic run can identify the actual native command failure.
- Full-chain Dodge run
  `20260807T2054129739533Z-dcc9e8bb415c491f80cd306230dde85f` identified the
  exact nested condition: the detached native command finished with
  `result=Interrupt`. The fixture now records that result and continues to its
  existing strict mechanical assertions instead of treating the result enum as
  the behavior. Missing Grit spend, buff, AC, availability, or cleanup still
  fails closed.
- Effect-based standalone run
  `20260807T2057209416590Z-a48e33c01d6f48f2b407eb08fe361035` proved the
  finished `Interrupt` applied no timed Buff fact. Transaction
  `compat-20260807T205654Z-995e08e885b6` restored exactly. Standalone and the
  maximum passing optional combination are therefore
  `GUNSLINGER-REPAIR-REQUIRED`; working-save smoke was not attempted.
- Current package/DLL/AssetBundle/SoundBank SHA-256 are
  `5FD8DC95EAA96B4DCAF225C41AEBE700816D3B41FD4D12D70A5E69B6DE2CA0D1`,
  `B22C9ED4FE76E61C0152CFFF376CC19EE2A9380DB32BD10E8AA178168DB1A80A`,
  `F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B`, and
  `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

## 2026-08-07 - Human-confirmed Call of the Wild catalog conflict

- The user manually installed the exact local Call of the Wild 1.14.4c-2.1
  build with Gunslinger and reached new-game character creation. Call of the
  Wild classes including Antipaladin, Arcanist, Bloodrager, Brawler, and Hunter
  were visible; Gunslinger was absent from the final player class list.
- This supersedes timeout-only uncertainty for the player-facing result.
  Disposition is `CONFLICT-CONFIRMED`: the final character-creation catalog
  omits Gunslinger. The earlier guarded `request-accepted` timeouts remain a
  distinct automation/bootstrap-boundary problem.
- No cause is inferred yet. The next work stream must distinguish registration,
  final root catalog publication, stale selector cache, later removal/root
  replacement, or another exact compiled-DLL behavior before repairing.
- This conflict is independent of the standalone detached Gunslinger's Dodge
  fixture. Neither result is evidence for the other.
