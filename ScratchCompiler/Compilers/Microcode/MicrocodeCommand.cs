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
            IEnumerable<string> calls = Calls.SelectMany(c => c.CallNames);
            foreach (var call in calls)
            {
                signals.Add(call);
            }

            return signals;
        }

        /// <inheritdoc/>
        public override string ToString() => $"{Documentation}:{{{string.Join("|", Calls)}}}";
    }
}
