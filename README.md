# T6Injector
C# library and CLI for compiling / injecting T6 GSC scripts using gsc-tool. Currently in a very early state 😅.

## Usage Example
Here's a simple example of compiling a project:
```cs
// Get the directory containing gsc-tool.exe
string gscToolPath = "C:/Users/Josh/gsc-tool";
// Create an injector 
T6Injector injector = new T6Injector(gscToolpath);
// Get the project files 
string[] projectFiles = injector.GetProjectFiles("C:/Users/Josh/project_dir");
// Compile the project files 
byte[] compiledScript = injector.CompileProject(projectFiles);
```

## Purpose
Tools like Target Manager, Control Console API, and GSC Studio are difficult to source reliably, either relying on leaked Sony SDKs or websites that no longer exist.

T6Injector aims to be an open source alternative to GSC Studio that uses the MAPI communications library, an open source alternative to Target Manager and Control Console API. This means that unlike the previously mentioned software, T6Injector will always be updatable and accessible.

## Issues 
The CLI is not implemented which seems silly but I promise I'll get to it.

I've yet to implement the actual injection part of the project as I currently
have no PS3 to test on. I had a build of a C# MAPI library but it's back home with my console right now.