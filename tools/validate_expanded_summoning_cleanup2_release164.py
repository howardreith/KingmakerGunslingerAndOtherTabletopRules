"""v0.0.164 Summon Cleanup Iteration 2 release on released v0.0.163.

Relative to released v0.0.163 master (after its release record, PR #65) the
release changes no identity: the released ledger is unchanged. It changes only
the pinned cleanup production files and the version metadata; adds only the
pinned compile items, references and the Gorgon material-parameter content to
the project; modifies only the pinned assets and source art, in place, to
exactly their reviewed bytes; adds only the pinned assets and source art; and
adds only its own record to the static evidence. The released Sprint 29 public
roster and choice census still hold. The historical sprint and release
validators are left as they were.
"""
from __future__ import annotations

import argparse
import collections
import hashlib
import json
import subprocess
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_development_candidate as identities
import validate_expanded_summoning_sprint21150 as common
from expanded_summoning_manifest import NATIVE_EXPANDED_OPTIONS, parsed_creatures, suppressed_keys

# Released v0.0.163 master, after its release record (PR #65).
RELEASED = "a35a3277a9c5e2c111475523b868c2cf08422e05"
VERSION = "0.0.164"
INFORMATIONAL = "0.0.164-expanded-summoning-cleanup2"
RECORD_KEY = "expandedSummoningCleanup2Release164"

