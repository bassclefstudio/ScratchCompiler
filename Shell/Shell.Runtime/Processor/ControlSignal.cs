using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.Shell.Runtime.Processor
{
    /// <summary>
    /// An enum detailing all of the control signal types handled by the BassClefStudio.Shell processor.
    /// </summary>
    public enum ControlSignal
    {
        Null,
        NewLine,
        Wait,
        Get,
        Set,
        RGet,
        RSet,
        Poke,
        Cmd,
        Jump,
        CJump,
        IncPointer,
        GetPointer,
        Clear,
        Push,
        Copy,
        Return,
        Add,
        Neg,
        Prod,
        Inv,
        Eq,
        NEq,
        GThan,
        LThan,
        Not,
        Or,
        And,
        Dev,
        Mnt,
        MntSelf,
        DMnt,
        CID,
        GDev,
        SDev
    }

    /// <summary>
    /// An enum detailing the flags which can be set by <see cref="ISignal"/>s
    /// </summary>
    [Flags]
    public enum ControlFlags
    {
        None = 0,
        Return = 0b1
    }
}
