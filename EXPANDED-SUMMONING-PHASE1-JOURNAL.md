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
swings. (Superseded by the 2026-09-25 correction order below: the arming
has no duration of its own, and the attack's own d20 is chosen as a
natural 20 rather than the hit being granted.) The confirmation roll is
left ordinary on purpose - a guaranteed
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
automatic critical hit now; every record says so. (Superseded by the
2026-09-25 correction order below, which rejected the automatic critical
and chose the attack's own d20 on the pre-rolled-result seam instead.) And the creature review's
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

Visual distinctness had to be earned within the bounded view family, and
the first three attempts did not earn it. The design tinted a private
clone of each variant's material, and every run said the tint was
applied; the review renders said otherwise - six mephits wearing their
donors' faces. The Pteranodon's notes explained the first half: the
game's material controller drives a view's materials itself, and on the
mephit rig it rewrites the tint slot every frame. A coat on the main
texture, the tiger's trick, was applied and retained and changed nothing
either. The probe that finally answered it listed every slot on the
material at the moment the picture was taken: a translucent body with rim
lighting on, and the only thing that differs between the game's own air
mephit and fire mephit is the rim light colour - and even that slot is
painted every frame, by a looping rim animation the view carries. So the
mephits are told apart the way the game tells them apart: the view's own
rim animation is recoloured - sand, ice, ember, slime, salt and vapour
glows, pulsing as the natives pulse - on a clone the controller has been
made to re-read, so its fades reach it. No emission glow is claimed; the
shader has no slot for one. The review no longer trusts the attach-time
report: it records what is on the view when the picture is taken.

One correction fell out of the round-3 mechanical evidence rather than out
of Sprint 5: the Sprint 4 grapple case's holder-free and safeguard checks
had been reading the mound's CantAct while the native appearance buff was
still holding the freshly summoned mound still. Every link step had passed;
only the witness was looking at the wrong thing. The fixture now strips the
appearance buff and records the baseline before it grabs.

## Sprint 6 - Existing Signature Mechanics Repair

The repair sprint was mostly a matter of finishing sentences the earlier
sprints had started. Three profiles still said "grab is omitted because the
installed generic graph carries unrelated Shambling Mound constrict"; the
Sprint 4 lifecycle was built precisely so that sentence could be retired,
and the Monitor Lizard, Grizzly Bear and Dire Bear now carry the same grab
component the Owlbear does, with their own weapons and nothing of the
mound's. The domain suite pins the boundary the charter drew: exactly one
grabber constricts.

The spider's web asked for a decision rather than a discovery. The game
has a web spell that fills an area for minutes and a web-grappled state
that any unit can wear; the tabletop spider throws one web at one foe.
The bounded shape is the latter: a 50-foot extraordinary ability, a Reflex
save at the spider's poison DC, the native web-grappled state (entangled,
held, a break-free attempt every round) for at most ten rounds, two uses
per summoning. The spider wears the native web immunity so it walks through
its own work, and the native 60-foot blindsight stands in for tremorsense
the way it did for the flytrap; the game's own Tremorsense feature turned
out to be a kineticist talent with class prerequisites, not a sense a
summon can carry. Climb stays out: there is no save-safe seam for it.

The Pixie needed only a verdict. Its arrows and its dance were already
resource-bounded and already proven live by every mechanical run; the
sprint records that and pins it, and changes nothing.

## Sprint 7 - Big-Cat Combat System

The cats already pounced; what they lacked was honesty about their claws.
Kingmaker's established way of representing rake is to give a cat two extra
claw limbs, and those limbs attack on every full attack whether or not the
cat charged. The tabletop rake is a reward for the pounce: two more claws
on the charge, or against a foe the cat has already caught. The gap is
exactly one predicate, and the engine exposes both halves of it - the
attack rule knows whether it is a charge, and the grapple initiator part
says whether the cat is holding someone.