CLEANUP_PRODUCTION = {
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningAbilityBuilder.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningNativeOptionBuilder.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningPublication.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.Giants.cs",
    "src/KingmakerGunslinger/RuntimeTesting/Cleanup2CharacterCpuReference.cs",
    "src/KingmakerGunslinger/RuntimeTesting/Cleanup2GiantRenderScalePrototype.cs",
    "src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningInventoryObservationPolicy.cs",
    "src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningNativeProfilePolicy.cs",
    "src/KingmakerGunslinger/RuntimeTesting/NativeIconScreenEvidence.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningBoundary.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningCorrection.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningCreatureReview.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningNativeProfile.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningResourceReceipts.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningRosterPersistence.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint22Review.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint25Review.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint26Review.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint29Commands.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2AlternateMenus.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2Bodies.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2ClothProbe.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2ConsoleLearning.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2ConsoleScene.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2CrabAttack.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2CreatureCommands.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2DesktopLearning.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2ElementalConsole.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2ElementalPresentation.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2ElementalReview.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2ElementalSources.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2EttinLifecycle.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2GiantsOff.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2MenuAdapters.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2ModuleOff.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2PaletteReview.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2ProductionAttacks.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2Review.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2ScalePrototypeReview.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2SharedControls.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2TooltipReview.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs",
    "src/KingmakerGunslinger/RuntimeTesting/VisualRefinementReviewPolicy.cs",
    "src/KingmakerGunslinger/Summoning/Cleanup2CreatureReviewRoster.cs",
    "src/KingmakerGunslinger/Summoning/CrabSummonAnimationBindingPatch.cs",
    "src/KingmakerGunslinger/Summoning/CrabSummonAnimationBindingPolicy.cs",
    "src/KingmakerGunslinger/Summoning/DruidSummonConversionPolicy.cs",
    "src/KingmakerGunslinger/Summoning/DruidSummonConversionVariantPatch.cs",
    "src/KingmakerGunslinger/Summoning/ElementalConsoleBookPresentationPatch.cs",
    "src/KingmakerGunslinger/Summoning/ElementalNativeRowsPresentationPatch.cs",
    "src/KingmakerGunslinger/Summoning/ElementalSourceChooserConsolePatch.cs",
    "src/KingmakerGunslinger/Summoning/ElementalSourceChooserPatch.cs",
    "src/KingmakerGunslinger/Summoning/ElementalSourceChooserPolicy.cs",
    "src/KingmakerGunslinger/Summoning/ElementalSourceChooserRuntime.cs",
    "src/KingmakerGunslinger/Summoning/ElementalSourceChooserView.cs",
    "src/KingmakerGunslinger/Summoning/EttinAuthoredHeadMesh.cs",
    "src/KingmakerGunslinger/Summoning/EttinBodyMesh.cs",
    "src/KingmakerGunslinger/Summoning/EttinSecondHeadVisual.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningInventoryObserver.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningPteranodonViewPatch.cs",
    "src/KingmakerGunslinger/Summoning/GiantSummonPalettePolicy.cs",
    "src/KingmakerGunslinger/Summoning/GiantSummonPaletteRuntime.cs",
    "src/KingmakerGunslinger/Summoning/GiantSummonPresentationPatch.cs",
    "src/KingmakerGunslinger/Summoning/GiantSummonPresentationPolicy.cs",
    "src/KingmakerGunslinger/Summoning/StandaloneElementalDiscoveryPatch.cs",
    "src/KingmakerGunslinger/Summoning/StandaloneElementalInvocationContract.cs",
    "src/KingmakerGunslinger/Summoning/StandaloneElementalPresentationPolicy.cs",
    "src/KingmakerGunslinger/Summoning/SummonAttackGesturePolicy.cs",
    "src/KingmakerGunslinger/Summoning/SummonAttackGestureVisual.cs",
    "src/KingmakerGunslinger/Summoning/SummonChoicePresentationPolicy.cs",
    "src/KingmakerGunslinger/Summoning/SummonNativeOptionCatalog.cs",
    "src/KingmakerGunslinger/Summoning/SummonVariantMergePolicy.cs",
}
VERSION_PRODUCTION = {"src/KingmakerGunslinger/Properties/AssemblyInfo.cs"}
PROJECT = "src/KingmakerGunslinger/KingmakerGunslinger.csproj"
PROJECT_LINES = [
    "    <Compile Include=\"Summoning\\EttinBodyMesh.cs\" />",
    "    <Compile Include=\"Summoning\\Cleanup2CreatureReviewRoster.cs\" />",
    "    <Compile Include=\"Summoning\\SummonChoicePresentationPolicy.cs\" />",
    "    <Compile Include=\"Summoning\\GiantSummonPresentationPolicy.cs\" />",
    "    <Compile Include=\"Summoning\\GiantSummonPresentationPatch.cs\" />",
    "    <Compile Include=\"Summoning\\GiantSummonPalettePolicy.cs\" />",
    "    <Compile Include=\"Summoning\\GiantSummonPaletteRuntime.cs\" />",
    "    <Compile Include=\"Summoning\\CrabSummonAnimationBindingPolicy.cs\" />",
    "    <Compile Include=\"Summoning\\SummonAttackGesturePolicy.cs\" />",
    "    <Compile Include=\"Summoning\\SummonAttackGestureVisual.cs\" />",
    "    <Compile Include=\"Summoning\\CrabSummonAnimationBindingPatch.cs\" />",
    "    <Compile Include=\"Summoning\\StandaloneElementalPresentationPolicy.cs\" />",
    "    <Compile Include=\"Summoning\\ElementalSourceChooserPolicy.cs\" />",
    "    <Compile Include=\"Summoning\\ElementalSourceChooserRuntime.cs\" />",
    "    <Compile Include=\"Summoning\\ElementalSourceChooserView.cs\" />",
    "    <Compile Include=\"Summoning\\ElementalSourceChooserPatch.cs\" />",
    "    <Compile Include=\"Summoning\\ElementalNativeRowsPresentationPatch.cs\" />",
    "    <Compile Include=\"Summoning\\ElementalSourceChooserConsolePatch.cs\" />",
    "    <Compile Include=\"Summoning\\ElementalConsoleBookPresentationPatch.cs\" />",
    "    <Compile Include=\"Summoning\\DruidSummonConversionVariantPatch.cs\" />",
    "    <Compile Include=\"Summoning\\DruidSummonConversionPolicy.cs\" />",
    "    <Compile Include=\"Summoning\\StandaloneElementalDiscoveryPatch.cs\" />",
    "    <Compile Include=\"Summoning\\StandaloneElementalInvocationContract.cs\" />",
    "    <Reference Include=\"UniRx\">",
    "      <HintPath>$(KingmakerManagedDir)\\UniRx.dll</HintPath>",
    "      <Private>False</Private>",
    "    </Reference>",
    "    <Reference Include=\"UnityEngine.ClothModule\">",
    "      <HintPath>$(KingmakerManagedDir)\\UnityEngine.ClothModule.dll</HintPath>",
    "      <Private>False</Private>",
    "    </Reference>",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2Review.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2Bodies.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2ElementalReview.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2ElementalPresentation.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2ConsoleScene.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2ElementalConsole.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2ConsoleLearning.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2DesktopLearning.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2ElementalSources.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2MenuAdapters.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2AlternateMenus.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2CreatureCommands.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2ModuleOff.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2GiantsOff.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2ClothProbe.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2EttinLifecycle.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2ScalePrototypeReview.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2PaletteReview.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2TooltipReview.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2CrabAttack.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2ProductionAttacks.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\Cleanup2CharacterCpuReference.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\RuntimeTestRunner.SummonCleanup2SharedControls.cs\" />",
    "    <Compile Include=\"RuntimeTesting\\Cleanup2GiantRenderScalePrototype.cs\" />",
    "    <Content Include=\"..\\..\\assets\\ungulates\\gorgon-parameters.png\">",
    "      <Link>assets\\ungulates\\gorgon-parameters.png</Link>",
    "      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>",
    "    </Content>",
    "    <RequiredKingmakerAssembly Include=\"$(KingmakerManagedDir)\\UniRx.dll\" />",
    "    <RequiredKingmakerAssembly Include=\"$(KingmakerManagedDir)\\UnityEngine.ClothModule.dll\" />",
    "      <ForbiddenOutputAssembly Include=\"$(TargetDir)UniRx.dll\" />",
]

