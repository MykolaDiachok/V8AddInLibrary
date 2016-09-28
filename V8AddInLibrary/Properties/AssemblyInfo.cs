using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("V8AddInLibrary")]
[assembly: AssemblyDescription("Fiscal registrator fo IKS")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Master inc.")]
[assembly: AssemblyProduct("V8AddInLibrary")]
[assembly: AssemblyCopyright("Copyright Master© 2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(true)]
// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("2146a663-595d-4ec0-9d56-b17b926b68c4")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]


//"C:\Program Files\Microsoft SDKs\Windows\v6.0A\Bin\x64\sn.exe" -Vr<dllpath>

//reg DELETE "HKLM\Software\Microsoft\StrongName\Verification" /f
//reg ADD "HKLM\Software\Microsoft\StrongName\Verification\*,*" /f
//reg DELETE "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification" /f
//reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\*,*" /f