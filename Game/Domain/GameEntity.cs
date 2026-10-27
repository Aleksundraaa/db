using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Game.Domain;

public class GameEntity
{
    [BsonElement]
    private readonly List<Player> players;

    public GameEntity(int turnsCount)
        : this(Guid.Empty, GameStatus.WaitingToStart, turnsCount, 0, new List<Player>())
    {
    }

    [BsonConstructor]
    public GameEntity(Guid id, GameStatus status, int turnsCount, int currentTurnIndex, List<Player> players)
    {
        Id = id;
        Status = status;
        TurnsCount = turnsCount;
        CurrentTurnIndex = currentTurnIndex;
        this.players = players;
    }

    public Guid Id
    {
        get;
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local For MongoDB
        private set;
    }

    public IReadOnlyList<Player> Players => players.AsReadOnly();

    [BsonElement]
    public int TurnsCount { get; }

    public int CurrentTurnIndex { get; private set; }

    public GameStatus Status { get; private set; }

    public void AddPlayer(UserEntity user)
    {
        if (Status != GameStatus.WaitingToStart)
            throw new ArgumentException(Status.ToString());
        players.Add(new Player(user.Id, user.Login));
        if (Players.Count == 2)
            Status = GameStatus.Playing;
    }

    public bool IsFinished()
    {
        return CurrentTurnIndex >= TurnsCount
               || Status == GameStatus.Finished
               || Status == GameStatus.Canceled;
    }

    public void Cancel()
    {
        if (!IsFinished())
            Status = GameStatus.Canceled;
    }

    public bool HaveDecisionOfEveryPlayer => Players.All(p => p.Decision.HasValue);

    public void SetPlayerDecision(Guid userId, PlayerDecision decision)
    {
        if (Status != GameStatus.Playing)
            throw new InvalidOperationException(Status.ToString());
        foreach (var player in Players.Where(p => p.UserId == userId))
        {
            if (player.Decision.HasValue)
                throw new InvalidOperationException(player.Decision.ToString());
            player.Decision = decision;
        }
    }

    public GameTurnEntity FinishTurn()
    {
        var winnerId = Guid.Empty;
        var gameResult = GameResult.Draw;
        var player1 = Players[0];
        var player2 = Players[1];

        if (!player1.Decision.HasValue || !player2.Decision.HasValue)
            throw new InvalidOperationException();

        if (player1.Decision.Value.Beats(player2.Decision.Value))
        {
            player1.Score++;
            winnerId = player1.UserId;
            gameResult = GameResult.Win;
        }
        else if (player2.Decision.Value.Beats(player1.Decision.Value))
        {
            player2.Score++;
            winnerId = player2.UserId;
            gameResult = GameResult.Win;
        }

        var firstPlayerSnapshot = new Player(player1.UserId, player1.Name)
        {
            Score = player1.Score,
            Decision = player1.Decision
        };

        var secondPlayerSnapshot = new Player(player2.UserId, player2.Name)
        {
            Score = player2.Score,
            Decision = player2.Decision
        };

        var result = new GameTurnEntity
        {
            GameId = Id,
            TurnNumber = CurrentTurnIndex,
            FirstPlayer = firstPlayerSnapshot,
            SecondPlayer = secondPlayerSnapshot,
            WinnerId = winnerId,
            Result = gameResult
        };

        foreach (var player in Players)
            player.Decision = null;
        CurrentTurnIndex++;
        if (CurrentTurnIndex == TurnsCount)
            Status = GameStatus.Finished;

        return result;
    }
}