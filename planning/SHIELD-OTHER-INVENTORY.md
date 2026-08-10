# Shield Other Pre-implementation Inventory

## Baseline and installed profile

- Frozen base and starting HEAD:
  `7ba84439caa1fc92b8c8148ce95ea79fd59bdc57`.
- Exact Kingmaker references: private extracted 2.1.7b `Assembly-CSharp.dll`
  selected by `GamePath.props`.
- Installed Call of the Wild: 1.14.4c-2.1; DLL SHA-256
  `4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`.
- Static scans of CotW `blueprints.txt`, `loaded_blueprints.txt`, and complete
  IL contain no `Shield Other`, `ShieldOther`, or `shield_other` candidate.
  Final live scans before and after all `LoadDictionary` postfixes remain a
  mandatory runtime gate; static absence is not treated as final proof.

## Exact 2.1.7b damage contract

Read-only ILDASM inspection of
`Kingmaker.RuleSystem.Rules.Damage.RuleDealDamage` proves that this Kingmaker
build has no `RedirectionTarget` or `RedirectedPercent` field/property.
`OnTrigger` calculates per-entry mitigation, sets aggregate `Damage`, applies
difficulty, sets final `Damage`, then in this exact order sets
`UnitEntityData.LastHandledDamage`, consumes target temporary hit points through
`TemporaryHitPoints.HandleDamage`, increments `UnitEntityData.Damage`, raises
`IDamageHandler.HandleDamageDealt`, and publishes hit presentation.

Selected seam: a narrowly signature-validated Harmony transpiler inserts one
Shield Other callback after the final `set_Damage` following
`ApplyDifficultyModifiers`, and before `set_LastHandledDamage`. The callback may
replace only the private finalized aggregate `Damage` value and create one
guarded transfer event. A guarded transfer event is forced to its exact already
finalized share at the same seam, avoiding a second defense or difficulty pass,
then continues through native temporary-HP, HP, death/threshold, damage-handler,
and log pathways. Exact IL anchors and cardinality must fail closed.

Rejected alternatives:

- `RuleDealDamage` prefix: damage is not finalized and defenses have not run.
- `IDamageHandler`: HP and temporary-HP application already occurred.
- full damage, heal subject, damage caster: explicitly forbidden and exposes
  incorrect death, concentration, riders, and observers.
- editing `DamageBundle` before calculation: evaluates/redistributes defenses
  incorrectly and cannot conserve finalized HP damage.
- direct `UnitEntityData.Damage` mutation: bypasses native temporary HP and
  damage/death/log consumers.

## Donors still to validate

Native blueprint runtime inventory remains required for close-range harmless
ally targeting, hour/level duration, deflection AC, resistance saves,
dismissal, caster-linked buffs, range termination, and the five base spell
lists. Final GUIDs and component contracts will be recorded before blueprint
construction.

## Final live inventory - 2026-08-10

Guarded save-free Steam App ID 640820 run
`20260810T0408000666226Z-observe-shield-other-inventory` passed on exact
published source `ef8b80fd43b0e52e1e804c0399a3a0a3d94e9ef9`, loaded version
0.0.76. It scanned 104,644 final live blueprints at the first idle update after
all `LoadDictionary` postfixes and found zero BlueprintAbility candidates by
localized display name or ShieldOther/Shield_Other-equivalent internal name.
Together with the pre-load manifest/installed-assembly scans, this proves the
duplicate-content hard stop is not active in the installed final profile.

Required base spell lists, all native `Assembly-CSharp` identities:

- Cleric: `8443ce803d2d31347897a3d85cc32f53`, max level 9.
- Paladin: `9f5be2f7ea64fe04eb40878347b147bc`, max level 4.
- Inquisitor: `57c894665b7895c499b3dce058c284b3`, max level 6.
- Community domain: `75576ed8cab010644a11f9ecd512a7f9`, max level 9.
- Protection domain: `93228f4df23d2d448a0db59141af8aed`, max level 9.

Optional final live CotW class/spellbook/list chains were unambiguous:

- Oracle class `32c02466b2364c8a906e6e4761175099` -> spellbook
  `3587fa91b34341e49b3a22cfb5450e0d` -> list
  `f305174b73f64783a8379238a14c3283` (max 9).
- Warpriest class `e119d84528144a7797ad34fd718b1f87` -> spellbook
  `9995149d6ff043868cb1fd22ae6ac332` -> list
  `9ef48172d50446aca4c80f321402f743` (max 6).
- Psychic class `359bbaacabc445499049b59d295194cb` -> spellbook
  `9d9689e5253b4dad88b32ce2cf7c8f44` -> list
  `d8eda7e863824c42b3329279cac4d92a` (max 9).

No Friendship or Martyr list identity appeared in the bounded final-live list
scan, so neither subdomain is authorized for publication absent stronger later
evidence.

Native donor evidence:

- `ShieldOfFaith` ability `183d5bb91dea3a1489a6db6c9cb64445`
  provides native divine protective icon/FX/audio and harmless friendly spell
  presentation. Its CotW-added harmless/list components and its mechanics are
  not to be copied.
- `ShieldOfFaith` target buff is resolved only through that ability's exact
  `ContextActionApplyBuff`; its presentation may be cloned while every mechanic
  component is replaced.
- `DivineGuardianTrothBuff`
  `16dd5c27118a51b4f986f484ee388127` proves native
  `RemoveBuffIfCasterIsMissing` and `UniqueBuff` caster-linked patterns, but its
  Bodyguard/In Harm's Way mechanics are rejected.
- Exact IL proves `BuffAllSavesBonus` owns public `Descriptor` and `Value` and
  applies one native modifier to Fortitude, Reflex, and Will. It is selected for
  +1 resistance all saves. Native `AddStatBonus` is selected for +1 deflection
  AC.
- Exact `RuleDealDamage` IL and the selected pre-HP seam remain as recorded
  above; no Wrath redirection fields exist.