# Released assets and source art this release modifies in place, with their
# reviewed bytes: the revised Crab, Girallon, Stag Beetle, Stegosaurus,
# Elephant, Brachiosaurus, Griffon and Gorgon bodies, the painted Horse icon,
# the icon manifests and catalog, and the authoring sources behind them.
ART_MODIFIED = {
    "assets-source/original-icons/expanded-summoning/icon-manifest.json":
        "5d310b34c1dc2278f997f257d4d3f556b3ae352ffab14de79777c0b8fdac9271",
    "assets-source/original-icons/icon-catalog.json":
        "c7bb6c0141d1983f0762ed8f230167b284a6b94e3c2ebd1739541e5e7a63a947",
    "assets-source/original-models/sprint14-insects/SOURCE.md":
        "17fac66fcc8f50b1bc0ba74ed2d798938a05591fcc2f1524836b72d0e2c25303",
    "assets-source/original-models/sprint14-insects/generate_sprint14_insects.py":
        "774e237bf8bcd272e4bb9a001d668b9f5ed7ca35b509d4a9f2ada44de598f10a",
    "assets-source/original-models/sprint19-fourarmed/SOURCE.md":
        "7388caa95844761b07689a6228a85810955c28ea37c9e81a9045691f8ad18a93",
    "assets-source/original-models/sprint19-fourarmed/generate_fourarmed.py":
        "685bfbc23f94c24d1f387779f0c105f6da5bdf4cb3e8fa8364d46ede466c2ab5",
    "assets-source/original-models/sprint19-fourarmed/paint_fourarmed_albedo.py":
        "aff55c1c763bd62b20911c0d3e539d79dc2fe2304b3483c0076242ed290a82bc",
    "assets-source/original-models/sprint21-arthropods/SOURCE.md":
        "11d483e546c61d253c6cb8e98504ee9452ca05c7b27df94ef5ee31606509ee9a",
    "assets-source/original-models/sprint21-arthropods/generate_giant_crab.py":
        "b81974271b913262792ca5fe0ef29e1ffb44a40c649f9301c29ae27d1e441223",
    "assets-source/original-models/sprint23-armored-dinosaurs/SOURCE.md":
        "849e8bbacb1dc0fe6925025695cee6d6aa94ac93bf74f2d3cd028353e403bcc8",
    "assets-source/original-models/sprint23-armored-dinosaurs/generate_armored_dinosaurs.py":
        "1f53393018496b4fa7ee96f396bcef1e8875a74da672024dc2543b0c55a7d361",
    "assets-source/original-models/sprint25-giant-bodies/SOURCE.md":
        "f07581559f1a312bf0686cda1bfac7442a9aeb4744ffe8fcf008da4cc7812d5e",
    "assets-source/original-models/sprint26-gorgon/SOURCE.md":
        "516bf4a4a8adaaaa594a4ed6796ca683b3508861a59a39cc62b92c5b239a11fc",
    "assets-source/original-models/sprint26-gorgon/build_gorgon.py":
        "32d115765cffd54458143a742d458f22febe6c54a24fbbae81777359e6e05361",
    "assets-source/original-models/sprint27-griffon/SOURCE.md":
        "80a1d91729002df5d408d0f9d35129016e93998c6091d7d88efb32bd2173f550",
    "assets/game/icons/expanded-summoning/horse.png":
        "3bf6ac8d8c7b08869bf262c4018937d5aa6b72cf3e0b809efec916c6e0cf67c1",
    "assets/game/icons/expanded-summoning/icon-manifest.json":
        "02566fcdc203d6fa094c901ca25a4996c2c9ae06e908a405b7868c7e31180189",
    "assets/sprint14-insects/giant-stag-beetle-albedo.png":
        "967d506b96019914fd40363cc80b21b0ce08f80662ef96ef259f5e0a8caecde4",
    "assets/sprint14-insects/giant-stag-beetle-mesh.json":
        "2ecf222413eb857051b70df8b5fd61592a939108c7661ccdfb96f20fbcea794d",
    "assets/sprint19-fourarmed/girallon-albedo.png":
        "3070d2d51d578060fd74bcc7b85ef074491699b0701768707ebc655fa06cc841",
    "assets/sprint19-fourarmed/girallon-mesh.json":
        "d6a160c6b336cc0dc812f62ce03c90c5faa5baf611fcb46c7e81130ad71ca119",
    "assets/sprint21-arthropods/giant-crab-albedo.png":
        "ef06b21b0654bcee40ca893cfff28c20e5fea469402cadb1d7a3a524f8e48112",
    "assets/sprint21-arthropods/giant-crab-mesh.json":
        "75a6d5d625f8e88d9cfccec0de3d411610123be0f94f4655d5fc9eeadaf05472",
    "assets/sprint23-armored-dinosaurs/stegosaurus-albedo.png":
        "ab6d343e35377b0cb9d73a8b5a2ab19e70b4cfe1c6a0cb21f109673fa883d6d6",
    "assets/sprint23-armored-dinosaurs/stegosaurus-mesh.json":
        "3f9e1a219de1372bfec4db521a6de02e04771ea8e2bfc53d64a8435f440d466e",
    "assets/sprint25-giant-bodies/brachiosaurus-albedo.png":
        "160e4b71ae1335d6df37a0d8eac491e863b82dc272cfe24e42173a6d40107ea4",
    "assets/sprint25-giant-bodies/brachiosaurus-mesh.json":
        "73f3689c5abdda09fb68f9e08182cf671c6a753966bedaf21ebb16877c04c57c",
    "assets/sprint25-giant-bodies/elephant-albedo.png":
        "5a973962a26dbf38208b66feeb5ffd154d469ac69b25137ae487b9d6767f2919",
    "assets/sprint25-giant-bodies/elephant-mesh.json":
        "5afe5464331a083c0de63d7b4bee4a1266d71376fb8360f7d96b0bf7a4f13028",
    "assets/sprint27-griffon/griffon-albedo.png":
        "1ccd2ade87b48b1162910270395c32c294c7e84c20c3f961b8e720b40eddd124",
    "assets/sprint27-griffon/griffon-mesh.json":
        "e3db5d71b8f4c2bf4647bbcfdcea1b2438f4a060ad3d6c1ddc3ec6649af3eab9",
    "assets/ungulates/gorgon-albedo.png":
        "8974f854b188eb5f40589b214a6babca9df7fb002c2038f56109a46199075ffe",
    "assets/ungulates/gorgon-mesh.json":
        "2d524e002371fe10ca3f89ac2b59a32de823711cc3d05fc2e174104bb6f53370",
}
# Files this release adds: the Gorgon material-parameter map and the
# authoring inputs, scripts and notes for the revised bodies and the Horse.
ART_ADDED = {
    "assets-source/original-icons/expanded-summoning/edits/horse/EDIT.md":
        "15746d0ae555161994ee44c18619f5b9db20a9a4469c7c9ac36c497840bc7b6f",
    "assets-source/original-icons/expanded-summoning/edits/horse/horse.png":
        "d70b28852dd0fc1080f1ae687b5db269673da4cc292274b3413b319ee9f8ce1f",
    "assets-source/original-models/sprint14-insects/build_giant_stag_beetle.py":
        "8ad9d88f93c1d5859e7c377969d03352cf44e46e9a86888aa56fdb94d52f0adb",
    "assets-source/original-models/sprint19-fourarmed/girallon-hand-stress-poses.json":
        "b6df6ba41d7e4fe48b704dbb8821f43523e54a6ad5f72a720185189a4c5f19a9",
    "assets-source/original-models/sprint19-fourarmed/girallon_connected_hands.py":
        "93c2b06c2c781c7c84083fca359ed69669eee156ba4c3783bc70812931f4486a",
    "assets-source/original-models/sprint21-arthropods/build_giant_crab.py":
        "a7066c102f7a3a3d2bb733aede89a290778a49ddac42242bfe6ca1878c45791c",
    "assets-source/original-models/sprint21-arthropods/cleanup2-arthropod-stress-poses.json":
        "2a5c386bc42d5fba12d3d0001166d0eb4a02fca36de40a0e64e27c9146455748",
    "assets-source/original-models/sprint21-arthropods/cleanup2_authored_parts.py":
        "7db53d3c2fb48daa938e9260f81da0df8860858a061e1ab1fa9a1bf7ea3eead2",
    "assets-source/original-models/sprint21-arthropods/verify_cleanup2_bodies.py":
        "bf593c5414bfddea685f49fcfbed5b1b65d8c060c7b146464a7de06d805dea6e",
    "assets-source/original-models/sprint23-armored-dinosaurs/build_stegosaurus.py":
        "59e8042545a2d6cae49c4506fbf6ba483c44209c229f3736ef1f2eef1de1ebbb",
    "assets-source/original-models/sprint23-armored-dinosaurs/stegosaurus-stress-poses.json":
        "6c53ae81798a1399d97ea1759f70790dc1edefd143e7afae8f6d46bee2ee4af3",
    "assets-source/original-models/sprint25-giant-bodies/cleanup2-brachiosaurus-input.json":
        "ce826df01509d0333e50491077a376a8e985f4bf8d735c7c2814aa16d434e18a",
    "assets-source/original-models/sprint25-giant-bodies/cleanup2-elephant-input.json":
        "ed1e0dffc817a7ebda0453c4125b07386e1b03d309ebfc84d1e2b8c027ed8087",
    "assets-source/original-models/sprint25-giant-bodies/refine_giant_bodies.py":
        "f0417cb89e8f2ec9e363b7cc48cabb6725dfff00ab65ae1cd6379820c326eff9",
    "assets-source/original-models/sprint25-giant-bodies/repair_brachiosaurus_tail_weights.py":
        "4a63bd896e6a5804e5bb4845834d08496a3b4e18f538a66fe2db1eff7b33cbd4",
    "assets-source/original-models/sprint26-gorgon/cleanup2-gorgon-input.json":
        "0f3c8ce10c4c0d50650ecb5a042985d1b242977716c5b7a0286754d370fdc508",
    "assets-source/original-models/sprint27-griffon/cleanup2-griffon-input.json":
        "4694292d3cc275590fddade1e68169968884185573cc172455daf8b875e048ac",
    "assets-source/original-models/sprint27-griffon/refine_griffon.py":
        "557bb99f179cb310f2a927ed790044b7e41e50168acfa758ace276e2040bc1b4",
    "assets-source/original-models/sprint29-ettin/SOURCE.md":
        "3706562cdffd6aaed4e07aee3cb643d4d6d1433da22edd3f073067c263654714",
    "assets-source/original-models/sprint29-ettin/authoring-poses.json":
        "fc33698734872280471e36cb4bc2499d390186d7e70ed8e6dde9c216dbc7fddd",
    "assets-source/original-models/sprint29-ettin/build_ettin_head.py":
        "00fea8eaac5c678c83c0821ac2a0aed4c4f1fc798f2c27a61d936cc9321fdaff",
    "assets/ungulates/gorgon-parameters.png":
        "241c44e0ef86feac0acde88abd835c29ee138791e2b31af4d3b92133ea7d1351",
}
ART = dict(ART_MODIFIED, **ART_ADDED)

