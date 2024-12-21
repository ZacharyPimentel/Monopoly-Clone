public class ThemeColor
{
    public int Id { get; set; }
    public int ThemeId { get; set; }
    public int ColorGroupId { get; set; }
    public required string Color { get; set; }
    public int Shade { get; set; }
}