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
                new JProperty("Name", $"{Documentation.CommandName}{Documentation.InputMode}"),
                new JProperty("Signals", string.Join("|", Calls.SelectMany(c => c.CallNames).Where(c => !string.IsNullOrEmpty(c)))));
        }
    }
}
