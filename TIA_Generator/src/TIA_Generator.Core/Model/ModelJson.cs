using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TIA_Generator.Model
{
    /// <summary>Branje/pisanje <c>*.hmimodel.json</c> (camelCase, zamiki, brez null polj, UTF-8 brez BOM).</summary>
    public static class ModelJson
    {
        static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver { NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = false } },
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static string Serialize(MachineModelFile m) => JsonConvert.SerializeObject(m, Settings);
        public static MachineModelFile Deserialize(string json) => JsonConvert.DeserializeObject<MachineModelFile>(json, Settings);

        public static void Save(MachineModelFile m, string path) => File.WriteAllText(path, Serialize(m), new UTF8Encoding(false));
        public static MachineModelFile Load(string path) => Deserialize(File.ReadAllText(path, Encoding.UTF8));
    }
}
