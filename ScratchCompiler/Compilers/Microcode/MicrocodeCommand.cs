using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.ScratchCompiler.Compilers.Microcode
{
    /// <summary>
    /// Represnts a machine-code command defined by .mcs microcode.
    /// </summary>
    public struct MicrocodeCommand
    {
        /// <summary>
        /// The documentation for this <see cref="MicrocodeCommand"/>, as a <see cref="MicrocodeDoc"/> value.
        /// </summary>
        public MicrocodeDoc Documentation { get; set; }

        /// <summary>
        /// A collection of all <see cref="MicrocodeCall"/>s this <see cref="MicrocodeCommand"/> makes.
        /// </summary>
        public IEnumerable<MicrocodeCall> Calls { get; set; }

        /// <summary>
        /// Gets the compiled JSON associated with this <see cref="MicrocodeCommand"/>.
        /// </summary>
        /// <returns>A <see cref="JObject"/> with 'Name' and 'Signals' properties.</returns>
        public JObject GetJson()
        {
            return new JObject(
                new JProperty("Name", Documentation.GetFullName()),
                new JProperty("Signals", string.Join("|", TrimCalls())));
        }

        /// <summary>
        /// Trims (using various defined optimizations) the <see cref="Calls"/> collection for this <see cref="MicrocodeCommand"/>.
        /// </summary>
        /// <returns>The trimmed, optimized <see cref="IEnumerable{T}"/> of <see cref="string"/> control signals.</returns>
        public IEnumerable<string> TrimCalls()
        {
            List<string> signals = new List<string>();
            string regF = null;
            string regT = null;
            IEnumerable<string> calls = Calls.SelectMany(c => c.CallNames);
            foreach (var call in calls)
            {
                if (string.IsNullOrEmpty(call))
                {
                    // Ignore empty control signals.
                }
                else
                {
                    // Register access optimization.
                    if (call.StartsWith("Rf"))
                    {
                        string reg = call[2..];
                        if (reg != regF)
                        {
                            regF = reg;
                            signals.Add(call);
                        }
                    }
                    else if (call.StartsWith("Rt"))
                    {
                        string reg = call[2..];
                        if (reg != regT)
                        {
                            regT = reg;
                            signals.Add(call);
                        }
                    }
                    else
                    {
                        signals.Add(call);
                    }
                }
            }

            return signals;
        }
    }
}
