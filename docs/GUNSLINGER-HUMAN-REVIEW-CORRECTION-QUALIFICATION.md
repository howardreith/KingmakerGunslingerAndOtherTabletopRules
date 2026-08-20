# Gunslinger Human-Review Correction Qualification

## Acadamae priority continuation override - 2026-08-20

The exact installed `0.0.88` human cast remained Full-Round and produced no save. This supersedes the former 15/15 PASS conclusion. Preserved run evidence is under `C:/Dev/KingmakerGunslingerLab/runtime-evidence/acadamae-human-runs/20260820T1949234419079Z`.

Root cause is proven at the first boundary: ordinary KMG summon selection supplied an outer project variant over a detached native `SummonMonsterISingle` node; neither node carried `ParamSpellSlot`. The old resolver did not search memorized slots by canonical blueprint identity and therefore logged `prepared=False`, `canSpend=False`, `preRequireFullRound=True`, `status=not-prepared`.

The replacement is source-qualified only. It resolves canonical spellbook, blueprint, slot, level, school, summoning marker, and pre-Acadamae time without mutating UI data, then binds the resolved slot only in the observed three-argument command constructor. Repository validation, `1,162/1,162`, clean Release/output, SoundBank, package, and strict-package gates pass. Two fresh guarded runtime runs and ordinary human OFF/ON confirmation remain required.

## Intake baseline

- Branch/ref equality: `e2e3d9ec941549a889a1e03a590e24241b745b7f` after remote fetch.
- Required ancestry: PASS.
- Worktree and external runtime/profile state: clean/restored.
- Repository validator: PASS.
- Complete dependency-free domain/reflection suite: 1,160/1,160 PASS.
- Candidate version: `0.0.88`.

These results establish only an unchanged baseline. They do not override the
fresh unloadable-save, ordinary Acadamae, icon, merchant, visual, or acquisition
findings.

## Release gates

P0 requires root cause, a reproducing focused test, safe affected-copy recovery
disposition, the complete disposable Focused Aim serialization matrix, and two
fresh-process save/load passes. Acadamae requires two real prepared-player-path
passes covering forced success/failure plus all action, slot, cancellation,
fatigue, rest, Cord, and ineligible controls. Every source issue additionally
requires focused tests, repository/full suite, clean Release/output, relevant
asset and SoundBank gates, deterministic strict package, diff/artifact audit,
guarded runtime evidence, and compatibility restoration.

Visual and audible judgments remain human gates. No `0.0.89` release claim is
permitted while the unloadable-save defect or real-player Acadamae path remains
unresolved.
# P0 qualification result - 2026-08-20

`PASS` for source prevention and packaged compatibility fields: repository validator, `1161/1161`, clean Release build, output validation, SoundBank validation, deterministic package, and strict package validation. `NOT QUALIFIED` for full save recovery: the old Focused Aim FX reconstruction exception is absent, but both the affected copy and a same-area no-marker control stall before the native after-load callback. Version advancement and final release qualification remain prohibited until this is resolved or exact human evidence closes the Tenebrous runtime boundary.

## Acadamae real-player qualification - 2026-08-20

`PASS` for the repaired installed boundary. The rejected fixture exposed a selected Summon Monster I variant with a null outer prepared slot and an exact available slot on its `ConvertedFrom` root. Production now resolves and binds only that exact same-spellbook chain slot at command construction. Repository validation, `1161/1161`, clean Release/output, SoundBank, deterministic package, and strict package validation pass.

Fresh Steam run IDs `20260820T1346113518157Z-cac809535698401786e65c307f7be644` and `20260820T1348390845989Z-5c143a2e41254043849e50afa8aa4598` each passed 15/15. Both exercised native `UnitUseAbility.OnAction` and proved Standard presentation/execution, one slot spend, exactly one Fortitude save, DC 16, forced success without fatigue, forced failure with permanent canonical fatigue, rest removal, Cord substitution, toggle-off, cancellation, ineligible controls, and cleanup. Final visible ordinary-play confirmation remains in the consolidated human pass; the former synthetic conclusion remains superseded.

## Acquisition re-audit qualification - 2026-08-20

