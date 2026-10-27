using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace Game.Domain;

public class MongoGameTurnRepository : IGameTurnRepository
{
    private readonly IMongoCollection<GameTurnEntity> collection;
    public const string CollectionName = "gameTurns";

    public MongoGameTurnRepository(IMongoDatabase database)
    {
        collection = database.GetCollection<GameTurnEntity>(CollectionName);
        var indexKeys = Builders<GameTurnEntity>.IndexKeys
            .Ascending(t => t.GameId)
            .Descending(t => t.TurnNumber);
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<GameTurnEntity>(indexKeys, indexOptions);
        collection.Indexes.CreateOne(indexModel);
    }

    public GameTurnEntity Insert(GameTurnEntity turn)
    {
        collection.InsertOne(turn);
        return turn;
    }

    public GameTurnEntity FindById(Guid id)
    {
        return collection.Find(t => t.Id == id).FirstOrDefault();
    }

    public IEnumerable<GameTurnEntity> FindLast(Guid gameId, int count)
    {
        return collection
            .Find(t => t.GameId == gameId)
            .SortByDescending(t => t.TurnNumber)
            .Limit(count)
            .ToList();
    }
}