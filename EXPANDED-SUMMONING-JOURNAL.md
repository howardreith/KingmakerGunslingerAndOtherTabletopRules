# Expanded Summoning journal

## 2026-08-11 - baseline and mission intake

- Intended repository confirmed at `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger` with remote `git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git`.
- Fetch completed. `origin/master` at `2894d9fcce250708e354894ffd8e1be9c7493b9b` is the newest qualified non-experimental descendant of required baseline `e4d560f8dd2909518614e3a20e77ba4d70dadeb8`.
- Created `codex/expanded-summoning` from that merged 0.0.77 baseline.
- Mandatory inspection proved `ShieldOtherLinkValidityPolicy` still removes an established bond outside close range. Separate prerequisite repair is next.
- GitHub CLI exists but its stored token is invalid. Work and SSH publication continue; draft-PR credential state will be rechecked after all local qualification.

## 2026-08-11 - Shield Other prerequisite qualified

- Removed all post-cast distance dependence from `ShieldOtherLinkValidityPolicy`.
  Close range remains on `ShieldOtherBlueprints` for initial targeting.
- Preserved missing/dead endpoint, missing caster-level, and area-separation
  termination. Existing removal/dispel and duration behavior is unchanged.
- Added focused extreme-distance and unavailable-distance regression cases plus
  a source contract forbidding reintroduction of established-link distance use.
- `git diff --check`: PASS.
- Repository validation: PASS for 0.0.77.
- Complete domain suite: 981/981 PASS.
- Clean Release build and strict standalone package: PASS.
- DLL SHA-256: `6cc7d0186f7b5d57b58644bffb2fc23c71feb898816bdea0da2acf63954f29b0`.
- Package SHA-256: `6d097f33e70cfce3364a015d9e59c541d14444cbaf55f082314d47e026f0d431`.
