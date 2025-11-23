// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// FIXED MANUALLY:
// - CA1862 (68 occurrences): Replaced ToLower().Contains() with StringComparison.OrdinalIgnoreCase
// - CA1816 (3 occurrences): Added GC.SuppressFinalize() to Dispose methods

// SUPPRESSED WITH JUSTIFICATION:

// CA1303: Do not pass literals as localized parameters
// Justification: This is not a localized application - English only
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", 
    Justification = "Application is not localized")]

// CA1304, CA1305, CA1307, CA1310, CA1311: Culture-specific operations
// Justification: Culture-specific behavior not required for this application
[assembly: SuppressMessage("Globalization", "CA1304:Specify CultureInfo", 
    Justification = "Culture-specific behavior not required")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", 
    Justification = "Culture-specific behavior not required")]
[assembly: SuppressMessage("Globalization", "CA1307:Specify StringComparison for correctness", 
    Justification = "Ordinal comparison is implicit")]
[assembly: SuppressMessage("Globalization", "CA1310:Specify StringComparison for correctness", 
    Justification = "Ordinal comparison is implicit")]
[assembly: SuppressMessage("Globalization", "CA1311:Specify a culture or use an invariant version", 
    Justification = "Culture-specific behavior not required")]

// CA1848, CA1849: Use LoggerMessage delegates
// Justification: Performance optimization not needed - logging is not in hot path
[assembly: SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", 
    Justification = "Performance optimization not needed for these log statements")]
[assembly: SuppressMessage("Performance", "CA1849:Call async methods when in an async method", 
    Justification = "Synchronous logging is acceptable")]

// CA1002: Change List<T> to Collection<T>
// Justification: List<T> is appropriate for internal data structures and JSON serialization
[assembly: SuppressMessage("Design", "CA1002:Do not expose generic lists", 
    Justification = "List<T> is appropriate for data models and JSON serialization")]

// CA2227: Collection properties should be read only
// Justification: Mutable collections are required for JSON serialization/deserialization
[assembly: SuppressMessage("Usage", "CA2227:Collection properties should be read only", 
    Justification = "Mutable collections required for JSON serialization")]

// CA2007: Do not directly await a Task
// Justification: ConfigureAwait(false) not needed for application code (only library code)
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", 
    Justification = "ConfigureAwait not needed in application code")]

// CA5394: Do not use insecure randomness
// Justification: Random is used for game seeds and procedural generation, not security
[assembly: SuppressMessage("Security", "CA5394:Do not use insecure randomness", 
    Justification = "Random used for game generation, not security")]

// CA1031: Do not catch general exception types
// Justification: Top-level exception handling needs to catch all exceptions
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", 
    Justification = "Top-level exception handling for user-facing errors")]

// CA1062: Validate arguments of public methods
// Justification: Nullable reference types provide compile-time null safety
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", 
    Justification = "Nullable reference types enabled - null safety at compile time")]

// CA1707: Identifiers should not contain underscores
// Justification: Test methods use underscores for readability (Pattern: Method_Scenario_Expected)
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", 
    Justification = "Test method names use underscores for readability", 
    Scope = "namespaceanddescendants", Target = "~N:SoloAdventureSystem.Engine.Tests")]
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", 
    Justification = "Test method names use underscores for readability", 
    Scope = "namespaceanddescendants", Target = "~N:SoloAdventureSystem.CLI.Tests")]

// CA1711: Identifiers should not have incorrect suffix
// Justification: "EventHandler" suffix is appropriate for event handler delegates
[assembly: SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", 
    Justification = "EventHandler suffix is appropriate for delegates")]

// CA2201: Do not raise reserved exception types
// Justification: Generic exceptions appropriate for test mock implementations
[assembly: SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", 
    Justification = "Generic exceptions used in test mocks", 
    Scope = "namespaceanddescendants", Target = "~N:SoloAdventureSystem.Engine.Tests")]

// CA1852: Seal internal types
// Justification: Internal test types don't need to be sealed for inheritance prevention
[assembly: SuppressMessage("Performance", "CA1852:Seal internal types", 
    Justification = "Test types don't require sealing", 
    Scope = "namespaceanddescendants", Target = "~N:SoloAdventureSystem.Engine.Tests")]
[assembly: SuppressMessage("Performance", "CA1852:Seal internal types", 
    Justification = "Test types don't require sealing", 
    Scope = "namespaceanddescendants", Target = "~N:SoloAdventureSystem.CLI.Tests")]

// CA1822: Member does not access instance data and can be marked as static
// Justification: Test helper methods and validation tool methods are designed for clarity over performance
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", 
    Justification = "Test helper methods designed for clarity", 
    Scope = "namespaceanddescendants", Target = "~N:SoloAdventureSystem.Engine.Tests")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", 
    Justification = "Validation tool methods designed for clarity", 
    Scope = "type", Target = "~T:SoloAdventureSystem.ValidationTool.WorldQualityAnalyzer")]

// CA1845: Use span-based string operations
// Justification: Substring is used for display/logging where performance is not critical
[assembly: SuppressMessage("Performance", "CA1845:Use span-based 'string.Concat'", 
    Justification = "Substring used in display/logging code where performance is not critical")]

// CA1866, CA1847, CA1854: Use char overload, Dictionary TryGetValue
// Justification: Minor performance optimizations not critical for this application
[assembly: SuppressMessage("Performance", "CA1866:Use char overload", 
    Justification = "String overload is more readable")]
[assembly: SuppressMessage("Performance", "CA1847:Use char overload for IndexOf", 
    Justification = "String overload is more readable")]
[assembly: SuppressMessage("Performance", "CA1854:Prefer Dictionary TryGetValue", 
    Justification = "ContainsKey pattern is more readable")]

// CA1805: Do not initialize unnecessarily
// Justification: Explicit initialization improves code clarity
[assembly: SuppressMessage("Performance", "CA1805:Do not initialize unnecessarily", 
    Justification = "Explicit initialization for clarity")]

// CA2254: Template should be compile-time static
// Justification: Dynamic log messages are acceptable for this application
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", 
    Justification = "Dynamic log messages for flexibility")]
