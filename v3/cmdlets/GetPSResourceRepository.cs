// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Diagnostics.CodeAnalysis;
//using System.Management.Automation;
//using System.Management.Automation.Internal;
//using System.Net;
//using System.Text;

using Microsoft.PowerShell.Commands.Internal.Format;



namespace Microsoft.PowerShellGet.Commands
{
    /// <summary>
    /// The Register-PSResourceRepository cmdlet.
    /// It retrieves a repository that was registered with Register-PSResourceRepository
    /// Returns a single repository or multiple repositories.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "PSResourceRepository", SupportsShouldProcess = true,
        HelpUri = "<add>", RemotingCapability = RemotingCapability.None)]
    public sealed
    class RegisterPSResourceRepository : PSCmdlet
    {
        /// <summary>
        /// Specifies the desired name for the repository to be registered.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = trueParameterSet)]
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
        /// </summary>
        protected override void BeginProcessing()
        {
            // 1. find xml
            //Microsoft.PowerShell.Management\Join-Path -Path $script:PSGetAppLocalPath -ChildPath "PSRepositories.xml"
        }

        /// <summary>
        /// </summary>
        protected override void ProcessRecord()
        {

            if (_name)
                foreach (string repo in _name)
                {

                    // Get package source

                    // get new module source from package source

                    //Find and return repository
                    /*
                    PSResourceRepository = [PSCustomObject] @{
                        Name = $Name
                        URL = "placeholder-for-url"
                        Trusted = "placeholder-for-trusted"
                        Priority = "placeholder-for-priority"
                    }
                    */
                    //return $PSResourceRepository

                }
        }



    }



    /// <summary>
    /// </summary>
    protected override void EndProcessing()
    {
        /// Currently no processing for end
    }

}
