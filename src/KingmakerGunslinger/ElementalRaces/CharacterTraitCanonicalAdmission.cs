using System;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class CharacterTraitCanonicalAdmission
    {
        // A complete immutable identity snapshot is inspected before any
        // Unity object is allocated or any host selection is changed.
        internal static T[] Resolve<T>(Func<CharacterTraitNode,T> lookup,
            Func<CharacterTraitNode,T,bool> validate) where T : class
        {
            if(lookup==null || validate==null) throw new ArgumentNullException("canonicalAdmission");
            var nodes=ElementalCharacterTraitCatalog.Nodes();
            var existing=nodes.Select(lookup).ToArray();
            if(existing.All(v=>v==null)) return null;
            if(existing.Any(v=>v==null)) throw new InvalidOperationException("Partial stable character-trait graph.");
            for(int i=0;i<nodes.Length;i++)
                if(!validate(nodes[i],existing[i]))
                    throw new InvalidOperationException("Foreign or malformed canonical identity: "+nodes[i].Symbol);
            return existing;
        }
    }
}
