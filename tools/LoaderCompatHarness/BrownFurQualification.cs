using System.Collections.Generic;

namespace KmgLoaderCompatHarness
{
    internal sealed partial class BrownFurProbe
    {
        // Qualification modes are added with the A2 product change.
        private IEnumerable<object> Qualification()
        {
            problems.Add("unknown mode " + mode);
            yield break;
        }
    }
}
