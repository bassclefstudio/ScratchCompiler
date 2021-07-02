using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.Shell.Core
{
    /// <summary>
    /// An enum that defines the type of source code to treat any input files as.
    /// </summary>
    public enum CompileType
    {
        /// <summary>
        /// Determine the <see cref="CompileType"/> from the source code file.
        /// </summary>
        Auto = 0,
        /// <summary>
        /// Compiles the combinational microcode that defines assembly language.
        /// </summary>
        Microcode = 1,
        /// <summary>
        /// Compiles programs that run in memory.
        /// </summary>
        Commands = 2
    }
}
