﻿namespace Eventum.Persistence.InMemory.Tests;

using System;
using Xunit;
using System.Threading.Tasks;
using Eventum.EventSourcing;
using System.Collections.Concurrent;
using Moq;
using Eventum.Telemetry;
using System.Diagnostics;

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

    [Fact]
    public async Task Expect_LoadStreamAsync_TracksMetricWithNoException()
    {
        // Arrange

        var mockTelemetryProvider = new Mock<ITelemetryProvider>();
        var events = new BlockingCollection<IEventStreamEvent>
        {
            new LoadTestEvent("testStream", Guid.NewGuid().ToString(), 1),
            new LoadTestEvent("testStream", Guid.NewGuid().ToString(), 2)
        };
        
        var store = new InMemoryStore(events, mockTelemetryProvider.Object);

        // Act

        var stream = await store.LoadStreamAsync<LoadTestEventStream>("testStream");

        // Assert

        mockTelemetryProvider.Verify(tp => tp.TrackMetric("InMemoryStore.LoadStreamAsync.Time",
                                                          It.IsAny<double>(),
                                                          null,
                                                          TelemetryVerbosity.Info),
                                           Times.Once);
        mockTelemetryProvider.Verify(tp => tp.TrackException(It.IsAny<Exception>(),
                                                             It.IsAny<IDictionary<string, string>>(),
                                                             It.IsAny<TelemetryVerbosity>()),
                                           Times.Never);
    }

    [Fact(Skip = "Temporarily skipping this until setup can be fixed.")]
    public async Task WhenErrorOccurs_Expect_LoadStreamAsync_TracksException()
    {
        // Arrange

        var mockTelemetryProvider = new Mock<ITelemetryProvider>();
        var events = new BlockingCollection<IEventStreamEvent>
            {
                new LoadTestEvent("testStream", Guid.NewGuid().ToString(), 1),
            };

        // Simulate an error by throwing an exception
        _mockTelemetryProvider.Setup(t => t.TrackMetric("InMemoryStore.LoadStreamAsync.Time", It.IsAny<double>(), null, TelemetryVerbosity.Info))
                              .Throws(new EventStreamHandlerException(null));

        
        var store = new InMemoryStore(events, mockTelemetryProvider.Object);

        // Act

        await Assert.ThrowsAsync<EventStreamHandlerException>(() => store.LoadStreamAsync<LoadTestEventStream>("testStream"));

        // Assert

        mockTelemetryProvider.Verify(tp => tp.TrackException(It.IsAny<Exception>(),
                                                             It.Is<IDictionary<string, string>>(d => d["Operation"] == "LoadStreamAsync"),
                                                             TelemetryVerbosity.Error),
                                           Times.Once);
    }

    [Fact]
    public async Task Expect_SaveStreamAsync_TracksMetricAndNoException()
    {
        // Arrange

        var mockTelemetryProvider = new Mock<ITelemetryProvider>();
        var events = new BlockingCollection<IEventStreamEvent>();
        var store = new InMemoryStore(events, mockTelemetryProvider.Object);
        var stream = new LoadTestEventStream { StreamId = "testStream" };


        // Act

        var result = await store.SaveStreamAsync(stream, 0);

        // Assert

        Assert.True(result);
        mockTelemetryProvider.Verify(tp => tp.TrackMetric("InMemoryStore.SaveStreamAsync.Time", It.IsAny<double>(), null, TelemetryVerbosity.Info), Times.Once);
        mockTelemetryProvider.Verify(tp => tp.TrackException(It.IsAny<Exception>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<TelemetryVerbosity>()), Times.Never);
    }

    [Fact]
    public async Task WhenVersionMismatch_Expect_SaveStreamAsync_TrackEvents()
    {
        // Arrange

        var mockTelemetryProvider = new Mock<ITelemetryProvider>();
        var events = new BlockingCollection<IEventStreamEvent> {
            new LoadTestEvent("testStream", Guid.NewGuid().ToString(), 1),
        };

        var store = new InMemoryStore(events, mockTelemetryProvider.Object);
        var stream = new LoadTestEventStream { StreamId = "testStream" };

        // Act

        var result = await store.SaveStreamAsync(stream, 0);

        // Assert

        Assert.False(result);
        mockTelemetryProvider.Verify(tp => tp.TrackEvent("InMemoryStore.SaveStreamAsync.VersionMismatch",
                                                         It.IsAny<IDictionary<string, string>>(),
                                                         TelemetryVerbosity.Warning), Times.Once);
    }
}