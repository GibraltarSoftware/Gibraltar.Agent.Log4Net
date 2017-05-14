#region File Header
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
using log4net.Appender;
using log4net.Core;
using log4net.Repository;

#endregion File Header

namespace Gibraltar.Agent.Log4Net
{
    /// <summary>
    /// The Log4Net-compatible appender for Gibraltar logging
    /// </summary>
    /// <remarks>When registered with Log4Net via the App.Config file, this class will
    /// automatically capture logging data from Log4Net and record it into the current 
    /// log file, along with any other logging information that is being captured such as
    /// metrics and information from trace logging.</remarks>
    public class GibraltarAppender : AppenderSkeleton, IDisposable
    {
        private bool _disposed;

        private const int SeverityCriticalDefault = 90000; // from log4net.Core.Level.Critical.Value
        private const int SeverityErrorDefault = 70000;    // from log4net.Core.Level.Error.Value
        private const int SeverityWarnDefault = 60000;     // from log4net.Core.Level.Warn.Value
        private const int SeverityInfoDefault = 40000;     // from log4net.Core.Level.Info.Value
        private const int SeverityVerboseDefault = 0;      // assumes no negative values are used for real events!

        // The threshold for Verbose is used as a filter to discard events below that level.
        // By default, we would want to just take everything, but we set a default threshold of 0,
        // so that any crazy negative-level events (bizarre usage) would be filtered out as presumably
        // not intended for serious collection, being far below even the Level.Verbose listening level.
        // However, this can be overridden in AppConfig by setting the Verbose threshold to "All".

        private int _severityCriticalThreshold = SeverityCriticalDefault;
        private int _severityErrorThreshold = SeverityErrorDefault;
        private int _severityWarnThreshold = SeverityWarnDefault;
        private int _severityInfoThreshold = SeverityInfoDefault;
        private int _severityVerboseThreshold = SeverityVerboseDefault;
        private bool _severitiesProcessed; // leave false to ensure lookup of levels (will overwrite the defaults above)
        private string _severityCritical;
        private string _severityError;
        private string _severityWarn;
        private string _severityInfo;
        private string _severityVerbose;
        private bool _endSessionOnAppenderClose;

        private readonly object _configLock = new object(); // Our lock target for thread-safe access to our member variables

        /// <summary>
        /// A property to receive the configuration setting for the EndSessionOnAppenderClose parameter.
        /// </summary>
        public bool EndSessionOnAppenderClose
        {
            get
            {
                lock (_configLock)
                {
                    return _endSessionOnAppenderClose;
                }
            }
            set
            {
                lock (_configLock)
                {
                    _endSessionOnAppenderClose = value;
                }
            }
        }

        /// <summary>
        /// A property to receive the configuration string for the SeverityCritical config parameter.
        /// </summary>
        public string SeverityCritical
        {
            get
            {
                lock (_configLock)
                {
                    return _severityCritical;
                }
            }
            set
            {
                lock (_configLock)
                {
                    _severitiesProcessed = false; // Update thresholds upon next usage
                    _severityCritical = value;
                }
            }
        }

        /// <summary>
        /// A property to receive the configuration string for the SeverityFatal config parameter.
        /// </summary>
        /// <remarks>This is an alias for the SeverityCritical config parameter.</remarks>
        public string SeverityFatal
        {
            get
            {
                lock (_configLock)
                {
                    return _severityCritical;
                }
            }
            set
            {
                lock (_configLock)
                {
                    _severitiesProcessed = false; // Update thresholds upon next usage
                    _severityCritical = value;
                }
            }
        }

        /// <summary>
        /// A property to receive the configuration string for the SeverityError config parameter.
        /// </summary>
        public string SeverityError
        {
            get
            {
                lock (_configLock)
                {
                    return _severityError;
                }
            }
            set
            {
                lock (_configLock)
                {
                    _severitiesProcessed = false; // Update thresholds upon next usage
                    _severityError = value;
                }
            }
        }

        /// <summary>
        /// A property to receive the configuration string for the SeverityWarn config parameter.
        /// </summary>
        /// <remarks>This is an alias for the SeverityWarning config parameter.</remarks>
        public string SeverityWarn
        {
            get
            {
                lock (_configLock)
                {
                    return _severityWarn;
                }
            }
            set
            {
                lock (_configLock)
                {
                    _severitiesProcessed = false; // Update thresholds upon next usage
                    _severityWarn = value;
                }
            }
        }

