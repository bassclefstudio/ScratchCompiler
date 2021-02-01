using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.ScratchCompiler.Compilers.Microcode
{
    /// <summary>
    /// Represents a single signal or condensed set of control signals (i.e. from an <see cref="MicrocodeOperator"/>), as parsed from .mcs microcode.
    /// </summary>
    public struct MicrocodeCall
    {
        private IEnumerable<string> callNames;
        /// <summary>
        /// The <see cref="string"/> text of the calls, in .mcs format.
        /// </summary>
        public IEnumerable<string> CallNames 
        { 
            get => callNames ?? Array.Empty<string>();
            set => callNames = value;
        }

        /// <summary>
        /// Creates a new <see cref="MicrocodeCall"/> for a single control signal.
        /// </summary>
        /// <param name="call">The name of the signal.</param>
        public MicrocodeCall(string call)
        {
            callNames = new string[] { call };
        }

        /// <summary>
        /// Creates a new <see cref="MicrocodeCall"/> for a collection of control signals.
        /// </summary>
        /// <param name="calls">The names of the signals, as <see cref="string"/>s.</param>
        public MicrocodeCall(params string[] calls)
        {
            callNames = calls;
        }
    }

    /// <summary>
    /// Represents a character operator that, when found in .mcs signals, can be used to apply an action to one or two registers (requires the system-defined Rf* and Rt* control signals).
    /// </summary>
    public struct MicrocodeOperator
    {
        /// <summary>
        /// The operator character - i.e. '>' or '|', etc.
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// The name of the signal that the <see cref="MicrocodeOperator"/> will apply to the set 'from' and 'to' registers, ommiting the "Reg" at the end.
        /// </summary>
        public string OperationSignal { get; set; }
    }
}
