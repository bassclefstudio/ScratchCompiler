using Pidgin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.Shell.Core.Commands
{
    /// <summary>
    /// Represents a single call in machine code to a command or processor directive.
    /// </summary>
    public struct CommandCall
    {
        /// <summary>
        /// The user-friendly name used to represent the <see cref="CommandCall"/> in machine code.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The input value to the given <see cref="CommandCall"/>.
        /// </summary>
        public ValueToken[] Inputs { get; set; }

        /// <summary>
        /// Represents the position of the <see cref="CommandCall"/> in the source code.
        /// </summary>
        public SourcePos Position { get; set; }

        /// <summary>
        /// The <see cref="CallType"/> type of call this <see cref="CommandCall"/> is.
        /// </summary>
        public CallType Type { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"{Name}({string.Join(",", Inputs)})";
    }

    /// <summary>
    /// An enum defining the function of a <see cref="CommandCall"/>.
    /// </summary>
    public enum CallType
    {
        /// <summary>
        /// Calling a regular command, as defined in microcode.
        /// </summary>
        Command = 0,
        /// <summary>
        /// The compiler manages this special <see cref="CommandCall"/>, instead of the processor.
        /// </summary>
        Compiler = 1,
    }
}
