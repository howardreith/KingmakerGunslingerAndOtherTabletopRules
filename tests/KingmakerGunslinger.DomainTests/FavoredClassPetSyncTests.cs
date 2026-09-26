using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Review finding 3: the pet-armor projection follows one qualified
    /// desired pet and never leaves an orphaned bonus.
    /// </summary>
    internal static class FavoredClassPetSyncTests
    {
        private sealed class Pet
        {
            internal readonly string Name;
            internal bool Projected;
            internal bool Qualified = true;
            internal string Master = "ranger";
            internal bool Alive = true;

            internal Pet(string name) { Name = name; }
        }

        /// <summary>A master with the counter: its tracked pet and its current pet.</summary>
        private sealed class Master
        {
            internal Pet Tracked;
            internal Pet Current;
            internal readonly HashSet<string> OtherHolders = new HashSet<string>(StringComparer.Ordinal);

            internal Pet Desired
            {
                get { return Current != null && Current.Alive && Current.Qualified ? Current : null; }
            }

            internal void Sync()
            {
                Pet tracked = Tracked != null && Tracked.Alive ? Tracked : null;
                FavoredClassPetSyncPlan plan = FavoredClassPetSync.Plan(tracked, Desired, pet => pet.Projected,
                    pet => pet.Master != "ranger" && OtherHolders.Contains(pet.Master));
                if (plan.Unproject) tracked.Projected = false;
                if (plan.Project) Desired.Projected = true;
                Tracked = Desired;
            }
        }

        private static void Invariant(Master master, IEnumerable<Pet> pets, string step)
        {
            foreach (Pet pet in pets.Where(pet => pet.Alive))
            {
                bool shouldHave = ReferenceEquals(pet, master.Desired) ||
                    (pet.Master != "ranger" && master.OtherHolders.Contains(pet.Master) && pet.Projected);
                Assertions.Equal(shouldHave, pet.Projected, step + ": " + pet.Name + " projection.");
            }
        }

        internal static void EveryTransitionKeepsOneProjection()
        {
            var master = new Master();
            var first = new Pet("first");
            var second = new Pet("second");
            var third = new Pet("third");
            var all = new[] { first, second, third };
            master.Current = first;
            master.Sync();
            Invariant(master, all, "linked");
            Assertions.True(first.Projected, "The linked pet is projected.");

            // Unlink without replacement (native SetMaster(null)).
            first.Master = null;
            master.Current = null;
            master.Sync();
            Invariant(master, all, "unlinked");
            Assertions.False(first.Projected, "The unlinked pet keeps no orphaned bonus.");
            Assertions.True(master.Tracked == null, "Nothing is tracked.");

            // Relink, then replacement.
            first.Master = "ranger";
            master.Current = first;
            master.Sync();
            Assertions.True(first.Projected, "Relinked: projected again, once.");
            first.Master = null;
            master.Current = second;
            master.Sync();
            Invariant(master, all, "replaced");
            Assertions.True(!first.Projected && second.Projected, "The replacement moves the projection.");

            // Dismissal (RemoveMaster) and resummoning a new pet.
            second.Master = null;
            master.Current = null;
            master.Sync();
            Assertions.False(second.Projected, "The dismissed pet keeps no bonus.");
            master.Current = third;
            master.Sync();
            Invariant(master, all, "resummoned");
            Assertions.True(third.Projected, "The resummoned pet is projected once.");

            // The same pet stops qualifying, then qualifies again.
            third.Qualified = false;
            master.Sync();
            Invariant(master, all, "unqualified");
            Assertions.False(third.Projected, "An unqualified pet loses the projection.");
            third.Qualified = true;
            master.Sync();
            Assertions.True(third.Projected, "Qualified again: projected once.");

            // Save/reload: the same tracked pet is refreshed, never doubled.
            FavoredClassPetSyncPlan reload = FavoredClassPetSync.Plan(third, third, pet => pet.Projected, pet => false);
            Assertions.True(reload.Refresh && !reload.Project && !reload.Unproject,
                "A reloaded projection is refreshed, not added twice or removed.");

            // Destroyed pet: forgotten, and a new pet is projected.
            third.Alive = false;
            master.Current = first;
            first.Master = "ranger";
            master.Sync();
            Invariant(master, all, "destroyed");
            Assertions.True(first.Projected, "After a destroyed pet the new one is projected.");

            // Transfer to another master who holds the same counter: kept.
            master.OtherHolders.Add("paladin");
            first.Master = "paladin";
            master.Current = null;
            master.Sync();
            Assertions.True(first.Projected, "Another holder of the counter keeps the projection.");
            Assertions.True(master.Tracked == null, "This master no longer tracks it.");
        }

        internal static void PlanDecisionsAreExact()
        {
            object a = new object(), b = new object();
            Func<object, bool> has = value => ReferenceEquals(value, a);
            Func<object, bool> none = value => false;
            FavoredClassPetSyncPlan plan = FavoredClassPetSync.Plan(a, null, has, none);
            Assertions.True(plan.Unproject && !plan.Project && !plan.Refresh, "Tracked, no desired: unproject.");
            plan = FavoredClassPetSync.Plan(a, b, has, none);
            Assertions.True(plan.Unproject && plan.Project && !plan.Refresh, "Replacement: move.");
            plan = FavoredClassPetSync.Plan(a, a, has, none);
            Assertions.True(!plan.Unproject && !plan.Project && plan.Refresh, "Same pet: refresh only.");
            plan = FavoredClassPetSync.Plan(a, null, has, value => true);
            Assertions.False(plan.Unproject, "Claimed by another holder: left alone.");
            plan = FavoredClassPetSync.Plan(null, b, has, none);
            Assertions.True(!plan.Unproject && plan.Project, "Nothing tracked: project the desired pet.");
            plan = FavoredClassPetSync.Plan<object>(null, null, has, none);
            Assertions.True(!plan.Unproject && !plan.Project && !plan.Refresh, "Nothing to do.");
            plan = FavoredClassPetSync.Plan(b, null, has, none);
            Assertions.False(plan.Unproject, "A tracked pet without the projection needs no removal.");
        }

        internal static void ProjectionAndUnlinkHookAreWired()
        {
            string root = Path.Combine(Environment.CurrentDirectory, "src", "KingmakerGunslinger", "FavoredClass");
            string projection = File.ReadAllText(Path.Combine(root, "Mechanics", "FavoredClassPetNaturalArmor.cs"));
            foreach (string token in new[] {
                "FavoredClassPetSyncPlan plan = FavoredClassPetSync.Plan(tracked, desired,",
                "return pet != null && FavoredClassPets.IsQualified(pet, PetClassGuid) ? pet : null;",
                "ReferenceEquals(unit, m_ProjectedPet.Value))",
                "master.Descriptor.Progression.Features.HasFact(Fact.Blueprint);",
                "internal static void SyncMaster(UnitEntityData master)" })
                Assertions.True(projection.Contains(token), "Projection lacks: " + token);
            string hook = File.ReadAllText(Path.Combine(root, "Hooks", "FavoredClassPetUnlinkPatch.cs"));
            Assertions.True(hook.Contains("[HarmonyPatch(typeof(UnitDescriptor), \"SetMaster\")]") &&
                hook.Contains("out UnitEntityData __state") &&
                hook.Contains("FavoredClassPetArmorProjection.SyncMaster(__state);") &&
                !hook.Contains("static UnitEntityData"),
                "The unlink hook carries the previous master in its own call state only.");
        }
    }
}
