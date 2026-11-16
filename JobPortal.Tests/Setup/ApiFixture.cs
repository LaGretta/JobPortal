namespace JobPortal.Tests.Setup;

public class ApiFixture : IAsyncLifetime
{
    public ApiFactory Factory { get; } = new();

    public async Task InitializeAsync()
    {
        await Factory.CreateDatabaseAsync();

        await Factory.SeedAdminAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DropDatabaseAsync();
        await Factory.DisposeAsync();
    }
}

[CollectionDefinition("api")]
public class ApiCollection : ICollectionFixture<ApiFixture>;