using System;

namespace Game.Domain;

public class GameTurnEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GameId { get; set; }
    public int TurnNumber { get; set; }
    public Player FirstPlayer { get; init; }
    public Player SecondPlayer { get; init; }
    public GameResult Result { get; set; }
    public Guid WinnerId { get; set; }
}