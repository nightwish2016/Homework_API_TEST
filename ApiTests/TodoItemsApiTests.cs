using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using FluentAssertions;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace ApiTests
{
    // Base class for common entities and utility methods
    public class TodoItemsApiTestBase
    {
        protected const string BaseUrl = "http://127.0.0.1:5089";

        public class TodoItem
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool isComplete { get; set; }
        }

        public class ErrorResponse
        {
            public string type { get; set; }
            public string title { get; set; }
            public int status { get; set; }
            public string traceId { get; set; }
        }

        protected static readonly List<TodoItem> CompleteTestData = new List<TodoItem>
        {
            new TodoItem { id = 1, name = "learning english", isComplete = true },
            new TodoItem { id = 2, name = "AI website creation", isComplete = true },
            new TodoItem { id = 3, name = "Swimming", isComplete = false },
            new TodoItem { id = 4, name = "Reading", isComplete = true }
        };

        [TestInitialize]
        public async Task TestInitialize()
        {
            await CleanupAllTodoItems();
        }

        protected async Task CleanupAllTodoItems()
        {
            var client = new RestClient(BaseUrl);
            var getRequest = new RestRequest("/api/TodoItems", Method.Get);
            getRequest.AddHeader("accept", "text/plain");
            var getResponse = await client.ExecuteAsync(getRequest);
            
            if (getResponse.IsSuccessful && !string.IsNullOrEmpty(getResponse.Content))
            {
                var existingItems = JsonSerializer.Deserialize<List<TodoItem>>(getResponse.Content);
                if (existingItems != null && existingItems.Any())
                {
                    foreach (var item in existingItems)
                    {
                        var deleteRequest = new RestRequest($"/api/TodoItems/{item.id}", Method.Delete);
                        deleteRequest.AddHeader("accept", "*/*");
                        await client.ExecuteAsync(deleteRequest);
                    }
                }
            }
        }
    }

    [TestClass]
    public class TodoItemsGetTests : TodoItemsApiTestBase
    {
        [TestMethod]
        [Description("Verify that when getting all TodoItems, the returned list contains the expected items.")]
        [DataRow(1, "learning english", true)]
        [DataRow(2, "AI website creation", true)]
        public async Task GetAllTodoItems_ShouldReturnExpectedList(int expectedId, string expectedName, bool expectedIsComplete)
        {
            var client = new RestClient(BaseUrl);
            
            var testData = CompleteTestData;
            
            foreach (var item in testData)
            {
                var createRequest = new RestRequest("/api/TodoItems", Method.Post);
                createRequest.AddHeader("accept", "text/plain");
                createRequest.AddHeader("Content-Type", "application/json");
                createRequest.AddJsonBody(item);
                var createResponse = await client.ExecuteAsync(createRequest);
                createResponse.IsSuccessful.Should().BeTrue();
            }
            
            var request = new RestRequest("/api/TodoItems", Method.Get);
            request.AddHeader("accept", "text/plain");
            var response = await client.ExecuteAsync(request);

            response.IsSuccessful.Should().BeTrue();
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            response.Content.Should().NotBeNullOrEmpty();

            var items = JsonSerializer.Deserialize<List<TodoItem>>(response.Content);
            items.Should().NotBeNull();
            items.Count.Should().BeGreaterThanOrEqualTo(testData.Count);

            var targetItem = items.FirstOrDefault(item => item.id == expectedId);
            targetItem.Should().NotBeNull();
            targetItem.id.Should().Be(expectedId);
            targetItem.name.Should().Be(expectedName);
            targetItem.isComplete.Should().Be(expectedIsComplete);
        }

        [TestMethod]
        [Description("Verify that when getting TodoItem by ID, the returned content matches the expected values.")]
        [DataRow(1, "learning english", true)]
        [DataRow(2, "AI website creation", true)]
        public async Task GetTodoItemById_ShouldReturnExpectedItem(int itemId, string expectedName, bool expectedIsComplete)
        {
            var client = new RestClient(BaseUrl);

            var getRequest = new RestRequest($"/api/TodoItems/{itemId}", Method.Get);
            getRequest.AddHeader("accept", "text/plain");
            var getResponse = await client.ExecuteAsync(getRequest);

            if (getResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var createRequest = new RestRequest("/api/TodoItems", Method.Post);
                createRequest.AddHeader("accept", "text/plain");
                createRequest.AddHeader("Content-Type", "application/json");
                var newTodoItem = new TodoItem
                {
                    id = itemId,
                    name = expectedName,
                    isComplete = expectedIsComplete
                };
                createRequest.AddJsonBody(newTodoItem);
                var createResponse = await client.ExecuteAsync(createRequest);
                createResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            }

            var finalGetRequest = new RestRequest($"/api/TodoItems/{itemId}", Method.Get);
            finalGetRequest.AddHeader("accept", "text/plain");
            var finalGetResponse = await client.ExecuteAsync(finalGetRequest);

            finalGetResponse.IsSuccessful.Should().BeTrue();
            finalGetResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            finalGetResponse.Content.Should().NotBeNullOrEmpty();

            var item = JsonSerializer.Deserialize<TodoItem>(finalGetResponse.Content);
            item.Should().NotBeNull();
            item.id.Should().Be(itemId);
            item.name.Should().Be(expectedName);
            item.isComplete.Should().Be(expectedIsComplete);
        }

        [TestMethod]
        [Description("Verify that when getting TodoItem by non-existent ID, the API returns 404 Not Found.")]
        [DataRow(999)]
        public async Task GetTodoItemByNonExistentId_ShouldReturnNotFound(int nonExistentId)
        {
            var client = new RestClient(BaseUrl);
            var request = new RestRequest($"/api/TodoItems/{nonExistentId}", Method.Get);
            request.AddHeader("accept", "text/plain");
            var response = await client.ExecuteAsync(request);

            response.IsSuccessful.Should().BeFalse();
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [TestMethod]
        [Description("Verify that when there are no TodoItems, getting all TodoItems returns an empty list.")]
        public async Task GetAllTodoItemsWhenEmpty_ShouldReturnEmptyList()
        {
            var client = new RestClient(BaseUrl);
            var request = new RestRequest("/api/TodoItems", Method.Get);
            request.AddHeader("accept", "text/plain");
            var response = await client.ExecuteAsync(request);

            response.IsSuccessful.Should().BeTrue();
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            response.Content.Should().NotBeNullOrEmpty();

            var items = JsonSerializer.Deserialize<List<TodoItem>>(response.Content);
            items.Should().NotBeNull();
            items.Count.Should().Be(0);
        }
    }

    [TestClass]
    public class TodoItemsPostTests : TodoItemsApiTestBase
    {
        [TestMethod]
        [Description("Verify that when creating a new TodoItem, the API returns created status and correct content.")]
        [DataRow(4, "Swimming", false)]
        [DataRow(5, "Reading", true)]
        public async Task CreateTodoItem_ShouldReturnCreated(int newId, string newName, bool newIsComplete)
        {
            var client = new RestClient(BaseUrl);
            
            var getRequest = new RestRequest("/api/TodoItems", Method.Get);
            getRequest.AddHeader("accept", "text/plain");
            var getResponse = await client.ExecuteAsync(getRequest);
            
            getResponse.IsSuccessful.Should().BeTrue();
            var existingItems = JsonSerializer.Deserialize<List<TodoItem>>(getResponse.Content);
            
            var existingItem = existingItems.FirstOrDefault(item => item.id == newId);
            existingItem.Should().BeNull($"ID {newId} already exists, cannot create duplicate TodoItem");
            
            var request = new RestRequest("/api/TodoItems", Method.Post);
            request.AddHeader("accept", "text/plain");
            request.AddHeader("Content-Type", "application/json");
            
            var newTodoItem = new TodoItem
            {
                id = newId,
                name = newName,
                isComplete = newIsComplete
            };
            
            request.AddJsonBody(newTodoItem);
            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Content.Should().NotBeNullOrEmpty();

            var createdItem = JsonSerializer.Deserialize<TodoItem>(response.Content);
            createdItem.Should().NotBeNull();
            createdItem.id.Should().Be(newId);
            createdItem.name.Should().Be(newName);
            createdItem.isComplete.Should().Be(newIsComplete);
        }

        [TestMethod]
        [Description("Verify that when creating TodoItem without ID field, the server should auto-increment ID and return it.")]
        public async Task CreateTodoItem_WithoutId_ShouldAutoIncrementId()
        {
            var client = new RestClient(BaseUrl);

            // First get all current TodoItems to find the maximum existing ID
            var getRequest = new RestRequest("/api/TodoItems", Method.Get);
            getRequest.AddHeader("accept", "text/plain");
            var getResponse = await client.ExecuteAsync(getRequest);
            getResponse.IsSuccessful.Should().BeTrue();
            var existingItems = JsonSerializer.Deserialize<List<TodoItem>>(getResponse.Content);
            int maxId = existingItems.Any() ? existingItems.Max(x => x.id) : 0;

            // Construct request body without ID field
            var request = new RestRequest("/api/TodoItems", Method.Post);
            request.AddHeader("accept", "text/plain");
            request.AddHeader("Content-Type", "application/json");
            var newTodo = new { name = "aaa", isComplete = true };
            request.AddJsonBody(newTodo);
            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Content.Should().NotBeNullOrEmpty();

            var createdItem = JsonSerializer.Deserialize<TodoItem>(response.Content);
            createdItem.Should().NotBeNull();
            createdItem.name.Should().Be("aaa");
            createdItem.isComplete.Should().BeTrue();
            createdItem.id.Should().BeGreaterThan(maxId);
        }

/*
        [TestMethod]
        [Description("Verify that when creating TodoItem with invalid data (e.g., missing fields), the API returns 400 Bad Request.")]
        [DataRow(200, "Missing name field")]
        public async Task CreateTodoItemWithInvalidData_ShouldReturnBadRequest(int testId, string testDescription)
        {
            var client = new RestClient(BaseUrl);
            var request = new RestRequest("/api/TodoItems", Method.Post);
            request.AddHeader("accept", "text/plain");
            request.AddHeader("Content-Type", "application/json");
            
            var invalidData = new { id = testId }; // Missing name field
            request.AddJsonBody(invalidData);
            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [TestMethod]
        [Description("Verify that when creating TodoItem with invalid ID, the API returns 400 Bad Request.")]
        [DataRow(-1, "Invalid Item")]
        [DataRow(-100, "Negative Item")]
        [DataRow(0, "Zero Item")]
        public async Task CreateTodoItemWithInvalidId_ShouldReturnBadRequest(int invalidId, string itemName)
        {
            var client = new RestClient(BaseUrl);
            var request = new RestRequest("/api/TodoItems", Method.Post);
            request.AddHeader("accept", "text/plain");
            request.AddHeader("Content-Type", "application/json");
            
            var invalidTodoItem = new TodoItem
            {
                id = invalidId,
                name = itemName,
                isComplete = false
            };
            
            request.AddJsonBody(invalidTodoItem);
            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [TestMethod]
        [Description("Verify that when creating TodoItem with invalid name (e.g., empty, null, too long), the API returns 400 Bad Request.")]
        [DataRow(201, "", "Empty name")]
        public async Task CreateTodoItemWithInvalidName_ShouldReturnBadRequest(int testId, string invalidName, string testDescription)
        {
            var client = new RestClient(BaseUrl);
            var request = new RestRequest("/api/TodoItems", Method.Post);
            request.AddHeader("accept", "text/plain");
            request.AddHeader("Content-Type", "application/json");

            if (invalidName == "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA...")
                invalidName = new string('A', 1000);

            var invalidTodoItem = new TodoItem
            {
                id = testId,
                name = invalidName,
                isComplete = false
            };

            request.AddJsonBody(invalidTodoItem);
            var response = await client.ExecuteAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }*/
    }

    [TestClass]
    public class TodoItemsPutTests : TodoItemsApiTestBase
    {
        [TestMethod]
        [Description("Verify that when updating TodoItem, the API returns success status.")]
        [DataRow(2, "learning english2", true)]
        public async Task UpdateTodoItem_ShouldReturnSuccess(int itemId, string newName, bool newIsComplete)
        {
            var client = new RestClient(BaseUrl);

            var getRequest = new RestRequest($"/api/TodoItems/{itemId}", Method.Get);
            getRequest.AddHeader("accept", "text/plain");
            var getResponse = await client.ExecuteAsync(getRequest);

            if (getResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var createRequest = new RestRequest("/api/TodoItems", Method.Post);
                createRequest.AddHeader("accept", "text/plain");
                createRequest.AddHeader("Content-Type", "application/json");
                var newTodoItem = new TodoItem
                {
                    id = itemId,
                    name = "Original Name",
                    isComplete = false
                };
                createRequest.AddJsonBody(newTodoItem);
                var createResponse = await client.ExecuteAsync(createRequest);
                createResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            }

            var updateRequest = new RestRequest($"/api/TodoItems/{itemId}", Method.Put);
            updateRequest.AddHeader("accept", "*/*");
            updateRequest.AddHeader("Content-Type", "application/json");

            var updatedTodoItem = new TodoItem
            {
                id = itemId,
                name = newName,
                isComplete = newIsComplete
            };

            updateRequest.AddJsonBody(updatedTodoItem);
            var updateResponse = await client.ExecuteAsync(updateRequest);

            updateResponse.IsSuccessful.Should().BeTrue();
            updateResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        }

        [TestMethod]
        [Description("Verify that when updating non-existent TodoItem, the API returns 404 Not Found.")]
        [DataRow(999, "Non-existent Item")]
        [DataRow(888, "Missing Item")]
        [DataRow(777, "Not Found Item")]
        public async Task UpdateNonExistentTodoItem_ShouldReturnNotFound(int nonExistentId, string itemName)
        {
            var client = new RestClient(BaseUrl);
            var request = new RestRequest($"/api/TodoItems/{nonExistentId}", Method.Put);
            request.AddHeader("accept", "*/*");
            request.AddHeader("Content-Type", "application/json");

            var updatedTodoItem = new TodoItem
            {
                id = nonExistentId,
                name = itemName,
                isComplete = true
            };

            request.AddJsonBody(updatedTodoItem);
            var response = await client.ExecuteAsync(request);

            response.IsSuccessful.Should().BeFalse();
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }


        // [TestMethod]       
        // [Description("Verify that when updating TodoItem with invalid data (e.g., null name), the API returns 400 Bad Request.")]
        // [DataRow(301, "Base Item", null, "Null name update")]
        // public async Task UpdateTodoItemWithInvalidData_ShouldReturnBadRequest(int itemId, string originalName, string invalidName, string testDescription)
        // {
        //     var client = new RestClient(BaseUrl);
            
        //     var createRequest = new RestRequest("/api/TodoItems", Method.Post);
        //     createRequest.AddHeader("accept", "text/plain");
        //     createRequest.AddHeader("Content-Type", "application/json");
        //     createRequest.AddJsonBody(new TodoItem { id = itemId, name = originalName, isComplete = false });
        //     var createResponse = await client.ExecuteAsync(createRequest);
        //     createResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        //     var updateRequest = new RestRequest($"/api/TodoItems/{itemId}", Method.Put);
        //     updateRequest.AddHeader("accept", "*/*");
        //     updateRequest.AddHeader("Content-Type", "application/json");
            
        //     var invalidData = new { id = itemId, name = invalidName };
        //     updateRequest.AddJsonBody(invalidData);
        //     var updateResponse = await client.ExecuteAsync(updateRequest);

        //     updateResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        // }
    }

    [TestClass]
    public class TodoItemsDeleteTests : TodoItemsApiTestBase
    {
        [TestMethod]
        [Description("Verify that when deleting existing TodoItem, the API returns success.")]
        [DataRow(1, true)]
        public async Task DeleteTodoItem_ShouldReturnSuccess(int itemId, bool shouldSucceed)
        {
            var client = new RestClient(BaseUrl);

            var getRequest = new RestRequest($"/api/TodoItems/{itemId}", Method.Get);
            getRequest.AddHeader("accept", "text/plain");
            var getResponse = await client.ExecuteAsync(getRequest);

            if (getResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var createRequest = new RestRequest("/api/TodoItems", Method.Post);
                createRequest.AddHeader("accept", "text/plain");
                createRequest.AddHeader("Content-Type", "application/json");
                var newTodoItem = new TodoItem
                {
                    id = itemId,
                    name = $"Test Item {itemId}",
                    isComplete = false
                };
                createRequest.AddJsonBody(newTodoItem);
                var createResponse = await client.ExecuteAsync(createRequest);
                createResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            }

            var deleteRequest = new RestRequest($"/api/TodoItems/{itemId}", Method.Delete);
            deleteRequest.AddHeader("accept", "*/*");
            var deleteResponse = await client.ExecuteAsync(deleteRequest);

            deleteResponse.IsSuccessful.Should().BeTrue();
            deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        }

        [TestMethod]
        [Description("Verify that when deleting non-existent TodoItem, the API returns 404 Not Found.")]
        [DataRow(999, false)]
        public async Task DeleteNonExistentTodoItem_ShouldReturnNotFound(int itemId, bool shouldFail)
        {
            var client = new RestClient(BaseUrl);
            var request = new RestRequest($"/api/TodoItems/{itemId}", Method.Delete);
            request.AddHeader("accept", "*/*");
            var response = await client.ExecuteAsync(request);

            response.IsSuccessful.Should().Be(shouldFail);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            response.Content.Should().NotBeNullOrEmpty();

            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(response.Content);
            errorResponse.Should().NotBeNull();
            errorResponse.status.Should().Be(404);
            errorResponse.title.Should().Be("Not Found");
            errorResponse.type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
        }
    }
}