using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TIA_ToolKit.Analyzer
{
    /// <summary>SAF-ACK-REQ: F-modul ACK_REQ neposredno na ACK_REI = samodejna reintegracija F-I/O brez potrditve operaterja.</summary>
    public sealed class FAckReqToReiRule : IRule
    {
        public string Id => "SAF-ACK-REQ";
        public string Title => "Samodejna reintegracija F-I/O (ACK_REQ → ACK_REI)";

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            foreach (var b in plc.Blocks.Where(x => x.IsFailsafe))
            {
                var hits = new List<string>(); var nws = new List<Network>();
                foreach (var nw in b.Networks.Where(n => n.Lad != null))
                    foreach (var coil in nw.Lad.Parts.Values.Where(p => p.IsCoil && p.Operand != null && p.Operand.EndsWith(".ACK_REI\"")))
                    {
                        var src = nw.Lad.SourceOf(coil.Uid);
                        if (src == null || src.Powerrail || !nw.Lad.Parts.TryGetValue(src.Uid, out var c)) continue;
                        if (c.Name == "Contact" && !c.Negated && c.Operand != null && c.Operand.EndsWith(".ACK_REQ\"")
                            && nw.Lad.SourceOf(c.Uid)?.Powerrail == true)
                        {
                            hits.Add(coil.Operand.Trim('"').Replace(".ACK_REI", ""));
                            if (!nws.Contains(nw)) nws.Add(nw);
                        }
                    }
                if (hits.Count == 0) continue;
                var f = new Finding
                {
                    RuleId = Id, Category = Categories.Safety, Severity = Severity.Critical,
                    Title = Title,
                    Description = "ACK_REQ je vezan neposredno na ACK_REI za " + hits.Count + " F-modul(ov) (" + string.Join(", ", hits.Take(4)) + (hits.Count > 4 ? ", …" : "") +
                                  "). Po pasivaciji (diskrepanca, prekinitev žice) se modul reintegrira sam, brez potrditve operaterja. Običajno je potreben ročni ACK_REI ali ACK_REI_GLOB.",
                };
                f.Locations.Add(FindingLocation.Of(b, nws.ToArray()));
                yield return f;
            }
        }
    }

    /// <summary>SAF-ACK-CLOCK: bit za potrditev (ime vsebuje ACK) se napaja iz taktnega bita Clock_x = samodejna potrditev F-komunikacije/F-napak.</summary>
    public sealed class ClockAckRule : IRule
    {
        public string Id => "SAF-ACK-CLOCK";
        public string Title => "Potrditev F-komunikacije iz taktnega bita (Clock)";
        static readonly Regex ClockRx = new Regex("^\"Clock_", RegexOptions.IgnoreCase);
        static readonly Regex AckRx = new Regex("ACK", RegexOptions.IgnoreCase);

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            foreach (var b in plc.Blocks)
                foreach (var nw in b.Networks)
                {
                    var s = nw.Stl;
                    for (int i = 0; i + 1 < s.Count; i++)
                    {
                        if (!(s[i].Op == "A" || s[i].Op == "O") || s[i].Operand == null || !ClockRx.IsMatch(s[i].Operand)) continue;
                        // takoj naslednji Assign (brez vmesnih logičnih pogojev)
                        int j = i + 1;
                        while (j < s.Count && (s[j].Op == "COMMENT" || s[j].Op == "EMPTY_LINE")) j++;
                        if (j < s.Count && s[j].Op == "Assign" && s[j].Operand != null && AckRx.IsMatch(s[j].Operand))
                        {
                            var f = new Finding
                            {
                                RuleId = Id, Category = Categories.Safety, Severity = Severity.Critical,
                                Title = Title,
                                Description = s[i] + " / = " + s[j].Operand + ": bit za potrditev se postavlja s taktom. Če je vezan na ACK_REI (RCVDP/F-modul) ali ACK_GLOB, se F-komunikacija po motnji samodejno reintegrira brez potrditve operaterja.",
                            };
                            f.Locations.Add(FindingLocation.Of(b, nw));
                            yield return f;
                        }
                    }
                }
        }
    }

    /// <summary>SAF-PWR-COIL: v F-programu je tuljava vezana neposredno na napajalno letev (vedno TRUE) - pogoj ni preverjen.</summary>
    public sealed class FPowerrailCoilRule : IRule
    {
        public string Id => "SAF-PWR-COIL";
        public string Title => "F-program: varnostni pogoj trdo TRUE (PWR → Coil)";

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            foreach (var b in plc.Blocks.Where(x => x.IsFailsafe))
                foreach (var nw in b.Networks.Where(n => n.Lad != null))
                {
                    var coils = nw.Lad.Parts.Values
                        .Where(p => p.IsCoil && p.Operand != null && nw.Lad.SourceOf(p.Uid)?.Powerrail == true)
                        .Select(p => p.Operand).ToList();
                    if (coils.Count == 0) continue;
                    var f = new Finding
                    {
                        RuleId = Id, Category = Categories.Safety, Severity = Severity.High, Confidence = "Preveriti",
                        Title = Title,
                        Description = "Tuljava " + string.Join(", ", coils) + " je vezana neposredno na PWR (brez pogoja), zato je vedno TRUE. " +
                                      "Če je to varnostni pogoj (npr. PREVIOUS/NEXT_MACHINE_OK, DOOR_CLOSED/LOCKED proti sosednjemu stroju), se ne preverja.",
                    };
                    f.Locations.Add(FindingLocation.Of(b, nw));
                    yield return f;
                }
        }
    }

    /// <summary>
    /// SAF-FORCED: ostanek zagona - "SET" in nato S/= signala, ki bi moral priti od zunaj (vhod, VDC/EMG/AIR OK, vrata, bypass).
    /// Primeri: "SET / S #VDC_OK", "// pobriši / = #EMG_OK", "// removee SET S "S_Door_Close_iSig"", "SET S "M_Bypass"".
    /// </summary>
    public sealed class ForcedSignalRule : IRule
    {
        public string Id => "SAF-FORCED";
        public string Title => "Signal prisiljen na TRUE (SET → S/=) – ostanek zagona";
        static readonly Regex SuspectRx = new Regex(@"(VDC|EMG|E_?STOP|AIR|Air_|Door|Lock|_iSig|_iSIG|_SW\b|Bypass|SAFETY|Safety_OK|ENABLED|Disable_)", RegexOptions.IgnoreCase);
        static readonly HashSet<string> RloUsers = new HashSet<string> { "A", "AN", "O", "ON", "X", "XN", "A_BRACK", "O_BRACK", "AN_BRACK", "ON_BRACK", "L", "CALL", "JC", "JCN", "JU", "BRACKET", "NOT", "CLR" };

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            foreach (var b in plc.Blocks.Where(x => !x.IsDb))
                foreach (var nw in b.Networks)
                {
                    var s = nw.Stl;
                    for (int i = 0; i < s.Count; i++)
                    {
                        if (s[i].Op != "SET") continue;
                        var forced = new List<string>();
                        for (int j = i + 1; j < s.Count && !RloUsers.Contains(s[j].Op); j++)
                            if ((s[j].Op == "S" || s[j].Op == "Assign") && s[j].Operand != null && SuspectRx.IsMatch(s[j].Operand))
                                forced.Add(s[j].Op.Replace("Assign", "=") + " " + s[j].Operand);
                        if (forced.Count == 0) continue;
                        var f = new Finding
                        {
                            RuleId = Id, Category = Categories.Safety, Severity = Severity.High, Confidence = "Preveriti",
                            Title = Title,
                            Description = "SET / " + string.Join(" / ", forced) + ": signal se brezpogojno postavi na TRUE. Tipičen ostanek zagona " +
                                          "(prisiljen VDC/EMG/AIR OK, vhod vrat, bypass), zaradi katerega nadzor ne deluje.",
                        };
                        f.Locations.Add(FindingLocation.Of(b, nw));
                        yield return f;
                    }
                }
        }
    }
}
