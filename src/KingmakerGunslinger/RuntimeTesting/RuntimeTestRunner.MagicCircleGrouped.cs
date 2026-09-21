using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.UI.ActionBar;
using Kingmaker.Visual.Decals;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.MagicCircle;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static AbilityData CircleGroupedVariant(AbilityData parent, BlueprintAbility child)
        {
            // Request-local mechanical casts use the native selected-variant
            // data contract. The separate UI scenario proves real popup choices.
            // The installed CotW patches the direct spontaneous-slot helper with
            // a memorized-slot parameter; the player popup uses a different path.
            if (parent?.Spellbook == null || !parent.Blueprint.HasVariant(child))
                throw new InvalidOperationException("The chosen Circle is not in this spell family's native variant list.");
            return new AbilityData(child, parent.Spellbook) { ConvertedFrom = parent };
        }

        private static AbilityData CirclePreparedVariant(SpellSlot prepared, BlueprintAbility child)
        {
            var slot = new MechanicActionBarSlotMemorizedSpell { SpellSlot = prepared, Unit = prepared.Spell.Caster.Unit };
            return slot.GetConvertedAbilityData().Single(value => value.Blueprint == child);
        }

        private static bool CirclePublishedFamily(BlueprintSpellList list, BlueprintAbility expected)
        {
            var entries = list.SpellsByLevel.Single(level => level.SpellLevel == 3).Spells;
            var circles = BlueprintBootstrap.MagicCircles;
            return entries.Count(spell => MagicCircleBlueprints.Families.Contains(spell)) == (expected == null ? 0 : 1) &&
                (expected == null || entries.Count(spell => ReferenceEquals(spell, expected)) == 1) &&
                !entries.Any(spell => circles.Any(circle => ReferenceEquals(circle.Spell, spell))) &&
                circles.All(circle => circle.Spell.IsInSpellList(list) ==
                    (expected != null && expected.Variants.Contains(circle.Spell)));
        }

        private static bool CircleBoundaryMatches(AreaEffectEntityData area)
        {
            if (area?.View == null || area.IsEnded) return false;
            var rings = area.View.GetComponentsInChildren<MagicCircleRadiusVisual>(true);
            if (rings.Length != 1) return false;
            var ring = rings[0]; var decal = ring.Boundary; var material = decal.SharedMaterial;
            var circle = BlueprintBootstrap.MagicCircles.Single(value => ReferenceEquals(value.Area, area.Blueprint));
            var expectedColor = circle.Alignment == "Evil" ? new Color32(41, 158, 255, 224) :
                circle.Alignment == "Good" ? new Color32(255, 199, 46, 224) :
                circle.Alignment == "Chaos" ? new Color32(255, 46, 31, 224) : new Color32(196, 125, 255, 224);
            var scale = decal.transform.lossyScale;
            return ReferenceEquals(decal, ring) && decal.isActiveAndEnabled && ScreenSpaceDecal.All.Contains(decal) &&
                decal.Type == ScreenSpaceDecal.DecalType.GUI && material != null && material.shader.isSupported &&
                material.shader.name == "PF/Decals/GUIDecal" && material.mainTexture?.name == "Sector" && material.IsKeywordEnabled("CIRCLE_ON") &&
                material.GetFloat("_ZTest") == 7 && material.GetFloat("_CullMode") == 1 &&
                material.GetFloat("_GradientMode") == 1 && material.GetFloat("_ExpGradient") == 0 &&
                ring.AreaId == area.UniqueId && ring.Alignment == circle.Alignment &&
                CircleRenderedColor(decal.MaterialProperties.GetColor("_Color")).Equals(expectedColor) &&
                Math.Abs(ring.Radius - area.Blueprint.Size.Meters) < .001f &&
                Math.Abs(scale.x - 2 * area.Blueprint.Size.Meters) < .005f &&
                Math.Abs(scale.z - 2 * area.Blueprint.Size.Meters) < .005f &&
                Math.Abs(scale.y - MagicCircleRadiusVisual.ProjectionHeight) < .005f &&
                Vector3.Dot(decal.transform.up, Vector3.up) > .9999f &&
                (decal.transform.position - area.Position).sqrMagnitude < .001f;
        }

        private static Color32 CircleRenderedColor(Color color)
        {
            // Compare the same four family colors as the accepted renderer,
            // using nearest-byte rounding for Unity's floating color storage.
            return new Color32((byte)Mathf.RoundToInt(color.r * 255), (byte)Mathf.RoundToInt(color.g * 255),
                (byte)Mathf.RoundToInt(color.b * 255), (byte)Mathf.RoundToInt(color.a * 255));
        }

        private static bool CircleBoundaryGone(AreaEffectEntityData area)
        {
            return !Resources.FindObjectsOfTypeAll<MagicCircleRadiusVisual>().Any(ring => ring != null &&
                ring.AreaId == area.UniqueId && ring.isActiveAndEnabled) &&
                !ScreenSpaceDecal.All.OfType<MagicCircleRadiusVisual>().Any(ring => ring.AreaId == area.UniqueId);
        }

        private static string CircleBoundaryState(AreaEffectEntityData area)
        {
            var rings = area?.View?.GetComponentsInChildren<MagicCircleRadiusVisual>(true);
            var ring = rings?.FirstOrDefault();
            if (ring == null) return "rings=" + (rings == null ? -1 : rings.Length) + ";decal=absent";
            var material = ring.SharedMaterial; var color = ring.MaterialProperties.GetColor("_Color");
            return CircleUiState(new { rings = rings.Length, area.IsEnded, ring.enabled, ring.gameObject.activeInHierarchy,
                registered = ScreenSpaceDecal.All.Contains(ring), renderer = ring.GetInstanceID(), material = material?.GetInstanceID(),
                ring.Radius, ring.Alignment, expectedRadius = area.Blueprint.Size.Meters,
                scale = ring.transform.lossyScale, position = ring.transform.position, type = ring.Type.ToString(),
                color = new[] { color.r, color.g, color.b, color.a }, shader = material?.shader?.name,
                texture = material?.mainTexture?.name, zTest = material?.GetFloat("_ZTest"), cull = material?.GetFloat("_CullMode") })
                .ToString(Newtonsoft.Json.Formatting.None);
        }

        private IEnumerable<int> CircleChooseGroupedUi(AbilityData parent, BlueprintAbility chosen, Action<AbilityData> selected)
        {
            // The native spells popup materializes only the selected level's
            // rows. Navigate its real level tab before finding the family slot.
            const BindingFlags instanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var spellGroup = ActionBarManager.Instance.Group.GroupElements.Single(value =>
                (ActionBarSlotType)typeof(ActionBarGroupElement).GetField("SlotType", instanceFields).GetValue(value) == ActionBarSlotType.Spell);
            if (!spellGroup.ToggleState) spellGroup.OnClick();
            for (int frame = 0; frame < 20; frame++) yield return 0;
            var levelMenu = (ActionBarSubGroupLevels)typeof(ActionBarGroupElement).GetField("m_Levels", instanceFields).GetValue(spellGroup);
            levelMenu.OnClick(parent.SpellLevel);
            for (int frame = 0; frame < 20; frame++) yield return 0;
            Func<ActionBarGroupSlot, bool> isParent = row => {
                var rowData = row.MechanicSlot?.GetContentData() as AbilityData;
                return rowData?.Blueprint == parent.Blueprint && ReferenceEquals(rowData.Spellbook, parent.Spellbook);
            };
            var groups = ActionBarManager.Instance.Group.GroupElements.Where(value =>
                value.GetComponentsInChildren<ActionBarGroupSlot>(true).Any(isParent)).ToArray();
            if (groups.Length != 1) {
                CircleUiCapture("grouped-source-missing-" + chosen.name, new { parent = parent.Blueprint.AssetGuid,
                    book = parent.Spellbook.Blueprint.AssetGuid, known = parent.Spellbook.IsKnown(parent.Blueprint),
                    parent.Blueprint.Hidden, groups = groups.Length,
                    rows = ActionBarManager.Instance.GetComponentsInChildren<ActionBarGroupSlot>(true).Select(row => {
                        var value = row.MechanicSlot?.GetContentData() as AbilityData;
                        return new { row.name, active = row.gameObject.activeInHierarchy, spell = value?.Blueprint.AssetGuid,
                            book = value?.Spellbook?.Blueprint.AssetGuid }; }).ToArray() });
                throw new InvalidOperationException("Native family source group count=" + groups.Length + ";parent=" + parent.Blueprint.AssetGuid);
            }
            var group = groups[0];
            if (!group.ToggleState) group.OnClick();
            for (int frame = 0; frame < 20; frame++) yield return 0;
            var source = group.GetComponentsInChildren<ActionBarGroupSlot>(true).Single(row => row.gameObject.activeInHierarchy && isParent(row));
            source.OnToggleGroupClick();
            for (int frame = 0; frame < 20; frame++) yield return 0;
            var popup = (ActionBarSpellsGroup)typeof(ActionBarGroupSlot).GetField("SubGroup", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(source);
            if (popup == null) throw new InvalidOperationException("Native variant popup was not created by its source slot.");
            var rows = popup.GetComponentsInChildren<ActionBarSlot>(true).Where(row => row.gameObject.activeInHierarchy &&
                (row.MechanicSlot?.GetContentData() as AbilityData)?.ConvertedFrom?.Blueprint == parent.Blueprint).ToArray();
            var data = rows.Select(row => row.MechanicSlot.GetContentData() as AbilityData).ToArray();
            bool exact = rows.Length == parent.Blueprint.Variants.Length &&
                parent.Blueprint.Variants.All(variant => data.Count(value => value.Blueprint == variant) == 1) &&
                rows.All(row => ReferenceEquals(row.Icon.sprite, (row.MechanicSlot.GetContentData() as AbilityData).Blueprint.Icon));
            CircleUiCapture("grouped-variants-" + chosen.name, new { parent = parent.Blueprint.AssetGuid,
                variants = data.Select(value => new { id = value.Blueprint.AssetGuid, level = value.SpellLevel, icon = value.Icon?.name }).ToArray() });
            CircleUiAssert("grouped-variants-" + chosen.name, "native family popup shows exactly the legal children and their matching icons",
                "rows=" + rows.Length, exact);
            foreach (int frame in _circleUiScreens.Capture("grouped-variants-" + chosen.name,
                CircleUiState(new { parent = parent.Blueprint.AssetGuid, chosen = chosen.AssetGuid, count = rows.Length }),
                () => exact && popup.gameObject.activeInHierarchy && !_workingSaveSmoke.WriteObserved)) yield return frame;
            rows.Single(row => (row.MechanicSlot.GetContentData() as AbilityData)?.Blueprint == chosen).OnClick();
            for (int frame = 0; frame < 6; frame++) yield return 0;
            var actual = Game.Instance.SelectedAbilityHandler.Ability;
            if (actual?.Blueprint != chosen || actual.ConvertedFrom?.Blueprint != parent.Blueprint)
                throw new InvalidOperationException("The native variant click did not select the requested family child.");
            CircleUiAssert("grouped-choice-" + chosen.name, "native variant click selects its child and retains the prepared family's resource link",
                actual.Blueprint.AssetGuid, actual.SpellLevel == parent.SpellLevel && ReferenceEquals(actual.Spellbook, parent.Spellbook));
            selected(actual);
            source.HideSubGroup();
            if (group.ToggleState) group.OnClick();
        }

        private static void CircleBoundaryLifecycle(AreaEffectEntityData area, UnitEntityData bearer,
            IList<UnitEntityData> actors, List<RuntimeTestAssertion> assertions)
        {
            var ring = area.View.GetComponentInChildren<MagicCircleRadiusVisual>(true);
            int id = ring == null ? 0 : ring.GetInstanceID();
            var before = bearer.Position;
            bool appeared = CircleBoundaryMatches(area);
            string initial = CircleBoundaryState(area);
            bearer.Position += new Vector3(1.5f, 0, .5f); CircleRefresh(area, actors);
            bool moved = CircleBoundaryMatches(area) && (area.Position - bearer.Position).sqrMagnitude < .001f &&
                area.View.GetComponentInChildren<MagicCircleRadiusVisual>(true)?.GetInstanceID() == id;
            bearer.Position = before; CircleRefresh(area, actors);
            assertions.Add(Assertion("circle-visible-boundary-" + area.Blueprint.AssetGuid,
                "one colored 10-foot boundary follows the native area without recreation", "renderer=" + id +
                    ";appeared=" + appeared + ";moved=" + moved + ";initial=" + initial + ";final=" + CircleBoundaryState(area),
                appeared && moved && CircleBoundaryMatches(area),
                "actual native ScreenSpaceDecal volume/material/color/registration/ownership before and after bearer movement; framebuffer inspection is separate visual evidence"));
        }
    }
}
