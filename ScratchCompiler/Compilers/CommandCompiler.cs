using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.ScratchCompiler.Compilers
{
    /// <summary>
    /// The <see cref="ICompiler"/> for .ccs microcode.
    /// </summary>
    public class CommandCompiler : ICompiler
    {
        /// <inheritdoc/>
        public CompileType CompileType { get; } = CompileType.Microcode;

        /// <inheritdoc/>
        public async Task CompileAsync(FileInfo inputFile, DirectoryInfo outputDirectory)
        {
            throw new NotImplementedException();
        }
    }
}
