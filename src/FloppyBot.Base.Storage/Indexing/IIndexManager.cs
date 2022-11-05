namespace FloppyBot.Base.Storage.Indexing;

public interface IIndexManager
{
    bool SupportsIndices { get; }
    bool IndexExists(string collectionName, string indexName, string[] fields);
    void CreateIndex(string collectionName, string indexName, string[] fields);
    void DeleteIndex(string collectionName, string indexName);
}
