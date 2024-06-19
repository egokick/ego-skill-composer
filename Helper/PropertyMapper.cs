using skill_composer.Models;
using System.Data;

namespace skill_composer.Helper
{
    /// <summary>
    /// Maps properties from data table rows to concrete objects.
    /// </summary>
    public static class PropertyMapper
    {
        /// <summary>
        /// Converts a collection of DataRows to a list of the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static IList<T> ConvertTo<T>(DataRowCollection rows)
        {
            if (rows == null) return null;
            IList<T> list = new List<T>();
            foreach (DataRow row in rows)
            {
                var item = CreateItem<T>(row);
                list.Add(item);
            }

            return list;
        }

        /// <summary>
        /// Creates an object from the given row.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T CreateItem<T>(DataRow row)
        {
            var obj = default(T);
            if (row == null) return obj;
            obj = Activator.CreateInstance<T>();
            foreach (DataColumn column in row.Table.Columns)
            {
                var prop = obj.GetType().GetProperty(column.ColumnName);
                if(prop==null || prop.SetMethod == null) continue;
                try
                {
                    var value = row[column.ColumnName];
                    if (value is DBNull) continue;
                    if (prop.PropertyType == typeof(byte) && value.GetType() == typeof(sbyte))
                    {
                        var sbyteValue = (sbyte)value;
                        prop.SetValue(obj, (byte)sbyteValue, null);
                    }
                    else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                    {
                        prop.SetValue(obj, Convert.ToBoolean(value), null);
                    }
                    else if (prop.PropertyType == typeof(int) && column.DataType == typeof(long))
                    {
                        prop.SetValue(obj, Convert.ToInt32(value), null);
                    }
                    else if (prop.PropertyType == typeof(int) && column.DataType == typeof(decimal))
                    {
                        prop.SetValue(obj, decimal.ToInt32((decimal)value), null);
                    }
                    else if (prop.PropertyType == typeof(int) && column.DataType == typeof(string))
                    {
                        prop.SetValue(obj, int.Parse((string)value));
                    }
                    else if (prop.PropertyType == typeof(Time))
                    {
                        prop.SetValue(obj, new Time((TimeSpan)value), null);
                    }                    
                    else if (prop.PropertyType == typeof(int[]) && column.DataType == typeof(string))
                    {
                        var stringValue = (string) value;
                        prop.SetValue(obj, stringValue.Split(',').Select(x => int.Parse(x)).ToArray(), null);
                    }
                    else if (prop.PropertyType == typeof(string) && column.DataType == typeof(TimeSpan))
                    {
                        var stringValue =  ((TimeSpan)value).ToString(@"hh\:mm\:ss");
                        prop.SetValue(obj, stringValue, null);
                    }
                    else if ((prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?)) && column.DataType == typeof(UInt64))
                    {
                        var ulongValue = (UInt64)value;
                        if (ulongValue > (UInt64)int.MaxValue)
                        {
                            Console.WriteLine($"ERROR: Value too large for Int32: {ulongValue} PropertyMapper.CreateItem");
                            continue;
                        }
                        prop.SetValue(obj, Convert.ToInt32(ulongValue), null);
                    }
                    else
                    {
                        prop.SetValue(obj, value, null);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"ERROR {ex.Message}  PropertyMapper.CreateItem");
                }
            }

            return obj;
        }

        /// <summary>
        /// Maps properties to a passed in object
        /// </summary>
        /// <typeparam name="T1">fromObject</typeparam>
        /// <typeparam name="T2">toObject</typeparam>
        /// <param name="fromObject"></param>
        /// <param name="toObject"></param>
        /// <returns></returns>
        public static T2 MapProperties<T1, T2>(T1 fromObject, T2 toObject)
        {
            var toFields = toObject.GetType().GetProperties();
            var fromType = fromObject.GetType();

            foreach (var toField in toFields)
            {
                var fromField = fromType.GetProperty(toField.Name);
                if (fromField == null) { continue; }
                try
                {
                    toField.SetValue(toObject, fromField.GetValue(fromObject, null), null);
                }
                catch (Exception) { /* Ignore Exception */ }
            }

            return toObject;
        }

    }
}