So the rake gate is small. The cat's rake claws are the last two weapon
slots of its body (the game fills the additional limbs first and the
secondary limbs after them, which is why the leopard's third and fourth
claws and the smilodon's secondary pair land in the same place), and an
attack roll with one of them that is neither a charge nor made while
holding is turned into a silent automatic miss: no die, no damage, no line
in the log. The attack still exists inside the command - removing limbs on
the fly would mean rewriting the full-attack builder the charter told us
not to touch - but nothing of it reaches the player. The mechanical fixture
drives all four cases on the leopard: a primary claw on an ordinary attack,
a rake claw on an ordinary attack, the same rake claw on a charge, and the
same rake claw against a foe the leopard has just grabbed through the
shared lifecycle.

The grab itself was the Sprint 6 pattern again, with each cat's own claw.
The lion's visual is a tint on the leopard rig, warm rather than spotted-
grey; a mane would need geometry the shared rig does not have, and the
charter asks for a lion visual, not a lion model.

## Sprint 8 - Big-Cat Roster Completion

There is no tiger in Kingmaker. The charter knew it and named the answer -
a striped albedo on the leopard rig scaled Large - and the Pteranodon had
already shown that a project texture on a private material survives
everything the game does to a summon's renderer. What the Pteranodon could
not answer was how to paint stripes onto a rig whose texture layout we do
not own and cannot copy. The answer turned out to be geometry: the mesh
knows where each vertex sits on the body, and it knows where each vertex
sits in its texture. Rasterizing the mesh's own triangles into a fresh
texture, colouring each by its position along the spine, produces stripes
that follow the animal rather than the atlas; the same generator with a
cellular field produces a cheetah's spots. Nothing of the game's art is
read - not one pixel - and the coat is private to the view, like every
variant since Sprint 5.

The tiger's numbers are the tabletop's, with one project weapon: the game
has no 1d8 claw, so the 1d6 claw animation is cloned with 1d8 dice, the way
the tail and bite weapons were made in earlier sprints. Its grab and rake
are the Sprint 7 pattern verbatim, which was the point of building that
pattern first.

The cheetah's sprint asked the question the profile had deferred since the
freeze: how to bound a once-per-hour tenfold speed burst inside a summoning.
The bound is the resource - one sprint per summoning - and the effect is a
one-round enhancement bonus to speed that the game caps on its own. It is a
smaller thing than the tabletop's, and it is exactly as repeatable as the
charter allows, which is not at all.

## 2026-09-25 - The correction order

The reviewer's order on the draft was exact and it was right: the labels
had said "complete" over a set of documented deviations that were not
accepted deviations at all. The cats grabbed with claws that the stat blocks
give to the bite; the rake was "charge or holding" without asking whether
the foe had been held since the round began, and the full attack still
carried the rake swings it merely refused to roll; grab ignored size; the
worm swallowed on the grab; the flytrap held one foe; the mephits had lost
four of their chartered roles to "no native spell"; the cyclops had no
armor and an automatic critical instead of a chosen roll; the web asked for
a Reflex save where the rules make a touch attack; the hooves were primary;
and every material a variant made lived as long as the process.

The corrections are all in the code now, and the shape of each is the same:
find the exact seam the rule needs, and refuse the adaptation that would
have been easier. Attack identity is the weapon entity's slot in the body,
never the blueprint the foreclaws and rake claws share. The rake's "held
since the round began" is the held state's own round counter, and the
sequencing seam is the game's own full-attack builder, so the rake is not
rolled-and-refused but never planned. Swallow and engulf are the maintain
check of a later turn used as though pinning, with the size limits the
Bestiary states. The flytrap's links are buffs on the targets that name the
flytrap and the bite that took them, which is what lets four of them save,
load and count on their own. The mephit roles are project abilities on the
Charisma-based DC the stat blocks give; the cloud is the native cloud area
cloned with an enemy-of-caster gate on every action, which is the only way
"ally-safe" can be a property of the effect rather than of where the AI
happened to put it. The cyclops's insight is the pre-rolled-result seam the
d20 rule already exposes, so the confirmation is the dice's. The web is the
projectile delivery the game's rays use, with the ray weapon, so the roll is
against touch AC. The hooves are one flag the game already honours. And
every clone, texture and controller instance a variant makes is now written
into an ownership record that the view's own destruction releases.

Two of the findings changed my own earlier records rather than the code
alone. The cyclops's natural armor had drifted to +9 in an attempt to reach
an armor class the stat block never states; it is +7 and the class is 19,
with the +4 armor as a fact. And the "charge-only rake" phrase, which every
document repeated, was retired for the rule as written.

Three engine facts came out of the shake-outs and are worth keeping. A
buff advances its round number only when one of its components asks for
rounds, so a held state without such a component never counts a round (the
grappled state now carries a no-op round component). The units inside an
area effect are read from the area's spatial grid, which the game's move
controller advances only for a unit that walked, so a unit placed by
translocation stays indexed where it spawned until it moves; the fixture
re-indexes a placed unit where it stands, as the game does for a unit that
moved. The working save's area has walls a few metres from the party,
so every fixture placement takes a compass point whose line of sight is
clear in the game's own sight geometry. The game ticks no mode controller
behind an active loading screen or loading process, so a scenario must
wait for those flags to clear before it expects an area effect, a buff or
the sleep list to advance on the game's own frames. And a summon is not
player faction: the game's own ally relation from a summon's side excludes
the party it fights for, so an ally-scoped effect a summon casts must key
on the enemy relation, which is exact from both sides.

The proof is the point, so the correction added two scenarios, both of
them spanning frames. The rules scenario exercises every corrected rule on
live units with the hostile's size, touch AC and saves set exactly; the wind
wall and the stinking cloud are found through the game's own grid on the
frames after their placement, and the web's projectile flies on world time
and makes its own attack roll, recorded by a global rulebook observer. The
lifecycle scenario waits for the game to destroy each view at the end of the
frame that disposed its unit, and counts the variant's objects back to
baseline after each cycle.

The first full run of the gate list on candidate `c6eb242a` found two more
defects, both in the mechanics rather than the fixtures. The engine's
combat-maneuver rule decides by the sum alone - d20 plus CMB against CMD,
with no automatic failure or success - so the worm's later-turn check with
a natural 1 and a hundred points of CMB maintained the hold and swallowed;
a combat maneuver check is an attack roll on the tabletop, so the summon
grapple's grab and maintain checks now apply the natural 1 (always fails)
and the natural 20 (always succeeds) over the engine's result
(`IsSummonManeuverSuccess`, pinned by the suite and the validator). And the
Cyclops's Flash of Insight state, armed for one round before the working
save, had lapsed by the time the reloaded cyclops attacked, so the reload
found no arming and the attack rolled its own 1. The tabletop ability is
chosen at the roll and never lapses unspent, so the arming now has no
duration of its own: the native `RemoveBuffOnAttack` alone ends it, one
use per summoning, one roll touched, present exactly once after the reload
(`CyclopsFlashOfInsightLastsUntilUsed`, pinned). The same pass retired the
last "automatic critical hit" wording, which the manifest note, the profile
comment and the inventory assertion text still carried, and isolated the
mechanical scenario's sub-cases from one another: each frees the hostile
of any hold or swallow the previous one left, and each exception keeps its
frames.
## 2026-09-26 - the owner accepts the persistence limitation

The last open question of the 2026-09-26 order was the one the engine
answered for us. A hold established before a save is not there after the
load: Kingmaker carries no active grapple across one, and it writes a unit
part on these summons by type without its contents, so neither the native
link nor the project's record of the establishing limb comes back. Four
reproducible observations established that, and the item went to the owner
as BLOCKED rather than as an accepted deviation.

The owner accepted the limitation and drew the line where it belongs: an
active grab, hold, swallow or engulf, the mouth occupancy that goes with it
and the held-target rake may be session-scoped, so long as a save and a
reload resolve to a clean released state - no lingering conditions, no
occupied mouths, no delayed damage, no dangling links, no unusable units and
no module-disabled deserialization problem. That is what the persistence
trio already proves. Re-establishing holds on load would be a persistence
subsystem of its own, chartered separately if it is ever worth the risk, and
is not in PR #23.

Recording it took the claims out before it put the decision in. Every
comment that called the store durable or said the limb and mouth mapping
survives a reload now says what the store actually is: the owner of the
establishing attack for the life of that hold, resolved against the holder's
body when it is read. The same correction ran through the state file, the
report, the fidelity matrix, the domain tests, the static record, the
changelog, the manifest note with its regenerated roster and the PR body,
and took with it the Giant Flytrap's stale 1d8 acid, the stale "nothing is
BLOCKED" wording, the malformed status lines and the 1799 domain-test count.
Nothing here is described as grapple persistence and nothing is left
BLOCKED.

The closeout changed no executable behaviour - its only non-comment edits
are record text, one profile note and test token names - so the live matrix
was not rerun. The four gates the decision named ran on the closeout head
`fd877587` and passed: the domain suite at 1806 tests with no failures,
repository and manifest and static validation, the exact-reference Release
build, and strict package validation. Draft PR #23 goes to the owner for
review and stays unmerged.
