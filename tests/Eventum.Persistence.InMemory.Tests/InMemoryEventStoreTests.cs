namespace Eventum.Persistence.InMemory.Tests;

using System;
using Xunit;
using System.Threading.Tasks;
using Eventum.EventSourcing;
using System.Collections.Concurrent;
using Moq;
using Eventum.Telemetry.Abstractions;

public partial class InMemoryStoreTests
{
    private Mock<ITelemetryProvider> _mockTelemetryProvider;

    public InMemoryStoreTests()
    {
        _mockTelemetryProvider = new Mock<ITelemetryProvider>();
    }

    [Fact]
    public async Task WhenStreamIsSaved_Expect_EventVersion_Set()
    {
        // Arrange

        var events = new BlockingCollection<IEventStreamEvent>();
        var store = new InMemoryStore(_mockTelemetryProvider.Object);
        var stream = new LoadTestEventStream("test-stream", Guid.NewGuid().ToString());
        await store.SaveStreamAsync(stream, 0);

        // Act

        var loadedStream = await store.LoadStreamAsync<LoadTestEventStream>("test-stream");

        // Assert

        Assert.Equal("test-stream", loadedStream.StreamId);
        Assert.Equal(1, loadedStream.Version);
    }

    [Fact]
    public async Task WhenStreamDoesNotExist_Expect_StreamHasNoEvents_And_Version0()
    {
        // Arrange

        var store = new InMemoryStore(_mockTelemetryProvider.Object);

        // Act

        var stream = await store.LoadStreamAsync<LoadTestEventStream>("non-existent-stream");

        // Assert

        Assert.Equal(0, stream.Version);
    }

    [Fact]
    public async Task WhenStreamIsSavedSuccessfully_Expect_SaveStreamAsync_ReturnsTrue()
    {
        // Arrange

        var store = new InMemoryStore(_mockTelemetryProvider.Object);
        var stream = new LoadTestEventStream { StreamId = "test-stream", Version = 1 };

        // Act

        var result = await store.SaveStreamAsync(stream, 0);

        // Assert

        Assert.True(result);
    }

    [Fact]
    public async Task WhenVersionMismatch_Expect_SaveStreamAsync_ReturnsFalse()
    {
        // Arrange

        var store = new InMemoryStore(_mockTelemetryProvider.Object);
        var stream = new LoadTestEventStream("test-stream", Guid.NewGuid().ToString());

        // Act
        await store.SaveStreamAsync(stream, 1);
        var result = await store.SaveStreamAsync(stream, 1);

        // Assert

        Assert.False(result);
    }
}
