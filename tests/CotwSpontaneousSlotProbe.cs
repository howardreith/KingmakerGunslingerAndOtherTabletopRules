using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;

public static class CotwSpontaneousSlotProbe
{
    private const BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
    private static int _passed;
    private sealed class References<T> : IEqualityComparer<T> where T : class
    {
        public bool Equals(T left,T right) {return ReferenceEquals(left,right);}
        public int GetHashCode(T value) {return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);}
    }
    private static readonly List<IntPtr> Blocks = new List<IntPtr>();
    private static T Raw<T>() { return (T)FormatterServices.GetUninitializedObject(typeof(T)); }
    private static void Set(object owner, string name, object value) { owner.GetType().GetField(name, Hidden).SetValue(owner, value); }
    private static T Blueprint<T>() where T : UnityEngine.Object
    {
        T value = Raw<T>();
        IntPtr block = Marshal.AllocHGlobal(64);
        Marshal.Copy(new byte[64], 0, block, 64);
        Blocks.Add(block);
        typeof(UnityEngine.Object).GetField("m_CachedPtr", Hidden).SetValue(value, block);
        return value;
    }
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception("FAIL " + name);
        _passed++; Console.WriteLine("PASS " + name);
    }
    private static UnitDescriptor Owner()
    {
        UnitDescriptor owner = Raw<UnitDescriptor>();
        UnitProgressionData progression = Raw<UnitProgressionData>();
        Set(progression, "Features", new FeatureCollection(owner));
        Set(owner, "Progression", progression);
        Set(owner, "Abilities", new AbilityCollection(owner));
        Set(owner, "m_Spellbooks", new Dictionary<BlueprintSpellbook,Spellbook>(new References<BlueprintSpellbook>()));
        return owner;
    }
    private static Spellbook Book(UnitDescriptor owner, BlueprintAbility source, BlueprintAbility conversion)
    {
        Spellbook book = Raw<Spellbook>();
        Set(book, "Owner", owner);
        BlueprintSpellbook bp = Blueprint<BlueprintSpellbook>();
        bp.Spontaneous = true;
        Set(book, "Blueprint", bp);
        ((Dictionary<BlueprintSpellbook,Spellbook>)typeof(UnitDescriptor).GetField("m_Spellbooks", Hidden).GetValue(owner)).Add(bp,book);
        Set(book, "m_KnownSpellLevels", new Dictionary<BlueprintAbility,int>(new References<BlueprintAbility>()) { {source, 1} });
        BlueprintAbility[] list = new BlueprintAbility[10]; list[1] = conversion;
        Set(book, "m_SpellConversionLists", new List<BlueprintAbility[]> { list });
        Set(book, "m_SpontaneousSlots", new[] {0, 3, 0, 0, 0, 0, 0, 0, 0, 0});
        return book;
    }
    private static AbilityData Spell(BlueprintAbility blueprint, UnitDescriptor owner, Spellbook book)
    {
        AbilityData spell = Raw<AbilityData>();
        Set(spell, "Blueprint", blueprint); Set(spell, "Caster", owner);
        Set(spell, "m_CachedSpellbook", book);
        return spell;
    }
    public static void Run(Assembly mod)
    {
        HarmonyInstance harmony = HarmonyInstance.Create("KMG.Tests.CotwSpontaneousSlot");
        Type adapter = mod.GetType("KingmakerGunslinger.Compatibility.CotwSpontaneousSlotCompatibility", true);
        Assembly cotw = typeof(CallOfTheWild.Main).Assembly;
        MethodInfo brokenPrefix = (MethodInfo)adapter.GetMethod("VerifyContract", Hidden).Invoke(null, new object[] {cotw});
        MethodInfo target = typeof(MechanicActionBarSlotSpontaneusSpell).GetMethod("GetConvertedAbilityData");
        try
        {
            UnitDescriptor owner = Owner();
            BlueprintAbility source = Blueprint<BlueprintAbility>(); source.ComponentsArray = new BlueprintComponent[0];
            BlueprintAbility conversion = Blueprint<BlueprintAbility>(); conversion.ComponentsArray = new BlueprintComponent[0];
            Spellbook book = Book(owner, source, conversion);
            AbilityData spell = Spell(source, owner, book);
            var slot = Raw<MechanicActionBarSlotSpontaneusSpell>(); slot.Spell = spell;
            Check(!typeof(MechanicActionBarSlotMemorizedSpell).IsInstanceOfType(slot), "native slot classes are siblings");

            // Same unchecked receiver shape as the installed prefix. Return object
            // before dereferencing a possibly bogus AbilityData; report its type.
            var read = new DynamicMethod("ProbeWrongCotwRead", typeof(object), new[] {typeof(object)}, typeof(CotwSpontaneousSlotProbe), true);
            ILGenerator il = read.GetILGenerator(); il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, typeof(MechanicActionBarSlotMemorizedSpell).GetProperty("Spell").GetGetMethod()); il.Emit(OpCodes.Ret);
            object wrong = ((Func<object,object>)read.CreateDelegate(typeof(Func<object,object>)))(slot);
            Console.WriteLine("BASELINE wrongRead=" + (wrong == null ? "null" : wrong.GetType().FullName));
            Check(!ReferenceEquals(wrong, spell), "installed wrong getter does not return spontaneous AbilityData");
            Check(ReferenceEquals(adapter.GetMethod("ReadSpell", Hidden).Invoke(null, new object[] {slot}), spell), "correct read preserves exact spell reference");

            FieldInfo settingsField = typeof(CallOfTheWild.Main).GetField("settings", Hidden);
            object settings = FormatterServices.GetUninitializedObject(settingsField.FieldType);
            settingsField.SetValue(null, settings);
            FieldInfo meta = settings.GetType().GetField("<metamagic_for_spontaneous_spell_conversion>k__BackingField", Hidden);
            harmony.Patch(target, new HarmonyMethod(brokenPrefix), null, null);
            Patches before = harmony.GetPatchInfo(target);
            // Match production order: CotW already patched the native method
            // before KMG instruments the existing prefix itself.
            harmony.Patch(brokenPrefix, null, null, new HarmonyMethod(adapter.GetMethod("Transpile", Hidden)));
            Patches after = harmony.GetPatchInfo(target);
            Check(before.Prefixes.Count == after.Prefixes.Count &&
                before.Prefixes.Zip(after.Prefixes,(left,right)=>left.owner == right.owner &&
                    left.index == right.index && left.patch == right.patch && left.priority == right.priority).All(value=>value),
                "native prefix ownership/order unchanged");
            foreach (bool enabled in new[] {false, true})
            {
                meta.SetValue(settings, enabled);
                List<AbilityData> result = slot.GetConvertedAbilityData();
                Check(result.Count == 1 && ReferenceEquals(result[0].Blueprint, conversion), "actual CotW conversion retained; metamagic=" + enabled);
                Check(ReferenceEquals(result[0].ConvertedFrom, spell) && ReferenceEquals(result[0].Spellbook, book) &&
                    ReferenceEquals(result[0].Caster, owner), "conversion preserves exact owner/book/source; metamagic=" + enabled);
            }
            BlueprintAbility variant = Blueprint<BlueprintAbility>(); variant.ComponentsArray = new BlueprintComponent[0];
            AbilityVariants variants = Blueprint<AbilityVariants>(); variants.Variants = new[] {variant};
            BlueprintAbility variantSource = Blueprint<BlueprintAbility>();
            variantSource.ComponentsArray = new BlueprintComponent[] {variants};
            Spellbook variantBook = Book(owner, variantSource, conversion);
            AbilityData variantSpell = Spell(variantSource, owner, variantBook);
            slot.Spell = variantSpell;
            List<AbilityData> withVariants = slot.GetConvertedAbilityData();
            Check(withVariants.Count == 2 && ReferenceEquals(withVariants[1].Blueprint, variant), "CotW variant option retained alongside conversion");
            Check(ReferenceEquals(withVariants[1].ConvertedFrom, variantSpell) && ReferenceEquals(withVariants[1].Spellbook, variantBook), "variant retains exact source/book");
            slot.Spell = spell;
            var memorized = Raw<MechanicActionBarSlotMemorizedSpell>();
            memorized.SpellSlot = new SpellSlot(1, SpellSlotType.Common, 0); memorized.SpellSlot.Spell = spell;
            Check(ReferenceEquals(memorized.Spell, spell), "memorized getter remains unchanged");
            Type memoryPatch = cotw.GetType("CallOfTheWild.SpellManipulationMechanics.MechanicActionBarSlotMemorizedSpell__GetConvertedAbilityData__Patch", true);
            harmony.Patch(typeof(MechanicActionBarSlotMemorizedSpell).GetMethod("GetConvertedAbilityData"),
                new HarmonyMethod(memoryPatch.GetMethod("Prefix", Hidden)), null, null);
            Check(memorized.GetConvertedAbilityData().Any(value => ReferenceEquals(value.Blueprint, conversion)), "valid memorized CotW conversion retained");
            slot.Spell = null;
            Check(slot.GetConvertedAbilityData().Count == 0, "empty native slot is safe");
            slot.Spell = Spell(source, null, book);
            Check(slot.GetConvertedAbilityData().Count == 0, "unbound caster is safe");
            slot.Spell = spell;
            Check(slot.GetConvertedAbilityData().Count == 1, "later native rebind reads ready context without cached empty list");
            Spellbook other = Book(Owner(), source, conversion);
            slot.Spell = Spell(source, owner, other);
            Check(slot.GetConvertedAbilityData().Count == 0, "wrong owner is not substituted");
            Spellbook second = Book(owner, source, conversion);
            slot.Spell = Spell(source, owner, second);
            Check(ReferenceEquals(slot.GetConvertedAbilityData()[0].Spellbook, second), "second spellbook retains its own identity");
            slot.Spell = spell;
            for(int i=0;i<20;i++) Check(slot.GetConvertedAbilityData().Count == 1, "repeated binding " + i);
            Check(((int[])typeof(Spellbook).GetField("m_SpontaneousSlots", Hidden).GetValue(book))[1] == 3, "presentation does not spend slots");
            bool rejected = false;
            try { adapter.GetMethod("VerifyContract", Hidden).Invoke(null, new object[] {mod}); }
            catch (TargetInvocationException) { rejected = true; }
            Check(rejected, "unsupported provider identity refused");
            Console.WriteLine("Native-contract adapter assertions: PASS=" + _passed + ";FAIL=0;saveLoad=NOT_RUN");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            foreach(IntPtr block in Blocks) Marshal.FreeHGlobal(block);
        }
    }
}
