using BassClefStudio.Shell.Runtime.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BassClefStudio.Shell.Runtime.Processor.Signals
{
    /// <summary>
    /// <see cref="ControlSignal.Null"/> control signal.
    /// </summary>
    public class NullSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Null;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            core.PullStack(null);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.NewLine"/> control signal.
    /// </summary>
    public class NewLineSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.NewLine;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            core.PullStack(Environment.NewLine);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Wait"/> control signal.
    /// </summary>
    public class WaitSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Wait;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            Thread.Sleep((int)Math.Round(core.PushNum() * 1000));
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Cmd"/> control signal.
    /// </summary>
    public class CmdSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Cmd;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var cmd = core.PushString();
            core.CommandName = cmd;
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Cmd"/> control signal.
    /// </summary>
    public class IncPSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.IncPointer;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            core.Pointer++;
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.GetPointer"/> control signal.
    /// </summary>
    public class GetPSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.GetPointer;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            core.PullStack(core.Pointer);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Push"/> control signal.
    /// </summary>
    public class PushSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Push;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            _ = core.PushString();
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Clear"/> control signal.
    /// </summary>
    public class ClearSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Clear;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            core.Stack.Clear();
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Copy"/> control signal.
    /// </summary>
    public class CopySignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Copy;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var val = core.PushString();
            core.PullStack(val);
            core.PullStack(val);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Return"/> control signal.
    /// </summary>
    public class ReturnSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Return;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            return ControlFlags.Return;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Jump"/> control signal.
    /// </summary>
    public class JumpSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Jump;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var address = core.PushAddress();
            core.Pointer = address;
            return ControlFlags.Return;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.CJump"/> control signal.
    /// </summary>
    public class CJumpSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.CJump;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var address = core.PushAddress();
            if (core.PushBool())
            {
                core.Pointer = address;
            }
            return ControlFlags.Return;
        }
    }
}
