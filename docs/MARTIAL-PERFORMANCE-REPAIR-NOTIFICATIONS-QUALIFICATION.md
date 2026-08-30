# Martial Performance, loaded repair, and condition-notification qualification

## 0.0.109 release-promotion identity

- Release-candidate commit:
  2e99bb82ad90d4cf84640cb23ec945a2190b394d
- Mod/package version: 0.0.109
- Informational version:
  0.0.109-martial-performance-repair-notifications
- Source-state fingerprint:
  803DF3105EF08116FFDE2858914DAEDD940C2F1BEEF5430D9AAB9638DAB2FA0D
- Runtime package SHA-256:
  5AFBF228C916B5C17BFB16B25F3280F4802870E5CC2A4B8D2D5D7F126FB28F31
- Runtime DLL SHA-256:
  1CFF818811BF2B89FBD35BDB75D53CE79426C0350D97CEDFDCC6F14B6E77CED7
- Runtime DLL MVID: a1824875-8468-44f9-b051-117329c91aa5
- Immutable deployment manifest:
  runtime-evidence/deployments/20260830T0141451717411Z/deployment.json

The release promotion changes version/package/validation/documentation
contracts only. The functional implementation remains commit
b82b5d6c71468d500585f6898456cbe687b9f470.

## Original implementation identity

- Branch: codex/martial-performance-repair-notifications
- Authoritative starting commit:
  2a442a2516bf41f67b175f8e84e3b8e0768c265e
- Mod/package version: 0.0.108 (the existing repository version was preserved)
- Final standalone package SHA-256:
  24ADD06BB3E3E7BE8162367C087EB247A59B638F898D105D41A0536152327A53
- Final mod DLL SHA-256:
  CC20B7FC7C3D8D7B51BF0DA01046AF49799236726811F949FF3F58FE752BEA51
- Final mod DLL MVID: 80fb3300-1407-4dca-bf7b-e2b0b6697e36
- Runtime-qualified implementation commit:
  b82b5d6c71468d500585f6898456cbe687b9f470
- Runtime source-state fingerprint:
  99728DDF70B1079FA11B837C985346F3425A43FE73C230322306B7DD8A14F71E
- Immutable deployment manifest:
  runtime-evidence/deployments/20260829T1902279773898Z/deployment.json

Generated packages, deployment manifests, compatibility transactions, saves,
and raw runtime captures are not part of the source commit.

## Native contracts established

The installed Call of the Wild Martial Performance selection was identified by
all three stable contract fields:

- GUID: 19d1ff4cf70845d094b0ec231473e97f
- Type:
  Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection
- Internal name: MartialPerformanceFeatureSelection

The exact native child donor is
b7786666fe5b4694b8c4560efa6053c3 /
DaggerMartialPerformanceFeature. It has one AddParametrizedFeatures row that
grants native Weapon Focus
(1e1f627d26ad36f43bbd26cc2bf8ac7e) and one
PrerequisiteProficiency component.

Assembly-CSharp inspection established that
BlueprintFeatureSelection.ExtractSelectionItems(UnitDescriptor,
UnitDescriptor) reads BlueprintFeatureSelection.AllFeatures and returns
IFeatureSelectionItem values; BlueprintFeatureSelection.CanSelect is the
native eligibility gate. The live observer therefore exercises the real
before-level-up and preview descriptors, CanSelect, FeatureSelectionState.Select,
and feature AddFact/RemoveFact lifecycle.

The installed Kingmaker 2.1.7b transient warning route is:

1. Kingmaker.UI.Common.UIUtility.SendWarning(System.String)
2. EventBus.RaiseEvent<IWarningNotificationUIHandler>
3. IWarningNotificationUIHandler.HandleWarning(message, true)
4. Kingmaker.UI.WarningsText

The production adapter uses the ordinary public UIUtility.SendWarning(string)
API. It does not use reflection, a custom overlay, a bark, a modal dialog, or
the combat log as a substitute.

## Implemented behavior

### Martial Performance

Seven persistent optional children are derived from the exact native donor:
Pistol, Musket, Blunderbuss, Elven Branched Spear, Wakizashi, Katana, and
Nodachi. Firearm rows reuse the existing firearm Weapon Focus choice and
proficiency facts. Other rows use the same authoritative category-proficiency
authority as weapon use, including Katana's established direct-or-broad-martial
grip rule.

Publication mutates only the exact optional selection's AllFeatures array.
Foreign/native choices remain in their original order, enabled custom choices
form one deterministic tail, disabled module families are absent, and repeated
publication cannot accumulate duplicates. Optional-provider absence is inert.
Wrong type, internal identity, donor membership, component shape, or native
Weapon Focus identity fails closed before publication. Rollback restores the
exact original Features and AllFeatures array references.

