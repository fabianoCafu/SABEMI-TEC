using System.ComponentModel;
using System.Reflection;

namespace SABEMITEC.PagamentoAPI.Util
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo? field = value.GetType().GetField(value.ToString());

            if (field == null)
            {
                return value.ToString();
            }

            DescriptionAttribute? attribute = field.GetCustomAttribute<DescriptionAttribute>();

            return attribute?.Description ?? value.ToString();
        }
    }
}