        /// <summary>
        /// A property to receive the configuration string for the SeverityWarning config parameter.
        /// </summary>
        public string SeverityWarning
        {
            get
            {
                lock (_configLock)
                {
                    return _severityWarn;
                }
            }
            set
            {
                lock (_configLock)
                {
                    _severitiesProcessed = false; // Update thresholds upon next usage
                    _severityWarn = value;
                }
            }
        }

        /// <summary>
        /// A property to receive the configuration string for the SeverityInfo config parameter.
        /// </summary>
        /// <remarks>This is an alias for the SeverityInformation config parameter.</remarks>
        public string SeverityInfo
        {
            get
            {
                lock (_configLock)
                {
                    return _severityInfo;
                }
            }
            set
            {
                lock (_configLock)
                {
                    _severitiesProcessed = false; // Update thresholds upon next usage
                    _severityInfo = value;
                }
            }
        }

        /// <summary>
        /// A property to receive the configuration string for the SeverityInformation config parameter.
        /// </summary>
        public string SeverityInformation
        {
            get
            {
                lock (_configLock)
                {
                    return _severityInfo;
                }
            }
            set
            {
                lock (_configLock)
                {
                    _severitiesProcessed = false; // Update thresholds upon next usage
                    _severityInfo = value;
                }
            }
        }

        /// <summary>
        /// A property to receive the configuration string for the SeverityVerbose config parameter.
        /// </summary>
        public string SeverityVerbose
        {
            get
            {
                lock (_configLock)
                {
                    return _severityVerbose;
                }
            }
            set
            {
                lock (_configLock)
                {
                    _severitiesProcessed = false; // Update thresholds upon next usage
                    _severityVerbose = value;
                }
            }
        }

        /// <summary>
        /// A property to receive the configuration string for the SeverityDebug config parameter.
        /// </summary>
        /// <remarks>This is an alias for the SeverityVerbose config parameter.</remarks>
        public string SeverityDebug
        {
            get
            {
                lock (_configLock)
                {
                    return _severityVerbose;
                }
            }
            set
            {
                lock (_configLock)
                {
                    _severitiesProcessed = false; // Update thresholds upon next usage
                    _severityVerbose = value;
                }
            }
        }

