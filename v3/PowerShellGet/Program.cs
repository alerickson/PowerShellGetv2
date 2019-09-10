using System;
using Microsoft.PowerShellGet.Commands;
using Microsoft.PowerShellGet.Repository;

namespace PowerShellGet
{
    class Program
    {
        static void Main(string[] args)
        {
            RespositorySettings r = new RespositorySettings();



            
            //Find Repository XML
            if (r.FindRepositoryXML())
            {
                Console.Out.WriteLine("Found repository xml!");
            }
            else {
                Console.Out.WriteLine("Did NOT find repository xml");
            }


            //Create a new repository XML -- works
            r.CreateNewRepositoryXML();

            if (r.FindRepositoryXML())
            {
                Console.Out.WriteLine("Found repository xml!");
            }
            else
            {
                Console.Out.WriteLine("Did NOT find repository xml");
            }




            //Test add
            r.Add("testRepo1", "https://www.testrepo1.org", "2", "Trusted");
            r.Add("testRepo7", "https://www.testrepo7.org", "7", "Trusted");

            //Update
            r.Update("testRepo1", "https://www.testrepo2.org", "3", "UnTrusted");


            //Test remove
            r.Remove("testRepo1");



            //Test read
            r.Read("testRepo7");

            




            Console.WriteLine("Starting program.");
        }
    }
}
