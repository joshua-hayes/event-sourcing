using EventSourcing.Cosmos;
using EventSourcing.Projections;
using EventSourcing.Test.Data;
using Microsoft.Azure.Cosmos;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace EventSourcing.Infrastructure.Test
{
    public class CosmosMaterialisedViewRepositoryTests
    {
        private Mock<CosmosClient> _cosmosClient;
        private string _databaseId;
        private string _containerId;
        private string _viewName;
        private Type _type;

        public CosmosMaterialisedViewRepositoryTests()
        {
            _cosmosClient = new Mock<CosmosClient>();
            _databaseId = "databaseId";
            _containerId = "views";
            _viewName = "test_view";
            _type = typeof(TestView);
        }

        [Fact]
        public async Task When_Container_Throws_NotFound_Exception_Expect_LoadViewAsync_Returns_Empty_View()
        {
            // Arrange

            var mockContainer = new Mock<Container>();
            mockContainer.Setup(c => c.ReadItemAsync<JObject>(_viewName, It.IsAny<PartitionKey>(), null, default))
                         .Throws(new CosmosException(null, HttpStatusCode.NotFound, 0, "", 1));
            _cosmosClient.Setup(c => c.GetContainer(_databaseId, _containerId))
                        .Returns(mockContainer.Object);

            var sut = new CosmosMaterialisedViewRepository(_cosmosClient.Object, _databaseId, _containerId);

            // Act

            var view = await sut.LoadViewAsync(_viewName, _type);

            // Assert

            Assert.NotNull(view);
            Assert.IsType<TestView>(view);
            Assert.Equal(JsonDocument.Parse("{}").ToString(), view.View.ToString());
            Assert.Null(view.Etag);
        }

        [Fact]
        public async Task When_Container_ReadItemAsync_Returns_JObject_Expect_View_Property_Is_Set()
        {
            // Arrange

            var serialisedView = "{ 'name': 'Jack Dorsey'}";
            var mockItemResponse = new Mock<ItemResponse<JObject>>();
            mockItemResponse.Setup(r => r.Resource).Returns(JObject.Parse(serialisedView));

            var mockContainer = new Mock<Container>();
            _cosmosClient.Setup(c => c.GetContainer(_databaseId, _containerId))
                        .Returns(mockContainer.Object);
            mockContainer.Setup(c => c.ReadItemAsync<JObject>(_viewName, It.IsAny<PartitionKey>(), null, default))
                         .ReturnsAsync(mockItemResponse.Object);

            var sut = new CosmosMaterialisedViewRepository(_cosmosClient.Object, _databaseId, _containerId);

            // Act

            var view = await sut.LoadViewAsync(_viewName, _type) as TestView;

            // Assert

            Assert.NotNull(view);
            Assert.IsType<TestView>(view);
            Assert.Equal("Jack Dorsey", view.Name);
        }

        [Fact]
        public async Task When_ViewName_Is_Null_Expect_ArgumentNullException()
        {
            // Arrange

            string viewName = null;
            var sut = new CosmosMaterialisedViewRepository(_cosmosClient.Object, _databaseId, _containerId);

            // Act / Assert

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => sut.LoadViewAsync(viewName, _type));
            Assert.Equal("name", exception.ParamName);
        }

        [Fact]
        public async Task When_Type_Is_Null_Expect_ArgumentNullException()
        {
            // Arrange

            Type type = null;
            var sut = new CosmosMaterialisedViewRepository(_cosmosClient.Object, _databaseId, _containerId);

            // Act / Assert

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => sut.LoadViewAsync(_viewName, type));
            Assert.Equal("type", exception.ParamName);
        }

        [Fact]
        public async Task When_Container_Throws_NotFound_Exception_Expect_Generic_LoadViewAsync_Returns_Empty_View()
        {
            // Arrange

            var mockContainer = new Mock<Container>();
            mockContainer.Setup(c => c.ReadItemAsync<TestView>(_viewName, It.IsAny<PartitionKey>(), null, default))
                         .Throws(new CosmosException(null, HttpStatusCode.NotFound, 0, "", 1));
            _cosmosClient.Setup(c => c.GetContainer(_databaseId, _containerId))
                        .Returns(mockContainer.Object);

            var sut = new CosmosMaterialisedViewRepository(_cosmosClient.Object, _databaseId, _containerId);

            // Act

            var view = await sut.LoadViewAsync<TestView>(_viewName);

            // Assert

            Assert.NotNull(view);
            Assert.IsType<TestView>(view);
            Assert.Equal(JsonDocument.Parse("{}").ToString(), view.View.ToString());
            Assert.Null(view.Etag);
        }

        [Fact]
        public async Task When_Container_Generic_ReadItemAsync_Returns_View_Expect_View_Property_Is_Set()
        {
            // Arrange

            var serialisedView = "{ 'name': 'Jack Dorsey'}";
            var mockItemResponse = new Mock<ItemResponse<TestView>>();
            mockItemResponse.Setup(r => r.Resource).Returns(JsonConvert.DeserializeObject<TestView>(serialisedView));

            var mockContainer = new Mock<Container>();
            _cosmosClient.Setup(c => c.GetContainer(_databaseId, _containerId))
                        .Returns(mockContainer.Object);
            mockContainer.Setup(c => c.ReadItemAsync<TestView>(_viewName, It.IsAny<PartitionKey>(), null, default))
                         .ReturnsAsync(mockItemResponse.Object);

            var sut = new CosmosMaterialisedViewRepository(_cosmosClient.Object, _databaseId, _containerId);

            // Act

            var view = await sut.LoadViewAsync<TestView>(_viewName) as TestView;

            // Assert

            Assert.NotNull(view);
            Assert.IsType<TestView>(view);
            Assert.Equal("Jack Dorsey", view.Name);
        }

        [Fact]
        public async Task When_Generic_LoadViewAsync_Is_Called_With_No_ViewName_Expect_ArgumentNullException()
        {
            // Arrange

            string viewName = null;
            var sut = new CosmosMaterialisedViewRepository(_cosmosClient.Object, _databaseId, _containerId);

            // Act / Assert

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => sut.LoadViewAsync<TestView>(viewName));
            Assert.Equal("name", exception.ParamName);
        }

        [Fact]
        public async Task When_UpsertItemAsync_Fails_With_PreCondition_Failure_Expect_SaveViewAsync_Returns_False()
        {
            // Arrange

            var mockContainer = new Mock<Container>();
            mockContainer.Setup(c => c.UpsertItemAsync(It.IsAny<object>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), default))
                         .Throws(new CosmosException(null, HttpStatusCode.PreconditionFailed, 0, "", 1));

            _cosmosClient.Setup(c => c.GetContainer(_databaseId, _containerId))
                        .Returns(mockContainer.Object);

            // Arrange

            var view = new MaterialisedView();
            
            var sut = new CosmosMaterialisedViewRepository(_cosmosClient.Object, _databaseId, _containerId);

            // Act

            bool saved = await sut.SaveViewAsync(_viewName, view);

            // Assert

            Assert.False(saved);
            mockContainer.Verify(c => c.UpsertItemAsync(It.IsAny<object>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), default));
        }

        [Fact]
        public async Task Expect_ItemRequestOptions_Sets_Etag_Precondition_When_UpsertItemAsync()
        {
            // Arrange

            var mockContainer = new Mock<Container>();
            var etag = "etag";
            mockContainer.Setup(c => c.UpsertItemAsync(It.IsAny<object>(), It.IsAny<PartitionKey>(), It.Is<ItemRequestOptions>(o => o.IfMatchEtag == etag), default));

            _cosmosClient.Setup(c => c.GetContainer(_databaseId, _containerId))
                        .Returns(mockContainer.Object);

            // Arrange

            var view = new MaterialisedView
            {
                Etag = etag
            };

            var sut = new CosmosMaterialisedViewRepository(_cosmosClient.Object, _databaseId, _containerId);

            // Act

            _ = await sut.SaveViewAsync(_viewName, view);

            // Assert

            mockContainer.Verify(c => c.UpsertItemAsync(It.IsAny<object>(), It.IsAny<PartitionKey>(), It.Is<ItemRequestOptions>(o => o.IfMatchEtag == etag), default));
        }

        [Fact]
        public async Task When_View_Is_Saved_Expect_View_And_ETag_Properties_Are_Ommitted_From_Materialised_View_Data()
        {
            // Arrange

            var mockContainer = new Mock<Container>();
            mockContainer.Setup(c => c.UpsertItemAsync(It.IsAny<object>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), default));

            _cosmosClient.Setup(c => c.GetContainer(_databaseId, _containerId))
                        .Returns(mockContainer.Object);

            // Arrange

            var view = new TestView
            {
                Etag = "etag",
                Name = "Test"
            };

            var sut = new CosmosMaterialisedViewRepository(_cosmosClient.Object, _databaseId, _containerId);

            // Act

            _ = await sut.SaveViewAsync(_viewName, view);

            // Assert

            mockContainer.Verify(c => c.UpsertItemAsync(It.Is<MaterialisedViewData>(item => !item.View.ContainsKey("_etag")
                                                                                         && !item.View.ContainsKey("view")
                                                                                         && !item.View.ContainsKey("changeset")),
                                                        It.IsAny<PartitionKey>(),
                                                        It.IsAny<ItemRequestOptions>(),
                                                        default));
        }
    }
}