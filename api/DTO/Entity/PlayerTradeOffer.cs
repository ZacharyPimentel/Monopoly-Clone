using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Entity;
[ExportTsInterface]
public class PlayerTradeOffer
{
    public required Guid PlayerId { get; set; }
    public int Money { get; set; } = 0;
    public int GetOutOfJailFreeCards { get; set; } = 0;
    public List<int> GamePropertyIds { get; set; } = [];
}