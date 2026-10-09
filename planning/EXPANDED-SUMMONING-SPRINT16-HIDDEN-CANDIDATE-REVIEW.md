# Sprint 16 hidden candidate review

## Candidate 0238bf03 — FAIL, restored, NOT QUALIFIED

Source `0238bf03166dfb3dd7bf0205c12f82a2d2757caa`, version `0.0.141`.
Full repository wrapper, 2018/2018 domain tests, clean exact-reference Release,
and strict standalone/current-runtime package validation PASS. Package counts
312 with sound / 310 without. These checks did not establish runtime correctness.

| Artifact | SHA-256 / identity |
| --- | --- |
| Source state | `b7fd84b9605dd00d47b1ced84d03b720a74ec7fd08c0d5e8ff858cfe9add3431` |
| Runtime ZIP | `52786637dabfa8d563f92595080227522ee4b2de6a9a580f814d164339e97182` |
| DLL | `e8c528592bbea7e07306fb88b25b40d62b2ed1dd2a56c443522402daaa714934` |
| DLL MVID | `24e630dc-e3ba-4961-b96e-15214c68954c` |

One immutable artifact, one baseline snapshot/deployment, five guarded fresh
Steam App 640820 launches, and one exact final restoration. All used only
`KMG_AUTOMATION_WORKING`. The prepare and cleanup stages each refused their save
write; neither saved the incomplete fixture. The final absence PASS does not
qualify persistence because prepare failed.

Evidence directories are under `C:/Dev/KingmakerGunslingerLab/runtime-evidence/`.

| Run directory | Assertions PASS / FAIL | Result SHA-256 |
| --- | --- | --- |
| `20261005T1535350208461Z-disposable-expanded-summoning-crocodilians` | 55 / 19 | `1D76EC35BF80204428036242BE8199E41D68E36A4D490D9F4A538B7A16DBF4C3` |
| `20261005T1538431720124Z-working-save-expanded-summoning-creature-review` | 16 / 4 | `79736EEBE1D158EF5B5005BE042386394A54D9489CD6CE4175CC4F2276EC7D99` |
| `20261005T1546159564262Z-working-save-expanded-summoning-prepare` | 6 / 3 | `A5669B7725D3D5B86A8402229B1CE6116ECE6829040D32672255F67EF69AC4E3` |
| `20261005T1549174154097Z-working-save-expanded-summoning-verify-cleanup` | 5 / 4 | `8B5B7785E32A204379D34BBD6C88A0E5496DE64D83B4AE8A4A68B34301D26449` |
| `20261005T1552180746230Z-working-save-expanded-summoning-verify-absent` | 5 / 0 | `C07F57A8CF0D306541F225D60C188EF7E0A26F403E447E665C40996CF2709AEA` |

### What the failed artifact actually proved

- The first copy of each creature had the printed skills. Later copies did not.
- Sprint's 20→40→20, Haste ordering, Slow, penalties, caps, six-second active
  state and sixty-second cooldown/deadline cases passed. Real-command/AI proof
  was not reached.
- Three later swallowed rounds each delivered one owned `3d6+13` bludgeoning
  bundle; activation/deactivation did not duplicate it. Native six-second
  break-free timing/checks passed. Initial Swallow bite damage was wrong.
- The native type/graph census confirmed the already activated
  `OwnerAcceptedEngineLimitation: SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED`.
  This is not an open engine blocker and does not excuse the failed initial bite.
- Base-component identity survived an energy component moved ahead of it.
  Actual bite/Death Roll DR, material, enhancement and weapon attribution agreed;
  no second attack was introduced. The baseline positive Strength damage was
  wrong, so the combined bonus assertions still failed.
- Crowd native movement, original mesh attachment, expiry and exact project-owned
  resource cleanup passed for both creatures. Separate guided single-subject
  movement assertions failed. Native UI, combat contacts and the twenty route
  checks were not reached in the first scenario.

Two crowd images were inspected only as optional art support. Indoor walls and
crowding obscure some anatomy. They do not establish mechanical correctness or
owner visual approval: `HumanReview: NOT_PERFORMED_NONBLOCKING`.

### Evidence-backed corrections prepared after restoration

1. `IHandleEntityComponent.OnEntityCreated` runs on a shared blueprint component.
   Its single `m_Applied` flag incorrectly skipped every later crocodilian. Remove
   shared mutable creation state; allocate each new unit's exact ranks, reject
   preallocated ranks, and leave deserialization alone. Both the mechanics and
   two-copy persistence observations exposed this defect.
2. Native HP generation gave 26/150, not 22/138. Initialize only these creatures'
   racial-dice base HP to 13/54 after their class levels, retaining the native
   live Constitution dependency. No global class/HP rule is changed.
3. Native weapon stats give a lone primary-hand natural bite 1.5× Strength even
   with a secondary additional tail. Actual bites were `1d8+6` / `3d6+19`, and
   Death Rolls `1d8+8` / `3d6+25`. Use the native multiplier-override seam on the
   exact crocodilian bite to restore ordinary 1× Strength. Death Roll still uses
   that live bite and adds only the positive extra half.
4. Native NPC overridden weapon dice skip size scaling. An exact-owner/weapon
   component uses the native size table relative to Large/Gargantuan for these
   printed bite/tail dice; other existing dice overrides remain respected.
5. Prone immunity masked an added condition until removal. Do not add Prone when
   native `HasConditionImmunity(Prone)` is true; the fixture checks both during
   immunity and after its removal.
6. The secondary tail's `ItemEntityWeapon.IsSecondary` is authoritative;
   `RuleCalculateWeaponStats.SecondaryWeapon` is a separate override flag, not
   the live weapon role. Keep real attack/damage checks, fix the observation.
7. A lethal damage rule is followed by the native life-controller update. The
   synchronous fixture now ticks only its exact disposable victim's native life
   controller and the production hold path before checking cleanup. It records
   whether death had already settled on damage return; it never forces death.
8. The correction fixture read a nonexistent `State.AllUnits` reflection member.
   Its empty snapshots allowed it to dispose original area units and falsely
   pass cleanup, preventing the next fixture from finding the party's area.
   Use native `State.Units.All`, require a nonempty census containing the party,
   destroy only explicitly owned actors, and verify every original HoldingState
   reference and non-destroyed unit. This was in-memory fixture damage only;
   that scenario never wrote the working save. Historical single-fixture cleanup
   claims are not evidence of area preservation; no completed creature is
   redesigned on that basis.
9. The visual request included crocodilian ground assertions but omitted them
   from the existing guided movement measurement. Add exactly these two subjects
   to its awake/unpaused connected-floor measurement.

Focused 216/216 (2018 registered) and incremental exact-reference Release compile
PASS after these corrections. Corrected runtime proof remains pending. The next
candidate must rerun the affected gates; no failed assertion is waived.

### Restoration and unchanged publication

Snapshot: `runtime-backups/live-mod/20261005T1532081508820Z`.
Deployment: `deployments/20261005T1535349205786Z/deployment.json`.
Restoration record:
`expanded-summoning-restoration/20261005T1555173824556Z-disposable-expanded-summoning-crocodilians.json`.
Record SHA-256: `CF71CB42FDF6F082112A9B718937E7AE1BE8C487C1272C87C010ABBAE854B6F5`.

Actual post-batch tree: **136 files, Info 0.0.117**,
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
No Kingmaker process or compatibility lock remained; all six batch/restore
leases were Completed. No permanent deployment or publication occurred.
Surface stays 976 registered / 970 published / 6 Dire withheld / 29 wrappers /
999 visible choices. Sprints 14–15 remain accepted. Sprint 17 has not begun.
