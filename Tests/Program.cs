using System.Diagnostics;
using T6InjectorLib;

namespace Tests
{
    internal class Program
    {
        const string PROJECT_NAME = "T6Injector";
        const string TESTS_PROJECT_NAME = "Tests";
        const string DEBUG_OR_RELEASE = "Debug";
        const string DOT_NET_VERSION = "net8.0";

        static int Main(string[] args)
        {
            if(!GeneralUseTest())
            {
                Console.WriteLine("GENERAL USE TEST FAILED");
                return -1;
            }

            Console.WriteLine("ALL TESTS PASSED");

            return 0;
        }

        static bool GeneralUseTest()
        {
            // Get test project directory 
            string testProjectName = "test_project";
            string testsDirectory = GetTestsDirectory(TESTS_PROJECT_NAME);
            string testProjectDir = Path.Combine(testsDirectory, testProjectName);
            string gscToolDirectory = Path.Combine(testsDirectory, "gsc-tool");

            // Create injector 
            T6Injector injector = new T6Injector(gscToolDirectory);

            // Get project files 
            string[] projectFiles = injector.GetProjectFiles(testProjectDir);
            if (projectFiles.Length < 1)
            {
                Console.WriteLine("Incorrect project files length");
                return false;
            }

            // Check for main script 
            bool hasMainScript = injector.ProjectHasMainScript(testProjectDir);
            if (!hasMainScript)
            {
                Console.WriteLine("Incoreectly reported missing main script");
                return false;
            }

            // Check project syntax 
            SyntaxResult[] syntaxResults = injector.CheckProjectSyntax(projectFiles);
            bool errorFound = false;
            foreach(var result in syntaxResults)
            {
                if(result.HasError)
                {
                    errorFound = true;
                    break;
                }
            }

            if(!errorFound)
            {
                Console.WriteLine("Incorrectly reported no syntax errors");
                return false;
            }

            // Get project scripts without errors 
            string[] compileScripts = syntaxResults.Where(result => !result.HasError).Select(result => result.FilePath).ToArray();
            byte[] compiledScript = injector.CompileProject(compileScripts);

            if(compileScripts.Length < 1)
            {
                Console.WriteLine("Incorrectly reported no compiled bytes");
                return false;
            }

            return true;
        }

        static string GetSolutionDirectory()
        {
            // Get the directory of the test being executed 
            // For ex C:\Projects\T6Injector\Tests\bin\{debugOrRelease}\{dotNetVersion}\
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Move up the base path
            // For ex move from C:\Projects\T6Injector\Tests\bin\{debugOrRelease}\{dotNetVersion}\
            // to C:\Projects\T6Injector
            string? solutionDirectory = (Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName) ?? throw new NullReferenceException();

            return solutionDirectory;
        }

        static string GetTestsDirectory(string testsProjectName)
        {
            // Get solution directory 
            string solutionDirectory = GetSolutionDirectory();
            // Get tests directory 
            string testsDirectory = Path.Combine(solutionDirectory, testsProjectName);

            return testsDirectory;
        }

        static string GetInjectorDirectory(string projectName, string debugOrRelease, string dotNetVersion)
        {
            // Get solution directory 
            string solutionDirectory = GetSolutionDirectory();
            // Get injector directory 
            string injectorDirectory = Path.Combine(solutionDirectory, projectName, "bin", debugOrRelease, dotNetVersion);

            return injectorDirectory;
        }
    }
}
