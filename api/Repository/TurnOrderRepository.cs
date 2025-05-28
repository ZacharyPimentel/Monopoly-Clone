using System.Data;
using api.Interface;

namespace api.Repository;
public class TurnOrderRepository(IDbConnection db): BaseRepository<TurnOrder,Guid>(db,"TurnOrder"), ITurnOrderRepository
{
}