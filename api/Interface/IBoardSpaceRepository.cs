namespace api.Interface;
public interface IBoardSpaceRepository : IBaseRepository<BoardSpace, int>
{
    public Task<List<BoardSpace>> GetAllForGameWithDetailsAsync(Guid gameId);
}