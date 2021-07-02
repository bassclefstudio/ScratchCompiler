using BassClefStudio.Shell.Runtime.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.Shell.Runtime.Devices
{
    public class Keyboard : IDeviceDriver
    {
        /// <inheritdoc/>
        public string Name { get; } = "Keyboard";

        /// <inheritdoc/>
        public MemoryChunk MainChunk { get; }

        /// <inheritdoc/>
        public IEnumerable<MemoryChunk> Chunks { get; }

        /// <inheritdoc/>
        public RuntimeConfiguration Configuration { get; set; }

        /// <summary>
        /// Creates a new <see cref="Keyboard"/>.
        /// </summary>
        public Keyboard()
        {
            MainChunk = new MemoryChunk(8003, 1);
            Chunks = new MemoryChunk[] { MainChunk };
        }

        /// <inheritdoc/>
        public void Initialize()
        {
        }

        /// <inheritdoc/>
        public string HandleRequest(MemoryRequest request)
        {
            uint address = request.Address - MainChunk.Address;
            if (address == 0)
            {
                if (request.Action == MemoryAction.Set)
                {
                    //// Set listening key.
                    return null;
                }
                else if (request.Action == MemoryAction.Get)
                {
                    //// Get listening key.
                    return false.ToString();
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
