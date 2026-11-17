// Purpose: Base integration test fixture providing an in-process ASP.NET Core test server and PostgreSQL test data constants.

using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PRPA.Portfolio.Examples.Tests;

/// <summary>
/// Base class for integration tests that run against an ASP.NET Core WebApplicationFactory.
/// Configures pre-seeded test fixtures isolated within the ID range >= 99000.
/// </summary>
public class TestBase : WebApplicationFactory<TestProgramEntrypoint>
{
    /// <summary>
    /// Test data ID range start — all test entities use IDs >= this value
    /// to avoid colliding with active development or seed records.
    /// </summary>
    protected const int TestIdRangeStart = 99000;

    /// <summary>
    /// Well-known test college IDs matching database seed scripts.
    /// </summary>
    protected static class TestCollegeIds
    {
        public const int ScienceAndEngineering = 99001;
        public const int Business = 99002;
        public const int Arts = 99003;
    }

    /// <summary>
    /// Well-known test department IDs.
    /// </summary>
    protected static class TestDepartmentIds
    {
        public const int ComputerScience = 99001;
        public const int Mathematics = 99002;
        public const int Accounting = 99003;
    }

    /// <summary>
    /// Well-known test academic program IDs.
    /// </summary>
    protected static class TestProgramIds
    {
        public const int ComputerScienceBS = 99001;
        public const int ComputerScienceMS = 99002;
        public const int MathematicsBA = 99003;
    }

    /// <summary>
    /// Creates an authenticated HTTP test client with pre-configured bearer tokens.
    /// </summary>
    protected HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-mock-jwt-token");
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        // In production, database connection overrides and mock auth handlers are registered here.
    }
}

// Stub representing ASP.NET Core Program entrypoint
public class TestProgramEntrypoint { }