`PASS` for source, transaction, density, and packaged live publication. Repository validation, `1161/1161`, clean Release/output, SoundBank, deterministic package, and strict package validation passed. Run `20260820T1425182231173Z-observe-rare-firearm-acquisition` proved all 30 exact project uniques at 30 fixed targets, one loot row and zero vendor rows per item, retired-row normalization, and maximum normalized named-area density 2 rather than the rejected prior maximum 6. Package/DLL SHA-256 were `74b6c3b160520308f4cf53fa61c32c9d33d6d0b62f048f1477ad4de93c90155d` / `dae237d18e6b91bd86fc5b017fbcdad9d074c268dc5e06eb393d54a2b278b5af`. Exact live-mod restoration succeeded from backup `20260820T1425147446013Z`. The read-only locator does not loot or mutate targets; ordinary interaction/accessibility and final pacing/theme acceptance remain human-gated.

## BTSL merchant split qualification - 2026-08-20

`PASS` for exact table identity, role responsibility, transactional normalization, retained-row order, source/build/package gates, and live blueprint publication. The complete suite passed `1162/1162`. Run `20260820T1444126864934Z-observe-rare-firearm-acquisition` proved Honest Guy owns the six firearm/permanent central rows plus module-enabled Eastern weapons and spears, while Xelliren owns only Black Powder 200, Lead Balls 200, Paper Cartridges 200, Repair Kits 10, Overhaul Kits 5, and Gunsmith's Kit 1. Wrong-role project rows were absent. Package/DLL SHA-256 were `24485e1762cdf75a5ee9f734b870a3916e5e21954485a3d23e99bc53d07450fb` / `de43f1941d499ea02ccf7c88c27257cdc1b60f6516b91d0f5de0e694af48fb44`; exact restore from `20260820T1444092863954Z` passed. Actual merchant materialization and list aesthetics remain human-gated.

## 2026-08-20 firearm feat icon correction checkpoint

- Status: implemented; source qualification and guarded UI observation pending.
- Human rejection: the 0.0.88 dark circular firearm monograms and Rapid Reload medallion are superseded.

- Root boundary: the accepted Nodachi parameter appearance is produced by CustomWeaponSelectorRuntime through FeatureUIData with a null sprite plus the NO monogram; nodachi.png is item art, not the parameter template.
- Repair: retained every stable firearm choice blueprint and exact publication mapping, replaced only the six project-owned rendered assets with a deterministic reconstruction of the native selector grammar, and added a separate pale-field oxblood reload glyph.
- Source/provenance: JSON palette and monograms plus PowerShell vector/source generator; Segoe Script and Georgia system fonts are rendered but not packaged; no native pixels or proprietary fonts are included.
- Automated evidence: deterministic 64/32 contact sheet generated; focused/full/build/package/runtime gates pending.
- Human gate: compare P/M/B/Ri/Rv and Rapid Reload beside native choices at actual UI scale.
- Next action: run focused icon test, repository validator, complete suite, clean Release/package gates, then the packaged disposable firearm-dependent-feats observer.
## 2026-08-20 firearm feat icon automated qualification

- Status: automated-qualified; final aesthetic judgment remains human-gated.
- Determinism: a second tools/New-FirearmFeatIcons.ps1 run reproduced all six PNG and 64/32 contact-sheet SHA-256 values exactly.
- Repository/source: PASS.
- Complete dependency-free suite: 1,162/1,162 PASS, including firearm-feat-icons.semantic-publication.
- Clean Release/package: PASS; output validation PASS; firearm AssetBundle manifest/output validation PASS; SoundBank validation PASS; strict standalone package validation PASS.
- Guarded runtime scenario: disposable-firearm-dependent-feats, run 20260820T1505344745363Z-71ef2e5f35aa45ce9c929d0dc5369f47, 13/13 PASS.
- Runtime publication: distinct exact P/M/B/Ri/Rv sprites resolved under Weapon Focus, Greater Weapon Focus, Weapon Specialization, Greater Weapon Specialization, Improved Critical, and Rapid Reload children; the separate Rapid Reload top sprite resolved; all native top-level icons were preserved.
- Local-runtime package SHA-256: f256f59f65587d7475672eb415ed0e648cc60c7c85e4e388f60fa35021630b70.
- DLL SHA-256: c6060a14968fe0227b601fd0fe5c2c2f736241d4044b24036717576071900ecf.
- Firearm AssetBundle SHA-256: 1aa75fa1230abfb60cd5148ca90b99d604dbece7d80d98d85cb7d7c0a885a8ff.
- SoundBank SHA-256: 0e9f88c562f4f937a8941ace0f241bb31a7ed56b46fbca549c98f764392edf18.
- Runtime deployment backup: C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20260820T1505311020164Z; exact only-target restore verified.
- Human gate: inspect the five calligraphic parameter fields and Rapid Reload beside native feats at real 32/64 UI scale.
## 2026-08-20 long-gun yaw refinement qualification

