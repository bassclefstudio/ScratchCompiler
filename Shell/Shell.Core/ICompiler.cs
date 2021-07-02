using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.Shell.Core
{
    /// <summary>
    /// Represents a service that can compile Scratch Shell source code.
    /// </summary>
    public interface ICompiler
    {
        /// <summary>
        /// Gets the type of compilation this <see cref="ICompiler"/> supports.
        /// </summary>
        CompileType CompileType { get; }

        /// <summary>
        /// Compiles and produces code output for the given input file.
        /// </summary>
        /// <param name="inputFile">A human-readable <see cref="FileInfo"/> file containing the source code.</param>
        /// <param name="outputDirectory">The <see cref="DirectoryInfo"/> in which to save output files.</param>
        Task CompileAsync(FileInfo inputFile, DirectoryInfo outputDirectory);

        /// <summary>
        /// Compiles and produces code output for the given input file and documentation.
        /// </summary>
        /// <param name="inputFile">A human-readable <see cref="FileInfo"/> file containing the source code.</param>
        /// <param name="documentationFiles">A collection of .mcd/.ccd files found in the source directory, which can be used for advanced parsing.</param>
        /// <param name="outputDirectory">The <see cref="DirectoryInfo"/> in which to save output files.</param>
        Task CompileAsync(FileInfo inputFile, FileInfo[] documentationFiles, DirectoryInfo outputDirectory);
    }
}
