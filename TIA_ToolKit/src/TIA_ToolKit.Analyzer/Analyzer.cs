using System;
using System.Collections.Generic;
using System.Linq;

namespace TIA_ToolKit.Analyzer
{
    public sealed class AnalysisResult
    {
        public ExportProject Project;
        public List<Finding> Findings = new List<Finding>();
    }

    /// <summary>Prebere export, požene vsa pravila in oštevilči ugotovitve (S1.., F1.., C1.., H1.., T1.., M1..).</summary>
    public static class ExportAnalyzer
    {
        public static AnalysisResult Analyze(string projectDir, Action<string> log = null, IList<IRule> rules = null)
        {
            log = log ?? (s => { });
            rules = rules ?? RuleSet.All();
            var project = ExportReader.Read(projectDir, log);
            var res = new AnalysisResult { Project = project };
            foreach (var plc in project.Plcs)
                foreach (var r in rules)
                {
                    try
                    {
                        var found = r.Check(project, plc).ToList();
                        if (found.Count > 0) log("  " + r.Id + ": " + found.Count);
                        res.Findings.AddRange(found);
                    }
                    catch (Exception ex) { log("  Pravilo " + r.Id + " je padlo: " + ex.Message); }
                }
            Number(res.Findings);
            return res;
        }

        static void Number(List<Finding> list)
        {
            var sorted = list
                .OrderBy(f => Array.IndexOf(Categories.Order, f.Category))
                .ThenBy(f => (int)f.Severity)
                .ThenBy(f => f.RuleId)
                .ThenBy(f => f.Locations.FirstOrDefault()?.Path)
                .ToList();
            var counters = new Dictionary<string, int>();
            foreach (var f in sorted)
            {
                string p = Categories.Prefix(f.Category);
                counters[p] = counters.TryGetValue(p, out var n) ? n + 1 : 1;
                f.Id = p + counters[p];
            }
            list.Clear();
            list.AddRange(sorted);
        }
    }
}
