using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.Shell.Runtime.Memory
{
    /// <summary>
    /// Represents a contiguous chunk of memory managed by an <see cref="IMemoryHandler"/>.
    /// </summary>
    public struct MemoryChunk : IEquatable<MemoryChunk>
    {
        /// <summary>
        /// The starting address of this <see cref="MemoryChunk"/>.
        /// </summary>
        public uint Address { get; }

        /// <summary>
        /// The size, in addresses, of the <see cref="MemoryChunk"/>.
        /// </summary>
        public uint Size { get; }

        /// <summary>
        /// A <see cref="bool"/> indicating whether the <see cref="MemoryChunk"/> is a clone of a parent chunk.
        /// </summary>
        public bool IsClone { get; }

        /// <summary>
        /// Creates a new <see cref="MemoryChunk"/>.
        /// </summary>
        /// <param name="address">The starting address of this <see cref="MemoryChunk"/>.</param>
        /// <param name="size">The size, in addresses, of the <see cref="MemoryChunk"/>.</param>
        /// <param name="isClone">A <see cref="bool"/> indicating whether the <see cref="MemoryChunk"/> is a clone of a parent chunk.</param>
        public MemoryChunk(uint address, uint size, bool isClone = false)
        {
            Address = address;
            Size = size;
            IsClone = isClone;
        }

        /// <summary>
        /// Creates a new clone <see cref="MemoryChunk"/> from a parent <see cref="MemoryChunk"/>.
        /// </summary>
        /// <param name="parent">The parent <see cref="MemoryChunk"/>, from which this chunk's <see cref="Address"/> and <see cref="Size"/> are sourced.</param>
        public MemoryChunk(MemoryChunk parent) : this(parent.Address, parent.Size, true)
        { }

        /// <summary>
        /// Checks if the given address lies within this <see cref="MemoryChunk"/>.
        /// </summary>
        /// <param name="address">The <see cref="uint"/> address to check.</param>
        /// <returns>A <see cref="bool"/> indicating whether ths chunk contains <paramref name="address"/>.</returns>
        public bool Contains(uint address)
        {
            return address >= Address && address < (Address + Size);
        }

        #region Operators

        public override bool Equals(object obj)
        {
            return obj is MemoryChunk chunk && Equals(chunk);
        }

        public bool Equals(MemoryChunk other)
        {
            return Address == other.Address &&
                   Size == other.Size &&
                   IsClone == other.IsClone;
        }

        public static bool operator ==(MemoryChunk left, MemoryChunk right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MemoryChunk left, MemoryChunk right)
        {
            return !(left == right);
        }

        #endregion
    }
}
