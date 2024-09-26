using RestSharp;
using RestSharp.Authenticators;
using StorySpoil.Models;
using System.Net;
using System.Text.Json;

namespace StorySpoil
{
    public class StorySpoil
    {
        private RestClient client;        
        private static string storyId;

        [OneTimeSetUp]
        public void Setup()
        {
            string accessToken = GetAccessToken("sanya", "123456");

            var restOptions = new RestClientOptions("https://d5wfqm7y6yb3q.cloudfront.net")
            {
                Authenticator = new JwtAuthenticator(accessToken),
            };

            this.client = new RestClient(restOptions);
        }

        private string GetAccessToken(string username, string password)
        {
            var authClient = new RestClient("https://d5wfqm7y6yb3q.cloudfront.net");

            var authRequest = new RestRequest("/api/User/Authentication", Method.Post);
            authRequest.AddJsonBody(new AuthenticationRequest
            {
                UserName = username,
                Password = password
            });

            var response = authClient.Execute(authRequest);

            if (response.IsSuccessStatusCode)
            {
                var content = JsonSerializer.Deserialize<AuthenticationResponse>(response.Content);
                var accessToken = content.AccessToken;
                return accessToken;
            }
            else
            {
                throw new InvalidOperationException("Authentication failed!");
            }
        }

        [Order(1)]
        [Test]
        public void CreateStorySoiler_WithRequiredFields_ShouldSucceed()
        {
            // Arrange
            var newStory = new StoryDTO()
            {
                Title = "New Story for fun",
                Description = "Test Description",
                Url = "",
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newStory);

            // Act
            var response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            storyId = responseData.StoryId;

            Assert.That(responseData.Message, Is.EqualTo("Successfully created!"));
        }

        [Order(2)]
        [Test]
        public void EditStorySpoiler_WithNewTitle_ShouldSucceed()
        {
            // Arrange
            var editedStory = new StoryDTO()
            {
                Title = "Edited Title",
                Description = "Test description with edits",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{storyId}", Method.Put);
            request.AddJsonBody(editedStory);

            // Act
            var response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(content.Message, Is.EqualTo("Successfully edited"));
        }

        [Order(3)]
        [Test]
        public void DeleteStory_WithCorrectId_ShouldBeSuccesful()
        {
            // Arrange
            var request = new RestRequest($"/api/Story/Delete/{storyId}", Method.Delete);

            // Act
            var response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(content.Message, Is.EqualTo("Deleted successfully!"));
        }

        [Order(4)]
        [Test]
        public void CreateNewStory_WithIncorrectData_ShouldFail()
        {
            // Arrange
            var request = new RestRequest("/api/Story/Create", Method.Post);

            request.AddJsonBody(new { });

            // Act
            var response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(5)]
        [Test]
        public void EditStory_WithNonExisitingId_ShouldFail()
        {
            // Arrange
            var newStory = new StoryDTO()
            {
                Title = "New Title",
                Description = "Test description",
            };

            var request = new RestRequest($"/api/Story/Edit/XXXXXXXXXXX", Method.Put);
            request.AddJsonBody(newStory);

            // Act
            var response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var content = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(content.Message, Is.EqualTo("No spoilers..."));
        }

        [Order(6)]
        [Test]
        public void DeleteStory_WithNonExistingId_ShouldFail()
        {
            // Arrange
            var request = new RestRequest("/api/Story/Delete/XASDAXAS", Method.Delete);

            // Act
            var response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var content = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(content.Message, Is.EqualTo("Unable to delete this story spoiler!"));
        }



    }
}