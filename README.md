Loupe Agent for Log4Net
===================

This agent extends Log4Net to send messages to Loupe.  If you don't need
to modify the source code just download the latest 
[Loupe Agent for Log4Net](https://nuget.org/packages/Gibraltar.Agent.Log4Net). 
It extends the [Loupe Agent](https://nuget.org/packages/Gibraltar.Agent/) so you can 
use any viewer for Loupe to review the agent's information

Loupe was designed to be the best way to process and view Log4Net data available.

* Log4Net event levels map to Loupe Log Message Severity.  There are extra levels in Log4Net 
  however the structure is maintained and all values are mapped to sensible Loupe values by default.
* Log4Net Loggers are carried over as Loupe Log Category Names.  The hierarchal nature of loggers is maintained.
* Log4Net LocationInfo is augmented and stored in Loupe to provide the class & method 
  where each log message originated and, if available, the file name and line number.

Configuring Log4Net to use the Loupe Appender
---------------

To attach the Loupe Appender to Log4Net and route messages to it you need to modify your application configuration file. 
A minimal example is shown below:

```xml

<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net" />
  </configSections>
  <log4net>
    <appender name="GibraltarAppender" type="Gibraltar.Agent.Log4Net.GibraltarAppender, Gibraltar.Agent.Log4Net" >
      <param name="EndSessionOnAppenderClose" value="false" /> <!-- This is optional; it is false by default -->
    </appender>
    <root>
      <level value="ALL" />
      <appender-ref ref="GibraltarAppender" />
    </root>
  </log4net>
</configuration>                
```

If you compile the Loupe Appender source code into another assembly, you will need to change the 
type information above to reflect the correct class name and assembly that you want your application to use. 
The example above uses the Loupe Appender for Log4Net assembly.

For complete information on how to configure Log4Net to work with appenders, consult the Log4Net documentation.

Building the Agent
------------------

This project is designed for use with Visual Studio 2012 with NuGet package restore enabled.
When you build it the first time it will retrieve dependencies from NuGet.

Contributing
------------

Feel free to branch this project and contribute a pull request to the development branch.
If your changes are incorporated into the master version they'll be published out to NuGet for
everyone to use!
