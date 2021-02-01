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
            List<Tuple<int, List<object>>> memory = new List<Tuple<int, List<object>>>();
            int address = Memory.First().Key;
            memory.Add(new Tuple<int, List<object>>(address, new List<object>()));
            foreach (var item in Memory)
            {
                if(item.Key != address)
                {
                    address = item.Key;
                    memory.Add(new Tuple<int, List<object>>(address, new List<object>()));
                }
                memory.Last().Item2.Add(item.Value);
                address++;
            }

            return new JObject(
                memory.Select(m => new JProperty(m.Item1.ToString(), m.Item2.Select(i => i.ToString()))));
        }
    }
}
