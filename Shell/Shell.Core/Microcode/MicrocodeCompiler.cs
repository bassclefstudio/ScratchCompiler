using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace BassClefStudio.Shell.Core.Microcode
{
    /// <summary>
    /// The <see cref="ICompiler"/> for .mcs microcode.
    /// </summary>
    public class MicrocodeCompiler : ICompiler
    {
        readonly MicrocodeAlias[] Aliases;
        readonly char[] KnownInputModes;

        readonly Parser<char, string> CommentHead;
        readonly Parser<char, char> ExceptEndLine;
        readonly Parser<char, MicrocodeAlias> Operator;

        readonly Parser<char, MicrocodeDoc> Documentation;
        readonly Parser<char, Unit> Comment;
        readonly Parser<char, MicrocodeCall> SignalCall;
        readonly Parser<char, MicrocodeCommand> Command;
        readonly Parser<char, IEnumerable<MicrocodeCommand>> Microcode;

        /// <inheritdoc/>
        public CompileType CompileType { get; } = CompileType.Microcode;

        /// <summary>
        /// Creates and initializes a new <see cref="MicrocodeCompiler"/>.
        /// </summary>
        public MicrocodeCompiler()
        {
            Aliases = new MicrocodeAlias[]
            {
                
            };

            KnownInputModes = new char[] { '$', '#', '?' };

            CommentHead = String("//");
            Operator = OneOf(Aliases.Select(o => Try(String(o.Alias)).ThenReturn(o)));
            ExceptEndLine = AnyCharExcept('\r', '\n');

            Comment = CommentHead.Then(ExceptEndLine.SkipMany()).Labelled("comment");
            Documentation =
                from name in LetterOrDigit.AtLeastOnceString().Labelled("name")
                from inputs in OneOf(KnownInputModes).Labelled("input-mode").Many().Labelled("input-modes")
                from colon in Char(':').Before(SkipWhitespaces)
                from desc in ExceptEndLine.ManyString().Labelled("description")
                select new MicrocodeDoc() 
                {
                    CommandName = name, 
                    InputModes = inputs.Select(i => i.GetValueType()).ToArray(),
                    Description = desc 
                };

            SignalCall = OneOf(
                Operator
                    .Select(o => new MicrocodeCall(o.OperationName))
                    .Labelled("control alias"),
                Letter.AtLeastOnceString()
                    .Select(s => new MicrocodeCall(s))
                    .Labelled("control signal"));

            Command =
                from n in Documentation
                from lb in EndOfLine
                from cs in Char(' ').SkipMany().Then(SignalCall.Or(Comment.ThenReturn(new MicrocodeCall()))).SeparatedAndOptionallyTerminatedAtLeastOnce(EndOfLine)
                select new MicrocodeCommand() { Documentation = n, Calls = cs };
            
            Microcode =
                Comment.ThenReturn(new Maybe<MicrocodeCommand>()).Or(
                Command.Select(c => new Maybe<MicrocodeCommand>(c)))
                .SeparatedAtLeastOnce(SkipWhitespaces)
                .Select(c => c.Where(m => m.HasValue).Select(m => m.Value));
        }

        /// <inheritdoc/>
        public Task CompileAsync(FileInfo inputFile, FileInfo[] documentationFiles, DirectoryInfo outputDirectory) => CompileAsync(inputFile, outputDirectory);
        /// <inheritdoc/>
        public async Task CompileAsync(FileInfo inputFile, DirectoryInfo outputDirectory)
        {
            string inputCode = File.ReadAllText(inputFile.FullName);
            var result = Microcode.Parse(inputCode);
            if(result.Success)
            {
                JArray output = new JArray(result.Value.Select(cs => cs.GetJson()));
                File.WriteAllText(Path.Combine(outputDirectory.FullName, $"{Path.GetFileNameWithoutExtension(inputFile.Name)}.mco"), output.ToString(Formatting.None));
                string docs = JsonConvert.SerializeObject(result.Value.Select(cs => cs.Documentation), Formatting.Indented);
                File.WriteAllText(Path.Combine(outputDirectory.FullName, $"{Path.GetFileNameWithoutExtension(inputFile.Name)}.mcd"), docs);
            }
            else
            {
                throw new CompilationException(result.Error.RenderErrorMessage());
            }
        }
    }
}
