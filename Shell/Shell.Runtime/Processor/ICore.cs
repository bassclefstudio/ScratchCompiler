using BassClefStudio.NET.Core;
using BassClefStudio.NET.Core.Streams;
using BassClefStudio.Shell.Runtime.Devices;
using BassClefStudio.Shell.Runtime.Memory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.Shell.Runtime.Processor
{
    /// <summary>
    /// Represents a processor core, which is capable of running instructions.
    /// </summary>
    public interface ICore : IDevice, IMemoryController
    {
        /// <summary>
        /// The <see cref="uint"/> ID of this specific processor core.
        /// </summary>
        uint Id { get; set; }

        /// <summary>
        /// The current stack of <see cref="object"/> values managed in processor execution.
        /// </summary>
        Stack<string> Stack { get; }

        /// <summary>
        /// The <see cref="uint"/> address in memory the <see cref="ICore"/> is currently processing.
        /// </summary>
        uint Pointer { get; set; }

        /// <summary>
        /// The current <see cref="string"/> command name, usually set by the <see cref="ISignal"/> in charge of the 'fetch' cycle.
        /// </summary>
        string CommandName { get; set; }

        /// <summary>
        /// The <see cref="ISystem"/> describing the system this <see cref="ICore"/> is part of.
        /// </summary>
        ISystem System { get; set; }

        /// <summary>
        /// Initializes all processor resources and memory and starts the processor execution.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Queues the given <see cref="uint"/> address to be executed by the <see cref="ICore"/>.
        /// </summary>
        /// <param name="address">The <see cref="uint"/> address (in managed core memory) to start execution.</param>
        void QueueExecution(uint address);

        /// <summary>
        /// Starts a local <see cref="MemoryRequest"/> to manage memory internally stored on the <see cref="ICore"/>'s <see cref="IMemoryHandler.Chunks"/>.
        /// </summary>
        /// <param name="request">The <see cref="MemoryRequest"/> to execute.</param>
        /// <returns>A <see cref="string"/> value returned by the memory operation.</returns>
        /// <exception cref="MemoryAccessException">The operation failed because the given memory could not be accessed by this <see cref="ICore"/> in the requested manner.</exception>
        string LocalRequest(MemoryRequest request);
    }

    /// <summary>
    /// A defaut implementation of <see cref="ICore"/> that matches the behavior of the Scratch cores.
    /// </summary>
    public class Core : ICore
    {
        #region Properties

        /// <inheritdoc/>
        public Stack<string> Stack { get; }

        /// <inheritdoc/>
        public uint Pointer { get; set; }

        /// <inheritdoc/>
        public string CommandName { get; set; }

        /// <inheritdoc/>
        public ISystem System { get; set; }

        /// <inheritdoc/>
        public uint Id { get; set; }

        /// <inheritdoc/>
        public string Name => $"Core{Id}";

        /// <inheritdoc/>
        public MemoryChunk MainChunk { get; private set; }

        private List<MemoryChunk> chunks;
        /// <inheritdoc/>
        public IEnumerable<MemoryChunk> Chunks => chunks.AsEnumerable();

        /// <summary>
        /// An <see cref="IDictionary{TKey, TValue}"/> associating all registered <see cref="ISignal"/>s with the <see cref="ControlSignal"/> they handle.
        /// </summary>
        public IDictionary<ControlSignal, ISignal> Signals { get; }

        /// <summary>
        /// The collection of <see cref="CommandDefinition"/> commands.
        /// </summary>
        public IDictionary<string, CommandDefinition> Microcode { get; private set; }

        /// <summary>
        /// The injected <see cref="ICommandProvider"/> defining available commands.
        /// </summary>
        public ICommandProvider CommandProvider { get; }

        /// <summary>
        /// The injected <see cref="RuntimeConfiguration"/>.
        /// </summary>
        public RuntimeConfiguration Configuration { get; }

        /// <summary>
        /// A <see cref="SourceStream{T}"/> that manages execution requests.
        /// </summary>
        private SourceStream<uint> ExecutionStream { get; }

        #endregion
        #region Initialize

        /// <summary>
        /// Creates a new <see cref="Core"/>.
        /// </summary>
        public Core(RuntimeConfiguration config, IEnumerable<ISignal> signals, ICommandProvider commandProvider)
        {
            Stack = new Stack<string>();
            chunks = new List<MemoryChunk>();
            Signals = signals.ToDictionary(s => s.Signal);
            ExecutionStream = new SourceStream<uint>();
            ExecutionStream.BindResult(Execute);
            CommandProvider = commandProvider;
            Configuration = config;
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            MainChunk = new MemoryChunk(Configuration.BootSize + (Id * Configuration.CoreMemorySize), Configuration.CoreMemorySize);
            Microcode = CommandProvider.GetCommandsAsync().ToDictionary(m => m.Name);
            MountChunk(MainChunk);
        }

        #endregion
        #region Memory

        private Dictionary<MemoryChunk, string[]> storage = new Dictionary<MemoryChunk, string[]>();
        /// <inheritdoc/>
        public string HandleRequest(MemoryRequest request)
        {
            var chunk = this.GetChunk(request.Address, false);
            if (chunk.HasValue)
            {
                return HandleRequest(chunk.Value, request);
            }
            else
            {
                throw new MemoryAccessException("This core does not manage the requested memory.", request);
            }
        }

        /// <summary>
        /// Starts a local <see cref="MemoryRequest"/> to manage memory internally stored on the <see cref="ICore"/>'s <see cref="Chunks"/>.
        /// </summary>
        /// <param name="request">The <see cref="MemoryRequest"/> to execute.</param>
        /// <returns>A <see cref="string"/> value returned by the memory operation.</returns>
        /// <exception cref="MemoryAccessException">The operation failed because the given memory could not be accessed by this <see cref="ICore"/> in the requested manner.</exception>
        public string LocalRequest(MemoryRequest request)
        {
            var chunk = this.GetChunk(request.Address);
            if (chunk.HasValue)
            {
                return HandleRequest(chunk.Value, request);
            }
            else
            {
                throw new MemoryAccessException("This core could not find the requested memory.", request);
            }
        }

        private string HandleRequest(MemoryChunk chunk, MemoryRequest request)
        {
            var mem = storage[chunk];
            uint index = request.Address - chunk.Address;
            if (request.Action == MemoryAction.Get)
            {
                return mem[index];
            }
            else if (request.Action == MemoryAction.Set)
            {
                mem[index] = request.Value;
                return null;
            }
            else if (request.Action == MemoryAction.Poke)
            {
                if (!this.Manages(request.Address))
                {
                    throw new MemoryAccessException("Sending a poke request to a core requires that the core manages the specific memory.", request);
                }
                else
                {
                    QueueExecution(request.Address);
                    return null;
                }
            }
            else
            {
                throw new ArgumentException($"The given memory request attempted action {request.Action} which was not supported.", "request");
            }
        }

        public void MountChunk(MemoryChunk chunk)
        {
            chunks.Add(chunk);
            storage.Add(chunk, new string[chunk.Size]);
        }

        public void DismountChunk(MemoryChunk chunk)
        {
            chunks.Remove(chunk);
            storage.Remove(chunk);
        }

        #endregion
        #region Execution

        /// <inheritdoc/>
        public void QueueExecution(uint startAddress)
        {
            SynchronousTask emitTask = new SynchronousTask(
                () => Task.Run(() => ExecutionStream.EmitValue(startAddress)));
            emitTask.RunTask();
        }

        private void Execute(uint startAddress)
        {
            string ExMessage(Exception ex)
            {
                return $"{ex.GetType().FullName}\r\n{ex.Message}\r\n{ex.StackTrace}";
            }

            Pointer = startAddress;
            var flags = ControlFlags.None;
            while(!flags.HasFlag(ControlFlags.Return))
            {
                try
                {
                    flags &= RunCommand("Fetch");
                }
                catch(Exception ex)
                {
                    Configuration.ConsoleWriter.EmitValue($"An error occurred during FETCH:\r\n{ExMessage(ex)}");
                    return;
                }

                try
                {
                    flags &= RunCommand(CommandName);
                }
                catch(Exception ex)
                {
                    if (Configuration.DebugInfo.ContainsKey(Pointer.ToString()))
                    {
                        JToken position = Configuration.DebugInfo[Pointer.ToString()];
                        Configuration.ConsoleWriter.EmitValue($"An error occurred at ({position[0]},{position[1]}):\r\n{ExMessage(ex)}");
                    }
                    else
                    {
                        Configuration.ConsoleWriter.EmitValue($"An error occurred at an unknown location:\r\n{ExMessage(ex)}");
                    }
                    return;
                }
            }
        }

        private ControlFlags RunCommand(string commandName)
        {
            ControlFlags flags = ControlFlags.None;
            foreach(var s in Microcode[commandName].Signals)
            {
                flags &= Signals[s].Execute(this);
            }
            return flags;
        }

        #endregion
    }

    /// <summary>
    /// An <see cref="Exception"/> thrown when an <see cref="ICore.Stack"/> operation encounters an error.
    /// </summary>
    [Serializable]
    public class StackException : Exception
    {
        /// <inheritdoc/>
        public StackException() { }
        /// <inheritdoc/>
        public StackException(string message) : base(message) { }
        /// <inheritdoc/>
        public StackException(string message, Exception inner) : base(message, inner) { }
        /// <inheritdoc/>
        protected StackException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}