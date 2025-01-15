using EventSourcing.Projection;
using EventSourcing.Test.Data;
using Microsoft.Azure.Cosmos;
using Moq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace EventSourcing.Test.Projections
{
    public class MaterialisedViewProjectionEngineTests
    {
        [Fact]
        public async Task Expect_Projecting_Null_Event_Throws_ArgumentNullExeption()
        {
            // Arrange

            var mockMaterialisedViewRepository = new Mock<IMaterialisedViewRepository>();
            var sut = new MaterialisedViewProjectionEngine(Assembly.GetExecutingAssembly(), mockMaterialisedViewRepository.Object);

            // Act // Assert

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ProjectAsync(null));
        }

        [Fact]
        public async Task When_MaterialisedViewRepository_Error_Occurs_Expect_EventProjectionException_Is_Thrown()
        {
            // Arrange

            var @event = new UserRegisteredEvent(Guid.NewGuid().ToString(), "John Carmack", 50);
            var mockMaterialisedViewRepository = new Mock<IMaterialisedViewRepository>();
            var sut = new MaterialisedViewProjectionEngine(Assembly.GetExecutingAssembly(), null);

            // Act // Assert

            await Assert.ThrowsAsync<EventProjectionException>(() => sut.ProjectAsync(@event));
        }

        [Fact]
        public async Task When_MaterialisedViewRepository_SaveView_Fails_Expect_EventProjectionException_Is_Thrown()
        {
            // Arrange

            var @event = new UserRegisteredEvent(Guid.NewGuid().ToString(), "John Carmack", 50);
            var mockMaterialisedViewRepository = new Mock<IMaterialisedViewRepository>();
            mockMaterialisedViewRepository.Setup(r => r.LoadViewAsync(It.IsAny<string>(), It.IsAny<Type>())).ReturnsAsync(new TestView());
            mockMaterialisedViewRepository.Setup(r => r.SaveViewAsync(It.IsAny<string>(), It.IsAny<MaterialisedView>())).ReturnsAsync(false);

            var sut = new MaterialisedViewProjectionEngine(Assembly.GetExecutingAssembly(), mockMaterialisedViewRepository.Object);

            // Act // Assert

            await Assert.ThrowsAsync<EventProjectionException>(() => sut.ProjectAsync(@event));
        }
    }
}