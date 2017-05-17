using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Change following lines when changing version number
[assembly: AssemblyVersion("0.2.*")]

// Change following lines when creating project
[assembly: AssemblyTitle("Losgap-Core")]
[assembly: AssemblyDescription("Core project for Ophidian LOSGAP. Required by all other LOSGAP modules (and therefore by any application that uses LOSGAP).")]

// Following lines never need changing
#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#elif DEVELOPMENT
[assembly: AssemblyConfiguration("DEVELOPMENT")]
#elif RELEASE
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyCompany("Ophidian Games")]
[assembly: AssemblyProduct("Lightweight Open Source 3D Games & Applications Platform")]
[assembly: AssemblyCopyright("Copyright © Ophidian Games 2014")]
[assembly: ComVisible(false)]
[assembly: AssemblyCulture("")]
[assembly: InternalsVisibleTo("LosgapTests")]
[assembly: CLSCompliant(false)]


