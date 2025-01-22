﻿using Eventum.EventSourcing;
using Eventum.EventSourcing.Test.Data;
using Eventum.Persistence;
using Eventum.Projection.Tests.Data;
using Eventum.Serialisation;
using Eventum.Telemetry;
using Moq;
using System.Collections.Concurrent;
using Xunit;

namespace Eventum.Projection.Tests
{
    public class EventProjectionTests
    {
        private Mock<IEventSerialiser> _mockSerialiser;
        private Mock<ITelemetryProvider> _mockTelemetryProvider;

        public EventProjectionTests()
        {
            _mockSerialiser = new Mock<IEventSerialiser>();
            _mockTelemetryProvider = new Mock<ITelemetryProvider>();
        }

        [Fact]
        public void Expect_New_Projection_Sets_View()
        {
            // Arrange

            var view = new TestView();

            // Act

            var projection = new TestProjection(view, _mockSerialiser.Object, _mockTelemetryProvider.Object);

            // Assert

            Assert.NotNull(projection.View);
            Assert.Equal(view, projection.View);
        }


        [Fact]
        public void When_ApplyChange_Cannot_Handle_Event_Expect_EventProjectionException_Is_Thrown()
        {
            // Arrange

            var @event = new UnhandledEvent();
            var projection = new TestProjection(new TestView(), _mockSerialiser.Object, _mockTelemetryProvider.Object);

            // Act / Assert

            Assert.Throws<EventProjectionException>(() => projection.ApplyChange(@event));
        }

        [Fact]
        public void When_Apply_Change_Expect_Change_Added_To_Changeset()
        {
            // Arrange

            var view = new TestView();
            var change1 = new TestEvent1("stream1", "f1", 1);
            _mockSerialiser.Setup(s => s.Serialise(view)).Returns("dummy-view");

            // Act

            var projection = new TestProjection(view, _mockSerialiser.Object, _mockTelemetryProvider.Object);
            projection.ApplyChange(@change1);

            // Assert

            Assert.NotNull(projection.View);
            Assert.NotEmpty(view.Changeset);
            _mockSerialiser.Verify();
        }

        [Fact]
        public void Expect_Applying_Events_Out_Of_Order_Produces_Different_Endstate_Hash()
        {
            // Arrange

            var change1 = new TestEvent1("stream1", "f1", 1)
            {
                Version = 1
            };
            var change2 = new TestEvent2("stream1", "f3", DateTime.MinValue)
            {
                Version = 2
            };

            var view = new TestView();
            var projection = new TestProjection(view, _mockSerialiser.Object, _mockTelemetryProvider.Object);
            _mockSerialiser.Setup(s => s.Serialise(view)).Returns("dummy-view");
            projection.ApplyChange(change1);
            _mockSerialiser.Setup(s => s.Serialise(view)).Returns("dummy-view2");
            projection.ApplyChange(change2);
            var inOrderHash = view.Changeset.Last();

            // Act

            var compareView = new TestView();
            var compareProjection = new TestProjection(compareView, _mockSerialiser.Object, _mockTelemetryProvider.Object);
            _mockSerialiser.Setup(s => s.Serialise(compareView)).Returns("dummy-view2");
            compareProjection.ApplyChange(change2);
            _mockSerialiser.Setup(s => s.Serialise(compareView)).Returns("dummy-view1");
            compareProjection.ApplyChange(change1);
            var outOfOrderHash = compareView.Changeset.Last();

            // Assert

            Assert.NotNull(projection.View);
            Assert.NotEmpty(view.Changeset);
            Assert.NotEqual(inOrderHash, outOfOrderHash);
            _mockSerialiser.Verify(s => s.Serialise(It.IsAny<IMaterialisedView>()), Times.Exactly(4));
        }

