using System.ComponentModel;
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
    public static string GetEnumDescription(this Enum enumValue)
    {
        var field = enumValue.GetType().GetField(enumValue.ToString());
        if (field == null)
            return enumValue.ToString();

        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
        {
            return attribute.Description;
        }

        return enumValue.ToString();
    }
}
