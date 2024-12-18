using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T6InjectorLib
{
    internal class GscToolDirectoryException : Exception
    {
        public GscToolDirectoryException() : base("GscToolDirectory cannot be null or empty") { }

        public GscToolDirectoryException(string message) : base(message) { }

        public GscToolDirectoryException(string message, Exception inner) : base(message, inner) { }
    }
}