        /// <summary>
        /// Process the severity threshold configuration settings before they are used.
        /// </summary>
        /// <param name="levelMap">The LevelMap maintained by the repository from which we receive LoggingEvent entries.</param>
        /// <remarks><para>This method ensures that the threshold configuration settings have been processed,
        /// and ensures that the thresholds are set in a legal and consistent way.  This method should
        /// be called before using the thresholds, and will return immediately if the config settings
        /// have already been processed since the last time they were touched.</para>
        /// <para>If the thresholds are then to be accessed, the caller should do so within a lock (on m_ConfigLock)
        /// around the call to this method, to ensure the config doesn't get updated in between.  If the caller
        /// is invoking us but not accessing the thresholds (e.g. to move this expensive logic out of the event-handling
        /// path), then a lock is not necessary since this method will lock within itself.</para></remarks>
        private void ProcessSeverityThresholdSettings(LevelMap levelMap)
        {
            lock (_configLock) // Generally a second lock since we already lock ourselves in GibraltarMessageSeverity()
            {
                if (_severitiesProcessed)
                {
                    return; // Nothing to update
                }

                int levelValue; // output placeholder for TryParse calls (needs outer scope?)

                // Process in order from most severe to least.  Inconsistencies should adjust the lower severity's threshold.

                if (String.IsNullOrEmpty(_severityCritical))
                {
                    _severityCriticalThreshold = Level.Critical.Value;
                }
                else if (int.TryParse(_severityCritical, out levelValue))
                {
                    // They gave us a raw integer value which we successfully parsed, so use that.
                    _severityCriticalThreshold = levelValue;
                }
                else if (_severityCritical == "const")
                {
                    // special (case-sensitive!) case to get our hard-coded const default
                    _severityCriticalThreshold = SeverityCriticalDefault;
                }
                else
                {
                    Level level = levelMap[_severityCritical];
                    if (ReferenceEquals(level, null) && _severityCritical != "Critical")
                    {
                        // They gave us a non-existent Level name, try again with our default name

                        string levelName = "Critical";
                        level = levelMap[levelName]; 

                        // They might have remapped the default "Fatal" level and ignored "Critical", so make sure we take
                        // whichever one is lower, to be safer.
                        Level fatal = levelMap["Fatal"];
                        if (!ReferenceEquals(level, null))
                        {
                            // We found "Critical", see if we also found "Fatal"
                            if (!ReferenceEquals(fatal, null))
                            {
                                // we found both "Critical" and "Fatal", take whichever is lower
                                if (fatal.Value < level.Value)
                                {
                                    level = fatal; // "Fatal" is lower, so use that
                                }
                                levelName = "minimum of Critical or Fatal";
                            }
                        }
                        else if (!ReferenceEquals(fatal, null))
                        {
                            level = fatal; // "Critical" wasn't found but "Fatal" was, so use that
                            levelName = "Fatal";
                        }

                        // In any case, this was a config error,
                        // so issue a one-time(?) Warn event for bad configuration setting (but we still work).
                        Log.TraceWarning("Invalid configuration option value \"{0}\" for {1}: {2} ({3})",
                                         _severityCritical, "SeverityCritical",
                                         "Named level not found in level map",
                                         "trying " + levelName + " instead");
                    }

                    if (!ReferenceEquals(level, null))
                    {
                        _severityCriticalThreshold = level.Value; // Get the integer value for the located level
                    }
                    else
                    {
                        _severityCriticalThreshold = Level.Critical.Value; // Still found nothing, get a default value
                        // Should this be a Warning or just Information?
                        Log.TraceInformation("Could not resolve bad configuration value \"{0}\" for {1}: {2} ({3})",
                                             _severityCritical, "SeverityCritical",
                                             "No level named Critical or Fatal found in level map",
                                             "defaulting to pre-defined Level.Critical");
                    }
                }

                // Todo: Should threshold for Critical be sanity-checked in any way?
                Log.TraceVerbose("Configuration of {0} threshold set to {1}", "SeverityCritical", _severityCriticalThreshold);

                if (String.IsNullOrEmpty(_severityError))
                {
                    _severityErrorThreshold = Level.Error.Value;
                }
                else if (int.TryParse(_severityError, out levelValue))
                {
                    // They gave us a raw integer value which we successfully parsed, so use that.
                    _severityErrorThreshold = levelValue;
                }
                else if (_severityError == "const")
                    // special (case-sensitive!) case to get our hard-coded const default
                {
                    _severityErrorThreshold = SeverityErrorDefault;
                }
                else
                {
                    Level level = levelMap[_severityError];
                    if (ReferenceEquals(level, null) && _severityError != "Error")
                    {
                        // They gave us a non-existent Level name, try again with our default name
                        level = levelMap["Error"];

                        // This was a config error, so issue a one-time(?) Warn event for bad configuration setting (but we still work).
                        Log.TraceWarning("Invalid configuration option value \"{0}\" for {1}: {2} ({3})",
                                         _severityError, "SeverityError",
                                         "Named level not found in level map",
                                         "trying Error instead");
                    }

                    if (!ReferenceEquals(level, null))
                    {
                        _severityErrorThreshold = level.Value; // Get the integer value for the located level
                    }
                    else
                    {
                        _severityErrorThreshold = Level.Error.Value; // Still found nothing, get a default value
                        // Should this be a Warning or just Information?
                        Log.TraceInformation("Could not resolve bad configuration value \"{0}\" for {1}: {2} ({3})",
                                             _severityError, "SeverityError",
                                             "No level named Error found in level map",
                                             "defaulting to pre-defined Level.Error");
                    }
                }

                if (_severityErrorThreshold > _severityCriticalThreshold) // Consistency check!
                {
                    // Invalid setting.  Error threshold higher than Critical can never be met.
                    // Issue a one-time(?) Warn event for bad configuration setting (but still works)
                    Log.TraceWarning("Improper severity threshold configuration: {0}={1} can't exceed {2}={3}",
                                     "SeverityError", _severityErrorThreshold,
                                     "SeverityCritical", _severityCriticalThreshold);
                    _severityErrorThreshold = _severityCriticalThreshold; // Fix the inconsistency.
                }
                if (_severityErrorThreshold == _severityCriticalThreshold)
                {
                    // Notice that we allow it to be set equal (give an Info event, or should this be a Warn?)
                    // which would mean that Error severity will never be seen (all are considered Critical).
                    Log.TraceInformation("Unusual severity threshold configuration: {0} is not below {1} and can thus never occur",
                                         "SeverityError", "SeverityCritical");
                }

                Log.TraceVerbose("Configuration of {0} threshold set to {1}", "SeverityError", _severityErrorThreshold);

                if (String.IsNullOrEmpty(_severityWarn))
                {
                    _severityWarnThreshold = Level.Warn.Value;
                }
                else if (int.TryParse(_severityWarn, out levelValue))
                {
                    // They gave us a raw integer value which we successfully parsed, so use that.
                    _severityWarnThreshold = levelValue;
                }
                else if (_severityWarn == "const") // special (case-sensitive!) case to get our hard-coded const default
                {
                    _severityWarnThreshold = SeverityWarnDefault;
                }
                else
                {
                    Level level = levelMap[_severityWarn];
                    if (ReferenceEquals(level, null) && _severityWarn != "Warn")
                    {
                        // They gave us a non-existent Level name, try again with our default name
                        level = levelMap["Warn"];

                        // This was a config error, so issue a one-time(?) Warn event for bad configuration setting (but we still work).
                        Log.TraceWarning("Invalid configuration option value \"{0}\" for {1}: {2} ({3})",
                                         _severityWarn, "SeverityWarn",
                                         "Named level not found in level map",
                                         "trying Warn instead");
                    }

                    if (!ReferenceEquals(level, null))
                    {
                        _severityWarnThreshold = level.Value; // Get the integer value for the located level
                    }
                    else
                    {
                        _severityWarnThreshold = Level.Warn.Value; // Still found nothing, get a default value
                        // Should this be a Warning or just Information?
                        Log.TraceInformation("Could not resolve bad configuration value \"{0}\" for {1}: {2} ({3})",
                                             _severityWarn, "SeverityWarn",
                                             "No level named Warn found in level map",
                                             "defaulting to pre-defined Level.Warn");
                    }
                }

                if (_severityWarnThreshold > _severityErrorThreshold) // Consistency check!
                {
                    // Invalid setting.  Warn threshold higher than Error can never be met.
                    // Issue a one-time(?) Warn event for bad configuration setting (but still works)
                    Log.TraceWarning("Improper severity threshold configuration: {0}={1} can't exceed {2}={3}",
                                     "SeverityWarn", _severityWarnThreshold,
                                     "SeverityError", _severityErrorThreshold);
                    _severityWarnThreshold = _severityErrorThreshold; // Fix the inconsistency.
                }
                if (_severityWarnThreshold == _severityErrorThreshold)
                {
                    // Notice that we allow it to be set equal (give an Info event, or should this be a Warn?)
                    // which would mean that Warning severity will never be seen (all are considered Error).
                    Log.TraceInformation("Unusual severity threshold configuration: {0} is not below {1} and can thus never occur",
                                         "SeverityWarn", "SeverityError");
                }

                Log.TraceVerbose("Configuration of {0} threshold set to {1}", "SeverityWarn", _severityWarnThreshold);

                if (String.IsNullOrEmpty(_severityInfo))
                {
                    _severityInfoThreshold = Level.Info.Value;
                }
                else if (int.TryParse(_severityInfo, out levelValue))
                {
                    // They gave us a raw integer value which we successfully parsed, so use that.
                    _severityInfoThreshold = levelValue;
                }
                else if (_severityInfo == "const") // special (case-sensitive!) case to get our hard-coded const default
                {
                    _severityInfoThreshold = SeverityInfoDefault;
                }
                else
                {
                    Level level = levelMap[_severityInfo];
                    if (ReferenceEquals(level, null) && _severityInfo != "Info")
                    {
                        // They gave us a non-existent Level name, try again with our default name
                        level = levelMap["Info"];

                        // This was a config error, so issue a one-time(?) Warn event for bad configuration setting (but we still work).
                        Log.TraceWarning("Invalid configuration option value \"{0}\" for {1}: {2} ({3})",
                                         _severityInfo, "SeverityInfo",
                                         "Named level not found in level map",
                                         "trying Info instead");
                    }

                    if (!ReferenceEquals(level, null))
                    {
                        _severityInfoThreshold = level.Value; // Get the integer value for the located level
                    }
                    else
                    {
                        _severityInfoThreshold = Level.Info.Value; // Still found nothing, get a default value
                        // Should this be a Warning or just Information?
                        Log.TraceInformation("Could not resolve bad configuration value \"{0}\" for {1}: {2} ({3})",
                                             _severityInfo, "SeverityInfo",
                                             "No level named Info found in level map",
                                             "defaulting to pre-defined Level.Info");
                    }
                }

                if (_severityInfoThreshold > _severityWarnThreshold) // Consistency check!
                {
                    // Invalid setting.  Info threshold higher than Warn can never be met.
                    // Issue a one-time(?) Warn event for bad configuration setting (but still works)
                    Log.TraceWarning("Improper severity threshold configuration: {0}={1} can't exceed {2}={3}",
                                     "SeverityInfo", _severityInfoThreshold,
                                     "SeverityWarn", _severityWarnThreshold);
                    _severityInfoThreshold = _severityWarnThreshold; // Fix the inconsistency.
                }
                if (_severityInfoThreshold == _severityWarnThreshold)
                {
                    // Notice that we allow it to be set equal (give an Info event, or should this be a Warn?)
                    // which would mean that Information severity will never be seen (all are considered Warning).
                    Log.TraceInformation("Unusual severity threshold configuration: {0} is not below {1} and can thus never occur",
                                         "SeverityInfo", "SeverityWarn");
                }

                Log.TraceVerbose("Configuration of {0} threshold set to {1}", "SeverityInfo", _severityInfoThreshold);

                // Note: SeverityVerbose is processed much the same way, but it defines a filter threshold.

                // This is a special case.  Everything below m_SeverityInfoThreshold is considered our Verbose, but we
                // allow a minimum threshold to be set for Verbose to define a point below which Gibraltar will ignore.
                // By default this is at 0, but can be changed to "All", explicitly to "Verbose", or to other legal settings.
                if (String.IsNullOrEmpty(_severityVerbose))
                {
                    _severityVerboseThreshold = Level.Verbose.Value;
                    // This is a special case because there is no static pre-defined Level with Value 0 to key on.
                    // By default we want "everything", but defaulting to 0 allows for events with negative-value Level to be
                    // ignored by Gibraltar.  But if they've redefined Level.Verbose itself to be negative, leave it at that.
                    if (_severityVerboseThreshold > 0 && _severityVerboseThreshold > SeverityVerboseDefault)
                    {
                        _severityVerboseThreshold = SeverityVerboseDefault; // Lower it to the default (normally 0)
                    }
                }
                else if (int.TryParse(_severityVerbose, out levelValue))
                {
                    // They gave us a raw integer value which we successfully parsed, so use that.
                    _severityVerboseThreshold = levelValue;
                }
                else if (_severityInfo == "const") // special (case-sensitive!) case to get our hard-coded const default
                {
                    _severityVerboseThreshold = SeverityVerboseDefault;
                }
                else
                {
                    Level level = levelMap[_severityVerbose];
                    if (ReferenceEquals(level, null) && _severityVerbose != "Verbose")
                    {
                        // They gave us a non-existent Level name, try again with our default name
                        level = levelMap["Verbose"];
                        string usingString = "using Verbose instead";
                        bool noVerbose = false;

                        if (!ReferenceEquals(level, null))
                        {
                            // Special case: unless the level we get is negative-valued, we need to lower it to our constant instead.
                            if (level.Value > 0 && level.Value > SeverityVerboseDefault)
                            {
                                level = null; // null it out so we fall through to the constant default below!
                                usingString = "overriding Verbose down to " + SeverityVerboseDefault;
                            }
                        }
                        else
                        {
                            noVerbose = true;
                        }

                        // This was a config error, so issue a one-time(?) Warn event for bad configuration setting (but we still work).
                        Log.TraceWarning("Invalid configuration option value \"{0}\" for {1}: {2} ({3})",
                                         _severityVerbose, "SeverityVerbose",
                                         "Named level not found in level map",
                                         usingString);

                        if (noVerbose)
                        {
                            // Should this be a Warning or just Information?
                            Log.TraceInformation("Could not resolve bad configuration value \"{0}\" for {1}: {2} ({3})",
                                                 _severityInfo, "SeverityInfo",
                                                 "No level named Verbose found in level map",
                                                 "defaulting to " + SeverityVerboseDefault);
                        }
                    }

                    if (!ReferenceEquals(level, null))
                    {
                        _severityVerboseThreshold = level.Value; // Get the integer value for the located level
                    }
                    else
                    {
                        _severityVerboseThreshold = SeverityVerboseDefault; // Still found nothing, use our default value
                    }
                }

                if (_severityVerboseThreshold > _severityInfoThreshold) // Consistency check!
                {
                    // Invalid setting.  Verbose threshold higher than Info can never be met.
                    // Issue a one-time(?) Warn event for bad configuration setting (but still works)
                    Log.TraceWarning("Improper severity threshold configuration: {0}={1} can't exceed {2}={3}",
                                     "SeverityVerbose", _severityVerboseThreshold,
                                     "SeverityInfo", _severityInfoThreshold);

                    // The consequences of such a configuration error are more significant here, since events get ignored!
                    // So this special case overrides the Verbose threshold back down to constant default, rather than
                    // just equal to Info threshold (unless Info threshold is also lower than constant default!).
                    // We're basically assuming that it was a mistake and not a deliberate attempt to set the
                    // Verbose threshold at or above the Info threshold.
                    if (_severityInfoThreshold > SeverityVerboseDefault) // Take whichever is lower!
                    {
                        _severityVerboseThreshold = SeverityVerboseDefault; // Override it down to constant default
                    }
                    else
                    {
                        _severityVerboseThreshold = _severityInfoThreshold; // Fix the inconsistency with an equal setting.
                    }

                    // Give an Information event that we overrode the Verbose threshold
                    Log.TraceInformation("Minimum threshold for recording events from log4net to Gibraltar overriden to {0}",
                                         _severityVerboseThreshold);
                }
                if (_severityVerboseThreshold == _severityInfoThreshold)
                {
                    // Notice that we allow it to be set equal (give an Info event, or should this be a Warn?)
                    // which would mean that Verbose severity will never be seen (all are considered Information).
                    Log.TraceInformation("Unusual severity threshold configuration: {0} is not below {1} and can thus never occur",
                                         "SeverityVerbose", "SeverityInfo");
                }

                Log.TraceVerbose("Configuration of {0} threshold set to {1}", "SeverityVerbose", _severityVerboseThreshold);

                _severitiesProcessed = true; // Mark our thresholds as valid so we don't have to process them again
            }
        }

