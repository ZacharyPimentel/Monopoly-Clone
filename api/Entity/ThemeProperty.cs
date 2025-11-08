namespace api.Entity;
public class ThemeProperty
{
    public int Id { get; set; }
    public int ThemeId { get; set; }
    public int PropertyId { get; set; }
    public required string Color { get; set; }
}