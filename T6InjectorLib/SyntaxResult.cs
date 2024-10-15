using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T6InjectorLib
{
    public class SyntaxResult
    {
        private readonly string filePath;
        private readonly bool hasError;
        private readonly string? errorMessage;

        public string FilePath
        {
            get { return filePath; }
        }

        public bool HasError
        {
            get { return hasError; }
        }

        public string? ErrorMessage
        {
            get { return errorMessage; }
        }

        public SyntaxResult(string filePath, bool hasError, string? errorMessage)
        {
            this.filePath = filePath;
            this.hasError = hasError;
            this.errorMessage = errorMessage;
        }
    }
}