        /// <summary>
        /// Temporary default constructor for a debugging breakpoint.
        /// </summary>
        public GibraltarAppender()
        {
            // Set this false by default, unless overridden by config.
            EndSessionOnAppenderClose = false; // Also handy for a breakpoint.
        }

        #region AppenderSkeleton Members

        /// <summary>
        /// Called when the appender is closed
        /// </summary>
        protected override void OnClose()
        {
            // We only really close our central Gibraltar Log when the application is actually exiting, because
            // log4net might allow appenders like us to be removed and new ones attached without exiting the process.
            // This allows Gibraltar to collect events from various possible logging systems into one central
            // log session for the process (e.g. even using both log4net and Trace in your application).

            // So we may actually have nothing we need to do here.

            // We should tell Gibraltar to shut down, unless our configuration tells us not to.
            if (EndSessionOnAppenderClose)
            {
                Log.EndSession(SessionStatus.Normal, 5, "Log4Net GibraltarAppender has been closed.");
            }
        }

        /// <summary>
        /// Write the provided logging event to the central Gibraltar log.
        /// </summary>
        /// <remarks>
        /// <para>The central Gibraltar logging class (Log) makes common decisions about when to write out logging information
        /// and where to write it out, therefore not all Log4Net logging events may be chosen to be permanently recorded</para>
        /// <para>Required as part of the Log4Net IAppender interface implementation.</para></remarks>
        /// <param name="loggingEvent">The individual LoggingEvent to be appended.</param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            // We shouldn't need a lock in this method because we don't access any of our member variables here,
            // and we don't need a recursion-guard because we don't send our own log events through log4net.

