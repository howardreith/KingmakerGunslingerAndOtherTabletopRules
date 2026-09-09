using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal static class TeleportationDiagnosticJson
    {
        internal static string Serialize(object value)
        {
            // Create, rather than CreateDefault/JsonConvert's default overload,
            // isolates these flattened snapshots from the game's save resolver.
            var serializer = JsonSerializer.Create(new JsonSerializerSettings {
                ContractResolver = new DefaultContractResolver(), PreserveReferencesHandling = PreserveReferencesHandling.None,
                TypeNameHandling = TypeNameHandling.None, Culture = CultureInfo.InvariantCulture
            });
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            { serializer.Serialize(writer, value); return writer.ToString(); }
        }
    }
}
