using BassClefStudio.ScratchCompiler.Compilers.Microcode;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;
using Newtonsoft.Json.Linq;

namespace BassClefStudio.ScratchCompiler.Compilers.Commands
{
    /// <summary>
    /// The <see cref="ICompiler"/> for .ccs assembly language.
    /// </summary>
    public class CommandCompiler : ICompiler
    {
        /// <summary>
        /// A collection of reserved names for compiler commands.
        /// </summary>
        readonly IEnumerable<string> ReservedCommandNames;

        readonly Parser<char, char> LiteralMarker;
        readonly Parser<char, char> CharMarker;
        readonly Parser<char, char> EnumStart;
        readonly Parser<char, char> EnumEnd;
        readonly Parser<char, char> AddressMarker;
        readonly Parser<char, char> DirectiveMarker;
        readonly Parser<char, char> ReferenceMarker;
        readonly Parser<char, char> AnyCommandMarker;
        readonly Parser<char, string> CommentHead;
        readonly Parser<char, char> ExceptEndLine;

        readonly Parser<char, ValueToken> NumLiteral;
        readonly Parser<char, ValueToken> CharLiteral;
        readonly Parser<char, ValueToken> BoolLiteral;
        readonly Parser<char, ValueToken> EnumLiteral;
        readonly Parser<char, ValueToken> NullLiteral;
        readonly Parser<char, ValueToken> Address;
        readonly Parser<char, ValueToken> Directive;
        readonly Parser<char, ValueToken> Reference;

        readonly Parser<char, ValueToken> Value;
        readonly Parser<char, CommandCall> CallCommand;
        readonly Parser<char, CommandCall> CompilerCommand;
        readonly Parser<char, CommandCall> Command;
        readonly Parser<char, IEnumerable<CommandCall?>> Code;

        readonly Parser<char, string> TrueString;
        readonly Parser<char, string> FalseString;
        readonly Parser<char, string> NullString;

        /// <inheritdoc/>
        public CompileType CompileType { get; } = CompileType.Commands;

        /// <summary>
        /// A collection of known <see cref="string"/> command names.
        /// </summary>
        private IEnumerable<string> KnownCommandNames { get; set; }

        /// <summary>
        /// Creates and initializes a new <see cref="CommandCompiler"/>.
        /// </summary>
        public CommandCompiler()
        {
            CommentHead = String("//");
            LiteralMarker = Char('#');
            CharMarker = Char('\'');
            AddressMarker = Char('$');
            DirectiveMarker = Char('.');
            ReferenceMarker = Char('?');
            AnyCommandMarker = OneOf(LiteralMarker, AddressMarker, ReferenceMarker);
            EnumStart = Char('{');
            EnumEnd = Char('}');
            ExceptEndLine = AnyCharExcept('\r', '\n');

            TrueString = String("true");
            FalseString = String("false");
            NullString = String("null");

            ReservedCommandNames = new string[]
            {
                "Define",
                "Var",
                "Constant"
            };

            NumLiteral =
                from l in CurrentPos
                from n in LiteralMarker.Then(Digit.Or(Char('.')).Or(Char('-')).ManyString())
                select new ValueToken() { Value = n, Type = ValueType.Immediate, Position = l };
            CharLiteral =
                from l in CurrentPos
                from c in Any.Between(CharMarker)
                select new ValueToken() { Value = c.ToString(), Type = ValueType.Immediate, Position = l };
            BoolLiteral = 
                from l in CurrentPos
                from b in OneOf(Try(TrueString), FalseString)
                select new ValueToken() { Value = b, Type = ValueType.Immediate, Position = l };
            EnumLiteral =
                from l in CurrentPos
                from e in LetterOrDigit.ManyString().Between(EnumStart, EnumEnd)
                select new ValueToken() { Value = e, Type = ValueType.Immediate, Position = l };
            NullLiteral =
                from l in CurrentPos
                from n in NullString
                select new ValueToken() { Value = string.Empty, Type = ValueType.Immediate, Position = l };
            Address = 
                from l in CurrentPos
                from a in AddressMarker
                from n in Num
                select new ValueToken() { Value = n.ToString(), Type = ValueType.Address, Position = l };
            Directive = 
                from l in CurrentPos
                from d in DirectiveMarker
                from n in LetterOrDigit.Or(DirectiveMarker).ManyString()
                select new ValueToken() { Value = n, Type = ValueType.Directive, Position = l };
            Reference =
                from l in CurrentPos
                from r in ReferenceMarker
                from n in LetterOrDigit.Or(DirectiveMarker).ManyString()
                select new ValueToken() { Value = n, Type = ValueType.Reference, Position = l };

            Value = OneOf(
                NumLiteral.Labelled("number"),
                CharLiteral.Labelled("char"),
                EnumLiteral.Labelled("enum"),
                Try(BoolLiteral).Labelled("bool"),
                Try(NullLiteral).Labelled("null"),
                Address.Labelled("address"),
                Directive.Labelled("directive"),
                Reference.Labelled("reference"));

            CompilerCommand =
                from l in CurrentPos
                from c in LetterOrDigit.AtLeastOnceString().Where(n => ReservedCommandNames.Contains(n)).Labelled("reserved command")
                from v in Try(SkipWhitespaces.Then(Value)).Many()
                select new CommandCall() 
                { 
                    Name = c, 
                    Inputs = v.ToArray(), 
                    Type = CallType.Compiler,
                    Position = l
                };

            CallCommand =
                from l in CurrentPos
                from c in LetterOrDigit.Or(AnyCommandMarker).AtLeastOnceString().Where(n => KnownCommandNames.Contains(n)).Labelled("known command")
                from v in Try(SkipWhitespaces.Then(Value)).Many()
                select new CommandCall() 
                { 
                    Name = c,
                    Inputs = v.ToArray(),
                    Type = CallType.Command,
                    Position = l
                };

            Command = OneOf(Try(CompilerCommand).Labelled("compiler call"), CallCommand.Labelled("command call"));
            Code = OneOf(
                    Try(CommentHead.Then(ExceptEndLine.SkipMany())).ThenReturn<CommandCall?>(null).Labelled("comment"), 
                    Command.Select<CommandCall?>(c => c))
                .SeparatedAndOptionallyTerminatedAtLeastOnce(SkipWhitespaces);
        }

