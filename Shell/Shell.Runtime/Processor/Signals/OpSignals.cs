using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.Shell.Runtime.Processor.Signals
{
    /// <summary>
    /// <see cref="ControlSignal.Add"/> control signal.
    /// </summary>
    public class AddSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Add;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var a = core.PushNum();
            var b = core.PushNum();
            core.PullStack(a + b);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Neg"/> control signal.
    /// </summary>
    public class NegSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Neg;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var a = core.PushNum();
            core.PullStack(-a);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Prod"/> control signal.
    /// </summary>
    public class ProdSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Prod;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var a = core.PushNum();
            var b = core.PushNum();
            core.PullStack(a * b);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Inv"/> control signal.
    /// </summary>
    public class InvSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Inv;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var a = core.PushNum();
            core.PullStack(1 / a);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Eq"/> control signal.
    /// </summary>
    public class EqSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Eq;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var a = core.PushString();
            var b = core.PushString();
            core.PullStack(a == b);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.NEq"/> control signal.
    /// </summary>
    public class NEqSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.NEq;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var a = core.PushString();
            var b = core.PushString();
            core.PullStack(a != b);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.GThan"/> control signal.
    /// </summary>
    public class GThanSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.GThan;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var a = core.PushNum();
            var b = core.PushNum();
            core.PullStack(b > a);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.LThan"/> control signal.
    /// </summary>
    public class LThanSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.LThan;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var a = core.PushNum();
            var b = core.PushNum();
            core.PullStack(b < a);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Not"/> control signal.
    /// </summary>
    public class NotSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Not;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var val = core.PushBool();
            core.PullStack(!val);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.Or"/> control signal.
    /// </summary>
    public class OrSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.Or;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var a = core.PushBool();
            var b = core.PushBool();
            core.PullStack(b || a);
            return ControlFlags.None;
        }
    }

    /// <summary>
    /// <see cref="ControlSignal.And"/> control signal.
    /// </summary>
    public class AndSignal : ISignal
    {
        /// <inheritdoc/>
        public ControlSignal Signal { get; } = ControlSignal.And;

        /// <inheritdoc/>
        public ControlFlags Execute(ICore core)
        {
            var a = core.PushBool();
            var b = core.PushBool();
            core.PullStack(b && a);
            return ControlFlags.None;
        }
    }
}
