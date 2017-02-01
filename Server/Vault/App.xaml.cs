using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace RDV
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static string ServerUrl = "http://localhost:8000/CommService";
        public static string ClientUrl = "http://localhost:8001/CommService";

        internal static Sender clientSender;
        internal static Receiver clientReceiver;

        internal static ClientFileReceiver clientFileReceiver;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            clientSender = new Sender();
            clientSender.Name = "ClientSender";
            clientSender.Connect(ServerUrl);
            clientSender.Start();

            clientReceiver = new Receiver(ClientUrl);

            clientFileReceiver = new ClientFileReceiver();
            ClientFileReceiver.FilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            clientFileReceiver.PrepareToReceiveFile("http://localhost:8090/ClientFileService");
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            clientSender.Stop();
            clientSender.Wait();

            foreach (var item in clientReceiver.VaultDispatcher.CommunicatorLookup.Keys)
            {
                var ICommService = clientReceiver.VaultDispatcher.CommunicatorLookup[item] as AbstractCommunicator;
                if (ICommService.Name != "ClientSender")
                {
                    ICommService.Stop();
                    ICommService.Wait();
                }
            }
            clientReceiver.Stop();
            clientReceiver.Close();
        }
    }
}
