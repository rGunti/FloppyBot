using DotNet.Testcontainers.Containers;
using FloppyBot.Base.Storage.MongoDb;
using FloppyBot.FileStorage.Entities;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace FloppyBot.IntegrationTest;

public class SampleTest : IAsyncLifetime
{
    private readonly IContainer _container;
    private IMongoClient _mongoClient;
    private MongoDbRepositoryFactory _repositoryFactory;

    public SampleTest()
    {
        _container = new MongoDbBuilder().WithPortBinding(27017, true).Build();
    }

    [Fact]
    public void RunTest()
    {
        var repository = _repositoryFactory.GetRepository<FileHeader>();
        var fileHeader = new FileHeader(null!, "Raphael", "Sample File", 25000, "text/plain");
        repository.Insert(fileHeader);

        var fileHeaders = repository.GetAll().ToArray();

        Assert.Single(fileHeaders, h => h == fileHeader with { Id = null!, });
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var mongoUrl = MongoUrl.Create(
            $"mongodb://mongo:mongo@localhost:{_container.GetMappedPublicPort(27017)}"
        );
        _mongoClient = new MongoClient(mongoUrl);
        _repositoryFactory = new MongoDbRepositoryFactory(_mongoClient.GetDatabase("test"));
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }
}
