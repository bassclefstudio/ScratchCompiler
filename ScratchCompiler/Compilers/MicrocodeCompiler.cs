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

namespace BassClefStudio.ScratchCompiler.Compilers
{
    /// <summary>
    /// The <see cref="ICompiler"/> for .mcs microcode.
    /// </summary>
    public class MicrocodeCompiler : ICompiler
    {
        readonly Parser<char, string> CommentHead;
        readonly Parser<char, char> ExceptEndLine;
        readonly Parser<char, string> Operator;

        readonly Parser<char, string> Declaration;
        readonly Parser<char, Unit> Comment;
        readonly Parser<char, string[]> Signal;
        readonly Parser<char, JToken> Command;
        readonly Parser<char, JToken> Microcode;

        /// <inheritdoc/>
        public CompileType CompileType { get; } = CompileType.Microcode;

        public MicrocodeCompiler()
        {
            CommentHead = String("//");
            Operator = OneOf(Char('>').ThenReturn("T"), Char('+').ThenReturn("Inc"), Char('?').ThenReturn("TEq"));
            ExceptEndLine = AnyCharExcept('\r', '\n');

            Comment = CommentHead.Then(ExceptEndLine.SkipMany());
            Declaration = LetterOrDigit.AtLeastOnceString().Before(Char(':'));
            
            Signal = OneOf(
                Try(Map((r1, t, r2) => new string[] { $"Rf{r1}", $"Rt{r2}", $"{t}Reg" },  Letter.AtLeastOnceString(), Operator, Letter.AtLeastOnceString())),
                Try(Map((r1, t) => new string[] { $"Rf{r1}", $"{t}Reg" }, Letter.AtLeastOnceString(), Operator)),
                Letter.AtLeastOnceString().Select(s => new string[] { s }));
            
            Command = Map(
                (n, d, cs) => new JObject(
                    new JProperty("Name", n),
                    new JProperty("Help", d),
                    new JProperty("Signals", string.Join("|", cs.SelectMany(c => c).Where(c => !string.IsNullOrEmpty(c))))) as JToken,
                Declaration.Between(SkipWhitespaces),
                ExceptEndLine.ManyString().Before(EndOfLine), 
                Signal.Or(Comment.ThenReturn(Array.Empty<string>())).SeparatedAndOptionallyTerminated(EndOfLine));
            Microcode = Command.Many().Select<JToken>(cs => new JArray(cs));
        }

        /// <inheritdoc/>
        public async Task CompileAsync(FileInfo inputFile, DirectoryInfo outputDirectory)
        {
            string inputCode = await File.ReadAllTextAsync(inputFile.FullName);
            var result = Microcode.Parse(inputCode);
            if(result.Success)
            {
                await File.WriteAllTextAsync(Path.Combine(outputDirectory.FullName, $"{Path.GetFileNameWithoutExtension(inputFile.Name)}.mco"), result.Value.ToString(Formatting.None));
            }
            else
            {
                throw new CompilationException(result.Error.RenderErrorMessage());
            }
        }
    }
}
