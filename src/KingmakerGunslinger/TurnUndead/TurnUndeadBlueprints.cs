using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.TurnUndead
{
    internal sealed class TurnUndeadSourceBlueprints
    {
        internal TurnUndeadSourceBlueprints(TurnUndeadSourceContract contract,
            BlueprintFeature sourceFeature, BlueprintAbility nativeHarm,
            BlueprintAbility direct, BlueprintAbility menuChild)
        {
            Contract = contract;
            SourceFeature = sourceFeature;
            NativeHarm = nativeHarm;
            Direct = direct;
            MenuChild = menuChild;
        }

        internal TurnUndeadSourceContract Contract { get; private set; }
        internal BlueprintFeature SourceFeature { get; private set; }
        internal BlueprintAbility NativeHarm { get; private set; }
        internal BlueprintAbility Direct { get; private set; }
        internal BlueprintAbility MenuChild { get; private set; }

        /// <summary>
        /// Call of the Wild's alternate paladin payment route, when loaded: its extra-channel harm
        /// (the donor) and Turn's child that spends the same extra pool. Null otherwise.
        /// </summary>
        internal BlueprintAbility ExtraHarm { get; private set; }
        internal BlueprintAbility ExtraMenuChild { get; private set; }

        /// <summary>
        /// How this source's DC level is computed: "native" (source class only), "progression"
        /// (the installed donor's CotW channel progression), or "fallback: reason" (the donor's
        /// progression could not be copied; source class only, reported).
        /// </summary>
        internal string DcStatus { get; set; }

        internal void AttachExtraRoute(BlueprintAbility harm, BlueprintAbility child)
        {
            ExtraHarm = harm;
            ExtraMenuChild = child;
        }

        /// <summary>Every Turn child this source places under a loaded provider's wrapper.</summary>
        internal IEnumerable<KeyValuePair<BlueprintAbility, BlueprintAbility>> MenuRoutes
        {
            get
            {
                yield return new KeyValuePair<BlueprintAbility, BlueprintAbility>(NativeHarm, MenuChild);
                if (ExtraMenuChild != null)
                    yield return new KeyValuePair<BlueprintAbility, BlueprintAbility>(ExtraHarm, ExtraMenuChild);
            }
        }
    }

    internal sealed class TurnUndeadBlueprintSet
    {
        internal TurnUndeadBlueprintSet(BlueprintFeature feat, BlueprintBuff fleeing,
            TurnUndeadSourceBlueprints[] sources)
        {
            Feat = feat;
            Fleeing = fleeing;
            Sources = sources;
        }

        internal BlueprintFeature Feat { get; private set; }
        internal BlueprintBuff Fleeing { get; private set; }
        internal TurnUndeadSourceBlueprints[] Sources { get; private set; }

        /// <summary>Why the CotW paladin extra route is or is not present ("absent", "present", or a drift reason).</summary>
        internal string ExtraRouteStatus { get; set; }

        /// <summary>Per-source DC computation for the completion line, e.g. "cleric:progression,...".</summary>
        internal string DcSummary
        {
            get
            {
                return string.Join(",", Sources.Select(value => value.Contract.Key + ":" +
                    (value.DcStatus == null ? "?" : value.DcStatus.StartsWith("fallback", StringComparison.Ordinal) ?
                        "fallback" : value.DcStatus)).ToArray());
            }
        }

        internal IEnumerable<BlueprintAbility> AllAbilities
        {
            get
            {
                return Sources.SelectMany(value => value.ExtraMenuChild == null
                    ? new[] { value.Direct, value.MenuChild }
                    : new[] { value.Direct, value.MenuChild, value.ExtraMenuChild });
            }
        }
    }

    /// <summary>A validated DC calculator plan: its status and a factory for one fresh calculator per ability.</summary>
    internal sealed class TurnUndeadDcPlan
    {
        internal TurnUndeadDcPlan(string status, Func<BlueprintComponent> create)
        {
            Status = status;
            Create = create;
        }

        internal string Status { get; private set; }
        internal Func<BlueprintComponent> Create { get; private set; }
    }

    /// <summary>
    /// Private Turn Undead definitions. Native donors (each source's
    /// harm-undead channel and the shared FrightenedUndead buff) are read and
    /// cloned, never edited; every kept component is a fresh instance so no
    /// native ScriptableObject is shared or mutated.
    /// </summary>
    internal static class TurnUndeadBlueprints
    {
        internal const string FeatIconKey = "turn-undead-feat";
        internal const string ActionIconKey = "turn-undead-action";

        private const BindingFlags PrivateInstance =
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        internal static TurnUndeadBlueprintSet Register(LibraryScriptableObject library,
            BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            Sprite featIcon = ProjectAssetIcons.RequireIcon(FeatIconKey);
            Sprite actionIcon = ProjectAssetIcons.RequireIcon(ActionIconKey);
            BlueprintFeature undeadType = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                library, TurnUndeadSourceCatalog.UndeadTypeGuid, "native UndeadType");
            BlueprintBuff nativeFlight = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, TurnUndeadSourceCatalog.NativeFrightenedUndeadGuid,
                "native FrightenedUndead buff");
            ValidateNativeFlight(nativeFlight);

            BlueprintBuff fleeing = registry.Register<BlueprintBuff>(
                TurnUndeadSourceCatalog.FleeingBuffSymbol,
                () => CreateFleeingBuff(nativeFlight));

            // The feat is registered before the abilities that check for it.
            // Its grants are attached once every ability exists.
            BlueprintFeature feat = registry.Register<BlueprintFeature>(
                TurnUndeadSourceCatalog.FeatSymbol, () => CreateFeat(featIcon));

            var sources = new List<TurnUndeadSourceBlueprints>();
            string extraStatus = "absent";
            foreach (TurnUndeadSourceContract contract in TurnUndeadSourceCatalog.All)
            {
                BlueprintFeature sourceFeature = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                    library, contract.SourceFeatureGuid, "native " + contract.DisplayName +
                        " positive-channel feature");
                BlueprintAbility harm = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    library, contract.NativeHarmGuid, "native " + contract.DisplayName +
                        " positive harm-undead channel");
                BlueprintAbilityResource resource = BlueprintLibraryLookup.RequireExact<
                    BlueprintAbilityResource>(library, contract.ResourceGuid, "native " +
                        contract.DisplayName + " channel resource");
                BlueprintCharacterClass dcClass = BlueprintLibraryLookup.RequireExact<
                    BlueprintCharacterClass>(library, contract.DcClassGuid, contract.DcClassName +
                        " class");
                ValidateNativeSource(contract, harm, resource);
                var route = new TurnUndeadRoute(contract.DisplayName, contract.DisplayName, harm, resource,
                    contract.ResourceCost, contract.Kind == TurnUndeadSourceKind.Paladin ?
                        "two uses of lay on hands" : "one use of channel positive energy");
                // A core source always registers: a drifted CotW progression falls back to the
                // source class alone and is reported, never rolling back the core identities.
                TurnUndeadDcPlan dc = PlanDcCalculator(contract, contract.DisplayName, harm, dcClass, true);
                BlueprintAbility direct = registry.Register<BlueprintAbility>(
                    contract.DirectAbilitySymbol, () => CreateAbility(contract, route, dc,
                        dcClass, sourceFeature, feat, undeadType, fleeing, actionIcon, false));
                BlueprintAbility child = registry.Register<BlueprintAbility>(
                    contract.MenuChildSymbol, () => CreateAbility(contract, route, dc,
                        dcClass, sourceFeature, feat, undeadType, fleeing, actionIcon, true));
                var built = new TurnUndeadSourceBlueprints(contract, sourceFeature, harm, direct, child) { DcStatus = dc.Status };
                if (contract.Kind == TurnUndeadSourceKind.Paladin)
                    extraStatus = RegisterPaladinExtra(library, registry, contract, built, dcClass,
                        feat, undeadType, fleeing, actionIcon);
                sources.Add(built);
            }

            AttachFeatComponents(feat, sources);
            var set = new TurnUndeadBlueprintSet(feat, fleeing, sources.ToArray()) { ExtraRouteStatus = extraStatus };
            Validate(set, featIcon, actionIcon, undeadType);
            return set;
        }

        /// <summary>
        /// The CotW paladin extra-channel route: present only when Call of the Wild defines both its
        /// extra harm channel and pool. A defined but drifted route is skipped and reported; it never
        /// blocks the four native sources.
        /// </summary>
        private static string RegisterPaladinExtra(LibraryScriptableObject library, BlueprintRegistry registry,
            TurnUndeadSourceContract contract, TurnUndeadSourceBlueprints paladin, BlueprintCharacterClass dcClass,
            BlueprintFeature feat, BlueprintFeature undeadType, BlueprintBuff fleeing, Sprite actionIcon)
        {
            BlueprintScriptableObject harmValue, resourceValue;
            library.BlueprintsByAssetId.TryGetValue(TurnUndeadSourceCatalog.PaladinExtraHarmGuid, out harmValue);
            library.BlueprintsByAssetId.TryGetValue(TurnUndeadSourceCatalog.PaladinExtraResourceGuid, out resourceValue);
            if (harmValue == null && resourceValue == null) return "absent";
            return BuildPaladinExtra(contract, paladin, harmValue as BlueprintAbility, resourceValue as BlueprintAbilityResource,
                dcClass, feat, undeadType, fleeing, actionIcon,
                factory => registry.Register<BlueprintAbility>(TurnUndeadSourceCatalog.PaladinExtraMenuChildSymbol, factory));
        }

        /// <summary>
        /// Builds the CotW paladin extra-channel child inside its own failure boundary: any drift in the
        /// donor's payment, alignment, targeting, selective structure or DC calculator, or a failure while
        /// constructing or registering the child, leaves the route unavailable and reported
        /// (cotw.extra-route-unavailable) and returns normally, so the core Turn identities registered
        /// before it are never rolled back. The registry runs a factory before it changes the library, so a
        /// failed construction registers nothing. Registration and the harness (with an isolated donor and
        /// its own register delegate) use this same method.
        /// </summary>
        internal static string BuildPaladinExtra(TurnUndeadSourceContract contract, TurnUndeadSourceBlueprints paladin,
            BlueprintAbility harm, BlueprintAbilityResource resource, BlueprintCharacterClass dcClass,
            BlueprintFeature feat, BlueprintFeature undeadType, BlueprintBuff fleeing, Sprite actionIcon,
            Func<Func<BlueprintAbility>, BlueprintAbility> register)
        {
            string drift = null;
            try
            {
                drift = ExtraRouteDrift(harm, resource);
                if (drift == null)
                {
                    TurnUndeadDcPlan dc = PlanDcCalculator(contract, "PaladinExtra", harm, dcClass, false);
                    var route = new TurnUndeadRoute("PaladinExtra", "Paladin, Extra Channel", harm, resource,
                        TurnUndeadSourceCatalog.PaladinExtraCost,
                        "one use of extra channel (the pool Call of the Wild's Extra Channel (Paladin) feat provides)");
                    BlueprintAbility child = register(() => CreateAbility(contract, route, dc,
                        dcClass, paladin.SourceFeature, feat, undeadType, fleeing, actionIcon, true));
                    paladin.AttachExtraRoute(harm, child);
                    return "present";
                }
            }
            catch (Exception exception)
            {
                drift = exception.GetType().Name + ": " + exception.Message;
            }
            TurnUndeadDiagnostics.Warning("cotw.extra-route-unavailable",
                "Call of the Wild's paladin extra-channel route was not given a Turn Undead action (" +
                (harm == null ? "no extra harm" : harm.name) + "): " + drift + ". The four native sources are unaffected.");
            return "drift: " + drift;
        }

        private static string ExtraRouteDrift(BlueprintAbility harm, BlueprintAbilityResource resource)
        {
            string drift = null;
            if (harm == null || resource == null)
                drift = "extra harm " + (harm != null) + ", extra pool " + (resource != null);
            else
            {
                AbilityResourceLogic[] logic = (harm.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .OfType<AbilityResourceLogic>().ToArray();
                if (logic.Length != 1 || !ReferenceEquals(logic[0].RequiredResource, resource) ||
                    !logic[0].IsSpendResource || logic[0].Amount != TurnUndeadSourceCatalog.PaladinExtraCost)
                    drift = "extra harm no longer spends " + TurnUndeadSourceCatalog.PaladinExtraCost + " of the extra pool";
                else if (!(harm.ComponentsArray ?? Array.Empty<BlueprintComponent>()).OfType<AbilityCasterAlignment>().Any())
                    drift = "extra harm lost its lawful good restriction";
                else if (harm.ActionType != UnitCommand.CommandType.Standard ||
                    (harm.ComponentsArray ?? Array.Empty<BlueprintComponent>()).OfType<AbilityTargetsAround>().Count() != 1)
                    drift = "extra harm is no longer a standard-action burst";
                else
                {
                    try { NativeSelectiveChannel(harm); }
                    catch (InvalidOperationException exception) { drift = exception.Message; }
                }
            }
            return drift;
        }

        private static BlueprintBuff CreateFleeingBuff(BlueprintBuff nativeFlight)
        {
            // The clone keeps the native buff's empty FX links, stacking policy,
            // presentation flags and its native Fear icon; only the logic changes.
            BlueprintBuff result = BlueprintCloneService.Clone(nativeFlight,
                "KMG_TurnUndead_Fleeing");
            var state = ScriptableObject.CreateInstance<TurnUndeadFleeingState>();
            state.name = TurnUndeadFleeingState.ComponentName;
            result.ComponentsArray = new BlueprintComponent[] { state };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.TurnUndead.Fleeing.Name", "Turned"),
                LocalizationService.Create("KMG.TurnUndead.Fleeing.Description",
                    "This undead creature was turned by channeled positive energy and flees in panic. Intelligent undead attempt a new Will save each round to end the effect; mindless undead do not. The effect lasts 1 minute."),
                nativeFlight.Icon);
            return result;
        }

        private static BlueprintFeature CreateFeat(Sprite icon)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_TurnUndead_Feat";
            feature.Ranks = 1;
            feature.HideInUI = false;
            feature.IsClassFeature = false;
            feature.Groups = new[] { FeatureGroup.Feat };
            feature.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.Feat.TurnUndead.Name", "Turn Undead"),
                LocalizationService.Create("KMG.Feat.TurnUndead.Description",
                    "Prerequisite: Channel positive energy class feature.\n" +
                    "Benefit: As a standard action, you can use one of your uses of channel positive energy to cause all undead within 30 feet of you to flee, as if panicked. Undead receive a Will save to negate the effect. The DC for this Will save is equal to 10 + 1/2 your cleric level + your Charisma modifier. Undead that fail their save flee for 1 minute. Intelligent undead receive a new saving throw each round to end the effect. If you use channel energy in this way, it has no other effect (it does not heal or harm nearby creatures).\n" +
                    "In Kingmaker: undead allies are affected too, unless you have Selective Channel, in which case only enemy undead are affected. Supported channels are a cleric's positive channel, a paladin's channel (two uses of lay on hands, or one use of the extra channel pool from Call of the Wild's Extra Channel (Paladin) feat when that mod is loaded), a hospitaler's channel and an empyreal sorcerer's channel; other channel energy sources, including those added by other mods, do not qualify. Each supported channel gains its own Turn Undead action that spends that channel's uses, with its own DC: 10 + half that class's level (with Call of the Wild, half the total of that class's levels and the levels of classes that advance that channel), rounded down, + the channel's statistic modifier (Wisdom for an empyreal sorcerer). Panic is represented by the game's frightened fleeing condition; affected undead do not drop what they are holding."),
                icon);
            return feature;
        }

        private static void AttachFeatComponents(BlueprintFeature feat,
            List<TurnUndeadSourceBlueprints> sources)
        {
            var prerequisite = ScriptableObject.CreateInstance<PrerequisiteFeaturesFromList>();
            prerequisite.name = "$KMG_TurnUndead_Prerequisite";
            prerequisite.Features = sources.Select(value => value.SourceFeature).ToArray();
            prerequisite.Amount = 1;
            prerequisite.Group = Prerequisite.GroupType.All;
            var components = new List<BlueprintComponent> { prerequisite };
            foreach (TurnUndeadSourceBlueprints source in sources)
            {
                // Grants only the action of a source the character actually has;
                // re-checked natively at every level-up, so a later second
                // source adds its own action. Never creates or refills a pool.
                var grant = ScriptableObject.CreateInstance<AddFeatureIfHasFact>();
                grant.name = "$KMG_TurnUndead_Grant_" + source.Contract.DisplayName;
                grant.CheckedFact = source.SourceFeature;
                grant.Feature = source.Direct;
                grant.Not = false;
                components.Add(grant);
            }
            feat.ComponentsArray = components.ToArray();
        }

        /// <summary>One payment route of a source: the harm channel it mirrors, the pool it spends and its cost.</summary>
        private sealed class TurnUndeadRoute
        {
            internal TurnUndeadRoute(string key, string displayName, BlueprintAbility harm,
                BlueprintAbilityResource resource, int cost, string poolText)
            {
                Key = key; DisplayName = displayName; Harm = harm; Resource = resource; Cost = cost; PoolText = poolText;
            }

            internal string Key { get; private set; }
            internal string DisplayName { get; private set; }
            internal BlueprintAbility Harm { get; private set; }
            internal BlueprintAbilityResource Resource { get; private set; }
            internal int Cost { get; private set; }
            internal string PoolText { get; private set; }
        }

        private static BlueprintAbility CreateAbility(TurnUndeadSourceContract contract,
            TurnUndeadRoute route, TurnUndeadDcPlan dc, BlueprintCharacterClass dcClass, BlueprintFeature sourceFeature,
            BlueprintFeature feat, BlueprintFeature undeadType, BlueprintBuff fleeing,
            Sprite icon, bool menuChild)
        {
            BlueprintAbility harm = route.Harm;
            string stem = "KMG_TurnUndead_" + route.Key +
                (menuChild ? "_MenuChild" : "_Ability");
            BlueprintAbility result = BlueprintCloneService.Clone(harm, stem);
            string statName = contract.DcStat == TurnUndeadDcStat.Wisdom ? "Wisdom" : "Charisma";
            BlueprintComponent parameters = dc.Create();
            BlueprintCharacterClass[] progression;
            StatType ignored;
            ReadCalculator(parameters, out progression, out ignored);
            // The game sums every qualifying class level, halves the total (rounded down), then adds the statistic.
            string level = progression.Length > 1
                ? "half the total of your " + contract.DcClassName + " levels and your levels in classes that advance this channel, rounded down,"
                : "1/2 your " + contract.DcClassName + " level (rounded down)";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.TurnUndead." + route.Key + ".Name",
                    "Turn Undead (" + route.DisplayName + ")"),
                LocalizationService.Create("KMG.TurnUndead." + route.Key + ".Description",
                    "As a standard action, spend " + route.PoolText +
                    " to cause undead within 30 feet of you to flee for 1 minute: every undead creature, allies included, or only enemy undead if you have Selective Channel. Each affected undead can attempt a Will save to negate the effect (DC 10 + " +
                    level + " + your " + statName +
                    " modifier). Intelligent undead attempt a new save each round to end the effect; mindless undead do not. This channel neither heals nor harms."),
                icon);
            result.Type = AbilityType.Supernatural;
            result.ActionType = UnitCommand.CommandType.Standard;
            result.AvailableMetamagic = 0;
            result.Parent = null;
            result.Hidden = false;
            result.LocalizedDuration = LocalizationService.Create(
                "KMG.TurnUndead.Duration", "1 minute");
            result.LocalizedSavingThrow = LocalizationService.Create(
                "KMG.TurnUndead.SavingThrow", "Will negates");
            if (result.MaterialComponent == null)
                result.MaterialComponent = new BlueprintAbility.MaterialComponentData();

            BlueprintComponent[] native = harm.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            var components = new List<BlueprintComponent>();

            components.Add(parameters);

            var resourceLogic = ScriptableObject.CreateInstance<AbilityResourceLogic>();
            resourceLogic.name = "$KMG_TurnUndead_" + route.Key + "_Resource";
            resourceLogic.RequiredResource = route.Resource;
            resourceLogic.IsSpendResource = true;
            resourceLogic.Amount = route.Cost;
            resourceLogic.CostIsCustom = false;
            resourceLogic.ResourceCostIncreasingFacts = new List<BlueprintUnitFact>();
            components.Add(resourceLogic);

            AbilityTargetsAround around = native.OfType<AbilityTargetsAround>().Single();
            AbilityTargetsAround targets = UnityEngine.Object.Instantiate(around);
            targets.name = "$KMG_TurnUndead_" + route.Key + "_Targets";
            SetField(targets, "m_Condition", new ConditionsChecker
            {
                Operation = Operation.And,
                Conditions = new Condition[]
                {
                    // Every undead within the native 30-foot burst, the channeler
                    // included when it is itself undead (the native targeting
                    // includes the caster); no other selection exception exists.
                    new ContextConditionHasFact { Fact = undeadType, Not = false },
                }
            });
            components.Add(targets);

            // The native harm channel's own structure: with Selective Channel the burst
            // reaches only enemy undead, otherwise every undead; only included units
            // roll (the save is inside each branch, the effect's own save is Unknown).
            BlueprintUnitFact selective = NativeSelectiveChannel(harm);
            Func<GameAction> turnSave = () => new ContextActionSavingThrow
            {
                Type = SavingThrowType.Will,
                Actions = new ActionList
                {
                    Actions = new GameAction[]
                    {
                        new ContextActionConditionalSaved
                        {
                            Succeed = new ActionList { Actions = Array.Empty<GameAction>() },
                            Failed = new ActionList
                            {
                                Actions = new GameAction[]
                                {
                                    new ContextActionApplyBuff
                                    {
                                        Buff = fleeing,
                                        DurationValue = new ContextDurationValue
                                        {
                                            Rate = DurationRate.Minutes,
                                            DiceType = Kingmaker.RuleSystem.DiceType.Zero,
                                            DiceCountValue = 0,
                                            BonusValue = TurnUndeadRules.DurationMinutes
                                        },
                                        IsFromSpell = false,
                                        IsNotDispelable = true,
                                        AsChild = true
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            effect.name = "$KMG_TurnUndead_" + route.Key + "_Effect";
            effect.SavingThrowType = SavingThrowType.Unknown;
            effect.Actions = new ActionList
            {
                Actions = new GameAction[]
                {
                    new Conditional
                    {
                        Comment = "Selective Channel: enemy undead only",
                        ConditionsChecker = new ConditionsChecker
                        {
                            Operation = Operation.And,
                            Conditions = new Condition[] { new ContextConditionCasterHasFact { Fact = selective, Not = false } }
                        },
                        IfTrue = new ActionList
                        {
                            Actions = new GameAction[]
                            {
                                new Conditional
                                {
                                    Comment = "enemy",
                                    ConditionsChecker = new ConditionsChecker
                                    {
                                        Operation = Operation.And,
                                        Conditions = new Condition[] { new ContextConditionIsEnemy { Not = false } }
                                    },
                                    IfTrue = new ActionList { Actions = new GameAction[] { turnSave() } },
                                    IfFalse = new ActionList { Actions = Array.Empty<GameAction>() }
                                }
                            }
                        },
                        IfFalse = new ActionList { Actions = new GameAction[] { turnSave() } }
                    }
                }
            };
            components.Add(effect);

            foreach (AbilitySpawnFx fx in native.OfType<AbilitySpawnFx>())
            {
                AbilitySpawnFx copy = UnityEngine.Object.Instantiate(fx);
                copy.name = "$KMG_TurnUndead_" + route.Key + "_Fx";
                components.Add(copy);
            }
            foreach (AbilityCasterAlignment alignment in native.OfType<AbilityCasterAlignment>())
            {
                AbilityCasterAlignment copy = UnityEngine.Object.Instantiate(alignment);
                copy.name = "$KMG_TurnUndead_" + route.Key + "_Alignment";
                components.Add(copy);
            }

            // Execution requires both the feat and this exact source. Two
            // single-fact checkers: the native NeedsAll mode only tests the last.
            components.Add(CasterHas("$KMG_TurnUndead_" + route.Key + "_RequiresFeat", feat));
            components.Add(CasterHas("$KMG_TurnUndead_" + route.Key + "_RequiresSource",
                sourceFeature));
            if (menuChild)
            {
                // Variant menus show a child only when both facts are present.
                components.Add(ShowIf("$KMG_TurnUndead_" + route.Key + "_ShowIfFeat", feat));
                components.Add(ShowIf("$KMG_TurnUndead_" + route.Key + "_ShowIfSource",
                    sourceFeature));
            }
            result.ComponentsArray = components.ToArray();
            return result;
        }

        private const string CotwClassesCalculator = "ContextCalculateAbilityParamsBasedOnClasses";

        /// <summary>
        /// The DC calculator plan for one route, validated once before any ability is built. Vanilla
        /// donors carry none, so Turn uses the native single-class calculator (source class level, source
        /// statistic). When a loaded provider gave the installed donor Call of the Wild's multi-class
        /// calculator (the source class plus the prestige classes whose levels advance that channel, e.g.
        /// Holy Vindicator and Dawnflower Anchorite), each Turn ability gets a fresh copy with the source's
        /// own statistic (Wisdom for an empyreal sorcerer; the native Empyreal override names only the
        /// native heal and harm). A donor whose calculator cannot be copied (several calculators, an
        /// unknown type, a copy that loses the source class or fixes no statistic) is drift: with
        /// <paramref name="allowFallback"/> (a core source) Turn uses the source class alone and reports
        /// cotw.dc-progression-unavailable; without it (an optional route) the plan throws.
        /// </summary>
        internal static TurnUndeadDcPlan PlanDcCalculator(TurnUndeadSourceContract contract, string key,
            BlueprintAbility harm, BlueprintCharacterClass dcClass, bool allowFallback)
        {
            StatType stat = contract.DcStat == TurnUndeadDcStat.Wisdom ? StatType.Wisdom : StatType.Charisma;
            Func<BlueprintComponent> native = () =>
            {
                var parameters = ScriptableObject.CreateInstance<ContextCalculateAbilityParamsBasedOnClass>();
                parameters.name = "$KMG_TurnUndead_" + key + "_Dc";
                parameters.CharacterClass = dcClass;
                parameters.StatType = stat;
                parameters.UseKineticistMainStat = false;
                return parameters;
            };
            BlueprintComponent[] donors = (harm.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Where(component => component is ContextAbilityParamsCalculator).ToArray();
            string drift = null;
            if (donors.Length == 0) return new TurnUndeadDcPlan("native", native);
            if (donors.Length > 1) drift = donors.Length + " DC calculators";
            else if (donors[0].GetType().Name == CotwClassesCalculator)
            {
                BlueprintComponent donor = donors[0];
                BlueprintComponent probe = null;
                try
                {
                    probe = CopyProgression(donor, key, stat);
                    BlueprintCharacterClass[] classes;
                    StatType copied;
                    if (!ReadCalculator(probe, out classes, out copied) || copied != stat || !classes.Contains(dcClass))
                        drift = "the copied channel progression lost " + contract.DcClassName + " or fixes no statistic";
                }
                catch (Exception exception) { drift = exception.GetType().Name + ": " + exception.Message; }
                finally { if (probe != null) UnityEngine.Object.DestroyImmediate(probe); }
                if (drift == null) return new TurnUndeadDcPlan("progression", () => CopyProgression(donor, key, stat));
            }
            else if (donors[0] is ContextCalculateAbilityParamsBasedOnClass) return new TurnUndeadDcPlan("native", native);
            else drift = "unknown DC calculator " + donors[0].GetType().FullName;
            if (!allowFallback)
                throw new InvalidOperationException("the installed " + harm.name + " DC calculator drifted: " + drift);
            TurnUndeadDiagnostics.Warning("cotw.dc-progression-unavailable",
                "Turn Undead (" + contract.DisplayName + ") uses its " + contract.DcClassName +
                " level alone: the installed " + harm.name + " channel progression could not be copied (" + drift + ").");
            return new TurnUndeadDcPlan("fallback: " + drift, native);
        }

        private static BlueprintComponent CopyProgression(BlueprintComponent donor, string key, StatType stat)
        {
            BlueprintComponent copy = UnityEngine.Object.Instantiate(donor);
            copy.name = "$KMG_TurnUndead_" + key + "_Dc";
            SetField(copy, "StatType", stat);
            return copy;
        }

        /// <summary>
        /// The classes whose levels set a calculator's DC level and its statistic. False for an
        /// unknown calculator or one whose statistic is not fixed (kineticist or property override).
        /// </summary>
        internal static bool ReadCalculator(BlueprintComponent component, out BlueprintCharacterClass[] classes,
            out StatType stat)
        {
            classes = Array.Empty<BlueprintCharacterClass>();
            stat = StatType.Unknown;
            var native = component as ContextCalculateAbilityParamsBasedOnClass;
            if (native != null)
            {
                classes = native.CharacterClass == null ? classes : new[] { native.CharacterClass };
                stat = native.StatType;
                return !native.UseKineticistMainStat && native.CharacterClass != null;
            }
            if (component == null || component.GetType().Name != CotwClassesCalculator) return false;
            Type type = component.GetType();
            FieldInfo classField = type.GetField("CharacterClasses", PrivateInstance);
            FieldInfo statField = type.GetField("StatType", PrivateInstance);
            FieldInfo kineticist = type.GetField("use_kineticist_main_stat", PrivateInstance);
            FieldInfo property = type.GetField("property", PrivateInstance);
            if (classField == null || statField == null || kineticist == null || property == null) return false;
            classes = (classField.GetValue(component) as BlueprintCharacterClass[]) ?? Array.Empty<BlueprintCharacterClass>();
            stat = (StatType)statField.GetValue(component);
            return !(bool)kineticist.GetValue(component) && property.GetValue(component) == null &&
                classes.Length > 0 && classes.All(value => value != null);
        }

        private static AbilityCasterHasFacts CasterHas(string name, BlueprintUnitFact fact)
        {
            var checker = ScriptableObject.CreateInstance<AbilityCasterHasFacts>();
            checker.name = name;
            checker.Facts = new[] { fact };
            checker.NeedsAll = false;
            return checker;
        }

        private static AbilityShowIfCasterHasFact ShowIf(string name, BlueprintUnitFact fact)
        {
            var show = ScriptableObject.CreateInstance<AbilityShowIfCasterHasFact>();
            show.name = name;
            show.UnitFact = fact;
            return show;
        }

        private static void ValidateNativeFlight(BlueprintBuff nativeFlight)
        {
            if (nativeFlight.Icon == null)
                throw new InvalidOperationException("Native FrightenedUndead has no icon.");
        }

        /// <summary>
        /// The Selective Channel fact the installed native harm channel branches on
        /// (its top-level effect Conditional on ContextConditionCasterHasFact). Turn
        /// mirrors that rule; registration refuses if the donor no longer carries it.
        /// </summary>
        private static BlueprintUnitFact NativeSelectiveChannel(BlueprintAbility harm)
        {
            AbilityEffectRunAction run = (harm.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AbilityEffectRunAction>().SingleOrDefault();
            BlueprintUnitFact[] facts = run == null || run.Actions == null || run.Actions.Actions == null
                ? Array.Empty<BlueprintUnitFact>()
                : run.Actions.Actions.OfType<Conditional>()
                    .Where(c => c.ConditionsChecker != null && c.ConditionsChecker.Conditions != null)
                    .SelectMany(c => c.ConditionsChecker.Conditions.OfType<ContextConditionCasterHasFact>())
                    .Where(c => !c.Not && c.Fact != null).Select(c => c.Fact).Distinct().ToArray();
            if (facts.Length != 1 || facts[0].AssetGuid != TurnUndeadSourceCatalog.SelectiveChannelGuid)
                throw new InvalidOperationException("The installed " + harm.name +
                    " no longer limits itself through Selective Channel " + TurnUndeadSourceCatalog.SelectiveChannelGuid + ".");
            return facts[0];
        }

        /// <summary>Every flight application inside a Turn effect, with the branch structure checked.</summary>
        private static ContextActionApplyBuff[] TurnApplications(AbilityEffectRunAction effect, BlueprintUnitFact selective)
        {
            Conditional top = effect.Actions.Actions.Length == 1 ? effect.Actions.Actions[0] as Conditional : null;
            Conditional enemy = top == null || top.IfTrue.Actions.Length != 1 ? null : top.IfTrue.Actions[0] as Conditional;
            if (top == null || enemy == null || top.ConditionsChecker.Conditions.Length != 1 ||
                !(top.ConditionsChecker.Conditions[0] is ContextConditionCasterHasFact) ||
                ((ContextConditionCasterHasFact)top.ConditionsChecker.Conditions[0]).Fact != selective ||
                top.ConditionsChecker.Conditions[0].Not || enemy.ConditionsChecker.Conditions.Length != 1 ||
                !(enemy.ConditionsChecker.Conditions[0] is ContextConditionIsEnemy) ||
                enemy.ConditionsChecker.Conditions[0].Not || enemy.IfFalse.Actions.Length != 0 ||
                enemy.IfTrue.Actions.Length != 1 || top.IfFalse.Actions.Length != 1)
                return null;
            var result = new List<ContextActionApplyBuff>();
            foreach (GameAction branch in new[] { enemy.IfTrue.Actions[0], top.IfFalse.Actions[0] })
            {
                var save = branch as ContextActionSavingThrow;
                ContextActionConditionalSaved saved = save == null || save.Type != SavingThrowType.Will ||
                    save.Actions.Actions.Length != 1 ? null : save.Actions.Actions[0] as ContextActionConditionalSaved;
                if (saved == null || saved.Succeed.Actions.Length != 0 || saved.Failed.Actions.Length != 1 ||
                    !(saved.Failed.Actions[0] is ContextActionApplyBuff)) return null;
                result.Add((ContextActionApplyBuff)saved.Failed.Actions[0]);
            }
            return result.ToArray();
        }

        private static void ValidateNativeSource(TurnUndeadSourceContract contract,
            BlueprintAbility harm, BlueprintAbilityResource resource)
        {
            NativeSelectiveChannel(harm);
            AbilityResourceLogic[] logic = (harm.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AbilityResourceLogic>().ToArray();
            if (logic.Length != 1 || !ReferenceEquals(logic[0].RequiredResource, resource) ||
                !logic[0].IsSpendResource || logic[0].Amount != contract.ResourceCost)
                throw new InvalidOperationException("The installed " + contract.DisplayName +
                    " positive channel no longer spends " + contract.ResourceCost +
                    " of resource " + contract.ResourceGuid + ".");
            if (harm.ActionType != UnitCommand.CommandType.Standard)
                throw new InvalidOperationException("The installed " + contract.DisplayName +
                    " positive channel is no longer a standard action.");
            if ((harm.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AbilityTargetsAround>().Count() != 1)
                throw new InvalidOperationException("The installed " + contract.DisplayName +
                    " positive channel no longer has one burst targeting component.");
            bool alignment = (harm.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AbilityCasterAlignment>().Any();
            if (alignment != contract.RequiresLawfulGood)
                throw new InvalidOperationException("The installed " + contract.DisplayName +
                    " positive channel's alignment restriction changed.");
        }

        private static void Validate(TurnUndeadBlueprintSet set, Sprite featIcon,
            Sprite actionIcon, BlueprintFeature undeadType)
        {
            BlueprintFeature feat = set.Feat;
            if (feat.Ranks != 1 || feat.HideInUI || feat.IsClassFeature ||
                feat.Groups == null || !feat.Groups.SequenceEqual(new[] { FeatureGroup.Feat }) ||
                !ReferenceEquals(feat.Icon, featIcon))
                throw new InvalidOperationException("Turn Undead must remain a visible rank-one general feat with its feat icon.");
            PrerequisiteFeaturesFromList prerequisite = feat.ComponentsArray
                .OfType<PrerequisiteFeaturesFromList>().Single();
            if (prerequisite.Amount != 1 || !prerequisite.Features.SequenceEqual(
                    set.Sources.Select(value => value.SourceFeature)))
                throw new InvalidOperationException("Turn Undead prerequisite must be exactly one supported positive channel.");
            AddFeatureIfHasFact[] grants = feat.ComponentsArray.OfType<AddFeatureIfHasFact>().ToArray();
            if (grants.Length != set.Sources.Length || set.Sources.Any(source =>
                    grants.Count(grant => ReferenceEquals(grant.CheckedFact, source.SourceFeature) &&
                        ReferenceEquals(grant.Feature, source.Direct) && !grant.Not) != 1))
                throw new InvalidOperationException("Turn Undead must grant each source's direct action only with that source.");
            if (set.Fleeing.ComponentsArray.Length != 1 ||
                !(set.Fleeing.ComponentsArray[0] is TurnUndeadFleeingState) ||
                set.Fleeing.ComponentsArray[0].name != TurnUndeadFleeingState.ComponentName)
                throw new InvalidOperationException("Turn Undead flight must carry only its owned state component.");
            foreach (TurnUndeadSourceBlueprints source in set.Sources)
                foreach (var check in new[]
                {
                    new { Ability = source.Direct, Harm = source.NativeHarm, Resource = source.Contract.ResourceGuid, Cost = source.Contract.ResourceCost },
                    new { Ability = source.MenuChild, Harm = source.NativeHarm, Resource = source.Contract.ResourceGuid, Cost = source.Contract.ResourceCost },
                    new { Ability = source.ExtraMenuChild, Harm = source.ExtraHarm, Resource = TurnUndeadSourceCatalog.PaladinExtraResourceGuid, Cost = TurnUndeadSourceCatalog.PaladinExtraCost },
                }.Where(value => value.Ability != null))
                {
                    BlueprintAbility ability = check.Ability;
                    AbilityResourceLogic resource = ability.ComponentsArray.OfType<AbilityResourceLogic>().Single();
                    BlueprintComponent[] calculators = ability.ComponentsArray.Where(value => value is ContextAbilityParamsCalculator).ToArray();
                    BlueprintComponent[] donorCalculators = check.Harm.ComponentsArray.Where(value => value is ContextAbilityParamsCalculator).ToArray();
                    BlueprintCharacterClass[] classes, donorClasses;
                    StatType stat, donorStat;
                    // A native single-class calculator names exactly the source class (vanilla, or a reported
                    // fallback); a copied CotW progression lists exactly the donor's classes.
                    bool dcValid = calculators.Length == 1 && ReadCalculator(calculators[0], out classes, out stat) &&
                        classes.Any(value => value.AssetGuid == source.Contract.DcClassGuid) &&
                        stat == (source.Contract.DcStat == TurnUndeadDcStat.Wisdom ? StatType.Wisdom : StatType.Charisma) &&
                        (calculators[0] is ContextCalculateAbilityParamsBasedOnClass ? classes.Length == 1 :
                            donorCalculators.Length == 1 && ReadCalculator(donorCalculators[0], out donorClasses, out donorStat) &&
                            classes.SequenceEqual(donorClasses));
                    AbilityEffectRunAction effect = ability.ComponentsArray.OfType<AbilityEffectRunAction>().Single();
                    ContextActionApplyBuff[] applications = TurnApplications(effect, NativeSelectiveChannel(check.Harm));
                    if (ability.Type != AbilityType.Supernatural ||
                        ability.ActionType != UnitCommand.CommandType.Standard ||
                        ability.AvailableMetamagic != 0 || ability.Parent != null ||
                        !ReferenceEquals(ability.Icon, actionIcon) ||
                        resource.RequiredResource == null ||
                        resource.RequiredResource.AssetGuid != check.Resource ||
                        resource.Amount != check.Cost || !resource.IsSpendResource || !dcValid ||
                        effect.SavingThrowType != SavingThrowType.Unknown || applications == null ||
                        applications.Any(apply => !ReferenceEquals(apply.Buff, set.Fleeing) ||
                            apply.DurationValue.Rate != DurationRate.Minutes ||
                            apply.DurationValue.BonusValue.Value != TurnUndeadRules.DurationMinutes) ||
                        ability.ComponentsArray.OfType<ContextRankConfig>().Any() ||
                        ability.ComponentsArray.OfType<AbilityCasterHasFacts>().Count() != 2 ||
                        ability.ComponentsArray.Intersect(check.Harm.ComponentsArray).Any() ||
                        ability.ComponentsArray.Select(value => value.name).Distinct().Count() !=
                            ability.ComponentsArray.Length)
                        throw new InvalidOperationException("Turn Undead (" +
                            source.Contract.DisplayName + ") ability contract is incomplete.");
                }
        }

        private static void SetField(object instance, string name, object value)
        {
            FieldInfo field = instance.GetType().GetField(name, PrivateInstance);
            if (field == null) throw new MissingFieldException(instance.GetType().FullName, name);
            field.SetValue(instance, value);
        }
    }
}
