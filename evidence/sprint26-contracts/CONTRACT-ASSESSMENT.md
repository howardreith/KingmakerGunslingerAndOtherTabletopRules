# Sprint 26 exact spatial-contract assessment

The exact private Kingmaker 2.1.7b assembly exposes a single usable native target query for this bounded burst:

```text
Kingmaker.Designers.GameHelper.GetTargetsAround(
  UnityEngine.Vector3 point,
  Kingmaker.Utility.Feet radius,
  bool checkLOS,
  bool includeDead)
```

Inspection of the installed method body establishes game-state unit enumeration, dead-unit exclusion, untargetable-unit exclusion, native `DistanceTo(Vector3)`, native unit corpulence, and optional line of sight. `Feet` converts to meters at 0.3048 per foot.

Selected contract: exact wielder position, exact firearm definition radius, `checkLOS=true`, `includeDead=false`. The exact wielder is also inserted explicitly once because self-inclusion is not delegated to query implementation details.

This evidence is compiler/research input only. The private assembly itself must not appear in any UMM or source package.