### Loaded repair

The authoritative Broken-to-Normal state transition now always produces
Normal, zero loaded rounds, and no loaded-ammunition identity. Ordinary repair
no longer rejects a loaded Broken firearm. The existing transaction still
removes exactly one repair kit only within the atomic operation and restores
the exact condition, round count, ammunition identity, and kit count on every
failure path. Successful repair never returns loaded ammunition to inventory.

The historical FirearmRepairStatus.Loaded numeric member is retained for
diagnostic compatibility but is unreachable. Player-facing availability,
ability descriptions, action policy, and result invariants now state that a
successful repair destroys every loaded round.

Wrecked recovery remains Wrecked -> Overhaul -> Broken -> ordinary repair ->
Normal. Overhaul remains empty on success and does not emit a degradation
notification.

### Broken and Wrecked notifications

A dependency-light dispatcher formats only committed severity increases:
Normal -> Broken and Broken -> Wrecked. The exact format is
"Wielder's Item is now broken." or "Wielder's Item is now wrecked.", with the
item-only fallback when no legitimate wielder name is available.

The post-commit boundary publishes the existing condition combat-log entry
first and then invokes the native warning adapter exactly once. Ignored,
prevented, unchanged, recovery, hydration, migration, reconciliation, and
failed-commit paths do not invoke it. Notification failures are logged softly
and cannot roll back the already committed condition.

The boundary is used by ordinary firearm misfires, Dead Shot, and Scatter Shot,
the three production firearm degradation mutation routes found by the audit.

## Automated qualification

- Version-aware repository validation: PASS.
- Focused Martial Performance tests: 11/11 PASS.
- Focused condition-notification tests: 9/9 PASS.
- Focused ordinary-repair transaction/result/presentation tests: 22/22 PASS.
- Complete dependency-free domain/reflection suite: 1,348/1,348 PASS.
- Exact Kingmaker 2.1.7b Release compilation: PASS.
- Final clean Release build/package/strict standalone validation: PASS. The
  archive contains exactly one DLL, KingmakerGunslinger.dll, whose hash matches
  the clean build output.

The focused repair coverage includes empty, loaded single-shot, loaded
multi-round, no-kit, wrong-condition, state-write failure, post-write
verification failure, inventory failure, exact round/ammunition identity
rollback, no ammunition refund, ability availability, presentation text, and
the unchanged Wrecked progression. Existing reload, firing, misfire, and
persistence suites are included in the complete run.

## Guarded runtime qualification

Every game launch used the repository's guarded request mechanism through
Steam App ID 640820. These request-local disposable or read-only observers did
not select, load, or write a save.

### Exact 0.0.109 release-candidate rerun

All three scenarios reused the immutable deployment identified above.

| Scenario | Structured result | Evidence directory |
| --- | --- | --- |
| disposable-overhaul-maintenance | PASS, 20/20 | 20260830T0141452393620Z-disposable-overhaul-maintenance |
| reliable-firearm-misfire-matrix | PASS, 10/10 | 20260830T0144066808429Z-reliable-firearm-misfire-matrix |
| observe-optional-mod-compatibility (Call of the Wild 1.14.4c-2.1) | PASS, 23/23; restoration verified | 20260830T0147110940751Z-observe-optional-mod-compatibility |

- Loaded-repair run:
  20260830T0141452550457Z-598679f1ee8043f588cd7f0b72be5ef9.
  The real ability was available for a loaded Broken Last Word; delivery
  changed one round / `kmg.debug.lead-ball` to zero / null, consumed one kit,
  returned no ammunition, preserved all three static enchantments, and ended
  Normal. Overhaul, combat logs, exact-item identity, and request cleanup also
  passed.
- Notification run:
  20260830T0144067120928Z-5c842b16db0b4877982d26ee2c1a2f8a.
  Committed Normal-to-Broken and Broken-to-Wrecked transitions produced the
  exact concise messages. Native adapter attempts/published progressed
  `0,0,1,1,2,3,3` / `0->3`, faults remained `0->0`, the unchanged
  boundary was silent, and condition combat logs progressed `0->3`.
- Martial Performance run:
  20260830T0147111097156Z-741ca12feb7e453b9fa48bb433e2cf22.
  The exact selection identity, seven singular custom rows, native donor/effect
  shape, non-proficient native parity, preview proficiency, commit, and Weapon
  Focus lifecycle all passed against Call of the Wild 1.14.4c-2.1.
  Transaction `compat-20260830T014636Z-91fd74d0de47` restored the complete
  Mods tree, Call of the Wild settings, and FeatureModules.json exactly.

