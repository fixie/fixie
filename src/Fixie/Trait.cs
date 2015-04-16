using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixie
{
    /// <summary>
    /// Represents a named oiece of metadata of a test method.
    /// </summary>
    ///<remarks>This is what you would use to define categories.</remarks>
    public class Trait
    {
        public Trait(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public string Value { get; private set; }
    }
}
