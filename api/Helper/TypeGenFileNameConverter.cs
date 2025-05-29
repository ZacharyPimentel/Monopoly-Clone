using TypeGen.Core.Converters;

namespace api.Helper;
public class NoOpFileNameConverter : ITypeNameConverter
{
    public string Convert(string input, Type type)
    {
        return input; // just return the original name unchanged
    }
}