/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Babel
{
    /*
     * The Babel.Package class is needed to register the VS package and be the entry point for the language service.
     * This class derives from the Babel.BabelPackage base class which provides all the necessary functionality for a
     * babel-based language service.  This class can be used to override and extend that base class if necessary.
     * Note that the Babel.BabelPackage class derives from the Managed Package Framework's Package class.
     *     
     * Of special interest is the GUID attribute that is used to uniquely identify this package.  
     * If this code is copied for a different package, then the GUID should be regenerated
     * so to not interfere with this sample package's GUID.
     */
    
    [Microsoft.VisualStudio.Shell.PackageRegistration(UseManagedResourcesOnly=true)]
    [Microsoft.VisualStudio.Shell.DefaultRegistryRoot(@"Software\Microsoft\VisualStudio\8.0Exp")]
    [Microsoft.VisualStudio.Shell.ProvideService(typeof(Babel.LanguageService))]
    [Microsoft.VisualStudio.Shell.ProvideLanguageExtension(typeof(Babel.LanguageService), Configuration.Extension)]
    [Microsoft.VisualStudio.Shell.ProvideLanguageService(typeof(Babel.LanguageService), Configuration.Name, 0,
        CodeSense = true,
        EnableCommenting = true,
        MatchBraces = true,
        ShowCompletion = true,
        ShowMatchingBrace = true,
        AutoOutlining = true,
        EnableAsyncCompletion = true,
        CodeSenseDelay = 0)]
    [Guid("7b98c6da-ca1b-4ff4-8638-b0ba4473c68e")]
    class Package : BabelPackage
    {
    }
}
