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
     * The Babel.LanguageService class is needed to register the VS language service.  
     * This class derives from the Babel.BabelLanguageService base class which provides all the necessary 
     * functionality for a babel-based language service.  This class can be used to override and extend that 
     * base class if necessary.
     * 
     * Note that the Babel.BabelLanguageService class derives from the Managed 
     * Package Framework's LanguageService class.
     *     
     * Of special interest is the GUID attribute that is used to uniquely identify this language service.  
     * If this code is copied for a different language service, then the GUID should be regenerated
     * so to not interfere with this sample language service's GUID.
     */

    [Guid("73DA124B-2CC5-4f79-A0DB-B11B6AAA2BE5")]
    class LanguageService : BabelLanguageService
    {
    }
}
