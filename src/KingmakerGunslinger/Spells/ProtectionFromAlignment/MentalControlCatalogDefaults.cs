using System.Collections.Generic;

namespace KingmakerGunslinger.Spells.ProtectionFromAlignment
{
    internal static class MentalControlCatalogDefaults
    {
        internal static MentalControlCatalog Create()
        {
            var catalog = new MentalControlCatalog();
            foreach (MentalControlCatalogEntry entry in Entries)
                catalog.Register(entry);
            return catalog;
        }

        internal static IReadOnlyList<MentalControlCatalogEntry> All
        { get { return Entries; } }

        private static MentalControlCatalogEntry Ability(string name, string guid,
            MentalControlContentSource source, string reason, bool required)
        {
            return new MentalControlCatalogEntry(name, guid,
                MentalControlBlueprintKind.Ability, source, reason, required);
        }

        private static MentalControlCatalogEntry Buff(string name, string guid,
            MentalControlContentSource source, string reason, bool required)
        {
            return new MentalControlCatalogEntry(name, guid,
                MentalControlBlueprintKind.Buff, source, reason, required);
        }

        private static readonly MentalControlCatalogEntry[] Entries = {
            Ability("DominatePerson", "d7cbd2004ce66a042aeab2e95a3c5c61",
                MentalControlContentSource.VanillaKingmaker,
                "Domination delivery that changes the victim to the creator's faction.", true),
            Ability("DominateMonster", "3c17035ec4717674cae2e841a190e757",
                MentalControlContentSource.VanillaKingmaker,
                "Creature-wide domination delivery using the native domination state.", true),
            Ability("DominateAnimal", "754c478a2aa9bb54d809e648c3f7ac0e",
                MentalControlContentSource.VanillaKingmaker,
                "Animal domination delivery using the native domination state.", true),
            Ability("C61_NyrissaDominateMonster",
                "e349d48d79783d24aba78006f3e84b8c",
                MentalControlContentSource.VanillaKingmaker,
                "Nyrissa's alternate monster-domination delivery.", true),
            Ability("EnchantmentDominateSpell",
                "0f368511a1f73ba4b8b3fd204e751572",
                MentalControlContentSource.VanillaKingmaker,
                "Alternate enchantment-theme domination spell delivery.", true),
            Ability("CharmAnimal", "08df458bd00ba704dab32dd493c61518",
                MentalControlContentSource.VanillaKingmaker,
                "Charm delivery that faction-converts an animal to the creator's side.", true),
            Ability("CharmPerson", "1af9d5995090e5a4185a30decf0959ad",
                MentalControlContentSource.VanillaKingmaker,
                "Charm delivery that faction-converts a humanoid to the creator's side.", true),
            Ability("BloodlineSerpentineScaledSoulCharmingGazeAbility",
                "a5d4f66181c8085429640339f417eae8",
                MentalControlContentSource.VanillaKingmaker,
                "Alternate charming-gaze delivery of the native faction-changing charm.", true),
            Buff("DominatePersonBuff", "c0f4e1c24c9cd334ca988ed1bd9d201f",
                MentalControlContentSource.VanillaKingmaker,
                "Authoritative native faction-changing domination terminal state.", true),
            Buff("DominatePersonUniqueBuff", "d6f8f810781b5394392d99204c6a02c2",
                MentalControlContentSource.VanillaKingmaker,
                "Alternate encounter domination terminal state.", true),
            Buff("EnchantmentDominatePersonBuff",
                "cb7e4dd25ad20f345b6351fdd4c621f3",
                MentalControlContentSource.VanillaKingmaker,
                "Alternate enchantment-theme faction-changing terminal state.", true),
            Buff("Charm", "9dc29118addce3d48ae9b92be953b5b4",
                MentalControlContentSource.VanillaKingmaker,
                "Authoritative native faction-changing charm terminal state.", true),
            Ability("KMG.Summoning.Special.Succubus.Dominate",
                "1662d63944d94cdeaa62562dc9ac9349",
                MentalControlContentSource.KingmakerGunslinger,
                "Expanded Summoning Succubus direct domination delivery.", true),
            Buff("KMG.Summoning.Special.Succubus.Domination",
                "6e1f6eb3e773451dbda9e0ecd07486d9",
                MentalControlContentSource.KingmakerGunslinger,
                "Expanded Summoning Succubus faction-changing terminal state.", true),
            Ability("ControlUndeadAbility", "998469fa09314fd687b4ffa051a95c59",
                MentalControlContentSource.CallOfTheWild,
                "Optional spell gives its creator ongoing control of an undead target.", false),
            Buff("ControlUndeadBuff", "21d20a30b93e4ae281a6d70d9ae1a64d",
                MentalControlContentSource.CallOfTheWild,
                "Optional Control Undead faction-changing terminal state.", false),
            Ability("ControlConstructAbility", "efec86b954ff42e99893d55f99e51a5e",
                MentalControlContentSource.CallOfTheWild,
                "Optional spell gives its creator ongoing control of a construct.", false),
            Buff("ControlConstructBuff", "fe97da5e44014fd8a643a54fe791e7ae",
                MentalControlContentSource.CallOfTheWild,
                "Optional Control Construct faction-changing terminal state.", false),
            Ability("WitchAnimalServantHexAbility",
                "583e661fe4244a319672bc6ccdc51294",
                MentalControlContentSource.CallOfTheWild,
                "Optional hex explicitly robs the target of free will and changes faction.", false),
            Buff("WitchAnimalServantHexBuff",
                "32b4b11964724f59a9034e61014dbb3c",
                MentalControlContentSource.CallOfTheWild,
                "Optional Animal Servant faction-changing terminal state.", false),
            Ability("SwayingWordAbility", "e5096e16c9cb46cf9460a9c84dea699b",
                MentalControlContentSource.CallOfTheWild,
                "Optional inquisition power applies a bounded Dominate Person effect.", false)
        };
    }
}
