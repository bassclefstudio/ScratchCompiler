using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.Shell.Runtime.Processor
{
    /// <summary>
    /// Represents the microcode information about a specific command.
    /// </summary>
    public struct CommandDefinition
    {
        /// <summary>
        /// The name of the command.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The <see cref="ControlSignal"/>s that make up this command.
        /// </summary>
        public IEnumerable<ControlSignal> Signals { get; }

        /// <summary>
        /// Creates a new <see cref="CommandDefinition"/>.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="signals">The <see cref="ControlSignal"/>s that make up this command.</param>
        public CommandDefinition(string name, IEnumerable<ControlSignal> signals)
        {
            Name = name;
            Signals = signals;
        }
    }
}
