using BassClefStudio.Shell.Runtime.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.Shell.Runtime.Devices
{
    /// <summary>
    /// Represents a service that connects a device or service to memory.
    /// </summary>
    public interface IDevice : IMemoryHandler
    {
        /// <summary>
        /// The <see cref="string"/> name of the device, which uniquely identifies it to the system.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The <see cref="MemoryChunk"/> that should be referenced by device mappings to represent this <see cref="IDevice"/>'s memory space.
        /// </summary>
        MemoryChunk MainChunk { get; }
    }

    /// <summary>
    /// Represents any <see cref="IDevice"/> that is not a processor core.
    /// </summary>
    public interface IDeviceDriver : IDevice
    {
        /// <summary>
        /// Initializes device operation.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Injected configuration and bindings to app I/O.
        /// </summary>
        RuntimeConfiguration Configuration { get; set; }
    }
}
