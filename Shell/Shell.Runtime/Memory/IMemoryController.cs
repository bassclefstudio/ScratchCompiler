using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.Shell.Runtime.Memory
{
    /// <summary>
    /// An <see cref="IMemoryHandler"/> with the ability to add and remove <see cref="MemoryChunk"/>s from the memory it handles.
    /// </summary>
    public interface IMemoryController : IMemoryHandler
    {
        /// <summary>
        /// Mounts the given <see cref="MemoryChunk"/> to the <see cref="IMemoryHandler.Chunks"/> collection.
        /// </summary>
        /// <param name="chunk">The <see cref="MemoryChunk"/> to add.</param>
        void MountChunk(MemoryChunk chunk);

        /// <summary>
        /// Dismounts the given <see cref="MemoryChunk"/> from the <see cref="IMemoryHandler.Chunks"/> collection.
        /// </summary>
        /// <param name="chunk">The <see cref="MemoryChunk"/> to remove.</param>
        void DismountChunk(MemoryChunk chunk);
    }
}
