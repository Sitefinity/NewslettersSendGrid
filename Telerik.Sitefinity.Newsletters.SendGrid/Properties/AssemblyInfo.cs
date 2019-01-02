using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using Telerik.Sitefinity.Newsletters.SendGrid;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Telerik.Sitefinity.Notifications.SendGrid")]
[assembly: AssemblyCopyright("Copyright © 2005-2019 Progress Software Corporation and/or one of its subsidiaries or affiliates. All rights reserved.")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("010e0b8a-31b6-4f53-af2f-34d81ab03bc0")]

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
[assembly: PreApplicationStartMethod(typeof(Startup), "OnPreApplicationStart")]
