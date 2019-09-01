using System;
using System.Linq;
using System.Windows.Markup;

namespace WindowManager.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(object[]))]
    public sealed class EnumValuesExtension : MarkupExtension
    {
        public EnumValuesExtension()
        {
        }

        public EnumValuesExtension(Type enumType)
        {
            EnumType = enumType;
        }

        [ConstructorArgument("enumType")]
        public Type EnumType { get; set; }

        public bool SortByName { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (EnumType == null)
                throw new InvalidOperationException("The enum type is not set");

            var values = Enum.GetValues(EnumType);
            if (SortByName)
            {
                var list = values.Cast<Enum>().ToList();
                list.Sort((a, b) => string.CompareOrdinal(a.ToString(), b.ToString()));
                return list;
            }

            return values;
        }
    }
}