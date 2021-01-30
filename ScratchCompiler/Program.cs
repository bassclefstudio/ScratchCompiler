using BassClefStudio.ScratchCompiler.Compilers;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BassClefStudio.ScratchCompiler
{
    class Program
    {
        static ICompiler[] Compilers = new ICompiler[]
        {
            new MicrocodeCompiler(),
            new CommandCompiler()
        };

        static async Task<int> Main(string[] args)
        {
            RootCommand rootCommand = new RootCommand("Compiles Scratch shell source code into a machine-readable form.")
            {
                new Option<FileInfo>(
                    new string[] { "--input-file", "-i" },
                    "The path to the source code file.")
                {
                    IsRequired = true
                }.ExistingOnly(),
                new Option<CompileType>(
                    new string[] { "--compile-type", "-t" },
                    () => CompileType.Auto,
                    "The type of compilation to apply to the source code."),
                new Option<DirectoryInfo>(
                    new string[] { "--output-dir", "-o" },
                    () => new DirectoryInfo(".\\"),
                    "The output directory to save compiled files to.")
                {
                    IsRequired = true
                }.ExistingOnly(),
            };
            rootCommand.TreatUnmatchedTokensAsErrors = true;

            rootCommand.Handler = CommandHandler.Create(
                (FileInfo inputFile, CompileType compileType, DirectoryInfo outputDir) => Compile(inputFile, compileType, outputDir));

            return await rootCommand.InvokeAsync(args);
        }

        public static async Task Compile(FileInfo inputFile, CompileType compileType, DirectoryInfo outputDir)
        {
            if (compileType == CompileType.Auto)
            {
                if (inputFile.Extension == ".mcs")
                {
                    compileType = CompileType.Microcode;
                }
                else if (inputFile.Extension == ".ccs")
                {
                    compileType = CompileType.Commands;
                }
                else
                {
                    throw new ArgumentException($"Failed to determine compilation type for file of type {inputFile.Extension}. Try specifying with the --compile-type option.");
                }
            }

            var compiler = Compilers.FirstOrDefault(c => c.CompileType == compileType);
            if(compiler != null)
            {
                await compiler.CompileAsync(inputFile, outputDir);
            }
            else
            {
                throw new CompilationException($"Could not find the ICompiler for code type {compileType}.");
            }
        }
    }
}
