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
using Microsoft.PowerShell.PowerShellGet.RepositorySettings;
using static NuGet.Protocol.Core.Types.PackageSearchMetadataBuilder;

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
        public string Version
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
        public string[] Repository
        {
            get { return _repository; }

            set { _repository = value; }
        }
        private string[] _repository;

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
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "NameParameterSet")]
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
        private readonly object p;

        // Define the cancellation token.
        CancellationTokenSource source;
        CancellationToken cancellationToken;
        List<string> pkgsLeftToFind;

        /// <summary>
        /// </summary>
        protected override void ProcessRecord()
        {
            source = new CancellationTokenSource();
            cancellationToken = source.Token;



            var r = new RespositorySettings();
            var listOfRepositories = r.Read(_repository);

            var returnedPkgsFound = new List<IEnumerable<IPackageSearchMetadata>>();
            pkgsLeftToFind = _name.ToList();
            foreach (var repoName in listOfRepositories)
            {
                // if it can't find the pkg in one repository, it'll look in the next one in the list
                // returns any pkgs found, and any pkgs that weren't found
                returnedPkgsFound.AddRange(FindPackagesFromSource(repoName.Properties["Url"].Value.ToString(), cancellationToken));
                if (!pkgsLeftToFind.Any())
                {
                    break;
                }
            }


            // Flatten returned pkgs before displaying output returnedPkgsFound.Flatten().ToList()[0]
            var flattenedPkgs = returnedPkgsFound.Flatten();
            // flattenedPkgs.ToList();


        }




        //
        public List<IEnumerable<IPackageSearchMetadata>> FindPackagesFromSource(string repositoryUrl, CancellationToken cancellationToken)
        {
            List<IEnumerable<IPackageSearchMetadata>> returnedPkgs = new List<IEnumerable<IPackageSearchMetadata>>();

            if (repositoryUrl.StartsWith("file://"))
            {

                FindLocalPackagesResourceV2 localResource = new FindLocalPackagesResourceV2(repositoryUrl);

                LocalPackageSearchResource resourceSearch = new LocalPackageSearchResource(localResource);
                LocalPackageMetadataResource resourceMetadata = new LocalPackageMetadataResource(localResource);

                SearchFilter filter = new SearchFilter(_prerelease);
                SourceCacheContext context = new SourceCacheContext();


                if ((_name == null) || (_name.Length == 0))
                {
                    returnedPkgs.AddRange(FindPackagesFromSourceHelper(repositoryUrl, null, resourceSearch, resourceMetadata, filter, context));
                }

                foreach (var n in _name)
                {
                    returnedPkgs.AddRange(FindPackagesFromSourceHelper(repositoryUrl, n, resourceSearch, resourceMetadata, filter, context));
                }
            }
            else
            {
                PackageSource source = new PackageSource(repositoryUrl);
                if (_credential != null)
                {
                    string password = new NetworkCredential(string.Empty, _credential.Password).Password;
                    source.Credentials = PackageSourceCredential.FromUserInput(repositoryUrl, _credential.UserName, password, true, null);
                }
                var provider = FactoryExtensionsV3.GetCoreV3(NuGet.Protocol.Core.Types.Repository.Provider);

                SourceRepository repository = new SourceRepository(source, provider);
                PackageSearchResource resourceSearch = repository.GetResourceAsync<PackageSearchResource>().GetAwaiter().GetResult();
                PackageMetadataResource resourceMetadata = repository.GetResourceAsync<PackageMetadataResource>().GetAwaiter().GetResult();

                SearchFilter filter = new SearchFilter(_prerelease);
                SourceCacheContext context = new SourceCacheContext();

                if ((_name == null) || (_name.Length == 0))
                {
                    returnedPkgs.AddRange(FindPackagesFromSourceHelper(repositoryUrl, null, resourceSearch, resourceMetadata, filter, context));
                }

                foreach (var n in _name)
                {
                    var foundPkgs = FindPackagesFromSourceHelper(repositoryUrl, n, resourceSearch, resourceMetadata, filter, context);
                    if (foundPkgs.Any() && foundPkgs.FirstOrDefault() != null)
                    {
                        returnedPkgs.AddRange(foundPkgs);
                        pkgsLeftToFind.Remove(n);
                    }
                }
            }

            return returnedPkgs;
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
                var tagsRowCollection = joinedTagsTable.AsEnumerable().Where(tagsPredicate).Select(p => p);

                // Add row collection to final table to be queried
                queryTable.Merge(tagsRowCollection.ToDataTable());
            }

            if (_type != null)
            {
                if (_type.Contains("Command", StringComparer.OrdinalIgnoreCase))
                {

                    var commandResults = from t0 in metadataTable.AsEnumerable()
                                         join t1 in commandsTable.AsEnumerable()
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
                    var commandsRowCollection = joinedCommandTable.AsEnumerable().Where(commandPredicate).Select(p => p);

                    // Add row collection to final table to be queried
                    queryTable.Merge(commandsRowCollection.ToDataTable());
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
                    var dscResourcesRowCollection = joinedDscResourceTable.AsEnumerable().Where(dscResourcePredicate).Select(p => p);

                    // Add row collection to final table to be queried
                    queryTable.Merge(dscResourcesRowCollection.ToDataTable());
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
                    var roleCapabilitiesRowCollection = joinedRoleCapabilityTable.AsEnumerable().Where(roleCapabilityPredicate).Select(p => p);

                    // Add row collection to final table to be queried
                    queryTable.Merge(roleCapabilitiesRowCollection.ToDataTable());
                }
            }



            // We'll build the rest of the predicate-- ie the portions of the predicate that do not rely on datatables
            predicate = predicate.Or(BuildPredicate(repositoryName));


            // we want to uniquely add datarows into the table
            // if queryTable is empty, we'll just query upon the metadata table
            if (queryTable == null || queryTable.Rows.Count == 0)
            {
                queryTable = metadataTable;
            }

            // final results -- All the appropriate pkgs with these tags
            var queryTableRowCollection = queryTable.AsEnumerable().Where(predicate).Select(p => p);

            // ensure distinct by key
            var finalQueryResults = queryTableRowCollection.AsEnumerable().DistinctBy(pkg => pkg.Field<string>("Key"));

            // Add row collection to final table to be queried
            // queryTable.Merge(distinctQueryTableRowCollection.ToDataTable());



            /// ignore-- testing.
            //if ((queryTable == null) || (queryTable.Rows.Count == 0) && (((_type == null) || (_type.Contains("Module", StringComparer.OrdinalIgnoreCase) || _type.Contains("Script", StringComparer.OrdinalIgnoreCase))) && ((_tags == null) || (_tags.Length == 0))))
            //{
            //    queryTable = metadataTable;
            //}


            List<IEnumerable<DataRow>> allFoundPkgs = new List<IEnumerable<DataRow>>();
            allFoundPkgs.Add(finalQueryResults);

            // Need to handle includeDependencies
            if (_includeDependencies)
            {
                foreach (var pkg in finalQueryResults)
                {
                    //allFoundPkgs.Add(DependencyFinder(finalQueryResults, dependenciesTable));
                }
            }

            IEnumerable<DataRow> flattenedFoundPkgs = (IEnumerable<DataRow>)allFoundPkgs.Flatten();

            // (IEnumerable<DataRow>)
            return flattenedFoundPkgs;
        }




        /*********************** come back to this
        private IEnumerable<DataRow> DependencyFinder(IEnumerable<DataRow> finalQueryResults, DataTable dependenciesTable )
        {
            List<IEnumerable<PackageMetadataAllTables>> foundDependencies = new List<IEnumerable<IPackageSearchMetadata>>();

            var queryResultsDT = finalQueryResults.ToDataTable();

            //var dependencyResults = from t0 in metadataTable.AsEnumerable()
            var dependencyResults = from t0 in queryResultsDT.AsEnumerable()
                                    join t1 in dependenciesTable.AsEnumerable()
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
                                        Dependencies = (Dependency)t1["Dependencies"],
                                    };

            var dependencyPredicate = PredicateBuilder.New<DataRow>(true);

            DataTable joinedDependencyTable = dependencyResults.ToDataTable();

            // Build predicate by combining names of dependencies searches with 'or'

            // public string Name { get; set; }
            // public string MinimumVersion { get; set; }
            // public string MaximumVersion { get; set; }



            // for each pkg that was found, check to see if it has a dep
            foreach (var pkg in dependencyResults)
            {
                // because it's an ienumerable, we need to pull out the pkg itself to access its properties, but there is only a 'default' option
                if (!string.IsNullOrEmpty(pkg.Dependencies.Name))
                {
                    // you could just add the data row
                    foundDependencies.Add((IEnumerable<PackageMetadataAllTables>) pkg);
                }

                // and then check to make sure dep name/version exists within the cache (via the metadata table)
                // call helper recursively
                // (IEnumerable<DataRow> finalQueryResults, DataTable dependenciesTable )
                DependencyFinder(pkg, dependencyResults, dependenciesTable)

                var roleCapabilityPredicate = PredicateBuilder.New<DataRow>(true);

                DataTable joinedRoleCapabilityTable = roleCapabilityResults.ToDataTable();

                // Build predicate by combining names of commands searches with 'or'
                // if no name is specified, we'll return all (?)
                foreach (string n in _name)
                {
                    roleCapabilityPredicate = roleCapabilityPredicate.Or(pkg => pkg.Field<string>("RoleCapability").Equals(n));
                }

                // final results -- All the appropriate pkgs with these tags
                var roleCapabilitiesRowCollection = joinedRoleCapabilityTable.AsEnumerable().Where(roleCapabilityPredicate).Select(p => p);

                // Add row collection to final table to be queried
                queryTable.Merge(roleCapabilitiesRowCollection.ToDataTable());
            }

            // final results -- All the
            var dependencies = joinedDependencyTable.AsEnumerable().Where(dependencyPredicate).Select(p => p);

            return
        }
        ***************/


        private ExpressionStarter<DataRow> BuildPredicate(string repository)
        {
            //NuGetVersion nugetVersion0;
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


            // cache will only contain the latest stable and latest prerelease of each package
            if (_version != null)
            {

                NuGetVersion nugetVersion;

                //VersionRange versionRange = VersionRange.Parse(version);

                if (NuGetVersion.TryParse(_version, out nugetVersion))
                {
                    predicate = predicate.And(pkg => pkg.Field<string>("Version").Equals(nugetVersion));
                }




            }
            if (!_prerelease)
            {
                predicate = predicate.And(pkg => pkg.Field<string>("IsPrerelease").Equals("false"));  // consider checking if it IS prerelease
            }
            return predicate;
        }









        private List<IEnumerable<IPackageSearchMetadata>> FindPackagesFromSourceHelper(string repositoryUrl, string name, PackageSearchResource pkgSearchResource, PackageMetadataResource pkgMetadataResource, SearchFilter searchFilter, SourceCacheContext srcContext)
        {

            List<IEnumerable<IPackageSearchMetadata>> foundPackages = new List<IEnumerable<IPackageSearchMetadata>>();
            //IEnumerable<IPackageSearchMetadata> foundPackages;

            // If module name is specified, use that as the name for the pkg to search for
            if (_moduleName != null)
            {
                // may need to take 1
                foundPackages.Add(pkgMetadataResource.GetMetadataAsync(_moduleName, _prerelease, false, srcContext, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult());
                //foundPackages = pkgMetadataResource.GetMetadataAsync(_moduleName, _prerelease, false, srcContext, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult();
            }
            else if (name != null)
            {
                // If a resource name is specified, search for that particular pkg name
                // search for specific pkg name
                if (!name.Contains("*"))
                {
                    foundPackages.Add(pkgMetadataResource.GetMetadataAsync(name, _prerelease, false, srcContext, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult());
                    //foundPackages = pkgMetadataResource.GetMetadataAsync(name, _prerelease, false, srcContext, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult();
                }
                // search for range of pkg names
                else
                {
                    // TODO:  follow up on this
                    foundPackages.Add(pkgSearchResource.SearchAsync(name, searchFilter, 0, 6000, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult());
                    //foundPackages = pkgSearchResource.SearchAsync(name, searchFilter, 0, 6000, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult();
                }
            }
            else
            {
                foundPackages.Add(pkgSearchResource.SearchAsync("", searchFilter, 0, 6000, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult());
                //foundPackages = pkgSearchResource.SearchAsync("", searchFilter, 0, 6000, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult();
            }



            List<IEnumerable<IPackageSearchMetadata>> filteredFoundPkgs = new List<IEnumerable<IPackageSearchMetadata>>();

            // Check version first to narrow down the number of pkgs before potential searching through tags
            if (_version != null)
            {

                if (_version.Equals("*"))
                {
                    // this is all that's needed if version == "*" (I think)
                    /*
                     if (repositoryUrl.Contains("api.nuget.org") || repositoryUrl.StartsWith("file:///"))
                     {
                         // need to reverse the order of the informaiton returned when using nuget.org v3 protocol
                         filteredFoundPkgs.Add(pkgMetadataResource.GetMetadataAsync(name, _prerelease, false, srcContext, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult().Reverse());
                     }
                     else
                     {
                         filteredFoundPkgs.Add(pkgMetadataResource.GetMetadataAsync(name, _prerelease, false, srcContext, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult());
                     }
                     */
                    // ensure that the latst version is returned first (the ordering of versions differ
                    filteredFoundPkgs.Add(pkgMetadataResource.GetMetadataAsync(name, _prerelease, false, srcContext, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult().OrderByDescending(p => p.Identity.Version, VersionComparer.VersionRelease));
                }
                else
                {
                    // try to parse into a singular NuGet version
                    NuGetVersion specificVersion;
                    NuGetVersion.TryParse(_version, out specificVersion);

                    foundPackages.RemoveAll(p => true);
                    VersionRange versionRange = null;

                    if (specificVersion != null)
                    {
                        // exact version
                        versionRange = new VersionRange(specificVersion, true, specificVersion, true, null, null);
                    }
                    else
                    {
                        // maybe try catch this
                        // check if version range
                        versionRange = VersionRange.Parse(_version);

                    }



                    // Search for packages within a version range
                    // ensure that the latst version is returned first (the ordering of versions differ
                    // test wth  Find-PSResource 'Carbon' -Version '[,2.4.0)'
                    foundPackages.Add(pkgMetadataResource.GetMetadataAsync(name, _prerelease, false, srcContext, NullLogger.Instance, cancellationToken).GetAwaiter().GetResult()
                        .Where(p => versionRange.Satisfies(p.Identity.Version))
                        .OrderByDescending(p => p.Identity.Version, VersionComparer.VersionRelease));

                    // choose the most recent version -- it's not doing this right now
                    int toRemove = foundPackages.First().Count() - 1;
                    var singlePkg = (System.Linq.Enumerable.SkipLast(foundPackages.FirstOrDefault(), toRemove));
                    filteredFoundPkgs.Add(singlePkg);
                }
            }
            else // version is null
            {
                // choose the most recent version -- it's not doing this right now
                int toRemove = foundPackages.First().Count() - 1;

                // var result = ((IEnumerable)firstPkg).Cast<IPackageSearchMetadata>();

                // var bleh = foundPackages.FirstOrDefault().(System.Linq.Enumerable.SkipLast(foundPackages.FirstOrDefault(), 20));

                var bleh = (System.Linq.Enumerable.SkipLast(foundPackages.FirstOrDefault(), toRemove));

                filteredFoundPkgs.Add(bleh);

            }




            // TAGS
            /// should move this to the last thing that gets filtered
            char[] delimiter = new char[] { ' ', ',' };
            var flattenedPkgs = filteredFoundPkgs.Flatten().ToList();
            if (_tags != null)
            {
                // clear filteredfoundPkgs because we'll just be adding back the pkgs we we
                filteredFoundPkgs.RemoveAll(p => true);
                var pkgsFilteredByTags = new List<IPackageSearchMetadata>();

                foreach (IPackageSearchMetadata p in flattenedPkgs)
                {
                    // Enumerable.ElementAt(0)
                    var tagArray = p.Tags.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string t in _tags)
                    {
                        // if the pkg contains one of the tags we're searching for
                        if (tagArray.Contains(t, StringComparer.OrdinalIgnoreCase))
                        {
                            pkgsFilteredByTags.Add(p);
                        }
                    }
                }
                // make sure just adding one pkg if not * versions
                if (_version != "*")
                {
                    filteredFoundPkgs.Add(pkgsFilteredByTags.DistinctBy(p => p.Identity.Id));
                }
                else
                {
                    // if we want all version, make sure there's only one package id/version in the returned list.
                    filteredFoundPkgs.Add(pkgsFilteredByTags.DistinctBy(p => p.Identity.Version));
                }
            }





            if (_type != null)
            {
                // can optimize this more later
                // this is handled
                /*
                if (_type.Contains("Command", StringComparer.OrdinalIgnoreCase))
                {
                    filteredFoundPkgs = FilterPkgsByResourceType(foundPackages, filteredFoundPkgs, "PSCommand_");
                }

                if (_type.Contains("DscResource", StringComparer.OrdinalIgnoreCase))
                {
                    filteredFoundPkgs = FilterPkgsByResourceType(foundPackages, filteredFoundPkgs, "PSDscResource_");
                }

                if (_type.Contains("RoleCapability", StringComparer.OrdinalIgnoreCase))
                {
                    filteredFoundPkgs = FilterPkgsByResourceType(foundPackages, filteredFoundPkgs, "PSRoleCapability_");
                }

                // module
                if (_type.Contains("Module", StringComparer.OrdinalIgnoreCase))
                {

                }
                // script
                if (_type.Contains("Script", StringComparer.OrdinalIgnoreCase))
                {
                    // PSScript
                }
                */


                var tempList = (FilterPkgsByResourceType(foundPackages, filteredFoundPkgs));
                filteredFoundPkgs.RemoveAll(p => true);

                filteredFoundPkgs.Add(tempList);


            }
            // else type == null
            // if type is null, we just don't filter on anything for type






            // Search for dependencies
            if (_includeDependencies && filteredFoundPkgs.Any())
            {
                List<IEnumerable<IPackageSearchMetadata>> foundDependencies = new List<IEnumerable<IPackageSearchMetadata>>();

                // need to parse the depenency and version and such
                var filteredFoundPkgsFlattened = filteredFoundPkgs.Flatten();
                foreach (IPackageSearchMetadata pkg in filteredFoundPkgsFlattened)
                {
                    // need to improve this later
                    // this function recursively finds all dependencies
                    // change into an ieunumerable (helpful for the finddependenciesfromsource function)

                    foundDependencies.AddRange(FindDependenciesFromSource(Enumerable.Repeat(pkg, 1), pkgMetadataResource, srcContext));
                }

                filteredFoundPkgs.AddRange(foundDependencies);
            }

            // return foundPackages;
            return filteredFoundPkgs;
        }




        private List<IEnumerable<IPackageSearchMetadata>> FindDependenciesFromSource(IEnumerable<IPackageSearchMetadata> pkg, PackageMetadataResource pkgMetadataResource, SourceCacheContext srcContext)
        {
            /// dependency resolver
            ///
            /// this function will be recursively called
            ///
            /// call the findpackages from source helper (potentially generalize this so it's finding packages from source or cache)
            ///
            List<IEnumerable<IPackageSearchMetadata>> foundDependencies = new List<IEnumerable<IPackageSearchMetadata>>();

            // 1)  check the dependencies of this pkg
            // 2) for each dependency group, search for the appropriate name and version
            // a dependency group are all the dependencies for a particular framework
            // first or default because we need this pkg to be an ienumerable (so we don't need to be doing strange object conversions)
            foreach (var dependencyGroup in pkg.FirstOrDefault().DependencySets)
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



                    // choose the most recent version
                    int toRemove = dependencies.Count() - 1;

                    // if no version/version range is specified the we just return the latest version
                    var depPkgToReturn = (versionRange == null ?
                        dependencies.FirstOrDefault() :
                        dependencies.Where(v => versionRange.Satisfies(v.Identity.Version)).FirstOrDefault());



                    // using the repeat function to convert the IPackageSearchMetadata object into an enumerable.
                    foundDependencies.Add(Enumerable.Repeat(depPkgToReturn, 1));

                    // 3) search for any dependencies the pkg has
                    foundDependencies.AddRange(FindDependenciesFromSource(Enumerable.Repeat(depPkgToReturn, 1), pkgMetadataResource, srcContext));
                }
            }

            // flatten after returning
            return foundDependencies;
        }










        private List<IPackageSearchMetadata> FilterPkgsByResourceType(List<IEnumerable<IPackageSearchMetadata>> foundPackages, List<IEnumerable<IPackageSearchMetadata>> filteredFoundPkgs)
        {


            char[] delimiter = new char[] { ' ', ',' };

            // If there are any packages that were filtered by tags, we'll continue to filter on those packages, otherwise, we'll filter on all the packages returned from the search
            var flattenedPkgs = filteredFoundPkgs.Any() ? filteredFoundPkgs.Flatten() : foundPackages.Flatten();

            var pkgsFilteredByResource = new List<IPackageSearchMetadata>();

            foreach (IPackageSearchMetadata pkg in flattenedPkgs)
            {
                // Enumerable.ElementAt(0)
                var tagArray = pkg.Tags.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);


                // check modules and scripts here ??

                foreach (var tag in tagArray)
                {

                    // iterate through type array
                    foreach (var resourceType in _type)
                    {

                        switch (resourceType)
                        {
                            case "Module":
                                if (tag.Equals("PSModule"))
                                {
                                    pkgsFilteredByResource.Add(pkg);
                                }
                                break;

                            case "Script":
                                if (tag.Equals("PSScript"))
                                {
                                    pkgsFilteredByResource.Add(pkg);
                                }
                                break;

                            case "Command":
                                if (tag.StartsWith("PSCommand_"))
                                {
                                    foreach (var resourceName in _name)
                                    {
                                        if (tag.Equals("PSCommand_" + resourceName))
                                        {
                                            pkgsFilteredByResource.Add(pkg);

                                        }
                                    }
                                }
                                break;

                            case "DscResource":
                                if (tag.StartsWith("PSDscResource_"))
                                {
                                    foreach (var resourceName in _name)
                                    {
                                        if (tag.Equals("PSDscResource_" + resourceName))
                                        {
                                            pkgsFilteredByResource.Add(pkg);

                                        }
                                    }
                                }
                                break;

                            case "RoleCapability":
                                if (tag.StartsWith("PSRoleCapability_"))
                                {
                                    foreach (var resourceName in _name)
                                    {
                                        if (tag.Equals("PSRoleCapability_" + resourceName))
                                        {
                                            pkgsFilteredByResource.Add(pkg);

                                        }
                                    }
                                }
                                break;
                        }

                    }
                }
            }

            return pkgsFilteredByResource;

        }






    }
}
