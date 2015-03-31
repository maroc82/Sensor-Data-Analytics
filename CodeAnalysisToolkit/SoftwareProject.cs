using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABB.SrcML;

namespace CodeAnalysisToolkit
{
    public class SoftwareProject
    {
        public string FullPath { get; set; }

        public Language PrimaryLanguage { get; set; }

        public string ProjectName { get; set; }

        public string DataDirectory { get { return String.Format("{0}_{1}", ProjectName, Version); } }

        public string Version { get; set; }

        public SoftwareProject(string projectName, string projectVersion, Language language, string rootDirectory)
        {
            ProjectName = projectName;
            Version = projectVersion;
            FullPath = Path.GetFullPath(rootDirectory);
            PrimaryLanguage = language;
        }

        public override string ToString()
        {
            return String.Format("{0} {1}", ProjectName, Version);
        }

    }
}
