using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace TIA_HMIColorTool
{
    /// <summary>En par zamenjave, shranjen kot tekst "R, G, B" (za XmlSerializer).</summary>
    public class ColorPair
    {
        public string Old { get; set; }
        public string New { get; set; }
    }

    static class ColorMapping
    {
        /// <summary>Odobrena Hidria paleta.</summary>
        public static readonly RgbColor[] ApprovedPalette =
        {
            new RgbColor(29, 83, 131),
            new RgbColor(24, 106, 154),
            new RgbColor(0, 126, 172),
            new RgbColor(0, 147, 190),
            new RgbColor(0, 163, 194),
            new RgbColor(42, 180, 198),
            new RgbColor(116, 198, 199),
        };

        public static string DefaultFile
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mapping.xml"); }
        }

        public static List<ColorPair> DefaultMapping()
        {
            return new List<ColorPair> { new ColorPair { Old = "59, 101, 151", New = "29, 83, 131" } };
        }

        public static List<ColorPair> Load(string file)
        {
            if (!File.Exists(file)) return DefaultMapping();
            using (var s = File.OpenRead(file))
                return (List<ColorPair>)new XmlSerializer(typeof(List<ColorPair>)).Deserialize(s);
        }

        public static void Save(string file, List<ColorPair> pairs)
        {
            using (var s = File.Create(file))
                new XmlSerializer(typeof(List<ColorPair>)).Serialize(s, pairs);
        }

        /// <summary>
        /// Pretvori pare v slovar in preveri napake. Vrne seznam napak (prazen = OK).
        /// Opozorila (npr. verige A->B, B->C) gredo v <paramref name="warnings"/>.
        /// </summary>
        public static List<string> Validate(IEnumerable<ColorPair> pairs,
            out Dictionary<RgbColor, RgbColor> map, out List<string> warnings)
        {
            var errors = new List<string>();
            warnings = new List<string>();
            map = new Dictionary<RgbColor, RgbColor>();
            int row = 0;
            foreach (var p in pairs)
            {
                row++;
                RgbColor o, n;
                if (!RgbColor.TryParse(p.Old, out o)) { errors.Add("Vrstica " + row + ": neveljavna stara barva '" + p.Old + "'"); continue; }
                if (!RgbColor.TryParse(p.New, out n)) { errors.Add("Vrstica " + row + ": neveljavna nova barva '" + p.New + "'"); continue; }
                if (o.Equals(n)) { warnings.Add("Vrstica " + row + ": stara in nova barva sta enaki (" + o + ") - preskočeno."); continue; }
                if (map.ContainsKey(o)) { errors.Add("Vrstica " + row + ": stara barva " + o + " je že v mapiranju."); continue; }
                map[o] = n;
            }
            var m = map;
            foreach (var kv in m.Where(kv => m.ContainsKey(kv.Value)))
                warnings.Add("Veriga: " + kv.Key + " -> " + kv.Value + " in " + kv.Value + " -> " + m[kv.Value] +
                             ". Zamenjava je enkratna (brez kaskade): " + kv.Key + " postane " + kv.Value + ".");
            return errors;
        }
    }
}
