using System.Reflection;

public static class ItemMapper
{
    public static void CopyValuesFromScriptableObject(ItemData target, ItemDataSO source)
    {
        var targetType = target.GetType();
        var sourceType = source.GetType();

        foreach (var field in sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var targetField = targetType.GetField(field.Name);
            if (targetField != null && targetField.FieldType == field.FieldType)
            {
                targetField.SetValue(target, field.GetValue(source));
            }
        }

        foreach (var property in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var targetProperty = targetType.GetProperty(property.Name);
            if (targetProperty != null && targetProperty.CanWrite && targetProperty.PropertyType == property.PropertyType)
            {
                targetProperty.SetValue(target, property.GetValue(source));
            }
        }
    }
}
