# Sprint 16 pre-candidate review - Crocodile and Dire Crocodile

Written while the Sprint 15 guarded batch ran, from source reading only. The
point of doing it here is the standing rule that the adversarial rules and
architecture review happens *before* the expensive candidate, so a
misunderstood signature is not discovered by a runtime matrix.

## Findings that change the implementation

### 1. Replacing the Crocodile's proxy touches no identity

`SummonCreatureSpec.Visual` defaults to the display name and is consumed in
exactly one place: `ExpandedSummoningBaselineInventory.ProxyVisualCreatures`,
which counts an entry as a proxy only when `Visual` differs from
`DisplayName`. Placements and GUIDs come from the append-only ledger and are
untouched by it.

So replacing the proxy is: drop the trailing `"Monitor Lizard"` argument from
`C("crocodile","Crocodile",3,true,3,"Monitor Lizard")` in
`ExpandedSummoningCatalog`, add the creature to the view patch's key map, the
asset runtime's dictionary, the bone policy, the view scale catalog and the
four shipping lists. The Crocodile's identity, placements and GUIDs are
preserved by construction, which is what the mission requires. The proxy count
in the inventory census moves by one and is pinned in tests.

`ExpandedSummoningDonorCatalog` keeps its Monitor Lizard row: that row names
the *donor rig*, which the project mesh still binds against, not the borrowed
appearance. The Monitor Lizard is also the right rig to keep - a low, wide,
four-legged body with a long tail is a crocodile's shape already, so the work
is a new mesh on a sound donor rather than a donor hunt.

### 2. Death roll has an exact native carrier; no barrier

Death Roll (Ex): while grappling a foe of its own size or smaller, on a
successful grapple check it deals its bite damage, knocks the target prone,
and keeps the grapple.

Every piece exists:
- the grapple check and the hold's maintenance are the Sprint 6 carrier,
  already qualified, and `ShouldSwallowOnMaintain` shows the shape of a
  maintain-time rider;
- prone is `UnitCondition.Prone` via `UnitDescriptor.State.AddCondition`,
  which this project already drives in `TwinShotKnockdownMechanics` - and that
  code verifies the engine accepted the condition rather than assuming it,
  which is the pattern to copy;
- the size restriction has a precedent in `IsSwallowSizeAllowed` /
  `ExpandedSummoningSpecialProfiles.IsSwallowSizeAllowed(targetSize,
  holderSize, delta)`; death roll's delta is 0 (own size or smaller) against
  the Purple Worm's -1.

The two risks worth instrumenting are named in the mission and are both about
cadence rather than capability: no duplicate damage when a round is processed
twice or an event replays, and no death roll without an exact valid held
target. Both have direct precedents - the Sprint 12 disease exercise replays a
runtime component to prove one resolution, and the rake/maintain work already
proves hold-target identity.

### 3. Sprint (Ex) has a native carrier

Once per minute, land speed rises to 40 feet for one round. `StatType.Speed`
takes stat bonuses and the project already writes them
(`ElementalAlternateTrait*` adds +5 Speed; the Elven Branched Spear subtracts
10). So this is a one-round +20 ft bonus with a once-per-minute resource, not
a new subsystem. It must not be a permanent speed increase.

### 4. Hold breath has no consumer and should be omitted with a reason

Rounds equal to four times Constitution, which only matters while submerged.
The mission forbids an underwater movement subsystem and restricts the
creature to a land adaptation, and Kingmaker models neither swimming nor
drowning, so nothing in the rules layer could consult it. This is the
luminescence case exactly: omit it, record the reason on the creature, and
never claim it works. It is not a candidate for the passive-sense label, which
covers only Scent, Darkvision and Low-light Vision.

The swim speed goes the same way as the ants' climb speed: Kingmaker exposes
one movement speed, the 20 ft land speed is used, and the 30 ft swim is
omitted.

### 5. The tail slap must be secondary, and the numbers say so

Crocodile: bite +5 (1d8+4) and tail slap +0 (1d12+2). The five-point gap and
the halved Strength bonus on the tail are a secondary natural attack, so the
tail belongs in `AdditionalSecondaryLimbs` via the `PS()` profile helper, not
in `AdditionalLimbs`. This is the exact mistake the owner rejected in Phase 1
when the Pony and Horse carried hooves as primary attacks, so it gets an
assertion that reads both limbs' live attack bonuses and requires the gap.

Dire Crocodile: bite +18 (3d6+13) and tail slap +13 (4d8+6) - the same
five-point gap, the same halved bonus, the same conclusion.

### 6. Reach

Crocodile is Large with Space 10 ft and Reach 5 ft, so it needs the
reduced-reach carrier the Large ungulates and the Giant Stag Beetle use. Dire
Crocodile is Gargantuan with Space 20 ft and Reach 15 ft, which is the
standard footprint for that size, so it must *not* carry reduced reach.

### 7. Swallow whole is the Purple Worm's carrier at a different size delta

Dire Crocodile: swallow whole 3d6+13, AC 16, 13 hp. Qualified carrier exists.
What is new is a second creature using it, so the size delta, the interior AC
and hit points have to come from the creature rather than from a Purple Worm
constant - the same class of defect as the Drone's poison difficulty class,
which the Sprint 15 pack proves is computed and not copied.

## Numbers to derive rather than assert

Crocodile CMD 18 (22 vs trip) and Dire Crocodile CMD 36 (40 vs trip). The
four-point trip difference on both is a four-legged trip defence, not the
eight-legged one the insects use: `TripDefenseFourLegs` already exists in the
fact table.

## Sequence

1. Freeze nothing new - the contract above is already frozen; this review adds
   no creature facts, only implementation decisions.
2. Death roll first, as a vertical slice on the Crocodile alone, before the
   Dire Crocodile is authored against it.
3. Meshes offline, one review sheet, then one sprint candidate and one batched
   guarded review.

---

# Sprint 17 donor survey - done here because it is tranche research

The mission requires the serpentine bind-frame proof before all three Sprint 17
visuals are committed to one implementation, and explicitly allows the
Salamander to be split off if it cannot share the snake seam. The donor
catalog, read offline, already answers the second question and narrows the
first.

The catalog's only limbless serpentine body is the **Purple Worm**
(`bf2216f48b3f4d24c9c502007649340d`). The Giant Centipede is a segmented body
with many legs, the Monitor Lizard is a quadruped, and nothing else in the
roster has a snake's topology. So the Viper and the Constrictor Snake should
both be authored against the Purple Worm's bind frame, and the proof to run is
that frame's capture plus one minimal vertical slice at each end of the size
range - a Tiny or Small Viper and a Medium Constrictor on a Gargantuan donor's
rig is a large rescale, which is precisely the kind of hypothesis the standing
rule says must survive a slice before a family is authored against it.

The **Salamander should be split off now rather than after a failed
experiment**. Its printed body is a humanoid upper half on a serpentine lower
half, it wields weapons, and its existing identity already ships wearing the
Lizardfolk (`e8276e28b2234a745900fed80670bfdb`), which is a humanoid rig with
arms that can hold a weapon. A Purple Worm frame has no arms at all, so forcing
one rig across all three would cost the Salamander its weapon handling - the
compromised universal rig the mission forbids. Keeping the Lizardfolk donor and
replacing only the mesh gives the serpentine lower body while preserving the
weapon seam, and it is also the smaller change to an identity that already
publishes.

That makes Sprint 17 two seams: the Purple Worm frame for the two snakes, and
the Salamander's existing humanoid donor with a new mesh. Recording the
decision now means the bind-frame proof only has to answer the snake question.