RECORD = {
    "releaseVersion": VERSION,
    "releaseInformationalVersion": INFORMATIONAL,
    "baseStableVersion": "0.0.163",
    "change": "Summon Cleanup Iteration 2: revised Crab, Girallon, Stag Beetle, "
              "Stegosaurus, Elephant, Brachiosaurus, Griffon, Gorgon and two-headed "
              "Ettin bodies, owned giant palettes and relative render scale, the "
              "painted Horse icon, family-first summon labels, the Druid Call of the "
              "Wild conversion projection, alternate-source menus and the "
              "source-preserving elemental chooser, learning and preparation",
    "appendedIdentities": 0,
    "releasedIdentityChanges": 0,
    "modifiedAssets": len(ART_MODIFIED),
    "addedAssets": len(ART_ADDED),
    "productionFiles": len(CLEANUP_PRODUCTION),
    "addedProjectLines": len(PROJECT_LINES),
    "visibleChoiceTotal": 1156,
    "guardedNativeReviewed": True,
    "ownerVisualReview": "artwork approved by the owner on 2026-10-23",
}


def check_identities(released_entries, current_entries):
    """The released ledger, exactly: this release appends nothing."""
    appended = identities.check_append_only(released_entries, current_entries)
    if appended != 0:
        raise AssertionError("v0.0.164 appends no identity, observed " + str(appended))


