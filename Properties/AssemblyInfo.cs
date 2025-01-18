using System.Reflection;
using Xunit;

[assembly: AssemblyTitle("Eventum")]
[assembly: AssemblyDescription("An event sourcing and materialised view projection library.")]

// Enable parallel test execution
[assembly: Xunit.CollectionBehavior(MaxParallelThreads = 4)]
