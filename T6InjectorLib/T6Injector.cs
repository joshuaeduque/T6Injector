using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace T6InjectorLib
{
    public class T6Injector
    {
        public enum System
        {
            PC,
            PS3,
            XBOX
        }

        private string? gscToolPath;
        private string? gscToolDirectory;
        private readonly System targetSystem;

        public string? GscToolPath
        {
            get { return gscToolPath; }
        }

        public string? GscToolDirectory
        {
            get { return gscToolDirectory; }
        }

        public System TargetSystem
        {
            get { return targetSystem; }
        }

        public T6Injector(System targetPlatform)
        {
            this.targetSystem = targetPlatform;
        }

        public void SetGscToolPath(string gscToolPath)
        {
            // Check gsc tool path argument 
            if(string.IsNullOrEmpty(gscToolPath))
                throw new ArgumentException("GSC tool path cannot be null or empty", nameof(gscToolPath));

            // Check if gsc tool exists at given path 
            if (!File.Exists(gscToolPath))
                throw new IOException("GSC tool does not exist at path");

            // Check if gsc tool path has a root directory.
            // We need both the path to the gsc tool executable and its root directory.
            // If the given gsc tool path IS the root directory, that's a problem.
            DirectoryInfo? gscToolInfo = Directory.GetParent(gscToolPath);
            if (gscToolInfo == null)
                throw new ArgumentException("GSC tool path is a root directory, not a file", nameof(gscToolPath));

            // Set gsc tool root 
            this.gscToolDirectory = gscToolInfo.FullName;
            // Set gsc tool path 
            this.gscToolPath = gscToolPath;
        }

        public string[] GetProjectFiles(string projectDirectory)
        {
            // Check project directory null or empty 
            if (string.IsNullOrEmpty(projectDirectory))
                throw new ArgumentException("Project directory cannot be null or empty", nameof(projectDirectory));

            // Check if project directory exists 
            if (!Directory.Exists(projectDirectory))
                throw new IOException("Project directory does not exist");

            // Try to get project files 
            try
            {
                string[] projectFiles = Directory.GetFiles(projectDirectory, "*.gsc", SearchOption.AllDirectories);
                return projectFiles;
            }
            catch(Exception ex)
            {
                throw new IOException("Failed to get project files", ex);
            }
        }

        public bool ProjectHasMainScript(string projectDirectory)
        {
            string mainPath = Path.Combine(projectDirectory, "main.gsc");
            return File.Exists(mainPath);
        }


        public SyntaxResult[] CheckProjectSyntax(string[] projectFiles)
        {
            // Check gsc tool path and directory 
            if (string.IsNullOrEmpty(gscToolPath))
                throw new InvalidOperationException("GSC tool path is null or empty");

            if (string.IsNullOrEmpty(gscToolDirectory))
                throw new InvalidOperationException("GSC tool root directory is null or empty");

            // Check if project files is null 
            if (projectFiles == null)
                throw new ArgumentNullException(nameof(projectFiles), "Project filles cannot be null");

            // Check if project files is empty 
            if(projectFiles.Length == 0)
                throw new ArgumentException("Project files cannot be empty", nameof(projectFiles));

            List<SyntaxResult> results = new List<SyntaxResult>();

            // Check syntax of each file 
            foreach(string file in projectFiles)
            {
                // Create process start info 
                string gscToolArguments = GetParseArguments(file);
                ProcessStartInfo startInfo = new ProcessStartInfo(gscToolPath, gscToolArguments)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                string filePath = file;
                bool hasError = false;
                string? errorMessage = null;

                // Start process 
                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;

                    process.ErrorDataReceived += (sender, e) => {
                        if (string.IsNullOrEmpty(e.Data))
                            return;

                        if(e.Data.StartsWith("[ERROR]"))
                        {
                            hasError = true;
                            errorMessage = e.Data;
                        }
                    };

                    bool started = process.Start();
                    if (!started)
                        throw new IOException("Failed to start gsc-tool process");

                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    process.Close();
                }

                results.Add(new SyntaxResult(filePath, hasError, errorMessage));
            }

            return results.ToArray();
        }

        public byte[] CompileProject(string[] projectFiles)
        {
            // Check gsc tool path and directory 
            if (string.IsNullOrEmpty(gscToolPath))
                throw new InvalidOperationException("GSC tool path is null or empty");

            if (string.IsNullOrEmpty(gscToolDirectory))
                throw new InvalidOperationException("GSC tool root directory is null or empty");

            // Check if project files is null 
            if (projectFiles == null)
                throw new ArgumentNullException(nameof(projectFiles), "Project filles cannot be null");

            // Check if project files is empty 
            if (projectFiles.Length == 0)
                throw new ArgumentException("Project files cannot be empty", nameof(projectFiles));

            // Create contents to compile 
            StringBuilder sb = new StringBuilder();
            foreach(var file in projectFiles)
            {
                string text = File.ReadAllText(file);
                sb.Append(text + "\n");
            }
            string precompileContents = sb.ToString();

            // Check if precompile file exists 
            string precompilePath = Path.Combine(gscToolDirectory, "precompile.gsc");
            string compiledPath = Path.Combine(gscToolDirectory, "compiled", "t6", "precompile.gsc");

            // Delete it if it does 
            // NOTE might not need this? 
            if (File.Exists(precompilePath))
                File.Delete(precompilePath);
            // Also delete compiled script 
            if (File.Exists(compiledPath))
                File.Delete(compiledPath);

            // Write new precompile file in gsc-tool directory 
            File.WriteAllText(precompilePath, precompileContents);

            // Get gsc-tool compile arguments 
            string arguments = GetCompileArguments(precompilePath);

            // Create gsc-tool start info 
            ProcessStartInfo startInfo = new ProcessStartInfo(gscToolPath, arguments)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = gscToolDirectory
            };

            // Start gsc-tool process 
            bool errorCompiling = false;
            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.ErrorDataReceived += (sender, e) => {
                    if (string.IsNullOrEmpty(e.Data))
                        return;
                    errorCompiling = true;
                    Console.WriteLine(e.Data);
                };

                bool started = process.Start();
                if (!started)
                    throw new IOException("Failed to start gsc-tool process");

                process.BeginErrorReadLine();
                process.WaitForExit();
                process.Close();
            }

            // TODO make this not doo-doo 
            if (errorCompiling)
                throw new IOException("Failed to compile script");

            // Check if precompile.gsc exists where expected 
            if (!File.Exists(compiledPath))
                throw new IOException("precompile.gsc does not exist");

            byte[] compiledScript = File.ReadAllBytes(compiledPath);

            return compiledScript;
        }

        private string GetCompileArguments(string filePath)
        {
            StringBuilder arguments = new StringBuilder();

            // Set mode 
            arguments.Append("-m comp ");

            // Set game 
            arguments.Append("-g t6 ");

            // Set target system  
            switch (targetSystem)
            {
                case System.PS3:
                    arguments.Append("-s ps3 ");
                    break;
                case System.XBOX:
                    arguments.Append("-s xb2 ");
                    break;
                case System.PC:
                    arguments.Append("-s pc ");
                    break;
                default:
                    throw new InvalidOperationException("Target system not set");
            }

            // Set file path 
            arguments.Append(filePath);

            return arguments.ToString();
        }

        private string GetParseArguments(string filePath)
        {
            // Check if file path null or empty 
            if(string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            // Check if file exists 
            if (!File.Exists(filePath))
                throw new InvalidOperationException("File does not exist at given file path");

            StringBuilder arguments = new StringBuilder();

            // Set mode 
            arguments.Append("-m parse ");

            // Set game 
            arguments.Append("-g t6 ");

            // Set target system  
            switch(targetSystem)
            {
                case System.PS3:
                    arguments.Append("-s ps3 ");
                    break;
                case System.XBOX:
                    arguments.Append("-s xb2 ");
                    break;
                case System.PC:
                    arguments.Append("-s pc ");
                    break;
                default:
                    throw new InvalidOperationException("Target system not set");
            }

            // Set dry 
            arguments.Append("-y ");

            // Set file path 
            arguments.Append(filePath);

            // Return arguments string 
            return arguments.ToString();
        }
    }
}
