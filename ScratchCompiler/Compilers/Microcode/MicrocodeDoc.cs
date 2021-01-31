using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.ScratchCompiler.Compilers.Microcode
{
    /// <summary>
    /// Represents the documentation for a single <see cref="MicrocodeCommand"/>.
    /// </summary>
    public struct MicrocodeDoc
    {
        /// <summary>
        /// The name of the command, without any suffixes.
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// A <see cref="char"/> indicating the input mode of this command, or 'null' if this command doesn't have/support input modes.
        /// </summary>
        public char? InputMode { get; set; }

        /// <summary>
        /// A description of the command and what it does.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Flags of the <see cref="Registers"/> whose values this command changes.
        /// </summary>
        public Registers InvolvedRegisters { get; set; }
    }


    /// <summary>
    /// An enum defining the different registers that the processor has - used for identifying which commands will (significantly) alter register values.
    /// </summary>
    [Flags]
    public enum Registers
    {
        /// <summary>
        /// No registers were involved.
        /// </summary>
        None = 0,
        /// <summary>
        /// The 'A' general-purpose register.
        /// </summary>
        A = 1 << 0,
        /// <summary>
        /// The 'B' general-purpose register.
        /// </summary>
        B = 1 << 1,
        /// <summary>
        /// The register designed for the current program address position. This <see cref="Registers"/> flag is only set if this register is not incremented as normal.
        /// </summary>
        Prog = 1 << 2,
        /// <summary>
        /// The 'X' temporary cache register.
        /// </summary>
        X = 1 << 3
    }
}
