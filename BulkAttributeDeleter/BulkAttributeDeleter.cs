using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;

/*
 * The intention of this code is to provide a simple method for the repeated deletion of a series of 
 * pre-defined attributes from a variety of systems. There is a tool already out there that deletes stuff
 * But my one takes a repeatable set of data from a file so can be run on multiple different systems with the same
 * basic field set in it
*/ 

namespace Meadowbank {
    class BulkAttributeDeleter {
        private  const string fileName = "deleteData.xml";
        private  const string outputFile = "log.txt";
        static void Main(string[] args) {
            try {
                DeleteData fred = new DeleteData();


                /*
                 //This section is used to construct the xml file that will ultimately serve up the data
                fred.entityName = "myEntity";
                fred.fieldList = new string[] { "First", "Second" };
                XmlSerializer x = new XmlSerializer(typeof(DeleteData));
                using (TextWriter myWriter = new StreamWriter("myData.xml")) {
                    x.Serialize(myWriter, fred);
                }
                return; 
                */

                //Set up the connections based upon the command line data
                if (args.Length < 2) {
                    Console.WriteLine("Nah. Need a url and tenant for the target system\ne.g. sumfing like http://crmdev11 EnterpriseInns");
                }
                if (File.Exists(outputFile)) File.Delete(outputFile);
                writeLog("System initialised.");
                writeLog("Target URL: " + args[0]);
                writeLog("Tenant: " + args[1]);
                IOrganizationService aService = GetOrganisationService(args[1], args[0]);

                DeleteData deleteData;
                XmlSerializer dx = new XmlSerializer(typeof(DeleteData));
                using (TextReader myReader = new StreamReader(fileName)) {
                    deleteData = (DeleteData)dx.Deserialize(myReader);
                }
                DeleteAttributeRequest dar = new DeleteAttributeRequest();
                dar.EntityLogicalName = deleteData.entityName;
                DeleteAttributeResponse delResponse;
                foreach (string attr in deleteData.fieldList) {
                    writeLog("");
                    writeLog("Deleting " + attr);
                    dar.LogicalName = attr;
                    try {
                        delResponse = (DeleteAttributeResponse)aService.Execute(dar);
                        writeLog("Delete successful");
                    } catch (System.ServiceModel.FaultException ssf) {
                        writeLog(ssf.Message);
                    }

                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        public static IOrganizationService GetOrganisationService(string tenant, string siteUrl) {
            OrganizationServiceProxy sp;
            Uri connectionUri = new Uri(siteUrl + (siteUrl.EndsWith("/") ? "" : "/") + tenant + "/XRMServices/2011/Organization.svc");
            ClientCredentials cc = new ClientCredentials(); cc.Windows.ClientCredential = (System.Net.NetworkCredential)System.Net.CredentialCache.DefaultCredentials;
            sp = new OrganizationServiceProxy(connectionUri, null, cc, null);
            return (IOrganizationService)sp;
        }

        public static void writeLog(string message){
            Console.WriteLine(message);
            using(TextWriter myWriter=new StreamWriter(outputFile,true)){
                myWriter.WriteLine(message);
            }
        }
    }

    /// <summary>
    /// The data class that will be deserialised from an xml file to determine which fields to be deleted.
    /// </summary>
    public class DeleteData  {
        public string entityName;
        public string[] fieldList;
    }
}

