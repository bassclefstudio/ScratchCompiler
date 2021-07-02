using BassClefStudio.Shell.Runtime.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BassClefStudio.Shell.Runtime.Processor
{
    /// <summary>
    /// Represents a single action that the processor can take
    /// </summary>
    public interface ISignal
    {
        /// <summary>
        /// The <see cref="ControlSignal"/> this action is bound to.
        /// </summary>
        ControlSignal Signal { get; }

        /// <summary>
        /// Performs the actions this <see cref="ISignal"/> describes on the given <see cref="ICore"/>.
        /// </summary>
        /// <param name="core">The <see cref="ICore"/> requesting the action.</param>
        /// <returns>A <see cref="ControlFlags"/> value indicating any flags that this <see cref="ISignal"/> sets.</returns>
        ControlFlags Execute(ICore core);
    }

    /// <summary>
    /// Provides extension methods of and for the <see cref="ISignal"/> interface.
    /// </summary>
    public static class SignalExtensions
    {
        /// <summary>
        /// Pushes a new <see cref="uint"/> address value off of the <see cref="ICore.Stack"/>.
        /// </summary>
        /// <param name="core">The <see cref="ICore"/> to perform the operation on.</param>
        /// <returns>The <see cref="uint"/> value from the stack.</returns>
        /// <exception cref="StackException">The value on the top of the stack is not of type <typeparamref name="T"/>.</exception>
        public static uint PushAddress(this ICore core)
        {
            var val = core.Stack.Pop();
            if (uint.TryParse(val, out var add))
            {
                return add;
            }
            else
            {
                throw new StackException($"Expected value of type uint; recieved {{{val}}}.");
            }
        }

        /// <summary>
        /// Pushes a new <see cref="float"/> value off of the <see cref="ICore.Stack"/>.
        /// </summary>
        /// <param name="core">The <see cref="ICore"/> to perform the operation on.</param>
        /// <returns>The <see cref="float"/> value from the stack.</returns>
        /// <exception cref="StackException">The value on the top of the stack is not of type <typeparamref name="T"/>.</exception>
        public static float PushNum(this ICore core)
        {
            var val = core.Stack.Pop();
            if (float.TryParse(val, out var add))
            {
                return add;
            }
            else
            {
                throw new StackException($"Expected value of type float; recieved {{{val}}}.");
            }
        }

        /// <summary>
        /// Pushes a new <see cref="bool"/> value off of the <see cref="ICore.Stack"/>.
        /// </summary>
        /// <param name="core">The <see cref="ICore"/> to perform the operation on.</param>
        /// <returns>The <see cref="bool"/> value from the stack.</returns>
        /// <exception cref="StackException">The value on the top of the stack is not of type <typeparamref name="T"/>.</exception>
        public static bool PushBool(this ICore core)
        {
            var val = core.Stack.Pop();
            if (bool.TryParse(val, out var add))
            {
                return add;
            }
            else
            {
                throw new StackException($"Expected value of type float; recieved {{{val}}}.");
            }
        }

        /// <summary>
        /// Pushes a new <see cref="string"/> value off of the <see cref="ICore.Stack"/>.
        /// </summary>
        /// <param name="core">The <see cref="ICore"/> to perform the operation on.</param>
        /// <returns>The <see cref="string"/> value from the stack.</returns>
        /// <exception cref="StackException">The value on the top of the stack is not of type <typeparamref name="T"/>.</exception>
        public static string PushString(this ICore core) => core.Stack.Pop();

        /// <summary>
        /// Pulls a new value onto the <see cref="ICore.Stack"/>.
        /// </summary>
        /// <param name="core">The <see cref="ICore"/> to perform the operation on.</param>
        /// <param name="value">The <see cref="object"/> value to add.</param>
        public static void PullStack(this ICore core, object value)
        {
            core.Stack.Push(value?.ToString());
        }

        public static void PullChunk(this ICore core, MemoryChunk chunk)
        {
            for (uint i = 0; i < chunk.Size; i++)
            {
                uint a = chunk.Address + i;
                core.LocalRequest(
                    new MemoryRequest(MemoryAction.Set, a,
                        core.System.HandleRequest(new MemoryRequest(MemoryAction.Get, a))));
            }
        }

        public static void PushChunk(this ICore core, MemoryChunk chunk)
        {
            for (uint i = 0; i < chunk.Size; i++)
            {
                uint a = chunk.Address + i;
                core.System.HandleRequest(
                    new MemoryRequest(MemoryAction.Set, a,
                        core.LocalRequest(new MemoryRequest(MemoryAction.Get, a))));
            }
        }
    }
}