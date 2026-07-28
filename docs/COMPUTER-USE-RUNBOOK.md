# Computer Use runbook

> Windows 10 restriction: do not use this runbook to qualify autonomous
> runtime assertions. Use the guarded request/result protocol documented in
> `WIN10-AUTONOMOUS-RUNTIME-TESTING.md`. Computer Use may not infer state from
> an unavailable Kingmaker capture surface.

Only after an explicitly authorized deployment:

1. Launch Steam and Kingmaker through the normal supported local mechanism.
2. Handle only routine, previously known launch behavior.
3. Reach the main menu.
4. Open the Unity Mod Manager overlay.
5. Confirm Kingmaker Gunslinger version 0.0.30 and green/loaded status.
6. Load only the explicitly named working copy of the verified test save.
7. Execute exactly one documented Sprint 30 scenario.
8. Capture the required version, state, matrix, resource, identity, fault, and
   persistence evidence.
9. Exit Kingmaker normally.
10. Run the evidence collector with explicit log and screenshot paths.

Stop immediately and report Ambiguous on Steam credential prompts, purchases,
cloud-save conflicts, unexpected installations or updates, the wrong mod
version, an unidentified save, unexpected campaign state, missing test
prerequisites, or ambiguous results. Do not click through or resolve these.

Unexpected dialogs also require stopping. Routine UI interaction is limited to
two attempts per intended control. There is no GUI automation, credential
entry, broad image search, save modification, forced process termination, or
fallback to another campaign.
