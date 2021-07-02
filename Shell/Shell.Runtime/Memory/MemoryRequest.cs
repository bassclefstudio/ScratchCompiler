using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.Shell.Runtime.Memory
{
    /// <summary>
    /// Represents a request to access a given piece of memory.
    /// </summary>
    public struct MemoryRequest
    {
        /// <summary>
        /// The <see cref="MemoryAction"/> action to request.
        /// </summary>
        public MemoryAction Action { get; }

        /// <summary>
        /// The <see cref="uint"/> address in memory to perform the action.
        /// </summary>
        public uint Address { get; }

        /// <summary>
        /// An optional <see cref="string"/> parameter.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Creates a new <see cref="MemoryRequest"/>.
        /// </summary>
        /// <param name="action">The <see cref="MemoryAction"/> action to request.</param>
        /// <param name="address">The <see cref="uint"/> address in memory to perform the action.</param>
        /// <param name="value">An optional <see cref="string"/> parameter.</param>
        public MemoryRequest(MemoryAction action, uint address, string value = null)
        {
            Action = action;
            Address = address;
            Value = value;
        }
    }

    /// <summary>
    /// The type of action being performed in memory.
    /// </summary>
    public enum MemoryAction
    {
        /// <summary>
        /// Retrieves a value from memory.
        /// </summary>
        Get = 0,
        /// <summary>
        /// Stores a value in memory.
        /// </summary>
        Set = 1,
        /// <summary>
        /// Pokes/notifies a given memory address, with an optional value.
        /// </summary>
        Poke = 2
    }
}
