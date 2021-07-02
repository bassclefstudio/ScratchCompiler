using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BassClefStudio.Shell.Runtime.Memory
{
    /// <summary>
    /// Represents any service or device that manages some section of the memory address space.
    /// </summary>
    public interface IMemoryHandler
    {
        /// <summary>
        /// Gets a collection of all <see cref="MemoryChunk"/>s this <see cref="IMemoryHandler"/> handles.
        /// </summary>
        IEnumerable<MemoryChunk> Chunks { get; }

        /// <summary>
        /// Handles a given <see cref="MemoryRequest"/> request.
        /// </summary>
        /// <param name="request">The <see cref="MemoryRequest"/> request to access this <see cref="IMemoryHandler"/>'s memory.</param>
        /// <returns>A <see cref="string"/> value returned by the memory operation.</returns>
        /// <exception cref="MemoryAccessException">The operation failed because the given memory could not be accessed by this <see cref="IMemoryHandler"/> in the requested manner.</exception>
        string HandleRequest(MemoryRequest request);
    }

    /// <summary>
    /// Provides extension methods for the <see cref="IMemoryHandler"/> interface.
    /// </summary>
    public static class MemoryHandlerExtensions
    {
        /// <summary>
        /// Checks whether this <see cref="IMemoryHandler"/> manages the given address in memory.
        /// </summary>
        /// <param name="handler">The <see cref="IMemoryHandler"/> handling the request.</param>
        /// <param name="address">The <see cref="uint"/> address to query.</param>
        /// <returns>A <see cref="bool"/> whether <paramref name="address"/> lies within one of the <see cref="IMemoryHandler.Chunks"/> that are managed (not clones) by this <see cref="IMemoryHandler"/>.</returns>
        public static bool Manages(this IMemoryHandler handler, uint address)
        {
            return handler.Chunks.Any(c => !c.IsClone && c.Contains(address));
        }

        /// <summary>
        /// Gets the first local <see cref="MemoryChunk"/> in this <see cref="IMemoryHandler"/> that contains the given address.
        /// </summary>
        /// <param name="handler">The <see cref="IMemoryHandler"/> handling the request.</param>
        /// <param name="address">The <see cref="uint"/> address to query.</param>
        /// <param name="acceptClones">A <see cref="bool"/> indicating whether to include chunks where <see cref="MemoryChunk.IsClone"/> is set to 'true'.</param>
        /// <returns>A <see cref="MemoryChunk"/> from <see cref="IMemoryHandler.Chunks"/> that contains the given address, or 'null' if none is found.</returns>
        public static MemoryChunk? GetChunk(this IMemoryHandler handler, uint address, bool acceptClones = true)
        {
            if(handler.Chunks.Any(c => c.Contains(address)))
            {
                return handler.Chunks.First(c => c.Contains(address) && (acceptClones || !c.IsClone));
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// An <see cref="Exception"/> thrown when an <see cref="IMemoryHandler"/> failed to manage a memory request.
    /// </summary>
    [Serializable]
    public class MemoryAccessException : Exception
    {
        public MemoryRequest Request { get; }

        /// <inheritdoc/>
        public MemoryAccessException(string message, MemoryRequest request) : base(message) { Request = request; }
        /// <inheritdoc/>
        public MemoryAccessException(string message, MemoryRequest request, Exception inner) : base(message, inner) { Request = request; }
        /// <inheritdoc/>
        protected MemoryAccessException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
