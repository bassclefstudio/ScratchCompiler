using Newtonsoft.Json.Linq;
using BassClefStudio.Shell.Runtime.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.Shell.Runtime.Devices
{
    public class Bootloader : IDeviceDriver
    {
        /// <inheritdoc/>
        public string Name { get; } = "Boot";

        /// <inheritdoc/>
        public MemoryChunk MainChunk { get; private set; }

        /// <inheritdoc/>
        public IEnumerable<MemoryChunk> Chunks { get; private set; }

        /// <summary>
        /// The contents of the boot volume.
        /// </summary>
        public string[] Volume { get; private set; }

        /// <inheritdoc/>
        public RuntimeConfiguration Configuration { get; set; }

        /// <summary>
        /// Creates a new <see cref="Bootloader"/>.
        /// </summary>
        public Bootloader()
        { }

        /// <inheritdoc/>
        public string HandleRequest(MemoryRequest request)
        {
            if (this.Manages(request.Address))
            {
                if (request.Action == MemoryAction.Set)
                {
                    Volume[request.Address - MainChunk.Address] = request.Value;
                    return null;
                }
                else if (request.Action == MemoryAction.Get)
                {
                    return Volume[request.Address - MainChunk.Address];
                }
                else
                {
                    throw new ArgumentException($"The given memory request attempted action {request.Action} which was not supported.", "request");
                }
            }
            else
            {
                throw new MemoryAccessException("The bootloader does not manage the requested memory.", request);
            }
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            MainChunk = new MemoryChunk(0, Configuration.BootSize);
            Chunks = new MemoryChunk[] { MainChunk };

            Volume = new string[Configuration.BootSize];
            JArray json = JArray.Parse(Configuration.BootFile);
            for (int i = 0; i < json.Count; i++)
            {
                if(json[i].Type == JTokenType.String)
                {
                    Volume[i] = (string)json[i];
                }
                else
                {
                    Volume[i] = json[i].ToString();
                }
            }
        }
    }
}
