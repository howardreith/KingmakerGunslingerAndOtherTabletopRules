using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Audio
{
    internal sealed class FirearmSoundBankManifestException : IOException
    {
        internal FirearmSoundBankManifestException(
            string stageCode,
            string message,
            Exception innerException = null)
            : base(message, innerException)
        {
            StageCode = stageCode;
        }

        internal string StageCode { get; private set; }
    }

    internal sealed class FirearmSoundBankManifestDocument
    {
        internal FirearmSoundBankManifestDocument(
            FirearmSoundBankManifest manifest,
            string pathIdentity,
            int byteLength,
            string manifestSha256,
            string encoding,
            string bom,
            string rawSchemaToken,
            string schemaTokenType)
        {
            Manifest = manifest;
            PathIdentity = pathIdentity;
            ByteLength = byteLength;
            ManifestSha256 = manifestSha256;
            Encoding = encoding;
            Bom = bom;
            RawSchemaToken = rawSchemaToken;
            SchemaTokenType = schemaTokenType;
        }

        internal FirearmSoundBankManifest Manifest { get; private set; }
        internal string PathIdentity { get; private set; }
        internal int ByteLength { get; private set; }
        internal string ManifestSha256 { get; private set; }
        internal string Encoding { get; private set; }
        internal string Bom { get; private set; }
        internal string RawSchemaToken { get; private set; }
        internal string SchemaTokenType { get; private set; }
    }

    internal static class FirearmSoundBankManifestLoader
    {
        private const int SupportedSchemaVersion = 1;
        private const string InMemoryIdentity = "<memory>";

        internal static FirearmSoundBankManifest Load(string path)
        {
            FirearmSoundBankManifestDocument document = Read(path);
            Validate(document.Manifest);
            return document.Manifest;
        }

        internal static FirearmSoundBankManifestDocument Read(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Manifest path required.", "path");

            string identity;
            byte[] bytes;
            try
            {
                identity = Path.GetFullPath(path);
                bytes = File.ReadAllBytes(identity);
            }
            catch (Exception exception)
            {
                throw new FirearmSoundBankManifestException(
                    "manifest.read",
                    "Manifest read failed: " + exception.Message,
                    exception);
            }

            DecodedManifest decoded = Decode(bytes);
            return ParseDocument(
                decoded.Text,
                identity,
                bytes.Length,
                Hash(bytes),
                decoded.Encoding,
                decoded.Bom);
        }

        internal static FirearmSoundBankManifest Parse(string json)
        {
            if (json == null)
                throw JsonParsingFailure("manifest text is null.");

            byte[] bytes = new UTF8Encoding(false, true).GetBytes(json);
            FirearmSoundBankManifestDocument document = ParseDocument(
                json,
                InMemoryIdentity,
                bytes.Length,
                Hash(bytes),
                "UTF-8",
                "None");
            Validate(document.Manifest);
            return document.Manifest;
        }

        internal static void Validate(FirearmSoundBankManifest value)
        {
            try
            {
                FirearmSoundBankManifestValidator.Validate(value);
            }
            catch (FirearmSoundBankManifestException)
            {
                throw;
            }
            catch (InvalidDataException exception)
            {
                throw new FirearmSoundBankManifestException(
                    "manifest.semantic-validation",
                    exception.Message,
                    exception);
            }
        }

        private static FirearmSoundBankManifestDocument ParseDocument(
            string json,
            string pathIdentity,
            int byteLength,
            string manifestSha256,
            string encoding,
            string bom)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw JsonParsingFailure("manifest is empty.");

            try
            {
                using (var stringReader = new StringReader(json))
                using (var reader = new JsonTextReader(stringReader))
                {
                    if (!reader.Read() || reader.TokenType != JsonToken.StartObject)
                        throw JsonParsingFailure(
                            "root must be a non-null JSON object; observed " +
                            TokenName(reader.TokenType) + ".");

                    var manifest = new FirearmSoundBankManifest();
                    var fields = new HashSet<string>(StringComparer.Ordinal);
                    bool schemaFound = false;
                    string rawSchemaToken = "<missing>";
                    string schemaTokenType = "<missing>";
                    bool rootEnded = false;

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.EndObject)
                        {
                            rootEnded = true;
                            break;
                        }
                        if (reader.TokenType != JsonToken.PropertyName)
                            throw JsonParsingFailure(
                                "expected a manifest property; observed " +
                                TokenName(reader.TokenType) + ".");

                        string name = (string)reader.Value;
                        if (!fields.Add(name))
                        {
                            if (name == "schemaVersion")
                                throw SchemaFailure("<duplicate>", "Duplicate");
                            throw SemanticFailure(
                                "duplicate field '" + name + "'.");
                        }
                        if (!reader.Read())
                            throw JsonParsingFailure(
                                "field '" + name + "' has no value.");

                        switch (name)
                        {
                            case "schemaVersion":
                                schemaFound = true;
                                rawSchemaToken = TokenValue(reader);
                                schemaTokenType = TokenName(reader.TokenType);
                                if (reader.TokenType != JsonToken.Integer)
                                    throw SchemaFailure(
                                        rawSchemaToken,
                                        schemaTokenType);
                                try
                                {
                                    manifest.SchemaVersion = Convert.ToInt32(
                                        reader.Value,
                                        CultureInfo.InvariantCulture);
                                }
                                catch (Exception exception)
                                {
                                    throw new FirearmSoundBankManifestException(
                                        "manifest.schema-extraction",
                                        SchemaFailureMessage(
                                            rawSchemaToken,
                                            schemaTokenType),
                                        exception);
                                }
                                break;
                            case "bankName":
                                manifest.BankName = ReadString(reader, name);
                                break;
                            case "bankFileName":
                                manifest.BankFileName = ReadString(reader, name);
                                break;
                            case "platform":
                                manifest.Platform = ReadString(reader, name);
                                break;
                            case "wwiseVersion":
                                manifest.WwiseVersion = ReadString(reader, name);
                                break;
                            case "sha256":
                                manifest.Sha256 = ReadString(reader, name);
                                break;
                            case "mediaEmbedded":
                                if (reader.TokenType != JsonToken.Boolean)
                                    throw WrongToken(name, "Boolean", reader);
                                manifest.MediaEmbedded = (bool)reader.Value;
                                break;
                            case "events":
                                manifest.Events = ReadEvents(reader);
                                break;
                            default:
                                throw SemanticFailure(
                                    "unknown field '" + name + "'.");
                        }
                    }

                    if (!rootEnded)
                        throw JsonParsingFailure(
                            "manifest object is not terminated.");
                    if (reader.Read())
                        throw JsonParsingFailure(
                            "trailing content is not permitted; observed " +
                            TokenName(reader.TokenType) + ".");
                    if (!schemaFound)
                        throw SchemaFailure("<missing>", "Missing");
                    if (manifest.SchemaVersion != SupportedSchemaVersion)
                        throw SchemaFailure(
                            manifest.SchemaVersion.ToString(
                                CultureInfo.InvariantCulture),
                            schemaTokenType);

                    return new FirearmSoundBankManifestDocument(
                        manifest,
                        pathIdentity,
                        byteLength,
                        manifestSha256,
                        encoding,
                        bom,
                        rawSchemaToken,
                        schemaTokenType);
                }
            }
            catch (FirearmSoundBankManifestException)
            {
                throw;
            }
            catch (JsonException exception)
            {
                throw new FirearmSoundBankManifestException(
                    "manifest.json-parsing",
                    "Manifest JSON parsing failed: " + exception.Message,
                    exception);
            }
        }

        private static IDictionary<string, string> ReadEvents(
            JsonTextReader reader)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw WrongToken("events", "Object", reader);

            var events = new Dictionary<string, string>(StringComparer.Ordinal);
            bool ended = false;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                {
                    ended = true;
                    break;
                }
                if (reader.TokenType != JsonToken.PropertyName)
                    throw JsonParsingFailure(
                        "expected an event property; observed " +
                        TokenName(reader.TokenType) + ".");

                string kind = (string)reader.Value;
                if (events.ContainsKey(kind))
                    throw SemanticFailure(
                        "duplicate event field '" + kind + "'.");
                if (!reader.Read())
                    throw JsonParsingFailure(
                        "event field '" + kind + "' has no value.");
                events.Add(kind, ReadString(reader, "events." + kind));
            }
            if (!ended)
                throw JsonParsingFailure("events object is not terminated.");
            return events;
        }

        private static string ReadString(JsonTextReader reader, string name)
        {
            if (reader.TokenType != JsonToken.String)
                throw WrongToken(name, "String", reader);
            return (string)reader.Value;
        }

        private static FirearmSoundBankManifestException WrongToken(
            string name,
            string expected,
            JsonTextReader reader)
        {
            return SemanticFailure(
                "field '" + name + "' expected token type " + expected +
                "; observed " + TokenName(reader.TokenType) + ".");
        }

        private static FirearmSoundBankManifestException JsonParsingFailure(
            string detail)
        {
            return new FirearmSoundBankManifestException(
                "manifest.json-parsing",
                "Manifest JSON parsing failed: " + detail);
        }

        private static FirearmSoundBankManifestException SchemaFailure(
            string observed,
            string tokenType)
        {
            return new FirearmSoundBankManifestException(
                "manifest.schema-extraction",
                SchemaFailureMessage(observed, tokenType));
        }

        private static string SchemaFailureMessage(
            string observed,
            string tokenType)
        {
            return "Manifest schema extraction failed: expected " +
                "schemaVersion=1 with token type Integer; observed value=" +
                observed + "; tokenType=" + tokenType + ".";
        }

        private static FirearmSoundBankManifestException SemanticFailure(
            string detail)
        {
            return new FirearmSoundBankManifestException(
                "manifest.semantic-validation",
                "Manifest semantic validation failed: " + detail);
        }

        private static string TokenName(JsonToken token)
        {
            return token == JsonToken.None ? "None" : token.ToString();
        }

        private static string TokenValue(JsonTextReader reader)
        {
            if (reader.TokenType == JsonToken.Null) return "<null>";
            if (reader.Value == null) return "<none>";
            return Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
        }

        private static DecodedManifest Decode(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            if (bytes.Length == 0)
                throw JsonParsingFailure("manifest is empty.");

            Encoding encoding;
            string name;
            string bom;
            int offset;
            if (HasPrefix(bytes, 0x00, 0x00, 0xfe, 0xff))
            {
                encoding = new UTF32Encoding(true, true, true);
                name = "UTF-32BE";
                bom = "UTF-32BE";
                offset = 4;
            }
            else if (HasPrefix(bytes, 0xff, 0xfe, 0x00, 0x00))
            {
                encoding = new UTF32Encoding(false, true, true);
                name = "UTF-32LE";
                bom = "UTF-32LE";
                offset = 4;
            }
            else if (HasPrefix(bytes, 0xef, 0xbb, 0xbf))
            {
                encoding = new UTF8Encoding(false, true);
                name = "UTF-8";
                bom = "UTF-8";
                offset = 3;
            }
            else if (HasPrefix(bytes, 0xfe, 0xff))
            {
                encoding = new UnicodeEncoding(true, true, true);
                name = "UTF-16BE";
                bom = "UTF-16BE";
                offset = 2;
            }
            else if (HasPrefix(bytes, 0xff, 0xfe))
            {
                encoding = new UnicodeEncoding(false, true, true);
                name = "UTF-16LE";
                bom = "UTF-16LE";
                offset = 2;
            }
            else
            {
                encoding = new UTF8Encoding(false, true);
                name = "UTF-8";
                bom = "None";
                offset = 0;
            }

            try
            {
                return new DecodedManifest(
                    encoding.GetString(bytes, offset, bytes.Length - offset),
                    name,
                    bom);
            }
            catch (DecoderFallbackException exception)
            {
                throw new FirearmSoundBankManifestException(
                    "manifest.json-parsing",
                    "Manifest JSON parsing failed: invalid " + name +
                    " encoding.",
                    exception);
            }
        }

        private static bool HasPrefix(byte[] bytes, params byte[] prefix)
        {
            if (bytes.Length < prefix.Length) return false;
            for (int index = 0; index < prefix.Length; index++)
                if (bytes[index] != prefix[index]) return false;
            return true;
        }

        private static string Hash(byte[] bytes)
        {
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes))
                    .Replace("-", string.Empty);
        }

        private struct DecodedManifest
        {
            internal DecodedManifest(string text, string encoding, string bom)
            {
                Text = text;
                Encoding = encoding;
                Bom = bom;
            }

            internal string Text;
            internal string Encoding;
            internal string Bom;
        }
    }
}
