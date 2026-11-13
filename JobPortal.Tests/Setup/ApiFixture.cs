namespace JobPortal.Tests.Setup;

public class ApiFixture : IAsyncLifetime
{
    public ApiFactory Factory { get; } = new();

    public Task InitializeAsync() => Factory.CreateDatabaseAsync();

    public async Task DisposeAsync()
    {
        await Factory.DropDatabaseAsync();
        await Factory.DisposeAsync();
    }
}

[CollectionDefinition("api")]
public class ApiCollection : ICollectionFixture<ApiFixture>;