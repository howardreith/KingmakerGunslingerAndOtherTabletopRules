using System.Collections.Generic;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Item-owned token storage boundary. Runtime implementations must make replacement
    /// transactional: failure must preserve the previously observable token set.
    /// </summary>
    internal interface IFirearmStateTokenStore
    {
        IReadOnlyList<string> ReadTokenIds(object itemInstance);

        void ReplaceToken(
            object itemInstance,
            string expectedCurrentTokenId,
            string targetTokenId);

        bool ClearTokens(object itemInstance);
    }
}
