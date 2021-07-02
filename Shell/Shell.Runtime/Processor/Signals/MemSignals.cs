using BassClefStudio.Shell.Runtime.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.Shell.Runtime.Processor.Signals
{
    /// <summary>
    /// <see cref="ControlSignal.Get"/> control signal.
    /// </summary>
    public class GetSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Get;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var address = core.PushAddress();
            core.PullStack(core.LocalRequest(new MemoryRequest(MemoryAction.Get, address)));
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Set"/> control signal.
    /// </summary>
    public class SetSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Set;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var address = core.PushAddress();
            var value = core.PushString();
            core.LocalRequest(new MemoryRequest(MemoryAction.Set, address, value));
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.RGet"/> control signal.
    /// </summary>
    public class RGetSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.RGet;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var address = core.PushAddress();
            core.PullStack(core.System.HandleRequest(new MemoryRequest(MemoryAction.Get, address)));
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.RSet"/> control signal.
    /// </summary>
    public class RSetSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.RSet;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var address = core.PushAddress();
            var value = core.PushString();
            core.System.HandleRequest(new MemoryRequest(MemoryAction.Set, address, value));
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Poke"/> control signal.
    /// </summary>
    public class PokeSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Poke;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var address = core.PushAddress();
            var value = core.PushString();
            core.System.HandleRequest(new MemoryRequest(MemoryAction.Poke, address, value));
            return ControlFlags.None;
        }
    }
}
