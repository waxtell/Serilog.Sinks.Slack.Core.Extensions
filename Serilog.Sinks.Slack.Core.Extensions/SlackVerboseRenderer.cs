using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Serilog.Events;

namespace Serilog.Sinks.Slack.Core.Extensions
{
    public class SlackVerboseRenderer
    {
        private readonly IFormatProvider _formatProvider;

        public SlackVerboseRenderer(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }

        private static IDictionary<string,string> Flatten(IReadOnlyDictionary<string, LogEventPropertyValue> properties, IFormatProvider formatProvider)
        {
            var attachmentFields = new List<(string section, IEnumerable<string> path, string value)>();

            void FlattenProperty(Stack<string> path, LogEventProperty property)
            {
                path.Push(property.Name);

                FlattenPropertyValue(path, property.Value);

                path.Pop();
            }

            void FlattenPropertyValue(Stack<string> path, LogEventPropertyValue propertyValue)
            {
                if (propertyValue is StructureValue sval)
                {
                    foreach (var p in sval.Properties)
                    {
                        FlattenProperty(path, p);
                    }

                    return;
                }

                using (var writer = new StringWriter())
                {
                    propertyValue.Render(writer,null, formatProvider);

                    AddField(path.Reverse().ToList(),writer.ToString());
                }
            }

            void AddField(IList<string> path, string value)
            {
                attachmentFields.Add((path.First(), path.Skip(1), value));
            }

            foreach (var property in properties)
            {
                FlattenPropertyValue(new Stack<string>(new[] { property.Key }), property.Value);
            }

            return 
                attachmentFields
                    .GroupBy(x => x.section)
                    .ToDictionary
                    (
                        item => item.Key, 
                        value => string
                            .Join
                            (
                                Environment.NewLine, 
                                value
                                    .Select
                                    (
                                        x => x.path.Any()
                                            ?$"{string.Join(".", x.path)}::{x.value}"
                                            :x.value
                                    )
                            )
                    );
        }

        // ReSharper disable once IdentifierTypo
        public string Render(LogEvent input, string username, string emoji)
        {
            object WrapExceptionInAttachment(Exception ex)
            {
                return new
                {
                    title = "Exception",
                    fallback = $"Exception: {ex.Message} {Environment.NewLine} {ex.StackTrace}",
                    color = GetAttachmentColor(LogEventLevel.Fatal),
                    fields = new[]
                    {
                        CreateAttachmentField("Message", ex.Message),
                        CreateAttachmentField("Type", $"`{ex.GetType().Name}`"),
                        CreateAttachmentField("Stack Trace", $"```{ex.StackTrace}```", false)
                    },
                    mrkdwn_in = new[] { "fields" }
                };
            }

            IEnumerable<dynamic> WrapInAttachment(LogEvent log)
            {
                var fields = new List<object>
                {
                    CreateAttachmentField("Level", log.Level.ToString()),
                    CreateAttachmentField("Timestamp", log.Timestamp.ToString())
                };
                fields
                    .AddRange
                    (
                        Flatten(input.Properties, _formatProvider)
                            .Select(x => CreateAttachmentField(x.Key,x.Value))
                    );

                var result = new List<dynamic>
                {
                    new
                    {
                        fallback = $"[{log.Level}]{log.RenderMessage()}",
                        color = GetAttachmentColor(log.Level),
                        fields
                    }
                };

                if (log.Exception != null)
                {
                    result.Add(WrapExceptionInAttachment(log.Exception));
                }

                return result;
            }

            string GetAttachmentColor(LogEventLevel level)
            {
                switch (level)
                {
                    case LogEventLevel.Information:
                        return "#5bc0de";
                    case LogEventLevel.Warning:
                        return "#f0ad4e";
                    case LogEventLevel.Error:
                    case LogEventLevel.Fatal:
                        return "#d9534f";
                    default:
                        return "#777";
                }
            }

            object CreateAttachmentField(string title, string value, bool @short = true)
            {
                return new { title, value, @short };
            }

            dynamic body = new ExpandoObject();
            body.text = input.RenderMessage();

            if (!string.IsNullOrEmpty(username))
            {
                body.username = username;
            }

            if (!string.IsNullOrEmpty(emoji))
            {
                body.icon_emoji = emoji;
            }

            body.attachments = WrapInAttachment(input).ToArray();

            return 
                Newtonsoft
                    .Json
                    .JsonConvert
                    .SerializeObject(body);
        }
    }
}