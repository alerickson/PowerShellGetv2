
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
    /// Sets a repository.
    /// It returns nothing.
    /// </summary>

    [Cmdlet(VerbsCommon.Set, "PSResourceRepository", DefaultParameterSetName = "NameParameterSet", SupportsShouldProcess = true,
        HelpUri = "<add>", RemotingCapability = RemotingCapability.None)]
    public sealed
    class SetPSResourceRepository : PSCmdlet
    {
        /// <summary>
        /// Specifies the desired name for the repository to be set.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = "NameParameterSet")]
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
        /// Specifies the location of the repository to be set.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "NameParameterSet")]
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
        /// Repositories is a hashtable and is used to register multiple repositories at once.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "RepositoriesParameterSet")]
        [ValidateNotNullOrEmpty]
        public Hashtable Repositories
        {
            get { return _repositories; }

            set { _repositories = value; }
        }

        private Hashtable _repositories



        /// <summary>
        /// Specifies whether the repository should be trusted.
        /// </summary>
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
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [ValidateRange(0, 50)]
        public int Priority
        {
            get { return _priority; }

            set { _priority = value; }
        }

        private int _priority = 50;






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
