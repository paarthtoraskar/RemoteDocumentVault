///////////////////////////////////////////////////////////////////////////////
// Receiver.cs - Document Vault Message Receiver                             //
//                                                                           //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2013           //
///////////////////////////////////////////////////////////////////////////////

/*
 *  Package Contents:
 *  -----------------
 *  This package defines two classes:
 *  Receiver
 *    Provides a wrapper around the VaultDispatcher.
 *  VaultDispatcher
 *    Makes the abstract class AbstractMessageDispatcher concrete, but for now
 *    adds no additional behaviors.
 *
 * Required Files:
 * - Receiver.cs                                             Handles incoming messages
 * - ICommLib.cs, AbstractCommunicator.cs, BlockingQueue.cs  Defines behavior of Communicators 
 * - ICommServiceLib.cs, CommServiceLibe.cs                  Defines message-passing service
 *
 *  Required References:
 *  - System.ServiceModel
 *  - System.Runtime.Serialization
 *
 *  Maintenace History:
 *  ver 2.0 : Nov 5, 2013
 *  - minor changes to host closure
 *  ver 1.0 : Oct 29, 2013
 *  - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace RDV
{
    public class VaultDispatcher : AbstractMessageDispatcher { }

    public class Receiver
    {
        public VaultDispatcher VaultDispatcher { get; set; }

        ServiceHost serviceHost = null;

        public Receiver(string url)
        {
            VaultDispatcher = new VaultDispatcher();
            VaultDispatcher.Verbose = true;
            VaultDispatcher.Name = "dispatcher";
            VaultDispatcher.Start();


            //ServiceHost serviceHost = null;
            try
            {
                serviceHost = Host.CreateChannel(url);
                serviceHost.Open();
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}", ex.Message);
            }
            finally
            {
                //serviceHost.Close();
            }
        }

        public void Stop()
        {
            VaultDispatcher.Stop();
        }

        public void Close()
        {
            if (serviceHost != null)
                serviceHost.Close();
        }

        public void Register(ICommService communicator)
        {
            VaultDispatcher.Register(communicator);
        }
    }
}
