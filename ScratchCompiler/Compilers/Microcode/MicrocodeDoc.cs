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
        /// A <see cref="ValueType"/> indicating the input mode of this command, or 'null' if this command doesn't have/support input modes.
        /// </summary>
        public ValueType? InputMode { get; set; }

        /// <summary>
        /// A description of the command and what it does.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Flags of the <see cref="Registers"/> whose values this command changes.
        /// </summary>
        public Registers InvolvedRegisters { get; set; }

        /// <summary>
        /// Gets the full name (including input-mode) of the command defined by this <see cref="MicrocodeDoc"/>.
        /// </summary>
        public string GetFullName()
        {
            return $"{CommandName}{InputMode?.GetInputMode()}";
        }
    }

    /// <summary>
    /// Extension methods for the <see cref="MicrocodeDoc"/> struct.
    /// </summary>
    public static class DocExtensions
    {
        /// <summary>
        /// Gets the documentation for the desired command and <see cref="ValueType"/>.
        /// </summary>
        /// <param name="list">The list of available <see cref="MicrocodeDoc"/>s.</param>
        /// <param name="name">The <see cref="string"/> name of the command.</param>
        /// <param name="valueType">The <see cref="ValueType"/> of the command's input.</param>
        /// <returns>The <see cref="MicrocodeDoc"/> satisfying these conditions, or 'null'.</returns>
        public static MicrocodeDoc? GetDoc(this IEnumerable<MicrocodeDoc> list, string name, ValueType? valueType)
        {
            if (list.Any(l => l.CommandName == name && (!l.InputMode.HasValue || l.InputMode.Value == valueType)))
            {
                return list.First(l => l.CommandName == name && (!l.InputMode.HasValue || l.InputMode.Value == valueType));
            }
            else
            {
                return null;
            }
        }
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