def check_project(released_text, current_text):
    """The released project, in order, plus exactly the pinned added lines.
    Some added lines (reference metadata) repeat existing ones, so compare
    the extra lines as a multiset; it does not depend on how the released
    lines are matched."""
    released = released_text.splitlines()
    added, matched = [], 0
    for line in current_text.splitlines():
        if matched < len(released) and line == released[matched]:
            matched += 1
        else:
            added.append(line)
    if matched != len(released):
        raise AssertionError("The project removed or reordered released lines")
    if collections.Counter(added) != collections.Counter(PROJECT_LINES):
        raise AssertionError("The project changed beyond the release's pinned lines")


def check_production_scope(changed):
    unexpected = sorted(set(changed) - CLEANUP_PRODUCTION - VERSION_PRODUCTION - {PROJECT})
    if unexpected:
        raise AssertionError("Production source outside the v0.0.164 release changed: " +
                             ", ".join(unexpected))


def check_art(changed, digest):
    """changed maps each asset/source-art path that differs from the release to
    its git status letter; digest returns a path's SHA-256."""
    unexpected = sorted(path for path in changed if path not in ART)
    if unexpected:
        raise AssertionError("Asset outside the reviewed cleanup changed: " +
                             ", ".join(unexpected[:5]))
    wrong = sorted(path for path, status in changed.items()
                   if status != ("M" if path in ART_MODIFIED else "A"))
    if wrong:
        raise AssertionError("Assets may only be modified in place or added as pinned: " +
                             ", ".join(wrong[:5]))
    missing = sorted(path for path in ART if path not in changed)
    if missing:
        raise AssertionError("Reviewed cleanup asset is absent or still the released file: " +
                             ", ".join(missing[:5]))
    for path, expected in sorted(ART.items()):
        if digest(path) != expected:
            raise AssertionError("Cleanup asset differs from its reviewed bytes: " + path)


