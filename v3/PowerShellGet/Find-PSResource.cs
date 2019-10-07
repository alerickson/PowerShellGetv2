// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using System;
using System.Collections;
using System.Management.Automation;
using System.Collections.Generic;
using NuGet.Configuration;

using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System.Threading.Tasks;
using System.Threading;


//using NuGet.Protocol.Core.Types;

namespace Microsoft.PowerShell.PowerShellGet.Cmdlets
{

    /// <summary>
    /// The Register-PSResourceRepository cmdlet registers the default repository for PowerShell modules.
    /// After a repository is registered, you can reference it from the Find-PSResource, Install-PSResource, and Publish-PSResource cmdlets.
    /// The registered repository becomes the default repository in Find-Module and Install-Module.
    /// It returns nothing.
    /// </summary>

    [Cmdlet(VerbsCommon.Find, "PSResource", DefaultParameterSetName = "NameParameterSet", SupportsShouldProcess = true,
        HelpUri = "<add>", RemotingCapability = RemotingCapability.None)]
    public sealed
    class FindPSResource : PSCmdlet
    {
       //  private string PSGalleryRepoName = "PSGallery";

        /// <summary>
        /// Specifies the desired name for the repository to be registered.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "NameParameterSet")]
        [ValidateNotNullOrEmpty]
        public string[] Name
        {
            get
            { return _name; }

            set
            { _name = value; }
        }
        private string[] _name;

        /// <summary>
        /// Specifies the location of the repository to be registered.
        /// </summary>
        [Parameter(ParameterSetName = "NameParameterSet")]
        [ValidateNotNullOrEmpty]
        public Uri URL
        {
            get
            { return _url; }

            set
            { _url = value; }
        }

        private Uri _url;

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
        /// Registers the PowerShell Gallery.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "PSGalleryParameterSet")]
        public SwitchParameter PSGallery
        {
            get
            { return _psgallery; }

            set
            { _psgallery = value; }
        }
        private SwitchParameter _psgallery;


        /// <summary>
        /// Repositories is a hashtable and is used to register multiple repositories at once.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "RepositoriesParameterSet")]
        [ValidateNotNullOrEmpty]
        public List<Hashtable> Repositories
        {
            get { return _repositories; }

            set { _repositories = value; }
        }

        private List<Hashtable> _repositories;


        /// <summary>
        /// Specifies whether the repository should be trusted.
        /// </summary>
        [Parameter(ParameterSetName = "PSGalleryParameterSet")]
        [Parameter(ParameterSetName = "NameParameterSet")]
        public SwitchParameter Trusted
        {
            get { return _trusted; }

            set { _trusted = value; }
        }

        private SwitchParameter _trusted;


        /// <summary>
        /// Specifies a proxy server for the request, rather than a direct connection to the internet resource.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public Uri Proxy
        {
            get
            { return _proxy; }

            set
            { _proxy = value; }
        }
        private Uri _proxy;



        /// <summary>
        /// Specifies a user account that has permission to use the proxy server that is specified by the Proxy parameter.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public PSCredential ProxyCredential
        {
            get
            { return _proxyCredential; }

            set
            { _proxyCredential = value; }
        }
        private PSCredential _proxyCredential;


        /// <summary>
        /// The following is the definition of the input parameter "Port".
        /// Specifies the port to be used when connecting to the ws management service.
        /// </summary>
        [Parameter(ParameterSetName = "PSGalleryParameterSet")]
        [Parameter(ParameterSetName = "NameParameterSet")]
        [ValidateNotNullOrEmpty]
        [ValidateRange(0, 50)]
        public int Priority
        {
            get { return _priority; }

            set { _priority = value; }
        }


        private int _priority = 50;


        // This will be a list of all the repository caches
        public static readonly List<string> RepoCacheFileName = new List<string>();
        // Temporarily store cache in this path for testing purposes
        public static readonly string RepositoryCacheDir = "c:/code/temp/repositorycache"; //@"%APPDTA%\NuGet";

