
using System;
using System.Management.Automation;
using System.Threading;
using NuGet.Versioning;
using System.IO;



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
        /// Used for pipeline input.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "InputObjectSet")]
        [ValidateNotNullOrEmpty]
        public PSCustomObject[] InputObject
        {
            get
            { return _inputObject; }

            set
            { _inputObject = value; }
        }
        private PSCustomObject[] _inputObject; // = new string[0];



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
        /// Specifies to allow installation of prerelease versions
        /// </summary>
        [Parameter(ParameterSetName = "NameParameterSet")]
        public SwitchParameter Prerelease
        {
            get
            { return _prerelease; }

            set
            { _prerelease = value; }
        }
        private SwitchParameter _prerelease;

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



        /// <summary>
        /// </summary>
        protected override void ProcessRecord()
        {
            source = new CancellationTokenSource();
            cancellationToken = source.Token;

            NuGetVersion nugetVersion;
            var bleh = NuGetVersion.TryParse(_version, out nugetVersion);

            foreach (var pkgName in _name)
            {
                UninstallHelper(pkgName, nugetVersion, cancellationToken);
            }

        }



        /// just unintall module, not dependencies


        private void UninstallHelper(string pkgName, NuGetVersion nugetVersion, CancellationToken cancellationToken)
        {
            // scope, admin rightsget

            // GET INSTALLED PKG

            // get the latest version from this as well


            /*
            if (pkg == null)
            {
                throw new ArgumentNullException(paramName: "pkg");
            }
            */

            var dirName = Path.Combine("C:/code/temp/installtestpath", pkgName);


            //  If version specified is *, delete the entire pkg directory
            if (_version.Equals("*") && _prerelease)
            {
                Console.WriteLine("*");
                Directory.Delete(dirName, true);
            }



            // check versions
            // if version is specified, delete that version or version range,
//            var dirNameVersion = Path.Combine(dirName, pVersion);


            // if version is NOT specified, delete the most recent version


//            if (String.IsNullOrWhiteSpace(dirNameVersion) || !Directory.Exists(dirNameVersion))
//            {
//                return;
//            }



//            if (Directory.GetDirectories(dirName).Length > 1)
//            {
                // If there's more than one version in the pkg Name directory, just delete the specific version
//                Directory.Delete(dirNameVersion, true);
//            }
//            else
//            {
                // Otherwise delete 
//                Directory.Delete(dirName, true);
//            }




        }













    }
}
