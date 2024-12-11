public class Card
{
    public int Id { get; set; }
    public int? AdvanceToSpaceId { get; set; }
    public int? Amount { get; set; }
    public int CardTypeId { get; set; }
    public required int CardActionId { get; set; }
}