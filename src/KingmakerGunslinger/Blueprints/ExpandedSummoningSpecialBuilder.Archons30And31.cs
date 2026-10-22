using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private const string Archon3031 = "KMG.Summoning.OutsiderCelestial.Archon.";
        private static void ConfigureArchon30And31(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            var lantern = Require<BlueprintUnit>(bySymbol, LanternUnitSymbol);
            var hound = Require<BlueprintUnit>(bySymbol, Hound30Unit);
            var carrier = Require<BlueprintBuff>(bySymbol, Archon3031 + "MenaceCarrier");
            var area = Require<BlueprintAbilityAreaEffect>(bySymbol, Archon3031 + "MenaceArea");
            var penalty = Require<BlueprintBuff>(bySymbol, Archon3031 + "MenacePenalty");
            var immunity = Require<BlueprintBuff>(bySymbol, Archon3031 + "MenaceImmunity");
            carrier.name = InternalName(Archon3031 + "MenaceCarrier");
            penalty.name = InternalName(Archon3031 + "MenacePenalty");
            immunity.name = InternalName(Archon3031 + "MenaceImmunity");
            area.name = InternalName(Archon3031 + "MenaceArea");
            carrier.Stacking = StackingType.Stack;
            penalty.Stacking = StackingType.Stack;
            immunity.Stacking = StackingType.Stack;
            var hitBreak = ScriptableObject.CreateInstance<ArchonMenaceHitBreak>();
            hitBreak.Immunity = immunity;
            var onePenalty = ScriptableObject.CreateInstance<ArchonMenacePenalty>();
            penalty.ComponentsArray = new BlueprintComponent[] { onePenalty, hitBreak,
                ScriptableObject.CreateInstance<ArchonMenaceParameters>() };
            immunity.ComponentsArray = Array.Empty<BlueprintComponent>();
            var access = BlueprintUnitFactAccess.Resolve();
            access.Configure(penalty, LocalizationService.Create(Archon3031 + "Penalty.Name", "Aura of Menace"),
                LocalizationService.Create(Archon3031 + "Penalty.Description",
                    "-2 on attacks, AC and saves until hitting this archon or 24 hours. Each archon keeps its own save and immunity."), null);
            access.Configure(immunity, LocalizationService.Create(Archon3031 + "Immunity.Name", "Resisted Aura of Menace"),
                LocalizationService.Create(Archon3031 + "Immunity.Description", "Immune to this archon's menace for 24 hours."), null);
            access.Configure(carrier, LocalizationService.Create(Archon3031 + "Carrier.Name", "Aura of Menace"),
                LocalizationService.Create(Archon3031 + "Carrier.Description", "Hostile creatures within 20 feet save against this archon's menace."), null);
            var exposure = ScriptableObject.CreateInstance<ContextActionArchonMenace>();
            var trumpet = Require<BlueprintUnit>(bySymbol, Trumpet31Unit);
            exposure.Owners = new[] { lantern, hound, trumpet }; exposure.Penalty = penalty; exposure.Immunity = immunity;
            var actions = ScriptableObject.CreateInstance<AbilityAreaEffectRunAction>();
            actions.UnitEnter = new ActionList { Actions = new GameAction[] { exposure } };
            actions.Round = new ActionList { Actions = new GameAction[] { exposure } };
            actions.UnitMove = new ActionList { Actions = Array.Empty<GameAction>() };
            actions.UnitExit = new ActionList { Actions = Array.Empty<GameAction>() };
            area.Shape = AreaEffectShape.Cylinder; area.Size = new Feet(20);
            area.AffectEnemies = true; area.AffectDead = false; area.AggroEnemies = false;
            area.IgnoreSleepingUnits = false; area.SpellResistance = false; area.Fx = new PrefabLink();
            area.ComponentsArray = new BlueprintComponent[] { actions };
            var emanation = ScriptableObject.CreateInstance<AddAreaEffect>(); emanation.AreaEffect = area;
            var cleanup = ScriptableObject.CreateInstance<ArchonMenaceEmitterCleanup>();
            cleanup.Penalty = penalty; cleanup.Immunity = immunity;
            carrier.ComponentsArray = new BlueprintComponent[] { emanation, cleanup };
            if (carrier.StayOnDeath) throw new InvalidOperationException("Archon emitter must end on death.");
            var nativeExtraplanar = Feature(library, ExtraplanarSubtypeGuid, "Lantern native extraplanar spell consumer");
            if (!lantern.AddFacts.Contains(nativeExtraplanar))
                lantern.AddFacts = lantern.AddFacts.Concat(new BlueprintUnitFact[] { nativeExtraplanar }).ToArray();
            var defenses = Require<BlueprintBuff>(bySymbol, LanternDefenseSymbol);
            defenses.ComponentsArray = defenses.ComponentsArray.Concat(ArchonPetrificationImmunities()).ToArray();
            var aid = ConfigureArchonAid(library, bySymbol, lantern, defenses,
                Require<BlueprintBrain>(bySymbol, LanternBrainSymbol), Archon3031 + "Lantern", 3, true);
            var houndAid = ConfigureArchonAid(library, bySymbol, hound,
                Require<Kingmaker.Blueprints.Classes.BlueprintFeature>(bySymbol, Hound30Prefix + "Traits"),
                Require<BlueprintBrain>(bySymbol, Hound30Prefix + "Brain"), Hound30Prefix, 6, false);
            var trumpetAid = ConfigureArchonAid(library, bySymbol, trumpet,
                Require<Kingmaker.Blueprints.Classes.BlueprintFeature>(bySymbol, Trumpet31Prefix + "Traits"),
                Require<BlueprintBrain>(bySymbol, Trumpet31Prefix + "Brain"), Trumpet31Prefix, 14, false);
            trumpet.AddFacts = trumpet.AddFacts.Concat(new BlueprintUnitFact[] { carrier, trumpetAid }).ToArray();
            hound.AddFacts = hound.AddFacts.Concat(new BlueprintUnitFact[] { carrier, houndAid }).ToArray();
            lantern.AddFacts = lantern.AddFacts.Where(fact => fact.AssetGuid != AuraOfMenaceBuffGuid)
                .Concat(new Kingmaker.Blueprints.Facts.BlueprintUnitFact[] { carrier, aid }).ToArray();
        }
        private static BlueprintAbility ConfigureArchonAid(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol, BlueprintUnit unit,
            BlueprintUnitFact defenses, BlueprintBrain brain, string prefix, int casterLevel, bool immediate)
        {
            var aid = Require<BlueprintAbility>(bySymbol, prefix + "Aid");
            CloneMephitSpell(library, aid, "03a9630394d10164a9410882d31572f0", "native archon Aid");
            aid.name = InternalName(prefix + "Aid"); aid.Type = AbilityType.SpellLike;
            if (immediate)
                aid.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
            var parameters = ScriptableObject.CreateInstance<Sprint29GiantSpellLikeParameters>();
            parameters.OwningBlueprint = unit; parameters.Abilities = new[] { aid };
            parameters.SpellLevels = new[] { 2 }; parameters.CasterLevel = casterLevel;
            defenses.ComponentsArray = defenses.ComponentsArray.Concat(new BlueprintComponent[] { parameters }).ToArray();
            var ai = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol,
                prefix + "AidAi"), prefix + "AidAi", aid, 0);
            var eligibility = Require<ArchonAidEligibility>(bySymbol, prefix + "AidEligibility");
            eligibility.name = InternalName(prefix + "AidEligibility");
            eligibility.OwningBlueprint = unit; eligibility.Ability = aid;
            eligibility.AidBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                "319b4679f25779e4e9d04360381254e1", "native Aid buff");
            ai.TargetConsiderations = new Consideration[] { eligibility }; ai.BaseScore = 4;
            brain.Actions = brain.Actions.Concat(new BlueprintAiAction[] { ai }).ToArray();
            return aid;
        }
    }
}
