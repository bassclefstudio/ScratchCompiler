using Pidgin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.ScratchCompiler.Compilers.Commands
{
    /// <summary>
    /// Represents a tokenized input value to an assembly language command.
    /// </summary>
    public struct ValueToken
    {
        /// <summary>
        /// The <see cref="string"/> value this <see cref="ValueToken"/> holds.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The type of this <see cref="ValueToken"/>.
        /// </summary>
        public ValueType Type { get; set; }

        /// <summary>
        /// The <see cref="SourcePos"/> position of this input value in the source code.
        /// </summary>
        public SourcePos Position { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"{{{Type.GetInputMode()}{Value}}}";
    }
}
