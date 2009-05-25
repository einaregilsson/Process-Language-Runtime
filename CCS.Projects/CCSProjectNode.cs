using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Package;
using System.Drawing;
using System.Windows.Forms;

namespace CCS.Projects {
    public class CCSProjectNode : ProjectNode {
        private static ImageList imageList;
        static CCSProjectNode() {
            imageList =
                Utilities.GetImageList(
                    typeof(CCSProjectNode).Assembly.GetManifestResourceStream(
                        "CCS.Projects.Resources.CCSProjectNode.bmp"));
        }

        internal static int imageIndex;
        public override int ImageIndex {
            get { return imageIndex + 0; }
        }

        private CCSProjectPackage package;

        public CCSProjectNode(CCSProjectPackage package) {
            this.package = package;
            imageIndex = this.ImageHandler.ImageList.Images.Count;
            foreach (Image img in imageList.Images) {
                this.ImageHandler.AddImage(img);
            }
        }

        public override Guid ProjectGuid {
            get { return GuidList.guidCCSProjectFactory; }
        }

        public override string ProjectType {
            get { return "CCSProjectType"; }
        }

        public override void AddFileFromTemplate(string source, string target) {
            this.FileTemplateProcessor.UntokenFile(source, target);
            this.FileTemplateProcessor.Reset();
        }
    }
}
