// /*
//    Copyright 2013 Gibraltar Software, Inc.
//    
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// */

using System;
using System.Windows.Forms;
using log4net;
using log4net.Config;
using log4net.Core;

namespace Log4NetTest
{
    internal static class Program
    {
        // In this case, let's get a logger based on a string name.
        // Elsewhere, we will get logger based on full type name.  So, logically,
        // the messages we write here will appear to be associated with the Log4NetTest
        // namespace.
        private static readonly ILog log = LogManager.GetLogger( "Log4NetTest" );

        [ STAThread ]
        private static void Main()
        {
            // This is a crucial line of code that tells log4net to load its configuration
            // from the application config file
            XmlConfigurator.Configure();

            log.Info( "Entering application." );
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new TestForm() );
            log.Info( "Exiting application." );
            LoggerManager.Shutdown();
        }
    }
}