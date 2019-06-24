using System;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Slack.Core;
using Serilog.Sinks.Slack.Core.Extensions;

// ReSharper disable once IdentifierTypo
// ReSharper disable once CheckNamespace
namespace Serilog
{
    public static class LoggerConfigurationExtensions
    {
        /// <summary>
        /// Adds a sink that writes log events to a channel in Slack.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="channels">List of Slack channels.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="formatProvider">FormatProvider to apply in <see cref="LogEvent.RenderMessage(IFormatProvider)"/>. It overrides default behaviour.</param>
        /// <param name="username">Optional bot name</param>
        /// <param name="iconUrl">Optional URL to an image to use as the icon for this message.</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration VerboseSlack(
            this LoggerSinkConfiguration loggerConfiguration,
            SlackChannelCollection channels,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IFormatProvider formatProvider = null,
            string username = null,
            string iconUrl = null
        )
        {
            if (loggerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(loggerConfiguration));
            }

            if (channels == null)
            {
                throw new ArgumentNullException(nameof(channels));
            }

            if (channels.Count == 0)
            {
                throw new ArgumentException("Must have at least one Slack channel defined.");
            }

            return 
                loggerConfiguration
                    .Sink
                    (
                        new SlackSink
                        (
                            channels,
                            new SlackVerboseRenderer(formatProvider).Render,
                            formatProvider,
                            username,
                            iconUrl
                        ),
                        restrictedToMinimumLevel
                    );
        }

        /// <summary>
        /// Adds a sink that writes log events to a channel in Slack.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="webhookUri">WebHook Uri that allows Slack Incoming Webhooks (https://api.slack.com/incoming-webhooks).</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="formatProvider">FormatProvider to apply in <see cref="LogEvent.RenderMessage(IFormatProvider)"/>. It overrides default behaviour.</param>
        /// <param name="username">Optional bot name</param>
        /// <param name="iconUrl">Optional URL to an image to use as the icon for this message.</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration VerboseSlack
        (
            this LoggerSinkConfiguration loggerConfiguration,
            string webhookUri,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IFormatProvider formatProvider = null,
            string username = null,
            string iconUrl = null
        )
        {
            if (loggerConfiguration == null)
            {
                throw new ArgumentNullException("loggerConfiguration");
            }

            if (string.IsNullOrWhiteSpace(webhookUri))
            {
                throw new ArgumentNullException("webhookUri");
            }

            return 
                loggerConfiguration
                    .Sink
                    (
                        new SlackSink
                        (
                            webhookUri,
                            new SlackVerboseRenderer(formatProvider).Render, 
                            formatProvider,
                            username,
                            iconUrl
                        ),
                        restrictedToMinimumLevel
                    );
        }
    }
}