- Status: automated-qualified; final visual judgment remains human-gated.
- Repair boundary: Musket held `Visual` uses local Y `+3` degrees and Blunderbuss held `Visual` uses local Y `+4` degrees. The normalized source meshes, scale, firing-hand root, support target, muzzle, projectile, materials, audio, and independent back prefabs are unchanged.
- Measured effect: live Musket muzzle `(0.05470153, 0, 1.04376733)` and Blunderbuss muzzle `(0.0437930971, 0, 0.626270533)` follow the exact revised held axes. Back `Visual` rotations remain `(0,0,12)` and `(0,0,346)` respectively.
- Determinism: two independently restaged Unity 2018.4.10f1 ForceRebuild passes produced identical 17,971,200-byte firearm bundles at SHA-256 `050197BA87F71B7C8D5D4FF056D4FF7CF0C9CCD1DBBD8FB23E748FCE6492C35C`.
- Gates: repository validation PASS; complete clean Release suite `1,162/1,162` PASS; clean Release/output PASS; SoundBank PASS; package and strict package PASS.
- Guarded runtime: `disposable-firearm-visual-rigs`, run `20260820T1526089673122Z-4727a84add664cbbbb4c93f1b3695c06`, `65/65` PASS.
- Runtime package/DLL/SoundBank SHA-256: `A7780EA797ABA10DFED36D47C2EB1B627EAC09FCD29E6F01727CD5A104D94959` / `ED3EAF1A30E3EED42C773EA9D231EE64DEE8106065293344B40CE106B1B78E46` / `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
- External state: exact pre-run backup `20260820T1526055855877Z` restored and verified for only the live Gunslinger mod directory.
- Human gate: compare Musket and Blunderbuss on world/inventory dolls, male/female/small bodies, idle/combat idle, attack/reload, switch, back state, direct/Scatter, support hand, muzzle, and clipping.

## Elven Branched Spear orientation and carry

`PASS` for source, deterministic bundle, blueprint, donor-equivalence, mechanics,
build, package, and guarded runtime contracts; human visual acceptance remains
open. The repair preserves the accepted 2.28 m meshes and stable item identities,
maps the source point to installed held forward `-Y`, and publishes a separate
diagonal BeltModel for every visual variant. Two Unity 2018.4.10f1 builds matched
at SHA-256 `33EB89C74EC4AE7CDA5A8155224A449233904B74CB59FC453C24AE022EE3CB2A`
and 126,658 bytes. The complete suite passed 1,162/1,162 and the clean Release,
output, SoundBank, package, and strict-package gates passed. Guarded Steam run
`20260820T1542457366433Z-eb6ee44b6d434229bfc2b1f671afc544` passed 25/25.
The pre-run live mod backup `20260820T1542422540763Z` was restored with 140
files and zero full-tree hash differences.

## Integrated final qualification - blocked release

- Branch implementation HEAD entering final gates:
  `95fd43dab2b19681e8a8d093ed58b4c7009c6413`.
- Version remains `0.0.88`; `0.0.89` was not assigned because P0 is open.
- Repository validation, 1,162/1,162 dependency-free tests, clean Release,
  build-output validation, SoundBank validation, deterministic package build,
  and strict standalone package validation passed.
- Final package:
  `artifacts/packages/KingmakerGunslinger-0.0.88-urban-barbarian.zip`, SHA-256
  `98BA3475B5CD2068DF6152C49DEAF47CF9D8C1247F889E1F12FB0646079265C9`.
- Final DLL SHA-256:
  `E6E08804CD19C69DACA8A3BE77DC04220497BFC78E0CE31B07BE0B498953B76D`.
- Firearm / spear / Eastern AssetBundle SHA-256:
  `050197BA87F71B7C8D5D4FF056D4FF7CF0C9CCD1DBBD8FB23E748FCE6492C35C` /
  `33EB89C74EC4AE7CDA5A8155224A449233904B74CB59FC453C24AE022EE3CB2A` /
  `079AA2E44E313291C144BD830D302782310274B11375204F9CE8FF6481EF3041`.
- SoundBank SHA-256:
  `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
- Qualified-combined optional-mod profile
  `compat-20260820T155202Z-b3c7b51f1e64` passed; runtime
  `20260820T1552555133802Z-05508b8ce4e947b9a4bf0c70644fdaea`
  passed 16/16 and the profile restored external state.
