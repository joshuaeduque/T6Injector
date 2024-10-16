using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T6InjectorLib
{
    internal class GscToolPathException : Exception
    {
        public GscToolPathException() : base("GscToolPath cannot be null or empty") { }

        public GscToolPathException(string message) : base(message) {}

        public GscToolPathException(string message, Exception inner) : base(message, inner) { }
    }
}
