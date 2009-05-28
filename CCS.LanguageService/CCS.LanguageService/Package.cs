/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

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

    [ProvideLoadKey("Standard", "1.0", "CCS Language Service", "Einar Egilsson", 1)]
    [PackageRegistration(UseManagedResourcesOnly=true)]
    [DefaultRegistryRoot(@"Software\Microsoft\VisualStudio\9.0Exp")]
    [ProvideService(typeof(CCS.LanguageService.CCSLanguage))]
    [ProvideLanguageExtension(typeof(CCS.LanguageService.CCSLanguage), Configuration.Extension)]
    [ProvideLanguageService(typeof(CCS.LanguageService.CCSLanguage), Configuration.Name, 0,
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
