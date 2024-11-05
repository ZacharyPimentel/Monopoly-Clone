public class GameUpdateParams
{
    public bool? InLobby { get; set; }
    public bool? GameOver { get; set; }
    public bool? GameStarted { get; set; }
}

public interface IGameRepository
{
    Task<Game> GetByIdAsync(int id);
    Task<bool> Update(int id,GameUpdateParams updateParams);
}