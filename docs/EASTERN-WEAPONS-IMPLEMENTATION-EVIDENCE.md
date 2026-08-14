# Eastern Weapons implementation evidence

## Investigation authority

- Verified base commit: `4ffd15b09992bd9cee9d330eee0a650ad2c94661`
- Branch: `codex/eastern-weapons`
- Baseline version: `0.0.79`
- Correct landed spear proficiency contract: **Weapon Proficiency (Elven Branched Spear)**
- Eastern proficiency children will use the same **Weapon Proficiency
  (Family)** structure.

The save-free guarded runtime scenario is:

```text
observe-eastern-weapon-contracts
```

It inventories installed weapon donors, native enchantments, proficiency and
grip rules, active equipment and combat-event boundaries, damage-size and
polymorph surfaces, coup-de-grace construction, selectors, fighter groups,
vendor tables, campaign loot, and direct blueprint owners. It performs no
selector, vendor, loot, blueprint, unit, inventory, input, or save mutation.

No production category value, blueprint identity, donor, enchantment identity,
bespoke-effect hook, model donor, price, vendor, or fixed-loot target will be
selected until this exact observer has run successfully against the packaged
baseline-compatible candidate and its structured evidence has been reviewed.

## Baseline qualification

| Surface | Result |
| --- | --- |
| Repository validation | PASS |
| Complete domain/reflection suite | PASS, 1,033/1,033 |
| Clean exact-reference Release build | PASS |
| Build-output validation | PASS |

## Runtime observation

Pending the first coherent observer build, package validation, commit, guarded
publication, and fresh Steam App ID 640820 launch. Exact run ID, loaded identity,
blueprint identities, native component contracts, and campaign reachability
evidence will be recorded here after the structured result is reviewed.

The observer source checkpoint passed repository validation, the complete
1,034-test domain/reflection suite, clean exact-reference Release build,
build-output validation, deterministic package creation, and strict standalone
package validation. Live observation remains pending until the coherent source
checkpoint is committed and published.

The first guarded attempt, run
`20260814T1104529826303Z-b2fcb4605fd84bbabd97ad2bf6af9aa2`, timed out before a
result after the active compatibility stack delayed the first runner update.
It performed no save interaction. That ambiguous attempt is not qualification
evidence. The observer was narrowed and instrumented for a fresh retry.

## Successful installed-contract observation

- Run ID: `20260814T1110588439047Z-7f131097a8ca48ac916f675e77b57c47`
- Source commit: `41b7687079f380a044ffed3a0bf0d3dac771228e`
- Status: PASS
- Loaded version: `0.0.79`
- DLL SHA-256:
  `89C06B4F29190A919512FB0FF5D275C054FC04D940749ECB0828EE6C6499FF4E`
- DLL MVID: `52f2dfea-aaf1-4cda-9c6e-c3f06270c3a7`
- Save interaction: none
- Automatic exit: requested and completed
- Installed blueprint count: 105,859
- Observer phase timings: library 13 ms; weapons/enchantments 425 ms;
  selectors/rules 23,158 ms; campaign inventory 43,509 ms

The exact native donor weapon-type identities found by the live graph are:

| Donor type | GUID | Damage | Critical | Group | Weight | Hand contract |
| --- | --- | --- | --- | --- | ---: | --- |
| Kukri | `006ecd4715809b343b7001e859e3ddb2` | 1d4 | 18-20/x2 | Light Blades | 2 | light |
| Shortsword | `a7da36e0e7bb60e42b9f23462ce2f4fc` | 1d6 | 19-20/x2 | Light Blades | 2 | light |
| Rapier | `2ece38f30500f454b8569136221e55b0` | 1d6 | 18-20/x2 | Light Blades | 2 | one-handed |
| Scimitar | `d9fbec4637d71bd4ebc977628de3daf3` | 1d6 | 18-20/x2 | Heavy Blades | 4 | one-handed |
| Longsword | `d56c44bc9eb10204c8b386a02c7eed21` | 1d8 | 19-20/x2 | Heavy Blades | 4 | one-handed |
| Bastard Sword | `d2fe2c5516b56f04da1d5ea51ae3ddfe` | 1d10 | 19-20/x2 | Heavy Blades | 6 | native special/two-hand-capable |
| Falchion | `6ddc9acbbb6e40746a6a1671df1f7b47` | 2d4 | 18-20/x2 | Heavy Blades | 8 | two-handed |
| Greatsword | `5f824fbb0766a3543bbd6ae50248688f` | 2d6 | 19-20/x2 | Heavy Blades | 8 | two-handed |

All observed melee donor ranges were 2 feet; no donor selection changes the
locked non-reach Eastern profiles.

The first-pass exact native enchantment identities are:

| Contract | GUID | Effective cost | Exact native component |
| --- | --- | ---: | --- |
| Masterwork | `6b38844e2bffbac48b63036b66e735be` | 0 | `WeaponMasterwork` |
| Enhancement +1 | `d42fc23b92c640846ac137dc26e000d4` | 1 | `WeaponEnhancementBonus(1, Stack=false)` |
| Enhancement +2 | `eb2faccc4c9487d43b3575d7e77ff3f5` | 2 | `WeaponEnhancementBonus(2, Stack=false)` |
| Enhancement +3 | `80bb8a737579e35498177e1e3c75899b` | 3 | `WeaponEnhancementBonus(3, Stack=false)` |
| Enhancement +4 | `783d7d496da6ac44f9511011fc5f1979` | 4 | `WeaponEnhancementBonus(4, Stack=false)` |
| Enhancement +5 | `bdba267e951851449af552aa9f9e3992` | 5 | `WeaponEnhancementBonus(5, Stack=false)` |
| Flaming | `30f90becaaac51f41bf56641966c4121` | 1 | `WeaponEnergyDamageDice(1d6 fire)` |
| Frost | `421e54078b7719d40915ce0672511d0b` | 1 | `WeaponEnergyDamageDice(1d6 cold)` |
| Agile | `a36ad92c51789b44fa8a1c5c116a1328` | 1 | `WeaponDamageStatReplacement(Dexterity, RequiresFinesse=true)` |
| Keen | `102a9c8c9b7a75e4fb5844e79deaf4c0` | 1 | `WeaponCriticalEdgeIncrease` |
| Ghost Touch | `47857e1a5a3ec1a46adf6491b1423b4f` | 1 | `WeaponReality(Ghost)` |
| Shock | `7bda5277d36ad114f9f9fd21d0dab658` | 1 | `WeaponEnergyDamageDice(1d6 electricity)` |
| Thundering | `690e762f7704e1f4aa1ac69ef0ce6a96` | 1 | `WeaponEnergyDamageDice(1d6 sonic)` |
| Holy | `28a9964d81fedae44bae3ca45710c140` | 2 | `WeaponDamageAgainstAlignment(Evil, Holy)` |
| Brilliant Energy | `66e9e299c9002ea4bb65b6f300e43770` | 4 | `BrilliantEnergy`; misses Undead and Construct facts |
| Speed | `f1c0c50108025d546b2554674ea1c006` | 3 | `WeaponExtraAttack(Number=1, Haste=true)` |

The first pass did not positively identify a native enchantment named Mighty
Cleaving or Impact. That is not treated as absence yet: a targeted follow-up
will inventory alternate blueprint/component names, loaded member-level rule
contracts, every weapon category/group value, and the exact bastard-sword grip
surface before production implementation begins.
