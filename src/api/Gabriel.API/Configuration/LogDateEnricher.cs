using System.Globalization;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Gabriel.API.Configuration;

// Adds a "LogDate" property to every event, formatted MM-dd-yyyy, so the
// Serilog.Sinks.Map sink can route each event into a file named after that
// date. Serilog.Sinks.File only supports its built-in yyyyMMdd rolling-period
// format; routing through Map is how we get an arbitrary filename pattern.
public sealed class LogDateEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var date = logEvent.Timestamp.LocalDateTime.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("LogDate", date));
    }
}

public static class LogDateEnricherExtensions
{
    // Surface as Enrich.WithLogDate() so it can be referenced by name from
    // appsettings.Serilog.Enrich.
    public static LoggerConfiguration WithLogDate(this LoggerEnrichmentConfiguration enrich)
        => enrich.With<LogDateEnricher>();
}
