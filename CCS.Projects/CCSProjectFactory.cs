/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 ﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Package;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace CCS.Projects {
    [Guid(GuidList.guidCCSProjectFactoryString)]
    public class CCSProjectFactory : ProjectFactory {

        private CCSProjectPackage package;

        public CCSProjectFactory(CCSProjectPackage package)
            : base(package) {
            this.package = package;
        }
        protected override ProjectNode CreateProject() {
            CCSProjectNode project = new CCSProjectNode(this.package);

            project.SetSite((IOleServiceProvider)
                ((IServiceProvider)this.package).GetService(
                    typeof(IOleServiceProvider)));
            return project;
        }
    }
}
