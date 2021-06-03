using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.ScratchCompiler.Compilers.Commands
{
    /// <summary>
    /// Represents a map of memory locations and assigned values.
    /// </summary>
    public class MemoryMap
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> of all memory locations to write to, along with their <see cref="string"/> key.
        /// </summary>
        public Dictionary<int, object> Memory { get; }

        /// <summary>
        /// Creates a new <see cref="MemoryMap"/>.
        /// </summary>
        public MemoryMap()
        {
            Memory = new Dictionary<int, object>();
        }

        /// <summary>
        /// Gets the compiled JSON associated with this <see cref="MemoryMap"/>.
        /// </summary>
        /// <returns>A <see cref="JToken"/> containing compressed key/value pairs.</returns>
        public JToken GetJson()
        {
            List<string> map = new List<string>();
            for (int i = 0; i < Memory.Keys.Max(); i++)
            {
                map.Add(Memory.GetValueOrDefault(i, string.Empty).ToString());
            }
            return new JArray(map);
        }
    }
}
