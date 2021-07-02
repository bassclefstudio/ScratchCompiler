using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.Shell.Core.Microcode
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
        /// A <see cref="ValueType"/> indicating the input modes of this command, or empty if this command doesn't have/support input modes.
        /// </summary>
        public ValueType[] InputModes { get; set; }

        /// <summary>
        /// A description of the command and what it does.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets the full name (including input-mode) of the command defined by this <see cref="MicrocodeDoc"/>.
        /// </summary>
        public string GetFullName()
        {
            return $"{CommandName}{InputModes.GetInputModes()}";
        }

        /// <inheritdoc/>
        public override string ToString() => GetFullName();
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
        /// <param name="name">The <see cref="string"/> name (or full name) of the command.</param>
        /// <param name="valueTypes">The <see cref="ValueType"/>s of the command's inputs, used to find overrides.</param>
        /// <returns>The <see cref="MicrocodeDoc"/> satisfying these conditions, or 'null'.</returns>
        public static MicrocodeDoc? GetDoc(this IEnumerable<MicrocodeDoc> list, string name, params ValueType[] valueTypes)
        {
            if (list.Any(l => l.CommandName == name && l.InputModes.SequenceEqual(valueTypes)))
            {
                return list.First(l => l.CommandName == name && l.InputModes.SequenceEqual(valueTypes));
            }
            else if (list.Any(l => l.GetFullName() == name))
            {
                return list.First(l => l.GetFullName() == name);
            }
            else
            {
                return null;
            }
        }
    }
}
