# Sprint 58 Stunning Shot qualification

## Result

Stunning Shot is runtime-qualified on exact commit
`f5dc6bbc8f2443c54b1294b1815c000288b4ccea` and version `0.0.58`.

## Source gates

- Repository validation: PASS.
- Complete dependency-free domain suite: 819/819 PASS.
- Exact-reference clean Release build: PASS.
- Strict standalone package validation: PASS.
- Package SHA-256:
  `dd0b46951c3d8c963b5705c1a6ad999b379dbe75f81ee8467cc796cc7d7ef777`.
- DLL SHA-256:
  `58377a40be8084968bf1e7471875b80d47f1abfe8af41a79c8a1044f78ca045b`.

## Runtime evidence

- Exact mod-load PASS:
  `20260802T0726286857532Z-mod-load-smoke`.
- Independent feature PASS:
  `20260802T0727464837674Z-disposable-gunslinger-stunning-shot`.
- Independent feature PASS:
  `20260802T0729102463349Z-disposable-gunslinger-stunning-shot`.

Both feature runs observed level-19 availability, one chamber consumed, native
damage `0->3`, grit `4->2->0->2`, a deterministic natural-1 Fortitude failure
applying the exact Stunned clone for 6 seconds, a natural-20 success applying
no condition, native critical immunity consuming the marker without grit or
condition, all markers consumed, and exact detached-unit cleanup.

The scenarios were save-free and launched through Steam App ID 640820. No save
selection, save loading, save mutation, input automation, or direct executable
launch occurred.
