using BassClefStudio.Shell.Runtime.Devices;
using BassClefStudio.Shell.Runtime.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.Shell.Runtime.Processor
{
    public interface ISystem
    {
        /// <summary>
        /// Ensures the system is correctly setup and starts program execution.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Gets the <see cref="IDevice"/> on the system by its name.
        /// </summary>
        /// <param name="name">The <see cref="IDevice.Name"/> of the device type.</param>
        /// <returns>The requested <see cref="IDevice"/> device.</returns>
        IDevice this[string name] { get; }

        /// <summary>
        /// Gets the <see cref="IDevice"/> on the system by the memory address it manages.
        /// </summary>
        /// <param name="address">The <see cref="uint"/> memory address the desired <see cref="IDevice"/> manages.</param>
        /// <returns>The requested <see cref="IDevice"/> device.</returns>
        IDevice this[uint address] { get; }
    }

    /// <summary>
    /// A default implementation of the <see cref="ISystem"/> interface.
    /// </summary>
    public class BaseSystem : ISystem
    {
        /// <summary>
        /// The collection of registered <see cref="IDevice"/> devices.
        /// </summary>
        public List<IDevice> Devices { get; }

        /// <summary>
        /// An injected <see cref="Func{TResult}"/> to create new <see cref="ICore"/> processor cores.
        /// </summary>
        public Func<ICore> CreateCore { get; set; }

        /// <summary>
        /// The injected collection of <see cref="IDeviceDriver"/>s.
        /// </summary>
        public IEnumerable<IDeviceDriver> Drivers { get; set; }

        /// <summary>
        /// The injected <see cref="RuntimeConfiguration"/>.
        /// </summary>
        public RuntimeConfiguration Configuration { get; set; }

        /// <summary>
        /// Creates a new <see cref="BaseSystem"/>.
        /// </summary>
        public BaseSystem()
        {
            Devices = new List<IDevice>();
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            foreach(var d in Drivers)
            {
                d.Initialize();
            }
            Devices.AddRange(Drivers);

            ICore mainCore = CreateCore();
            mainCore.Id = 0;
            mainCore.System = this;
            mainCore.Initialize();
            Devices.Add(mainCore);

            for (uint i = 1; i < Configuration.CoreCount; i++)
            {
                var core = CreateCore();
                core.Id = i;
                core.System = this;
                core.Initialize();
                Devices.Add(core);
            }

            //// System initialization
            var bootChunk = new MemoryChunk(this["Boot"].MainChunk);
            mainCore.MountChunk(bootChunk);
            mainCore.PullChunk(bootChunk);
            mainCore.QueueExecution(0);
        }

        /// <inheritdoc/>
        public IDevice this[string name]
        {
            get => Devices.First(d => d.Name == name);
        }

        /// <inheritdoc/>
        public IDevice this[uint address]
        {
            get => Devices.First(d => d.Manages(address));
        }
    }

    /// <summary>
    /// Provides extension methods for the <see cref="ISystem"/> interface.
    /// </summary>
    public static class SystemExtensions
    {
        /// <summary>
        /// Delegates the <see cref="IDevice"/> on the system that handles the requested memory to handle the given <see cref="MemoryRequest"/>.
        /// </summary>
        /// <param name="systemInfo">The <see cref="ISystem"/> handling the request.</param>
        /// <param name="request">The 'remote' <see cref="MemoryRequest"/>.</param>
        /// <returns>A <see cref="string"/> value returned by the memory operation.</returns>
        /// <exception cref="MemoryAccessException">The operation failed because the given memory could not be accessed by this <see cref="ISystem"/> in the requested manner.</exception>
        public static string HandleRequest(this ISystem systemInfo, MemoryRequest request)
        {
            return systemInfo[request.Address].HandleRequest(request);
        }
    }
}
