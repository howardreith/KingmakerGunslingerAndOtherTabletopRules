# Elemental Races 0.0.117 Crystalline Form native audit

## Outcome

PASS for this read-only native-contract audit only. Crystalline Form is not
implemented or mechanically qualified by this checkpoint. Release C still has
fifteen native-proven mechanics and six implementations plus full-release gates
outstanding. No save-bearing identity or gameplay provider changes here.

The dedicated `ElementalCrystallineFormNativeAuditScenario` inventories live
projectile-delivery components, exact ability/parent/weapon/projectile identities,
and native result-replacement API availability. It runs from the existing
feature-specific trait scenario; no mechanics are added to the central runner.
Its ten focused assertions per profile do not substitute for actual attack tests.

## Rules and engineering finding

[Crystalline Form](https://aonprd.com/RacesDisplay.aspx?ItemName=Oread) replaces
Earth Affinity with racial AC against rays and a chosen daily ray deflection.
[Deflect Arrows](https://aonprd.com/FeatDisplay.aspx?ItemName=Deflect%20Arrows)
uses no action and requires awareness/non-flat-footed status and a free hand.
The skin-based trait's application of those conditions must be carried through
the actual qualified implementation, not silently replaced with a swift cost.

Ranged touch attacks are not automatically rays. The printed
[Acid Splash](https://legacy.aonprd.com/coreRulebook/spells/acidSplash.html),
[Snowball](https://aonprd.com/SpellDisplay.aspx?ItemName=Snowball), and
[Battering Blast](https://aonprd.com/SpellDisplay.aspx?ItemName=Battering%20Blast)
create an acid projectile, snowball, and force sphere respectively. All three
nevertheless use the native Ray weapon category with Simple projectile delivery
and an attack roll in the actual KMG-only process.

| Native control | Exact ability GUID |
| --- | --- |
| Acid Splash | `0c852a2405dd9f14a8bbcfaf245ff823` |
| Snowball | `9f10909f0be1f5141bf1c102041f93d9` |
| Battering Blast | `0a2f7c6aa81bc6548ac7780d8b70bcbc` |

A projectile-only catalog is also insufficient: native Ray of Frost and
WaterDomainBaseAbility both use `d6c9daec1256561408a7a72a6979359e`;
ScorchingRayAcid and AcidArrow both use
`89cd363b66b1df440b5281f7d3ef188d`. Some native converted rays have no parent
link, so assuming every derivative points at the original spell is unsafe.

Read-only inspection of the exact installed native assembly establishes:

- `RuleAttackRoll.SetFake(AttackResult)` is public and writes the stored
  result. Native `AbilityDeliverProjectile` itself uses it for ForceAlwaysHit
  after triggering the attack rule. An early replacement can therefore be
  overwritten; merely assigning late AutoMiss does not change the stored result.
- `Projectile.OnHit` raises `IProjectileHitHandler` before native deflection,
  callback and OnHitTrigger handling. This supplies a possible owner-filtered
  local-component boundary; this audit does not prove that a particular handler
  safely suppresses every downstream ray effect.
- Native `TryDeflectArrow` requires IsFromWeapon, a successful non-natural
  weapon attack and the native non-flat-footed check. Retaining its feature
  flag alone cannot make spell rays deflectable.

Next: construct rules-audited exact semantic eligibility and qualify actual
native ray AC/deflection delivery, chosen activation, conditions, one-use
commitment, exclusions, resource lifetime, and fresh saves. No global patch,
blanket concealment/ranged-touch defense, or invented substitute is authorized.

## Immutable source and artifact

- Starting authoritative master:
  `6874dc15a27ded132456dbdd480f47c794543a05`.
- Embedded parent commit:
  `c93aafb4ab7ed5c8977797787553ade9ab6504ec`.
- Branch: `codex/elemental-races-expansion`.
- Source-state SHA-256:
  `177219881a0985a7f72f57811a97b746721cbca91828da3add812e585ea9cd3d`.
- Version: `0.0.117-elemental-traits`.
- ZIP: 23,212,363 bytes, 135 entries;
  `7fdcc4ddfb9aee38ca1e5f6fd94524401d5b8bf2c1b65dd982f9be6885512ca0`.
- DLL: 6,135,296 bytes;
  `6d9e821a5bd8cbd00a11182ee203f11d7801e2414b4568953578a6268e7e9653`.
- DLL MVID: `36cc8ae6-2415-489d-8de6-95da77b1606b`.
- Deployment: `20260906T1228517785330Z`;
  `312901a5f01dc647c475a43d770c90ec806da16dffb9445c002888a7f17cd402`.
- Installed Assembly-CSharp SHA-256:
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.

Repository validation, all 1,421 domain/reflection tests, clean exact-reference
Release build, and strict package validation pass. Manifest remains 1,853 total,
1,851 active, two reserved; 216 Elemental identities (215 active), including
69 Release C identities. This audit adds zero save-bearing identities.

After documentation, all source/build/package gates passed again at source
fingerprint
`1d51d092323e3c2b83c02ff8de806c20b0170b889f520643f6c275d7c242cf35`.
Both ZIP and DLL remained byte-identical to the guarded candidate. The original
package and its build metadata are retained in ignored
`artifacts/qualification/0.0.117/crystalline-native-audit`; this is not an
additional game run or a committed package.

## Guarded evidence and restoration

Both processes use the documented guarded request and Steam App ID 640820.
No campaign/save is opened or written.

| Profile | Run ID | Assertions | Deliveries inventoried | Ray-category deliveries |
| --- | --- | ---: | ---: | ---: |
| KMG only | `20260906T1231315440718Z-ea5c7afa110d42c082fea0064d9590a8` | 4908 | 308 | 63 |
| Highest-risk combined / Favored Class | `20260906T1236449844113Z-29da5e3f08af43f6857e55ace3e37fe0` | 4908 | 1004 | 194 |

All 9,816 assertions pass with zero runtime-result warnings. All observed
catalog/component-array references and order remain unchanged. Both exact
968-entry original mod manifests independently match the restored live tree,
including file contents and timestamps, with manifest SHA-256
`69d7a7cb41d83fe8be5917324cff9febc7c83c3eb1c1e5390e4e147bd31d730f`.
Settings hashes are unchanged. The exact profile transaction, result, actual
`runtime-evidence.json` manifest, audit JSON, native-log and attribution-summary
hashes are recorded in `releaseCCrystallineAudit` in the mission STATE.

The KMG-only native log has zero KeyNotFoundException occurrences; the combined
log has four, consistent in count with the preceding combined checkpoint.
Neither has a Fact.PostLoad signature. This is not a claim that the combined
native log is error-free or that this audit completes final warning attribution.

The first wrapper invocation failed argument binding before any game launch:
passing `-Confirm:$false` through an external powershell.exe -File call produced
a SwitchParameter conversion error. Its 968-entry profile transaction restored
successfully. The corrected direct PowerShell script invocation reused the
same artifact and unchanged authorization. The initial failure is retained,
not relabeled as a gameplay PASS.

No package, raw artifact, save, proprietary assembly, or optional-mod source/GUID
catalog is committed. No merge, tag or release publication occurred. The
mandatory branch-push allowlist blocker remains separate from local engineering.
