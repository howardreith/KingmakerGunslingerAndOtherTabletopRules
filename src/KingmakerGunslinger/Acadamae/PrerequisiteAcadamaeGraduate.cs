using System;
using System.Linq;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;

namespace KingmakerGunslinger.Acadamae
{
    [Serializable]
    public sealed class PrerequisiteAcadamaeGraduate : Prerequisite
    {
        public BlueprintCharacterClass WizardClass;
        public BlueprintFeatureSelection SchoolSelection;
        public BlueprintFeatureSelection OppositionSelection;
        public BlueprintFeature Universalist;

        public override bool Check(FeatureSelectionState selectionState,
            UnitDescriptor unit, LevelUpState state)
        {
            if (unit == null || unit.Progression == null || WizardClass == null ||
                SchoolSelection == null || OppositionSelection == null || Universalist == null)
                return false;
            Spellbook wizardBook = unit.Spellbooks.FirstOrDefault(book =>
                book != null && book.Blueprint != null &&
                ReferenceEquals(book.Blueprint.CharacterClass, WizardClass));
            BlueprintFeature pendingSchool = SelectedFeature(state, SchoolSelection);
            bool committedSpecialist = wizardBook != null &&
                Enum.GetValues(typeof(SpellSchool)).Cast<SpellSchool>()
                    .Any(wizardBook.IsSpecialistSchool);
            var request = new AcadamaePrerequisiteRequest {
                CommittedWizardLevel = unit.Progression.GetClassLevel(WizardClass),
                PendingWizardLevels = state != null &&
                    ReferenceEquals(state.SelectedClass, WizardClass) ? 1 : 0,
                HasSpecialistSchool = committedSpecialist,
                PendingSpecialistSchool = pendingSchool != null &&
                    !ReferenceEquals(pendingSchool, Universalist),
                IsUniversalist = wizardBook != null && !committedSpecialist,
                PendingUniversalist = ReferenceEquals(pendingSchool, Universalist),
                ConjurationForbidden = wizardBook != null &&
                    wizardBook.OppositionSchools.Contains(SpellSchool.Conjuration),
                PendingConjurationForbidden = SelectedFeatures(state,
                    OppositionSelection).Any(IsConjurationOpposition)
            };
            return AcadamaePrerequisitePolicy.Decide(request).Eligible;
        }

        public override string GetUIText()
        {
            return "Specialist Wizard level 1; Conjuration must not be a forbidden school";
        }

        private static BlueprintFeature SelectedFeature(LevelUpState state,
            BlueprintFeatureSelection selection)
        {
            return SelectedFeatures(state, selection).LastOrDefault();
        }

        private static BlueprintFeature[] SelectedFeatures(LevelUpState state,
            BlueprintFeatureSelection selection)
        {
            if (state == null || state.Selections == null)
                return new BlueprintFeature[0];
            return state.Selections.Where(value =>
                value != null && ReferenceEquals(value.Selection, selection) &&
                value.SelectedItem != null).Select(value =>
                    value.SelectedItem.Feature).Where(value => value != null).ToArray();
        }

        private static bool IsConjurationOpposition(BlueprintFeature feature)
        {
            return feature != null && feature.ComponentsArray != null &&
                feature.ComponentsArray.OfType<Kingmaker.UnitLogic.FactLogic.AddOppositionSchool>()
                    .Any(component => component.School == SpellSchool.Conjuration);
        }
    }
}
