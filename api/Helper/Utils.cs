using System.Reflection;

namespace api.Helper;
public static class Utils
{
    public static bool HasProperty<T>(string propertyName)
    {
        return typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance) != null;
    }

}
