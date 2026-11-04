using System.Collections.Generic;
using System.Linq;
using TIA_Generator.Model;

namespace TIA_Generator.Generate
{
    /// <summary>Potrebne spremembe PLC za standard 2.0 (časovniki → Delay[0..9]) – skupno za CLI in GUI.</summary>
    public static class PlcChanges
    {
        public static IEnumerable<Unit> UnitsNeedingChange(MachineModelFile m) =>
            m.Units.Where(u => u.HmiTags?.Timers?.RequiresPlcChange == true);

        /// <summary>Vrstice CSV (ločilo ';', prva vrstica = glava) za Excel.</summary>
        public static List<string> CsvLines(MachineModelFile m)
        {
            var rows = new List<string> { "Enota;DB;Stari član;Novi element;Prikaz na HMI;Uporabljen v (blok, networki)" };
            foreach (var u in UnitsNeedingChange(m))
                foreach (var t in u.Timers)
                    rows.Add(string.Join(";", u.Id, u.SettingsDb?.Name, t.Member, t.ArrayElement, t.Show ? "da" : "ne",
                                         t.UsedIn == null ? "" : string.Join(" | ", t.UsedIn)));
            return rows;
        }
    }
}
