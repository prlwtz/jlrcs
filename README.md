# jlrcs
C# .NET 8.0 library for interacting with the JLR Remote car API.


The library has been manually ported from the 'jlrpy' Python version from 'ardevd' https://github.com/ardevd/jlrpy
All honours for the JLR coding logic goes to that project.

A simple project using the library is provided in this 'jlrcs' repository.
Please see the 'ardevd/jlrpy' repository for further documentation and sample usage.

Porting has been done fully manually and source code is based on C# 8 standard
Class extensions to Json and String classes have been added to better align with python coding
 
The library structure has been kept as equal as possible to the original python code
Method names have been converted to C# CamelCase standard to satisfy the compiler
Property names have been untouched
