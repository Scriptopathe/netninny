using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetNinny.Core;
using NetNinny.Filtering;

namespace NetNinny.Service
{
    class Program
    {
        /// <summary>
        /// Displays an help message.
        /// </summary>
        static void DisplayHelp()
        {
            Console.WriteLine("Usage: ");
            Console.WriteLine("[-v] [--port=portnumber] [--filters=filters_file] [--force_utf8]");
        }

        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            const int DEFAULT_PORT = 5001;
            bool verboseMode = false;
            bool forceUtf8 = false;
            int port = DEFAULT_PORT;
            string filtersCollectionFile = "";
            FilterCollection collection = null;


            // Retrieve command line arguments
            foreach(string arg in args)
            {
                if(arg.Contains("--help"))
                {
                    DisplayHelp();
                    return;
                }
                // ------ FEATURE 7: Configurable proxy port ------ 
                else if (arg.Contains("--port="))
                {
                    if (!Int32.TryParse(arg.Split('=')[1], out port) || port > 65535 || port < 1024)
                    {
                        Console.WriteLine("Invalid port number (should be in the range [1024, 65535].");
                        Console.WriteLine("Now Exiting.");
                        System.Environment.Exit(0);
                    }
                }
                else if (arg == "-v")
                    verboseMode = true;
                else if (arg == "--force_utf8")
                    forceUtf8 = true;
                else if (arg.Contains("--filters="))
                {
                    filtersCollectionFile = arg.Split('=')[1].Trim('"');
                }
            }

            // Outputs the command lines args :
            Console.WriteLine("------ NetNinny Web Proxy ------");
            Console.WriteLine("Current options :");
            Console.WriteLine("Verbose         : " + verboseMode);
            Console.WriteLine("Filters         : " + filtersCollectionFile);
            Console.WriteLine("Port            : " + port);
            Console.WriteLine("Force-Utf8      : " + forceUtf8);

            // Sets the correct verbosity.
            Tools.LogVerbosity verbosity = verboseMode ? Tools.LogVerbosity.UserInfo_Error_Warning_Verbose : Tools.LogVerbosity.UserInfo_Error;

            // Gets the filter collection.
            if (filtersCollectionFile != "")
            {
                try { collection = FilterCollection.FromFile(filtersCollectionFile); }
                catch { Console.WriteLine("Error reading the filters file. Default filters loaded."); }
            }

            if (collection == null)
                collection = new FilterCollection() { "SpongeBob", "Norrköping", "Paris Hilton", "Britney Spears" };



            // Creates and starts the server.
            Console.WriteLine("Now starting...");
            ProxyServer server = new ProxyServer();
            server.ForceUtf8 = forceUtf8;
            server.SetLogToConsole(verbosity);
            server.Start(port, "127.0.0.1", new FilterCollection() { "SpongeBob" });
            while (server.IsRunning)
            {
                string str = Console.ReadLine();
                if (str == "exit")
                    server.Stop();
                
            }

            Console.WriteLine("Server stopped.");
            Console.ReadLine();
            
        }
    }
}
