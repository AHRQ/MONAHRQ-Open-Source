using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
using System.Windows;
using System.Windows.Markup;

[assembly: AssemblyTitle("Monahrq.Theme")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("81fef957-3bbb-4cc4-96fc-b8a54e1ced84")]

[assembly: NeutralResourcesLanguage("")]
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
)]



[assembly: XmlnsDefinition("http://schemas.monahrq.com/theme/xaml", "Monahrq.Theme.Converters")]
[assembly: XmlnsDefinition("http://schemas.monahrq.com/theme/xaml", "Monahrq.Theme.MarkupExtensions")]
[assembly: XmlnsDefinition("http://schemas.monahrq.com/theme/xaml", "Monahrq.Theme.Behaviors")]
[assembly: XmlnsDefinition("http://schemas.monahrq.com/theme/xaml", "Monahrq.Theme.Controls")]
