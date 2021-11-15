using System;
using System.ComponentModel;
using System.Linq;

namespace DungeonMapper2.Utilities
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum icon)
        {
            var iconMember = icon.GetType().GetMember(icon.ToString())?.FirstOrDefault();
            var descriptionAttribute = iconMember.GetCustomAttributes(typeof(DescriptionAttribute), false)?.FirstOrDefault() as DescriptionAttribute;
            return descriptionAttribute?.Description;
        }
    }
}
