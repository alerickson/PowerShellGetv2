
using System;
using System.Management.Automation;
using System.Threading;
using NuGet.Versioning;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Collections.ObjectModel;

namespace Microsoft.PowerShell.PowerShellGet.Cmdlets
{


    /// <summary>
    /// Uninstall 
    /// </summary>

    [Cmdlet(VerbsLifecycle.Uninstall, "PSResource", DefaultParameterSetName = "NameParameterSet", SupportsShouldProcess = true,
    HelpUri = "<add>", RemotingCapability = RemotingCapability.None)]
    public sealed
    class UninstallPSResource : PSCmdlet
    {
        //  private string PSGalleryRepoName = "PSGallery";

        /// <summary>
        /// Specifies the exact names of resources to install from a repository.
        /// A comma-separated list of module names is accepted. The resource name must match the resource name in the repository.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "NameParameterSet")]
        [ValidateNotNullOrEmpty]
        public string[] Name
        {
            get
            { return _name; }

            set
            { _name = value; }
        }
        private string[] _name; // = new string[0];


        /// <summary>
        /// Specifies the version or version range of the package to be installed
        /// </summary>
        [Parameter(ParameterSetName = "NameParameterSet")]
        [ValidateNotNullOrEmpty]
        public string Verison
        {
            get
            { return _version; }

            set
            { _version = value; }
        }
        private string _version;

        /// <summary>
        /// Specifies to allow ONLY prerelease versions to be uninstalled
        /// </summary>
        [Parameter(ParameterSetName = "NameParameterSet")]
        public SwitchParameter PrereleaseOnly
        {
            get
            { return _prereleaseOnly; }

            set
            { _prereleaseOnly = value; }
        }
        private SwitchParameter _prereleaseOnly;

        /// <summary>
        /// Overrides warning messages about resource installation conflicts.
        /// If a resource with the same name already exists on the computer, Force allows for multiple versions to be installed.
        /// If there is an existing resource with the same name and version, Force does NOT overwrite that version.
        /// </summary>
        [Parameter()]
        public SwitchParameter Force
        {
            get { return _force; }

            set { _force = value; }
        }
        private SwitchParameter _force;



        private CancellationTokenSource source;
        private CancellationToken cancellationToken;

        NuGetVersion nugetVersion;
        VersionRange versionRange;


        /// <summary>
        /// </summary>
        protected override void ProcessRecord()
        {
            source = new CancellationTokenSource();
            cancellationToken = source.Token;



            var bleh = NuGetVersion.TryParse(_version, out nugetVersion);


            if (nugetVersion == null)
            {
                VersionRange.TryParse(_version, out versionRange); 
            }



            foreach (var pkgName in _name)
            {
                UninstallPkgHelper(pkgName, cancellationToken);
            }

        }



        /// just uninstall module, not dependencies


        private void UninstallPkgHelper(string pkgName, CancellationToken cancellationToken)
        {
            // consider scope
           
            // update later, this is just for testing purposes 
            var psModulesPath = "C:\\code\\temp\\installtestpath";


            /*
            if (pkg == null)
            {
                throw new ArgumentNullException(paramName: "pkg");
            }
            */


            List<string> dirsToDelete = new List<string>();

            var dirName = Path.Combine("C:\\code\\temp\\installtestpath", pkgName);
            var versionDirs = Directory.GetDirectories(dirName);
            var parentDirFiles = Directory.GetFiles(dirName);


            // First check if module is installed (GET INSTALLED PKG)
            // If the pkg name isn't valid, or if the pkg directory doesn't exist, or if there's nothing in the pkg directory, simply return
            if (String.IsNullOrWhiteSpace(pkgName) || !Directory.Exists(dirName) || (versionDirs == null && parentDirFiles == null))
            {
                return;
            }

            // If prereleaseOnly is specified, we'll only take into account prerelease versions of pkgs
            if (_prereleaseOnly)
            {
                List<string> prereleaseOnlyVersionDirs = new List<string>();
                foreach (var dir in versionDirs)
                {
                    var nameOfDir = Path.GetFileName(dir);
                    var nugVersion = NuGetVersion.Parse(nameOfDir);

                    if (nugVersion.IsPrerelease)
                    {
                        prereleaseOnlyVersionDirs.Add(dir);
                    }
                }
                versionDirs = prereleaseOnlyVersionDirs.ToArray();
            }
           

            // if the version specificed is a version range
            if (versionRange != null)
            {
            
                foreach (var versionDirPath in versionDirs)
                {
                    var nameOfDir = Path.GetFileName(versionDirPath);
                    var nugVersion = NuGetVersion.Parse(nameOfDir);

                    if (versionRange.Satisfies(nugVersion))
                    {
                        dirsToDelete.Add(versionDirPath);
                    }
                }
            }
            else if (nugetVersion != null)
            {
                // if the version specified is a version
                
                dirsToDelete.Add(nugetVersion.ToNormalizedString());
            }
            else
            {
                // if no version is specified, just delete the latest version
                Array.Sort(versionDirs);

                dirsToDelete.Add(versionDirs[versionDirs.Length - 1]);
            }



            /// This is a primitive implementation
            /// TODO:  implement a dependencies database for querying dependency info
            /// Cannot uninstall a module if another module is dependent on it 
        
            using (System.Management.Automation.PowerShell pwsh = System.Management.Automation.PowerShell.Create())
            {
                // Check all modules for dependencies
                var results = pwsh.AddCommand("Get-Module").AddParameter("ListAvailable").Invoke();

                // Structure of LINQ call:
                // Results is a collection of PSModuleInfo objects that contain a property listing module dependencies, "RequiredModules".
                // RequiredModules is collection of PSModuleInfo objects that need to be iterated through to see if any of them are the pkg we're trying to uninstall
                // If we anything from the final call gets returned, there is a dependency on this pkg.
                var pkgsWithRequiredModules = results.Where(p => ((ReadOnlyCollection<PSModuleInfo>)p.Properties["RequiredModules"].Value).Where(rm => rm.Name.Equals(pkgName)).Any());


                //.Select(p => (p.Properties.Match("Name"), p.Properties.Match("Version")));

                if (pkgsWithRequiredModules.Any())
                {
                    var uniquePkgNames = pkgsWithRequiredModules.Select(p => p.Properties["Name"].Value).Distinct().ToArray();

                    var strUniquePkgNames = string.Join(",", uniquePkgNames);

                    throw new System.ArgumentException(string.Format(CultureInfo.InvariantCulture, "Cannot uninstall {0}, the following package(s) take a dependency on this package: {1}", pkgName, strUniquePkgNames));

                }
            }



            /*
            using (StreamReader sr = new StreamReader("c:\\code\\temp\\installtestpath\\PSGetModuleInfo.xml"))
            {

                string text = sr.ReadToEnd();
                var deserializedObj = (PSObject)PSSerializer.Deserialize(text);

                var dependencyInfo = deserializedObj.Properties.Match("Dependencies");

            };
            */



            
            // Delete the appropriate directories
            foreach (var dirVersion in dirsToDelete)
            {
                var dirNameVersion = Path.Combine(dirName, dirVersion);

                if (Directory.Exists(dirName))
                {
                    Directory.Delete(dirNameVersion.ToString(), true);
                }
            }


          
            // Finally:
            // Check to see if there's anything left in the parent directory, if not, delete that as well
            if (Directory.GetDirectories(dirName).Length == 0)
            {
                Directory.Delete(dirName, true);
            }

   

        }


    }
}