- Final Focused Aim run
  `20260820T1558117539372Z-38e92ddef9bb4892b87a21ad17f24384`
  passed 7/7. Final Acadamae run
  `20260820T1600539794795Z-b798433e90554ecfb153a1052aad1d83`
  passed 15/15.
- Final fresh-process working-save runs
  `20260820T1603352327573Z-0badc91ea9204a5f948a74dffd537b03`
  and `20260820T1606370368921Z-a040156b8ea249da873b49b434170133`
  each passed 11/11.
- Broad disposable acceptance run
  `20260820T1554523223418Z-9f9d0d577bae4dcc8b56be64ad295c16`
  failed 6 of 184 assertions: Dodge timed buff; Targeting Torso natural-19
  critical cache; Targeting Legs damage; Targeting Legs trip; Bleeding Wound HP
  fact; Evasive progression. The run still passed 178/184 and its relevant
  Grit/state/reload/audio assertions, but it is recorded as `FAIL` without
  weakening the aggregate gate.
- All final launch transactions were restored. Kingmaker is not running and no
  compatibility/Mods transaction sibling remains.
- Release disposition: `BLOCKED` by the affected/control Tenebrous
  scene-completion stall. Visual, audible, merchant-materialization, campaign
  pacing/accessibility, and ordinary-player acceptance remain human gates.
# Acadamae detached prepared-path qualification (2026-08-20)

## Result

`SOURCE-QUALIFIED`, `RUNTIME-QUALIFIED`, `HUMAN-ACCEPTED`, `REGRESSION-FROZEN` at product commit `7a38cdcd0f740d1fce1b2460166748fcae593ffd`, version `0.0.88`.

The current repair does not rely on the former detached fixture conclusion. The replacement fixture deliberately removes `ParamSpellSlot` from both the player-selected summon variant and its detached canonical node, then reaches the three-argument player command constructor and `UnitUseAbility.OnAction`.

## Evidence

- Root cause reproduced from the human log: the real selected variant had a canonical native `SummonMonsterISingle` ancestor and real Wizard spellbook, but no slot reference anywhere in the selected chain, causing `status=not-prepared`.
- UI mode OFF: `preRequireFullRound=True`, `resultBefore=True`, `resultAfter=True`.
- UI mode ON: `preRequireFullRound=True`, `status=full-round-to-standard`, `resultBefore=True`, `resultAfter=False`.
- Canonical resolution: native `SummonMonsterISingle` GUID `8fd74eddd9b6c224693d9ab241f25e84`, Wizard spellbook GUID `5a38c9ac8607890409fcb8f6342da6f4`, exact available memorized slot, level 1, Conjuration, Summoning descriptor.
- Native completion: exactly one Fortitude resolution at DC 16; forced success has no fatigue; forced failure records `fatigue=fatigued-permanent`; rest, Cord, cancellation, command failure, and cleanup assertions pass.
- Harmony audit: all six target seams report `applied=True`. CotW's actual owner is `CallOfTheWild`; the KMG `RequireFullRoundAction` and `RuleCastSpell.OnTrigger` patches report `after=CallOfTheWild`.
- Runtime runs: `20260820T2015089247100Z-disposable-acadamae-graduate` `15/15 PASS`; `20260820T2017093646741Z-disposable-acadamae-graduate` `15/15 PASS`.
- Human acceptance: Howie subsequently tested the exact installed candidate in ordinary gameplay and explicitly confirmed that Acadamae Graduate is working. This closes only the Acadamae ordinary-player gate.
- Full suite: `1162/1162 PASS`.
- Package: `artifacts/local-runtime/0.0.88/KingmakerGunslinger-0.0.88-local-runtime.zip`, SHA-256 `60FE24AB03616B63E376EA9B07187990737DBA8A5DEFF88F7AA71121226610AB`.
- DLL SHA-256: `11DE92F69051E95578C8DCECC33D392D2EE998870708A91760B87D96F5FB9BD0`.
- Firearms bundle SHA-256: `050197BA87F71B7C8D5D4FF056D4FF7CF0C9CCD1DBBD8FB23E748FCE6492C35C`.
- SoundBank SHA-256: `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

Acadamae Graduate's ordinary player path is accepted and regression-frozen. Overall 0.0.88 release qualification remains blocked by the separate Tenebrous completion issue; version remains 0.0.88 and no 0.0.89 release is authorized. Unrelated cosmetic, merchant-materialization, acquisition-accessibility, visual, and audible gates are unchanged.
