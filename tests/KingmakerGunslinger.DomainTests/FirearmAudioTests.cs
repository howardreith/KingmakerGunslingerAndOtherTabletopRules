using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using KingmakerGunslinger.Audio;
using KingmakerGunslinger.Firearms;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FirearmAudioTests
    {
        private const string BankHash =
            "0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18";
        private const string ValidJson =
            "{\"schemaVersion\":1," +
            "\"bankName\":\"KMG_Firearms\"," +
            "\"bankFileName\":\"KMG_Firearms.bnk\"," +
            "\"platform\":\"Windows\"," +
            "\"wwiseVersion\":\"2016.2.6.6153\"," +
            "\"sha256\":\"" + BankHash + "\"," +
            "\"mediaEmbedded\":true," +
            "\"events\":{" +
            "\"Pistol\":\"KMG_Firearm_Pistol_Shot\"," +
            "\"Musket\":\"KMG_Firearm_Musket_Shot\"," +
            "\"Blunderbuss\":\"KMG_Firearm_Blunderbuss_Shot\"," +
            "\"Revolver\":\"KMG_Firearm_Revolver_Shot\"," +
            "\"Rifle\":\"KMG_Firearm_Rifle_Shot\"}}";

        internal static void CatalogExact()
        {
            Assertions.Equal("KMG_Firearm_Pistol_Shot", FirearmSoundEventCatalog.Resolve(FirearmKind.Pistol), "Pistol event mismatch.");
            Assertions.Equal("KMG_Firearm_Musket_Shot", FirearmSoundEventCatalog.Resolve(FirearmKind.Musket), "Musket event mismatch.");
            Assertions.Equal("KMG_Firearm_Blunderbuss_Shot", FirearmSoundEventCatalog.Resolve(FirearmKind.Blunderbuss), "Blunderbuss event mismatch.");
            Assertions.Equal("KMG_Firearm_Revolver_Shot", FirearmSoundEventCatalog.Resolve(FirearmKind.Revolver), "Revolver event mismatch.");
            Assertions.Equal("KMG_Firearm_Rifle_Shot", FirearmSoundEventCatalog.Resolve(FirearmKind.Rifle), "Rifle event mismatch.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => FirearmSoundEventCatalog.Resolve(FirearmKind.Unknown), "Unknown firearm audio must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => FirearmSoundEventCatalog.Resolve((FirearmKind)99), "Undefined firearm audio must fail closed.");
        }

        internal static void ProductionManifestParsing()
        {
            string path = ProductionManifestPath();
            FirearmSoundBankManifestDocument document =
                FirearmSoundBankManifestLoader.Read(path);
            FirearmSoundBankManifestLoader.Validate(document.Manifest);
            AssertCanonical(document.Manifest);
            Assertions.Equal(Path.GetFullPath(path), document.PathIdentity,
                "Production manifest path identity mismatch.");
            Assertions.Equal(610, document.ByteLength,
                "Production manifest byte length mismatch.");
            Assertions.Equal(
                "BF57981AD5EC2CBF3149ECAFC3EF737D87BC9035B14BCCC7D254DCA8F991C62E",
                document.ManifestSha256,
                "Production manifest SHA-256 mismatch.");
            Assertions.Equal("UTF-8", document.Encoding,
                "Production manifest encoding mismatch.");
            Assertions.Equal("None", document.Bom,
                "Production manifest BOM mismatch.");
            Assertions.Equal("1", document.RawSchemaToken,
                "Production raw schema token mismatch.");
            Assertions.Equal("Integer", document.SchemaTokenType,
                "Production schema token type mismatch.");
            AssertCanonical(FirearmSoundBankManifestLoader.Load(path));
        }

        internal static void CopiedManifestRepresentationParsing()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "kmg-audio-manifest-copy-" + Guid.NewGuid().ToString("N"));
            try
            {
                string directory = Path.Combine(
                    root,
                    "KingmakerGunslinger",
                    "assets",
                    "soundbanks");
                Directory.CreateDirectory(directory);
                string copy = Path.Combine(
                    directory,
                    "firearm-soundbank-manifest.json");
                File.Copy(ProductionManifestPath(), copy);

                FirearmSoundBankManifest original =
                    FirearmSoundBankManifestLoader.Load(ProductionManifestPath());
                FirearmSoundBankManifest packaged =
                    FirearmSoundBankManifestLoader.Load(copy);
                AssertCanonical(packaged);
                AssertEquivalent(original, packaged);
                Assertions.Equal(
                    FirearmSoundBankStager.Hash(ProductionManifestPath()),
                    FirearmSoundBankStager.Hash(copy),
                    "Copied package/live manifest bytes changed.");
            }
            finally
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }

        internal static void ProcessGlobalSerializerIsolation()
        {
            Func<JsonSerializerSettings> original = JsonConvert.DefaultSettings;
            try
            {
                JsonConvert.DefaultSettings = delegate
                {
                    return new JsonSerializerSettings
                    {
                        ContractResolver = new HostileContractResolver()
                    };
                };
                string json = File.ReadAllText(ProductionManifestPath());
                FirearmSoundBankManifest implicitValue =
                    JsonConvert.DeserializeObject<FirearmSoundBankManifest>(json);
                Assertions.Equal(0, implicitValue.SchemaVersion,
                    "Hostile defaults no longer reproduce the old implicit-binding failure.");
                Assertions.Equal(null, implicitValue.BankName,
                    "Hostile defaults unexpectedly bound the old implicit contract.");

                AssertCanonical(FirearmSoundBankManifestLoader.Load(
                    ProductionManifestPath()));
            }
            finally
            {
                JsonConvert.DefaultSettings = original;
            }
        }

        internal static void StrictManifestParsingFailures()
        {
            RejectSchema(ValidJson.Replace("\"schemaVersion\":1,", string.Empty),
                "<missing>", "Missing");
            RejectSchema(ValidJson.Replace("\"schemaVersion\":1", "\"schemaVersion\":0"),
                "0", "Integer");
            RejectSchema(ValidJson.Replace("\"schemaVersion\":1", "\"schemaVersion\":2"),
                "2", "Integer");
            RejectSchema(ValidJson.Replace("\"schemaVersion\":1", "\"schemaVersion\":\"1\""),
                "1", "String");
            RejectSchema(ValidJson.Replace("\"schemaVersion\":1", "\"schemaVersion\":null"),
                "<null>", "Null");
            RejectSchema(ValidJson.Replace(
                "\"schemaVersion\":1,",
                "\"schemaVersion\":1,\"schemaVersion\":1,"),
                "<duplicate>", "Duplicate");

            Reject("null", "manifest.json-parsing");
            Reject(null, "manifest.json-parsing");
            Reject("{", "manifest.json-parsing");
            Reject(ValidJson + "{}", "manifest.json-parsing");
            FirearmSoundBankManifestException nullManifest =
                Assertions.Throws<FirearmSoundBankManifestException>(
                    () => FirearmSoundBankManifestLoader.Validate(null),
                    "A null manifest object was accepted.");
            Assertions.Equal(
                "manifest.semantic-validation",
                nullManifest.StageCode,
                "Null manifest failure stage mismatch.");
            Reject(ValidJson.Replace(
                "\"bankName\":\"KMG_Firearms\",", string.Empty),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"bankFileName\":\"KMG_Firearms.bnk\",", string.Empty),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"platform\":\"Windows\",", string.Empty),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"wwiseVersion\":\"2016.2.6.6153\",", string.Empty),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"sha256\":\"" + BankHash + "\",", string.Empty),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"mediaEmbedded\":true,", string.Empty),
                "manifest.semantic-validation");
            int eventsOffset=ValidJson.IndexOf(",\"events\":{",
                StringComparison.Ordinal);
            Reject(ValidJson.Substring(0,eventsOffset)+"}",
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"bankName\":\"KMG_Firearms\"",
                "\"bankName\":null"),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"bankName\":\"KMG_Firearms\"",
                "\"bankName\":7"),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace("KMG_Firearms\"", "WrongBank\""),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "KMG_Firearm_Pistol_Shot",
                "KMG_Firearm_Musket_Shot"),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(BankHash, BankHash.ToLowerInvariant()),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(BankHash, "00"),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"mediaEmbedded\":true",
                "\"mediaEmbedded\":false"),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"mediaEmbedded\":true",
                "\"mediaEmbedded\":\"true\""),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"events\":{",
                "\"events\":["),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"Pistol\":\"KMG_Firearm_Pistol_Shot\",",
                string.Empty),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"events\":{",
                "\"events\":{\"Unknown\":\"KMG_Firearm_Unknown_Shot\","),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"Pistol\":\"KMG_Firearm_Pistol_Shot\",",
                "\"Pistol\":\"KMG_Firearm_Pistol_Shot\"," +
                "\"Pistol\":\"KMG_Firearm_Pistol_Shot\","),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"bankName\":\"KMG_Firearms\",",
                "\"bankName\":\"KMG_Firearms\",\"unknown\":true,"),
                "manifest.semantic-validation");
            Reject(ValidJson.Replace(
                "\"bankName\":\"KMG_Firearms\",",
                "\"bankName\":\"KMG_Firearms\",\"bankName\":\"KMG_Firearms\","),
                "manifest.semantic-validation");
        }

        internal static void RetryParserAfterActualFault()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "kmg-audio-retry-" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(root);
                string path = Path.Combine(
                    root,
                    "firearm-soundbank-manifest.json");
                File.WriteAllText(
                    path,
                    ValidJson.Replace(
                        "\"schemaVersion\":1",
                        "\"schemaVersion\":2"),
                    new UTF8Encoding(false));
                FirearmSoundBankManifestException fault =
                    Assertions.Throws<FirearmSoundBankManifestException>(
                        () => FirearmSoundBankManifestLoader.Load(path),
                        "An actual configuration fault was not observed.");
                Assertions.Equal("manifest.schema-extraction", fault.StageCode,
                    "Initial retry fixture fault stage mismatch.");

                File.Copy(ProductionManifestPath(), path, true);
                AssertCanonical(FirearmSoundBankManifestLoader.Load(path));

                string runtime = File.ReadAllText(Path.Combine(
                    RepositoryRoot(),
                    "src",
                    "KingmakerGunslinger",
                    "Audio",
                    "FirearmSoundRuntime.cs"));
                Assertions.True(
                    runtime.Contains("RetryConfigurationForDevelopment") &&
                    runtime.Contains("_machine=null;") &&
                    runtime.Contains("Configure(context);") &&
                    runtime.Contains("FirearmSoundBankManifestLoader.Read(manifestPath)"),
                    "Development retry no longer resets a fault and reruns the production parser.");
            }
            finally
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }

        internal static void PackageAndDeploymentContracts()
        {
            string root = RepositoryRoot();
            string package = File.ReadAllText(Path.Combine(
                root, "scripts", "validate-package.ps1"));
            string deployment = File.ReadAllText(Path.Combine(
                root, "scripts", "Deploy-Local.ps1"));
            string buildLocal = File.ReadAllText(Path.Combine(
                root, "scripts", "Build-Local.ps1"));
            string runtime = File.ReadAllText(Path.Combine(
                root, "src", "KingmakerGunslinger", "Audio",
                "FirearmSoundRuntime.cs"));

            Assertions.True(
                package.Contains("--validate-firearm-artifact") &&
                package.Contains("Source and packaged firearm manifests differ") &&
                package.Contains("Package contains forbidden Init.bnk") &&
                package.Contains("Package contains forbidden or unexpected Wwise artifacts"),
                "Package validation lacks production parsing, byte parity, or strict audio allowlisting.");
            Assertions.True(
                deployment.Contains("deployedFirearmManifestSha256") &&
                deployment.Contains("deployedFirearmSoundBankSha256") &&
                deployment.Contains("Packaged and deployed firearm audio files differ"),
                "Transactional deployment lacks exact firearm artifact parity.");
            Assertions.False(
                buildLocal.Contains(
                    "-Configuration Release -AllowMissingFirearmSoundBank"),
                "Production Build-Local still permits a missing firearm SoundBank.");
            Assertions.Equal(1, Count(runtime,
                "new FirearmSoundBankStager().Stage("),
                "Runtime configuration must stage the SoundBank exactly once.");
        }

        internal static void ValidateExternalArtifact(
            string manifestPath,
            string bankPath)
        {
            FirearmSoundBankManifest manifest =
                FirearmSoundBankManifestLoader.Load(manifestPath);
            AssertCanonical(manifest);
            Assertions.Equal(manifest.Sha256,
                FirearmSoundBankStager.Hash(bankPath),
                "External artifact bank hash does not match its parsed manifest.");
            FirearmSoundBankBinaryValidator.Validate(bankPath);
        }
        internal static void ManifestValidation()
        {
            FirearmSoundBankManifest valid=Manifest(HashBytes(new byte[]{1,2,3})); FirearmSoundBankManifestValidator.Validate(valid);
            Invalid(valid,m=>m.SchemaVersion=2); Invalid(valid,m=>m.BankName="Init"); Invalid(valid,m=>m.BankFileName="Init.bnk");
            Invalid(valid,m=>m.BankFileName="..\\KMG_Firearms.bnk"); Invalid(valid,m=>m.BankFileName="C:\\KMG_Firearms.bnk");
            Invalid(valid,m=>m.Platform="Mac"); Invalid(valid,m=>m.WwiseVersion="2024.1.0"); Invalid(valid,m=>m.Sha256="00");
            Invalid(valid,m=>m.Sha256=m.Sha256.ToLowerInvariant()); Invalid(valid,m=>m.MediaEmbedded=false);
            Invalid(valid,m=>m.Events.Remove("Pistol")); Invalid(valid,m=>m.Events["Pistol"]="wrong");
            Invalid(valid,m=>m.Events["Pistol"]=m.Events["Musket"]);
        }
        internal static void StagingLifecycle()
        {
            string root=Path.Combine(Path.GetTempPath(),"kmg-audio-"+Guid.NewGuid().ToString("N"));
            try
            {
                string mod=Path.Combine(root,"mod"), data=Path.Combine(root,"Kingmaker_Data"), banks=Path.Combine(mod,"assets","soundbanks"); Directory.CreateDirectory(banks);
                byte[] bytes=Bank(false); string source=Path.Combine(banks,FirearmSoundEventCatalog.BankFileName); File.WriteAllBytes(source,bytes);
                FirearmSoundBankManifest manifest=Manifest(FirearmSoundBankStager.Hash(source)); var stager=new FirearmSoundBankStager();
                FirearmSoundBankStageResult first=stager.Stage(mod,data,manifest); Assertions.Equal(FirearmSoundBankStageStatus.Copied,first.Status,"First stage must copy.");
                Assertions.True(first.HashParity,"First stage did not produce source/destination hash parity.");
                string destination=first.DestinationPath; Assertions.Equal(manifest.Sha256,FirearmSoundBankStager.Hash(destination),"Destination hash mismatch.");
                string sibling=Path.Combine(Path.GetDirectoryName(destination),"Init.bnk"); File.WriteAllBytes(sibling,new byte[]{4,4});
                FirearmSoundBankStageResult second=stager.Stage(mod,data,manifest); Assertions.Equal(FirearmSoundBankStageStatus.Skipped,second.Status,"Matching destination must skip.");
                Assertions.True(second.HashParity,"Skipped stage did not retain source/destination hash parity.");
                File.WriteAllBytes(destination,new byte[]{0}); FirearmSoundBankStageResult third=stager.Stage(mod,data,manifest); Assertions.Equal(FirearmSoundBankStageStatus.Copied,third.Status,"Mismatched exact destination must be replaced.");
                Assertions.True(third.HashParity,"Replacement stage did not restore source/destination hash parity.");
                Assertions.Equal(2,File.ReadAllBytes(sibling).Length,"Sibling Init bank was modified.");
                File.Delete(source);
                FirearmSoundBankStageException missing =
                    Assertions.Throws<FirearmSoundBankStageException>(
                        () => stager.Stage(mod,data,manifest),
                        "Missing source must reject.");
                Assertions.Equal("bank.validation",missing.StageCode,
                    "Missing source failure stage mismatch.");
            }
            finally { if(Directory.Exists(root)) Directory.Delete(root,true); }
        }
        internal static void BankBinaryValidation()
        {
            FirearmSoundBankBinaryValidator.Validate(Bank(false));
            Assertions.Throws<InvalidDataException>(()=>FirearmSoundBankBinaryValidator.Validate(Bank(true)),"Zero-length embedded media was accepted.");
            byte[] truncated=Bank(false); Array.Resize(ref truncated,truncated.Length-1);
            Assertions.Throws<InvalidDataException>(()=>FirearmSoundBankBinaryValidator.Validate(truncated),"Truncated SoundBank was accepted.");
            Assertions.Throws<ArgumentNullException>(()=>FirearmSoundBankBinaryValidator.Validate((byte[])null),"Null SoundBank was accepted.");
        }
        internal static void StateMachineLifecycle()
        {
            var engine=new FakeEngine(); var machine=new FirearmSoundStateMachine(engine); machine.MarkStaged();
            Assertions.False(machine.EnsureReady(),"Uninitialized Wwise must wait."); Assertions.Equal(0,machine.LoadAttempts,"Waiting must not load.");
            engine.Initialized=true; Assertions.True(machine.EnsureReady(),"Initialized Wwise must load."); Assertions.True(machine.EnsureReady(),"Ready state must be idempotent."); Assertions.Equal(1,engine.Loads,"Bank loaded more than once.");
            Assertions.Equal((uint)41,machine.TryPost("event",new object()),"Valid playing ID rejected."); Assertions.Equal(1,machine.AcceptedPosts,"Accepted count mismatch.");
            engine.PlayingId=0; Assertions.Equal((uint)0,machine.TryPost("event",new object()),"Invalid playing ID accepted."); Assertions.Equal(1,machine.AcceptedPosts,"Rejected post incremented acceptance.");
            Assertions.Equal("post-event.rejected",machine.LastFailureStage,
                "Zero playing ID failure stage mismatch.");

            var loadEngine=new FakeEngine { Initialized=true, ThrowOnLoad=true };
            var loadFault=new FirearmSoundStateMachine(loadEngine); loadFault.MarkStaged();
            Assertions.False(loadFault.EnsureReady(),"Bank load exception was accepted.");
            Assertions.Equal(FirearmSoundState.Faulted,loadFault.State,
                "Bank load exception did not fault readiness.");
            Assertions.Equal("bank.loading",loadFault.LastFailureStage,
                "Bank load failure stage mismatch.");
            Assertions.Equal(1,loadFault.LoadAttempts,
                "Bank load fault did not retain the one attempted load.");

            var configurationFault=new FirearmSoundStateMachine(
                new FakeEngine { Initialized=true });
            configurationFault.Fault(
                "manifest.schema-extraction",
                new InvalidDataException("fixture fault"));
            Assertions.False(configurationFault.EnsureReady(),
                "Configuration fault masqueraded as Ready.");
            Assertions.Equal(FirearmSoundState.Faulted,configurationFault.State,
                "Configuration fault state changed unexpectedly.");
        }
        private static string RepositoryRoot()
        {
            DirectoryInfo cursor = new DirectoryInfo(
                AppDomain.CurrentDomain.BaseDirectory);
            while (cursor != null)
            {
                if (File.Exists(Path.Combine(
                        cursor.FullName,
                        "KingmakerGunslinger.sln")) &&
                    File.Exists(Path.Combine(
                        cursor.FullName,
                        "assets",
                        "soundbanks",
                        "firearm-soundbank-manifest.json")))
                    return cursor.FullName;
                cursor = cursor.Parent;
            }
            throw new DirectoryNotFoundException(
                "Could not locate the Kingmaker Gunslinger repository root " +
                "from the test executable directory.");
        }
        private static string ProductionManifestPath()
        {
            return Path.Combine(
                RepositoryRoot(),
                "assets",
                "soundbanks",
                "firearm-soundbank-manifest.json");
        }
        private static void AssertCanonical(FirearmSoundBankManifest manifest)
        {
            Assertions.Equal(1,manifest.SchemaVersion,
                "Production manifest schema mismatch.");
            Assertions.Equal("KMG_Firearms",manifest.BankName,
                "Production manifest bank name mismatch.");
            Assertions.Equal("KMG_Firearms.bnk",manifest.BankFileName,
                "Production manifest bank filename mismatch.");
            Assertions.Equal("Windows",manifest.Platform,
                "Production manifest platform mismatch.");
            Assertions.Equal("2016.2.6.6153",manifest.WwiseVersion,
                "Production manifest Wwise version mismatch.");
            Assertions.Equal(BankHash,manifest.Sha256,
                "Production manifest bank SHA-256 mismatch.");
            Assertions.True(manifest.MediaEmbedded,
                "Production manifest must require embedded media.");
            Assertions.Equal(5,manifest.Events.Count,
                "Production manifest event count mismatch.");
            Assertions.Equal("KMG_Firearm_Pistol_Shot",manifest.Events["Pistol"],
                "Production Pistol event mismatch.");
            Assertions.Equal("KMG_Firearm_Musket_Shot",manifest.Events["Musket"],
                "Production Musket event mismatch.");
            Assertions.Equal("KMG_Firearm_Blunderbuss_Shot",manifest.Events["Blunderbuss"],
                "Production Blunderbuss event mismatch.");
            Assertions.Equal("KMG_Firearm_Revolver_Shot",manifest.Events["Revolver"],
                "Production Revolver event mismatch.");
            Assertions.Equal("KMG_Firearm_Rifle_Shot",manifest.Events["Rifle"],
                "Production Rifle event mismatch.");
        }
        private static void AssertEquivalent(
            FirearmSoundBankManifest expected,
            FirearmSoundBankManifest actual)
        {
            Assertions.Equal(expected.SchemaVersion,actual.SchemaVersion,
                "Copied manifest schema changed.");
            Assertions.Equal(expected.BankName,actual.BankName,
                "Copied manifest bank name changed.");
            Assertions.Equal(expected.BankFileName,actual.BankFileName,
                "Copied manifest bank filename changed.");
            Assertions.Equal(expected.Platform,actual.Platform,
                "Copied manifest platform changed.");
            Assertions.Equal(expected.WwiseVersion,actual.WwiseVersion,
                "Copied manifest Wwise version changed.");
            Assertions.Equal(expected.Sha256,actual.Sha256,
                "Copied manifest bank hash changed.");
            Assertions.Equal(expected.MediaEmbedded,actual.MediaEmbedded,
                "Copied manifest embedded-media policy changed.");
            Assertions.Equal(expected.Events.Count,actual.Events.Count,
                "Copied manifest event count changed.");
            foreach (KeyValuePair<string,string> item in expected.Events)
                Assertions.Equal(item.Value,actual.Events[item.Key],
                    "Copied manifest event mapping changed for " + item.Key + ".");
        }
        private static void RejectSchema(
            string json,
            string observed,
            string tokenType)
        {
            FirearmSoundBankManifestException exception =
                Assertions.Throws<FirearmSoundBankManifestException>(
                    () => FirearmSoundBankManifestLoader.Parse(json),
                    "Invalid schema representation was accepted.");
            Assertions.Equal("manifest.schema-extraction",exception.StageCode,
                "Schema rejection stage mismatch.");
            Assertions.True(exception.Message.Contains("expected schemaVersion=1"),
                "Schema rejection omitted the expected version.");
            Assertions.True(exception.Message.Contains("observed value="+observed),
                "Schema rejection omitted the observed value.");
            Assertions.True(exception.Message.Contains("tokenType="+tokenType),
                "Schema rejection omitted the observed token type.");
        }
        private static void Reject(string json,string stage)
        {
            FirearmSoundBankManifestException exception =
                Assertions.Throws<FirearmSoundBankManifestException>(
                    () => FirearmSoundBankManifestLoader.Parse(json),
                    "Invalid manifest representation was accepted.");
            Assertions.Equal(stage,exception.StageCode,
                "Manifest rejection stage mismatch.");
        }
        private static int Count(string value,string token)
        {
            int count=0;
            int offset=0;
            while ((offset=value.IndexOf(token,offset,StringComparison.Ordinal))>=0)
            {
                count++;
                offset+=token.Length;
            }
            return count;
        }
        private static FirearmSoundBankManifest Manifest(string hash) { return new FirearmSoundBankManifest { SchemaVersion=1,BankName=FirearmSoundEventCatalog.BankName,BankFileName=FirearmSoundEventCatalog.BankFileName,Platform="Windows",WwiseVersion="2016.2.6.6153",Sha256=hash,MediaEmbedded=true,Events=new Dictionary<string,string>{{"Pistol",FirearmSoundEventCatalog.Resolve(FirearmKind.Pistol)},{"Musket",FirearmSoundEventCatalog.Resolve(FirearmKind.Musket)},{"Blunderbuss",FirearmSoundEventCatalog.Resolve(FirearmKind.Blunderbuss)},{"Revolver",FirearmSoundEventCatalog.Resolve(FirearmKind.Revolver)},{"Rifle",FirearmSoundEventCatalog.Resolve(FirearmKind.Rifle)}}}; }
        private static void Invalid(FirearmSoundBankManifest original,Action<FirearmSoundBankManifest> mutate){FirearmSoundBankManifest copy=Manifest(original.Sha256);mutate(copy);Assertions.Throws<InvalidDataException>(()=>FirearmSoundBankManifestValidator.Validate(copy),"Invalid manifest accepted.");}
        private static string HashBytes(byte[] bytes){string path=Path.GetTempFileName();try{File.WriteAllBytes(path,bytes);return FirearmSoundBankStager.Hash(path);}finally{File.Delete(path);}}
        private static byte[] Bank(bool zeroMedia)
        {
            using(var stream=new MemoryStream()) using(var writer=new BinaryWriter(stream,Encoding.ASCII,true))
            {
                Chunk(writer,"BKHD",new byte[28]);
                using(var index=new MemoryStream()) using(var entries=new BinaryWriter(index,Encoding.ASCII,true))
                {
                    for(uint i=0;i<5;i++){entries.Write(i+1);entries.Write(i*10);entries.Write(zeroMedia&&i==2?0u:10u);}
                    Chunk(writer,"DIDX",index.ToArray());
                }
                Chunk(writer,"DATA",new byte[50]); Chunk(writer,"HIRC",new byte[]{1,0,0,0}); return stream.ToArray();
            }
        }
        private static void Chunk(BinaryWriter writer,string id,byte[] payload){writer.Write(Encoding.ASCII.GetBytes(id));writer.Write((uint)payload.Length);writer.Write(payload);}
        private sealed class HostileContractResolver:DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(
                MemberInfo member,
                MemberSerialization memberSerialization)
            {
                JsonProperty property=base.CreateProperty(
                    member,
                    memberSerialization);
                property.PropertyName="hostile_"+property.PropertyName;
                return property;
            }
        }
        private sealed class FakeEngine:IFirearmSoundEngine
        {
            internal bool Initialized;
            internal bool ThrowOnLoad;
            internal int Loads;
            internal uint PlayingId=41;
            public bool IsInitialized(){return Initialized;}
            public void LoadBank(string name)
            {
                Loads++;
                if(ThrowOnLoad)throw new InvalidOperationException("load failed");
            }
            public uint PostEvent(string name,object emitter){return PlayingId;}
        }
    }
}
