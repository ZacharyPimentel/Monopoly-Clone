public interface IThemeRepository
{
    Task<List<BoardSpace>> GetBoardSpaces(int themeId);
    Task<List<Theme>> GetAll();
}