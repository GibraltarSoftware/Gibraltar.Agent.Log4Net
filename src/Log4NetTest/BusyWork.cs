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

using System.Threading;
using log4net;

namespace Log4NetTest
{
    public class BusyWork
    {
        private readonly string m_Name;
        private readonly ILog log;

        public BusyWork( string name )
        {
            // In this case, we create a logger combining info from the type with aditional info
            log = LogManager.GetLogger( typeof( TestForm ).FullName + "." + name );
            m_Name = name;
        }

        public void Run( int messages )
        {
            // Queue the task and data.
            if ( !ThreadPool.QueueUserWorkItem( new WaitCallback( ThreadProc ), messages ) )
                log.Warn( "QueueUserWorkItem failed" );
        }

        private void ThreadProc( object info )
        {
            int count = ( int )info;

            for ( int i = 1; i <= count; i++ )
            {
                log.DebugFormat( "{0} message {1} of {2}", m_Name, i, count );
                Thread.Sleep( 500 );
            }
        }
    }
}