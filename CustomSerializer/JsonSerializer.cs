using System.Collections;
using System.Reflection;
using System.Text;

namespace SerializeReflection.CustomSerializer
{
    public static class JsonSerializer
    {
        /// <summary>
        /// Метод сериализации
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="obj">Объект сериализации</param>
        /// <returns>Возвращает сериализаванную строку формата Json</returns>
        public static string Serialize<T>(T obj) => Serialize(obj, new StringBuilder());
        private static string Serialize<T>(T obj, StringBuilder jsonBuilder)
        {
            jsonBuilder.Append('{');
            var tType = obj?.GetType();
            var tProperties = tType?.GetProperties();
            if (tProperties is not null)
            {
                var count = tProperties.Length;
                for (int index = 0; index < count; index++)
                {
                    var pType = tProperties[index].PropertyType;

                    if (IsIEnumerable(pType))
                    {
                        SerializeCollection(obj, tProperties[index], jsonBuilder);
                    }
                    else
                    {
                        SerializeProperty(obj, tProperties[index], jsonBuilder);
                    }

                    if (index != count - 1)
                        jsonBuilder.Append(',');
                }
            }
            jsonBuilder.Append('}');
            return jsonBuilder.ToString();
        }

        private static bool IsIEnumerable(Type pType)
        {
            return pType.GetInterfaces()
                                   .Any(x => x == typeof(IEnumerable) && pType != typeof(string));
        }

        private static void SerializeProperty<T>(T mainValue, PropertyInfo? value, StringBuilder propertyBuilder)
        {
            if (value?.PropertyType == typeof(string)
                || value?.PropertyType?.IsEnum is true
                || value?.PropertyType == typeof(DateTime))
                propertyBuilder.AppendFormat("\"{0}\": \"{1}\"", value?.Name, value?.GetValue(mainValue));

            else if (value?.PropertyType?.IsValueType is true
                && value?.PropertyType != typeof(DateTime))
                propertyBuilder.AppendFormat("\"{0}\": {1}", value?.Name, value?.GetValue(mainValue));

            else if (value?.PropertyType?.IsClass is true)
                Serialize(value?.GetValue(mainValue));
        }

        private static void SerializeCollection<T>(T mainValue, PropertyInfo? value, StringBuilder collectionBuilder)
        {
            collectionBuilder.AppendFormat("\"{0}\":[", value?.Name);
            if (value?.GetValue(mainValue) is IEnumerable collection)
            {
                var enumerator = collection.GetEnumerator();
                var isNext = enumerator.MoveNext();
                while (isNext)
                {
                    if (enumerator.Current is object item)
                    {
                        if (item.GetType() is Type tItem)
                        {
                            if (tItem == typeof(string)
                            || tItem.IsEnum is true
                            || tItem == typeof(DateTime))
                            {
                                collectionBuilder.AppendFormat("\"{0}\"", item);
                            }
                            else if (tItem.IsValueType)
                            {
                                collectionBuilder.AppendFormat("{0}", item.ToString()?.ToLower());
                            }
                            else
                            {
                                Serialize(item, collectionBuilder);
                            }

                            if (isNext = enumerator.MoveNext())
                                collectionBuilder.Append(',');
                        }
                    }
                }
            }
            collectionBuilder.Append(']');
        }

        /// <summary>
        /// Десириализация строки в формате Json в объект 
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="jsonString">Строка в формате Json</param>
        /// <returns>Возвращает сериализованный объект</returns>
        public static T Deserialize<T>(string jsonString) where T : new()
        {
            var obj = new T();
            var jsonSpan = jsonString.AsSpan();
            var dictionatyJson = Parse(jsonSpan);
            var objType = obj.GetType();
            var properties = objType.GetProperties();
            var count = properties.Length;
            for (int index = 0; index < count; index++)
            {
                if(dictionatyJson.GetValueOrDefault(properties[index].Name) is object propertyValue)
                {
                    if (IsIEnumerable(properties[index].PropertyType))
                    {
                        object? collection = null;
                        if (propertyValue is List<object> collectionJson)
                            collection = FillingCollection(properties[index].PropertyType, collectionJson);

                        if (collection is not null)
                        {
                            properties[index].SetValue(obj, collection);
                        }
                    }
                    else
                    {
                        if (SetProperty(properties[index], propertyValue) is object convertedValue)
                            properties[index].SetValue(obj, convertedValue);
                    }

                }
            }
            return obj;
        }

        private static object? FillingCollection(Type objType, List<object> objJsonValue)
        {
            var objGenericType = objType.GetGenericTypeDefinition();
            if (objType.GetGenericArguments() is Type[] objArguments)
            {
                foreach (var objArgument in objArguments)
                {
                    var objCollectionType = objGenericType.MakeGenericType(objArgument);
                    if (Activator.CreateInstance(objCollectionType) is object objCollection)
                    {
                        foreach (var row in objJsonValue)
                        {
                            object[] perametr = FillingParameterOfCollection(objArgument, row);
                            if (objCollectionType.GetMethod("Add") is MethodInfo addMethod)
                                addMethod.Invoke(objCollection, perametr);
                        }
                        return objCollection;
                    }
                }
            }
            return default;
        }

        private static object[] FillingParameterOfCollection(Type objArgument, object objectValue)
        {
            var perametr = Array.Empty<object>();
            var dictionatyJson = objectValue as Dictionary<string, object>;
            if (objArgument == typeof(string) || objArgument.IsValueType)
            {
                perametr = new object[] {
                                    Convert.ChangeType(objectValue, objArgument)
                                };
            }
            else if (dictionatyJson is not null
                && SetClass(objArgument, dictionatyJson) is object objElement)
            {
                perametr = new object[] {
                                    objElement
                                };
            }

