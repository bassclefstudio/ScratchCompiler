using BassClefStudio.Shell.Runtime.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.Shell.Runtime.Processor.Signals
{
    /// <summary>
    /// <see cref="ControlSignal.Dev"/> control signal.
    /// </summary>
    public class DevSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Dev;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var name = core.PushString();
            var dev = core.System[name];
            core.PullStack(dev.MainChunk.Size);
            core.PullStack(dev.MainChunk.Address);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Mnt"/> control signal.
    /// </summary>
    public class MntSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Mnt;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var address = core.PushAddress();
            var size = core.PushAddress();
            core.MountChunk(new MemoryChunk(address, size, true));
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.MntSelf"/> control signal.
    /// </summary>
    public class MntSelfSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.MntSelf;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var address = core.PushAddress();
            var size = core.PushAddress();
            core.MountChunk(new MemoryChunk(address, size, false));
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.DMnt"/> control signal.
    /// </summary>
    public class DMntSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.DMnt;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var address = core.PushAddress();
            var chunk = core.GetChunk(address).Value;
            core.DismountChunk(chunk);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.CID"/> control signal.
    /// </summary>
    public class CIDSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.CID;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            core.PullStack(core.Name);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.GDev"/> control signal.
    /// </summary>
    public class GDevSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.GDev;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var address = core.PushAddress();
            var chunk = core.GetChunk(address).Value;
            core.PullChunk(chunk);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.SDev"/> control signal.
    /// </summary>
    public class SDevSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.SDev;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var address = core.PushAddress();
            var chunk = core.GetChunk(address).Value;
            core.PushChunk(chunk);
            return ControlFlags.None;
        }
    }
}
