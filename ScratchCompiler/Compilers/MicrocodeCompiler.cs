using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        readonly Parser<char, string> Declaration;
        readonly Parser<char, string> Documentation;
        readonly Parser<char, Unit> Comment;
        readonly Parser<char, string> Call;
        readonly Parser<char, string> Microcode;

        /// <inheritdoc/>
        public CompileType CompileType { get; } = CompileType.Microcode;

        public MicrocodeCompiler()
        {
            Comment = String("//").Then(AnyCharExcept('\r','\n').SkipMany());
            Declaration = LetterOrDigit.AtLeastOnceString().Before(Char(':'));
            Documentation = AnyCharExcept('\r', '\n').ManyString();
            Call = LetterOrDigit.Or(Char('>')).AtLeastOnceString();
            Microcode = Map((n, d, cs) => $"{n}:{d}:{string.Join("|", cs.Where(c => !string.IsNullOrEmpty(c)))}:", Declaration.Between(SkipWhitespaces), Documentation.Before(EndOfLine), Call.Or(Comment.ThenReturn<string>(null)).SeparatedAndOptionallyTerminated(EndOfLine.Between(SkipWhitespaces))).ManyString();
        }

        /// <inheritdoc/>
        public async Task CompileAsync(FileInfo inputFile, DirectoryInfo outputDirectory)
        {
            string inputCode = await File.ReadAllTextAsync(inputFile.FullName);
            var result = Microcode.Parse(inputCode);
            if(result.Success)
            {
                await File.WriteAllTextAsync(Path.Combine(outputDirectory.FullName, $"{Path.GetFileNameWithoutExtension(inputFile.Name)}.out"), result.Value);
            }
            else
            {
                throw new CompilationException(result.Error.RenderErrorMessage());
            }
        }
    }
}
