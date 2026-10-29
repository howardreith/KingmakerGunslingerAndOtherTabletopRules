# 0.0.170 - Satyr walking and Lillend presentation repair

- Fixes summoned Satyrs sliding with planted feet. Replacing the owner's private
  weapon animation set now releases only its own retired locomotion handle, so
  the ordinary native manager resumes walking in bow and dagger modes.
- Rebuilds Lillend Azata's visible anatomy, auburn hair, teal/ivory/gold clothing,
  layered wings and emerald serpent body on the existing rig. Sword grip, tail
  animation and contact timing retain their existing contracts.

Summon routes, quantities, costs, abilities, blueprint GUIDs and all 7,511 saved
identities remain unchanged. No new dependency or Sprint33 content is included.
The owner reviewed the repair gallery and approved merging and releasing it.

The repair's original development artifact passed 53/53 guarded native assertions,
9 focused domain cases and 3 Blender export checks. Native observations cover
both Satyr weapon gaits against native controls, attacks, Lillend movement,
sword/tail contact, actual healing and performance poses, wound/state presentation,
scheduled expiry and owned-resource cleanup. Protected installation/settings and
save intake were restored exactly after those runs.

The release publisher runs the complete source/build/package gate and two clean
deterministic exact-reference builds. Its version-stamped DLL differs from the
native-tested development DLL; those native results are reviewed reuse of the
same repaired product source, not fresh execution of the published binary.

Affected cold save/reload remains **NOT_RUN**. The earlier disclosed v0.0.167 and
v0.0.168 public-route, cold-load and final-integration gaps remain open. This
repair does not claim whole-roster/all-profile qualification or a played death
animation absent from the native donor. Existing gameplay adaptations and optional
Suggestion provider requirements from [v0.0.169](RELEASE-NOTES-0.0.169.md) remain.

See the [repair record](../planning/SATYR-LILLEND-PRESENTATION-REPAIR.md) for original
failure records, evidence identities and observation limits.
