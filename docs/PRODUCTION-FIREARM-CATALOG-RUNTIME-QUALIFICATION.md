# Production firearm catalog runtime qualification

Sprint 31 is runtime-qualified on exact commit `1539ae9`.

The production catalog registers distinct stable item/type pairs for Pistol,
Musket, and Blunderbuss. Pistol and Musket are player-fireable. Blunderbuss
retains the authoritative `special` range marker and one fail-closed equipment
restriction until Sprint 32 scatter execution is qualified. Native crossbows
remain presentation sources only.

## Source and package gates

- Repository validation: PASS.
- Dependency-free domain suite: 624/624 PASS.
- Exact private-reference clean Release compile: PASS.
- Strict standalone package validation: PASS.
- Package SHA-256:
  `0ca093bd05eaa19a6dc3e3577b618fea2b3db018b29e61965fc0742815e2c342`.
- DLL SHA-256:
  `c0e59abe94e89ec478a55c43327c8ce7763851dc1d50f4a141c39e7ad0767473`.

## Guarded runtime evidence

- `mod-load-smoke` PASS:
  `20260801T1334059331758Z-9736bc0a7d7844bd83bc9d26b5a30676`.
- Fresh-process catalog PASS 1:
  `20260801T1335276981327Z-5145ec8fbc864500889d489fb4c23fad`.
- Fresh-process catalog PASS 2:
  `20260801T1336546357107Z-1986affde5794aad8eb0710a31932eb0`.

Both feature runs used the guarded Steam App ID 640820 path and exact
`KMG_AUTOMATION_WORKING` descriptor correlation. All catalog assertions passed,
the fingerprint was stable, the baseline was distinct and never loaded, and no
save-writing API was observed.

## Remaining boundary

This evidence does not qualify scatter attacks, a starting-equipment grant, or
later acquisition. The Blunderbuss remains intentionally unavailable until the
Sprint 32 scatter path passes its own source and two-run runtime gates.
