
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using System;
using System.IO;
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
using Microsoft.PowerShell.Commands.Internal.Format;
using System.Xml.Linq;
using NuGet.Common;
using System.Xml;
using System.Security.Policy;
using System.Linq;

namespace Microsoft.PowerShellGet.Repository
{

    /// <summary>
    /// Repository settings
    /// </summary>

    class RespositorySettings
    {
        /// <summary>
        /// Default file name for a settings file is 'psresourcerepository.config'
        /// Also, the user level setting file at '%APPDATA%\NuGet' always uses this name
        /// </summary>
        public static readonly string DefaultRepositoryFileName = "PSResourceRepository.xml";
        public static readonly string DefaultRepositoryPath = "c:/code/temp"; //@"%APPDTA%\NuGet";
        public static readonly string DefaultFullRepositoryPath = Path.Combine(DefaultRepositoryPath, DefaultRepositoryFileName);


        public RespositorySettings(){

        }


        /// <summary>
        /// Find a repository XML
        /// Returns:
        /// </summary>
        /// <param name="sectionName"></param>
        /// /// TEMP DONE
        public bool FindRepositoryXML()
        {
            // Search in the designated location for the repository XML
            if (File.Exists(DefaultFullRepositoryPath))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Create a new repository XML
        /// Returns: void
        /// </summary>
        /// <param name="sectionName"></param>
        // consider adding parameters for creating of new file
        /// /// TEMP DONE
        public void CreateNewRepositoryXML()
        {
            // Check to see if the file already exists; if it does return
            if (FindRepositoryXML())
            {
                return;
            }

            // If the repository xml file doesn't exist yet, create one
            XDocument newRepoXML = new XDocument(
                    new XElement("configuration")
            );

            // Should be saved in: 
            newRepoXML.Save(DefaultFullRepositoryPath);
        }



        /// <summary>
        /// Add a repository to the XML
        /// Returns: void
        /// </summary>
        /// <param name="sectionName"></param>
        /// TEMP DONE
        public void Add(string repoName, string repoURL, string repoPriority, string repoTrusted)
        {
            // Check to see if information we're trying to add to the repository is valid
            if (string.IsNullOrEmpty(repoName))
            {
                // throw new ArgumentException(Resources.Argument_Cannot_Be_Null_Or_Empty, nameof(sectionName));
                throw new ArgumentException("Repository name cannot be null or empty");
            }
            if (string.IsNullOrEmpty(repoURL))
            {
                // throw new ArgumentException(Resources.Argument_Cannot_Be_Null_Or_Empty, nameof(sectionName));
                throw new ArgumentException("Repository URL cannot be null or empty");
            }

            // Call CreateNewRepositoryXML()  [Create will make a new xml if one doesn't already exist]
            try
            {
                CreateNewRepositoryXML();
            }
            catch {
                throw new ArgumentException("Was not able to successfully create xml");
            }

            // open file 
            XDocument doc = XDocument.Load(DefaultFullRepositoryPath);

            // Check if what's being added already exists, if it does throw an error
            var node = doc.Descendants("Repository").SingleOrDefault(e => (string)e.Attribute("Url") == repoURL);
            if (node != null)
            {
                throw new ArgumentException("Repository already exists");
            }

            // Else, keep going
            // Get root of XDocument (XElement)
            var root = doc.Root;

            // Create new element
            XElement newElement = new XElement(
                "Repository",
                new XAttribute("Name", repoName),
                new XAttribute("Url", repoURL),
                new XAttribute("Priority", repoPriority),
                new XAttribute("Trusted", repoTrusted)
                );

            root.Add(newElement);

            // close the file
            root.Save(DefaultFullRepositoryPath);
            //doc.Save(DefaultFullRepositoryPath);
        }

        /// <summary>
        /// Updates a repository name, URL, priority, or installation policy
        /// Returns:  void
        /// </summary>
        /// if the user wants a value to be null, we'll check 
        /// TEMP UPDATE
        public void Update(string repoName, string repoURL, string repoPriority, string repoTrusted)
        {
        
            // write to the file

            // close the file



            // Check to see if information we're trying to add to the repository is valid
            if (string.IsNullOrEmpty(repoName))
            {
                // throw new ArgumentException(Resources.Argument_Cannot_Be_Null_Or_Empty, nameof(sectionName));
                throw new ArgumentException("Repository name cannot be null or empty");
            }


            // Call FindRepositoryXML()  [We expect the xml to exist, if it doesn't user needs to register a repository]
            try
            {
                FindRepositoryXML();
            }
            catch
            {
                throw new ArgumentException("Was not able to successfully find xml-- try registering a repository first");
            }

            // open file 
            XDocument doc = XDocument.Load(DefaultFullRepositoryPath);

            // Check if what's being updated is actually there first
            var node = doc.Descendants("Repository").SingleOrDefault(e => (string)e.Attribute("Name") == repoName);
            if (node == null)
            {
                throw new ArgumentException("Cannot find the repository because it does not exist. try registering the repository");
            }

            // Else, keep going
            // Get root of XDocument (XElement)
            var root = doc.Root;

            // Create new element
            /*
            XElement newElement = new XElement(
                "Repository",
                new XAttribute("Name", repoName),
                new XAttribute("Url", repoURL),
                new XAttribute("Priority", repoPriority),
                new XAttribute("Trusted", repoTrusted)
                );
            */

            if (!String.IsNullOrEmpty(repoURL))
            {
                node.Attribute("Url").Value = repoURL;
            }

            if (!String.IsNullOrEmpty(repoPriority))
            {
                node.Attribute("Priority").Value = repoPriority;
            }

            if (!String.IsNullOrEmpty(repoTrusted))
            {
                node.Attribute("Trusted").Value = repoTrusted;
            }

            // close the file
            root.Save(DefaultFullRepositoryPath);
            //doc.Save(DefaultFullRepositoryPath);

        }






        /// TODO
        /// 
        /// 
        /// <summary>
        /// Removes a repository from the XML
        /// Returns: void
        /// 
        /// Temp DONE
        /// </summary>
        /// <param name="sectionName"></param>

        public void Remove(string repoName)
        {

            // Check to see if information we're trying to add to the repository is valid
            if (string.IsNullOrEmpty(repoName))
            {
                // throw new ArgumentException(Resources.Argument_Cannot_Be_Null_Or_Empty, nameof(sectionName));
                throw new ArgumentException("Repository name cannot be null or empty");
            }

            // Call FindRepositoryXML()  [Create will make a new xml if one doesn't already exist]
            if (!FindRepositoryXML())
            {
                throw new ArgumentException("Was not able to successfully find xml");
            }
            
            // open file 
            XDocument doc = XDocument.Load(DefaultFullRepositoryPath);

            // Check if what's being added doesn't already exist, throw an error
            var node = doc.Descendants("Repository").SingleOrDefault(e => (string)e.Attribute("Name") == repoName);
            if (!node.IsEmpty)
            {
                throw new ArgumentException("Repository does not exist");
            }

            // Else, keep going
            // Get root of XDocument (XElement)
            var root = doc.Root;

            // remove item from file
            node.Remove();

            // close the file
            root.Save(DefaultFullRepositoryPath);
            //doc.Save(DefaultFullRepositoryPath);
        }



        // should make this an array
        public void Read(string repoName)
        {

            // Check to see if information we're trying to add to the repository is valid
            if (string.IsNullOrEmpty(repoName))
            {
                // throw new ArgumentException(Resources.Argument_Cannot_Be_Null_Or_Empty, nameof(sectionName));
                throw new ArgumentException("Repository name cannot be null or empty");
            }

            // Call FindRepositoryXML()  [Create will make a new xml if one doesn't already exist]
            if (!FindRepositoryXML())
            {
                throw new ArgumentException("Was not able to successfully find xml");
            }

            // open file 
            XDocument doc = XDocument.Load(DefaultFullRepositoryPath);


            // return some object?

            // turn into array later
            // Check if what's being added doesn't already exist, throw an error
            var node = doc.Descendants("Repository").SingleOrDefault(e => (string)e.Attribute("Name") == repoName);
            if (node == null)
            {
                throw new ArgumentException("Repository does not exist");
            }

        

         
        }



    }



}
