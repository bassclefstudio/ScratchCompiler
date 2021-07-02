using BassClefStudio.Shell.Core.Commands;
using BassClefStudio.Shell.Core.Microcode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.Shell.Core
{
    /// <summary>
    /// An enum representing the type of value a <see cref="ValueToken"/> holds or a or <see cref="MicrocodeDoc"/> input requests.
    /// </summary>
    public enum ValueType
    {
        /// <summary>
        /// A literal value.
        /// </summary>
        Immediate = 0,
        /// <summary>
        /// A numerical address in memory.
        /// </summary>
        Address = 1,
        /// <summary>
        /// An address that's been dynamically assigned a name.
        /// </summary>
        Directive = 2,
        /// <summary>
        /// A reference to a memory location that contains a dynamic address.
        /// </summary>
        Reference = 3
    }

    /// <summary>
    /// Provides extension methods for input-mode <see cref="char"/>s and the <see cref="ValueType"/> enum.
    /// </summary>
    public static class ValueTypeExtensions
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> of <see cref="ValueType"/>s and their associated input-modes.
        /// </summary>
        public static List<Tuple<char, ValueType>> ModeBindings { get; } = new List<Tuple<char, ValueType>>()
        {
            new Tuple<char, ValueType>('$', ValueType.Address),
            new Tuple<char, ValueType>('#', ValueType.Immediate),
            new Tuple<char, ValueType>('.', ValueType.Directive),
            new Tuple<char, ValueType>('?', ValueType.Reference)
        };

        /// <summary>
        /// Get the associated <see cref="ValueType"/> for the given <see cref="char"/> input-mode.
        /// </summary>
        public static ValueType GetValueType(this char inputMode) => ModeBindings.First(b => b.Item1 == inputMode).Item2;

        /// <summary>
        /// Get the associated <see cref="char"/> input-mode for the given <see cref="ValueType"/>.
        /// </summary>
        public static char GetInputMode(this ValueType valueType) => ModeBindings.First(b => b.Item2 == valueType).Item1;

        /// <summary>
        /// Get the associated <see cref="char"/> input-mode for the given <see cref="ValueType"/>.
        /// </summary>
        public static string GetInputModes(this ValueType[] valueTypes) => string.Concat(valueTypes.Select(v => v.GetInputMode()));
    }
}
