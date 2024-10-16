using System.Diagnostics;
using T6InjectorLib;

namespace Tests
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("RUNNING GENERAL USE TEST");
            if(!GeneralUseTest())
            {
                Console.WriteLine("[FAIL] GENERAL USE TEST FAILED");
                return -1;
            }

            Console.WriteLine("RUNNING SYNTAX ERROR TEST");
            if(!SyntaxErrorTest())
            {
                Console.WriteLine("[FAIL] SYNTAX ERROR TEST FAILED");
                return -2;
            }

            Console.WriteLine("ALL TESTS PASSED");

            return 0;
        }

        static bool GeneralUseTest()
        {
            string testsDir = GetTestsDirectory();
            string gscToolDir = Path.Combine(testsDir, "gsc-tool");
            string projectDir = Path.Combine(testsDir, "general_project");

            // Create injector 
            T6Injector injector = new T6Injector(gscToolDir);

            // Get project files 
            string[] projectFiles = injector.GetProjectFiles(projectDir);
            if(projectFiles.Length < 2)
            {
                Console.WriteLine($"EXPECTED TWO FILES, GOT {projectFiles.Length}");
                return false;
            }

            // Check for main.gsc 
            bool projectHasMain = injector.ProjectHasMainScript(projectDir);
            if(!projectHasMain)
            {
                Console.WriteLine($"EXPECTED main.gsc IN PROJECT ROOT");
                return false;
            }

            // Check syntax of files 
            SyntaxResult[] syntaxResults = injector.CheckProjectSyntax(projectFiles);
            foreach(var result in syntaxResults)
            {
                if(result.HasError)
                {
                    Console.WriteLine($"EXPECTED NO ERRORS, GOT ERROR IN {result.FilePath}");
                    return false;
                }
            }

            // Compile project 
            byte[] compiledProject = injector.CompileProject(projectFiles);
            if(compiledProject.Length < 1)
            {
                Console.WriteLine("EXPECTED PROJECT TO COMPILE");
                return false;
            }
            
            return true;
        }

        static bool SyntaxErrorTest()
        {
            string testsDir = GetTestsDirectory();
            string gscToolDir = Path.Combine(testsDir, "gsc-tool");
            string projectDir = Path.Combine(testsDir, "syntax_project");

            // Create injector 
            T6Injector injector = new T6Injector(gscToolDir);

            // Get project files 
            string[] projectFiles = injector.GetProjectFiles(projectDir);

            // Check project syntax 
            SyntaxResult[] syntaxResults = injector.CheckProjectSyntax(projectFiles);
            bool errorFound = false;
            foreach(var result in syntaxResults)
            {
                if (result.HasError)
                {
                    errorFound = true;
                    break;
                }
            }

            // Check for expected error in error.gsc 
            if(!errorFound)
            {
                Console.WriteLine("EXPECTED TO FIND ERROR IN error.gsc");
                return false;
            }
            
            return true;
        }

        static string GetTestsDirectory()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, @"..\..\.."));

            return projectDir;
        }
    }
}
