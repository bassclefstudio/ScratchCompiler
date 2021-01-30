using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.ScratchCompiler.Compilers
{
    /// <summary>
    /// An enum that defines the type of source code to treat any input files as.
    /// </summary>
    public enum CompileType
    {
        Auto = 0,
        Microcode = 1,
        Commands = 2
    }
}
