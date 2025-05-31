using System.Reflection;
using System.Runtime.Serialization;
namespace api.Helper;
public static class EnumExtensions
{
    public static string GetEnumStringValue(this Enum enumValue)
    {
        var memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();
        if (memberInfo != null)
        {
            var enumMemberAttr = memberInfo.GetCustomAttribute<EnumMemberAttribute>();
            if (enumMemberAttr != null && enumMemberAttr.Value != null)
            {
                return enumMemberAttr.Value;
            }
        }
        // fallback to the enum name as string
        return enumValue.ToString();
    }
}
