using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pidgin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.Shell.Core.Commands
{
    /// <summary>
    /// Represents a map of memory locations and assigned values.
    /// </summary>
    public class MemoryMap
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> of all memory locations to write to, along with their <see cref="int"/> key.
        /// </summary>
        public Dictionary<int, object> Memory { get; }

        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> associating memory positions with their respective <see cref="SourcePos"/> positions in the source code.
        /// </summary>
        public Dictionary<int, SourcePos> SourcePositions { get; }

        /// <summary>
        /// Creates a new <see cref="MemoryMap"/>.
        /// </summary>
        public MemoryMap()
        {
            Memory = new Dictionary<int, object>();
            SourcePositions = new Dictionary<int, SourcePos>();
        }

        /// <summary>
        /// Adds a new item to the <see cref="MemoryMap"/>.
        /// </summary>
        /// <param name="position">The memory address to add the item at.</param>
        /// <param name="sourcePosition">The <see cref="SourcePos"/> source position at which this occurs.</param>
        /// <param name="value">The <see cref="object"/> value to add to memory.</param>
        public void AddMemory(int position, SourcePos sourcePosition, object value)
        {
            Memory.Add(position, value);
            SourcePositions.Add(position, sourcePosition);
        }

        /// <summary>
        /// Adds a new item to the <see cref="MemoryMap"/>.
        /// </summary>
        /// <param name="position">The memory address to add the item at.</param>
        /// <param name="value">The <see cref="ValueToken"/> containing the value and source position of the memory item.</param>
        public void AddMemory(int position, ValueToken value) => AddMemory(position, value.Position, value);

        /// <summary>
        /// Gets the compiled JSON associated with this <see cref="MemoryMap"/>.
        /// </summary>
        /// <returns>A <see cref="JToken"/> containing compressed key/value pairs.</returns>
        public JToken GetJson()
        {
            List<string> map = new List<string>();
            for (int i = 0; i < Memory.Keys.Max() + 1; i++)
            {
                map.Add((Memory.ContainsKey(i) ? Memory[i] : string.Empty).ToString());
            }
            return new JArray(map);
        }

        /// <summary>
        /// Gets the compiled JSON associated with this <see cref="MemoryMap"/>'s position documentation.
        /// </summary>
        /// <returns>A <see cref="JToken"/> matching output memory addresses with source code positions.</returns>
        public JToken GetDocJson()
        {
            return new JObject(SourcePositions.Select(p => new JProperty(p.Key.ToString(), new JArray(p.Value.Line, p.Value.Col))));
        }
    }
}
