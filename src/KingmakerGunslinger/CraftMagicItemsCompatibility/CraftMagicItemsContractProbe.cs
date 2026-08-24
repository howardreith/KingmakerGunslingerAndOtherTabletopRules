using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KingmakerGunslinger.CraftMagicItemsCompatibility
{
    internal sealed class CraftMagicItemsContract
    {
        internal CraftMagicItemsContract(Assembly assembly, Type mainType,
            Type itemDataType, Type recipeDataType, Type recipeBasedType,
            Type blueprintPatcherType, FieldInfo itemDataField,
            FieldInfo enabledField, FieldInfo harmonyInstanceField,
            FieldInfo blueprintPatcherField,
            FieldInfo selectedIndexField, FieldInfo subCraftingDataField,
            FieldInfo typeToItemField, FieldInfo enchantmentToItemField,
            FieldInfo enchantmentToRecipeField,
            FieldInfo enchantmentToCostField, MethodInfo initializeData,
            MethodInfo addAllCraftingFeats, MethodInfo onToggle,
            MethodInfo canEnchant,
            MethodInfo recipeApplies, MethodInfo blueprintMatchesSlot,
            MethodInfo itemMatchesEnchantments,
            MethodInfo renderRecipeBased, MethodInfo renderMundane,
            MethodInfo craftItem, MethodInfo addRecipeForEnchantment,
            MethodInfo getSelectedCrafter, MethodInfo drawSelection,
            MethodInfo renderCraftingSkill, MethodInfo renderCraftControl,
            MethodInfo buildCustomRecipeGuid, MethodInfo readJsonFile,
            MethodInfo addItemIdForEnchantment,
            MethodInfo itemPlusEquivalent, MethodInfo rulesRecipeItemCost)
        {
            Assembly = assembly;
            MainType = mainType;
            ItemDataType = itemDataType;
            RecipeDataType = recipeDataType;
            RecipeBasedType = recipeBasedType;
            BlueprintPatcherType = blueprintPatcherType;
            ItemDataField = itemDataField;
            EnabledField = enabledField;
            HarmonyInstanceField = harmonyInstanceField;
            BlueprintPatcherField = blueprintPatcherField;
            SelectedIndexField = selectedIndexField;
            SubCraftingDataField = subCraftingDataField;
            TypeToItemField = typeToItemField;
            EnchantmentToItemField = enchantmentToItemField;
            EnchantmentToRecipeField = enchantmentToRecipeField;
            EnchantmentToCostField = enchantmentToCostField;
            InitializeData = initializeData;
            AddAllCraftingFeats = addAllCraftingFeats;
            OnToggle = onToggle;
            CanEnchant = canEnchant;
            RecipeApplies = recipeApplies;
            BlueprintMatchesSlot = blueprintMatchesSlot;
            ItemMatchesEnchantments = itemMatchesEnchantments;
            RenderRecipeBased = renderRecipeBased;
            RenderMundane = renderMundane;
            CraftItem = craftItem;
            AddRecipeForEnchantment = addRecipeForEnchantment;
            GetSelectedCrafter = getSelectedCrafter;
            DrawSelection = drawSelection;
            RenderCraftingSkill = renderCraftingSkill;
            RenderCraftControl = renderCraftControl;
            BuildCustomRecipeGuid = buildCustomRecipeGuid;
            ReadJsonFile = readJsonFile;
            AddItemIdForEnchantment = addItemIdForEnchantment;
            ItemPlusEquivalent = itemPlusEquivalent;
            RulesRecipeItemCost = rulesRecipeItemCost;
        }

        internal Assembly Assembly { get; private set; }
        internal Type MainType { get; private set; }
        internal Type ItemDataType { get; private set; }
        internal Type RecipeDataType { get; private set; }
        internal Type RecipeBasedType { get; private set; }
        internal Type BlueprintPatcherType { get; private set; }
        internal FieldInfo ItemDataField { get; private set; }
        internal FieldInfo EnabledField { get; private set; }
        internal FieldInfo HarmonyInstanceField { get; private set; }
        internal FieldInfo BlueprintPatcherField { get; private set; }
        internal FieldInfo SelectedIndexField { get; private set; }
        internal FieldInfo SubCraftingDataField { get; private set; }
        internal FieldInfo TypeToItemField { get; private set; }
        internal FieldInfo EnchantmentToItemField { get; private set; }
        internal FieldInfo EnchantmentToRecipeField { get; private set; }
        internal FieldInfo EnchantmentToCostField { get; private set; }
        internal MethodInfo InitializeData { get; private set; }
        internal MethodInfo AddAllCraftingFeats { get; private set; }
        internal MethodInfo OnToggle { get; private set; }
        internal MethodInfo CanEnchant { get; private set; }
        internal MethodInfo RecipeApplies { get; private set; }
        internal MethodInfo BlueprintMatchesSlot { get; private set; }
        internal MethodInfo ItemMatchesEnchantments { get; private set; }
        internal MethodInfo RenderRecipeBased { get; private set; }
        internal MethodInfo RenderMundane { get; private set; }
        internal MethodInfo CraftItem { get; private set; }
        internal MethodInfo AddRecipeForEnchantment { get; private set; }
        internal MethodInfo GetSelectedCrafter { get; private set; }
        internal MethodInfo DrawSelection { get; private set; }
        internal MethodInfo RenderCraftingSkill { get; private set; }
        internal MethodInfo RenderCraftControl { get; private set; }
        internal MethodInfo BuildCustomRecipeGuid { get; private set; }
        internal MethodInfo ReadJsonFile { get; private set; }
        internal MethodInfo AddItemIdForEnchantment { get; private set; }
        internal MethodInfo ItemPlusEquivalent { get; private set; }
        internal MethodInfo RulesRecipeItemCost { get; private set; }
    }

    internal sealed class CraftMagicItemsContractResolution
    {
        internal CraftMagicItemsContractResolution(
            CraftMagicItemsContract contract, string failedCheck)
        {
            Contract = contract;
            FailedCheck = failedCheck ?? string.Empty;
        }

        internal CraftMagicItemsContract Contract { get; private set; }
        internal string FailedCheck { get; private set; }
        internal bool IsCompatible { get { return Contract != null; } }
    }

    internal static class CraftMagicItemsContractProbe
    {
        internal const string ModId = "CraftMagicItems";
        internal const string AssemblyName = "CraftMagicItems";
        internal const string MainTypeName = "CraftMagicItems.Main";
        private const BindingFlags Static = BindingFlags.Static |
            BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags Instance = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;

        internal static CraftMagicItemsContractResolution Probe(
            Assembly assembly, bool requireAssemblyIdentity)
        {
            if (assembly == null) return Fail("assembly-null");
            try
            {
                if (requireAssemblyIdentity && !string.Equals(
                        assembly.GetName().Name, AssemblyName,
                        StringComparison.Ordinal))
                    return Fail("assembly-name");

                Type main = assembly.GetType(MainTypeName, false, false);
                Type itemData = assembly.GetType(
                    "CraftMagicItems.ItemCraftingData", false, false);
                Type recipeData = assembly.GetType(
                    "CraftMagicItems.RecipeData", false, false);
                Type recipeBased = assembly.GetType(
                    "CraftMagicItems.RecipeBasedItemCraftingData", false,
                    false);
                Type blueprintPatcher = assembly.GetType(
                    "CraftMagicItems.CraftMagicItemsBlueprintPatcher", false,
                    false);
                if (main == null || itemData == null || recipeData == null ||
                    recipeBased == null || blueprintPatcher == null ||
                    !itemData.IsAssignableFrom(recipeBased))
                    return Fail("required-types");

                FieldInfo itemDataField = Field(main, "ItemCraftingData",
                    true);
                FieldInfo enabledField = Field(main, "modEnabled", true);
                FieldInfo harmonyInstanceField = Field(main,
                    "harmonyInstance", true);
                FieldInfo blueprintPatcherField = Field(main,
                    "blueprintPatcher", true);
                FieldInfo selectedIndexField = Field(main, "SelectedIndex",
                    true);
                FieldInfo subCraftingDataField = Field(main,
                    "SubCraftingData", true);
                FieldInfo typeToItemField = Field(main, "TypeToItem", true);
                FieldInfo enchantmentToItemField = Field(main,
                    "EnchantmentIdToItem", true);
                FieldInfo enchantmentToRecipeField = Field(main,
                    "EnchantmentIdToRecipe", true);
                FieldInfo enchantmentToCostField = Field(main,
                    "EnchantmentIdToCost", true);
                if (itemDataField == null ||
                    itemDataField.FieldType != itemData.MakeArrayType() ||
                    enabledField == null || enabledField.FieldType !=
                        typeof(bool) || harmonyInstanceField == null ||
                    harmonyInstanceField.FieldType.FullName !=
                        "HarmonyLib.Harmony" || !ValidateHarmonyShape(
                            harmonyInstanceField.FieldType) ||
                    blueprintPatcherField == null ||
                    blueprintPatcherField.FieldType != blueprintPatcher ||
                    !IsDictionary(selectedIndexField, typeof(string),
                        typeof(int)) ||
                    !IsDictionaryWithStringKey(subCraftingDataField) ||
                    !IsDictionaryWithStringKey(typeToItemField) ||
                    !IsDictionaryWithStringKey(enchantmentToItemField) ||
                    !IsDictionaryWithStringKey(enchantmentToRecipeField) ||
                    !IsDictionary(enchantmentToCostField, typeof(string),
                        typeof(int)))
                    return Fail("main-static-fields");

                if (!ValidateItemDataShape(itemData, recipeBased,
                        recipeData))
                    return Fail("crafting-data-shape");

                Type lifecycle = main.GetNestedType("MainMenuStartPatch",
                    BindingFlags.NonPublic | BindingFlags.Public);
                MethodInfo initializeData = lifecycle == null ? null :
                    Method(lifecycle, "InitialiseCraftingData", true, 0,
                        typeof(void));
                MethodInfo addAllCraftingFeats = lifecycle == null ? null :
                    Method(lifecycle, "AddAllCraftingFeats", true, 0,
                        typeof(void));
                MethodInfo onToggle = Method(main, "OnToggle", true, 2,
                    typeof(bool));
                MethodInfo canEnchant = Method(main, "CanEnchant", true, 1,
                    typeof(bool));
                MethodInfo recipeApplies = Method(main,
                    "RecipeAppliesToBlueprint", true, 4, typeof(bool));
                MethodInfo blueprintMatchesSlot = Method(main,
                    "DoesBlueprintMatchSlot", true, 2, typeof(bool));
                MethodInfo itemMatchesEnchantments = Method(main,
                    "DoesItemMatchAllEnchantments", true, 5, typeof(bool));
                MethodInfo renderRecipeBased = Method(main,
                    "RenderRecipeBasedCrafting", true, 3, typeof(void));
                MethodInfo renderMundane = Method(main,
                    "RenderCraftMundaneItemsSection", true, 0,
                    typeof(void));
                MethodInfo craftItem = Method(main, "CraftItem", true, 2,
                    typeof(void));
                MethodInfo addRecipe = Method(main,
                    "AddRecipeForEnchantment", true, 2, typeof(void));
                MethodInfo getCrafter = Method(main, "GetSelectedCrafter",
                    true, 1, null);
                MethodInfo drawSelection = main.GetMethods(Static)
                    .SingleOrDefault(value => value.Name ==
                        "DrawSelectionUserInterfaceElements" &&
                        !value.IsGenericMethod && value.ReturnType ==
                        typeof(int) && Parameters(value, typeof(string),
                            typeof(string[]), typeof(int)));
                MethodInfo renderSkill = Method(main,
                    "RenderCraftingSkillInformation", true, 9,
                    typeof(int));
                MethodInfo renderControl = Method(main,
                    "RenderRecipeBasedCraftItemControl", true, 6,
                    typeof(void));
                MethodInfo buildGuid = blueprintPatcher.GetMethods(Instance)
                    .SingleOrDefault(value => value.Name ==
                        "BuildCustomRecipeItemGuid" && value.ReturnType ==
                        typeof(string) && value.GetParameters().Length >= 2 &&
                        value.GetParameters()[0].ParameterType ==
                        typeof(string) && typeof(IEnumerable<string>)
                            .IsAssignableFrom(value.GetParameters()[1]
                                .ParameterType));
                MethodInfo readJson = main.GetMethods(Static)
                    .SingleOrDefault(value => value.Name == "ReadJsonFile" &&
                        value.IsGenericMethodDefinition &&
                        value.GetGenericArguments().Length == 1 &&
                        value.GetParameters().Length == 2 &&
                        value.GetParameters()[0].ParameterType ==
                            typeof(string));
                MethodInfo addItemId = Method(main,
                    "AddItemIdForEnchantment", true, 1, typeof(void));
                MethodInfo itemPlus = Method(main, "ItemPlusEquivalent",
                    true, 1, typeof(int));
                MethodInfo rulesCost = main.GetMethods(Static)
                    .SingleOrDefault(value => value.Name ==
                        "RulesRecipeItemCost" && !value.IsGenericMethod &&
                        value.ReturnType == typeof(int) &&
                        value.GetParameters().Length == 3);
                if (initializeData == null || addAllCraftingFeats == null ||
                    onToggle == null ||
                    canEnchant == null || recipeApplies == null ||
                    blueprintMatchesSlot == null ||
                    itemMatchesEnchantments == null ||
                    renderRecipeBased == null || renderMundane == null ||
                    craftItem == null || addRecipe == null ||
                    getCrafter == null || drawSelection == null ||
                    renderSkill == null || renderControl == null ||
                    buildGuid == null || readJson == null ||
                    addItemId == null || itemPlus == null ||
                    rulesCost == null)
                    return Fail("required-methods");

                return new CraftMagicItemsContractResolution(
                    new CraftMagicItemsContract(assembly, main, itemData,
                        recipeData, recipeBased, blueprintPatcher,
                        itemDataField, enabledField, harmonyInstanceField,
                        blueprintPatcherField,
                        selectedIndexField, subCraftingDataField,
                        typeToItemField, enchantmentToItemField,
                        enchantmentToRecipeField, enchantmentToCostField,
                        initializeData, addAllCraftingFeats, onToggle,
                        canEnchant, recipeApplies,
                        blueprintMatchesSlot, itemMatchesEnchantments,
                        renderRecipeBased, renderMundane, craftItem, addRecipe,
                        getCrafter, drawSelection, renderSkill, renderControl,
                        buildGuid, readJson, addItemId, itemPlus, rulesCost),
                    string.Empty);
            }
            catch (Exception exception)
            {
                return Fail("probe-exception:" +
                    exception.GetType().FullName);
            }
        }

        private static bool ValidateItemDataShape(Type itemData,
            Type recipeBased, Type recipeData)
        {
            FieldInfo newBases = Field(itemData, "m_NewItemBaseIDs", false);
            FieldInfo cachedBases = Field(itemData,
                "m_CachedNewItemBaseIDs", false);
            PropertyInfo newBasesProperty = itemData.GetProperty(
                "NewItemBaseIDs", Instance);
            PropertyInfo dataType = itemData.GetProperty("DataType", Instance);
            FieldInfo recipes = Field(recipeBased, "Recipes", false);
            FieldInfo subRecipes = Field(recipeBased, "SubRecipes", false);
            FieldInfo slots = Field(recipeBased, "Slots", false);
            FieldInfo slotRestrictions = Field(recipeBased,
                "SlotRestrictions", false);
            FieldInfo recipeFiles = Field(recipeBased, "RecipeFileNames",
                false);
            FieldInfo resultItem = Field(recipeData, "m_ResultItem", false);
            FieldInfo enchantments = Field(recipeData, "m_Enchantments",
                false);
            PropertyInfo resultProperty = recipeData.GetProperty("ResultItem",
                Instance);
            PropertyInfo enchantmentProperty = recipeData.GetProperty(
                "Enchantments", Instance);
            Type dataTypeEnum = dataType == null ? null :
                dataType.PropertyType;
            Type slotEnum = slots == null ? null :
                slots.FieldType.GetElementType();
            Type restrictionEnum = Field(recipeData, "Restrictions", false)
                ?.FieldType.GetElementType();
            Type costEnum = Field(recipeData, "CostType", false)?.FieldType;
            return dataType != null && dataType.CanWrite &&
                IsEnumWith(dataTypeEnum, "RecipeBased") &&
                newBases != null && newBases.FieldType.IsArray &&
                newBases.FieldType.GetElementType().IsArray &&
                cachedBases != null && cachedBases.FieldType.IsArray &&
                newBasesProperty != null &&
                newBasesProperty.PropertyType == cachedBases.FieldType &&
                HasFields(itemData,
                    FieldShape.Of<string>("Name"),
                    FieldShape.Of<string>("NameId"),
                    FieldShape.Of<string>("ParentNameId"),
                    FieldShape.Of<string>("FeatGuid"),
                    FieldShape.Of<int>("MinimumCasterLevel"),
                    FieldShape.Of<bool>("PrerequisitesMandatory"),
                    FieldShape.Of<int>("Count")) && recipes != null &&
                recipes.FieldType == recipeData.MakeArrayType() &&
                subRecipes != null && IsDictionaryWithStringKey(subRecipes) &&
                slots != null && slots.FieldType.IsArray &&
                IsEnumWith(slotEnum, "Weapon", "Usable") &&
                slotRestrictions != null &&
                slotRestrictions.FieldType.IsArray &&
                recipeFiles != null && recipeFiles.FieldType ==
                    typeof(string[]) &&
                HasFields(recipeBased,
                    FieldShape.Of<int>("MundaneBaseDC"),
                    FieldShape.Of<bool>("MundaneEnhancementsStackable")) &&
                resultItem != null &&
                resultItem.FieldType.IsArray && enchantments != null &&
                enchantments.FieldType.IsArray &&
                enchantments.FieldType.GetElementType().IsArray &&
                resultProperty != null && enchantmentProperty != null &&
                enchantmentProperty.PropertyType.IsArray &&
                HasFields(recipeData,
                    FieldShape.Of<string>("Name"),
                    FieldShape.Of<string>("NameId"),
                    FieldShape.Of<string>("ParentNameId"),
                    FieldShape.Of<bool>("EnchantmentsCumulative"),
                    FieldShape.Of<int>("CasterLevelStart"),
                    FieldShape.Of<int>("CasterLevelMultiplier"),
                    FieldShape.ArrayField("PrerequisiteSpells"),
                    FieldShape.Of<int>("CostFactor"),
                    FieldShape.Of<int>("CostAdjustment"),
                    FieldShape.ArrayField("OnlyForSlots"),
                    FieldShape.ArrayField("Restrictions"),
                    FieldShape.Of<bool>("CanApplyToMundaneItem")) &&
                IsEnumWith(costEnum, "Flat", "EnhancementLevelSquared") &&
                IsEnumWith(restrictionEnum, "Weapon");
        }

        private static bool ValidateHarmonyShape(Type harmony)
        {
            Type method = harmony == null ? null : harmony.Assembly.GetType(
                "HarmonyLib.HarmonyMethod", false, false);
            ConstructorInfo harmonyConstructor = harmony == null ? null :
                harmony.GetConstructor(new[] { typeof(string) });
            ConstructorInfo methodConstructor = method == null ? null :
                method.GetConstructor(new[] { typeof(MethodInfo) });
            MethodInfo[] patches = harmony == null ? new MethodInfo[0] :
                harmony.GetMethods(BindingFlags.Instance |
                    BindingFlags.Public).Where(value => value.Name ==
                    "Patch" && value.GetParameters().Length == 5 &&
                    typeof(MethodBase).IsAssignableFrom(value.GetParameters()[0]
                        .ParameterType)).ToArray();
            MethodInfo[] unpatches = harmony == null ? new MethodInfo[0] :
                harmony.GetMethods(BindingFlags.Instance |
                    BindingFlags.Public).Where(value => value.Name ==
                    "UnpatchAll" && value.ReturnType == typeof(void) &&
                    value.GetParameters().Length == 1 &&
                    value.GetParameters()[0].ParameterType == typeof(string))
                    .ToArray();
            return harmonyConstructor != null && methodConstructor != null &&
                patches.Length == 1 && unpatches.Length == 1;
        }

        private static bool HasFields(Type type, params FieldShape[] shapes)
        {
            return shapes.All(shape => shape.Matches(Field(type, shape.Name,
                false)));
        }

        private static bool IsEnumWith(Type type, params string[] names)
        {
            return type != null && type.IsEnum && names.All(name =>
                Enum.GetNames(type).Contains(name, StringComparer.Ordinal));
        }

        private static FieldInfo Field(Type type, string name,
            bool requireStatic)
        {
            FieldInfo value = type.GetField(name,
                requireStatic ? Static : Instance);
            return value != null && value.IsStatic == requireStatic ? value :
                null;
        }

        private static MethodInfo Method(Type type, string name,
            bool requireStatic, int parameterCount, Type returnType)
        {
            MethodInfo[] matches = type.GetMethods(requireStatic ? Static :
                    Instance).Where(value => value.Name == name &&
                    value.IsStatic == requireStatic &&
                    !value.IsGenericMethod && value.GetParameters().Length ==
                    parameterCount && (returnType == null ||
                        value.ReturnType == returnType)).ToArray();
            return matches.Length == 1 ? matches[0] : null;
        }

        private static bool IsDictionary(FieldInfo field, Type key,
            Type value)
        {
            if (field == null || !field.FieldType.IsGenericType ||
                field.FieldType.GetGenericTypeDefinition() !=
                    typeof(Dictionary<,>)) return false;
            Type[] arguments = field.FieldType.GetGenericArguments();
            return arguments[0] == key && arguments[1] == value;
        }

        private static bool IsDictionaryWithStringKey(FieldInfo field)
        {
            return field != null && field.FieldType.IsGenericType &&
                field.FieldType.GetGenericTypeDefinition() ==
                    typeof(Dictionary<,>) &&
                field.FieldType.GetGenericArguments()[0] == typeof(string) &&
                typeof(IDictionary).IsAssignableFrom(field.FieldType);
        }

        private static bool Parameters(MethodInfo method,
            params Type[] types)
        {
            ParameterInfo[] parameters = method.GetParameters();
            return parameters.Length == types.Length && parameters.Select(
                value => value.ParameterType).SequenceEqual(types);
        }

        private static CraftMagicItemsContractResolution Fail(string check)
        { return new CraftMagicItemsContractResolution(null, check); }

        private sealed class FieldShape
        {
            private FieldShape(string name, Type type, bool array)
            {
                Name = name;
                Type = type;
                IsArray = array;
            }

            internal string Name { get; private set; }
            internal Type Type { get; private set; }
            internal bool IsArray { get; private set; }

            internal static FieldShape Of<T>(string name)
            { return new FieldShape(name, typeof(T), false); }

            internal static FieldShape ArrayField(string name)
            { return new FieldShape(name, null, true); }

            internal bool Matches(FieldInfo field)
            {
                return field != null && (IsArray
                    ? field.FieldType.IsArray
                    : field.FieldType == Type);
            }
        }
    }
}
