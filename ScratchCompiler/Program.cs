using BassClefStudio.ScratchCompiler.Compilers;
using BassClefStudio.ScratchCompiler.Compilers.Commands;
using BassClefStudio.ScratchCompiler.Compilers.Microcode;
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
                new Option<DirectoryInfo>(
                    new string[] { "--input-dir", "-i" },
                    "The path to the source code files and any documentation files.")
                {
                    IsRequired = true
                }.ExistingOnly(),
                new Option<CompileType>(
                    new string[] { "--compile-type", "-t" },
                    () => CompileType.Auto,
                    "The type of compilation to apply to the source code."),
                new Option<DirectoryInfo>(
                    new string[] { "--output-dir", "-o" },
                    () => new DirectoryInfo($".{Path.DirectorySeparatorChar}"),
                    "The output directory to save compiled files to.")
                {
                    IsRequired = true
                }.ExistingOnly(),
            };
            rootCommand.TreatUnmatchedTokensAsErrors = true;

            rootCommand.Handler = CommandHandler.Create(
                (DirectoryInfo inputDir, CompileType compileType, DirectoryInfo outputDir) => Compile(inputDir, compileType, outputDir));

            return await rootCommand.InvokeAsync(args);
        }

        public static async Task Compile(DirectoryInfo inputDir, CompileType compileType, DirectoryInfo outputDir)
        {
            FileInfo[] microFiles = inputDir.GetFiles("*.mcs");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Found {microFiles.Length} .mcs source file(s).");

            int filesCompiled = 0;
            int sucesses = 0;

            if(microFiles.Length > 0 && (compileType == CompileType.Auto || compileType == CompileType.Microcode))
            {
                var microCompiler = Compilers.FirstOrDefault(c => c.CompileType == CompileType.Microcode);
                if (microCompiler != null)
                {
                    foreach (var micro in microFiles)
                    {
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine($"Compiling {micro.Name}...");
                            filesCompiled++;
                            await microCompiler.CompileAsync(micro, outputDir);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Compilation complete!");
                            sucesses++;
                        }
                        catch(Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Compilation of {micro.Name} failed: {ex.Message}");
                        }
                    }
                }
                else
                {
                    throw new CompilationException($"Could not find a required ICompiler for compilation type {compileType}.");
                }
            }

            FileInfo[] codeFiles = inputDir.GetFiles("*.ccs");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Found {codeFiles.Length} .ccs source file(s).");

            if (codeFiles.Length > 0 && (compileType == CompileType.Auto || compileType == CompileType.Commands))
            {
                FileInfo[] docFiles = 
                    inputDir.GetFiles("*.mcd")
                    .Concat(outputDir.GetFiles("*.mcd"))
                    .ToArray();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Found {docFiles.Length} .mcd documentation file(s).");

                var codeCompiler = Compilers.FirstOrDefault(c => c.CompileType == CompileType.Commands);
                if (codeCompiler != null)
                {
                    foreach (var code in codeFiles)
                    {
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine($"Compiling {code.Name}...");
                            filesCompiled++;
                            await codeCompiler.CompileAsync(code, docFiles, outputDir);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Compilation complete!");
                            sucesses++;
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Compilation of {code.Name} failed: {ex.Message}");
                        }
                    }
                }
                else
                {
                    throw new CompilationException($"Could not find a required ICompiler for compilation type {compileType}.");
                }
            }

            if (sucesses == filesCompiled)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if(sucesses == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            Console.WriteLine($"Compilation complete: {sucesses} out of {filesCompiled} succeeded.");
            Console.ResetColor();
        }
    }
}
