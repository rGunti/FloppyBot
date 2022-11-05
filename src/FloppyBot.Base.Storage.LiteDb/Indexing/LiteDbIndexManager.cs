using FloppyBot.Base.Storage.Indexing;

namespace FloppyBot.Base.Storage.LiteDb.Indexing;

public class LiteDbIndexManager : IIndexManager
{
    public bool SupportsIndices => false;

    public bool IndexExists(string collectionName, string indexName, string[] fields)
    {
        throw new NotImplementedException();
    }

    public void CreateIndex(string collectionName, string indexName, string[] fields)
    {
        throw new NotImplementedException();
    }

    public void DeleteIndex(string collectionName, string indexName)
    {
        throw new NotImplementedException();
    }
}