def check_census(creatures, hidden):
    """The released Sprint 29 public roster and choice census, unchanged."""
    if hidden:
        raise AssertionError("The release withholds a released creature")
    monster = sum(10 - c["monster"] for c in creatures if c["monster"] is not None)
    ally = sum(10 - c["ally"] for c in creatures if c["ally"] is not None)
    if len(creatures) != 117 or sum(c["monster"] is not None for c in creatures) != 96 or \
            monster != 545 or sum(c["ally"] is not None for c in creatures) != 104 or \
            ally != 582 or monster + ally + len(NATIVE_EXPANDED_OPTIONS) != 1156:
        raise AssertionError("Released Sprint 29 public census differs")


def check_static(released, current):
    for key in set(released) | set(current):
        if key in ("version", "milestone", RECORD_KEY):
            continue
        if released.get(key) != current.get(key):
            raise AssertionError("Released static evidence changed: " + key)
    if current.get("version") != VERSION or current.get("milestone") != INFORMATIONAL:
        raise AssertionError("Static record version metadata mismatch")
    if current.get(RECORD_KEY) != RECORD:
        raise AssertionError("v0.0.164 release static record mismatch")


def check_version(info_version, props, assembly):
    if info_version != VERSION:
        raise AssertionError("Unsupported v0.0.164 release version: " + repr(info_version))
    if "<KmgVersion>" + VERSION + "</KmgVersion>" not in props or \
            "<KmgInformationalVersion>" + INFORMATIONAL + \
            "</KmgInformationalVersion>" not in props or \
            'AssemblyVersion("' + VERSION + '")' not in assembly or \
            'AssemblyFileVersion("' + VERSION + '")' not in assembly or \
            'AssemblyInformationalVersion("' + INFORMATIONAL + '")' not in assembly:
        raise AssertionError("Active v0.0.164 release version metadata mismatch")


