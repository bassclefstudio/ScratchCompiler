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
        readonly Parser<char, char> AddressMarker;
        readonly Parser<char, char> DirectiveMarker;
        readonly Parser<char, string> CommentHead;
        readonly Parser<char, char> ExceptEndLine;

        readonly Parser<char, ValueToken> NumLiteral;
        readonly Parser<char, ValueToken> CharLiteral;
        readonly Parser<char, ValueToken> BoolLiteral;
        readonly Parser<char, ValueToken> NullLiteral;
        readonly Parser<char, ValueToken> Address;
        readonly Parser<char, ValueToken> Directive;

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
            ExceptEndLine = AnyCharExcept('\r', '\n');

            TrueString = String("true");
            FalseString = String("false");
            NullString = String("null");

            ReservedCommandNames = new string[]
            {
                "Define",
                "Position",
                "Var",
                "Constant"
            };

            NumLiteral = LiteralMarker.Then(Digit.Or(Char('.')).Or(Char('-')).ManyString().Select(h => new ValueToken() { Value = h, Type = ValueType.Immediate }));
            CharLiteral = Any.Between(CharMarker).Select(c => new ValueToken() { Value = c.ToString(), Type = ValueType.Immediate });
            BoolLiteral = OneOf(Try(TrueString), FalseString).Select(t => new ValueToken() { Value = t, Type = ValueType.Immediate });
            NullLiteral = NullString.Select(t => new ValueToken() { Value = string.Empty, Type = ValueType.Immediate });
            Address = AddressMarker.Then(Num.Select(h => new ValueToken() { Value = h.ToString(), Type = ValueType.Address }));
            Directive = DirectiveMarker.Then(LetterOrDigit.ManyString().Select(s => new ValueToken() { Value = s, Type = ValueType.Directive }));

            Value = OneOf(
                NumLiteral.Labelled("number"),
                CharLiteral.Labelled("char"),
                Try(BoolLiteral).Labelled("bool"),
                Try(NullLiteral).Labelled("null"),
                Address.Labelled("address"),
                Directive.Labelled("directive"));

            CompilerCommand =
                from c in LetterOrDigit.AtLeastOnceString().Where(n => ReservedCommandNames.Contains(n)).Labelled("reserved command")
                from ws in SkipWhitespaces
                from v in Value.Optional()
                select new CommandCall() { Name = c, Input = v.Match<ValueToken?>(v => v, () => null), Type = CallType.Compiler };

            CallCommand =
                from c in LetterOrDigit.AtLeastOnceString().Where(n => KnownCommandNames.Contains(n)).Labelled("known command")
                from ws in SkipWhitespaces
                from v in Value.Optional()
                select new CommandCall() { Name = c, Input = v.Match<ValueToken?>(v => v, () => null), Type = CallType.Command };

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
            KnownCommandNames = documentation.Select(d => d.CommandName).Distinct();
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
                        MicrocodeDoc? commandDoc = null;
                        if (currentCommand.Input.HasValue)
                        {
                            ValueType reqValType = currentCommand.Input.Value.Type == ValueType.Directive ? ValueType.Address : currentCommand.Input.Value.Type;
                            commandDoc = documentation.GetDoc(currentCommand.Name, reqValType);
                            if (commandDoc.HasValue)
                            {
                                memoryMap.Memory.Add(currentMemoryPosition, commandDoc.Value.GetFullName());
                                currentMemoryPosition++;
                                memoryMap.Memory.Add(currentMemoryPosition, currentCommand.Input.Value);
                                currentMemoryPosition++;
                            }
                            else
                            {
                                throw new CompilationException($"Could not find an override of {currentCommand.Name} that supports input type {reqValType}.");
                            }
                        }
                        else
                        {
                            commandDoc = documentation.GetDoc(currentCommand.Name, null);
                            if (commandDoc.HasValue)
                            {
                                memoryMap.Memory.Add(currentMemoryPosition, commandDoc.Value.GetFullName());
                                currentMemoryPosition++;
                            }
                            else
                            {
                                throw new CompilationException($"Could not find an override of {currentCommand.Name} that supports no input.");
                            }
                        }
                    }
                    else
                    {
                        if(currentCommand.Name == "Define")
                        {
                            if (currentCommand.Input.HasValue)
                            {
                                if (currentCommand.Input?.Type == ValueType.Directive)
                                {
                                    directivePositions.Add(currentCommand.Input?.Value, currentMemoryPosition);
                                }
                                else
                                {
                                    throw new CompilationException($"Cannot call Define with {currentCommand.Input} - expected directive.");
                                }
                            }
                            else
                            {
                                throw new CompilationException("\'Define\' compiler call requires a directive input.");
                            }
                        }
                        else if (currentCommand.Name == "Var")
                        {
                            if (currentCommand.Input.HasValue)
                            {
                                if (currentCommand.Input?.Type == ValueType.Directive)
                                {
                                    directivePositions.Add(currentCommand.Input?.Value, currentMemoryPosition);
                                    memoryMap.Memory.Add(currentMemoryPosition, string.Empty);
                                    currentMemoryPosition++;
                                }
                                else
                                {
                                    throw new CompilationException($"Cannot call Var with {currentCommand.Input} - expected directive.");
                                }
                            }
                            else
                            {
                                throw new CompilationException("\'Var\' compiler call requires a directive input.");
                            }
                        }
                        else if(currentCommand.Name == "Position")
                        {
                            if (currentCommand.Input.HasValue)
                            {
                                if (currentCommand.Input?.Type == ValueType.Address)
                                {
                                    currentMemoryPosition = int.Parse(currentCommand.Input?.Value);
                                }
                                else
                                {
                                    throw new CompilationException($"Cannot call Define with {currentCommand.Input} - expected address.");
                                }
                            }
                            else
                            {
                                throw new CompilationException("\'Position\' compiler call requires an address input.");
                            }
                        }
                        else if (currentCommand.Name == "Constant")
                        {
                            if (currentCommand.Input.HasValue)
                            {
                                memoryMap.Memory.Add(currentMemoryPosition, currentCommand.Input.Value);
                                currentMemoryPosition++;
                            }
                            else
                            {
                                throw new CompilationException("\'Constant\' compiler call requires an input.");
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
                        if (value.Type == ValueType.Directive)
                        {
                            memoryMap.Memory[address] = directivePositions[value.Value];
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
                throw new CompilationException(result.Error.RenderErrorMessage());
            }
        }
    }
}