            // But we do need to first make sure the information we need is available from the LoggingEvent.
            // If it hasn't had any Fix flags set then we should be clear to access them without doing a Fix.

            const FixFlags ourFixFlags = FixFlags.Message | FixFlags.LocationInfo // | FixFlags.UserName
                                       | FixFlags.Identity | FixFlags.Exception;
            FixFlags fixedFlags = loggingEvent.Fix;
            if (fixedFlags != FixFlags.None)
            {
                // Hmmm, someone already did a Fix on this LoggingEvent, which apparently locks other values from
                // being updated upon reference.  So we need to make sure the ones we need are also "fixed".
                // But don't do this if the flags are None because we don't want to lock the loggingEvent against
                // being updated by other references if it isn't already.

                // ToDo: Find out if this is actually necessary or if we are guaranteed to get our own LoggingEvent
                // independent of any other appender.  It seems odd that appenders could break each other this way.

                FixFlags neededFlags = ourFixFlags & ~fixedFlags; // What we need which isn't already fixed
                if (neededFlags != FixFlags.None)
                {
                    // Uh-oh, there's something we do need to have "fixed"
                    loggingEvent.Fix = ourFixFlags; // should we set both (ourFixFlags | FixFlags.Partial) ?
                    // Flags that were already set are ignored in the new value, they can't change back to 0.
                }
            }

