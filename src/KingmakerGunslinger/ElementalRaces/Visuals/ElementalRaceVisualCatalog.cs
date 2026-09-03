using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces.Visuals
{
    internal static class ElementalRaceVisualCatalog
    {
        internal const int RaceVisualBlueprintCount = 4;
        internal const int BlueprintIdentityCount =
            ElementalRaceCatalog.RaceCount * RaceVisualBlueprintCount;
        internal const int ResourceIdentityCount = 28;
        internal const int SkinRampCount = 7;
        internal const string EmptyAssetId =
            "f283c87e2db77e648a7d4c1ae1b1f79c";

        internal const string IfritBody = "KMG.ElementalRaces.Ifrit.Visual.Body";
        internal const string IfritPresetStandard = "KMG.ElementalRaces.Ifrit.Visual.Preset.Standard";
        internal const string IfritPresetHeavy = "KMG.ElementalRaces.Ifrit.Visual.Preset.Heavy";
        internal const string IfritPresetSlender = "KMG.ElementalRaces.Ifrit.Visual.Preset.Slender";
        internal const string OreadBody = "KMG.ElementalRaces.Oread.Visual.Body";
        internal const string OreadPresetStandard = "KMG.ElementalRaces.Oread.Visual.Preset.Standard";
        internal const string OreadPresetHeavy = "KMG.ElementalRaces.Oread.Visual.Preset.Heavy";
        internal const string OreadPresetSlender = "KMG.ElementalRaces.Oread.Visual.Preset.Slender";
        internal const string SylphBody = "KMG.ElementalRaces.Sylph.Visual.Body";
        internal const string SylphPresetStandard = "KMG.ElementalRaces.Sylph.Visual.Preset.Standard";
        internal const string SylphPresetHeavy = "KMG.ElementalRaces.Sylph.Visual.Preset.Heavy";
        internal const string SylphPresetSlender = "KMG.ElementalRaces.Sylph.Visual.Preset.Slender";
        internal const string UndineBody = "KMG.ElementalRaces.Undine.Visual.Body";
        internal const string UndinePresetStandard = "KMG.ElementalRaces.Undine.Visual.Preset.Standard";
        internal const string UndinePresetHeavy = "KMG.ElementalRaces.Undine.Visual.Preset.Heavy";
        internal const string UndinePresetSlender = "KMG.ElementalRaces.Undine.Visual.Preset.Slender";

        private static readonly ElementalRaceNativeVisualAsset Empty = A(
            EmptyAssetId,
            "EE_FacialandHair_Empty_U_Any");
        private static readonly ElementalRaceNativeVisualAsset AasimarMaleBody =
            A("61a8f7b272e6a08499794018da672892", "EE_Naked_M_AS");
        private static readonly ElementalRaceNativeVisualAsset AasimarFemaleBody =
            A("30b6ec56e86c64c46b2c05c4b2f49ebe", "EE_Nude_F_AS");
        private static readonly ElementalRaceNativeVisualAsset HumanMaleBody =
            A("9de0a2db83f2dc3489781466970aa10c", "EE_Naked_M_HM");
        private static readonly ElementalRaceNativeVisualAsset HumanFemaleBody =
            A("a32ecc7c82240af488ad0f62ea01b7ad", "EE_Nude_F_HM");
        private static readonly ElementalRaceNativeVisualAsset TieflingMaleBody =
            A("742d39424f3afdc4aa2fa2666292f7c7", "EE_Naked_M_TF");
        private static readonly ElementalRaceNativeVisualAsset TieflingFemaleBody =
            A("9ec1bf45f668a324bb3d64a51dee99c3", "EE_Naked_F_TF");

        private static readonly ElementalRaceNativeVisualAsset HumanSkinSource =
            A("632957a5e5d53884692a74e01e6378bd", "EE_Head_Face01_M_HM");
        private static readonly ElementalRaceNativeVisualAsset AasimarSkinSource =
            A("944a6c1d75a489b43bb9535fcb164d3c", "EE_Head_Face01_M_AS");
        private static readonly ElementalRaceNativeVisualAsset TieflingSkinSource =
            A("5a0a2a2f0c8081e4b846f2226b85ea11", "EE_Head_Face01_M_TF");
        private static readonly ElementalRaceNativeVisualAsset ElfSkinSource =
            A("0562049a4c9ee9c4a9b70cae2edadf62", "EE_Head_Face01_M_EL");
        private static readonly ElementalRaceNativeVisualAsset HalfOrcSkinSource =
            A("0999a00ea6c69f041912158f5a838fad", "EE_Head_Face01_M_HO");

        private static readonly ElementalRaceVisualDefinition[] Definitions =
        {
            BuildIfrit(), BuildOread(), BuildSylph(), BuildUndine()
        };

        internal static IReadOnlyList<ElementalRaceVisualDefinition> Ordered()
        {
            Validate();
            return (ElementalRaceVisualDefinition[])Definitions.Clone();
        }

        internal static string[] BlueprintSymbols()
        {
            return Definitions.SelectMany(value => new[]
            {
                value.BodyBlueprintSymbol,
                value.PresetSymbols[0], value.PresetSymbols[1],
                value.PresetSymbols[2]
            }).ToArray();
        }

        internal static string[] ResourceSymbols()
        {
            return Definitions.SelectMany(value =>
                value.Male.Proxies().Concat(value.Female.Proxies()))
                .Select(value => value.Symbol).ToArray();
        }

        internal static void Validate()
        {
            if (Definitions.Length != ElementalRaceCatalog.RaceCount ||
                Definitions[0].Kind != ElementalRaceKind.Ifrit ||
                Definitions[1].Kind != ElementalRaceKind.Oread ||
                Definitions[2].Kind != ElementalRaceKind.Sylph ||
                Definitions[3].Kind != ElementalRaceKind.Undine)
                throw new InvalidOperationException(
                    "Elemental visual definitions must remain in race order.");
            string[] blueprints = BlueprintSymbols();
            string[] resources = ResourceSymbols();
            string[] all = blueprints.Concat(resources).ToArray();
            if (blueprints.Length != BlueprintIdentityCount ||
                resources.Length != ResourceIdentityCount ||
                all.Any(string.IsNullOrWhiteSpace) ||
                all.Distinct(StringComparer.Ordinal).Count() != all.Length)
                throw new InvalidOperationException(
                    "Elemental visual identity inventory drifted or collided.");
            foreach (ElementalRaceVisualDefinition definition in Definitions)
                if (definition.SkinPalette.Length != SkinRampCount)
                    throw new InvalidOperationException(
                        definition.Kind + " must expose exactly " +
                        SkinRampCount + " stable skin-ramp indexes.");
        }

        private static ElementalRaceVisualDefinition BuildIfrit()
        {
            var male = new ElementalRaceSexVisualDefinition(
                P("KMG.ElementalRaces.Ifrit.Visual.Body.Male",
                    TieflingMaleBody, AasimarMaleBody, true),
                new[]
                {
                    P("KMG.ElementalRaces.Ifrit.Visual.Head.Male.01",
                        A("5a0a2a2f0c8081e4b846f2226b85ea11",
                            "EE_Head_Face01_M_TF"),
                        A("944a6c1d75a489b43bb9535fcb164d3c",
                            "EE_Head_Face01_M_AS"), true),
                    P("KMG.ElementalRaces.Ifrit.Visual.Head.Male.02",
                        A("4360cadfdf62ad04cb8a508d413ab9e4",
                            "EE_Head_Face02_M_TF"),
                        A("a168fdf6428df1c44a08b6bb75f8a0d4",
                            "EE_Head_Face02_M_AS"), true)
                },
                new[]
                {
                    A("d37eb08d7aa2bce4d8cd3e807e185b12",
                        "EE_Hair_HairShort_M_HM"),
                    A("6a812788ed84107468921a8e80937cda",
                        "EE_Hair_HairMedium_M_HM"),
                    A("a9558cfc0705d4e48af7ecd2ebd75411",
                        "EE_Hair_HairLongWavy_M_HM"),
                    A("79d6abe5ac201d54c852d7f4a452876f",
                        "EE_Hair_HairLongCurly_M_HM"),
                    Empty
                },
                new[]
                {
                    A("9edf6b60bbf4d834facd4789837a3e0b",
                        "EE_Eyebrows_Face01_M_HM"),
                    A("a924d58e53cf54046a5242b1d9d4ca56",
                        "EE_Eyebrows_Face02_M_HM")
                },
                new[]
                {
                    Empty,
                    A("5164572594050ae4ebf0f1982273e203",
                        "EE_Facial_Bristle_M_HM"),
                    A("108a90f9e754aa24092d4372f08ce98b",
                        "EE_Facial_BeardShort_M_HM"),
                    A("d930decdcdf2e0046a9251e56c841feb",
                        "EE_Facial_BeardMedium_M_HM")
                },
                new[]
                {
                    P("KMG.ElementalRaces.Ifrit.Visual.Horn.Male.01",
                        A("a5bb53ee34384a541b80ce7aa207f0ef",
                            "EE_HornsTieflingRam_M_TF"), Empty, false),
                    P("KMG.ElementalRaces.Ifrit.Visual.Horn.Male.02",
                        A("867c97950369353479962d4a77d1802e",
                            "EE_HornsTieflingChamois_M_TF"), Empty, false)
                });
            var female = new ElementalRaceSexVisualDefinition(
                P("KMG.ElementalRaces.Ifrit.Visual.Body.Female",
                    TieflingFemaleBody, AasimarFemaleBody, true),
                new[]
                {
                    P("KMG.ElementalRaces.Ifrit.Visual.Head.Female.01",
                        A("29af70c10c0ffc940915e391c86e9cc0",
                            "EE_Head_Face01_F_TF"),
                        A("8ec7909c9d82d2a4f8ab6e2b0c63e1ef",
                            "EE_Head_Face01_F_AS"), true),
                    P("KMG.ElementalRaces.Ifrit.Visual.Head.Female.02",
                        A("e0cd7483cb920c94ab50caeb3b0156b5",
                            "EE_Head_Face02_F_TF"),
                        A("c1f41e469923ecf43a6c8050318c5e13",
                            "EE_Head_Face02_F_AS"), true)
                },
                new[]
                {
                    A("9c056dfe89108d04783971d02e86c3a6",
                        "EE_Hair_HairLong_F_HM"),
                    A("2c853a2e9e482ae4fb3a7719235d52ba",
                        "EE_Hair_HairSlick_F_HM"),
                    A("a2dce84d8be76d242b364083296bbc73",
                        "EE_Hair_PonytailClassic_F_HM"),
                    A("994ec31442e763d4580ac72cd8ef6108",
                        "EE_Hair_HairLongWavy_F_HM"),
                    Empty
                },
                new[]
                {
                    A("2825e6468fcea8848aa29b8941650081",
                        "EE_Eyebrows_Face01_F_HM"),
                    A("102d9e70bb8a34446847096b8087a4dd",
                        "EE_Eyebrows_Face02_F_HM")
                },
                new ElementalRaceNativeVisualAsset[0],
                new[]
                {
                    P("KMG.ElementalRaces.Ifrit.Visual.Horn.Female.01",
                        A("7972b6c4342aac94995412fe4fee299b",
                            "EE_HornsTieflingRam_F_TF"), Empty, false),
                    P("KMG.ElementalRaces.Ifrit.Visual.Horn.Female.02",
                        A("9559248f2345a1d40a429f5336ab4e93",
                            "EE_HornsTieflingChamois_F_TF"), Empty, false)
                });
            return D(ElementalRaceKind.Ifrit, IfritBody,
                IfritPresetStandard, IfritPresetHeavy, IfritPresetSlender,
                male, female, new[]
                {
                    R(TieflingSkinSource, "CL_Skin_U_TF",
                        "CR_Skin_Red_U_TF"),
                    R(TieflingSkinSource, "CL_Skin_U_TF",
                        "CR_Skin_DarkRed_U_TF"),
                    R(TieflingSkinSource, "CL_Skin_U_TF",
                        "CR_Skin_Brown_U_TF"),
                    R(TieflingSkinSource, "CL_Skin_U_TF",
                        "CR_Skin_Golden_U_TF"),
                    R(TieflingSkinSource, "CL_Skin_U_TF",
                        "CR_Skin_Black2_U_TF"),
                    R(TieflingSkinSource, "CL_Skin_U_TF",
                        "CR_Skin_Tan_U_TF"),
                    R(TieflingSkinSource, "CL_Skin_U_TF",
                        "CR_Skin_Pale_U_TF")
                });
        }

        private static ElementalRaceVisualDefinition BuildOread()
        {
            var male = new ElementalRaceSexVisualDefinition(
                P("KMG.ElementalRaces.Oread.Visual.Body.Male",
                    HumanMaleBody, AasimarMaleBody, true),
                new[]
                {
                    P("KMG.ElementalRaces.Oread.Visual.Head.Male.01",
                        A("a6fbea7cf9f513e428b67109055e034d",
                            "EE_Head_Face05_M_HM"),
                        A("3dd7d2374aca7bd40bb6d9359f3fe061",
                            "EE_Head_Face03_M_AS"), true),
                    P("KMG.ElementalRaces.Oread.Visual.Head.Male.02",
                        A("beb94c288b261af42ab29fa542ea6f4b",
                            "EE_Head_Face06_M_HM"),
                        A("735afffaddc84524db32796b68db203b",
                            "EE_Head_Face04_M_AS"), true)
                },
                new[]
                {
                    A("d37eb08d7aa2bce4d8cd3e807e185b12",
                        "EE_Hair_HairShort_M_HM"),
                    A("62cf1dbe82c90384cb658ac3499325d7",
                        "EE_Hair_Mohawk_M_HM"),
                    A("79d6abe5ac201d54c852d7f4a452876f",
                        "EE_Hair_HairLongCurly_M_HM"),
                    A("686e5b707afa91d4eba7f3b7e94859ec",
                        "EE_Hair_HairUncombed_M_HM"),
                    Empty
                },
                new[]
                {
                    A("d91987ba67e06fa4ab5a10a929214e7f",
                        "EE_Eyebrows_Face05_M_HM"),
                    A("067044941a2425a439d877dee84ced20",
                        "EE_Eyebrows_Face06_M_HM")
                },
                new[]
                {
                    Empty,
                    A("108a90f9e754aa24092d4372f08ce98b",
                        "EE_Facial_BeardShort_M_HM"),
                    A("d930decdcdf2e0046a9251e56c841feb",
                        "EE_Facial_BeardMedium_M_HM"),
                    A("8a7d63a7231caad4fa9b96f8e030c76d",
                        "EE_Facial_BeardWiseman_M_HM"),
                    A("cdc1cce085875fd459c860940c15950b",
                        "EE_Facial_BeardOldmanLong_M_HM")
                }, new ElementalRaceVisualProxySpec[0]);
            var female = new ElementalRaceSexVisualDefinition(
                P("KMG.ElementalRaces.Oread.Visual.Body.Female",
                    HumanFemaleBody, AasimarFemaleBody, true),
                new[]
                {
                    P("KMG.ElementalRaces.Oread.Visual.Head.Female.01",
                        A("2ed69e4781fe4e1489a2a82c41d984ac",
                            "EE_Head_Face04_F_HM"),
                        A("e99fa443817bee540a443bb9c574f8ca",
                            "EE_Head_Face03_F_AS"), true),
                    P("KMG.ElementalRaces.Oread.Visual.Head.Female.02",
                        A("883ba9509773be549a9175332376cc1a",
                            "EE_Head_Face05_F_HM"),
                        A("f14681e91b52dfd49b700920154e7ed3",
                            "EE_Head_Face04_F_AS"), true)
                },
                new[]
                {
                    A("9c056dfe89108d04783971d02e86c3a6",
                        "EE_Hair_HairLong_F_HM"),
                    A("2c853a2e9e482ae4fb3a7719235d52ba",
                        "EE_Hair_HairSlick_F_HM"),
                    A("981dc6e470055a34a92129cb70563b11",
                        "EE_Hair_BobCut_F_HM"),
                    A("994ec31442e763d4580ac72cd8ef6108",
                        "EE_Hair_HairLongWavy_F_HM"),
                    Empty
                },
                new[]
                {
                    A("206b471c60452a548b4d2843e35b813e",
                        "EE_Eyebrows_Face04_F_HM"),
                    A("0f069f128f63aad40bf4fd6516b0dd2b",
                        "EE_Eyebrows_Face05_F_HM")
                }, new ElementalRaceNativeVisualAsset[0],
                new ElementalRaceVisualProxySpec[0]);
            return D(ElementalRaceKind.Oread, OreadBody,
                OreadPresetStandard, OreadPresetHeavy, OreadPresetSlender,
                male, female, new[]
                {
                    R(ElfSkinSource, "CL_Skin_U_EL",
                        "CR_Skin_GrayDead_U_EL"),
                    R(HumanSkinSource, "CL_Skin_U_HM",
                        "CR_Skin_Tan_U_HM"),
                    R(HumanSkinSource, "CL_Skin_U_HM",
                        "CR_Skin_Black2_U_HM"),
                    R(HalfOrcSkinSource, "CL_Skin_U_HO",
                        "CR_Skin_BrownLight_U_HO"),
                    R(HalfOrcSkinSource, "CL_Skin_U_HO",
                        "CR_Skin_BrownDark_U_HO"),
                    R(AasimarSkinSource, "CL_Skin_U_AS",
                        "CR_Skin_Black_U_AS"),
                    R(HumanSkinSource, "CL_Skin_U_HM",
                        "CR_Skin_Pale_U_HM")
                });
        }

        private static ElementalRaceVisualDefinition BuildSylph()
        {
            var male = new ElementalRaceSexVisualDefinition(
                P("KMG.ElementalRaces.Sylph.Visual.Body.Male",
                    AasimarMaleBody, AasimarMaleBody, true),
                new[]
                {
                    P("KMG.ElementalRaces.Sylph.Visual.Head.Male.01",
                        A("944a6c1d75a489b43bb9535fcb164d3c",
                            "EE_Head_Face01_M_AS"),
                        A("944a6c1d75a489b43bb9535fcb164d3c",
                            "EE_Head_Face01_M_AS"), true),
                    P("KMG.ElementalRaces.Sylph.Visual.Head.Male.02",
                        A("a168fdf6428df1c44a08b6bb75f8a0d4",
                            "EE_Head_Face02_M_AS"),
                        A("a168fdf6428df1c44a08b6bb75f8a0d4",
                            "EE_Head_Face02_M_AS"), true)
                },
                new[]
                {
                    A("658e202937ebb8f4a8a6a15f6d1b4147",
                        "EE_Hair_HairMediumEmo_M_AS"),
                    A("24e1c92c114261f4f9bb11a2ebddaa9e",
                        "EE_Hair_HairMediumMess_M_AS"),
                    A("24c5b0a90952b8843a93ec33feacb78b",
                        "EE_Hair_HairLongBangs_M_AS"),
                    A("e6225e3842f51904281cec8e4b95cc6b",
                        "EE_Hair_HairShort_M_AS"),
                    Empty
                },
                new[]
                {
                    A("5056fc2f26affae4fa73e736fb6baa35",
                        "EE_Eyebrows_Face01_M_AS"),
                    A("545b21a250d6dac4f8155279aee982b8",
                        "EE_Eyebrows_Face02_M_AS")
                },
                new[]
                {
                    Empty,
                    A("0f170f6f3861e924fba39709d91b1834",
                        "EE_Facial_BeardSimple_M_AS"),
                    A("bc0019c6f8486054a83595e11aa84954",
                        "EE_Facial_BeardMedium_M_AS"),
                    A("2c344cd434ab92d4ca3c96b5c0d03f88",
                        "EE_Facial_BeardWiseman_M_AS")
                }, new ElementalRaceVisualProxySpec[0]);
            var female = new ElementalRaceSexVisualDefinition(
                P("KMG.ElementalRaces.Sylph.Visual.Body.Female",
                    AasimarFemaleBody, AasimarFemaleBody, true),
                new[]
                {
                    P("KMG.ElementalRaces.Sylph.Visual.Head.Female.01",
                        A("8ec7909c9d82d2a4f8ab6e2b0c63e1ef",
                            "EE_Head_Face01_F_AS"),
                        A("8ec7909c9d82d2a4f8ab6e2b0c63e1ef",
                            "EE_Head_Face01_F_AS"), true),
                    P("KMG.ElementalRaces.Sylph.Visual.Head.Female.02",
                        A("c1f41e469923ecf43a6c8050318c5e13",
                            "EE_Head_Face02_F_AS"),
                        A("c1f41e469923ecf43a6c8050318c5e13",
                            "EE_Head_Face02_F_AS"), true)
                },
                new[]
                {
                    A("1ae6efef4fa457d41b34bfd505d89b27",
                        "EE_Hair_HairLongBack_F_AS"),
                    A("0ea0ea3831a81dc47973d1f6e1444aab",
                        "EE_Hair_BobCut_F_AS"),
                    A("8a44085f8413da7438f178ed55de56cb",
                        "EE_Hair_AsymmetricPonyTail_F_AS"),
                    A("b0905618d53d1be4c9f8b54f7c8ac0d4",
                        "EE_Hair_HairMediumCombedBack_F_AS"),
                    Empty
                },
                new[]
                {
                    A("f8c7dd9c7968c9541a46fd7f8bed1235",
                        "EE_Eyebrows_Face01_F_AS"),
                    A("dc2bce78975629743bae94f20d143534",
                        "EE_Eyebrows_Face02_F_AS")
                }, new ElementalRaceNativeVisualAsset[0],
                new ElementalRaceVisualProxySpec[0]);
            return D(ElementalRaceKind.Sylph, SylphBody,
                SylphPresetStandard, SylphPresetHeavy, SylphPresetSlender,
                male, female, new[]
                {
                    R(ElfSkinSource, "CL_Skin_U_EL", "CR_Skin_White_U_EL"),
                    R(ElfSkinSource, "CL_Skin_U_EL", "CR_Skin_Pale_U_EL"),
                    R(ElfSkinSource, "CL_Skin_U_EL", "CR_Skin_BlueLight_U_EL"),
                    R(ElfSkinSource, "CL_Skin_U_EL", "CR_Skin_GreenLight_U_EL"),
                    R(ElfSkinSource, "CL_Skin_U_EL", "CR_Skin_GrayDead_U_EL"),
                    R(AasimarSkinSource, "CL_Skin_U_AS", "CR_Skin_White_U_AS"),
                    R(AasimarSkinSource, "CL_Skin_U_AS", "CR_Skin_Pale_U_AS")
                });
        }

        private static ElementalRaceVisualDefinition BuildUndine()
        {
            var male = new ElementalRaceSexVisualDefinition(
                P("KMG.ElementalRaces.Undine.Visual.Body.Male",
                    AasimarMaleBody, AasimarMaleBody, true),
                new[]
                {
                    P("KMG.ElementalRaces.Undine.Visual.Head.Male.01",
                        A("3dd7d2374aca7bd40bb6d9359f3fe061",
                            "EE_Head_Face03_M_AS"),
                        A("3dd7d2374aca7bd40bb6d9359f3fe061",
                            "EE_Head_Face03_M_AS"), true),
                    P("KMG.ElementalRaces.Undine.Visual.Head.Male.02",
                        A("735afffaddc84524db32796b68db203b",
                            "EE_Head_Face04_M_AS"),
                        A("735afffaddc84524db32796b68db203b",
                            "EE_Head_Face04_M_AS"), true)
                },
                new[]
                {
                    A("658e202937ebb8f4a8a6a15f6d1b4147",
                        "EE_Hair_HairMediumEmo_M_AS"),
                    A("24e1c92c114261f4f9bb11a2ebddaa9e",
                        "EE_Hair_HairMediumMess_M_AS"),
                    A("e6225e3842f51904281cec8e4b95cc6b",
                        "EE_Hair_HairShort_M_AS"),
                    A("afe7591ca066f1d46b01268bd2f88f4b",
                        "EE_Hair_HairMediumBun_M_AS"),
                    Empty
                },
                new[]
                {
                    A("445561075786c3d48b33321642019728",
                        "EE_Eyebrows_Face03_M_AS"),
                    A("0cc00185478edd849b2cdea1f4d1c616",
                        "EE_Eyebrows_Face04_M_AS")
                },
                new[]
                {
                    Empty,
                    A("0f170f6f3861e924fba39709d91b1834",
                        "EE_Facial_BeardSimple_M_AS"),
                    A("bc0019c6f8486054a83595e11aa84954",
                        "EE_Facial_BeardMedium_M_AS"),
                    A("2c344cd434ab92d4ca3c96b5c0d03f88",
                        "EE_Facial_BeardWiseman_M_AS")
                }, new ElementalRaceVisualProxySpec[0]);
            var female = new ElementalRaceSexVisualDefinition(
                P("KMG.ElementalRaces.Undine.Visual.Body.Female",
                    AasimarFemaleBody, AasimarFemaleBody, true),
                new[]
                {
                    P("KMG.ElementalRaces.Undine.Visual.Head.Female.01",
                        A("e99fa443817bee540a443bb9c574f8ca",
                            "EE_Head_Face03_F_AS"),
                        A("e99fa443817bee540a443bb9c574f8ca",
                            "EE_Head_Face03_F_AS"), true),
                    P("KMG.ElementalRaces.Undine.Visual.Head.Female.02",
                        A("f14681e91b52dfd49b700920154e7ed3",
                            "EE_Head_Face04_F_AS"),
                        A("f14681e91b52dfd49b700920154e7ed3",
                            "EE_Head_Face04_F_AS"), true)
                },
                new[]
                {
                    A("1ae6efef4fa457d41b34bfd505d89b27",
                        "EE_Hair_HairLongBack_F_AS"),
                    A("0ea0ea3831a81dc47973d1f6e1444aab",
                        "EE_Hair_BobCut_F_AS"),
                    A("8a44085f8413da7438f178ed55de56cb",
                        "EE_Hair_AsymmetricPonyTail_F_AS"),
                    A("f9a0858ed3d3aa94fbf7dc695f92f774",
                        "EE_Hair_HairSideKare_F_AS"),
                    Empty
                },
                new[]
                {
                    A("1d68aa94a9485e046a4f4c785089a56d",
                        "EE_Eyebrows_Face03_F_AS"),
                    A("03cca2eaebd59d7429aa39c689744d1a",
                        "EE_Eyebrows_Face04_F_AS")
                }, new ElementalRaceNativeVisualAsset[0],
                new ElementalRaceVisualProxySpec[0]);
            return D(ElementalRaceKind.Undine, UndineBody,
                UndinePresetStandard, UndinePresetHeavy,
                UndinePresetSlender, male, female, new[]
                {
                    R(TieflingSkinSource, "CL_Skin_U_TF", "CR_Skin_Blue_U_TF"),
                    R(TieflingSkinSource, "CL_Skin_U_TF", "CR_Skin_DarkBlue_U_TF"),
                    R(HalfOrcSkinSource, "CL_Skin_U_HO",
                        "CR_Skin_AquamarineLight_U_HO"),
                    R(HalfOrcSkinSource, "CL_Skin_U_HO",
                        "CR_Skin_Aquamarine_U_HO"),
                    R(ElfSkinSource, "CL_Skin_U_EL", "CR_Skin_BlueLight_U_EL"),
                    R(ElfSkinSource, "CL_Skin_U_EL", "CR_Skin_GreenLight_U_EL"),
                    R(HumanSkinSource, "CL_Skin_U_HM", "CR_Skin_Pale_U_HM")
                });
        }

        private static ElementalRaceVisualDefinition D(ElementalRaceKind kind,
            string body, string standard, string heavy, string slender,
            ElementalRaceSexVisualDefinition male,
            ElementalRaceSexVisualDefinition female,
            ElementalRaceRampReference[] skin)
        {
            return new ElementalRaceVisualDefinition(kind, body,
                new[] { standard, heavy, slender }, skin, male, female);
        }

        private static ElementalRaceVisualProxySpec P(string symbol,
            ElementalRaceNativeVisualAsset donor,
            ElementalRaceNativeVisualAsset fallback, bool skin)
        {
            return new ElementalRaceVisualProxySpec(symbol, donor, fallback,
                skin);
        }

        private static ElementalRaceNativeVisualAsset A(string id, string name)
        {
            return new ElementalRaceNativeVisualAsset(id, name);
        }

        private static ElementalRaceRampReference R(
            ElementalRaceNativeVisualAsset source, string profile, string ramp)
        {
            return new ElementalRaceRampReference(source, profile, ramp);
        }
    }
}
