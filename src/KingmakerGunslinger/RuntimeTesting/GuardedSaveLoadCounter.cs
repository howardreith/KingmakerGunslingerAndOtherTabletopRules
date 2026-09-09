using System;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // A request-local protocol for the single native LoadRoutine header update.
    // This does not authorize a disk write or a gameplay SaveRoutine.
    internal sealed class GuardedSaveLoadCounter
    {
        private readonly JObject _expected;
        private int _stage;
        internal GuardedSaveLoadCounter(string nativeHeader)
        {
            _expected = JObject.Parse(nativeHeader);
            JToken value = _expected["LoadedTimes"];
            if (value == null || value.Type != JTokenType.Integer ||
                value.Value<long>() < 0 || value.Value<long>() >= int.MaxValue)
                throw new InvalidOperationException("Unproven native save load counter.");
            _expected["LoadedTimes"] = value.Value<int>() + 1;
        }
        internal bool Complete { get { return _stage == 2; } }
        internal void SuppressHeader(string name, string json)
        {
            if (_stage != 0 || name != "header" ||
                !JToken.DeepEquals(_expected, JObject.Parse(json)))
                throw new InvalidOperationException("Guarded load rejected an unexpected save mutation.");
            _stage = 1;
        }
        internal void SuppressCommit()
        {
            if (_stage != 1)
                throw new InvalidOperationException("Guarded load rejected an uncorrelated or duplicate commit.");
            _stage = 2;
        }
    }
}
