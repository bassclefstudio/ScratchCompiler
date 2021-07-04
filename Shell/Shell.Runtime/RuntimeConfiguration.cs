using BassClefStudio.Graphics.Core;
using BassClefStudio.Graphics.Input;
using BassClefStudio.NET.Core.Streams;
using BassClefStudio.Shell.Runtime.Processor;
using Newtonsoft.Json.Linq;

namespace BassClefStudio.Shell.Runtime
{
    /// <summary>
    /// Represents the configuration of an executable BassClefStudio.Shell runtime app, including the bindings between the runtime devices and the application's I/O.
    /// </summary>
    public class RuntimeConfiguration
    {
        /// <summary>
        /// The number of desired <see cref="ICore"/>s in this system.
        /// </summary>
        public uint CoreCount { get; set; } = 4;

        /// <summary>
        /// The desired size of the <see cref="ICore"/>s' memory space.
        /// </summary>
        public uint CoreMemorySize { get; set; } = 500;

        /// <summary>
        /// The desired size of the bootloader's memory space.
        /// </summary>
        public uint BootSize { get; set; } = 1000;

        /// <summary>
        /// The <see cref="string"/> contents of the microcode definitions file.
        /// </summary>
        public string MicrocodeFile { get; set; }

        /// <summary>
        /// The <see cref="string"/> contents of the compiled boot volume.
        /// </summary>
        public string BootFile { get; set; }

        /// <summary>
        /// The <see cref="JObject"/> parsed debug information for the bootloader code.
        /// </summary>
        public JObject DebugInfo { get; set; }

        /// <summary>
        /// The <see cref="IGraphicsView"/> for drawing content on the screen.
        /// </summary>
        public IGraphicsView GraphicsView { get; set; }

        /// <summary>
        /// The <see cref="IInputWatcher"/> for managing keyboard and mouse input.
        /// </summary>
        public IInputWatcher InputWatcher { get; set; }

        /// <summary>
        /// The <see cref="SourceStream{T}"/> that the console output should be written to.
        /// </summary>
        public SourceStream<string> ConsoleWriter { get; set; }
    }
}