            LocationInfo locationInfo = loggingEvent.LocationInformation;
            IMessageSourceProvider messageSource = new LogMessageSource(locationInfo); // Wrap it with our converter
            Exception exception = loggingEvent.ExceptionObject;
            string loggerName = loggingEvent.LoggerName;
            string message = Layout != null ? RenderLoggingEvent(loggingEvent) : loggingEvent.RenderedMessage;
            string userName = loggingEvent.Identity;
            if (string.IsNullOrEmpty(userName))
            {
                // No thread-specific impersonating user, we'll just get the invariant process username later;
                userName = null;
            }

            // This is a slight hack.  We get our repository from the first LoggingEvent we see after a config update
            // and use it to update our thresholds configuration.  We are assuming each instance of an appender is
            // associated with only one repository (but might in general be attached to multiple loggers).
            // Notice that GibraltarMessageSeverity() will get the lock for access to our threshold config member variables.
            LogMessageSeverity severity = GibraltarMessageSeverity(loggingEvent.Level, loggingEvent.Repository);

            if (severity != LogMessageSeverity.None) // Filter out severities less than configured Verbose threshold
            {
                // We have a real event message.  Log it to the central Gibraltar queue, tagged as from Log4Net
                Log.Write(severity, "Log4Net", messageSource, userName, exception, LogWriteMode.Queued, null, loggerName, null, message); 
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Called to indicate that this object is no longer needed and can release resources it holds.
        /// </summary>
        /// <remarks>Provides deterministic release of resources.</remarks>
        public void Dispose()
        {
            // Call the underlying implementation
            Dispose(true);

            // SuppressFinalize because there won't be anything left to finalize
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs the actual releasing of managed and unmanaged resources.
        /// Most usage should instead call Dispose(), which will call Dispose(true) for you
        /// and will suppress redundant finalization.
        /// </summary>
        /// <param name="releaseManaged">Indicates whether to release managed resources.
        /// This should only be called with true, except from the finalizer which should call Dispose(false).</param>
        protected virtual void Dispose(bool releaseManaged)
        {
            if (!_disposed)
            {
                if (releaseManaged)
                {
                    // Free managed resources here (normal Dispose() stuff, which should itself call Dispose(true))
                    // Other objects may be referenced in this case

                    Close();
                }
                // Free native resources here (alloc's, etc)
                // May be called from within the finalizer, so don't reference other objects here

                _disposed = true;
            }
        }

        #endregion

        /// <summary>
        /// Converts a Log4Net event Level to the corresponding Gibraltar LogMessageSeverity enum.
        /// </summary>
        /// <param name="level">The Log4Net event Level.</param>
        /// <param name="repository">The repository from which this appender receives LoggingEvent entries.</param>
        /// <returns>The corresponding LogMessageSeverity value.</returns>
        /// <remarks>Because this method accesses our severity threshold instance variables,
        /// it does so within our internal lock (m_ConfigLock).</remarks>
        private LogMessageSeverity GibraltarMessageSeverity(Level level, ILoggerRepository repository)
        {
            int value = level.Value;
            LogMessageSeverity severity;

            lock (_configLock) // Lock ourselves for thread-safe access to our threshold member variables!
            {
                // We need to map log4net event Level into our LogMessageSeverity enum.
                // First make sure our thresholds are properly configured and consistent.
                LevelMap levelMap = repository.LevelMap; // Get the map of levels defined for our repository
                ProcessSeverityThresholdSettings(levelMap);

                // By definition, higher severity takes priority; the severity is the highest one whose threshold is met.
                // But we're assuming that lower severity are more common, so to improve performance in the more common
                // low-severity cases, we test for those first.
                // The configuration *must* ensure that the thresholds do not have inverted order (but equality is allowed).
                if (value < _severityVerboseThreshold)
                {
                    // Filter out events that don't meet our Verbose threshold, leave as Unknown
                    severity = LogMessageSeverity.None; // Illegal value will cause event to be ignored by Append()
                }
                else if (value < _severityInfoThreshold)
                {
                    // Met the minimum threshold for Verbose (all normally do), but not for Info, so it's a Verbose event.
                    severity = LogMessageSeverity.Verbose;
                }
                else if (value < _severityWarnThreshold)
                {
                    // Met at least the minimum threshold for Info, but not for Warn, so it's an Information event.
                    severity = LogMessageSeverity.Information;
                }
                else if (value < _severityErrorThreshold)
                {
                    // Met at least the minimum threshold for Warn, but not for Error, so it's a Warning event.
                    severity = LogMessageSeverity.Warning;
                }
                else if (value < _severityCriticalThreshold)
                {
                    // Met at least the minimum threshold for Error, but not for Critical, so it's an Error event.
                    severity = LogMessageSeverity.Error;
                }
                else
                {
                    // Met the minimum threshold for Critical, so it's a Critical event.
                    severity = LogMessageSeverity.Critical;
                }
            }

            return severity;  // returning Unknown severity should cause this event to be ignored.
        }

        #region Private helper subclass

        /// <summary>
        /// A private helper class for GibraltarAppender.
        /// </summary>
        /// <remarks>This is a helper class to convert from Log4Net's LocationInfo through Gibraltar's
        /// IMessageSourceProvider interface to supply the information to the Gibraltar log.</remarks>
        private class LogMessageSource : IMessageSourceProvider
        {
            private readonly LocationInfo m_LocationInfo;

            /// <summary>
            /// Creates a LogMessageSource (an IMessageSourceProvider for Gibraltar) from a LocationInfo object.
            /// </summary>
            /// <remarks>This wrapper must be created while still in the same context as the LocationInfo object
            /// in order to get the correct ThreadId.  It will then persist the information as long as the
            /// underlying LocationInfo is not altered.</remarks>
            /// <param name="locationInfo">The LocationInfo object provided by Log4Net with a LoggingEvent message.</param>
            public LogMessageSource(LocationInfo locationInfo)
            {
                m_LocationInfo = locationInfo;
            }

            /// <summary>
            /// Returns the integer ID for the managed thread which originated this object.
            /// </summary>
            public string MethodName { get { return m_LocationInfo.MethodName; } }

            public string ClassName { get { return m_LocationInfo.ClassName; } }

            public string FileName { get { return m_LocationInfo.FileName; } }

            public int LineNumber
            {
                get
                {
                    int lineNumber;
                    if (! int.TryParse(m_LocationInfo.LineNumber, out lineNumber))
                    {
                        lineNumber = 0; // Failed to parse, just return 0 (illegal line number indicating unknown)
                    }
                    return lineNumber;
                }
            }
        }

        #endregion

    }
}