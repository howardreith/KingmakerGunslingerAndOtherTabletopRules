using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class TeleportationSpellBlueprintSet
    {
        internal TeleportationSpellBlueprintSet(BlueprintAbility teleport, BlueprintAbility greater, BlueprintAbility recall)
        { Teleport = teleport; GreaterTeleport = greater; WordOfRecall = recall; }
        internal BlueprintAbility Teleport { get; private set; }
        internal BlueprintAbility GreaterTeleport { get; private set; }
        internal BlueprintAbility WordOfRecall { get; private set; }
        internal BlueprintAbility Get(TeleportSpellKind kind)
        {
            switch (kind) {
                case TeleportSpellKind.Teleport: return Teleport;
                case TeleportSpellKind.GreaterTeleport: return GreaterTeleport;
                case TeleportSpellKind.WordOfRecall: return WordOfRecall;
                default: throw new ArgumentOutOfRangeException("kind");
            }
        }
    }

    internal static class TeleportationSpellBlueprints
    {
        internal const int IdentityCount = 3;
        internal const string TeleportSymbol = "KMG.Spells.Teleport.Ability";
        internal const string GreaterTeleportSymbol = "KMG.Spells.GreaterTeleport.Ability";
        internal const string WordOfRecallSymbol = "KMG.Spells.WordOfRecall.Ability";
        internal const string DimensionDoorId = "5bdc37e4acfa209408334326076a43bc";

        internal static TeleportationSpellBlueprintSet Register(LibraryScriptableObject library, BlueprintRegistry registry)
        {
            var donor = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, DimensionDoorId, "native Dimension Door icon/presentation donor");
            if (donor.Icon == null) throw new InvalidOperationException("Dimension Door has no native icon.");
            var teleport = registry.Register<BlueprintAbility>(TeleportSymbol, () => Create(donor, "Teleport",
                "While on the world map, select a previously visited location and choose Teleport from its destination actions. Familiarity determines whether you arrive exactly, arrive elsewhere, or suffer a mishap. Only currently available prepared uses or spell slots can be spent. Travel takes no world-map time and never enters the location's local area."));
            var greater = registry.Register<BlueprintAbility>(GreaterTeleportSymbol, () => Create(donor, "Greater Teleport",
                "Also known as Teleport Without Error. While on the world map, select a previously visited location and choose Greater Teleport from its destination actions. You arrive exactly at that world-map point. This consumes one available prepared use or spell slot, takes no travel time, and never enters the location's local area."));
            var recall = registry.Register<BlueprintAbility>(WordOfRecallSymbol, () => Create(donor, "Word of Recall",
                "While on the world map, select Oleg's Trading Post before establishing your capital, or select your capital afterward, and choose Word of Recall from its destination actions. You arrive exactly at that world-map point. This consumes one available prepared use or spell slot, takes no travel time, and never enters the location's local area."));
            var result = new TeleportationSpellBlueprintSet(teleport, greater, recall);
            foreach (var spell in new[] { teleport, greater, recall }) Validate(spell);
            if (new[] { teleport.AssetGuid, greater.AssetGuid, recall.AssetGuid }.Distinct(StringComparer.Ordinal).Count() != IdentityCount)
                throw new InvalidOperationException("Strategic spell identities must be distinct.");
            return result;
        }

        private static BlueprintAbility Create(BlueprintAbility donor, string displayName, string description)
        {
            string key = displayName.Replace(" ", string.Empty);
            var ability = BlueprintCloneService.Clone(donor, "KMG_" + key + "_Ability");
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create("KMG.Teleportation." + key + ".Name", displayName),
                LocalizationService.Create("KMG.Teleportation." + key + ".Description", description), donor.Icon);
            ability.Parent = null;
            ability.Type = AbilityType.Spell;
            ability.Range = AbilityRange.Personal;
            ability.CanTargetPoint = false;
            ability.CanTargetEnemies = false;
            ability.CanTargetFriends = false;
            // The native scroll activation boundary gives scroll abilities a
            // Personal anchor targeting the reader; self-targeting must be
            // allowed or a genuine scroll can never be activated. The world-map
            // caster checker still forbids every local use.
            ability.CanTargetSelf = true;
            ability.SpellResistance = false;
            ability.ActionBarAutoFillIgnored = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.None;
            ability.ActionType = UnitCommand.CommandType.Standard;
            ability.AvailableMetamagic = 0;
            ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            ability.LocalizedDuration = LocalizationService.Create("KMG.Teleportation.Instantaneous", "Instantaneous");
            ability.LocalizedSavingThrow = LocalizationService.Create("KMG.Teleportation.NoSavingThrow", "None");
            var school = ScriptableObject.CreateInstance<SpellComponent>();
            school.name = "$KMG_" + key + "_Conjuration";
            school.School = SpellSchool.Conjuration;
            var checker = ScriptableObject.CreateInstance<TeleportationWorldMapCasterChecker>();
            checker.name = "$KMG_" + key + "_WorldMapOnly";
            // No copied Dimension Door delivery, variants, targets, resources, or local effects.
            ability.ComponentsArray = new BlueprintComponent[] { school, checker };
            return ability;
        }

        // Native/compatible spell-list finalization appends SpellListComponent metadata.
        // Its exact native type contains only SpellList and SpellLevel; it has no
        // delivery/effect implementation. Every other extra component remains rejected.
        internal static void Validate(BlueprintAbility ability)
        {
            if (ability == null || ability.Parent != null || ability.Type != AbilityType.Spell || ability.Hidden || ability.Icon == null ||
                ability.ActionType != UnitCommand.CommandType.Standard || ability.MaterialComponent == null ||
                ability.SpellResistance || ability.AvailableMetamagic != 0 || !ability.ActionBarAutoFillIgnored ||
                ability.CanTargetPoint || ability.CanTargetEnemies || ability.CanTargetFriends || ability.CanTargetSelf ||
                ability.ComponentsArray.Any(value => value == null ||
                    (value.GetType() != typeof(SpellComponent) && value.GetType() != typeof(TeleportationWorldMapCasterChecker) &&
                     value.GetType() != typeof(SpellListComponent))) ||
                ability.ComponentsArray.OfType<SpellComponent>().Count(value => value.School == SpellSchool.Conjuration) != 1 ||
                ability.ComponentsArray.OfType<TeleportationWorldMapCasterChecker>().Count() != 1)
                throw new InvalidOperationException("Strategic spell blueprint contract failed: " + (ability == null ? "null" :
                    "id=" + ability.AssetGuid + ";parent=" + (ability.Parent == null ? "none" : ability.Parent.AssetGuid) +
                    ";type=" + ability.Type + ";hidden=" + ability.Hidden + ";icon=" + (ability.Icon != null) +
                    ";action=" + ability.ActionType + ";material=" + (ability.MaterialComponent != null) +
                    ";sr=" + ability.SpellResistance + ";metamagic=" + ability.AvailableMetamagic +
                    ";barIgnored=" + ability.ActionBarAutoFillIgnored + ";targets=" + ability.CanTargetPoint + "," + ability.CanTargetEnemies + "," + ability.CanTargetFriends + "," + ability.CanTargetSelf +
                    ";components=" + string.Join(",", ability.ComponentsArray.Select(value => value == null ? "null" : value.GetType().FullName))));
        }
    }

    // Availability is positive only in the strategic context. The contextual runtime
    // additionally revalidates all cast-state, source, destination and transaction gates.
    internal sealed class TeleportationWorldMapCasterChecker : BlueprintComponent, IAbilityCasterChecker
    {
        public bool CorrectCaster(UnitEntityData caster)
        {
            return caster != null && Game.Instance != null && Game.Instance.CurrentMode == GameModeType.GlobalMap &&
                BlueprintBootstrap.TeleportationPublication != null;
        }
        public string GetReason()
        { return LocalizationService.Create("KMG.Teleportation.UseDestinationActions", "Select an eligible world-map destination to use this spell."); }
    }
}
