# Brown-Fur Transmutation Spell Inventory

Status: runtime inventory capture pending for the current source checkpoint.

The authoritative source is the resolved CotW Arcanist casting spell list in the
installed Kingmaker process. The guarded, save-free scenario
`observe-brown-fur-transmutation-inventory` enumerates every genuine
Transmutation root and recursively follows every `AbilityVariants` relationship.
It records spell levels, source spellbook, range and target flags, duration,
metamagic, component/action graphs, applied buffs, bonus carriers, modifier
descriptors, context/static values, polymorph and size components, hard-coded
caster routing, and save/dispel presentation.

The first capture deliberately labels every record `Unexplained`. That is an
investigation state, not a release classification. Brown-Fur blueprint
publication remains absent while any entry is `Unexplained`. After the runtime
capture, each entry will be curated into exactly one of:

- Supported by generic contract
- Supported by named adapter
- Intentionally ineligible
- Blocked by an understood engine limitation
- Unexplained

Converted-from relationships live on per-cast `AbilityData`, not solely on the
shared blueprint. The inventory records this runtime boundary; variant and
converted-chain cast fixtures must supplement the static blueprint graph before
final qualification.
