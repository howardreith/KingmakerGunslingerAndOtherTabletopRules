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

## Sprint 4 - Native Publication Pack II

The grapple design started from the game's own assembly rather than from
the native mound's graph. The graph is a fine mound - slam, grapple check,
a caster buff that forbids attacking, a target buff that constricts for
4d6 plus Strength every round - and a poor owlbear, because the constrict
and the target state are baked into it. The reusable part turned out to be
underneath: `ContextActionGrapple` only initialises two unit parts, the
parts add the buffs and conditions, and `UnitGrappleController` already
handles escape, unconsciousness and reach. So the shared lifecycle is a
grab component that runs the game's grapple check after a hit with a grab
weapon and initialises those parts with the project's own buffs, and a hold
component on the holder's buff that maintains each round at the tabletop
+5, deals the natural attack (plus constrict) or releases, and releases
its own target whenever the buff turns off. That last rule is the whole
"link ownership" clause of the charter in one place: the summon's side owns
the link, the target's side never reaches back, and a summon that expires,
is dismissed or is dispelled mid-hold cannot leave a party member pinned.

The one thing the native parts do that had to be respected rather than
worked around: a holding initiator cannot act. Kingmaker's grapple is a
lockdown, and the tabletop worm's grab-then-swallow-next-round has no path
through it, because the worm that holds cannot bite. The game's own
summoned worm swallows on the grab check, so ours does too, through the
native swallow-whole part, whose spit-out on death and destruction was
verified in the destruction controller's own order of operations
(destruction is raised before disposal). The traits buff spits out on any
other end. And because a swallowed unit whose swallower has left the loaded
area is otherwise stuck forever - the native part simply returns when the
swallower resolves to nothing - a small area safeguard releases party
members held or swallowed by a KMG summon when the party leaves, and repairs
dangling ones when an area finishes loading.

Two corrections came out of the round-2 evidence rather than the design.
The Cyclops's armed natural 1 hit but registered no threat: the automatic-hit
path in `RuleAttackRoll` never rolls, and decides the critical only from the
two automatic flags together, so "auto-hit with an ordinary confirmation"
is not a thing the engine can do. The bounded Flash of Insight is an
automatic critical hit now; every record says so. And the creature review's
first run failed with nothing but "observed 0": the request writer had
silently dropped the creatures parameter, and the spawn helper, when it did
run, was stripping the mod's own same-turn activation postfix from the
summon rule after each cast. Both are fixed, and the helper now names a
second witness - the units that actually appeared in the caster's area -
before it will call a cast a failure.

The plants were data. The plant class carries the plant traits through its
progression, as the magical beast and humanoid classes did in Sprint 3; the
mound keeps its native slams and resistances and loses a poison aura the
tabletop creature never had; the flytrap keeps its four bites, its acid
resistance and a native 60-foot blindsight standing in for tremorsense, and
holds one target at a time because the initiator part does. The worm is
rebuilt on the natural builder from the native summoned worm's own bite,
sting and poison, without the burrowing kit the charter told us not to
chase.

## Sprint 5 - Mephit Family Expansion

Six mephits on four bodies. The charter's own phrase for it - the proven
mephit rig and shared spell-like-ability infrastructure - turned out to be
exactly right: the native summoned mephits are one chassis with an
element's worth of facts bolted on, so a variant is the same sanitized
clone with the donor's element unbolted and its own bolted on. What made it
more than a palette swap was reading the tabletop family honestly. Every
breath sickens except the magma's; the ooze's save negates everything or
nothing; dust and salt breathe grit, not energy; the ice mephit is an air
creature that happens to be cold. Those are data in a profile now, and the
breath builder reads them off the donor's own cone.

The one design rule that reached into the breath itself was the charter's
ally-safety line. The native mephit cones hurt whoever stands in them,
which is fine for an enemy and a menace for a summon that the AI will
point wherever it likes. The variant breaths run their whole effect inside
an enemies-of-the-caster conditional, and the two project bursts reach
only enemies by targeting, so no new mephit can harm a party member no
matter how it is aimed. The mechanical fixture proves both live: the
steam breath sickens the hostile and leaves the caster alone; the salt
dehydrate damages the hostile and touches neither the caster nor the other
summons.

Visual distinctness had to be earned within the bounded view family. A
material tint can only darken or shift, never brighten, which is why the
pale salt and steam mephits sit on the pale air and water rigs with their
earth and fire subtypes restored as facts, and why magma and steam carry an
emission glow. The tint is applied to a private clone of the view's
materials when the view attaches, so the donor's shared material - and every
native mephit that uses it - is never written. The creature review records
what was applied on each reviewed view, so the render check and the
mechanism check are the same run.

One correction fell out of the round-3 mechanical evidence rather than out
of Sprint 5: the Sprint 4 grapple case's holder-free and safeguard checks
had been reading the mound's CantAct while the native appearance buff was
still holding the freshly summoned mound still. Every link step had passed;
only the witness was looking at the wrong thing. The fixture now strips the
appearance buff and records the baseline before it grabs.