def changed_production(root: Path):
    changed = subprocess.check_output(
        ["git", "diff", "--name-only", RELEASED, "--", "src/KingmakerGunslinger"],
        cwd=root, text=True).splitlines()
    changed += subprocess.check_output(
        ["git", "ls-files", "--others", "--exclude-standard", "--",
         "src/KingmakerGunslinger"], cwd=root, text=True).splitlines()
    return changed


def changed_assets(root: Path):
    changed = {}
    for line in subprocess.check_output(
            ["git", "diff", "--name-status", "--no-renames", RELEASED, "--",
             "assets", "assets-source"], cwd=root, text=True).splitlines():
        fields = line.split("\t")
        changed[fields[-1]] = fields[0][:1]
    for path in subprocess.check_output(
            ["git", "ls-files", "--others", "--exclude-standard", "--",
             "assets", "assets-source"], cwd=root, text=True).splitlines():
        changed[path] = "A"
    return changed


def validate(root: Path):
    released_ledger = json.loads(common.blob(root, RELEASED, "blueprints/blueprints.json"))
    current_ledger = common.document(root, "blueprints/blueprints.json")
    for key in ("$schema", "schemaVersion", "namespace", "policy"):
        if current_ledger.get(key) != released_ledger.get(key):
            raise AssertionError("The identity ledger header changed: " + key)
    current = current_ledger["entries"]
    check_identities(released_ledger["entries"], current)
    check_census(parsed_creatures(), suppressed_keys())
    check_production_scope(changed_production(root))
    check_project(common.blob(root, RELEASED, PROJECT).decode("utf-8-sig"),
                  (root / PROJECT).read_text(encoding="utf-8-sig"))
    check_art(changed_assets(root),
              lambda path: hashlib.sha256((root / path).read_bytes()).hexdigest())
    check_static(json.loads(common.blob(root, RELEASED, "validation/static-validation.json")),
                 common.document(root, "validation/static-validation.json"))
    check_version(common.document(root, "Info.json").get("Version"),
                  (root / "Directory.Build.props").read_text(encoding="utf-8-sig"),
                  (root / "src/KingmakerGunslinger/Properties/AssemblyInfo.cs")
                  .read_text(encoding="utf-8-sig"))

    manifest = subprocess.run(
        [sys.executable, "-B", str(root / "tools/expanded_summoning_manifest.py")],
        cwd=root, text=True, capture_output=True, check=False)
    if manifest.returncode:
        raise AssertionError("Expanded Summoning manifest validation failed: " +
                             (manifest.stdout + manifest.stderr)[-1000:])
    print("Cleanup 2 release PASS: identities=" + str(len(current)) +
          "; modifiedAssets=" + str(len(ART_MODIFIED)) + "; addedAssets=" + str(len(ART_ADDED)) +
          "; version=" + VERSION + "; milestone=" + INFORMATIONAL + ".")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Cleanup 2 release FAIL: " + str(error), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
