using System.Data;
using api.DTO.Entity;
using api.Entity;
using api.Interface;
using Dapper;

namespace api.Repository;
public class PlayerRepository : BaseRepository<Player, Guid>, IPlayerRepository
{
    public PlayerRepository(IDbConnection db, IPlayerIconRepository playerIconRepository) : base(db, "Player")
    {
        _playerIconRepository = playerIconRepository;
        _playerIconsLazy = new Lazy<Task<List<PlayerIcon>>>(LoadPlayerIconsAsync);
        _db = db;
    }
    public readonly Lazy<Task<List<PlayerIcon>>> _playerIconsLazy;
    public Task<List<PlayerIcon>> PlayerIcons => _playerIconsLazy.Value;

    private readonly IDbConnection _db;
    private readonly IPlayerIconRepository _playerIconRepository;
    public required IPlayerIconRepository PlayerIconRepository { get; init; }
    private async Task<List<PlayerIcon>> LoadPlayerIconsAsync()
    {
        var icons = await _playerIconRepository.GetAllAsync();
        return [.. icons];
    }
    public async Task<Player> GetByIdWithIconAsync(Guid id)
    {
        var sql = @"
            SELECT * FROM Player
            WHERE Id = @Id;

            SELECT * FROM PlayerDebt
            WHERE PlayerId = @Id
            AND DebtPaid = FALSE;
        ";

        var response = await _db.QueryMultipleAsync(sql, new { Id = id });
        Player player = (await response.ReadAsync<Player>()).ToList()[0];
        IEnumerable<PlayerDebt> debts = await response.ReadAsync<PlayerDebt>();
        player.Debts = debts;

        PlayerIcon icon = (await LoadPlayerIconsAsync()).First(pi => pi.Id == player.IconId);
        player.IconUrl = icon.IconUrl;
        player.IconName = icon.IconName;

        return player;
    }

    public async Task<IEnumerable<Player>> GetAllWithIconsAsync()
    {
        List<PlayerIcon> playerIcons = await LoadPlayerIconsAsync();
        var players = await GetAllAsync();

        var playerIds = players.Select(p => p.Id).ToList();

        var playerDebtSql = @"
            SELECT * FROM PlayerDebt
            WHERE PlayerId = ANY(@PlayerIds)
            AND DebtPaid = FALSE
        ";

        var playerDebts = await _db.QueryAsync<PlayerDebt>(playerDebtSql, new { PlayerIds = playerIds });

        foreach (var player in players)
        {
            player.IconUrl = playerIcons.First(pi => pi.Id == player.IconId).IconUrl;
            player.Debts = playerDebts.Where(pd => pd.PlayerId == player.Id);
        }

        return players.AsList();
    }
    public async Task<IEnumerable<Player>> SearchWithIconsAsync(PlayerWhereParams? includeParams, PlayerWhereParams? excludeParams)
    {
        List<PlayerIcon> playerIcons = await LoadPlayerIconsAsync();

        var players = await SearchAsync(includeParams, excludeParams);

        var playerIds = players.Select(p => p.Id).ToList();

        var playerDebtSql = @"
            SELECT * FROM PlayerDebt
            WHERE PlayerId = ANY(@PlayerIds)
            AND DebtPaid = FALSE;
        ";
        var playerDebts = await _db.QueryAsync<PlayerDebt>(playerDebtSql, new { PlayerIds = playerIds });

        var turnOrderSql = @"
            SELECT * FROM TurnOrder
            WHERE PlayerId = ANY(@PlayerIds);
        ";
        var playerTurnOrders = await _db.QueryAsync<TurnOrder>(turnOrderSql, new { PlayerIds = playerIds });

        foreach (var player in players)
        {
            player.IconUrl = playerIcons.First(pi => pi.Id == player.IconId).IconUrl;
            player.Debts = playerDebts.Where(pd => pd.PlayerId == player.Id);
            if(playerTurnOrders.FirstOrDefault(to => to.PlayerId == player.Id) is TurnOrder turnOrder)
            {
                player.TurnOrder = turnOrder.PlayOrder;
            }
        }

        return players.AsList();
    }

    public async Task AddMoneyToPlayer(Guid playerId, int amount)
    {
        var sql = @"
            UPDATE Player
            SET Money = Money + @Amount
            WHERE Id = @PlayerId
        ";
        await _db.ExecuteAsync(sql, new { PlayerId = playerId, Amount = amount });
    }
    public async Task SubtractMoneyFromPlayer(Guid playerId, int amount)
    {
        var sql = @"
            UPDATE Player
            SET Money = Money - @Amount
            WHERE Id = @PlayerId
        ";
        await _db.ExecuteAsync(sql, new { PlayerId = playerId, Amount = amount });
    }
}