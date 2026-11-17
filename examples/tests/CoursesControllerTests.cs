// Purpose: Integration tests verifying CoursesController endpoints, HTTP status codes, and CRUD lifecycle.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PRPA.Portfolio.Examples.Models;
using Xunit;

namespace PRPA.Portfolio.Examples.Tests;

/// <summary>
/// Integration test suite exercising course endpoints against the test server.
/// Implements <see cref="IAsyncLifetime"/> for automated post-test entity cleanup.
/// </summary>
public class CoursesControllerTests : TestBase, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly HttpClient _authClient;
    private readonly List<string> _cleanupEndpoints = new();

    public CoursesControllerTests()
    {
        _client = CreateClient();
        _authClient = CreateAuthenticatedClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// Executes after every test to delete any created test courses, preventing database pollution.
    /// </summary>
    public new async Task DisposeAsync()
    {
        foreach (string endpoint in _cleanupEndpoints)
        {
            await _authClient.DeleteAsync(endpoint);
        }
    }

    [Fact]
    public async Task GetAllCourses_ReturnsOkWithList()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/courses");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<Course>? courses = await response.Content.ReadFromJsonAsync<List<Course>>();
        Assert.NotNull(courses);
        Assert.NotEmpty(courses);
    }

    [Fact]
    public async Task GetCourseById_WithValidId_ReturnsOkAndCourse()
    {
        // Arrange: Fetch any existing test course from the seed pool
        HttpResponseMessage listResponse = await _client.GetAsync("/api/courses");
        List<Course>? courses = await listResponse.Content.ReadFromJsonAsync<List<Course>>();
        Assert.NotNull(courses);
        Course testCourse = courses.First();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/courses/{testCourse.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Course? course = await response.Content.ReadFromJsonAsync<Course>();
        Assert.NotNull(course);
        Assert.Equal(testCourse.Id, course.Id);
    }

    [Fact]
    public async Task GetCourseById_WithInvalidId_ReturnsNotFound()
    {
        // Act: Query an ID that will never exist
        HttpResponseMessage response = await _client.GetAsync("/api/courses/9999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCourse_WithValidPayload_ReturnsCreatedAndPersists()
    {
        // Arrange
        var newCourse = new Course
        {
            Cr = 3,
            CrLoad = 3,
            SectionMax = 30,
            PercentOfStudents = 1.0
        };

        // Act
        HttpResponseMessage response = await _authClient.PostAsJsonAsync("/api/courses", newCourse);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Course? created = await response.Content.ReadFromJsonAsync<Course>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);

        // Register for post-test cleanup
        _cleanupEndpoints.Add($"/api/courses/{created.Id}");

        // Verify entity can be fetched immediately
        HttpResponseMessage fetchResponse = await _client.GetAsync($"/api/courses/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, fetchResponse.StatusCode);
    }

    [Fact]
    public async Task CreateCourse_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange: Unauthenticated client
        var newCourse = new Course { Cr = 3 };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/courses", newCourse);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
