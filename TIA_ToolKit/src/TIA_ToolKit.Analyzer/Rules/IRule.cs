using System.Collections.Generic;

namespace TIA_ToolKit.Analyzer
{
    /// <summary>
    /// Eno pravilo analize. Pravilo vrne ugotovitve za en PLC program projekta.
    /// Pravila so deterministična (brez LLM): kar najdejo, je ali potrjena napaka ali kandidat (Confidence = "Preveriti").
    /// Seznam pravil in primeri iz dosedanjih analiz: docs\ANALYZER_SPEC.md.
    /// </summary>
    public interface IRule
    {
        string Id { get; }
        string Title { get; }
        IEnumerable<Finding> Check(ExportProject project, PlcProgram plc);
    }

    public static class RuleSet
    {
        /// <summary>Vsa pravila, v vrstnem redu izvajanja.</summary>
        public static List<IRule> All() => new List<IRule>
        {
            new FAckReqToReiRule(),
            new ClockAckRule(),
            new FPowerrailCoilRule(),
            new ForcedSignalRule(),
            new DoubleAssignRule(),
            new DuplicateOperandRule(),
            new ContradictionRule(),
            new CommentedNetworkRule(),
            new HandshakeStubRule(),
            new UnitIndexRule(),
            new ErrorObRule(),
            new OeeQualityRule(),
            new TagOverlapRule(),
            new SystemByteRule(),
        };
    }
}
