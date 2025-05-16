public interface IThemeRepository
{
    Task<List<BoardSpace>> GetBoardSpaces(int themeId);
    Task<List<Theme>> GetAll();
    Task<List<ThemeColor>> GetColors(int themeId);
}