using System.Data;
using api.Interface;

namespace api.Repository;

public interface ICardRepository : IBaseRepository<Card, int>
{
    
}
public class CardRepository(IDbConnection db) : BaseRepository<Card, int>(db, "Card"), ICardRepository
{
    
}