            return perametr;
        }

        private static object? SetClass(Type objArgument, Dictionary<string, object> jsonClass)
        {
                if (Activator.CreateInstance(objArgument) is object objElement)
                {
                    foreach (var property in objArgument.GetProperties())
                    {
                        if(jsonClass.GetValueOrDefault(property.Name) is object jsonObject)
                        {
                            if (jsonObject is Dictionary<string, object> propertyJson)
                            {
                                SetClass(property.PropertyType, propertyJson);
                            }

                            else if (jsonObject is List<object> listJson)
                            {
                                if (FillingCollection(property.PropertyType, listJson) is object collectionJson)
                                {
                                    property.SetValue(objElement, collectionJson);
                                }
                            }
                            else if (SetProperty(property, jsonObject) is object convertedValue)
                            {
                                property.SetValue(objElement, convertedValue);
                            }

                        }
                    }
                    return objElement;
                }
            return default;
        }

        private static object? SetProperty(PropertyInfo property, object objectJson)
        {

            //if (property.PropertyType.IsEnum && objectJson is string enumString)
            if (property.PropertyType.IsEnum)
            {
                if (objectJson is double){
                    if(Convert.ChangeType(objectJson, Enum.GetUnderlyingType(property.PropertyType)) is int intObjectJson){
                        return Enum.ToObject(property.PropertyType, intObjectJson);
                    }
                }
                return Enum.ToObject(property.PropertyType, objectJson);

            }
            else if (objectJson is object propertyJson)
            {
                return Convert.ChangeType(propertyJson, property.PropertyType);
            }
            return default;
        }

        public static Dictionary<string, object> Parse(ReadOnlySpan<char> jsonString)
        {
            var jsonSpanLenght = 0;
            SkipWhitespace(jsonString, ref jsonSpanLenght);
            return ParseObject(jsonString, ref jsonSpanLenght);
        }

        private static Dictionary<string, object> ParseObject(ReadOnlySpan<char> jsonString, ref int index)
        {
            var obj = new Dictionary<string, object>();
            Expect('{', jsonString, ref index);
            SkipWhitespace(jsonString, ref index);
            while (jsonString[index] != '}')
            {
                var key = ParseString(jsonString, ref index);
                SkipWhitespace(jsonString, ref index);
                Expect(':', jsonString, ref index);
                SkipWhitespace(jsonString, ref index);
                var value = ParseValue(jsonString, ref index);
                obj[key] = value;
                SkipWhitespace(jsonString, ref index);
                if (jsonString[index] == ',')
                {
                    index++;
                    SkipWhitespace(jsonString, ref index);
                }
            }
            Expect('}', jsonString, ref index);
            return obj;
        }

        private static List<object> ParseArray(ReadOnlySpan<char> jsonString, ref int index)
        {
            var array = new List<object>();
            Expect('[', jsonString, ref index);
            SkipWhitespace(jsonString, ref index);
            while (jsonString[index] != ']')
            {
                array.Add(ParseValue(jsonString, ref index));
                SkipWhitespace(jsonString, ref index);
                if (jsonString[index] == ',')
                {
                    index++;
                    SkipWhitespace(jsonString, ref index);
                }
            }
            Expect(']', jsonString, ref index);
            return array;
        }

        private static object ParseValue(ReadOnlySpan<char> jsonString, ref int index)
        {
            SkipWhitespace(jsonString, ref index);
            if (jsonString[index] == '"')
            {
                return ParseString(jsonString, ref index);
            }
            else if (jsonString[index] == '{')
            {
                return ParseObject(jsonString, ref index);
            }
            else if (jsonString[index] == '[')
            {
                return ParseArray(jsonString, ref index);
            }
            else if (char.IsDigit(jsonString[index]) || jsonString[index] == '-')
            {
                return ParseNumber(jsonString, ref index);
            }
            else if (jsonString[index] == 't' && jsonString.Slice(index, 4).SequenceEqual("true"))
            {
                index += 4;
                return true;
            }
            else if (jsonString[index] == 'f' && jsonString.Slice(index, 5).SequenceEqual("false"))
            {
                index += 5;
                return false;
            }
            else if (jsonString[index] == 'n' && jsonString.Slice(index, 4).SequenceEqual("null"))
            {
                index += 4;
                return null;
            }
            else
            {
                throw new InvalidOperationException("Unexpected character");
            }
        }

        private static string ParseString(ReadOnlySpan<char> jsonString, ref int index)
        {
            Expect('"', jsonString, ref index);
            var stringBuilder = new StringBuilder();
            while (jsonString[index] != '"')
            {
                stringBuilder.Append(jsonString[index]);
                index++;
            }
            Expect('"', jsonString, ref index);
            return stringBuilder.ToString();
        }

        private static double ParseNumber(ReadOnlySpan<char> jsonString, ref int index)
        {
            int startIndex = index;
            while (index < jsonString.Length && (char.IsDigit(jsonString[index]) || jsonString[index] == '.' || jsonString[index] == '-'))
            {
                index++;
            }
            return double.Parse(jsonString.Slice(startIndex, index - startIndex));
        }

        private static void SkipWhitespace(ReadOnlySpan<char> jsonString, ref int index)
        {
            while (index < jsonString.Length && char.IsWhiteSpace(jsonString[index]))
            {
                index++;
            }
        }

        private static void Expect(char expectedChar, ReadOnlySpan<char> jsonString, ref int index)
        {
            if (jsonString[index] != expectedChar)
            {
                throw new InvalidOperationException($"Expected '{expectedChar}' but found '{jsonString[index]}'");
            }
            index++;
        }
    }
}
