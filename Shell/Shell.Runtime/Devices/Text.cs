using BassClefStudio.Shell.Runtime.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.Shell.Runtime.Devices
{
    public class Text : IDeviceDriver
    {
        /// <inheritdoc/>
        public string Name { get; } = "Console";

        /// <inheritdoc/>
        public MemoryChunk MainChunk { get; }

        /// <inheritdoc/>
        public IEnumerable<MemoryChunk> Chunks { get; }

        /// <inheritdoc/>
        public RuntimeConfiguration Configuration { get; set; }

        /// <summary>
        /// Creates a new <see cref="Text"/>.
        /// </summary>
        public Text()
        {
            MainChunk = new MemoryChunk(8010, 2);
            Chunks = new MemoryChunk[] { MainChunk };
        }

        /// <inheritdoc/>
        public void Initialize()
        {
        }

        private string currentLine = string.Empty;
        /// <inheritdoc/>
        public string HandleRequest(MemoryRequest request)
        {
            uint address = request.Address - MainChunk.Address;
            if(address == 0)
            {
                if(request.Action == MemoryAction.Set)
                {
                    currentLine += request.Value;
                    return null;
                }
                else if(request.Action == MemoryAction.Get)
                {
                    throw new NotImplementedException();
                }
                else if(request.Action == MemoryAction.Poke)
                {
                    Configuration.ConsoleWriter.EmitValue(currentLine);
                    currentLine = string.Empty;
                    return null;
                }
                else
                {
                    throw new ArgumentException($"The given memory request attempted action {request.Action} which was not supported.", "request");
                }
            }
            else
            {
                throw new MemoryAccessException("This device could not find the requested memory.", request);
            }
        }
    }
}
