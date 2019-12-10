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
using LinqKit;
using Microsoft.PowerShell.PowerShellGet;
using Microsoft.PowerShell.PowerShellGet.RepositorySettings;
using MoreLinq.Extensions;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        /// Specifies the desired name for the resource to be searched.
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
        private string[] _name; // = new string[0];

        /// <summary>
        /// Specifies the type of the resource to be searched for. 
        /// </summary>
        [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateSet(new string[] { "Module", "Script", "DscResource", "RoleCapability", "Command" })]
        public string[] Type
        {
            get
            { return _type; }

            set
            { _type = value; }
        }
        private string[] _type;

        /// <summary>
        /// Specifies the version or version range of the package to be searched for
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
        /// Specifies to search for prerelease resources.
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
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "NameParameterSet")]
        [ValidateNotNullOrEmpty]
        public string ModuleName
        {
            get
            { return _moduleName; }

            set
            { _moduleName = value; }
        }
        private string _moduleName;

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
        /// </summary>
        public SwitchParameter IncludeDependencies
        {
            get { return _includeDependencies; }

            set { _includeDependencies = value; }
        }
        private SwitchParameter _includeDependencies;




        // This will be a list of all the repository caches
        public static readonly List<string> RepoCacheFileName = new List<string>();
        // Temporarily store cache in this path for testing purposes
        public static readonly string RepositoryCacheDir = "c:/code/temp/repositorycache"; //@"%APPDTA%\NuGet";



        /// <summary>
        /// </summary>
        protected override void ProcessRecord()  
        {


     

        }



        private IEnumerable<DataRow> FindPackageFromCache(string repositoryName)
        {
            DataSet cachetables = new CacheSettings().CreateDataTable(repositoryName);

            return FindPackageFromCacheHelper(cachetables, repositoryName); 
        }


        private IEnumerable<DataRow> FindPackageFromCacheHelper(DataSet cachetables, string repositoryName)
        {
            DataTable metadataTable = cachetables.Tables[0];
            DataTable tagsTable = cachetables.Tables[1];
            DataTable dependenciesTable = cachetables.Tables[2];
            DataTable commandsTable = cachetables.Tables[3];
            DataTable dscResourceTable = cachetables.Tables[4];
            DataTable roleCapabilityTable = cachetables.Tables[5];

            DataTable queryTable = new DataTable();
            var predicate = PredicateBuilder.New<DataRow>(true);


            if ((_tags != null) && (_tags.Length != 0))
            {

                var tagResults = from t0 in metadataTable.AsEnumerable()
                            join t1 in tagsTable.AsEnumerable()
                            on t0.Field<string>("Key") equals t1.Field<string>("Key")
                            select new PackageMetadataAllTables {
                                Key = t0["Key"],
                                Name = t0["Name"],
                                Version = t0["Version"],
                                Type = (string)t0["Type"],
                                Description = t0["Description"],
                                Author = t0["Author"],
                                Copyright = t0["Copyright"],
                                PublishedDate = t0["PublishedDate"],
                                InstalledDate = t0["InstalledDate"],
                                UpdatedDate = t0["UpdatedDate"],
                                LicenseUri = t0["LicenseUri"],
                                ProjectUri = t0["ProjectUri"],
                                IconUri = t0["IconUri"],
                                PowerShellGetFormatVersion = t0["PowerShellGetFormatVersion"],
                                ReleaseNotes = t0["ReleaseNotes"],
                                RepositorySourceLocation = t0["RepositorySourceLocation"],
                                Repository = t0["Repository"],
                                IsPrerelease = t0["IsPrerelease"],
                                Tags = t1["Tags"],
                            };

                DataTable joinedTagsTable = tagResults.ToDataTable();

                var tagsPredicate = PredicateBuilder.New<DataRow>(true);

                // Build predicate by combining tag searches with 'or'
                foreach (string t in _tags)
                {
                    tagsPredicate = tagsPredicate.Or(pkg => pkg.Field<string>("Tags").Equals(t));
                }


                // final results -- All the appropriate pkgs with these tags
                var tags = joinedTagsTable.AsEnumerable().Where(tagsPredicate).Select(p => p);
 
            }
       
            if (_type != null)
            { 
                if (_type.Contains("Command", StringComparer.OrdinalIgnoreCase))
                {
                
                    var commandResults = from t0 in metadataTable.AsEnumerable()
                                join t1 in commandsTable.AsEnumerable()
                                on t0.Field<string>("Key") equals t1.Field<string>("Key")
                                select new PackageMetadataAllTables {
                                    Key = t0["Key"],
                                    Name = t0["Name"],
                                    Version = t0["Version"],
                                    Type = (string)t0["Type"],
                                    Description = t0["Description"],
                                    Author = t0["Author"],
                                    Copyright = t0["Copyright"],
                                    PublishedDate = t0["PublishedDate"],
                                    InstalledDate = t0["InstalledDate"],
                                    UpdatedDate = t0["UpdatedDate"],
                                    LicenseUri = t0["LicenseUri"],
                                    ProjectUri = t0["ProjectUri"],
                                    IconUri = t0["IconUri"],
                                    PowerShellGetFormatVersion = t0["PowerShellGetFormatVersion"],
                                    ReleaseNotes = t0["ReleaseNotes"],
                                    RepositorySourceLocation = t0["RepositorySourceLocation"],
                                    Repository = t0["Repository"],
                                    IsPrerelease = t0["IsPrerelease"],
                                    Commands = t1["Commands"],
                                };

                    DataTable joinedCommandTable = commandResults.ToDataTable();

                    var commandPredicate = PredicateBuilder.New<DataRow>(true);

                    // Build predicate by combining names of commands searches with 'or'
                    // if no name is specified, we'll return all (?)
                    foreach (string n in _name)
                    {
                        commandPredicate = commandPredicate.Or(pkg => pkg.Field<string>("Commands").Equals(n));
                    }

                    // final results -- All the appropriate pkgs with these tags
                    var commands = joinedCommandTable.AsEnumerable().Where(commandPredicate).Select(p => p);                   
                }


                if (_type.Contains("DscResource", StringComparer.OrdinalIgnoreCase))
                {
                  
                    var dscResourceResults = from t0 in metadataTable.AsEnumerable()
                             join t1 in dscResourceTable.AsEnumerable()
                             on t0.Field<string>("Key") equals t1.Field<string>("Key")
                             select new PackageMetadataAllTables
                             {
                                  Key = t0["Key"],
                                  Name = t0["Name"],
                                  Version = t0["Version"],
                                  Type = (string) t0["Type"],
                                  Description = t0["Description"],
                                  Author = t0["Author"],
                                  Copyright = t0["Copyright"],
                                  PublishedDate = t0["PublishedDate"],
                                  InstalledDate = t0["InstalledDate"],
                                  UpdatedDate = t0["UpdatedDate"],
                                  LicenseUri = t0["LicenseUri"],
                                  ProjectUri = t0["ProjectUri"],
                                  IconUri = t0["IconUri"],
                                  PowerShellGetFormatVersion = t0["PowerShellGetFormatVersion"],
                                  ReleaseNotes = t0["ReleaseNotes"],
                                  RepositorySourceLocation = t0["RepositorySourceLocation"],
                                  Repository = t0["Repository"],
                                  IsPrerelease = t0["IsPrerelease"],
                                  DscResources = t1["DscResources"],
                             };
                    
                    var dscResourcePredicate = PredicateBuilder.New<DataRow>(true);

                    DataTable joinedDscResourceTable = dscResourceResults.ToDataTable();

                    // Build predicate by combining names of commands searches with 'or'
                    // if no name is specified, we'll return all (?)
                    foreach (string n in _name)
                    {
                        dscResourcePredicate = dscResourcePredicate.Or(pkg => pkg.Field<string>("DscResources").Equals(n));
                    }

                    // final results -- All the appropriate pkgs with these tags
                    var dscResources = joinedDscResourceTable.AsEnumerable().Where(dscResourcePredicate).Select(p => p);
                }

                if (_type.Contains("RoleCapability", StringComparer.OrdinalIgnoreCase))
                {
                   
                    var roleCapabilityResults = from t0 in metadataTable.AsEnumerable()
                            join t1 in roleCapabilityTable.AsEnumerable()
                            on t0.Field<string>("Key") equals t1.Field<string>("Key")
                            select new PackageMetadataAllTables
                            {
                                Key = t0["Key"],
                                   Name = t0["Name"],
                                   Version = t0["Version"],
                                   Type = (string)t0["Type"],
                                   Description = t0["Description"],
                                   Author = t0["Author"],
                                   Copyright = t0["Copyright"],
                                   PublishedDate = t0["PublishedDate"],
                                   InstalledDate = t0["InstalledDate"],
                                   UpdatedDate = t0["UpdatedDate"],
                                   LicenseUri = t0["LicenseUri"],
                                   ProjectUri = t0["ProjectUri"],
                                   IconUri = t0["IconUri"],
                                   PowerShellGetFormatVersion = t0["PowerShellGetFormatVersion"],
                                   ReleaseNotes = t0["ReleaseNotes"],
                                   RepositorySourceLocation = t0["RepositorySourceLocation"],
                                   Repository = t0["Repository"],
                                   IsPrerelease = t0["IsPrerelease"],
                                   RoleCapability = t1["RoleCapability"],
                            };

                    var roleCapabilityPredicate = PredicateBuilder.New<DataRow>(true);

                    DataTable joinedRoleCapabilityTable = roleCapabilityResults.ToDataTable();

                    // Build predicate by combining names of commands searches with 'or'
                    // if no name is specified, we'll return all (?)
                    foreach (string n in _name)
                    {
                        roleCapabilityPredicate = roleCapabilityPredicate.Or(pkg => pkg.Field<string>("RoleCapability").Equals(n));
                    }

                    // final results -- All the appropriate pkgs with these tags
                    var roleCapabilities = joinedRoleCapabilityTable.AsEnumerable().Where(roleCapabilityPredicate).Select(p => p);
                }
            }



            predicate = BuildPredicate(repositoryName);


            /// do we need this???
            if ((queryTable == null) || (queryTable.Rows.Count == 0) && (((_type == null) || (_type.Contains("Module", StringComparer.OrdinalIgnoreCase) || _type.Contains("Script", StringComparer.OrdinalIgnoreCase))) && ((_tags == null) || (_tags.Length == 0))))
            {
                queryTable = metadataTable;
            }


            var distinctPkgs = queryTable.AsEnumerable().DistinctBy

            /*
            enumerable5 = DistinctByExtension.DistinctBy<DataRow, string>((IEnumerable<DataRow>) EnumerableRowCollectionExtensions.Where<DataRow>(DataTableExtensions.AsEnumerable(table7), (Func<DataRow, bool>) starter), delegate (DataRow p) {
                return DataRowExtensions.Field<string>(p, "Key");
            });
            */



            List<DataRow> list = Enumerable.ToList<DataRow>(enumerable5);

            IEnumerable<DataRow> foundPkgs = 







            if (_includeDependencies)
            {
    
                var dependencyResults = from t0 in metadataTable.AsEnumerable()
                        join t1 in dependenciesTable.AsEnumerable()
                        on t0.Field<string>("Key") equals t1.Field<string>("Key")
                        select new PackageMetadataAllTables
                        {
                            Key = t0["Key"],
                            Name = t0["Name"],
                            Version = t0["Version"],
                            Type = (string) t0["Type"],
                            Description = t0["Description"],
                            Author = t0["Author"],
                            Copyright = t0["Copyright"],
                            PublishedDate = t0["PublishedDate"],
                            InstalledDate = t0["InstalledDate"],
                            UpdatedDate = t0["UpdatedDate"],
                            LicenseUri = t0["LicenseUri"],
                            ProjectUri = t0["ProjectUri"],
                            IconUri = t0["IconUri"],
                            PowerShellGetFormatVersion = t0["PowerShellGetFormatVersion"],
                            ReleaseNotes = t0["ReleaseNotes"],
                            RepositorySourceLocation = t0["RepositorySourceLocation"],
                            Repository = t0["Repository"],
                            IsPrerelease = t0["IsPrerelease"],
                            Dependencies = (Dependency) t1["Dependencies"],
                        };
                
                    var dependencyPredicate = PredicateBuilder.New<DataRow>(true);

                    DataTable joinedDependencyTable = dependencyResults.ToDataTable();




                    // final results -- All the appropriate pkgs with these tags
                    var roleCapabilities = joinedDependencyTable.AsEnumerable().Where(roleCapabilityPredicate).Select(p => p);
                  

                    /// NEED TO COMBINE FINAL RESULTS TABLES?


                    NuGetVersion minVersion = string.IsNullOrEmpty(tables.Dependencies.MinimumVersion) ? null : new NuGetVersion(tables.Dependencies.MinimumVersion);
                    NuGetVersion maxVersion = string.IsNullOrEmpty(tables.Dependencies.MaximumVersion) ? null : new NuGetVersion(tables.Dependencies.MaximumVersion);
                    VersionRange range = new VersionRange(minVersion, true, maxVersion, true, null, null);

                    // need to optimize this
                    list.AddRange(FindPackageFromCacheHelper(cachetables, repositoryName, name, range.ToString()));
            }


            // (IEnumerable<DataRow>) 
            return list; 
        }






        private ExpressionStarter<DataRow> BuildPredicate(string repository, string version)
        {
            NuGetVersion nugetVersion;
            var predicate = PredicateBuilder.New<DataRow>(true);

            if (_type != null)
            {
                var typePredicate = PredicateBuilder.New<DataRow>(true);

                if (_type.Contains("Script", StringComparer.OrdinalIgnoreCase))
                {
                    typePredicate = typePredicate.Or(pkg => pkg.Field<string>("Type").Equals("Script"));
                }
                if (_type.Contains("Module", StringComparer.OrdinalIgnoreCase))
                {
                    typePredicate = typePredicate.Or(pkg => pkg.Field<string>("Type").Equals("Module"));
                }
                predicate.And(typePredicate);

            }

            ExpressionStarter<DataRow> starter2 = PredicateBuilder.New<DataRow>(true);
            if (_moduleName != null)
            {
                predicate = predicate.And(pkg => pkg.Field<string>("Name").Equals(_moduleName));
            }

            if ((_type == null) || ((_type.Length == 0) || !(_type.Contains("Module", StringComparer.OrdinalIgnoreCase) || _type.Contains("Script", StringComparer.OrdinalIgnoreCase))))
            {
                var typeNamePredicate = PredicateBuilder.New<DataRow>(true);
                foreach (string name in _name)
                {

                    //// ?
                    typeNamePredicate = typeNamePredicate.Or(pkg => pkg.Field<string>("Type").Equals("Script"));
                }
            }

            

            if (version != null)
            {

                NuGetVersion nugetVersion = null;
                NuGetVersion.TryParse(version, out nugetVersion);

                VersionRange versionRange = VersionRange.Parse(version);

                predicate = predicate.And(pkg => versionRange.Satisfies(pkg.Field<string>("Version")));
                {
                   
                }




            }
            if (!_prerelease)
            {
                predicate = predicate.And(pkg => pkg.Field<string>("IsPrerelease").Equals(false));  // consider checking if it IS prerelease
            }
            return predicate;
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
