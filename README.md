# T6Injector
Command line utility for compiling and injecting T6 GSC scripts. It's currently in an early state.

## Purpose
I've found tools like Target Manager, Control Console API, and GSC Studio are becoming
more difficult to reliably source as time goes on. Relying on leaked SDKs is
cumbersome, CCAPI is no longer supported, and GSC Studio has no official
download anymore. 

T6Injector is a simple tool to check the syntax of a project, compile it, and inject it into memory.
It uses MAPI for injection which is open source and comes installed on modern CFW.

## Issues 
T6Injector is slow since it uses a separate process to check the syntax of every GSC script 
in a project. There will be a rewrite to make this process asynchronous in the future.

I've yet to implement the actual injection part of the project as I currently
have no PS3 to test on. I had a build of a C# MAPI library but it's back home with my console right now.