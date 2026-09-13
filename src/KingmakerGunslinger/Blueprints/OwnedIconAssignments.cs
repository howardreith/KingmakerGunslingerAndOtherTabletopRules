using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace KingmakerGunslinger.Blueprints
{
    // Exact owned presentation only. The authoring catalog validates this table;
    // it is never read by the running mod. No recursive or native-donor mapping.
    internal static class OwnedIconAssignments
    {
        internal sealed class Binding
        {
            internal Binding(string symbol, string key, Type type)
            { Symbol = symbol; Key = key; BlueprintType = type; }
            internal string Symbol { get; private set; }
            internal string Key { get; private set; }
            internal Type BlueprintType { get; private set; }
            internal bool IsStrategic => Symbol.StartsWith("KMG.Spells.", StringComparison.Ordinal);

            internal BlueprintScriptableObject Resolve(LibraryScriptableObject library,
                BlueprintManifest manifest)
            {
                string guid = manifest.ResolveActive(Symbol, BlueprintType).Id.Value;
                BlueprintScriptableObject value;
                if (!library.BlueprintsByAssetId.TryGetValue(guid, out value) ||
                    value == null || value.AssetGuid != guid || value.GetType() != BlueprintType)
                    throw new InvalidOperationException("Missing exact owned icon consumer: " + Symbol);
                return value;
            }
        }

        private static readonly Binding[] Entries = {
            new Binding("KMG.ElementalRaces.Ifrit.Race", "general-ifrit", typeof(BlueprintRace)),
            new Binding("KMG.ElementalRaces.Ifrit.FireResistance", "fire-resistance", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Ifrit.FireAffinity", "fire-affinity", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Oread.Race", "general-oread", typeof(BlueprintRace)),
            new Binding("KMG.ElementalRaces.Oread.AcidResistance", "acid-resistance", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Oread.AcidAffinity", "acid-affinity", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Sylph.Race", "general-sylph", typeof(BlueprintRace)),
            new Binding("KMG.ElementalRaces.Sylph.ElectricityResistance", "electricity-resistance", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Sylph.AirAffinity", "air-affinity", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Undine.Race", "general-undine", typeof(BlueprintRace)),
            new Binding("KMG.ElementalRaces.Undine.ColdResistance", "cold-resistance", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Undine.WaterAffinity", "water-affinity", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Undine.HydraulicPushFeature", "hydraulic-push", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Undine.HydraulicPushAbility", "hydraulic-push", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Ifrit.HeritageSelection", "general-ifrit", typeof(BlueprintFeatureSelection)),
            new Binding("KMG.ElementalRaces.Oread.HeritageSelection", "general-oread", typeof(BlueprintFeatureSelection)),
            new Binding("KMG.ElementalRaces.Sylph.HeritageSelection", "general-sylph", typeof(BlueprintFeatureSelection)),
            new Binding("KMG.ElementalRaces.Undine.HeritageSelection", "general-undine", typeof(BlueprintFeatureSelection)),
            new Binding("KMG.ElementalRaces.Ifrit.Heritage.General", "general-ifrit", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Ifrit.Heritage.Lavasoul", "lavasoul", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Ifrit.Heritage.Sunsoul", "sunsoul", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Oread.Heritage.General", "general-oread", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Oread.Heritage.Gemsoul", "gemsoul", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Oread.Heritage.Ironsoul", "ironsoul", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Sylph.Heritage.General", "general-sylph", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Sylph.Heritage.Smokesoul", "smokesoul", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Sylph.Heritage.Stormsoul", "stormsoul", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Undine.Heritage.General", "general-undine", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Undine.Heritage.Mistsoul", "mistsoul", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Undine.Heritage.Rimesoul", "rimesoul", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Ifrit.Lavasoul.MagmaAffinity", "magma-affinity", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Ifrit.Sunsoul.SolarAffinity", "solar-affinity", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Oread.Gemsoul.CrystalAffinity", "crystal-affinity", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Oread.Ironsoul.MetalAffinity", "metal-affinity", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Sylph.Smokesoul.SmokeAffinity", "smoke-affinity", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Sylph.Stormsoul.LightningAffinity", "lightning-affinity", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Undine.Mistsoul.MistAffinity", "mist-affinity", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Undine.Rimesoul.IceAffinity", "ice-affinity", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Oread.Ironsoul.UnerringWeaponFeature", "unerring-weapon", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Oread.Ironsoul.UnerringWeaponAbility", "unerring-weapon", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Undine.Rimesoul.ChillTouchFeature", "chill-touch", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Undine.Rimesoul.ChillTouchAbility", "chill-touch", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Oread.Ironsoul.UnerringWeaponPrimaryAbility", "unerring-weapon-primary", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Oread.Ironsoul.UnerringWeaponSecondaryAbility", "unerring-weapon-secondary", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Undine.Rimesoul.ChillTouchDeliveryAbility", "chill-touch", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Feats.ElementalStrike", "elemental-strike", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Feats.ScorchingWeapons", "scorching-weapons", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Feats.InnerFlame", "inner-flame", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Feats.BlazingAura", "blazing-aura", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Feats.Firesight", "firesight", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Feats.AiryStep", "airy-step", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Feats.WingsOfAir", "wings-of-air", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Feats.CloudGazer", "cloud-gazer", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Feats.InnerBreath", "inner-breath", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Feats.HydraulicManeuver", "hydraulic-maneuver", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Feats.TritonPortal", "triton-portal", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Feats.ElementalStrike.Ability", "elemental-strike", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Feats.ElementalStrike.Buff", "elemental-strike", typeof(BlueprintBuff)),
            new Binding("KMG.ElementalRaces.Feats.ScorchingWeapons.Ability", "scorching-weapons", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Feats.ScorchingWeapons.Buff", "scorching-weapons", typeof(BlueprintBuff)),
            new Binding("KMG.ElementalRaces.Feats.BlazingAura.Ability", "blazing-aura", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Feats.BlazingAura.Buff", "blazing-aura", typeof(BlueprintBuff)),
            new Binding("KMG.ElementalRaces.Feats.WingsOfAir.Buff", "wings-of-air", typeof(BlueprintBuff)),
            new Binding("KMG.ElementalRaces.Feats.HydraulicManeuver.Ability", "hydraulic-maneuver", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Feats.HydraulicManeuver.BullRushAbility", "hydraulic-bull-rush", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Feats.HydraulicManeuver.DisarmAbility", "hydraulic-disarm", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Feats.HydraulicManeuver.TripAbility", "hydraulic-trip", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Feats.HydraulicManeuver.DirtyTrickBlindAbility", "hydraulic-blind", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Feats.TritonPortal.Ability", "triton-portal", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Traits.Ifrit.EnergyResistanceSelection", "ifrit-energy-resistance-selection", typeof(BlueprintFeatureSelection)),
            new Binding("KMG.ElementalRaces.Traits.Ifrit.ElementalAffinitySelection", "ifrit-elemental-affinity-selection", typeof(BlueprintFeatureSelection)),
            new Binding("KMG.ElementalRaces.Traits.Ifrit.RacialSpellLikeAbilitySelection", "ifrit-racial-spell-like-ability-selection", typeof(BlueprintFeatureSelection)),
            new Binding("KMG.ElementalRaces.Traits.Oread.EnergyResistanceSelection", "oread-energy-resistance-selection", typeof(BlueprintFeatureSelection)),
            new Binding("KMG.ElementalRaces.Traits.Oread.ElementalAffinitySelection", "oread-elemental-affinity-selection", typeof(BlueprintFeatureSelection)),
            new Binding("KMG.ElementalRaces.Traits.Oread.RacialSpellLikeAbilitySelection", "oread-racial-spell-like-ability-selection", typeof(BlueprintFeatureSelection)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.EnergyResistanceSelection", "sylph-energy-resistance-selection", typeof(BlueprintFeatureSelection)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.ElementalAffinitySelection", "sylph-elemental-affinity-selection", typeof(BlueprintFeatureSelection)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.RacialSpellLikeAbilitySelection", "sylph-racial-spell-like-ability-selection", typeof(BlueprintFeatureSelection)),
            new Binding("KMG.ElementalRaces.Traits.Undine.RacialSpellLikeAbilitySelection", "undine-racial-spell-like-ability-selection", typeof(BlueprintFeatureSelection)),
            new Binding("KMG.ElementalRaces.Traits.Ifrit.EnergyResistance.Retain", "ifrit-energy-resistance-selection", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Ifrit.ElementalAffinity.Retain", "ifrit-elemental-affinity-selection", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Ifrit.RacialSpellLikeAbility.Retain", "ifrit-racial-spell-like-ability-selection", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Oread.EnergyResistance.Retain", "oread-energy-resistance-selection", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Oread.ElementalAffinity.Retain", "oread-elemental-affinity-selection", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Oread.RacialSpellLikeAbility.Retain", "oread-racial-spell-like-ability-selection", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.EnergyResistance.Retain", "sylph-energy-resistance-selection", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.ElementalAffinity.Retain", "sylph-elemental-affinity-selection", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.RacialSpellLikeAbility.Retain", "sylph-racial-spell-like-ability-selection", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Undine.RacialSpellLikeAbility.Retain", "undine-racial-spell-like-ability-selection", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Ifrit.WildfireHeart", "wildfire-heart", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Ifrit.BrazenFlame", "brazen-flame", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Ifrit.FireInTheBlood", "fire-in-the-blood", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Ifrit.EfreetiMagic", "efreeti-magic", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Ifrit.ForgeHardened", "forge-hardened", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Ifrit.FireInsight", "fire-insight", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Oread.CrystallineForm", "crystalline-form", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Oread.EarthInsight", "earth-insight", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Oread.GraniteSkin", "granite-skin", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Oread.StoneInTheBlood", "stone-in-the-blood", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Oread.TreacherousEarth", "treacherous-earth", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.AirInsight", "air-insight", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.BreezeKissed", "breeze-kissed", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.LikeTheWind", "like-the-wind", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.Secretive", "secretive", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.StormInTheBlood", "storm-in-the-blood", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.ThunderousResilience", "thunderous-resilience", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.WhisperingWind", "whispering-wind", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Undine.AcidBreath", "acid-breath", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Undine.NereidFascination", "nereid-fascination", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Undine.OozeBreath", "ooze-breath", typeof(BlueprintFeature)),
            new Binding("KMG.ElementalRaces.Traits.Ifrit.FireInTheBlood.FastHealingBuff", "fire-in-the-blood", typeof(BlueprintBuff)),
            new Binding("KMG.ElementalRaces.Traits.Oread.StoneInTheBlood.FastHealingBuff", "stone-in-the-blood", typeof(BlueprintBuff)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.StormInTheBlood.FastHealingBuff", "storm-in-the-blood", typeof(BlueprintBuff)),
            new Binding("KMG.ElementalRaces.Traits.Ifrit.EfreetiMagic.Ability", "efreeti-magic", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Traits.Oread.CrystallineForm.ArmedBuff", "deflect-next-ray", typeof(BlueprintBuff)),
            new Binding("KMG.ElementalRaces.Traits.Oread.CrystallineForm.Mode", "deflect-next-ray", typeof(BlueprintActivatableAbility)),
            new Binding("KMG.ElementalRaces.Traits.Undine.AcidBreath.Ability", "acid-breath", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Traits.Undine.OozeBreath.Ability", "ooze-breath", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.BreezeKissed.Gust", "breeze-gust", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.BreezeKissed.BullRush", "breeze-bull-rush", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.BreezeKissed.Trip", "breeze-trip", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.BreezeKissed.CalmedBuff", "calm-winds", typeof(BlueprintBuff)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.BreezeKissed.CalmWinds", "calm-winds", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Traits.Sylph.BreezeKissed.RenewWinds", "renew-winds", typeof(BlueprintAbility)),
            new Binding("KMG.Spells.Teleport.Ability", "teleport", typeof(BlueprintAbility)),
            new Binding("KMG.Spells.GreaterTeleport.Ability", "greater-teleport", typeof(BlueprintAbility)),
            new Binding("KMG.Spells.WordOfRecall.Ability", "word-of-recall", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Traits.Undine.NereidFascination.Ability", "nereid-fascination", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Traits.Undine.NereidFascination.FascinatedBuff", "nereid-fascination", typeof(BlueprintBuff)),
            new Binding("KMG.ElementalRaces.Traits.Undine.NereidFascination.ShakeFreeAbility", "shake-free", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Traits.Undine.NereidFascination.AssistanceBuff", "shake-free", typeof(BlueprintBuff)),
            new Binding("KMG.ElementalRaces.Traits.Undine.NereidFascination.AuraBuff", "nereid-fascination", typeof(BlueprintBuff)),
            new Binding("KMG.ElementalRaces.Traits.Oread.TreacherousEarth.Ability", "treacherous-earth", typeof(BlueprintAbility)),
            new Binding("KMG.ElementalRaces.Traits.Oread.TreacherousEarth.TerrainBuff", "treacherous-earth", typeof(BlueprintBuff)),
            new Binding("KMG.Spells.Teleport.Scroll", "teleport", typeof(BlueprintItemEquipmentUsable)),
            new Binding("KMG.Spells.GreaterTeleport.Scroll", "greater-teleport", typeof(BlueprintItemEquipmentUsable)),
            new Binding("KMG.Spells.WordOfRecall.Scroll", "word-of-recall", typeof(BlueprintItemEquipmentUsable)),
        };

        internal static IEnumerable<Binding> Bindings => Entries;
        internal static IEnumerable<string> IconKeys => Entries.Select(value => value.Key)
            .Distinct(StringComparer.Ordinal);

        internal static void Apply(LibraryScriptableObject library,
            BlueprintManifest manifest, bool strategicIdentitiesRegistered)
        {
            if (library == null || manifest == null) throw new ArgumentNullException("library/manifest");
            // Preserve the existing independent Teleportation registration failure
            // boundary. An OFF module still registers its save-hydration identities.
            var targets = Entries.Where(value => !value.IsStrategic || strategicIdentitiesRegistered)
                .Select(value => new { Blueprint = value.Resolve(library, manifest),
                    Icon = ProjectAssetIcons.RequireIcon(value.Key) }).ToArray();
            var facts = BlueprintUnitFactAccess.Resolve();
            var items = BlueprintItemAccess.Resolve();
            foreach (var target in targets)
            {
                var fact = target.Blueprint as BlueprintUnitFact;
                var item = target.Blueprint as BlueprintItem;
                if (fact != null) facts.SetIcon(fact, target.Icon);
                else if (item != null) items.SetIcon(item, target.Icon);
                else throw new InvalidOperationException("Unsupported owned icon type: " + target.Blueprint.GetType());
            }
        }
    }
}
