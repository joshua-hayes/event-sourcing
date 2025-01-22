using Eventum.Telemetry;
using Eventum.Telemetry.OpenTelemetry;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Eventum.Tests
{
    public class OpenTelemetryTelemetryProviderTests
    {
        private readonly Mock<IMeterWrapper> _mockMeter;
        private readonly Mock<IActivitySourceWrapper> _mockActivitySource;
        private readonly Mock<ICounterWrapper> _mockCounter;
        private readonly OpenTelemetryTelemetryProvider _telemetryProvider;

        public OpenTelemetryTelemetryProviderTests()
        {
            _mockMeter = new Mock<IMeterWrapper>();
            _mockActivitySource = new Mock<IActivitySourceWrapper>();
            _mockCounter = new Mock<ICounterWrapper>();
            _telemetryProvider = new OpenTelemetryTelemetryProvider(TelemetryVerbosity.Info, _mockMeter.Object, _mockActivitySource.Object);
        }

        [Theory]
        [InlineData(TelemetryVerbosity.Debug)]
        [InlineData(TelemetryVerbosity.Info)]
        [InlineData(TelemetryVerbosity.Warning)]
        [InlineData(TelemetryVerbosity.Error)]
        public void Expect_TrackMetric_RespectsVerbosity(TelemetryVerbosity verbosity)
        {
            // Arrange
            _mockMeter.Setup(m => m.CreateCounter(It.IsAny<string>())).Returns(_mockCounter.Object);

            // Act
            _telemetryProvider.TrackMetric("TestMetric", 42, new Dictionary<string, string> { { "key", "value" } }, verbosity);

            // Assert
            if (verbosity >= TelemetryVerbosity.Info)
            {
                _mockCounter.Verify(c => c.Add(42, It.IsAny<KeyValuePair<string, object>[]>()), Times.Once);
            }
            else
            {
                _mockCounter.Verify(c => c.Add(It.IsAny<double>(), It.IsAny<KeyValuePair<string, object>[]>()), Times.Never);
            }
        }

        [Theory]
        [InlineData(TelemetryVerbosity.Debug)]
        [InlineData(TelemetryVerbosity.Info)]
        [InlineData(TelemetryVerbosity.Warning)]
        [InlineData(TelemetryVerbosity.Error)]
        public void Expect_TrackEvent_RespectsVerbosity(TelemetryVerbosity verbosity)
        {
            // Arrange
            var mockActivity = new Mock<Activity>("TestEvent");

            _mockActivitySource.Setup(a => a.StartActivity(It.IsAny<string>())).Returns(mockActivity.Object);

            // Act
            _telemetryProvider.TrackEvent("TestEvent", new Dictionary<string, string> { { "key", "value" } }, verbosity);

            // Assert
            if (verbosity >= TelemetryVerbosity.Info)
            {
                _mockActivitySource.Verify(a => a.StartActivity("TestEvent"), Times.Once);
            }
            else
            {
                _mockActivitySource.Verify(a => a.StartActivity("TestEvent"), Times.Never);
            }
        }

        [Theory]
        [InlineData(TelemetryVerbosity.Debug)]
        [InlineData(TelemetryVerbosity.Info)]
        [InlineData(TelemetryVerbosity.Warning)]
        [InlineData(TelemetryVerbosity.Error)]
        public void Expect_TrackException_RespectsVerbosity(TelemetryVerbosity verbosity)
        {
            // Arrange
            var exception = new Exception("TestException");
            var mockActivity = new Mock<Activity>("Exception");

            _mockActivitySource.Setup(a => a.StartActivity(It.IsAny<string>())).Returns(mockActivity.Object);

            // Act
            _telemetryProvider.TrackException(exception, new Dictionary<string, string> { { "key", "value" } }, verbosity);

            // Assert
            if (verbosity >= TelemetryVerbosity.Info)
            {
                _mockActivitySource.Verify(a => a.StartActivity("Exception"), Times.Once);
            }
            else
            {
                _mockActivitySource.Verify(a => a.StartActivity("Exception"), Times.Never);
            }
        }
    }
}
