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







        /*
                    IEnumerable<IPackageSearchMetadata> enumerable;

                    List<IEnumerable<IPackageSearchMetadata>> list3;
                    List<IEnumerable<IPackageSearchMetadata>> list4;

                    List<IEnumerable<IPackageSearchMetadata>> foundPackages = new List<IEnumerable<IPackageSearchMetadata>>();

                    FindLocalPackagesResourceV2 localResource = new FindLocalPackagesResourceV2(repositoryUrl);

                    LocalPackageSearchResource resource = new LocalPackageSearchResource(localResource);
                    LocalPackageMetadataResource resource2 = new LocalPackageMetadataResource(localResource);

                    SearchFilter filter = new SearchFilter(_prerelease);
                    SourceCacheContext context = new SourceCacheContext();

                    if (_moduleName == null)
                    {

                    }
                    else
                    {
                        // Take(1)
                        var metadatapkgs = resource2.GetMetadataAsync(_moduleName, _prerelease, false, context, NullLogger.Instance, CancellationToken.None).GetAwaiter().GetResult().FirstOrDefault();


                        var list3 = new List<IEnumerable<IPackageSearchMetadata>>();


                        char[] delimiters = new char[] { ' ', ',' };
                        char[] tagsArr;


                }
        */







        private List<IEnumerable<IPackageSearchMetadata>> GetPackagesHelper(string repositoryUrl, string name, PackageSearchResource pkgSearchResource, PackageMetadataResource pkgMetadataResource, SearchFilter searchFilter, SourceCacheContext srcContext)
        {
            IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata> enumerable3;
            List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>> list3;
            string[] strArray;
            string[] strArray2;
            int num;
            List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>> list4;
            string[] strArray4;
            int num3;
            string[] strArray6;
            int num5;
            string[] strArray8;
            int num7;
            string[] strArray10;
            int num9;
            string[] strArray12;
            int num11;
            string str2 = (typeTags + tags).Trim();
            List<IEnumerable<IPackageSearchMetadata>> foundPackages = new List<IEnumerable<IPackageSearchMetadata>>();
            IEnumerable<Lazy<INuGetResourceProvider>> enumerable = FactoryExtensionsV3.GetCoreV3(NuGet.Protocol.Core.Types.Repository.Provider);




            }
            NuGet.Protocol.Core.Types.SourceRepository repository = new NuGet.Protocol.Core.Types.SourceRepository(source, enumerable);
            NuGet.Protocol.Core.Types.PackageSearchResource result = repository.GetResourceAsync<NuGet.Protocol.Core.Types.PackageSearchResource>().GetAwaiter().GetResult();
            NuGet.Protocol.Core.Types.PackageMetadataResource resource = repository.GetResourceAsync<NuGet.Protocol.Core.Types.PackageMetadataResource>().GetAwaiter().GetResult();
            NuGet.Protocol.Core.Types.SearchFilter filter = new NuGet.Protocol.Core.Types.SearchFilter(this._prerelease);
            NuGet.Protocol.Core.Types.SourceCacheContext context = new NuGet.Protocol.Core.Types.SourceCacheContext();
            NuGet.Protocol.Core.Types.PackageSearchResource resource3 = new NuGet.Protocol.Core.Types.SourceRepository(new PackageSource(repositoryUrl), FactoryExtensionsV3.GetCoreV3(NuGet.Protocol.Core.Types.Repository.Provider)).GetResourceAsync<NuGet.Protocol.Core.Types.PackageSearchResource>().GetAwaiter().GetResult();
           
        
        
        if (this._moduleName == null)
            {
                goto TR_0076;
            }
            else
            {
                enumerable3 = Enumerable.Take<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(resource.GetMetadataAsync(this._moduleName, this._prerelease, false, context, NullLogger.Instance, CancellationToken.get_None()).GetAwaiter().GetResult(), 1);
                list3 = new List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>();
                char[] chArray1 = new char[] { ' ', ',' };
                char[] chArray = chArray1;
                strArray = Enumerable.FirstOrDefault<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(enumerable3).Tags.Split(chArray, (StringSplitOptions)StringSplitOptions.RemoveEmptyEntries);
                if (((this._type == null) || !Enumerable.Contains<string>(this._type, "Command", (IEqualityComparer<string>)StringComparer.get_OrdinalIgnoreCase())) || (this._name.Length == 0))
                {
                    goto TR_00C2;
                }
                else
                {
                    strArray2 = strArray;
                    num = 0;
                }
            }
            goto TR_00CE;
        TR_0003:
            num9++;
        TR_000E:
            while (true)
            {
                if (num9 >= strArray10.Length)
                {
                    list4 = list3;
                    break;
                }
                string str17 = strArray10[num9];
                if (str17.StartsWith("PSCommand_"))
                {
                    string str16 = str17.TrimStart("PSCommand_".ToCharArray());
                    string[] strArray11 = this._name;
                    int index = 0;
                    while (true)
                    {
                        if (index < strArray11.Length)
                        {
                            string str18 = strArray11[index];
                            if (!str16.Equals(str18))
                            {
                                index++;
                                continue;
                            }
                            list3.Add(enumerable3);
                            list4 = list3;
                        }
                        else
                        {
                            goto TR_0003;
                        }
                        break;
                    }
                    break;
                }
                goto TR_0003;
            }
            return list4;
        TR_0051:
            if ((foundPackages.Count == 0) && (Enumerable.Count<string>(this._name) == 0))
            {
                foundPackages.Add(result.SearchAsync(name, filter, 0, 100, NullLogger.Instance, CancellationToken.get_None()).GetAwaiter().GetResult());
            }
            List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>> list2 = new List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>();
            if (this._includeDependencies && Enumerable.Any<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>((IEnumerable<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>)foundPackages))
            {
                foreach (IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata> enumerable9 in this.FindDependencyPackages(repositoryUrl, foundPackages, resource))
                {
                    foundPackages.Add(enumerable9);
                }
            }
            return foundPackages;
        TR_0076:
            if (((name != null) && (foundPackages.Count == 0)) && (name.Length != 0))
            {
                IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata> item = resource.GetMetadataAsync(name, this._prerelease, false, context, NullLogger.Instance, CancellationToken.get_None()).GetAwaiter().GetResult();
                List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>> list5 = new List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>();
                if (this._version == null)
                {
                    if (Enumerable.Count<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(Enumerable.Take<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(item, 1)) > 0)
                    {
                        list5.Add(Enumerable.Take<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(item, 1));
                    }
                }
                else
                {
                    NuGetVersion minVersion = null;
                    try
                    {
                        minVersion = new NuGetVersion(this._version);
                    }
                    catch
                    {
                    }
                    VersionRange versionRange = (minVersion == null) ? VersionRange.Parse(this._version) : new VersionRange(minVersion, true, minVersion, true, null, null);
                    if (this._version.Equals("*"))
                    {
                        if (repositoryUrl.Contains("api.nuget.org") || repositoryUrl.StartsWith("file:///"))
                        {
                            list5.Add(Enumerable.Reverse<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(item));
                        }
                        else
                        {
                            list5.Add(item);
                        }
                    }
                    else if (repositoryUrl.EndsWith("index.json") || repositoryUrl.StartsWith("file:///"))
                    {
                        list5.Add(Enumerable.Reverse<NuGet.Protocol.Core.Types.IPackageSearchMetadata>((IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>)(from pkg in item select pkg)));
                    }
                    else
                    {
                        list5.Add(from pkg in item select pkg);
                    }
                }
                List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>> list6 = new List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>();
                if ((this._tags.Length == 0) || (list5 == null))
                {
                    foundPackages.AddRange((IEnumerable<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>)list5);
                }
                else
                {
                    foreach (IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata> enumerable6 in list5)
                    {
                        char[] chArray5 = new char[] { ' ', ',' };
                        char[] chArray2 = chArray5;
                        string[] strArray19 = Enumerable.ElementAt<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(enumerable6, 0).Tags.Split(chArray2, (StringSplitOptions)StringSplitOptions.RemoveEmptyEntries);
                        foreach (string str25 in this._tags)
                        {
                            if (Enumerable.Contains<string>(strArray19, str25, (IEqualityComparer<string>)StringComparer.get_OrdinalIgnoreCase()))
                            {
                                list6.Add(enumerable6);
                            }
                        }
                    }
                    return list6;
                }
            }
            if ((Enumerable.Count<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>((IEnumerable<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>)foundPackages) != 0) && ((Enumerable.Count<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>((IEnumerable<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>)foundPackages) <= 0) || Enumerable.Any<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(Enumerable.ElementAt<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>((IEnumerable<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>)foundPackages, 0))))
            {
                goto TR_0051;
            }
            else
            {
                List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>> list7 = new List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>();
                string[] strArray21 = this._name;
                int index = 0;
                while (true)
                {
                    if (index < strArray21.Length)
                    {
                        string str26 = strArray21[index];
                        if (str26.Contains("*"))
                        {
                            list7.Add(result.SearchAsync(str26, filter, 0, 0x1770, NullLogger.Instance, CancellationToken.get_None()).GetAwaiter().GetResult());
                        }
                        else
                        {
                            IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata> item = Enumerable.Take<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(resource.GetMetadataAsync(str26, this._prerelease, false, context, NullLogger.Instance, CancellationToken.get_None()).GetAwaiter().GetResult(), 1);
                            if (Enumerable.Count<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(item) > 0)
                            {
                                list7.Add(item);
                            }
                        }
                        index++;
                        continue;
                    }
                    List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>> list8 = new List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>();
                    if (this._name.Length == 0)
                    {
                        list8.Add(result.SearchAsync("", filter, 0, 0x1770, NullLogger.Instance, CancellationToken.get_None()).GetAwaiter().GetResult());
                    }
                    List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>> list9 = new List<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>();
                    if ((this._tags.Length == 0) || (foundPackages == null))
                    {
                        if (((this._type == null) || !Enumerable.Contains<string>(this._type, "Command", (IEqualityComparer<string>)StringComparer.get_OrdinalIgnoreCase())) || (this._name.Length == 0))
                        {
                            foundPackages.AddRange((IEnumerable<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>)list8);
                            goto TR_0051;
                        }
                        else
                        {
                            foreach (NuGet.Protocol.Core.Types.IPackageSearchMetadata pkg in Enumerable.ElementAt<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>((IEnumerable<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>)list8, 0))
                            {
                                char[] chArray7 = new char[] { ' ', ',' };
                                char[] chArray4 = chArray7;
                                string[] strArray24 = pkg.Tags.Split(chArray4, (StringSplitOptions)StringSplitOptions.RemoveEmptyEntries);
                                foreach (string str29 in strArray24)
                                {
                                    if (str29.StartsWith("PSCommand_"))
                                    {
                                        string str28 = str29.TrimStart("PSCommand_".ToCharArray());
                                        foreach (string str30 in this._name)
                                        {
                                            if (str28.Equals(str30))
                                            {
                                                Func<NuGet.Protocol.Core.Types.IPackageSearchMetadata, bool> <> 9__3;
                                                Func<NuGet.Protocol.Core.Types.IPackageSearchMetadata, bool> func4 = <> 9__3;
                                                if (<> 9__3 == null)
                                                {
                                                    Func<NuGet.Protocol.Core.Types.IPackageSearchMetadata, bool> local3 = <> 9__3;
                                                    func4 = <> 9__3 = delegate (NuGet.Protocol.Core.Types.IPackageSearchMetadata p) {
                                                        return object.ReferenceEquals(p.Identity, pkg.Identity);
                                                    };
                                                }
                                                list9.Add(Enumerable.Where<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(Enumerable.ElementAt<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>((IEnumerable<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>)list8, 0), func4));
                                            }
                                        }
                                    }
                                }
                            }
                            list4 = list9;
                        }
                    }
                    else
                    {
                        foreach (NuGet.Protocol.Core.Types.IPackageSearchMetadata metadata1 in Enumerable.ElementAt<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>((IEnumerable<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>)list8, 0))
                        {
                            char[] chArray6 = new char[] { ' ', ',' };
                            char[] chArray3 = chArray6;
                            string[] strArray22 = metadata1.Tags.Split(chArray3, (StringSplitOptions)StringSplitOptions.RemoveEmptyEntries);
                            foreach (string str27 in this._tags)
                            {
                                if (Enumerable.Contains<string>(strArray22, str27, (IEqualityComparer<string>)StringComparer.get_OrdinalIgnoreCase()))
                                {
                                    Func<NuGet.Protocol.Core.Types.IPackageSearchMetadata, bool> <> 9__2;
                                    Func<NuGet.Protocol.Core.Types.IPackageSearchMetadata, bool> func3 = <> 9__2;
                                    if (<> 9__2 == null)
                                    {
                                        Func<NuGet.Protocol.Core.Types.IPackageSearchMetadata, bool> local2 = <> 9__2;
                                        func3 = <> 9__2 = delegate (NuGet.Protocol.Core.Types.IPackageSearchMetadata p) {
                                            return object.ReferenceEquals(p.Identity, metadata1.Identity);
                                        };
                                    }
                                    list9.Add(Enumerable.Where<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(Enumerable.ElementAt<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>((IEnumerable<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>)list8, 0), func3));
                                }
                            }
                        }
                        list4 = list9;
                    }
                    break;
                }
            }
            return list4;
        TR_0077:
            foundPackages.Add(enumerable3);
            goto TR_0076;
        TR_0087:
            if ((this._type != null) && Enumerable.Contains<string>(this._type, "Module", (IEqualityComparer<string>)StringComparer.get_OrdinalIgnoreCase()))
            {
                if (!Enumerable.Contains<string>(strArray, "PSModule"))
                {
                    return list3;
                }
                else
                {
                    string[] strArray14 = this._name;
                    int index = 0;
                    if (index < strArray14.Length)
                    {
                        if (strArray14[index].Equals(Enumerable.ElementAt<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(enumerable3, 0).Title))
                        {
                            list3.Add(enumerable3);
                        }
                        return list3;
                    }
                }
            }
            if (!((this._type != null) && Enumerable.Contains<string>(this._type, "Script", (IEqualityComparer<string>)StringComparer.get_OrdinalIgnoreCase())))
            {
                goto TR_0077;
            }
            else if (!Enumerable.Contains<string>(strArray, "PSScript"))
            {
                list4 = list3;
            }
            else
            {
                string[] strArray15 = this._name;
                int index = 0;
                if (index < strArray15.Length)
                {
                    if (strArray15[index].Equals(Enumerable.ElementAt<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(enumerable3, 0).Title))
                    {
                        list3.Add(enumerable3);
                    }
                    list4 = list3;
                }
                else
                {
                    goto TR_0077;
                }
            }
            return list4;
        TR_0088:
            num11++;
        TR_0093:
            while (true)
            {
                if (num11 < strArray12.Length)
                {
                    string str20 = strArray12[num11];
                    if (str20.StartsWith("PSRoleCapability_"))
                    {
                        string str19 = str20.TrimStart("PSRoleCapability_".ToCharArray());
                        string[] strArray13 = this._name;
                        int index = 0;
                        while (true)
                        {
                            if (index < strArray13.Length)
                            {
                                string str21 = strArray13[index];
                                if (!str19.Equals(str21))
                                {
                                    index++;
                                    continue;
                                }
                                list3.Add(enumerable3);
                                list4 = list3;
                            }
                            else
                            {
                                goto TR_0088;
                            }
                            break;
                        }
                        break;
                    }
                }
                else
                {
                    goto TR_0087;
                }
                goto TR_0088;
            }
            return list4;
        TR_0096:
            if (this._type != null)
            {
                if (!((this._type != null) && Enumerable.Contains<string>(this._type, "RoleCapability", (IEqualityComparer<string>)StringComparer.get_OrdinalIgnoreCase())))
                {
                    goto TR_0087;
                }
                else
                {
                    strArray12 = strArray;
                    num11 = 0;
                }
            }
            else
            {
                strArray10 = strArray;
                num9 = 0;
                goto TR_000E;
            }
            goto TR_0093;
        TR_0097:
            num7++;
        TR_00A2:
            while (true)
            {
                if (num7 < strArray8.Length)
                {
                    string str14 = strArray8[num7];
                    if (str14.StartsWith("PSDscResource_"))
                    {
                        string str13 = str14.TrimStart("PSDscResource_".ToCharArray());
                        string[] strArray9 = this._name;
                        int index = 0;
                        while (true)
                        {
                            if (index < strArray9.Length)
                            {
                                string str15 = strArray9[index];
                                if (!str13.Equals(str15))
                                {
                                    index++;
                                    continue;
                                }
                                list3.Add(enumerable3);
                                list4 = list3;
                            }
                            else
                            {
                                goto TR_0097;
                            }
                            break;
                        }
                        break;
                    }
                }
                else
                {
                    goto TR_0096;
                }
                goto TR_0097;
            }
            return list4;
        TR_00A6:
            if ((((this._type == null) || (this._name.Length == 0)) || ((!Enumerable.Contains<string>(this._type, "Command", (IEqualityComparer<string>)StringComparer.get_OrdinalIgnoreCase()) && !Enumerable.Contains<string>(this._type, "DscResource", (IEqualityComparer<string>)StringComparer.get_OrdinalIgnoreCase())) && !Enumerable.Contains<string>(this._type, "RoleCapability", (IEqualityComparer<string>)StringComparer.get_OrdinalIgnoreCase()))) || Enumerable.Any<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>((IEnumerable<IEnumerable<NuGet.Protocol.Core.Types.IPackageSearchMetadata>>)list3))
            {
                Console.WriteLine("START");
                Console.WriteLine(Enumerable.ElementAt<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(enumerable3, 0).Title);
                Console.WriteLine("-----");
                Console.WriteLine(Enumerable.ElementAt<NuGet.Protocol.Core.Types.IPackageSearchMetadata>(enumerable3, 0).Tags.ToJson(0));
                Console.WriteLine("END");
                if (!((this._type != null) && Enumerable.Contains<string>(this._type, "DscResource", (IEqualityComparer<string>)StringComparer.get_OrdinalIgnoreCase())))
                {
                    goto TR_0096;
                }
                else
                {
                    strArray8 = strArray;
                    num7 = 0;
                }
            }
            else
            {
                return list3;
            }
            goto TR_00A2;
        TR_00A7:
            num5++;
        TR_00B2:
            while (true)
            {
                if (num5 < strArray6.Length)
                {
                    string str11 = strArray6[num5];
                    if (str11.StartsWith("PSRoleCapability_"))
                    {
                        string str10 = str11.TrimStart("PSRoleCapability_".ToCharArray());
                        string[] strArray7 = this._name;
                        int index = 0;
                        while (true)
                        {
                            if (index < strArray7.Length)
                            {
                                string str12 = strArray7[index];
                                if (!str10.Equals(str12))
                                {
                                    index++;
                                    continue;
                                }
                                list3.Add(enumerable3);
                                list4 = list3;
                            }
                            else
                            {
                                goto TR_00A7;
                            }
                            break;
                        }
                        break;
                    }
                }
                else
                {
                    goto TR_00A6;
                }
                goto TR_00A7;
            }
            return list4;
        TR_00B4:
            if (((this._type == null) || !Enumerable.Contains<string>(this._type, "RoleCapability", (IEqualityComparer<string>)StringComparer.get_OrdinalIgnoreCase())) || (this._name.Length == 0))
            {
                goto TR_00A6;
            }
            else
            {
                strArray6 = strArray;
                num5 = 0;
            }
            goto TR_00B2;
        TR_00B5:
            num3++;
        TR_00C0:
            while (true)
            {
                if (num3 < strArray4.Length)
                {
                    string str8 = strArray4[num3];
                    if (str8.StartsWith("PSDscResource_"))
                    {
                        string str7 = str8.Substring(str8.IndexOf('_') + 1);
                        string[] strArray5 = this._name;
                        int index = 0;
                        while (true)
                        {
                            if (index < strArray5.Length)
                            {
                                string str9 = strArray5[index];
                                if (!str7.Equals(str9))
                                {
                                    index++;
                                    continue;
                                }
                                list3.Add(enumerable3);
                                list4 = list3;
                            }
                            else
                            {
                                goto TR_00B5;
                            }
                            break;
                        }
                        break;
                    }
                }
                else
                {
                    goto TR_00B4;
                }
                goto TR_00B5;
            }
            return list4;
        TR_00C2:
            if (((this._type == null) || !Enumerable.Contains<string>(this._type, "DscResource", (IEqualityComparer<string>)StringComparer.get_OrdinalIgnoreCase())) || (this._name.Length == 0))
            {
                goto TR_00B4;
            }
            else
            {
                strArray4 = strArray;
                num3 = 0;
            }
            goto TR_00C0;
        TR_00C3:
            num++;
        TR_00CE:
            while (true)
            {
                if (num < strArray2.Length)
                {
                    string str5 = strArray2[num];
                    if (str5.StartsWith("PSCommand_"))
                    {
                        string str4 = str5.TrimStart("PSCommand_".ToCharArray());
                        string[] strArray3 = this._name;
                        int index = 0;
                        while (true)
                        {
                            if (index < strArray3.Length)
                            {
                                string str6 = strArray3[index];
                                if (!str4.Equals(str6))
                                {
                                    index++;
                                    continue;
                                }
                                list3.Add(enumerable3);
                                list4 = list3;
                            }
                            else
                            {
                                goto TR_00C3;
                            }
                            break;
                        }
                        break;
                    }
                }
                else
                {
                    goto TR_00C2;
                }
                goto TR_00C3;
            }
            return list4;
        }











        // Makes a call to search for local packages or online packages
        private List<IEnumerable<IPackageSearchMetadata>> GetPackages(string repositoryUrl)
        {
            List<IEnumerable<IPackageSearchMetadata>> returnedPkgs = new List<IEnumerable<IPackageSearchMetadata>>();



            // check if cache exists

            if (repositoryUrl.StartsWith("file://"))
            {

                FindLocalPackagesResourceV2 localResource = new FindLocalPackagesResourceV2(repositoryUrl);

                LocalPackageSearchResource resourceSearch = new LocalPackageSearchResource(localResource);
                LocalPackageMetadataResource resourceMetadata = new LocalPackageMetadataResource(localResource);

                SearchFilter filter = new SearchFilter(_prerelease);
                SourceCacheContext context = new SourceCacheContext();


                if ((_name == null) || (_name.Length == 0))
                {
                    returnedPkgs.AddRange(GetPackagesHelper(repositoryUrl, "", resourceSearch, resourceMetadata, filter, context));
                }

                foreach (var n in _name)
                {
                    returnedPkgs.AddRange(GetPackagesHelper(repositoryUrl, n, resourceSearch, resourceMetadata, filter, context));
                }
            }
            else
            {


                IEnumerable<Lazy<INuGetResourceProvider>> provider = FactoryExtensionsV3.GetCoreV3(NuGet.Protocol.Core.Types.Repository.Provider);

                PackageSource source = new PackageSource(repositoryUrl);

                if (_credential != null)
                {
                    string password = new NetworkCredential(string.Empty, _credential.Password).Password;
                    source.Credentials = PackageSourceCredential.FromUserInput(repositoryUrl, _credential.UserName, password, true, null);
                }

                SourceRepository repository = new SourceRepository(source, enumerable);
                PackageSearchResource result = repository.GetResourceAsync<PackageSearchResource>().GetAwaiter().GetResult();
                PackageMetadataResource resource = repository.GetResourceAsync<PackageMetadataResource>().GetAwaiter().GetResult();
                SearchFilter filter = new SearchFilter(_prerelease);
                SourceCacheContext context = new SourceCacheContext();


                PackageSearchResource resource3 = new SourceRepository(new PackageSource(repositoryUrl), FactoryExtensionsV3.GetCoreV3(NuGet.Protocol.Core.Types.Repository.Provider)).GetResourceAsync<NuGet.Protocol.Core.Types.PackageSearchResource>().GetAwaiter().GetResult();




                if ((_name == null) || (_name.Length == 0))
                {
                    returnedPkgs.AddRange(GetPackagesHelper(repositoryUrl, ""));
                }

                foreach (var n in _name)
                {
                    returnedPkgs.AddRange(GetPackagesHelper(repositoryUrl, n));
                }
            }

            return returnedPkgs;
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