        /// <inheritdoc/>
        public async Task CompileAsync(FileInfo inputFile, DirectoryInfo outputDirectory)
            => await CompileAsync(inputFile, Array.Empty<MicrocodeDoc>(), outputDirectory);

        /// <inheritdoc/>
        public async Task CompileAsync(FileInfo inputFile, FileInfo[] documentationFiles, DirectoryInfo outputDirectory)
        {
            List<MicrocodeDoc> documentation = new List<MicrocodeDoc>();
            foreach(var docFile in documentationFiles)
            {
                string docText = await File.ReadAllTextAsync(docFile.FullName);
                documentation.AddRange(JsonConvert.DeserializeObject<MicrocodeDoc[]>(docText));
            }
            await CompileAsync(inputFile, documentation, outputDirectory);
        }

        /// <summary>
        /// Internal - compiles commands and assembly language, using the provided <see cref="MicrocodeDoc"/>s to understand the command names and provide syntactical helper functions.
        /// </summary>
        /// <param name="inputFile">A human-readable <see cref="FileInfo"/> file containing the source code.</param>
        /// <param name="documentation">The <see cref="MicrocodeDoc"/>s for all microcode that this <see cref="CommandCompiler"/> understands.</param>
        /// <param name="outputDirectory">The <see cref="DirectoryInfo"/> in which to save output files.</param>
        private async Task CompileAsync(FileInfo inputFile, IEnumerable<MicrocodeDoc> documentation, DirectoryInfo outputDirectory)
        {
            string inputCode = await File.ReadAllTextAsync(inputFile.FullName);
            KnownCommandNames = documentation.SelectMany(d => new string[] { d.CommandName, d.GetFullName() }).Distinct();
            var result = Code.Parse(inputCode);
            if (result.Success)
            {
                //// Remove empty (null) commands (these are created by comments).
                CommandCall[] initialCommands = result.Value.Where(c => c.HasValue).Select(c => c.Value).ToArray();
                Dictionary<string, int> directivePositions = new Dictionary<string, int>();

                int currentMemoryPosition = 0;
                MemoryMap memoryMap = new MemoryMap();
                foreach (var currentCommand in initialCommands)
                {
                    if (currentCommand.Type == CallType.Command)
                    {
                        ValueType[] reqValTypes = currentCommand.Inputs.Select(i => i.Type == ValueType.Directive ? ValueType.Address : i.Type).ToArray();
                        MicrocodeDoc? commandDoc = documentation.GetDoc(currentCommand.Name, reqValTypes);
                        if (commandDoc.HasValue)
                        {
                            memoryMap.AddMemory(currentMemoryPosition, currentCommand.Position, commandDoc.Value.GetFullName());
                            currentMemoryPosition++;
                            foreach (var i in currentCommand.Inputs)
                            {
                                memoryMap.AddMemory(currentMemoryPosition, i);
                                currentMemoryPosition++;
                            }
                        }
                        else
                        {
                            throw new CompilationException($"Could not find an override of {currentCommand.Name} that supports input type(s) [{reqValTypes.GetInputModes()}].", currentCommand.Position);
                        }
                    }
                    else if(currentCommand.Type == CallType.Compiler)
                    {
                        if(currentCommand.Name == "Define")
                        {
                            if (currentCommand.Inputs.Length == 2)
                            {
                                if (currentCommand.Inputs[0].Type == ValueType.Directive && currentCommand.Inputs[1].Type == ValueType.Address)
                                {
                                    directivePositions.Add(currentCommand.Inputs[0].Value, int.Parse(currentCommand.Inputs[1].Value));
                                }
                                else
                                {
                                    throw new CompilationException($"Cannot call Define with {currentCommand.Inputs[0]},{currentCommand.Inputs[2]} - expected directive and address.", currentCommand.Position);
                                }
                            }
                            else if(currentCommand.Inputs.Length == 1)
                            {
                                if (currentCommand.Inputs[0].Type == ValueType.Directive)
                                {
                                    directivePositions.Add(currentCommand.Inputs[0].Value, currentMemoryPosition);
                                }
                                else
                                {
                                    throw new CompilationException($"Cannot call Define with {currentCommand.Inputs[0]} - expected directive.", currentCommand.Position);
                                }
                            }
                            else
                            {
                                throw new CompilationException("\'Define\' compiler call requires 1 or 2 inputs.", currentCommand.Position);
                            }
                        }
                        else if (currentCommand.Name == "Var")
                        {
                            if (currentCommand.Inputs.Length == 1)
                            {
                                if (currentCommand.Inputs[0].Type == ValueType.Directive)
                                {
                                    directivePositions.Add(currentCommand.Inputs[0].Value, currentMemoryPosition);
                                    memoryMap.AddMemory(currentMemoryPosition, currentCommand.Position, string.Empty);
                                    currentMemoryPosition++;
                                }
                                else
                                {
                                    throw new CompilationException($"Cannot call Var with {currentCommand.Inputs[0]} - expected directive.", currentCommand.Position);
                                }
                            }
                            else
                            {
                                throw new CompilationException("\'Var\' compiler call requires a single input.", currentCommand.Position);
                            }
                        }
                        else if (currentCommand.Name == "Constant")
                        {
                            if (currentCommand.Inputs.Length == 1)
                            {
                                memoryMap.AddMemory(currentMemoryPosition, currentCommand.Inputs[0]);
                                currentMemoryPosition++;
                            }
                            else
                            {
                                throw new CompilationException("\'Constant\' compiler call requires a single input.", currentCommand.Position);
                            }
                        }
                    }
                }

                //// Go through and replace all value with their now-known literals or memory locations.
                int[] allLocations = memoryMap.Memory.Keys.ToArray();
                for (int i = 0; i < allLocations.Length; i++)
                {
                    int address = allLocations[i];
                    if(memoryMap.Memory[address] is ValueToken value)
                    {
                        if (value.Type == ValueType.Directive || value.Type == ValueType.Reference)
                        {
                            try
                            {
                                memoryMap.Memory[address] = directivePositions[value.Value];
                            }
                            catch(KeyNotFoundException)
                            {
                                throw new CompilationException($"Resolving directive {value} failed.", memoryMap.SourcePositions[address]);
                            }
                        }
                        else
                        {
                            memoryMap.Memory[address] = value.Value;
                        }
                    }
                }

                JToken output = memoryMap.GetJson();
                await File.WriteAllTextAsync(Path.Combine(outputDirectory.FullName, $"{Path.GetFileNameWithoutExtension(inputFile.Name)}.cco"), output.ToString(Formatting.None));
            }
            else
            {
                throw new CompilationException(result.Error.RenderErrorMessage(), result.Error.ErrorPos);
            }
        }
    }
}
