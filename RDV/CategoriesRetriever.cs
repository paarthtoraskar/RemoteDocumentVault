/////////////////////////////////////////////////////////////////////
//	Provides functionality to search for text strings
//
//	Paarth Toraskar
//	SU - MSCE - S/W
//	pbtorask@syr.edu
/////////////////////////////////////////////////////////////////////

/*
 *  Package Contents:
 *  -----------------
 *  This package defines four classes:
 *  Server:
 *    Provides prototype behavior for the DocumentVault server.
 *  EchoCommunicator:
 *    Simply diplays its messages on the server Console.
 *  QueryCommunicator:
 *    Serves as a placeholder for query processing.  You should be able to
 *    invoke your Project #2 query processing from the ProcessMessages function.
 *  NavigationCommunicator:
 *    Serves as a placeholder for navigation processing.  You should be able to
 *    invoke your navigation processing from the ProcessMessages function.
 * 
 *  Required Files:
 *  - Server:      Server.cs, Sender.cs, Receiver.cs
 *  - Components:  ICommLib, AbstractCommunicator, BlockingQueue
 *  - CommService: ICommService, CommService
 *
 *  Required References:
 *  - System.ServiceModel
 *  - System.RuntimeSerialization
 *
 *  Build Command:  devenv Project4HelpF13.sln /rebuild debug
 *
 *  Maintenace History:
 *  ver 2.1 : Nov 7, 2013
 *  - replaced ServerSender with a merged Sender class
 *  ver 2.0 : Nov 5, 2013
 *  - fixed bugs in the message routing process
 *  ver 1.0 : Oct 29, 2013
 *  - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace RDV
{
    /// <summary>
    /// Retrieves categories that span across all files in the vault
    /// </summary>
    class ClientCategoriesRetrieverCommunicator : AbstractCommunicator
    {
        public Action<List<string>> UpdateData;

        // starts the processing of incoming messages
        protected override void ProcessMessages()
        {
            while (true)
            {
                ServiceMessage msg = bq.deQ();
                Console.Write("\n  {0} Received Message:\n", msg.TargetCommunicator);
                msg.ShowMessage();
                if ((string)msg.Contents == "quit")
                {
                    if (Verbose)
                        Console.Write("\n  ClientCategoriesRetrieverCommunicator shutting down");

                    // shut down dispatcher

                    msg.TargetCommunicator = "dispatcher";
                    AbstractMessageDispatcher.GetInstance().PostMessage(msg);
                    break;
                }
                processServiceMsg(msg);
            }
        }

        // performs specific actions related to this communicator on the incoming message
        void processServiceMsg(ServiceMessage msg)
        {
            List<string> categories = new List<string>();

            unzipCategoriesRetrieverServerMessage(msg, ref categories);

            if (Application.Current.Dispatcher.CheckAccess())
                UpdateData(categories);
            else
                Application.Current.Dispatcher.Invoke(
                  UpdateData,
                  System.Windows.Threading.DispatcherPriority.Background,
                  new object[] { categories }
                );
        }

        // disassebles the message sent by the server for this communicator
        void unzipCategoriesRetrieverServerMessage(ServiceMessage msg, ref List<string> categories)
        {
            XDocument xDoc = XDocument.Parse(msg.Contents.ToString());
            var ts = (from item in xDoc.Descendants() where item.Name == "Category" select item).ToList();
            foreach (var item in ts)
                categories.Add(item.Value);
        }
    }
}
