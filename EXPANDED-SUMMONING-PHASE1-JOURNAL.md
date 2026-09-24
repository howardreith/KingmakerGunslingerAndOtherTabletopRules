# Expanded Summoning Phase 1 journal (charter Sprints 3-8)

Mission: implement charter Sprints 3-8 under the owner's order of
2026-09-24, on one branch and one draft PR that is never merged under the
order, with separately reviewable sprint checkpoints. Sprint 9 is out of
scope; so are release publication and permanent deployment.

## Intake

Phase 0 was finalized and merged as `master` @ `b0641a58` (PR #21 at
`ba5e20a7`) a few minutes before this branch was created from that commit in
its own worktree, so the accepted baseline and the mission's starting point
are the same tree. The Phase 0 closeout's last two findings shape this
mission's habits: a visual is not accepted until an in-game image has been
looked at, because the one defect no assertion could see (a material clone
outside the game's fades) was found only by a render; and a native seam that
has never run live (the menu's scrolling installer) is assumed broken until a
measurement says otherwise.

The game's blueprints cannot be read offline and the third-party sources on
this machine name no unit the sprints need, so Sprint 3 opens with a mod-load
audit scenario that records, as metadata only, which units, classes, facts,
buffs and abilities the installed library offers for the creatures ahead, and
what the native Grab feature's action graph wires to. Tabletop stat blocks for
every Sprint 3-8 creature were fetched from the public SRD on 2026-09-24 and
sit in the Phase 1 report's appendix.

The guarded push script's branch allowlist gained this mission's branch, as
every earlier mission's branch was added; the script itself, its origin check
and its refusal of any history rewrite are unchanged.

## Sprint 3 - Native Publication Pack I

The mod-load audit did what it was built for: every donor, class, fact and
weapon GUID below came out of its `native-donor-audit.json`, and two of the
choices would have been wrong without it. The game ships dedicated summoned
Pony and Horse units (`PonySummoned`, `HorseSummoned`, Summoned faction,
extraplanar marker) that no third-party source mentioned; both hooves are
primary limbs on them, which settled how the tabletop "docile" secondary
hooves are represented. And the Owlbear's claws are the same `ClawLarge1d6`
item the Grizzly Bear already uses, so the magical beast rides the natural
builder unchanged except for its class.

Two builders learned one thing each. The natural builder maps a profile's
hit-die class through a small switch (animal, vermin, magical beast,
humanoid) instead of an animal-or-vermin ternary, and knows three more
weapon keys (the two hooves and the greataxe) and two more natural armor
values. The native option builder can point a cloned umbrella's single
direct spawn at a different unit; that is how the Frost Giant appears under
Summon Nature's Ally VII-IX without a second identity, carved from the
Mastodon options the publisher already suppresses. Everything else was data.

Flash of Insight was the one design decision. Tabletop lets the cyclops
choose the exact result of one of its own die rolls once a day, an
immediate action a player cannot express in Kingmaker's interface. The
bounded form: once per summoning, a swift supernatural action arms a
one-round state; the cyclops's next attack roll is an automatic hit and
critical threat, and the state ends after that one attack through the
native `RemoveBuffOnAttack`, so a full attack never carries it into later
swings. The confirmation roll is left ordinary on purpose - a guaranteed
critical would be more than the tabletop power, which only fixes one roll.
A brain spends it in combat; the action bar lets the player spend it first.
The component keeps no state of its own, which matters because a blueprint
component is shared by every cyclops on the field.

Icons were the expected gap. The 77 roster icons were generated with an
image model that this environment does not have, and the order forbids
buying one. Blender does exist here, so the four new concepts are procedural
renders - metaballs and primitives, procedural materials, the same warm key,
cool rim and bronze ring framing - reviewed at 128 pixels in this session.
They read as their creatures and they are plainer than the painted set;
both facts are recorded rather than argued. The renderer is committed with
the sources, so they can be regenerated or replaced without touching the
runtime.

The count pins were the largest edit and the least interesting: eleven
files carried the number 67, or 681, or 26, as a literal. The runtime
runner now derives them from the catalogs; the domain tests keep the
literals deliberately, so the next roster change fails a test rather than a
live run. The static validation chain, which had never seen a ledger append
without a release, gained a Phase 1 validator that pins the 92 appended
identities exactly and the Sprint 3 figures beside them, and the two older
validators that summed the ledger by feature learned to count the append.

One more instrument, because the closeout of Phase 0 earned it: a creature
review scenario that casts any named creature through its real parent chain
into the working save and renders it from the party camera idle, moving and
attacking, generalised from the Pteranodon review. Every visual creature of
Sprints 3-8 goes through it before its sprint closes.
