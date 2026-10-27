using System;
using System.Collections.Generic;

namespace Game.Domain;

public interface IGameTurnRepository
{
    GameTurnEntity Insert(GameTurnEntity turn);
    GameTurnEntity FindById(Guid id);
    IEnumerable<GameTurnEntity> FindLast(Guid gameId, int count);
}