        /// <summary>
        /// </summary>
        protected override void BeginProcessing()
        {        
        }


        /// <summary>
        /// </summary>
        protected override void ProcessRecord()  
        {
            Console.WriteLine("in process Record");


            await TempFunctionAsync();

        }

        /// <summary>
        /// </summary>
        protected override void EndProcessing()
        {
            /// Currently no processing for end
        }


        private async Task somefunctionAsync() {

            Console.WriteLine("in somefuncAsync - beginning");

            await TempFunctionAsync();

            Console.WriteLine("in somefuncAsync - end");
        }


        private async Task TempFunctionAsync()
        {


            /* modify this to accomodate our repository settings */

            Console.WriteLine("In TempFunctionAsync");


            // I think we need to create a source repository first (is a source repository an instance of a repository?)
            // CreateSource should return a new source repository: return new SourceRepository(new PackageSource(sourceUrl), providers.Concat(new Lazy<INuGetResourceProvider>[] { handler }));
            // returns a new source repository 
            var sourceUrl = "https://www.powershellgallery.com/api/v2";

            // public SourceRepository(PackageSource source, IEnumerable<INuGetResourceProvider> providers)
            // need IEnumerable<INuGetResourceProvider> providers
            // public static ISourceRepositoryProvider CreateProvider(IEnumerable<INuGetResourceProvider> resourceProviders)

            // need to create a resource provider... right now I don't believe we actually  need a resource provider though.
            var resourceProviders = new List<Lazy<INuGetResourceProvider>>();

            // I need to figure out what the providers are :  _resourceProviders = Repository.Provider.GetVisualStudio().Concat(resourceProviders);
            // Think it's possible you might need to create a repository object?

            // see NuGetPackageManagerTests.cs for more info
            var repo = new SourceRepository(new PackageSource(sourceUrl), resourceProviders);   // providers.Concat(new Lazy<INuGetResourceProvider>[] { handler }));
            // This 'repo' var should return...  


            // this seems to be getting 
            var packageSearchResource = await repo.GetResourceAsync<PackageSearchResource>();

            var searchFilter = new SearchFilter(includePrerelease: false);
            var searchResult = await packageSearchResource.SearchAsync("azure", searchFilter, 0, 1, NullLogger.Instance, CancellationToken.None);

            if (searchResult == null)
            {
                Console.WriteLine("search result is null");
            }
            else
            {
                Console.WriteLine("search result is NOT null");
            }

            //searchResult.Count();
            //var package = searchResult.FirstOrDefault(); // to json?









            //var searchResult = await packageSearchResource.SearchAsync("azure", searchFilter, 0, 1, NullLogger.Instance, CancellationToken.None);

            // public override async Task<IEnumerable<IPackageSearchMetadata>> SearchAsync(string searchTerm, SearchFilter filters, int skip, int take, Common.ILogger log, CancellationToken cancellationToken)


            /*
            var packageId = "cake.nuget";
            var version = "0.30.0";
            var framework = "net46";

            var package = new PackageIdentity(packageId, NuGetVersion.Parse(version));

            var settings = Settings.LoadDefaultSettings(root: null);
            var pkgSrcProvider = new PackageSourceProvider(settings);
            var sourceRepositoryProvider = new SourceRepositoryProvider(pkgSrcProvider, Repository.Provider.GetCoreV3());
            var nuGetFramework = NuGetFramework.ParseFolder(framework);
            var logger = NullLogger.Instance;

            // my addition
            var allrepos = sourceRepositoryProvider.GetRepositories();
           
            //

            using (var cacheContext = new SourceCacheContext())
            {
                foreach (var sourceRepository in sourceRepositoryProvider.GetRepositories())
                {
                    var dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>();
                    var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                        package, nuGetFramework, cacheContext, logger, CancellationToken.None);

                    if (dependencyInfo != null)
                    {
                        Console.WriteLine(dependencyInfo);
                        return;
                    }
                }
            }
            */
        }

    }
}
