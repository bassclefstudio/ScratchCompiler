using Pidgin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.Shell.Core
{
    /// <summary>
    /// An <see cref="Exception"/> thrown when compiling code using an <see cref="ICompiler"/> fails.
    /// </summary>
    [Serializable]
    public class CompilationException : Exception
    {
        /// <summary>
        /// The position in the source code where the error occurred.
        /// </summary>
        public SourcePos Position { get; }

        /// <inheritdoc/>
        public CompilationException(string message) : base(message)
        { }

        /// <inheritdoc/>
        public CompilationException(string message, SourcePos position) : base(message)
        {
            Position = position;
        }

        /// <inheritdoc/>
        protected CompilationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
