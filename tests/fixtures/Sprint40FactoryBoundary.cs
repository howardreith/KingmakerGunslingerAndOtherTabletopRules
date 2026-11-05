using System;
using System.Collections.Generic;
using System.IO;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using KingmakerGunslinger.Blueprints;

// Only external Unity allocation, engine library storage and logging are
// substituted. Production dispatch, naming, manifest and registry run intact.
namespace UnityEngine
{
    public class ScriptableObject
    {
        public string name;
        public static int Allocations;
        public static T CreateInstance<T>() where T : ScriptableObject
        { Allocations++; return (T)Activator.CreateInstance(typeof(T)); }
    }
}
namespace Kingmaker.Blueprints
{
    public class BlueprintScriptableObject : UnityEngine.ScriptableObject
    {
        private string m_AssetGuid;
        public string AssetGuid { get { return m_AssetGuid; } }
    }
    public class LibraryScriptableObject
    {
        public readonly Dictionary<string, BlueprintScriptableObject> BlueprintsByAssetId =
            new Dictionary<string, BlueprintScriptableObject>();
        private readonly List<BlueprintScriptableObject> all = new List<BlueprintScriptableObject>();
        public List<BlueprintScriptableObject> GetAllBlueprints() { return all; }
    }
}
namespace Kingmaker.Blueprints.Items.Ecnchantments
{ public sealed class BlueprintWeaponEnchantment : BlueprintScriptableObject { } }
namespace KingmakerGunslinger.Bootstrap
{
    internal sealed class ModLogger
    {
        internal void Info(string area, string action, string message) { }
        internal void Warning(string area, string action, string message) { }
    }
}
internal static class Sprint40FactoryBoundary
{
    private static int checks;
    private static void Check(bool pass, string message)
    { if (!pass) throw new InvalidOperationException(message); checks++; }
    private static void Refuses<T>(Action action) where T : Exception
    { try { action(); } catch (T) { checks++; return; } throw new InvalidOperationException("Expected refusal: " + typeof(T).Name); }
    public static int Main(string[] args)
    {
        try
        {
            var manifest = BlueprintManifest.Load(args[0]);
            const string symbol = "KMG.Summoning.Infernal.IceDevil.SpearFrost";
            var entry = manifest.ResolveActive(symbol, typeof(BlueprintWeaponEnchantment));
            var library = new LibraryScriptableObject();
            var registry = new BlueprintRegistry(library, manifest, new KingmakerGunslinger.Bootstrap.ModLogger());
            var shell = ExpandedSummoningBlueprints.RegisterAdditionalShell(symbol, entry.PlannedType, registry);
            Check(shell.GetType() == typeof(BlueprintWeaponEnchantment), "Actual typed enchantment construction.");
            Check(shell.name == "KMG_Summoning_Infernal_IceDevil_SpearFrost", "Production shell naming.");
            Check(shell.AssetGuid == "724a4bd231534a90a60429646f47f31d", "Allocated identity retained by real registry.");
            Check(registry.RegisteredCount == 1 && ReferenceEquals(library.BlueprintsByAssetId[shell.AssetGuid], shell) &&
                library.GetAllBlueprints().Count == 1 && ReferenceEquals(library.GetAllBlueprints()[0], shell), "Exact canonical registry ownership.");
            int allocated = UnityEngine.ScriptableObject.Allocations;
            Refuses<InvalidOperationException>(() => ExpandedSummoningBlueprints.RegisterAdditionalShell(symbol, entry.PlannedType, registry));
            Refuses<InvalidOperationException>(() => ExpandedSummoningBlueprints.RegisterAdditionalShell(symbol, "UnsupportedShell", registry));
            Refuses<InvalidOperationException>(() => ExpandedSummoningBlueprints.RegisterAdditionalShell(symbol, entry.PlannedType.ToUpperInvariant(), registry));
            Refuses<InvalidDataException>(() => ExpandedSummoningBlueprints.RegisterAdditionalShell(
                "KMG.Summoning.Infernal.IceDevil.Spear", entry.PlannedType, registry));
            Check(UnityEngine.ScriptableObject.Allocations == allocated && registry.RegisteredCount == 1, "Refusal before allocation or registration mutation.");
            var foreign = new BlueprintWeaponEnchantment { name = "foreign replacement" };
            library.BlueprintsByAssetId[shell.AssetGuid] = foreign;
            registry.RollbackAll();
            Check(registry.RegisteredCount == 0 && library.GetAllBlueprints().Count == 0 &&
                ReferenceEquals(library.BlueprintsByAssetId[shell.AssetGuid], foreign), "Rollback preserves another owner's replacement.");
            var collisionRegistry = new BlueprintRegistry(library, manifest, new KingmakerGunslinger.Bootstrap.ModLogger());
            Refuses<InvalidOperationException>(() => ExpandedSummoningBlueprints.RegisterAdditionalShell(symbol, entry.PlannedType, collisionRegistry));
            Check(collisionRegistry.RegisteredCount == 0 && UnityEngine.ScriptableObject.Allocations == allocated, "Foreign collision neither replaces nor allocates.");
            Console.WriteLine("Sprint40 production enchantment factory PASS=" + checks + "; external engine boundary only; no game/deployment/save.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
