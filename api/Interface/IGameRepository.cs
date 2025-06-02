namespace api.Interface;
public class GameCreateParams
{
    public required string GameName { get; set; }
    public required int ThemeId { get; set; }
}

public class GameUpdateParams
{
    public bool? InLobby { get; set; }
    public bool? GameOver { get; set; }
    public bool? GameStarted { get; set; }
    public int? StartingMoney { get; set; }
    public bool? FullSetDoublePropertyRent { get; set; }
}

public class GameWhereParams
{
    public string? GameName { get; set; }
}

public interface IGameRepository : IBaseRepository<Game, Guid>
{
    Task<Game?> GetByIdWithDetailsAsync(Guid id);
    Task<List<Game>> Search(GameWhereParams gameWhereParams);
    Task<List<Game>> GetAllWithPlayerCountAsync();
}