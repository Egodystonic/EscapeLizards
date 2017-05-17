using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Change following lines when changing version number
[assembly: AssemblyVersion("0.2.*")]

// Change following lines when creating project
[assembly: AssemblyTitle("Losgap-AssetManagement")]
[assembly: AssemblyDescription("Asset management module for LOSGAP: Aids loading common 3D/texture etc assets.")]

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

