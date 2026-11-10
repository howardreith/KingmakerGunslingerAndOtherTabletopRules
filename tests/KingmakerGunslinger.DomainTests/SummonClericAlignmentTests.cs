using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SummonClericAlignmentTests
    {
        private const string Cleric = SummonClericAlignmentPolicy.ClericClassGuid;
        private const string Sorcerer = "b3a505fb61437dc4097f43c3f8f9a4cf";
        private const string Erastil = "afc775188deb7a44aa4cbde03512c671", Asmodeus = "a3a5ccc9c670e6f4ca4a686d23b89900";
        private const string Pharasma = "458750bc214ab2e44abdeae404ab22e9", Unknown = "ffffffffffffffffffffffffffffffff";
        private const int LG = 10, NG = 3, CG = 18, LN = 9, TN = 1, CN = 17, LE = 12, NE = 5, CE = 20;

        private static SummonClericFacts Facts(int alignment, params string[] deities)
        { return new SummonClericFacts(new[] { Cleric }, alignment, deities); }

        private static SummonSpellAlignment Row(string key, bool starred = false)
        { return SummonClericAlignmentCatalog.For(key, starred); }

        private static bool Allowed(SummonClericFacts facts, string key, bool starred = false)
        { return SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(facts, Row(key, starred))); }

        internal static void BothAxesAndDeityRestrictionsApply()
        {
            Assertions.False(Allowed(Facts(LG), "vrock"), "A lawful good cleric cannot summon a chaotic evil demon.");
            Assertions.True(Allowed(Facts(LG), "lantern-archon"), "A lawful good cleric may summon an archon.");
            Assertions.False(Allowed(Facts(LN), "bralani-azata"), "Lawful forbids the chaotic axis alone.");
            Assertions.True(Allowed(Facts(LN), "lantern-archon"), "Lawful neutral has no objection to lawful good: good is not opposed.");
            Assertions.False(Allowed(Facts(NG), "lemure"), "Good forbids the evil axis alone.");
            Assertions.False(Allowed(Facts(TN, Asmodeus), "lantern-archon"),
                "A neutral cleric of a lawful evil deity gains the deity's prohibition on Good.");
            Assertions.False(Allowed(Facts(TN, Asmodeus), "bralani-azata"), "The deity's prohibition covers Chaos too.");
            Assertions.True(Allowed(Facts(TN, Asmodeus), "lemure"), "The deity does not forbid its own alignment.");
            Assertions.False(Allowed(Facts(CN, Erastil), "lemure"), "Deity restrictions add to the character's.");
        }

        internal static void TrueNeutralAndStarredChoicesFollowTheCaster()
        {
            Assertions.True(Allowed(Facts(TN), "vrock"), "A true neutral cleric with no deity has no prohibition.");
            Assertions.True(Allowed(Facts(TN), "lantern-archon"), "Neutral contributes nothing.");
            Assertions.True(Allowed(Facts(TN, Pharasma), "vrock"), "A neutral deity contributes nothing.");
            Assertions.True(Allowed(Facts(TN), "wolf", true), "A neutral caster's starred choice is not a Good or Evil spell.");
            Assertions.True(Allowed(Facts(LE), "wolf", true), "A starred creature matches its evil caster.");
            Assertions.True(Allowed(Facts(LG, Erastil), "wolf", true), "A starred creature matches its good caster.");
            Assertions.True(Row("wolf", true).CasterDependent && Row("wolf", true).EffectiveFor(SummonAlignmentAxes.Evil) == SummonAlignmentAxes.Evil,
                "A starred spell takes the caster's alignment.");
            Assertions.False(Allowed(Facts(CN, Erastil), "wolf", true),
                "A starred spell matches the caster, so a lawful deity still forbids a chaotic caster's choice.");
        }

        internal static void TableExceptionsUseTheirPrintedSubtypes()
        {
            Assertions.True(Row("astral-deva").Descriptors == SummonAlignmentAxes.Good, "Astral Deva is Good only.");
            Assertions.True(Allowed(Facts(CG), "astral-deva"), "A chaotic good cleric may summon the Good-only Astral Deva.");
            Assertions.False(Allowed(Facts(CG), "trumpet-archon"), "A lawful archon stays forbidden to a chaotic cleric.");
            Assertions.True(Allowed(Facts(LG), "astral-deva"), "A lawful good cleric may summon it.");
            Assertions.False(Allowed(Facts(LG), "ghaele-azata"), "A chaotic azata stays forbidden to a lawful cleric.");
            Assertions.True(Row("salamander").Descriptors == SummonAlignmentAxes.Evil, "Salamander is printed Evil only.");
            Assertions.True(Allowed(Facts(CE), "salamander"), "A chaotic evil cleric may summon a Salamander.");
            Assertions.True(Row("xill").Descriptors == (SummonAlignmentAxes.Evil | SummonAlignmentAxes.Law), "Xill is Evil, Lawful.");
            Assertions.False(Allowed(Facts(CE), "xill"), "Chaotic evil opposes Xill's lawful subtype.");
            Assertions.True(Allowed(Facts(LG), "air-mephit") && Allowed(Facts(CE), "small-fire-elemental") && Allowed(Facts(LG), "invisible-stalker"),
                "Elemental and Air subtypes carry no alignment.");
            Assertions.True(Allowed(Facts(LG), "redcap"), "A fey with no alignment subtype adds no descriptor, whatever its alignment.");
            Assertions.False(Allowed(Facts(LG), "soul-eater"), "An Evil-subtype outsider is an Evil spell.");
            Assertions.False(Allowed(Facts(CG), "axiomite"), "A Lawful-subtype outsider is a Lawful spell.");
            Assertions.True(Row("gorgon").Authority.StartsWith("Bestiary", StringComparison.Ordinal), "An addition records its authority.");
        }

        internal static void EveryCurrentChoiceIsClassifiedAcrossQuantities()
        {
            SummonClericAlignmentCatalog.Validate();
            SummonVariantSpec[] variants = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).ToArray();
            Assertions.True(variants.Length > 0, "The census found no Summon Monster placements.");
            foreach (var group in variants.GroupBy(value => value.Creature.Key))
            {
                SummonSpellAlignment[] rows = group.Select(value => SummonClericAlignmentCatalog.For(value.Creature.Key, value.Creature.MonsterTemplated)).ToArray();
                Assertions.True(rows.All(value => value.CasterDependent == rows[0].CasterDependent && value.Descriptors == rows[0].Descriptors),
                    "A creature's legality is identical across its quantity variants: " + group.Key);
            }
            foreach (SummonNativeExpansionSpec native in SummonNativeExpansionCatalog.All.Where(value => value.Family == SummonFamily.Monster))
                Assertions.False(string.IsNullOrEmpty(SummonClericAlignmentCatalog.For(native.IconKey, false).Authority), "Native addition needs authority.");
            Assertions.Throws<InvalidOperationException>(() => SummonClericAlignmentCatalog.For("no-such-creature", false),
                "An unclassified row must not silently count as neutral.");
            Assertions.False(variants.Any(value => value.Creature.Key == "stirge"), "Stirge has no Summon Monster route.");
        }

        internal static void ClericMembershipComesFromRealClassLevels()
        {
            var multiclass = new SummonClericFacts(new[] { Sorcerer, Cleric }, LG, new string[0]);
            Assertions.False(SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(multiclass, Row("vrock"))),
                "A multiclass cleric stays restricted, whichever spellbook is used.");
            var sorcerer = new SummonClericFacts(new[] { Sorcerer }, LG, new[] { Erastil });
            Assertions.Equal(SummonClericDecision.NotCleric, SummonClericAlignmentPolicy.Decide(sorcerer, Row("vrock")),
                "A deity does not make a non-cleric a cleric.");
            var none = new SummonClericFacts(new string[0], LG, new[] { Erastil });
            Assertions.True(SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(none, Row("vrock"))),
                "A unit with no cleric class levels, even with a cloned cleric book, is unrestricted.");
        }

        internal static void UnknownDeityIsNeverNeutral()
        {
            Assertions.Equal(SummonClericDecision.ForbiddenUnknownDeity, SummonClericAlignmentPolicy.Decide(Facts(TN, Unknown), Row("vrock")),
                "An unidentified deity must not read as neutral.");
            Assertions.Equal(SummonClericDecision.Allowed, SummonClericAlignmentPolicy.Decide(Facts(TN, Unknown), Row("air-mephit")),
                "A row with no alignment needs no deity knowledge.");
            Assertions.True(Allowed(Facts(LG, Unknown), "wolf", true), "A starred row still matches its caster.");
            Assertions.True(Allowed(Facts(TN, SummonClericDeityCatalog.AtheismGuid), "vrock"), "Atheism is a known absence of deity.");
            Assertions.Equal(26, SummonClericDeityCatalog.Guids.Count(), "The installed selection's 25 deities and Atheism.");
            Assertions.False(Allowed(Facts(TN, "8b535b6842e063d48a571a042c3c6e8f"), "lantern-archon"), "Gyronna is chaotic evil.");
            Assertions.False(Allowed(Facts(TN, "0a111217f49248eabdf8e9abe2537973"), "vrock"), "Ragathiel is lawful good.");
        }

        internal static void TwoCastersShareRowsAndOrderIsPreserved()
        {
            string[] shared = { "air-mephit", "lantern-archon", "lemure", "salamander", "vrock", "wolf" };
            string[] before = shared.ToArray();
            Func<SummonClericFacts, string[]> menu = facts => SummonClericAlignmentPolicy.RetainPermitted(shared,
                key => SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(facts, Row(key, key == "wolf")))).ToArray();
            Assertions.Equal("air-mephit|lantern-archon|wolf", string.Join("|", menu(Facts(LG, Erastil))), "The lawful good cleric's own menu keeps order.");
            Assertions.Equal("air-mephit|salamander|vrock|wolf", string.Join("|", menu(Facts(CE, SummonClericDeityCatalog.AtheismGuid))),
                "The second caster gets their own menu from the same shared rows.");
            Assertions.True(shared.SequenceEqual(before), "Filtering must not mutate the shared rows.");
            Assertions.Equal("", string.Join("|", SummonClericAlignmentPolicy.RetainPermitted(new string[0], key => true)), "An empty list is handled.");
        }

        private static readonly SummonConversionRoute Herald = DruidSummonConversionPolicy.Herald;

        internal static void HeraldRouteMatchesItsTierAndQuantityArrays()
        {
            Assertions.Equal("8fd74eddd9b6c224693d9ab241f25e84", DruidSummonConversionPolicy.Expected(Herald, 1, SummonMultiplicity.One), "Tier I converts to the shared parent.");
            Assertions.Equal<string>(null, DruidSummonConversionPolicy.Expected(Herald, 1, SummonMultiplicity.OneD3), "Tier I has no group row.");
            Assertions.Equal("7ab27a0d547742741beb5d089f1c3852", DruidSummonConversionPolicy.Expected(Herald, 2, SummonMultiplicity.One), "Tier II single is the exact native row.");
            Assertions.Equal("6223958f3615eeb4d91e52ba57820878", DruidSummonConversionPolicy.Expected(Herald, 2, SummonMultiplicity.OneD3), "Tier II 1d3 is the exact native row.");
            Assertions.Equal("22b9580c961c5e64f95f10ca9a82c564", DruidSummonConversionPolicy.Expected(Herald, 9, SummonMultiplicity.OneD4PlusOne), "Tier IX 1d4+1 is the exact native row.");
            foreach (SummonMultiplicity quantity in Enum.GetValues(typeof(SummonMultiplicity)).Cast<SummonMultiplicity>())
            {
                var array = Enumerable.Range(0, 10).Select(level => level == 0 ? null : DruidSummonConversionPolicy.Expected(Herald, level, quantity)).ToArray();
                Assertions.True(DruidSummonConversionPolicy.MatchesArray(Herald, array, quantity), "The route accepts its own array: " + quantity);
                Assertions.False(DruidSummonConversionPolicy.MatchesArray(DruidSummonConversionPolicy.Druid, array, quantity), "A Herald array is not a Druid array.");
            }
            int tier; SummonMultiplicity resolved;
            Assertions.True(DruidSummonConversionPolicy.TryResolve(Herald, "22b9580c961c5e64f95f10ca9a82c564", out tier, out resolved) && tier == 9 && resolved == SummonMultiplicity.OneD4PlusOne, "Native rows resolve to tier and quantity.");
            Assertions.False(DruidSummonConversionPolicy.TryResolve(Herald, "b8ac9c653789b2a46ad85a075734c0e2", out tier, out resolved), "A Nature's Ally row is not a Herald conversion.");
            Assertions.True(DruidSummonConversionPolicy.RouteForFeature("f6c112a579a81c0439c597f9ee222243") == Herald && DruidSummonConversionPolicy.RouteForClass(Cleric) == Herald, "The route is keyed by exact identity.");
        }

        private sealed class ProjectionRow { internal string Id; }

        internal static void HeraldProjectionKeepsTheChainWithoutDuplicates()
        {
            var single = new ProjectionRow { Id = "native-single" }; var group = new ProjectionRow { Id = "native-d3" }; var other = new ProjectionRow { Id = "cure" };
            var wolf = new ProjectionRow { Id = "wolf" }; var bear = new ProjectionRow { Id = "bear" }; var ape = new ProjectionRow { Id = "ape" };
            ProjectionRow[] original = { single, other, group };
            ProjectionRow[] projected = DruidSummonConversionPolicy.Project(original, new Dictionary<ProjectionRow, ProjectionRow[]> { { single, new[] { wolf } }, { group, new[] { ape, bear } } });
            ProjectionRow[] parentOrder = { ape, bear, wolf };
            ProjectionRow[] ordered = DruidSummonConversionPolicy.OrderProjectedChoices(projected, row => { int index = Array.IndexOf(parentOrder, row); return index < 0 ? (int?)null : index; });
            Assertions.Equal("ape|cure|bear|wolf", string.Join("|", ordered.Select(value => value.Id)), "Children interleave alphabetically; an unrelated conversion keeps its place.");
            Assertions.Equal(ordered.Length, ordered.Distinct().Count(), "No row is repeated.");
            ProjectionRow[] preparedParent = DruidSummonConversionPolicy.Project(original, new Dictionary<ProjectionRow, ProjectionRow[]>(), new[] { single, group });
            Assertions.Equal("cure", string.Join("|", preparedParent.Select(value => value.Id)), "A prepared Summon Monster drops its own quantity rows, so no creature is listed twice.");
            Assertions.Equal("native-single|cure|native-d3", string.Join("|", original.Select(value => value.Id)), "The native list is unchanged.");
        }

        internal static void RetainedNativeRowsFollowTheirEffectiveBranch()
        {
            SummonNativeOptionSpec[] rows = SummonNativeOptionCatalog.All.Where(value => value.Family == SummonFamily.Monster && value.Tier >= 2).ToArray();
            Assertions.Equal(23, rows.Length, "All 23 native Herald rows above tier I are classified.");
            foreach (var row in rows)
                foreach (SummonAlignmentAxes caster in new[] { SummonAlignmentAxes.None, SummonAlignmentAxes.Good, SummonAlignmentAxes.Evil })
                    Assertions.False(SummonClericAlignmentCatalog.ForNativeOption(row, caster) == null, "Every native row classifies for every caster branch.");
            SummonNativeOptionSpec nineOne = rows.Single(value => value.Guid == "e96593e67d206ab49ad1b567327d1e75");
            Assertions.False(SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(Facts(LG, Erastil), SummonClericAlignmentCatalog.ForNativeOption(nineOne, SummonAlignmentAxes.Good | SummonAlignmentAxes.Law))),
                "A lawful good cleric's branch of the tier IX umbrella summons a Ghaele, a chaotic spell.");
            Assertions.True(SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(Facts(CE), SummonClericAlignmentCatalog.ForNativeOption(nineOne, SummonAlignmentAxes.Evil | SummonAlignmentAxes.Chaos))),
                "An evil cleric's branch of the same row is the Evil-subtype Thanadaemon, which is legal for them.");
            SummonNativeOptionSpec two = rows.Single(value => value.Guid == "7ab27a0d547742741beb5d089f1c3852");
            Assertions.True(SummonClericAlignmentCatalog.ForNativeOption(two, SummonAlignmentAxes.None).CasterDependent, "A semantic duplicate uses its starred creature's rule, not a union of branches.");
        }

        internal static void DruidRouteIsUnchanged()
        {
            Assertions.Equal(DruidSummonConversionPolicy.TierOneParentGuid, DruidSummonConversionPolicy.Expected(1, SummonMultiplicity.One), "The Druid route keeps its tier I parent.");
            Assertions.Equal("848bd9df8b2643143a7020be7cde8800", DruidSummonConversionPolicy.Expected(2, SummonMultiplicity.One), "The Druid route keeps its Nature's Ally rows.");
            Assertions.True(DruidSummonConversionPolicy.RouteForFeature(DruidSummonConversionPolicy.FeatureGuid) == DruidSummonConversionPolicy.Druid, "The Druid feature keeps its route.");
            Assertions.True(DruidSummonConversionPolicy.RouteForClass("b3a505fb61437dc4097f43c3f8f9a4cf") == null, "Other classes have no conversion route.");
        }

        private const string SpawnBuff = "50d51854cf6a3434d96a87d050e1d09a", Desna = "2c0a3b9971327ba4d9d85354d16998c1";
        private const string OgreIdentity = "130211b7fcb443e9b67d5561d75cf6a0SummonMonsterOgreSpider", TarantulaIdentity = "130211b7fcb443e9b67d5561d75cf6a0SummonMonsterGiantTarantula";
        private const string VrockUnit = "ffffffffffffffffffffffffffffff01";

        private static SummonSpawn ForeignSpawn(string unit, params string[] after)
        {
            List<string> detail = (after.Length == 0 ? new[] { "apply-buff:" + SpawnBuff } : after).ToList();
            return new SummonSpawn { Unit = unit, CountDice = "Zero", CountDiceCount = SummonValue.Simple(0), CountBonus = SummonValue.Simple(1),
                AfterSpawnDetail = detail, AfterSpawnApplications = detail.SelectMany(ApplicationsOf).ToList() };
        }

        // A fixture application: what the reader records for a native buff application.
        private const string NativeApplicationProperties = "permanent=True;useSeconds=False;seconds=0;duration=null;fromSpell=False;notDispelable=True;asChild=True";

        private static string Application(string buff) { return SummonSignatureFormat.Application(false, buff, NativeApplicationProperties); }

        private static IEnumerable<string> ApplicationsOf(string detail)
        {
            if (detail.StartsWith("apply-buff:", StringComparison.Ordinal)) return new[] { Application(detail.Substring("apply-buff:".Length)) };
            if (detail.StartsWith("apply-buff-to-caster:", StringComparison.Ordinal)) return new[] { SummonSignatureFormat.Application(true, detail.Substring("apply-buff-to-caster:".Length), NativeApplicationProperties) };
            if (!detail.StartsWith("conditional:", StringComparison.Ordinal)) return new string[0];
            return System.Text.RegularExpressions.Regex.Matches(detail, "apply-buff:([0-9a-f]{32})").Cast<System.Text.RegularExpressions.Match>().Select(match => Application(match.Groups[1].Value));
        }

        private static SummonSignature ForeignDirect(string unit, params string[] after)
        { return new SummonSignature { RunActions = 1, Actions = new List<SummonNode> { SummonNode.ForSpawn(ForeignSpawn(unit, after)) } }; }

        private static SummonSignature ForeignBranch(string condition, SummonSpawn ifTrue, SummonSpawn ifFalse)
        { return new SummonSignature { RunActions = 1, Actions = new List<SummonNode> { SummonNode.ForConditional("And", new[] { condition },
            new[] { SummonNode.ForSpawn(ifTrue) }, new[] { SummonNode.ForSpawn(ifFalse) }) } }; }

        private static SummonSignature OgreGraph()
        { return ForeignBranch(SummonForeignRowClassifier.CasterEvilCondition, ForeignSpawn(SummonForeignRowClassifier.RedcapUnit), ForeignSpawn(SummonForeignRowClassifier.BralaniUnit)); }

        private static IReadOnlyList<IReadOnlyList<string>> DonorOf(SummonSignature graph)
        { return graph.Spawns().Select(value => (IReadOnlyList<string>)value.AfterSpawnApplications.ToArray()).ToArray(); }

        private static SummonForeignRowReading Ogre(SummonSignature graph)
        { return new SummonForeignRowReading { Identity = OgreIdentity, InternalName = "SummonMonsterOgreSpider", PublishedTier = 5,
            RequiredFacts = new[] { SummonForeignRowClassifier.SpiderSummonerFeatureGuid }, VisibilityFacts = new[] { SummonForeignRowClassifier.SpiderSummonerFeatureGuid }, Graph = graph, NativeApplications = DonorOf(graph) }; }

        private static SummonForeignRowReading Tarantula(SummonSignature graph)
        { return new SummonForeignRowReading { Identity = TarantulaIdentity, InternalName = "SummonMonsterGiantTarantula", PublishedTier = 7,
            RequiredFacts = new[] { SummonForeignRowClassifier.SpiderSummonerFeatureGuid }, VisibilityFacts = new[] { SummonForeignRowClassifier.SpiderSummonerFeatureGuid }, Graph = graph, NativeApplications = DonorOf(graph) }; }

        private static bool ForeignAllowed(SummonClericFacts facts, SummonForeignRowResult result)
        { return SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(facts, result.AlignmentFor(SummonClericAlignmentPolicy.AxesOf(facts.AlignmentValue)))); }

        internal static void ForeignRowIsJudgedByItsExecutingBranch()
        {
            SummonForeignRowResult ogre = SummonForeignRowClassifier.Classify(Ogre(OgreGraph()));
            Assertions.Equal(SummonForeignContractOutcome.ConditionallyGoverned, ogre.Outcome, "A verified two-branch graph is conditionally governed.");
            Assertions.False(ForeignAllowed(Facts(LG, Erastil), ogre), "A lawful good caster's non-evil branch is the chaotic Bralani.");
            Assertions.True(ForeignAllowed(Facts(CG, Desna), ogre), "A chaotic good caster may summon the Bralani.");
            Assertions.True(ForeignAllowed(Facts(LE, Asmodeus), ogre), "An evil caster's branch is the Redcap, which has no alignment subtype.");
            Assertions.False(ForeignAllowed(Facts(LN, Asmodeus), ogre), "A non-evil caster barred by the deity is refused, with no union of branches.");
            Assertions.True(ogre.AlignmentFor(SummonAlignmentAxes.Evil).Descriptors == SummonAlignmentAxes.None, "The evil branch contributes no descriptor.");
            Assertions.True(ogre.AlignmentFor(SummonAlignmentAxes.Good).Descriptors == (SummonAlignmentAxes.Good | SummonAlignmentAxes.Chaos), "The non-evil branch is Good and Chaos.");
            SummonForeignRowResult tarantula = SummonForeignRowClassifier.Classify(Tarantula(ForeignDirect(SummonForeignRowClassifier.TarantulaUnit, "apply-buff:" + SummonForeignRowClassifier.SummonBuffFor(SummonFamily.Monster, 7))));
            Assertions.Equal(SummonForeignContractOutcome.VerifiedInherentlyLegal, tarantula.Outcome, "A verified direct spider is inherently legal.");
            Assertions.True(ForeignAllowed(Facts(LG, Erastil), tarantula) && ForeignAllowed(Facts(CE), tarantula), "A verified spider has no alignment descriptor.");
        }

        private static void AssertUnresolved(SummonForeignRowReading reading, string because)
        {
            SummonForeignRowResult result = SummonForeignRowClassifier.Classify(reading);
            Assertions.Equal(SummonForeignContractOutcome.Unresolved, result.Outcome, because);
            Assertions.True(result.AlignmentFor(SummonAlignmentAxes.Good) == null, "An unresolved row reports no alignment: " + because);
        }

        internal static void ForeignRowsAreCoveredOnlyByWhatTheyAre()
        {
            AssertUnresolved(Tarantula(ForeignDirect("eeeeeeeeeeeeeeeeeeeeeeeeeeeeee01")), "A recognized spider name with a different creature is not verified.");
            AssertUnresolved(Tarantula(ForeignDirect(VrockUnit)), "A recognized name spawning a forbidden outsider with no descriptor flags is not verified.");
            var wrongRequired = Tarantula(ForeignDirect(SummonForeignRowClassifier.TarantulaUnit)); wrongRequired.RequiredFacts = new[] { "eeeeeeeeeeeeeeeeeeeeeeeeeeeeee02" };
            AssertUnresolved(wrongRequired, "The wrong required feature is not verified.");
            var wrongVisibility = Tarantula(ForeignDirect(SummonForeignRowClassifier.TarantulaUnit)); wrongVisibility.VisibilityFacts = new string[0];
            AssertUnresolved(wrongVisibility, "A missing visibility feature is not verified.");
            AssertUnresolved(Ogre(ForeignBranch("Kingmaker.UnitLogic.Mechanics.Conditions.ContextConditionAlignment{Alignment=Good;CheckCaster=True;Not=False}", ForeignSpawn(SummonForeignRowClassifier.RedcapUnit), ForeignSpawn(SummonForeignRowClassifier.BralaniUnit))), "An altered branch condition is not verified.");
            AssertUnresolved(Ogre(ForeignBranch(SummonForeignRowClassifier.CasterEvilCondition, ForeignSpawn(SummonForeignRowClassifier.BralaniUnit), ForeignSpawn(SummonForeignRowClassifier.RedcapUnit))), "Reversed branch units are not verified.");
            AssertUnresolved(Ogre(ForeignBranch(SummonForeignRowClassifier.CasterEvilCondition, ForeignSpawn(SummonForeignRowClassifier.RedcapUnit), ForeignSpawn(VrockUnit))), "A branch with an unknown creature is not verified.");
            AssertUnresolved(Ogre(ForeignBranch(SummonForeignRowClassifier.CasterEvilCondition, ForeignSpawn(SummonForeignRowClassifier.RedcapUnit, "KingmakerGunslinger.Summoning.ContextActionApplySummonTemplateByCaster"), ForeignSpawn(SummonForeignRowClassifier.BralaniUnit))), "An unrecognized template action is not verified.");
            AssertUnresolved(Ogre(ForeignDirect(SummonForeignRowClassifier.BralaniUnit)), "A single reference does not satisfy a two-branch contract.");
            AssertUnresolved(Tarantula(OgreGraph()), "Two references do not satisfy a direct contract.");
            var unknown = Ogre(OgreGraph()); unknown.Identity = "130211b7fcb443e9b67d5561d75cf6a0SummonMonsterNewRow";
            AssertUnresolved(unknown, "An unknown identity is unresolved, however clean it looks.");
            var flagged = Tarantula(ForeignDirect(SummonForeignRowClassifier.TarantulaUnit)); flagged.LiveDescriptors = SummonAlignmentAxes.Evil;
            AssertUnresolved(flagged, "A row carrying alignment descriptors is not the verified contract.");
            var wrongTier = Tarantula(ForeignDirect(SummonForeignRowClassifier.TarantulaUnit)); wrongTier.PublishedTier = 6;
            AssertUnresolved(wrongTier, "A row published at another tier is not the verified contract.");
            var quantity = ForeignDirect(SummonForeignRowClassifier.TarantulaUnit); quantity.Actions[0].Spawn.CountBonus = SummonValue.Simple(2);
            AssertUnresolved(Tarantula(quantity), "A changed quantity is not the verified contract.");
            var hidden = OgreGraph(); hidden.HiddenSpawns = 1;
            AssertUnresolved(Ogre(hidden), "A spawn the reader cannot model is not the verified contract.");
        }

        internal static void HeraldProjectionAdmitsVerifiedForeignRowsAtTheirOwnTierAndQuantity()
        {
            Assertions.False(SummonForeignRowPolicy.AdmittedToConversion(SummonFamily.Monster, 5, SummonMultiplicity.One, SummonForeignRowClassifier.Classify(Ogre(OgreGraph())), true),
                "An unrepaired row, however exactly known, is never projected into a conversion route.");
            SpiderSpeciesSpec ogreSpecies = SpiderSummonerCatalog.Find("ogre-spider");
            SummonForeignRowResult ogre = SummonForeignRowClassifier.Classify(Row(SummonFamily.Monster, ogreSpecies, Repaired(SummonFamily.Monster, ogreSpecies)));
            Func<SummonClericFacts, bool, bool> canonical = (facts, feature) => feature && ForeignAllowed(facts, ogre);
            Func<SummonClericFacts, bool, int, SummonMultiplicity, bool> herald = (facts, feature, tier, quantity) =>
                SummonForeignRowPolicy.AdmittedToConversion(SummonFamily.Monster, tier, quantity, ogre, feature) && ForeignAllowed(facts, ogre);
            foreach (SummonClericFacts facts in new[] { Facts(CG, Desna), Facts(LG, Erastil), Facts(LE, Asmodeus) })
                Assertions.Equal(canonical(facts, true), herald(facts, true, 5, SummonMultiplicity.One), "The Herald projection agrees with the canonical choice for a feature-qualified cleric.");
            Assertions.True(herald(Facts(CG, Desna), true, 5, SummonMultiplicity.One), "A permitted row survives.");
            Assertions.True(herald(Facts(LG, Erastil), true, 5, SummonMultiplicity.One), "The repaired Ogre is a spider, permitted to a lawful good cleric.");
            Assertions.False(herald(Facts(CG, Desna), false, 5, SummonMultiplicity.One), "Without its feature the row is excluded, as natively.");
            Assertions.False(herald(Facts(CG, Desna), true, 5, SummonMultiplicity.OneD3), "No invented quantity bucket.");
            Assertions.False(herald(Facts(CG, Desna), true, 5, SummonMultiplicity.OneD4PlusOne), "No invented quantity bucket.");
            Assertions.False(herald(Facts(CG, Desna), true, 4, SummonMultiplicity.One), "No other tier.");
            Assertions.False(SummonForeignRowPolicy.AdmittedToConversion(SummonFamily.NaturesAlly, 5, SummonMultiplicity.One, ogre, true), "A Summon Monster row never enters a Summon Nature's Ally conversion route.");
            Assertions.False(SummonForeignRowPolicy.AdmittedToConversion(SummonFamily.Monster, 5, SummonMultiplicity.One, SummonForeignRowClassifier.Classify(Row(SummonFamily.Monster, ogreSpecies, ForeignDirect(VrockUnit))), true), "An unresolved row is never projected.");
            Assertions.False(SummonForeignRowPolicy.AdmittedToConversion(SummonFamily.Monster, 5, SummonMultiplicity.One, null, true), "An unknown row is never projected.");
        }

        private static SummonForeignRowReading Row(SummonFamily family, SpiderSpeciesSpec species, SummonSignature graph)
        { return new SummonForeignRowReading { Identity = SpiderSummonerCatalog.RowIdentity(family, species), InternalName = species.RowName(family), PublishedTier = species.BaseTier,
            RequiredFacts = new[] { SummonForeignRowClassifier.SpiderSummonerFeatureGuid }, VisibilityFacts = new[] { SummonForeignRowClassifier.SpiderSummonerFeatureGuid }, Graph = graph, NativeApplications = DonorOf(graph) }; }

        private static SummonSignature Direct(string unit, params string[] after)
        { return new SummonSignature { RunActions = 1, Actions = new List<SummonNode> { SummonNode.ForSpawn(ForeignSpawn(unit, after)) } }; }

        private static string Buff(SummonFamily family, int tier) { return "apply-buff:" + SummonForeignRowClassifier.SummonBuffFor(family, tier); }

        private static SummonSignature Repaired(SummonFamily family, SpiderSpeciesSpec species)
        { return Direct(species.UnitGuid, Buff(family, species.BaseTier), family == SummonFamily.Monster ? SummonForeignRowClassifier.TemplateAction : SummonForeignRowClassifier.AlignmentAction, SummonForeignRowClassifier.BenefitAction); }

        private static string TemplateStep(string celestial)
        { return "conditional:And[" + SummonForeignRowClassifier.CasterEvilCondition + "]{t:apply-buff:" + SummonForeignRowClassifier.FiendishTemplateI + "}{f:apply-buff:" + celestial + "}"; }

        internal static void SpiderSummonerMatrixMatchesTheTable()
        {
            SpiderSummonerCatalog.Validate();
            foreach (SummonFamily family in SpiderSummonerCatalog.Families)
            {
                SpiderPlacement[] placements = SpiderSummonerCatalog.Placements(family).ToArray();
                Assertions.Equal(23, placements.Length, "Twenty-three placements per family.");
                Assertions.Equal(4, placements.Count(value => value.IsOriginal), "Four original own-tier placements.");
                foreach (SpiderSpeciesSpec species in SpiderSummonerCatalog.All)
                    for (int tier = species.BaseTier; tier <= 9; tier++)
                    {
                        SummonMultiplicity expected = tier == species.BaseTier ? SummonMultiplicity.One : tier == species.BaseTier + 1 ? SummonMultiplicity.OneD3 : SummonMultiplicity.OneD4PlusOne;
                        Assertions.Equal(expected, placements.Single(value => value.Species == species && value.Tier == tier).Quantity, species.Key + " tier " + tier);
                    }
                Assertions.False(placements.Any(value => value.Tier < value.Species.BaseTier), "No placement below the species' base tier.");
                Assertions.Equal(placements.Length, placements.Select(value => value.Symbol).Distinct().Count(), "Each placement has its own stable symbol.");
            }
            Assertions.True(SpiderSummonerCatalog.QuantityExpressionMatches("D3", "Simple", 1, "Rank", SummonMultiplicity.OneD3), "1d3 keeps the donor's rank bonus.");
            Assertions.True(SpiderSummonerCatalog.QuantityExpressionMatches("D4", "Simple", 1, "Rank", SummonMultiplicity.OneD4PlusOne), "1d4+1 keeps the donor's rank bonus.");
            Assertions.False(SpiderSummonerCatalog.QuantityExpressionMatches("D4", "Simple", 1, "Simple", SummonMultiplicity.OneD4PlusOne), "A fixed bonus would bypass the rank Superior Summoning uses.");
            Assertions.False(SpiderSummonerCatalog.QuantityExpressionMatches("D3", "Simple", 1, "Rank", SummonMultiplicity.OneD4PlusOne), "The wrong die is rejected.");
            Assertions.False(SpiderSummonerCatalog.QuantityExpressionMatches("D3", "Simple", 2, "Rank", SummonMultiplicity.OneD3), "An altered die count is rejected.");
            Assertions.Equal("KMG.Summoning.Spider.SNA.Tier9.GiantTarantula.OneD4PlusOne",
                SpiderSummonerCatalog.Placements(SummonFamily.NaturesAlly).Single(value => value.Tier == 9 && value.Species.Key == "giant-tarantula").Symbol, "Identities derive from family, tier, species and quantity.");
        }

        internal static void SpiderRowsAcceptOnlyTheKnownInputAndRepairedShapes()
        {
            SpiderSpeciesSpec crab = SpiderSummonerCatalog.Find("giant-crab-spider"), widow = SpiderSummonerCatalog.Find("giant-black-widow"), ogre = SpiderSummonerCatalog.Find("ogre-spider"), tarantula = SpiderSummonerCatalog.Find("giant-tarantula");
            SummonSignature crabInput = Direct(crab.UnitGuid, Buff(SummonFamily.Monster, 1), TemplateStep(SummonForeignRowClassifier.CelestialTemplateI));
            SummonSignature widowInput = Direct(widow.UnitGuid, Buff(SummonFamily.Monster, 4), TemplateStep(SummonForeignRowClassifier.CelestialTemplateII));
            Assertions.True(SummonForeignRowClassifier.LegacyInputProblem(Row(SummonFamily.Monster, crab, crabInput)) == null, "The Crab row's known input is recognized.");
            Assertions.True(SummonForeignRowClassifier.LegacyInputProblem(Row(SummonFamily.Monster, widow, widowInput)) == null, "The Black Widow row's known input is recognized.");
            Assertions.False(SummonForeignRowClassifier.LegacyInputProblem(Row(SummonFamily.Monster, crab, Direct(crab.UnitGuid, Buff(SummonFamily.Monster, 1), TemplateStep(SummonForeignRowClassifier.CelestialTemplateII)))) == null, "A template the row does not carry is not its input.");
            Assertions.True(SummonForeignRowClassifier.LegacyInputProblem(Row(SummonFamily.Monster, ogre, ForeignBranch(SummonForeignRowClassifier.CasterEvilCondition, ForeignSpawn(SummonForeignRowClassifier.RedcapUnit, Buff(SummonFamily.Monster, 5)), ForeignSpawn(SummonForeignRowClassifier.BralaniUnit, Buff(SummonFamily.Monster, 5))))) == null, "The legacy Ogre branch is recognized for repair.");
            Assertions.True(SummonForeignRowClassifier.LegacyInputProblem(Row(SummonFamily.Monster, tarantula, Direct(tarantula.UnitGuid, Buff(SummonFamily.Monster, 7)))) == null, "The Tarantula row's known input is recognized.");
            foreach (SpiderSpeciesSpec species in SpiderSummonerCatalog.All)
            {
                string snaProblem = SummonForeignRowClassifier.LegacyInputProblem(Row(SummonFamily.NaturesAlly, species, Direct(species.UnitGuid, Buff(SummonFamily.NaturesAlly, species.BaseTier))));
                Assertions.True(snaProblem == null, "The Summon Nature's Ally row input is recognized: " + species.Key + " " + snaProblem);
                foreach (SummonFamily family in SpiderSummonerCatalog.Families)
                {
                    SummonForeignRowResult result = SummonForeignRowClassifier.Classify(Row(family, species, Repaired(family, species)));
                    Assertions.Equal("verified-repaired-spider-contract", result.Reason, "The repaired shape is its own contract: " + species.Key + " " + family);
                    Assertions.True(result.AlignmentFor(SummonAlignmentAxes.Good).CasterDependent, "A repaired row is caster-matched, not its donor's alignment.");
                }
            }
            Assertions.True(ForeignAllowed(Facts(LG, Erastil), SummonForeignRowClassifier.Classify(Row(SummonFamily.Monster, ogre, Repaired(SummonFamily.Monster, ogre)))), "A repaired Ogre is judged by the spider, not the former Bralani.");
            Assertions.True(ForeignAllowed(Facts(CE), SummonForeignRowClassifier.Classify(Row(SummonFamily.Monster, ogre, Repaired(SummonFamily.Monster, ogre)))), "A repaired Ogre remains caster-matched for an evil caster.");
            Func<SummonSignature, string> problem = graph => SummonForeignRowClassifier.LegacyInputProblem(Row(SummonFamily.Monster, tarantula, graph));
            Assertions.False(problem(Direct(tarantula.UnitGuid, "apply-buff-to-caster:" + SummonForeignRowClassifier.SummonBuffFor(SummonFamily.Monster, 7))) == null, "A changed buff recipient is not the known input.");
            Assertions.False(problem(Direct(tarantula.UnitGuid, Buff(SummonFamily.Monster, 7), "Some.Unexpected.Action")) == null, "An unexpected effect is not the known input.");
            Assertions.False(problem(Direct(VrockUnit, Buff(SummonFamily.Monster, 7))) == null, "A different unit is not the known input.");
            var count = Direct(tarantula.UnitGuid, Buff(SummonFamily.Monster, 7)); count.Actions[0].Spawn.CountBonus = SummonValue.Simple(2);
            Assertions.False(problem(count) == null, "A different count is not the known input.");
            var hidden = Direct(tarantula.UnitGuid, Buff(SummonFamily.Monster, 7)); hidden.HiddenSpawns = 1;
            Assertions.False(problem(hidden) == null, "A hidden spawn is not the known input.");
            var wrongFeature = Row(SummonFamily.Monster, tarantula, Direct(tarantula.UnitGuid, Buff(SummonFamily.Monster, 7))); wrongFeature.RequiredFacts = new[] { "eeeeeeeeeeeeeeeeeeeeeeeeeeeeee02" };
            Assertions.False(SummonForeignRowClassifier.LegacyInputProblem(wrongFeature) == null, "The wrong feature is not the known input.");
            var wrongTier = Row(SummonFamily.Monster, tarantula, Direct(tarantula.UnitGuid, Buff(SummonFamily.Monster, 7))); wrongTier.PublishedTier = 6;
            Assertions.False(SummonForeignRowClassifier.LegacyInputProblem(wrongTier) == null, "The wrong tier is not the known input.");
            Assertions.False(SummonForeignRowClassifier.LegacyInputProblem(Row(SummonFamily.Monster, crab, ForeignBranch("Kingmaker.UnitLogic.Mechanics.Conditions.ContextConditionAlignment{Alignment=Good;CheckCaster=True;Not=False}", ForeignSpawn(crab.UnitGuid), ForeignSpawn(crab.UnitGuid)))) == null, "An altered condition is not the known input.");
            Assertions.Equal(SummonForeignContractOutcome.Unresolved, SummonForeignRowClassifier.Classify(Row(SummonFamily.Monster, tarantula, Direct(tarantula.UnitGuid, Buff(SummonFamily.Monster, 7), SummonForeignRowClassifier.TemplateAction))).Outcome, "A template with no benefit action is not the repaired result.");
            Assertions.Equal(SummonForeignContractOutcome.Unresolved, SummonForeignRowClassifier.Classify(Row(SummonFamily.Monster, tarantula, Direct(tarantula.UnitGuid, Buff(SummonFamily.Monster, 7), SummonForeignRowClassifier.TemplateAction, SummonForeignRowClassifier.BenefitAction, SummonForeignRowClassifier.BenefitAction))).Outcome, "A doubled benefit action is not the repaired result.");
            var natureRepaired = Row(SummonFamily.NaturesAlly, tarantula, Direct(tarantula.UnitGuid, Buff(SummonFamily.NaturesAlly, 7), SummonForeignRowClassifier.TemplateAction, SummonForeignRowClassifier.BenefitAction));
            Assertions.Equal(SummonForeignContractOutcome.Unresolved, SummonForeignRowClassifier.Classify(natureRepaired).Outcome, "A Summon Nature's Ally row carrying a Summon Monster template is not the repaired result.");
            Assertions.True(SummonForeignRowClassifier.IsKnownIdentity(SpiderSummonerCatalog.RowIdentity(SummonFamily.NaturesAlly, ogre)), "All eight original rows are known identities.");
        }

        internal static void SpiderContractsAgreeAcrossRepairClassificationAndConversion()
        {
            SpiderSpeciesSpec crab = SpiderSummonerCatalog.Find("giant-crab-spider"), widow = SpiderSummonerCatalog.Find("giant-black-widow");
            Func<SummonForeignRowReading> crabInput = () => Row(SummonFamily.Monster, crab, Direct(crab.UnitGuid, Buff(SummonFamily.Monster, 1), TemplateStep(SummonForeignRowClassifier.CelestialTemplateI)));
            // Variants components: only the inherited published menu, only on tier I.
            var inherited = crabInput(); inherited.VariantsComponents = new[] { SummonForeignRowClassifier.InheritedPublishedMenu };
            Assertions.True(SummonForeignRowClassifier.LegacyInputProblem(inherited) == null, "A tier I row carrying the inherited published menu is the known input.");
            var foreign = crabInput(); foreign.VariantsComponents = new[] { "foreign:$Other_Variants" };
            Assertions.False(SummonForeignRowClassifier.LegacyInputProblem(foreign) == null, "An unfamiliar variants component makes the row unsupported, so it is left untouched.");
            Assertions.Equal(SummonForeignContractOutcome.Unresolved, SummonForeignRowClassifier.Classify(foreign).Outcome, "And it is not classified.");
            var both = crabInput(); both.VariantsComponents = new[] { SummonForeignRowClassifier.InheritedPublishedMenu, "foreign:$Other_Variants" };
            Assertions.False(SummonForeignRowClassifier.LegacyInputProblem(both) == null, "The inherited menu does not excuse an additional unfamiliar component.");
            var widowMenu = Row(SummonFamily.Monster, widow, Direct(widow.UnitGuid, Buff(SummonFamily.Monster, 4), TemplateStep(SummonForeignRowClassifier.CelestialTemplateII)));
            widowMenu.VariantsComponents = new[] { SummonForeignRowClassifier.InheritedPublishedMenu };
            Assertions.False(SummonForeignRowClassifier.LegacyInputProblem(widowMenu) == null, "Only a tier I row can inherit the published menu.");
            var repairedMenu = Row(SummonFamily.Monster, crab, Repaired(SummonFamily.Monster, crab)); repairedMenu.VariantsComponents = new[] { SummonForeignRowClassifier.InheritedPublishedMenu };
            Assertions.False(SummonForeignRowClassifier.Classify(repairedMenu).IsRepaired, "A repaired row carries no variants component of any kind.");
            // Lifecycle actions are required, not merely allowed.
            foreach (SummonFamily family in SpiderSummonerCatalog.Families)
            {
                var empty = Row(family, crab, Direct(crab.UnitGuid, new string[0])); empty.Graph.Actions[0].Spawn.AfterSpawnDetail.Clear(); empty.Graph.Actions[0].Spawn.AfterSpawnApplications.Clear();
                empty.NativeApplications = DonorOf(Direct(crab.UnitGuid, Buff(family, 1)));
                Assertions.False(SummonForeignRowClassifier.LegacyInputProblem(empty) == null, "An empty after-spawn list is not the known input: " + family);
                SummonForeignRowResult result = SummonForeignRowClassifier.Classify(empty);
                Assertions.Equal(SummonForeignContractOutcome.Unresolved, result.Outcome, "A row refused for repair is not classified as verified: " + family);
                Assertions.False(SummonForeignRowPolicy.AdmittedToConversion(family, 1, SummonMultiplicity.One, result, true), "And it is never projected into a conversion route: " + family);
            }
            var snaExact = Row(SummonFamily.NaturesAlly, crab, Direct(crab.UnitGuid, Buff(SummonFamily.NaturesAlly, 1)));
            Assertions.True(SummonForeignRowClassifier.LegacyInputProblem(snaExact) == null, "The exact unrepaired Summon Nature's Ally row is the repair input.");
            Assertions.False(SummonForeignRowPolicy.AdmittedToConversion(SummonFamily.NaturesAlly, 1, SummonMultiplicity.One, SummonForeignRowClassifier.Classify(snaExact), true), "An unrepaired row never enters the Druid route.");
            // The buff application itself, not only its identity and recipient.
            string summon = SummonForeignRowClassifier.SummonBuffFor(SummonFamily.NaturesAlly, 1);
            var shortened = Row(SummonFamily.NaturesAlly, crab, Direct(crab.UnitGuid, Buff(SummonFamily.NaturesAlly, 1)));
            shortened.NativeApplications = new IReadOnlyList<string>[] { new[] { SummonSignatureFormat.Application(false, summon, NativeApplicationProperties.Replace("permanent=True", "permanent=False")) } };
            Assertions.False(SummonForeignRowClassifier.LegacyInputProblem(shortened) == null, "A summon buff applied differently from the native donor is not the known input.");
            var repaired = Row(SummonFamily.NaturesAlly, crab, Repaired(SummonFamily.NaturesAlly, crab));
            repaired.NativeApplications = shortened.NativeApplications;
            Assertions.False(SummonForeignRowClassifier.Classify(repaired).IsRepaired, "A repaired row whose summon buff no longer matches the native application is not the repaired result.");
            var unreadable = Row(SummonFamily.NaturesAlly, crab, Repaired(SummonFamily.NaturesAlly, crab)); unreadable.NativeApplications = null;
            Assertions.False(SummonForeignRowClassifier.Classify(unreadable).IsRepaired, "Without a readable native donor nothing is verified.");
            var reordered = Row(SummonFamily.Monster, crab, Direct(crab.UnitGuid, SummonForeignRowClassifier.BenefitAction, Buff(SummonFamily.Monster, 1), SummonForeignRowClassifier.TemplateAction));
            Assertions.False(SummonForeignRowClassifier.Classify(reordered).IsRepaired, "The repaired lifecycle is an exact sequence.");
            var withAugment = Row(SummonFamily.Monster, crab, Direct(crab.UnitGuid, Buff(SummonFamily.Monster, 1), SummonForeignRowClassifier.TemplateAction, SummonForeignRowClassifier.BenefitAction, "conditional:And[x]{t:apply-buff:169d03bbccdbdc542ae1a57d83673d80}{f:}"));
            Assertions.False(SummonForeignRowClassifier.Classify(withAugment).IsRepaired, "No extra action is tolerated in the repaired result.");
        }

        internal static void SpiderRowsAdmittedToTheirOwnFamilyRouteOnly()
        {
            SpiderSpeciesSpec crab = SpiderSummonerCatalog.Find("giant-crab-spider");
            SummonForeignRowResult monster = SummonForeignRowClassifier.Classify(Row(SummonFamily.Monster, crab, Repaired(SummonFamily.Monster, crab)));
            SummonForeignRowResult ally = SummonForeignRowClassifier.Classify(Row(SummonFamily.NaturesAlly, crab, Repaired(SummonFamily.NaturesAlly, crab)));
            Assertions.True(SummonForeignRowPolicy.AdmittedToConversion(SummonFamily.Monster, 1, SummonMultiplicity.One, monster, true), "The repaired Crab enters a flattened Herald tier I conversion.");
            Assertions.True(SummonForeignRowPolicy.AdmittedToConversion(SummonFamily.NaturesAlly, 1, SummonMultiplicity.One, ally, true), "The repaired Summon Nature's Ally Crab enters the Druid route.");
            Assertions.False(SummonForeignRowPolicy.AdmittedToConversion(SummonFamily.Monster, 1, SummonMultiplicity.One, ally, true), "Families stay separate.");
            Assertions.False(SummonForeignRowPolicy.AdmittedToConversion(SummonFamily.Monster, 1, SummonMultiplicity.One, monster, false), "A caster without the feature's visibility is excluded.");
        }

        internal static void SpiderBenefitIsExactInScopeAndNeverAccumulates()
        {
            Func<string, bool> receives = unit => SpiderSummonerBenefitPolicy.Receives(unit, true, true);
            foreach (SpiderSpeciesSpec species in SpiderSummonerCatalog.All)
                Assertions.True(receives(species.UnitGuid), "A named spider receives the benefit: " + species.Key);
            Assertions.True(receives(SpiderSummonerCatalog.OrdinaryGiantSpiderUnit), "The ordinary Giant Spider receives the benefit.");
            foreach (string unit in new[] { "bebelith000000000000000000000000", "giant-scorpion000000000000000000", "9e120b5e0ad3c794491c049aa24b9fde", SummonForeignRowClassifier.BralaniUnit, null })
                Assertions.False(receives(unit), "An ineligible unit never receives the benefit: " + (unit ?? "null"));
            Assertions.False(SpiderSummonerBenefitPolicy.Receives(SpiderSummonerCatalog.OrdinaryGiantSpiderUnit, true, false), "A caster without the feat grants nothing.");
            Assertions.False(SpiderSummonerBenefitPolicy.Receives(SpiderSummonerCatalog.OrdinaryGiantSpiderUnit, false, true), "A non-spell source grants nothing.");
            Assertions.True(SpiderSummonerBenefitPolicy.AdjustsParametersOf(SpiderSummonerCatalog.GiantSpiderPoisonFeature), "The ordinary Giant Spider poison is covered.");
            Assertions.True(SpiderSummonerBenefitPolicy.AdjustsParametersOf(SpiderSummonerCatalog.GiantSpiderWebAbility), "The ordinary Giant Spider web is covered.");
            Assertions.True(SpiderSummonerBenefitPolicy.AdjustsParametersOf(SpiderSummonerCatalog.All[2].PoisonFeatureGuid), "A named spider's poison is covered.");
            Assertions.False(SpiderSummonerBenefitPolicy.AdjustsParametersOf("a719abac0ea0ce346b401060754cc1c0"), "The shared native webbed buff is never adjusted.");
            Assertions.False(SpiderSummonerBenefitPolicy.AdjustsParametersOf("56ec8788092b6314e8f3c1c502e8433f"), "The shared native poison buff is not a parameter owner.");
            Assertions.False(SpiderSummonerBenefitPolicy.AdjustsParametersOf(null), "A missing blueprint is never adjusted.");
        }

        internal static void ModuleBoundaryLeavesNativeAvailabilityUnchanged()
        {
            Func<string, SummonClericDecision?> forbidden = row => SummonClericDecision.ForbiddenByAlignment;
            Func<string, SummonClericDecision?> notCleric = row => SummonClericDecision.NotCleric;
            Func<string, SummonClericDecision?> ungoverned = row => null;
            Assertions.False(SummonClericModuleBoundary.Available(true, true, "row", forbidden), "Active module, forbidden cleric choice: refused.");
            Assertions.True(SummonClericModuleBoundary.Available(true, true, "row", notCleric), "A non-cleric gains no new restriction.");
            Assertions.True(SummonClericModuleBoundary.Available(true, true, "row", ungoverned), "An ungoverned row keeps its native result.");
            Assertions.True(SummonClericModuleBoundary.Available(false, true, "row", forbidden), "Inactive module: the native result is preserved.");
            Assertions.False(SummonClericModuleBoundary.Available(false, false, "row", notCleric), "Inactive module: native unavailability is preserved.");
            Assertions.False(SummonClericModuleBoundary.Available(true, false, "row", notCleric), "An unavailable native result is never re-enabled.");
            var cache = new SummonGovernedRowCache<string, SummonClericDecision?>(); var library = new object(); int computed = 0;
            Func<string, SummonClericDecision?> cached = row => cache.Get(library, row, value => { computed++; return SummonClericDecision.ForbiddenByAlignment; });
            Assertions.False(SummonClericModuleBoundary.Available(true, true, "row", cached), "The lookup populates while active.");
            Assertions.True(SummonClericModuleBoundary.Available(false, true, "row", cached), "Inactive after the cache is populated: still the native result.");
            Assertions.Equal(1, computed, "The populated lookup is not consulted while inactive.");
            Assertions.False(SummonClericModuleBoundary.Available(true, true, "row", cached), "The same cached decision applies again once active.");
            Assertions.Equal(1, computed, "The decision is cached per row.");
        }

        internal static void FixturePrerequisitesRejectMissingClassOrBook()
        {
            Func<RuntimeTesting.SummonClericObservationVerdict.FixtureReading> good = () => new RuntimeTesting.SummonClericObservationVerdict.FixtureReading {
                ClassLevel = 20, ExpectedLevel = 20, ClassBook = "book", RegisteredBooks = 1, RegisteredBook = "book", BookIsClassBook = true, BookList = "list", ExpectedList = "list",
                PreparedSlot = true, DeityMatchesPrerequisite = true, AlignmentIsExact = true };
            Assertions.Equal(0, RuntimeTesting.SummonClericObservationVerdict.FixtureProblems(good()).Count, "A complete fixture has no problems.");
            var noBook = good(); noBook.RegisteredBooks = 0; noBook.RegisteredBook = null; noBook.BookIsClassBook = false;
            Assertions.True(RuntimeTesting.SummonClericObservationVerdict.FixtureProblems(noBook).Count >= 2, "A cleric with no book is rejected, as on the failed candidate.");
            var noClass = good(); noClass.ClassLevel = 0;
            Assertions.True(RuntimeTesting.SummonClericObservationVerdict.FixtureProblems(noClass).Any(value => value.StartsWith("class-level", StringComparison.Ordinal)), "Missing class data is rejected.");
            var wrongList = good(); wrongList.BookList = "other";
            Assertions.True(RuntimeTesting.SummonClericObservationVerdict.FixtureProblems(wrongList).Contains("wrong-spell-list"), "A wrong spell list is rejected.");
            var noSlot = good(); noSlot.PreparedSlot = false;
            Assertions.True(RuntimeTesting.SummonClericObservationVerdict.FixtureProblems(noSlot).Contains("no-valid-prepared-slot"), "A missing prepared slot is rejected.");
            var fallback = good(); fallback.RegisteredBook = "default-book"; fallback.BookIsClassBook = false;
            Assertions.True(RuntimeTesting.SummonClericObservationVerdict.FixtureProblems(fallback).Contains("registered-book-differs-from-class-book"), "A fallback book that is not the class data's book is rejected.");
        }

        internal static void ValidDeityDerivedCaseIsolatesTheDeity()
        {
            string Abadar = "6122dacf418611540a3c91e67197ee4e";
            Assertions.True(Allowed(Facts(LN, Abadar), "astral-deva"), "A lawful neutral character under a lawful neutral deity may summon the Good-only Astral Deva.");
            Assertions.False(Allowed(Facts(LN, Asmodeus), "astral-deva"), "The same character under a lawful evil deity may not: the deity is the only source.");
        }

        internal static void HeraldCoverageFailsWhenAConversionIsUncovered()
        {
            var parent = new RuntimeTesting.SummonClericObservationVerdict.HeraldConversion { Guid = "parent", SharedParent = true };
            var covered = new RuntimeTesting.SummonClericObservationVerdict.HeraldConversion { Guid = "covered", Projects = true, ExecutionGoverned = true };
            var unguarded = new RuntimeTesting.SummonClericObservationVerdict.HeraldConversion { Guid = "unguarded", Projects = true, ExecutionGoverned = false };
            var unprojected = new RuntimeTesting.SummonClericObservationVerdict.HeraldConversion { Guid = "unprojected", ExecutionGoverned = true };
            Assertions.Equal(0, RuntimeTesting.SummonClericObservationVerdict.UncoveredHeraldConversions(new[] { parent, covered }).Count, "Enforced conversions pass.");
            Assertions.Equal("unguarded|unprojected", string.Join("|", RuntimeTesting.SummonClericObservationVerdict.UncoveredHeraldConversions(new[] { parent, unguarded, unprojected })), "Route identification is not acceptance.");
        }

        internal static void ChangesAfterAChoiceWasObtainedAreReevaluated()
        {
            SummonSpellAlignment archon = Row("lantern-archon");
            Assertions.True(SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(Facts(LG, Erastil), archon)), "Legal before the shift.");
            Assertions.False(SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(Facts(LE, Erastil), archon)),
                "The same held choice is refused once the caster's alignment moves.");
            Assertions.False(SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(Facts(TN, Asmodeus), archon)),
                "The same held choice is refused after a deity change.");
        }
    }
}