        [Fact]
        public void When_ChangeSet_Limit_Not_Exceeded_Expect_Applying_Multiple_Events_Applies_Multipe_Changes()
        {
            // Arrange

            var view = new TestView();
            var projection = new TestProjection(view, _mockSerialiser.Object, _mockTelemetryProvider.Object);
            var change1 = new TestEvent1("stream1", "f1", 1);
            var change2 = new TestEvent2("stream1", "f3", DateTime.MinValue);

            // Act

            _mockSerialiser.Setup(s => s.Serialise(view)).Returns("dummy-view");
            projection.ApplyChange(change1);

            _mockSerialiser.Setup(s => s.Serialise(view)).Returns("dummy-view2");
            projection.ApplyChange(change2);

            // Assert

            Assert.NotNull(projection.View);
            Assert.NotEmpty(view.Changeset);
            Assert.Equal(2, view.Changeset.Count);
            _mockSerialiser.Verify(s => s.Serialise(It.IsAny<IMaterialisedView>()), Times.Exactly(2));
        }

        [Theory]
        [InlineData(5, 10)]
        [InlineData(11, 10)]
        public void Expect_Changeset_Max_Limit_Is_Respected(int changes, int changesetLimit)
        {
            // Arrange

            var view = new TestView();
            var projection = new TestProjection(view, _mockSerialiser.Object, _mockTelemetryProvider.Object, changesetLimit);
            _mockSerialiser.Setup(s => s.Serialise(view)).Returns("dummy-view");

            // Act

            for (int i = 1; i <= changes; i++)
            {
                var @event = new TestEvent1(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 1)
                {
                    Version = i
                };
                projection.ApplyChange(@event);
            }

            // Assert

            _mockSerialiser.Verify(s => s.Serialise(It.IsAny<IMaterialisedView>()), Times.Exactly(changes));
            Assert.NotEmpty(view.Changeset);
            if (changes > changesetLimit)
                Assert.Equal(changesetLimit, view.Changeset.Count);
            else
                Assert.Equal(changes, view.Changeset.Count);
        }

        [Fact]
        public void Expect_ApplyChange_TracksMetricAndNoException()
        {
            // Arrange
            
            _mockSerialiser.Setup(s => s.Serialise(It.IsAny<MaterialisedView>())).Returns("SerializedView");

            var view = new TestView();
            var projection = new TestProjection(view, _mockSerialiser.Object, _mockTelemetryProvider.Object);
            var @event = new TestEvent2("testStream", "field3", DateTime.MaxValue);

            // Act

            projection.ApplyChange(@event);

            // Assert

            _mockTelemetryProvider.Verify(tp => tp.TrackMetric("EventProjection.ApplyChange.Time",
                                                               It.IsAny<double>(),
                                                               null,
                                                               TelemetryVerbosity.Info), Times.Once);
            _mockTelemetryProvider.Verify(tp => tp.TrackException(It.IsAny<Exception>(),
                                                                  It.IsAny<IDictionary<string, string>>(),
                                                                  It.IsAny<TelemetryVerbosity>()), Times.Never);
        }

        [Fact]
        public void WhenErrorOccurs_Expect_ApplyChange_TracksException()
        {
            // Arrange

            _mockSerialiser.Setup(s => s.Serialise(It.IsAny<MaterialisedView>())).Throws(new Exception("Serialisation Error"));

            var view = new TestView();
            var projection = new TestProjection(view, _mockSerialiser.Object, _mockTelemetryProvider.Object);
            var @event = new TestEvent2("testStream", "field3", DateTime.MaxValue);

            // Act & Assert

            Assert.Throws<EventProjectionException>(() => projection.ApplyChange(@event));

            _mockTelemetryProvider.Verify(tp => tp.TrackException(It.IsAny<EventProjectionException>(),
                                                                  It.Is<IDictionary<string, string>>(d =>
                                                                  d["Operation"] == "ApplyChange" &&
                                                                  d["StreamId"] == @event.StreamId &&
                                                                  d["EventId"] == @event.Id),
                                                                  TelemetryVerbosity.Error), Times.Once);
        }

    }
}
