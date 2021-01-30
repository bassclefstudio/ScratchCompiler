using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.ScratchCompiler.Compilers
{
    /// <summary>
    /// An <see cref="Exception"/> thrown when compiling code using an <see cref="ICompiler"/> fails.
    /// </summary>
    [Serializable]
    public class CompilationException : Exception
    {
        /// <inheritdoc/>
        public CompilationException() { }
        /// <inheritdoc/>
        public CompilationException(string message) : base(message) { }
        /// <inheritdoc/>
        public CompilationException(string message, Exception inner) : base(message, inner) { }
        /// <inheritdoc/>
        protected CompilationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