### Original 0.0.108 implementation rerun

| Scenario | Structured result | Evidence directory |
| --- | --- | --- |
| disposable-overhaul-maintenance | PASS, 19/19 | 20260829T1902428044215Z-disposable-overhaul-maintenance |
| reliable-firearm-misfire-matrix | PASS, 10/10 | 20260829T1904370178694Z-reliable-firearm-misfire-matrix |
| observe-optional-mod-compatibility (Call of the Wild 1.14.4c-2.1) | PASS, 23/23; restoration verified | 20260829T1906493731101Z-observe-optional-mod-compatibility |

Loaded repair runtime result
(run 20260829T1902428224715Z-43e39dc2542a48baaab09d7a3ff61f0e):

- The real Repair Firearm ability was available for a loaded Broken copy of
  The Last Word.
- Delivery succeeded.
- Loaded state changed from 1 round / kmg.debug.lead-ball to 0 / null.
- Exactly one repair kit was consumed.
- No powder or lead-ball inventory count increased.
- The exact item became Normal; its three static enchantments remained intact.
- The preceding Wrecked-to-Broken overhaul and subsequent cleanup both passed.

Notification runtime result
(run 20260829T1904370506601Z-c94796bb714f487f943333de739acf6a):

- A committed Normal-to-Broken misfire dispatched the exact concise Broken
  message once.
- A committed Broken-to-Wrecked misfire dispatched the exact concise Wrecked
  message once.
- Native-adapter attempts/published counts progressed
  0,0,1,1,2,3,3 / 0->3, with zero adapter faults.
- The unchanged diagnostic boundary published zero additional notifications.
- Existing condition combat-log publication progressed 0->3 and retained the
  Wrecked entry.
- Three real UIUtility.SendWarning(string) calls returned successfully and the
  WarningsText handler type was present.

Martial Performance runtime result
(run 20260829T1906493886745Z-212da8a5738440f18033c9bfc36d38e1):

- The exact GUID/type/internal-name contract and seven registered children
  passed.
- All seven enabled custom categories appeared once in deterministic order,
  with the native donor retained.
- On a real non-proficient ChargenUnit, the native Dagger row and every custom
  row were visible but CanSelect returned false.
- Real proficiency facts added to the preview ChargenUnit made all seven
  custom rows visible and selectable.
- FeatureSelectionState.Select committed every enabled custom row.
- Adding and removing the Pistol child added and removed the native Weapon
  Focus parametrized fact with the same lifecycle as the donor.
- The Call of the Wild profile and feature-module settings were restored byte
  for byte after the run.

## Corrected probe history

Two fail-closed probe results were retained as engineering evidence rather than
reported as product failures:

- 20260829T1811206689616Z-reliable-firearm-misfire-matrix:
  the intended Broken and Wrecked assertions passed, but the request-local
  probe incorrectly assumed an Advanced Rifle would not legitimately degrade.
  The probe was narrowed to an explicit unchanged central boundary; production
  firearm behavior was not weakened.
- 20260829T1826163371544Z and
  20260829T1831182754397Z-observe-optional-mod-compatibility:
  the first observer used an unsafe Single lookup and the second filtered
  BlueprintFeatureSelection items as FeatureUIData. Assembly-CSharp inspection
  proved this selector returns IFeatureSelectionItem values. The corrected
  observer uses the native item interface and CanSelect; the final live profile
  passed.

All associated compatibility transactions reported exact restoration.

## Evidence boundaries and pending supervised checks

The runtime observer establishes live blueprint publication, real native
eligibility/preview/commit behavior, and applied effect shape. It does not
visually navigate the rendered Bard level-up screen. A supervised pass remains
pending to confirm the rows' rendered presentation while selecting proficient
and non-proficient characters.

The notification run establishes committed-state ordering, exact message
dispatch, successful calls through the native transient warning API, native
WarningsText handler presence, and retained combat-log entries. It did not use
screenshots or Computer Use and therefore does not claim visually observed
top-of-screen placement. That visual placement and exactly-once rendered
lifecycle remain pending supervised confirmation in a loaded game.

The real ability runtime covers a loaded single-shot firearm. The exact
multi-round discard and rollback behavior passes domain tests, but a loaded
multi-round in-game ability interaction remains pending supervised
confirmation.

The runtime-created unit had the engine placeholder CharacterName
"-unit name not set-". Exact friendly named-wielder and missing-wielder fallback
strings pass dependency-free formatting tests; a named player character remains
part of the supervised presentation check.
