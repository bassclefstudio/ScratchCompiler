using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.Shell.Runtime.Processor
{
    /// <summary>
    /// A service that can resolve <see cref="CommandDefinition"/>s from the system configuration.
    /// </summary>
    public interface ICommandProvider
    {
        /// <summary>
        /// Resolves the <see cref="CommandDefinition"/>s from configuration, parsing input files as needed.
        /// </summary>
        /// <returns>The collection of all <see cref="CommandDefinition"/> microcode the processor has available.</returns>
        IEnumerable<CommandDefinition> GetCommandsAsync();
    }

    /// <summary>
    /// A default implementation of <see cref="ICommandProvider"/>.
    /// </summary>
    public class MicrocodeProvider : ICommandProvider
    {
        private RuntimeConfiguration Configuration { get; }
        /// <summary>
        /// Creates a new <see cref="MicrocodeProvider"/>.
        /// </summary>
        public MicrocodeProvider(RuntimeConfiguration config)
        {
            Configuration = config;
        }

        private IEnumerable<CommandDefinition> commands;
        /// <inheritdoc/>
        public IEnumerable<CommandDefinition> GetCommandsAsync()
        {
            if (commands == null)
            {
                commands = ResolveCommandsAsync(Configuration.MicrocodeFile);
            }

            return commands;
        }

        private IEnumerable<CommandDefinition> ResolveCommandsAsync(string json)
        {
            var content = JArray.Parse(json);
            foreach (JObject command in content)
            {
                var signals = ((string)command["Signals"])
                    .Split('|')
                    .Select(s => (ControlSignal)Enum.Parse(typeof(ControlSignal), s));
                yield return new CommandDefinition((string)command["Name"], signals);
            }
        }
    }
}
