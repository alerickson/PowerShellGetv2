// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

//using System;
//using System.Management.Automation;
//using Microsoft.PowerShell.Commands;
//using Microsoft.PowerShell.PowerShellGet.RepositorySettings;



namespace Microsoft.PowerShell.PowerShellGet.Cmdlets
{
    /// <summary>
    /// It retrieves a resource that was installEd with Install-PSResource
    /// Returns a single resource or multiple resource.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "PSResource", SupportsShouldProcess = true,
        HelpUri = "<add>", RemotingCapability = RemotingCapability.None)]
    public sealed
    class GetPSResource : PSCmdlet
    {
        /// <summary>
        /// Specifies the desired name for the resource to look for.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = "NameParameterSet")]
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
        /// Specifies the version of the resource to include to look for. 
        /// </summary>
        [Parameter(ParameterSetName = "NameParameterSet")]
        [ValidateNotNullOrEmpty()]
        public string Version
        {
            get
            { return _version; }

            set
            { _version = value; }
        }
        private string _version;




        /// <summary>
        /// </summary>
        protected override void ProcessRecord()
        {
            // list all installation locations:
            var root = "c:/code/temp/test";


            /// list all the modules, or the specific name / version
            /// output:  version  |    name     |   repository      |   description



            /*

            foreach (var repo in listOfRepositories)
            {
                var repoPSObj = new PSObject();
                repoPSObj.Members.Add(new PSNoteProperty("Name", repo.Name));
                repoPSObj.Members.Add(new PSNoteProperty("Url", repo.Url));
                repoPSObj.Members.Add(new PSNoteProperty("Trusted", repo.Trusted));
                repoPSObj.Members.Add(new PSNoteProperty("Priority", repo.Priority));
                WriteObject(repoPSObj);
            }

    */
        }
    }
}
