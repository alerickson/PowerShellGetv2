﻿
using System;
using System.Collections;
using System.Management.Automation;
using System.Collections.Generic;
using NuGet.Configuration;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System.Threading;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System.Net;
using System.Linq;
using MoreLinq.Extensions;
using System.IO;
using Microsoft.PowerShell.PowerShellGet.RepositorySettings;
using System.Globalization;
using System.Security.Principal;

//using NuGet.Protocol.Core.Types;

namespace Microsoft.PowerShell.PowerShellGet.Cmdlets
{


    /// <summary>
    /// The Install-PSResource cmdlet installs a resource.
    /// It returns nothing.
    /// </summary>

    [Cmdlet(VerbsLifecycle.Install, "PSResource", DefaultParameterSetName = "NameParameterSet", SupportsShouldProcess = true,
    HelpUri = "<add>", RemotingCapability = RemotingCapability.None)]
    public sealed
    class InstallPSResource : PSCmdlet
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
        /// The destination where the resource is to be installed. Works for all resource types.
        /// </summary>
        [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "NameParameterSet")]
        [ValidateNotNullOrEmpty]
        public string[] DestinationPath
        {
            get
            { return _type; }

            set
            { _type = value; }
        }
        private string[] _type;

        /// <summary>
        /// Specifies the version or version range of the package to be installed
        /// </summary>
        [Parameter(ParameterSetName = "NameParameterSet")]
        [ValidateNotNullOrEmpty]
        public string Version
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
        /// Specifies a user account that has rights to find a resource from a specific repository.
        /// </summary>
        [Parameter(ParameterSetName = "NameParameterSet")]
        [ValidateNotNullOrEmpty]
        public string[] Repository
        {
            get
            { return _repository; }

            set
            { _repository = value; }
        }
        private string[] _repository;

        /// <summary>
        /// Specifies the type of the resource to be searched for.
        /// </summary>
        [Parameter(ValueFromPipeline = true, ParameterSetName = "NameParameterSet")]
        [ValidateNotNull]
        public string[] Tags
        {
            get
            { return _tags; }

            set
            { _tags = value; }
        }
        private string[] _tags;

        /// <summary>
        /// Specify which repositories to search in.
        /// </summary>
        [ValidateNotNullOrEmpty]
        [Parameter(ValueFromPipeline = true, ParameterSetName = "NameParameterSet")]
        public string[] Repositories
        {
            get { return _repositories; }

            set { _repositories = value; }
        }
        private string[] _repositories;

        /// <summary>
        /// Specifies a user account that has rights to find a resource from a specific repository.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "NameParameterSet")]
        public PSCredential Credential
        {
            get
            { return _credential; }

            set
            { _credential = value; }
        }
        private PSCredential _credential;

        /// <summary>
        /// Specifies to return any dependency packages.
        /// Currently only used when name param is specified.
        /// </summary>
        [Parameter()]
        [ValidateSet("CurrentUser", "AllUsers")]
        public string Scope
        {
            get { return _scope; }

            set { _scope = value; }
        }
        private string _scope;


        /// <summary>
        /// Overrides warning messages about installation conflicts about existing commands on a computer.
        /// Overwrites existing commands that have the same name as commands being installed by a module. AllowClobber and Force can be used together in an Install-Module command.
        /// Prevents installing modules that have the same cmdlets as a differently named module already
        /// </summary>
        [Parameter()]
        public SwitchParameter NoClobber
        {
            get { return _noClobber; }

            set { _noClobber = value; }
        }
        private SwitchParameter _noClobber;


        /// <summary>
        /// Suppresses being prompted if the publisher of the resource is different from the currently installed version.
        /// </summary>
        [Parameter()]
        public SwitchParameter IgnoreDifferentPublisher
        {
            get { return _ignoreDifferentPublisher; }

            set { _ignoreDifferentPublisher = value; }
        }
        private SwitchParameter _ignoreDifferentPublisher;


        /// <summary>
        /// Suppresses being prompted for untrusted sources.
        /// </summary>
        [Parameter()]
        public SwitchParameter TrustRepository
        {
            get { return _trustRepository; }

            set { _trustRepository = value; }
        }
        private SwitchParameter _trustRepository;

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


        /// <summary>
        /// Overwrites a previously installed resource with the same name and version.
        /// </summary>
        [Parameter()]
        public SwitchParameter Reinstall
        {
            get { return _reinstall; }

            set { _reinstall = value; }
        }
        private SwitchParameter _reinstall;


        /// <summary>
        /// Suppresses progress information.
        /// </summary>
        [Parameter()]
        public SwitchParameter Quiet
        {
            get { return _quiet; }

            set { _quiet = value; }
        }
        private SwitchParameter _quiet;


        /// <summary>
        /// For modules that require a license, AcceptLicense automatically accepts the license agreement during installation.
        /// </summary>
        [Parameter()]
        public SwitchParameter AcceptLicense
        {
            get { return _acceptLicense; }

            set { _acceptLicense = value; }
        }
        private SwitchParameter _acceptLicense;


        /// <summary>
        /// Returns the resource as an object to the console.
        /// </summary>
        [Parameter()]
        public SwitchParameter PassThru
        {
            get { return _passThru; }

            set { _passThru = value; }
        }
        private SwitchParameter _passThru;


        // This will be a list of all the repository caches
        public static readonly List<string> RepoCacheFileName = new List<string>();
        // Temporarily store cache in this path for testing purposes
        public static readonly string RepositoryCacheDir = "c:/code/temp/repositorycache"; //@"%APPDTA%\NuGet";

        // find the env variable for this
        private static readonly string psModulesPath = "C:/code/temp/installtestpath";
        private readonly List<string> psModulesPathAllDirs = (Directory.GetDirectories(psModulesPath)).ToList();

        // Define the cancellation token.
        CancellationTokenSource source;
        CancellationToken cancellationToken;


        /// <summary>
        /// </summary>
        protected override void ProcessRecord()
        {
            source = new CancellationTokenSource();
            cancellationToken = source.Token;

            var id = WindowsIdentity.GetCurrent();
            var consoleIsElevated = (id.Owner != id.User);


            // if Scope is AllUsers and there is no console elevation
            if (!string.IsNullOrEmpty(_scope) && _scope.Equals("AllUsers") && !consoleIsElevated)
            {
                // throw an error when Install-PSResource is used as a non-admin user and '-Scope AllUsers'
                throw new System.ArgumentException(string.Format(CultureInfo.InvariantCulture, "Install-PSResource requires admin privilege for AllUsers scope."));
            }

            // if no scope is specified (whether or not the console is elevated) default installation will be to CurrentUser
            // If running under admin on Windows with PowerShell less than PS6, default will be AllUsers
            if (string.IsNullOrEmpty(_scope))
            {
                _scope = "CurrentUser";
                if (!Platform.IsCoreCLR && consoleIsElevated)
                {
                    _scope = "AllUsers";
                }
            }
            // if scope is Current user & (no elevation or elevation)
            // install to current user path




            var r = new RespositorySettings();


            var listOfRepositories = r.Read(_repository);

            InstallHelper(listOfRepositories[0].Properties["Url"].Value.ToString(), cancellationToken);

           // foreach (var repoName in listOfRepositories)
            //{
            //    if (InstallHelper(repoName.Properties["Url"].Value.ToString(), cancellationToken))
            //    {

            //    }
            //}

        }





        // Installing a package will have a transactional behavior:
        // Package and its dependencies will be saved into a tmp folder
        // and will only be properly installed if all dependencies are found successfully
        // Once package is installed, we want to resolve and install all dependencies
        // Installing


        public void InstallHelper(string repositoryUrl, CancellationToken cancellationToken)
        {


            PackageSource source = new PackageSource(repositoryUrl);

            if (_credential != null)
            {
                string password = new NetworkCredential(string.Empty, _credential.Password).Password;
                source.Credentials = PackageSourceCredential.FromUserInput(repositoryUrl, _credential.UserName, password, true, null);
            }

            var provider = FactoryExtensionsV3.GetCoreV3(NuGet.Protocol.Core.Types.Repository.Provider);

            SourceRepository repository = new SourceRepository(source, provider);

            SearchFilter filter = new SearchFilter(_prerelease);
            //SourceCacheContext context = new SourceCacheContext();




            // I think we'll likely need to make a call to find first so that we can ensure we have the right version (if no version is specified)
            // Call Find

            // things we know:  at least one name,  we don't know versions though

            // returnedPkgs

            // IDEA:  use find to find all dependencies...



            //////////////////////  packages from source
            ///
            PackageSource source2 = new PackageSource(repositoryUrl);
            if (_credential != null)
            {
                string password = new NetworkCredential(string.Empty, _credential.Password).Password;
                source2.Credentials = PackageSourceCredential.FromUserInput(repositoryUrl, _credential.UserName, password, true, null);
            }
            var provider2 = FactoryExtensionsV3.GetCoreV3(NuGet.Protocol.Core.Types.Repository.Provider);

            SourceRepository repository2 = new SourceRepository(source2, provider2);
            PackageMetadataResource resourceMetadata2 = repository.GetResourceAsync<PackageMetadataResource>().GetAwaiter().GetResult();

            SearchFilter filter2 = new SearchFilter(_prerelease);
            SourceCacheContext context2 = new SourceCacheContext();



            foreach (var n in _name)
            {
                // returnedPkgs.AddRange(FindPackagesFromSourceHelper(repositoryUrl, n, resourceSearch, resourceMetadata, filter, context));

                //functionality here
                //////////////////    packages from source helper

                IPackageSearchMetadata filteredFoundPkgs = null;


                // Check version first to narrow down the number of pkgs before potential searching through tags


                VersionRange versionRange = null;
                if (_version == null)
                {
                    // ensure that the latst version is returned first (the ordering of versions differ
                    filteredFoundPkgs = (resourceMetadata2.GetMetadataAsync(n, _prerelease, false, context2, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult()
                        .OrderByDescending(p => p.Identity.Version, VersionComparer.VersionRelease)
                        .FirstOrDefault());
                }
                else
                {
                    // check if exact version
                    NuGetVersion nugetVersion;

                    //VersionRange versionRange = VersionRange.Parse(version);
                    NuGetVersion.TryParse(_version, out nugetVersion);
                    // throw

                    if (nugetVersion != null)
                    {
                        // exact version
                        versionRange = new VersionRange(nugetVersion, true, nugetVersion, true, null, null);
                    }
                    else
                    {
                        // check if version range
                        versionRange = VersionRange.Parse(_version);
                    }



                    // Search for packages within a version range
                    // ensure that the latst version is returned first (the ordering of versions differ
                    filteredFoundPkgs = (resourceMetadata2.GetMetadataAsync(n, _prerelease, false, context2, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult()
                        .Where(p => versionRange.Satisfies(p.Identity.Version))
                        .OrderByDescending(p => p.Identity.Version, VersionComparer.VersionRelease)
                        .FirstOrDefault());

                }


                List<IPackageSearchMetadata> foundDependencies = new List<IPackageSearchMetadata>();



                // found a package to install and looking for dependencies
                // Search for dependencies
                if (filteredFoundPkgs != null)
                {
                   

                    // need to parse the depenency and version and such

                    // need to improve this later
                    // this function recursively finds all dependencies
                    // might need to do add instead of AddRange
                    foundDependencies.AddRange(FindDependenciesFromSource(filteredFoundPkgs, resourceMetadata2, context2));


                }  /// end dep conditional



                // check which pkgs you actually need to install




                List<IPackageSearchMetadata> pkgsToInstall = new List<IPackageSearchMetadata>();

                // install pkg, then install any dependencies to a temp directory

                pkgsToInstall.Add(filteredFoundPkgs);
                pkgsToInstall.AddRange(foundDependencies);



                /*** NEED TO CHECK IF PKGS ARE ALREADY INSTALLED ***/
                // - we have a list of everything that needs to be installed (dirsToDelete)
                // - we check the system to see if that particular package AND package version is there (PSModulesPath)
                // - if it is, we remove it from the list of pkgs to install

                if (versionRange != null)
                {
                    // for each package name passed in
                    foreach (var name in _name)
                    {
                        var pkgDirName = Path.Combine(psModulesPath, name);

                        // Check to see if the package dir exists in the path
                        if (psModulesPathAllDirs.Contains(pkgDirName, StringComparer.OrdinalIgnoreCase))
                        {
                            // then check to see if the package version exists in the path
                            var pkgDirVersion = (Directory.GetDirectories(pkgDirName)).ToList();
                            List<string> pkgVersion = new List<string>();
                            foreach (var path in pkgDirVersion)
                            {
                                pkgVersion.Add(Path.GetFileName(path));
                            }


                            // these are all the packages already installed
                            var pkgsAlreadyInstalled = pkgVersion.FindAll(p => versionRange.Satisfies(NuGetVersion.Parse(p)));


                            if (pkgsAlreadyInstalled.Any())
                            {
                                // remove the pkg from the list of pkgs that need to be installed
                                var pkgsToRemove = pkgsToInstall.Find(p => string.Equals(p.Identity.Id, name, StringComparison.CurrentCultureIgnoreCase));

                                pkgsToInstall.Remove(pkgsToRemove);
                            }

                        }
                    }
                } // exact version installation
                else // if (versionRange != null)
                {
                    // for each package name passed in
                    foreach (var name in _name)
                    {
                        // case sensitivity issues here!
                        var dirName = Path.Combine(psModulesPath, name);

                        // Check to see if the package dir exists in the path
                        if (psModulesPathAllDirs.Contains(dirName, StringComparer.OrdinalIgnoreCase))
                        {
                            // then check to see if the package exists in the path

                            if (Directory.Exists(dirName))
                            {
                                // remove the pkg from the list of pkgs that need to be installed
                                //case sensitivity here 
                                var pkgsToRemove = pkgsToInstall.Find(p => string.Equals(p.Identity.Id, name, StringComparison.CurrentCultureIgnoreCase));

                                pkgsToInstall.Remove(pkgsToRemove);
                                //Directory.Delete(dirNameVersion.ToString(), true);

                            }

                        }
                    }
                }
                



                var installPath = "C:/code/temp/installtestpath";


                // install everything to a temp path
                foreach (var p in pkgsToInstall)
                {
                    var pkgIdentity = new PackageIdentity(p.Identity.Id, p.Identity.Version);


                    var resource = new DownloadResourceV2FeedProvider();
                    var resource2 = resource.TryCreate(repository, cancellationToken);

                    // Act
                    var cacheContext = new SourceCacheContext();


                    var downloadResource = repository.GetResourceAsync<DownloadResource>().GetAwaiter().GetResult();


                    var result = downloadResource.GetDownloadResourceResultAsync(
                        pkgIdentity,
                        new PackageDownloadContext(cacheContext),
                        installPath,
                        logger: NullLogger.Instance,
                        CancellationToken.None).GetAwaiter().GetResult();

                    // need to close the .nupkg
                    result.Dispose();

                    // create a download count to see if everything was installed properly

                    // 1) remove the *.nupkg file

                    // may need to modify due to capitalization
                    var dirNameVersion = Path.Combine(installPath, p.Identity.Id, p.Identity.Version.ToNormalizedString());
                    var nupkgMetadataToDelete = Path.Combine(dirNameVersion, ".nupkg.metadata");
                    var nupkgToDelete = Path.Combine(dirNameVersion, (p.Identity.Id + "." + p.Identity.Version + ".nupkg").ToLower());
                    var nupkgSHAToDelete = Path.Combine(dirNameVersion, (p.Identity.Id + "." + p.Identity.Version + ".nupkg.sha512").ToLower());
                    var nuspecToDelete = Path.Combine(dirNameVersion, (p.Identity.Id + ".nuspec").ToLower());


                    File.Delete(nupkgMetadataToDelete);
                    File.Delete(nupkgSHAToDelete);
                    File.Delete(nuspecToDelete);
                    File.Delete(nupkgToDelete);



                    // 2) Verify that all the proper modules installed correctly and


                    // 3) create xml
                    //Create PSGetModuleInfo.xml
                    //Set attribute as hidden [System.IO.File]::SetAttributes($psgetItemInfopath, [System.IO.FileAttributes]::Hidden)


                    var fullinstallPath = Path.Combine(dirNameVersion, "PSGetModuleInfo.xml");
                        
                    // Create XMLs
                    using (StreamWriter sw = new StreamWriter(fullinstallPath))
                    {
                        var psModule = "PSModule";

                        var tags = p.Tags.Split(" ", StringSplitOptions.RemoveEmptyEntries);


                        var module = tags.Contains("PSModule") ? "Module" : null;
                        var script = tags.Contains("PSScript") ? "Script" : null;


                        List<string> includesDscResource = new List<string>();
                        List<string> includesCommand = new List<string>();
                        List<string> includesFunction = new List<string>();
                        List<string> includesRoleCapability = new List<string>();
                        List<string> filteredTags = new List<string>();

                        var psDscResource = "PSDscResource_";
                        var psCommand = "PSCommand_";
                        var psFunction = "PSFunction_";
                        var psRoleCapability = "PSRoleCapability_";



                        foreach (var tag in tags)
                        {
                            if (tag.StartsWith(psDscResource))
                            {
                                includesDscResource.Add(tag.Remove(0, psDscResource.Length));
                            }
                            else if (tag.StartsWith(psCommand))
                            {
                                includesCommand.Add(tag.Remove(0, psCommand.Length));
                            }
                            else if (tag.StartsWith(psFunction))
                            {
                                includesFunction.Add(tag.Remove(0, psFunction.Length));
                            }
                            else if (tag.StartsWith(psRoleCapability))
                            {
                                includesRoleCapability.Add(tag.Remove(0, psRoleCapability.Length));
                            }
                            else if (!tag.StartsWith("PSWorkflow_") && !tag.StartsWith("PSCmdlet_") && !tag.StartsWith("PSIncludes_")
                                && !tag.Equals("PSModule") && !tag.Equals("PSScript"))
                            {
                                filteredTags.Add(tag);
                            }
                        }

                        Dictionary<string, List<string>> includes = new Dictionary<string, List<string>>() {
                            { "DscResource", includesDscResource },
                            { "Command", includesCommand },
                            { "Function", includesFunction },
                            { "RoleCapability", includesRoleCapability }
                        };


                        Dictionary<string, VersionRange> dependencies = new Dictionary<string, VersionRange>();
                        foreach (var depGroup in p.DependencySets)
                        {
                            PackageDependency depPkg = depGroup.Packages.FirstOrDefault();
                            dependencies.Add(depPkg.Id, depPkg.VersionRange);
                        }


                        var hash = new Hashtable()
                            {
                                {"Name", p.Identity.Id },
                                {"Version", p.Identity.Version },
                                {"Type",  module != null ? module : (script != null? script : null) },
                                {"Description", p.Description },
                                {"Author", p.Authors},
                                {"CompanyName", p.Owners },
                                {"PublishedDate", p.Published },
                                {"InstalledDate", System.DateTime.Now },
                                {"LicenseUri", p.LicenseUrl },
                                {"ProjectUri", p.ProjectUrl },
                                {"IconUri", p.IconUrl },
                                {"Tags", filteredTags },
                                {"Includes", includes.ToList() },                       ////  NOT GETTING DESERIALIZED PROPERLY
                                {"PowerShellGetFormatVersion", "3" },
                                {"ReleaseNotes", ""},                                   // TODO:  add release notes
                                {"Dependencies", dependencies.ToList() },
                                {"RepositorySourceLocation", repositoryUrl },
                                {"Repository", repositoryUrl },                         // TODO:  potentially change to repository name later, don't think this is necessary though
                                {"InstalledLocation", "" }                              // TODO:  add installation location
                            };



                        var psGetModuleInfoObj = new PSObject(hash);

                        psGetModuleInfoObj.TypeNames.Add("Microsoft.PowerShell.Commands.PSRepositoryItemInfo");


                        var serializedObj = PSSerializer.Serialize(psGetModuleInfoObj);
                        //PSObject deserializedObj = (PSObject) PSSerializer.Deserialize(serializedObj);





                        sw.Write(serializedObj);

                        // set the xml attribute to hidden
                        //System.IO.File.SetAttributes("c:\\code\\temp\\installtestpath\\PSGetModuleInfo.xml", FileAttributes.Hidden);

                        // 4) copy to proper path
                    }
                }


            }
            ////////////////////////////////////////










        }



        private List<IPackageSearchMetadata> FindDependenciesFromSource(IPackageSearchMetadata pkg, PackageMetadataResource pkgMetadataResource, SourceCacheContext srcContext)
        {
            /// dependency resolver
            ///
            /// this function will be recursively called
            ///
            /// call the findpackages from source helper (potentially generalize this so it's finding packages from source or cache)
            ///
            List<IPackageSearchMetadata> foundDependencies = new List<IPackageSearchMetadata>();

            // 1)  check the dependencies of this pkg
            // 2) for each dependency group, search for the appropriate name and version
            // a dependency group are all the dependencies for a particular framework
            foreach (var dependencyGroup in pkg.DependencySets)
            {

                //dependencyGroup.TargetFramework
                //dependencyGroup.

                foreach (var pkgDependency in dependencyGroup.Packages)
                {

                    // 2.1) check that the appropriate pkg dependencies exist
                    // returns all versions from a single package id.
                    var dependencies = pkgMetadataResource.GetMetadataAsync(pkgDependency.Id, _prerelease, true, srcContext, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult();

                    // then 2.2) check if the appropriate verion range exists  (if version exists, then add it to the list to return)

                    VersionRange versionRange = null;
                    try
                    {
                        versionRange = VersionRange.Parse(pkgDependency.VersionRange.OriginalString);
                    }
                    catch
                    {
                        Console.WriteLine("Error parsing version range");
                    }



                    // if no version/version range is specified the we just return the latest version

                    IPackageSearchMetadata depPkgToReturn = (versionRange == null ?
                        dependencies.FirstOrDefault() :
                        dependencies.Where(v => versionRange.Satisfies(v.Identity.Version)).FirstOrDefault());




                    // if the pkg already exists on the system, don't add it to the list of pkgs that need to be installed 
                    var dirName = Path.Combine(psModulesPath, pkgDependency.Id);

                    var dependencyAlreadyInstalled = false;
                    
                    // Check to see if the package dir exists in the path
                    if (psModulesPathAllDirs.Contains(dirName, StringComparer.OrdinalIgnoreCase))
                    {
                        // then check to see if the package exists in the path

                        if (Directory.Exists(dirName))
                        {
                            // then check to see if the package version exists in the path
                            var pkgDirVersion = (Directory.GetDirectories(dirName)).ToList();
                            List<string> pkgVersion = new List<string>();
                            foreach (var path in pkgDirVersion)
                            {
                                pkgVersion.Add(Path.GetFileName(path));
                            }


                            // these are all the packages already installed
                            var pkgsAlreadyInstalled = pkgVersion.FindAll(p => versionRange.Satisfies(NuGetVersion.Parse(p)));  // dont think ive tested this path

                            if (pkgsAlreadyInstalled.Any())
                            {
                                // don't add the pkg to the list of pkgs that need to be installed
                                dependencyAlreadyInstalled = true;
                            }
                        }

                    }

                    if (!dependencyAlreadyInstalled)
                    {
                        foundDependencies.Add(depPkgToReturn);
                    }

                    // 3) search for any dependencies the pkg has
                    foundDependencies.AddRange(FindDependenciesFromSource(depPkgToReturn, pkgMetadataResource, srcContext));
                }
            }




            // flatten after returning
            return foundDependencies;
        }












    }
}
