
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Net;
using System.Text;
using System.Globalization;
using NuGet.Configuration;
using Microsoft.PowerShellGet.Repository;

using NuGet.Configuration;
using Microsoft.PowerShell.Commands.Internal.Format;
using System.Xml.Linq;

namespace Microsoft.PowerShellGet.Commands
{

    /// <summary>
    /// The Register-PSResourceRepository cmdlet registers the default repository for PowerShell modules.
    /// After a repository is registered, you can reference it from the Find-PSResource, Install-PSResource, and Publish-PSResource cmdlets.
    /// The registered repository becomes the default repository in Find-Module and Install-Module.
    /// It returns nothing.
    /// </summary>

    [Cmdlet(VerbsLifecycle.Register, "PSResourceRepository", DefaultParameterSetName = "NameParameterSet", SupportsShouldProcess = true,
        HelpUri = "<add>", RemotingCapability = RemotingCapability.None)]
    public sealed
    class UnregisterPSResourceRepository : PSCmdlet
    {
        private string PSGalleryRepoName = "PSGallery";

        /// <summary>
        /// Specifies the desired name for the repository to be registered.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "NameParameterSet")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get
            { return _name; }

            set
            { _name = value; }
        }
        private string _name;

       


        /// <summary>
        /// </summary>
        protected override void BeginProcessing()
        {
            /// Currently no processing for begin
        }

        /// <summary>
        /// </summary>
        protected override void ProcessRecord()
        {

            
        }


        /// <summary>
        /// </summary>
        protected override void EndProcessing()
        {
            /// Currently no processing for end
        }

    }




}
