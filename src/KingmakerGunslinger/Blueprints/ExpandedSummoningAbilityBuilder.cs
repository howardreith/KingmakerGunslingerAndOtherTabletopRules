using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ExpandedSummoningAbilityBuilder
    {
        private static readonly string[] MonsterParents = {
            "8fd74eddd9b6c224693d9ab241f25e84", "1724061e89c667045a6891179ee2e8e7",
            "5d61dde0020bbf54ba1521f7ca0229dc", "7ed74a3ec8c458d4fb50b192fd7be6ef",
            "630c8b85d9f07a64f917d79cb5905741", "e740afbab0147944dab35d83faa0ae1c",
            "ab167fd8203c1314bac6568932f1752f", "d3ac756a229830243a72e84f3ab050d0",
            "52b5df2a97df18242aec67610616ded0" };
        private static readonly string[] AllyParents = {
            "c6147854641924442a3bb736080cfeb6", "298148133cdc3fd42889b99c82711986",
            "fdcf7e57ec44f704591f11b45f4acf61", "c83db50513abdf74ca103651931fac4b",
            "8f98a22f35ca6684a983363d32e51bfe", "55bbce9b3e76d4a4a8c8e0698d29002c",
            "051b979e7d7f8ec41b9fa35d04746b33", "ea78c04f0bd13d049a1cce5daf8d83e0",
            "a7469ef84ba50ac4cbf3d145e3173f8e" };
        private static readonly string[] NativeMonsterTemplateBuffs = {
            "83a8a909e7ad19b4fa57e306884cc8bd",
            "c707fdbe58d0c614a89a872b76777f4b",
            "ec906031bb8766f42a37a52cf1d89836",
            "4b808ccd7e0b7bd43b10b2d47733b1a4" };

        internal static IReadOnlyList<string> NativeTemplateIds { get { return Array.AsReadOnly(NativeMonsterTemplateBuffs); } }

        /// <summary>
        /// The DECLARED set of Summon Monster creatures given the explicit
        /// inherited-template removal action. Every one is
        /// monsterTemplated=false in ExpandedSummoningCatalog, so they take the
        /// untemplated path.
        ///
        /// It is a declaration, NOT a measurement, and GS-REP-0002 records that
        /// it disagrees with the installed library in both directions. Do not
        /// read this list as "the creatures whose native carrier applies a
        /// template" - that is what it was previously documented as, and the
        /// per-placement observation
        /// inherited-template-removal-wiring-per-placement reports a different
        /// population. The discrepancy is pre-existing on master and is not
        /// resolved here.
        ///
        /// The intended behaviour is not in doubt and is already executable:
        /// SummonCreatureSpec.TemplatePolicy maps monsterTemplated=false to
        /// SummonTemplatePolicy.None for every creature in the Summon Monster
        /// roster, which ExpandedSummoningCatalogTests
        /// .AlignmentPoliciesAreFamilyScoped asserts across the whole catalog.
        /// So an untemplated creature is meant to receive no celestial or
        /// fiendish template; what is open is when to correct this list and
        /// whether any creature is a deliberate exception.
        /// </summary>
        private static readonly string[] InheritedTemplateRemovalCreatures = {
            "lemure", "dretch", "hound-archon", "lantern-archon",
            "trumpet-archon", "astral-deva", "bearded-devil",
            "kyton-chain-devil", "barbed-devil", "bone-devil", "babau",
            "hezrou" };

        /// <summary>
        /// GS-AUD-0025: the one scoped rule for inherited-template removal.
        /// The canonical ordinary-spell builder and the alternate-caller
        /// adapter must agree, and previously only the canonical builder
        /// applied it. Keeping the set here means the two paths cannot diverge
        /// again.
        ///
        /// The finding's stated consequence - that an alternate caller's
        /// untemplated child therefore "kept the donor's template" - is
        /// WITHDRAWN and must not be repeated. Casting an affected adapter
        /// child measured appliedByGraph=0: the alternate-caller child is built
        /// from the caller's own host template, which applies no
        /// celestial/fiendish buff anywhere in its graph, so on that route
        /// there was nothing to inherit. Aligning the two paths is still worth
        /// doing, but it is hygiene against future divergence rather than the
        /// repair of a demonstrated defect on that route.
        ///
        /// Acceptance for this repair has TWO separate parts, and the first
        /// must never be allowed to close the second:
        ///
        /// 1. Graph/reader verification. The removal action exists on each
        ///    spawn branch, is bound to the correct creature unit, carries the
        ///    exact native identities, and occupies the final composed
        ///    position. This is what the alternate-summoning composition
        ///    observation checks - and that scenario states its own limit:
        ///    "Structural and graph evidence only... summon execution...
        ///    not exercised". It casts nothing.
        /// 2. Execution verification. ATTEMPTED AND CORRECTED, not discharged.
        ///    The inherited-template-removal stage of
        ///    disposable-alternate-summoning-natural casts an affected creature
        ///    selected from the live native menu BY WHAT ITS OWN GRAPH CARRIES
        ///    rather than by creature name, and inspects the spawned unit.
        ///
        ///    Measured on
        ///    KMG_Summoning_Adapter_MasterSummoner_Tier4_HoundArchon_One:
        ///    appliedByGraph=0, removalInGraph=true, one hound archon spawned
        ///    carrying the native summon marker and NO native template.
        ///
        ///    So the alternate-caller child does not copy the native Summon
        ///    Monster carrier; it is built from the caller's own host template,
        ///    which applies no celestial/fiendish buff anywhere in its graph.
        ///    On this path there is nothing to inherit and nothing to strip,
        ///    and GS-AUD-0025's description - "an alternate caller's untemplated
        ///    Summon Monster child kept the donor's template" - is NOT
        ///    reproducible on that route.
        ///
        ///    "Behaviour-neutral" here is an INFERENCE FROM SOURCE for the one
        ///    route measured, not a two-version native comparison: no
        ///    pre-repair build was deployed and observed. What was observed is
        ///    one post-repair cast whose graph applies no native template and
        ///    whose spawned unit carries none. Since the only change on that
        ///    route is an added removal action and nothing applies a template
        ///    there, the outcome cannot differ - but that last step is read
        ///    from the source, not executed.
        ///
        ///    An execution proof of removal EFFICACY can only come from the
        ///    canonical ordinary-spell path, where
        ///    inherited-template-removal-wiring-per-placement shows the apply
        ///    action is present in the registered graph. That observation is
        ///    GRAPH evidence: it reads action graphs and casts nothing, so it
        ///    establishes that an apply action exists, not that it executes or
        ///    leaves a buff behind. The execution proof is queued, not claimed
        ///    here.
        ///
        /// A passing graph observation alone leaves part 2 outstanding. Note
        /// also that the composition controls' original subjects were a
        /// TEMPLATED creature and a Nature's Ally creature, so they never
        /// entered the TemplateRemoval comparison branch at all; part 1 needs
        /// an affected subject, which composition-affected-subject-positive-
        /// control and its four negative controls supply.
        ///
        /// Membership below is a DECLARATION, and GS-REP-0002 records that it
        /// does not match the installed library. Do NOT read it as "the
        /// creatures that inherit a template" - that is what this comment used
        /// to say, and it is wrong in a way that matters:
        ///
        /// `inherited-template-removal-wiring-per-placement` measured 281
        /// untemplated Summon Monster PLACEMENTS across 63 creatures. 81
        /// placements contain a native-template apply action and 52 carry the
        /// removal, overlapping on only 16. Crucially
        /// `everyPlacementApplies=0`: not one creature applies on all of its
        /// own placements, because the determinant is the native variant
        /// NativeTemplate selects, keyed by (parent tier, multiplicity).
        ///
        /// So this declaration does not cover the placements that actually
        /// apply a template, and a per-creature key list cannot DESCRIBE which
        /// placements do.
        ///
        /// It does not follow that the repair must be structural, and an
        /// earlier version of this comment wrongly said so. Two different
        /// properties were being conflated:
        ///
        ///   - which placements CONTAIN a template-application action. That
        ///     varies by (parent tier, multiplicity) and can only be
        ///     established per placement.
        ///   - which creatures should RETAIN a celestial/fiendish template.
        ///     The approved contract is already per creature: TemplatePolicy
        ///     maps monsterTemplated=false to SummonTemplatePolicy.None.
        ///
        /// ContextActionRemoveInheritedSummonTemplates revalidates its owner
        /// and the four identities and then removes whichever of those buffs
        /// are actually present - so on a placement that applies none it
        /// removes nothing. A broader creature-based removal policy is
        /// therefore a live option: it would strip the template where one is
        /// applied and be inert elsewhere. Placement-aware wiring is another.
        /// The measurements do not prove either necessary.
        ///
        /// What the repair must do is enforce the approved no-template contract
        /// across the placements that apply one. Which implementation does that
        /// is for DEC-0004 and its own card.
        ///
        /// What the observation asserts is the per-placement WIRING invariant
        /// this code owns - the removal action is present exactly where its
        /// creature is declared - which holds. It does NOT assert the set is
        /// correct, and it does not "fail if a thirteenth creature inherits a
        /// template", which the superseded creature-level version claimed.
        /// </summary>
        internal static bool RequiresInheritedTemplateRemoval(SummonFamily family,
            SummonCreatureSpec creature)
        {
            if (creature == null) throw new ArgumentNullException("creature");
            return family == SummonFamily.Monster && !creature.MonsterTemplated &&
                Array.IndexOf(InheritedTemplateRemovalCreatures, creature.Key) >= 0;
        }

        /// <summary>
        /// Appends the inherited-template removal to every spawn branch, bound
        /// to its exact owning unit and the exact four native template
        /// identities the action itself revalidates at run time. Appended last
        /// so it is the final composed action of its branch, which is what the
        /// composition contract compares.
        /// </summary>
        internal static void AppendInheritedTemplateRemoval(
            BlueprintComponent[] components, BlueprintUnit unit,
            BlueprintBuff[] nativeTemplateBuffs)
        {
            if (components == null) throw new ArgumentNullException("components");
            if (unit == null) throw new ArgumentNullException("unit");
            if (nativeTemplateBuffs == null)
                throw new ArgumentNullException("nativeTemplateBuffs");
            foreach (var spawn in AlternateSummoningGraph
                .Objects<ContextActionSpawnMonster>(components))
            {
                if (spawn.AfterSpawn == null) spawn.AfterSpawn = new ActionList();
                var remove = ScriptableObject
                    .CreateInstance<ContextActionRemoveInheritedSummonTemplates>();
                remove.OwningBlueprint = unit;
                remove.NativeTemplates = (BlueprintBuff[])nativeTemplateBuffs.Clone();
                spawn.AfterSpawn.Actions = (spawn.AfterSpawn.Actions ??
                    Array.Empty<GameAction>())
                    .Concat(new GameAction[] { remove }).ToArray();
            }
        }

        /// <summary>Resolves the four native summon template buffs by their
        /// exact identities, for a caller that has only a library lookup.</summary>
        internal static BlueprintBuff[] ResolveNativeTemplateBuffs(
            Func<string, BlueprintScriptableObject> lookup)
        {
            if (lookup == null) throw new ArgumentNullException("lookup");
            var resolved = new BlueprintBuff[NativeMonsterTemplateBuffs.Length];
            for (int index = 0; index < NativeMonsterTemplateBuffs.Length; index++)
            {
                resolved[index] = lookup(NativeMonsterTemplateBuffs[index]) as BlueprintBuff;
                if (resolved[index] == null)
                    throw new InvalidOperationException(
                        "Native summon template buff missing: " +
                        NativeMonsterTemplateBuffs[index]);
            }
            return resolved;
        }

        internal static void Configure(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            SummonIconCatalog.Validate();
            SummonChoicePresentationPolicy.Validate();
            SummonClericAlignmentCatalog.Validate();
            ConfigureNativeTierOnePreservation(library, bySymbol,
                MonsterParents[0],
                ExpandedSummoningIdentityCatalog.NativeMonsterTierOneSymbol);
            ConfigureNativeTierOnePreservation(library, bySymbol,
                AllyParents[0],
                ExpandedSummoningIdentityCatalog.NativeNaturesAllyTierOneSymbol);
            BlueprintBuff[] replacedNativeTemplateBuffs =
                NativeMonsterTemplateBuffs.Select(guid =>
                    BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                        guid, "native summon template buff")).ToArray();
            foreach (SummonFamily family in new[] { SummonFamily.Monster,
                SummonFamily.NaturesAlly })
            foreach (SummonVariantSpec variant in
                ExpandedSummoningCatalog.GenerateVariants(family))
            {
                string symbol = ExpandedSummoningIdentityCatalog.AbilitySymbol(variant);
                BlueprintAbility ability = Require<BlueprintAbility>(bySymbol, symbol);
                BlueprintUnit unit = Require<BlueprintUnit>(bySymbol,
                    ExpandedSummoningIdentityCatalog.UnitSymbol(variant.Creature));
                BlueprintAbility native = NativeTemplate(library, variant);
                if (family == SummonFamily.Monster &&
                    variant.Creature.MonsterTemplated)
                {
                    BlueprintAbility celestial = Require<BlueprintAbility>(bySymbol,
                        symbol + ".Celestial");
                    BlueprintAbility fiendish = Require<BlueprintAbility>(bySymbol,
                        symbol + ".Fiendish");
                    SummonTemplateBand band = SummonTemplateBandPolicy.Select(
                        HitDice(unit));
                    BlueprintBuff celestialSmite = Require<BlueprintBuff>(bySymbol,
                        "KMG.Summoning.Smite.Celestial.Available");
                    BlueprintBuff fiendishSmite = Require<BlueprintBuff>(bySymbol,
                        "KMG.Summoning.Smite.Fiendish.Available");
                    ConfigureOne(celestial, native, unit, variant,
                        Require<BlueprintBuff>(bySymbol,
                            "KMG.Summoning.Template.Celestial." + band),
                        celestialSmite, replacedNativeTemplateBuffs, true,
                        SummonAlignmentMode.Celestial);
                    ConfigureOne(fiendish, native, unit, variant,
                        Require<BlueprintBuff>(bySymbol,
                            "KMG.Summoning.Template.Fiendish." + band),
                        fiendishSmite, replacedNativeTemplateBuffs, false,
                        SummonAlignmentMode.Fiendish);
                    ConfigureDynamicTemplate(ability, native, unit, variant,
                        Require<BlueprintBuff>(bySymbol,
                            "KMG.Summoning.Template.Celestial." + band),
                        Require<BlueprintBuff>(bySymbol,
                            "KMG.Summoning.Template.Fiendish." + band),
                        celestialSmite, fiendishSmite,
                        Require<BlueprintBuff>(bySymbol,
                            "KMG.Summoning.AlignmentMode.FiendishMarker"),
                        replacedNativeTemplateBuffs);
                }
                else ConfigureOne(ability, native, unit, variant, null, null,
                    replacedNativeTemplateBuffs, false,
                    family == SummonFamily.NaturesAlly ?
                        SummonAlignmentMode.Caster : (SummonAlignmentMode?)null);
            }
        }

        private static void ConfigureNativeTierOnePreservation(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            string parentGuid, string symbol)
        {
            BlueprintAbility parent = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(library, parentGuid,
                    "native direct tier-one summon parent");
            if ((parent.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AbilityVariants>().Any())
                throw new InvalidOperationException(
                    "Native tier-one preservation requires a direct ability: " +
                    parentGuid);
            BlueprintAbility target = Require<BlueprintAbility>(bySymbol, symbol);
            CopyFields(parent, target);
            target.name = InternalName(symbol);
            target.ComponentsArray = (parent.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(DeepCloneComponent).ToArray();
            target.MaterialComponent = parent.MaterialComponent == null ?
                new BlueprintAbility.MaterialComponentData() :
                parent.MaterialComponent;
        }

        private static BlueprintAbility NativeTemplate(
            LibraryScriptableObject library, SummonVariantSpec variant)
        {
            string guid = (variant.Family == SummonFamily.Monster ?
                MonsterParents : AllyParents)[variant.ParentTier - 1];
            BlueprintAbility parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, guid, "native summon parent tier " + variant.ParentTier);
            AbilityVariants variants = (parent.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<AbilityVariants>()
                .SingleOrDefault();
            if (variants == null)
            {
                if (variant.Multiplicity != SummonMultiplicity.One)
                    throw new InvalidOperationException(
                        "A direct summon parent cannot supply a quantity template.");
                return parent;
            }
            BlueprintAbility[] choices = variants.Variants ??
                Array.Empty<BlueprintAbility>();
            string token = variant.Multiplicity == SummonMultiplicity.One ? "single" :
                variant.Multiplicity == SummonMultiplicity.OneD3 ? "d3" : "d4";
            BlueprintAbility result = choices.SingleOrDefault(value => value != null &&
                value.name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
            if (result == null) throw new InvalidOperationException(
                "Native summon template missing for " + variant.StableKey + ".");
            return result;
        }

        private static void ConfigureOne(BlueprintAbility target,
            BlueprintAbility native, BlueprintUnit unit, SummonVariantSpec variant,
            BlueprintBuff templateBuff, BlueprintBuff smiteBuff,
            BlueprintBuff[] replacedNativeTemplateBuffs, bool celestial,
            SummonAlignmentMode? alignmentMode)
        {
            CopyFields(native, target);
            target.name = InternalName(ExpandedSummoningIdentityCatalog.AbilitySymbol(
                variant) + (templateBuff == null ? "" : celestial ?
                    ".Celestial" : ".Fiendish"));
            target.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(DeepCloneComponent).ToArray();
            int replacements = ReplaceSpawnUnits(target.ComponentsArray, unit);
            if (replacements < 1) throw new InvalidOperationException(
                "Expected at least one native spawn action for " + variant.StableKey + ".");
            if (templateBuff != null)
            {
                AppendTemplateBuff(target.ComponentsArray, templateBuff,
                    replacedNativeTemplateBuffs);
                AppendTemplateBuff(target.ComponentsArray, smiteBuff,
                    replacedNativeTemplateBuffs);
                var alignment = ScriptableObject.CreateInstance<AbilityCasterAlignment>();
                alignment.Alignment = celestial ? (AlignmentMaskType)63 :
                    (AlignmentMaskType)504;
                target.ComponentsArray = target.ComponentsArray.Concat(
                    new BlueprintComponent[] { alignment }).ToArray();
                SpellDescriptorComponent descriptor = target.ComponentsArray
                    .OfType<SpellDescriptorComponent>().Single();
                descriptor.Descriptor |= celestial ? SpellDescriptor.Good :
                    SpellDescriptor.Evil;
            }
            // Native SM carriers retain native celestial/fiendish after-spawn
            // behavior even when the project creature is untemplated, so these
            // scoped creatures need the removal action; unrelated released
            // graphs are untouched. GS-AUD-0025: the scoped set and the append
            // now live in one place so the alternate-caller adapter applies the
            // same rule instead of silently omitting it.
            if (RequiresInheritedTemplateRemoval(variant.Family, variant.Creature))
                AppendInheritedTemplateRemoval(target.ComponentsArray, unit,
                    replacedNativeTemplateBuffs);
            // The S30 Lemure has lawful/evil subtypes. Set the actual spell
            // descriptors on this new owned route, preserving class-specific
            // native eligibility rather than imposing a global caster alignment.
            if (variant.Creature.Key == "lemure")
                target.ComponentsArray.OfType<SpellDescriptorComponent>().Single()
                    .Descriptor |= SpellDescriptor.Law | SpellDescriptor.Evil;
            if (variant.Creature.Key == "dretch")
                target.ComponentsArray.OfType<SpellDescriptorComponent>().Single()
                    .Descriptor |= SpellDescriptor.Chaos | SpellDescriptor.Evil;
            if (variant.Creature.Key == "babau" ||
                variant.Creature.Key == "hezrou")
                target.ComponentsArray.OfType<SpellDescriptorComponent>().Single()
                    .Descriptor |= SpellDescriptor.Chaos | SpellDescriptor.Evil;
            if (variant.Creature.Key == "barbed-devil" ||
                variant.Creature.Key == "bone-devil")
                target.ComponentsArray.OfType<SpellDescriptorComponent>().Single()
                    .Descriptor |= SpellDescriptor.Law | SpellDescriptor.Evil;
            if (variant.Creature.Key == "hound-archon" || variant.Creature.Key == "lantern-archon" || variant.Creature.Key == "trumpet-archon")
                target.ComponentsArray.OfType<SpellDescriptorComponent>().Single()
                    .Descriptor |= SpellDescriptor.Law | SpellDescriptor.Good;
            if (variant.Creature.Key == "astral-deva")
            {
                SpellDescriptor astralDescriptor = AstralDevaRulesPolicy.HasGoodSummoningDescriptor ?
                    SpellDescriptor.Good : SpellDescriptor.None;
                if (AstralDevaRulesPolicy.HasLawSummoningDescriptor)
                    astralDescriptor |= SpellDescriptor.Law;
                target.ComponentsArray.OfType<SpellDescriptorComponent>().Single()
                    .Descriptor |= astralDescriptor;
            }
            if (alignmentMode.HasValue)
                AppendAlignmentAction(target.ComponentsArray,
                    alignmentMode.Value);
            AppendSpiderBenefitIfEligible(target.ComponentsArray, variant);
            AttachClericAlignmentGate(target, variant);
            target.Hidden = false;
            target.ActionBarAutoFillIgnored = templateBuff != null;
            target.MaterialComponent = native.MaterialComponent == null ?
                new BlueprintAbility.MaterialComponentData() : native.MaterialComponent;
            BlueprintUnitFactAccess.Resolve().Configure(target,
                LocalizationService.Create("KMG.ExpandedSummoning." +
                    variant.StableKey + (templateBuff == null ? "" : celestial ?
                        ".Celestial" : ".Fiendish") + ".Name",
                    SummonChoicePresentationPolicy.For(variant.Creature.Key,
                        variant.Multiplicity, templateBuff == null ?
                            SummonChoiceTemplate.None : celestial ?
                            SummonChoiceTemplate.Celestial :
                            SummonChoiceTemplate.Fiendish).Label),
                LocalizationService.Create("KMG.ExpandedSummoning." +
                    variant.StableKey + (templateBuff == null ? "" : celestial ?
                        ".Celestial" : ".Fiendish") + ".Description",
                    "Summons " + variant.Creature.DisplayName +
                    " through the native summon lifecycle." +
                    CreatureRulesText(variant.Creature.Key)), native.Icon);
        }

        private static void AttachClericAlignmentGate(BlueprintAbility target, SummonVariantSpec variant)
        {
            if (variant.Family != SummonFamily.Monster) return;
            AttachClericAlignmentGate(target, variant.Creature.Key, variant.Creature.MonsterTemplated);
        }

        internal static void AttachClericAlignmentGate(BlueprintAbility target, string creatureKey, bool monsterTemplated)
        {
            var gate = ScriptableObject.CreateInstance<SummonClericAlignmentGate>();
            gate.name = "$KMG_ClericAlignmentGate";
            gate.CreatureKey = creatureKey;
            gate.MonsterTemplated = monsterTemplated;
            target.ComponentsArray = target.ComponentsArray.Concat(new BlueprintComponent[] { gate }).ToArray();
            // CopyFields can bring a donor's cached providers along.
            AlternateSummoningAdapterBuilder.ResetComponentCaches(target);
        }

        private static void ConfigureDynamicTemplate(BlueprintAbility target,
            BlueprintAbility native, BlueprintUnit unit,
            SummonVariantSpec variant, BlueprintBuff celestialTemplate,
            BlueprintBuff fiendishTemplate, BlueprintBuff celestialSmite,
            BlueprintBuff fiendishSmite, BlueprintBuff neutralFiendishMode,
            BlueprintBuff[] replacedNativeTemplateBuffs)
        {
            CopyFields(native, target);
            target.name = InternalName(ExpandedSummoningIdentityCatalog
                .AbilitySymbol(variant));
            target.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Where(value =>
                    !(value is AbilityVariants) &&
                    !(value is AbilityCasterAlignment))
                .Select(DeepCloneComponent).ToArray();
            int replacements = ReplaceSpawnUnits(target.ComponentsArray, unit);
            if (replacements < 1) throw new InvalidOperationException(
                "Expected a direct native spawn graph for " +
                variant.StableKey + ".");
            AppendDynamicTemplate(target.ComponentsArray, celestialTemplate,
                fiendishTemplate, celestialSmite, fiendishSmite,
                neutralFiendishMode, replacedNativeTemplateBuffs);
            AppendSpiderBenefitIfEligible(target.ComponentsArray, variant);
            AttachClericAlignmentGate(target, variant);
            SpellDescriptorComponent descriptor = target.ComponentsArray
                .OfType<SpellDescriptorComponent>().Single();
            descriptor.Descriptor &= ~(SpellDescriptor.Good |
                SpellDescriptor.Evil);
            target.Hidden = false;
            target.ActionBarAutoFillIgnored = false;
            target.MaterialComponent = native.MaterialComponent == null ?
                new BlueprintAbility.MaterialComponentData() : native.MaterialComponent;
            BlueprintUnitFactAccess.Resolve().Configure(target,
                LocalizationService.Create("KMG.ExpandedSummoning." +
                    variant.StableKey + ".Name",
                    SummonChoicePresentationPolicy.For(variant.Creature.Key,
                        variant.Multiplicity).Label),
                LocalizationService.Create("KMG.ExpandedSummoning." +
                    variant.StableKey + ".Description",
                    "Summons a celestial or fiendish " +
                    variant.Creature.DisplayName + " based on the caster's " +
                    "alignment. Neutral casters use their persistent " +
                    "Fiendish Summoning mode." +
                    CreatureRulesText(variant.Creature.Key)), native.Icon);
        }

        private static string CreatureRulesText(string creatureKey)
        {
            if (creatureKey == "dire-rat")
                return " Its damaging bite exposes a target to native Filth Fever after a DC 11 Fortitude save.";
            if (creatureKey == "goblin-dog")
                return " It is immune to disease. Its dander causes a one-day allergic reaction (-2 Dexterity and -2 Charisma; Fortitude DC 12; nonstacking; positive magical healing or remove disease ends it) when its bite deals damage, when a natural weapon or unarmed attack deals damage to it, and when a creature attempts to grapple it. Goblinoid creatures are exempt; because Kingmaker has no Goblinoid subtype fact, the exemption is the exact enumerated set of native goblinoid unit types the installed library carries. Riding contact is omitted because mounted combat is out of scope.";
            return string.Empty;
        }

        internal static void AppendDynamicTemplate(
            IEnumerable<BlueprintComponent> components,
            BlueprintBuff celestialTemplate, BlueprintBuff fiendishTemplate,
            BlueprintBuff celestialSmite, BlueprintBuff fiendishSmite,
            BlueprintBuff neutralFiendishMode,
            BlueprintBuff[] replacedNativeTemplateBuffs)
        {
            var seen = new HashSet<object>(ReferenceComparer.Instance);
            int count = 0;
            foreach (BlueprintComponent component in components)
                AppendDynamicTemplate(component, celestialTemplate,
                    fiendishTemplate, celestialSmite, fiendishSmite,
                    neutralFiendishMode, replacedNativeTemplateBuffs, seen,
                    ref count);
            if (count < 1) throw new InvalidOperationException(
                "A dynamic templated summon requires a spawn branch.");
        }

        private static void AppendDynamicTemplate(object value,
            BlueprintBuff celestialTemplate, BlueprintBuff fiendishTemplate,
            BlueprintBuff celestialSmite, BlueprintBuff fiendishSmite,
            BlueprintBuff neutralFiendishMode,
            BlueprintBuff[] replacedNativeTemplateBuffs, ISet<object> seen,
            ref int count)
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                value is BlueprintScriptableObject || !seen.Add(value)) return;
            ContextActionSpawnMonster spawn = value as ContextActionSpawnMonster;
            if (spawn != null)
            {
                if (spawn.AfterSpawn == null) spawn.AfterSpawn = new ActionList();
                ContextActionApplySummonTemplateByCaster apply = ScriptableObject
                    .CreateInstance<ContextActionApplySummonTemplateByCaster>();
                apply.CelestialTemplate = celestialTemplate;
                apply.FiendishTemplate = fiendishTemplate;
                apply.CelestialSmite = celestialSmite;
                apply.FiendishSmite = fiendishSmite;
                apply.NeutralFiendishMode = neutralFiendishMode;
                apply.ReplacedNativeTemplateBuffs =
                    replacedNativeTemplateBuffs ?? Array.Empty<BlueprintBuff>();
                spawn.AfterSpawn.Actions = (spawn.AfterSpawn.Actions ??
                    Array.Empty<GameAction>()).Concat(new GameAction[] {
                        apply }).ToArray();
                count++;
            }
            foreach (FieldInfo field in Fields(value.GetType()))
            {
                object child = field.GetValue(value);
                IEnumerable sequence = child as IEnumerable;
                if (sequence != null && !(child is string))
                    foreach (object item in sequence)
                        AppendDynamicTemplate(item, celestialTemplate,
                            fiendishTemplate, celestialSmite, fiendishSmite,
                            neutralFiendishMode, replacedNativeTemplateBuffs,
                            seen, ref count);
                else AppendDynamicTemplate(child, celestialTemplate,
                    fiendishTemplate, celestialSmite, fiendishSmite,
                    neutralFiendishMode, replacedNativeTemplateBuffs, seen,
                    ref count);
            }
        }

        private static void AppendTemplateBuff(
            IEnumerable<BlueprintComponent> components, BlueprintBuff buff,
            BlueprintBuff[] replacedNativeTemplateBuffs)
        {
            var seen = new HashSet<object>(ReferenceComparer.Instance);
            int count = 0;
            foreach (BlueprintComponent component in components)
                AppendTemplateBuff(component, buff, replacedNativeTemplateBuffs,
                    seen, ref count);
            if (count < 1) throw new InvalidOperationException(
                "A templated summon requires at least one spawn branch.");
        }

        internal static void AppendAlignmentAction(
            IEnumerable<BlueprintComponent> components,
            SummonAlignmentMode mode)
        {
            var seen = new HashSet<object>(ReferenceComparer.Instance);
            int count = 0;
            foreach (BlueprintComponent component in components)
                AppendAlignmentAction(component, mode, seen, ref count);
            if (count < 1) throw new InvalidOperationException(
                "A summon alignment action requires at least one spawn branch.");
        }

        private static void AppendAlignmentAction(object value,
            SummonAlignmentMode mode, ISet<object> seen, ref int count)
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                value is BlueprintScriptableObject || !seen.Add(value)) return;
            ContextActionSpawnMonster spawn = value as ContextActionSpawnMonster;
            if (spawn != null)
            {
                if (spawn.AfterSpawn == null) spawn.AfterSpawn = new ActionList();
                ContextActionSetSummonAlignment alignment = ScriptableObject
                    .CreateInstance<ContextActionSetSummonAlignment>();
                alignment.Mode = mode;
                spawn.AfterSpawn.Actions = (spawn.AfterSpawn.Actions ??
                    Array.Empty<GameAction>()).Concat(new GameAction[] {
                        alignment
                    }).ToArray();
                count++;
            }
            foreach (FieldInfo field in Fields(value.GetType()))
            {
                object child = field.GetValue(value);
                IEnumerable sequence = child as IEnumerable;
                if (sequence != null && !(child is string))
                    foreach (object item in sequence)
                        AppendAlignmentAction(item, mode, seen, ref count);
                else AppendAlignmentAction(child, mode, seen, ref count);
            }
        }

        private static void AppendTemplateBuff(object value, BlueprintBuff buff,
            BlueprintBuff[] replacedNativeTemplateBuffs, ISet<object> seen,
            ref int count)
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                value is BlueprintScriptableObject || !seen.Add(value)) return;
            ContextActionSpawnMonster spawn = value as ContextActionSpawnMonster;
            if (spawn != null)
            {
                if (spawn.AfterSpawn == null) spawn.AfterSpawn = new ActionList();
                ContextActionApplySummonBuff apply = ScriptableObject
                    .CreateInstance<ContextActionApplySummonBuff>();
                apply.Buff = buff;
                apply.ReplacedNativeTemplateBuffs =
                    replacedNativeTemplateBuffs ?? Array.Empty<BlueprintBuff>();
                spawn.AfterSpawn.Actions = (spawn.AfterSpawn.Actions ??
                    Array.Empty<GameAction>()).Concat(new GameAction[] { apply }).ToArray();
                count++;
            }
            foreach (FieldInfo field in Fields(value.GetType()))
            {
                object child = field.GetValue(value);
                IEnumerable sequence = child as IEnumerable;
                if (sequence != null && !(child is string))
                    foreach (object item in sequence)
                        AppendTemplateBuff(item, buff,
                            replacedNativeTemplateBuffs, seen, ref count);
                else AppendTemplateBuff(child, buff,
                    replacedNativeTemplateBuffs, seen, ref count);
            }
        }

        internal static int HitDiceOf(BlueprintUnit unit) { return HitDice(unit); }

        // The ordinary Giant Spider keeps normal access. The action itself
        // grants nothing unless the caster holds the actual feat at cast time.
        private static void AppendSpiderBenefitIfEligible(BlueprintComponent[] components, SummonVariantSpec variant)
        {
            if (variant.Creature.Key == "giant-spider") AppendSpiderBenefit(components);
        }

        /// <summary>One benefit action on each spawn, so every spawned creature is covered.</summary>
        internal static void AppendSpiderBenefit(IEnumerable<BlueprintComponent> components)
        {
            int count = 0;
            foreach (ContextActionSpawnMonster spawn in AlternateSummoningGraph.Objects<ContextActionSpawnMonster>(components))
            {
                if (spawn.AfterSpawn == null) spawn.AfterSpawn = new ActionList();
                spawn.AfterSpawn.Actions = (spawn.AfterSpawn.Actions ?? Array.Empty<GameAction>())
                    .Concat(new GameAction[] { ScriptableObject.CreateInstance<ContextActionApplySpiderSummonerBenefit>() }).ToArray();
                count++;
            }
            if (count < 1) throw new InvalidOperationException("A spider summon requires a spawn action.");
        }

        private static int HitDice(BlueprintUnit unit)
        {
            int total = 0;
            foreach (BlueprintComponent component in unit.ComponentsArray ??
                Array.Empty<BlueprintComponent>())
            {
                if (component.GetType().Name != "AddClassLevels") continue;
                FieldInfo levels = Fields(component.GetType()).Single(value =>
                    value.Name == "Levels" && value.FieldType == typeof(int));
                total += (int)levels.GetValue(component);
            }
            return total;
        }

        private static string QuantitySuffix(SummonMultiplicity value)
        { return value == SummonMultiplicity.One ? "" : value ==
            SummonMultiplicity.OneD3 ? " (1d3)" : " (1d4+1)"; }

        /// <summary>The same deep clone for any action or value object.</summary>
        internal static object DeepCloneComponentObject(object source)
        { return DeepClone(source,
            new Dictionary<object, object>(ReferenceComparer.Instance)); }

        internal static BlueprintComponent DeepCloneComponent(BlueprintComponent source)
        { return (BlueprintComponent)DeepClone(source,
            new Dictionary<object, object>(ReferenceComparer.Instance)); }

        private static object DeepClone(object source, IDictionary<object, object> seen)
        {
            if (source == null) return null;
            Type type = source.GetType();
            if (type == typeof(ActionList))
            {
                ActionList actions = (ActionList)source;
                return new ActionList {
                    Actions = (actions.Actions ?? Array.Empty<GameAction>())
                        .Select(action => (GameAction)DeepClone(action, seen))
                        .ToArray()
                };
            }
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) ||
                type == typeof(decimal) || type.IsValueType) return source;
            BlueprintScriptableObject blueprint = source as BlueprintScriptableObject;
            if (blueprint != null) return blueprint;
            UnityEngine.Object unity = source as UnityEngine.Object;
            if (unity != null && !(source is BlueprintComponent) &&
                !(source is GameAction)) return unity;
            object existing;
            if (seen.TryGetValue(source, out existing)) return existing;
            Array array = source as Array;
            if (array != null)
            {
                Array copy = Array.CreateInstance(type.GetElementType(), array.Length);
                seen.Add(source, copy);
                for (int index = 0; index < array.Length; index++)
                    copy.SetValue(DeepClone(array.GetValue(index), seen), index);
                return copy;
            }
            object result = source is BlueprintComponent || source is GameAction ?
                ScriptableObject.CreateInstance(type) :
                FormatterServices.GetUninitializedObject(type);
            seen.Add(source, result);
            foreach (FieldInfo field in Fields(type))
            {
                if (field.IsInitOnly || field.DeclaringType == typeof(UnityEngine.Object))
                    continue;
                field.SetValue(result, DeepClone(field.GetValue(source), seen));
            }
            return result;
        }

        internal static int ReplaceSpawnUnits(IEnumerable<BlueprintComponent> components,
            BlueprintUnit unit)
        {
            int count = 0;
            var seen = new HashSet<object>(ReferenceComparer.Instance);
            foreach (BlueprintComponent component in components)
                Replace(component, unit, seen, ref count);
            return count;
        }

        private static void Replace(object value, BlueprintUnit unit,
            ISet<object> seen, ref int count)
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                value is BlueprintScriptableObject || !seen.Add(value)) return;
            Type type = value.GetType();
            foreach (FieldInfo field in Fields(type))
            {
                if (type.Name == "ContextActionSpawnMonster" &&
                    field.Name == "Blueprint" &&
                    typeof(BlueprintUnit).IsAssignableFrom(field.FieldType))
                { field.SetValue(value, unit); count++; continue; }
                object child = field.GetValue(value);
                IEnumerable sequence = child as IEnumerable;
                if (sequence != null && !(child is string))
                    foreach (object item in sequence) Replace(item, unit, seen, ref count);
                else Replace(child, unit, seen, ref count);
            }
        }

        internal static void CopyFields(BlueprintAbility source,
            BlueprintAbility target)
        {
            foreach (FieldInfo field in Fields(typeof(BlueprintAbility)))
            {
                if (field.Name == "m_AssetGuid" || field.IsInitOnly ||
                    field.DeclaringType == typeof(UnityEngine.Object)) continue;
                object value = field.GetValue(source);
                Array array = value as Array;
                field.SetValue(target, array == null ? value : array.Clone());
            }
        }

        private static IEnumerable<FieldInfo> Fields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (Type current = type; current != null; current = current.BaseType)
                foreach (FieldInfo field in current.GetFields(flags)) yield return field;
        }

        private static T Require<T>(IDictionary<string, BlueprintScriptableObject> values,
            string symbol) where T : BlueprintScriptableObject
        { return (T)values[symbol]; }
        private static string InternalName(string symbol)
        { return symbol.Replace('.', '_').Replace('-', '_'); }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj) { return RuntimeHelpers.GetHashCode(obj); }
        }
    }
}
