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

namespace Log4NetTest
{
    public partial class TestForm : Form
    {
        // In this case, we define a logger based on a type.  Internal to log4net
        // this is equivalent to just entering the full typename as a string.
        // In this case, that string would be Log4NetTest.TestForm
        private static readonly ILog log = LogManager.GetLogger( typeof( TestForm ) );

        public TestForm()
        {
            log.Info( "Entering TestForm." );
            InitializeComponent();
        }

        protected override void OnClosed( EventArgs e )
        {
            log.Info( "Exiting TestForm." );
        }

        private void BusyWorker_Click( object sender, EventArgs e )
        {
            BusyWork worker = new BusyWork( (( Button )sender).Text );
            worker.Run( 10 );
        }
    }
}