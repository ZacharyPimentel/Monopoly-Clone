public class BoardSpace
{
    public int Id { get; set; }
    public required int BoardSpaceCategoryId { get; set; }
    public Property? Property { get; set; }
    //Joined from BoardSpaceTheme
    public required string BoardSpaceName { get; set; }
}