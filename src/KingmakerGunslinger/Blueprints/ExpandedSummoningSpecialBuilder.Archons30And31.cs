using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
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
            penalty.ComponentsArray = new BlueprintComponent[] { onePenalty, hitBreak };
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
            exposure.Owners = new[] { lantern }; exposure.Penalty = penalty; exposure.Immunity = immunity;
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
            var aid = Require<BlueprintAbility>(bySymbol, Archon3031 + "LanternAid");
            CloneMephitSpell(library, aid, "03a9630394d10164a9410882d31572f0", "native Aid");
            aid.name = InternalName(Archon3031 + "LanternAid"); aid.Type = AbilityType.SpellLike;
            // The retained wisp rig has no cast clip. Its immediate native
            // ability animation still pays the ordinary command action cost.
            aid.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
            var parameters = ScriptableObject.CreateInstance<Sprint29GiantSpellLikeParameters>();
            parameters.OwningBlueprint = lantern; parameters.Abilities = new[] { aid };
            parameters.SpellLevels = new[] { 2 }; parameters.CasterLevel = 3;
            var defenses = Require<BlueprintBuff>(bySymbol, LanternDefenseSymbol);
            defenses.ComponentsArray = defenses.ComponentsArray.Concat(new BlueprintComponent[] { parameters }).ToArray();
            var ai = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol,
                Archon3031 + "LanternAidAi"), Archon3031 + "LanternAidAi", aid, 0);
            var eligibility = Require<ArchonAidEligibility>(bySymbol, Archon3031 + "LanternAidEligibility");
            eligibility.name = InternalName(Archon3031 + "LanternAidEligibility");
            eligibility.OwningBlueprint = lantern; eligibility.Ability = aid;
            eligibility.AidBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                "319b4679f25779e4e9d04360381254e1", "native Aid buff");
            ai.TargetConsiderations = new Consideration[] { eligibility }; ai.BaseScore = 4;
            var brain = Require<BlueprintBrain>(bySymbol, LanternBrainSymbol);
            brain.Actions = brain.Actions.Concat(new BlueprintAiAction[] { ai }).ToArray();
            lantern.AddFacts = lantern.AddFacts.Where(fact => fact.AssetGuid != AuraOfMenaceBuffGuid)
                .Concat(new Kingmaker.Blueprints.Facts.BlueprintUnitFact[] { carrier, aid }).ToArray();
        }
    }
}
