using System.Diagnostics;
using T6InjectorLib;

namespace Tests
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("RUNNING GENERAL USE TEST");
            if(GeneralUseTest() > 0)
            {
                Console.WriteLine("[FAIL] GENERAL USE TEST FAILED");
                return -1;
            }

            Console.WriteLine("ALL TESTS PASSED");

            return 0;
        }

        static int GeneralUseTest()
        {
            string testsDir = GetTestsDirectory();
            string gscToolDir = Path.Combine(testsDir, "gsc-tool");
            string projectDir = Path.Combine(testsDir, "general_project");

            T6Injector injector = new T6Injector(gscToolDir);

            // Get project files 
            string[] projectFiles = injector.GetProjectFiles(projectDir);
            if(projectFiles.Length < 2)
            {
                Console.WriteLine($"EXPECTED TWO FILES, GOT {projectFiles.Length}");
                return -1;
            }

            // Check for main.gsc 
            bool projectHasMain = injector.ProjectHasMainScript(projectDir);
            if(!projectHasMain)
            {
                Console.WriteLine($"EXPECTED main.gsc IN PROJECT ROOT");
                return -2;
            }

            // Check syntax of files 
            SyntaxResult[] syntaxResults = injector.CheckProjectSyntax(projectFiles);
            foreach(var result in syntaxResults)
            {
                if(result.HasError)
                {
                    Console.WriteLine($"EXPECTED NO ERRORS, GOT ERROR IN {result.FilePath}");
                    return -3;
                }
            }

            // Compile project 
            byte[] compiledProject = injector.CompileProject(projectFiles);
            if(compiledProject.Length < 1)
            {
                Console.WriteLine("EXPECTED PROJECT TO COMPILE");
                return -4;
            }
            
            return 0;
        }

        static string GetTestsDirectory()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, @"..\..\.."));

            return projectDir;
        }
    }
}
