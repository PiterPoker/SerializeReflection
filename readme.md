# Сериализация свойств класса в строку

Этот проект демонстрирует, как сериализовать свойства или поля класса в строку на C#. Для этого используется самописный класс сериализации. Производительность процесса сериализации измеряется путем замера времени выполнения до и после вызова функции. Сериализация выполняется в цикле для большей точности.

## Определение класса

Класс `F` имеет пять целочисленных полей: `i1`, `i2`, `i3`, `i4` и `i5`.

```csharp
public class F 
{ 
    public int I1 {get; set;}
    public int I2 {get; set;}
    public int I3 {get; set;}
    public int I4 {get; set;}
    public int I5 {get; set;}
    public static F Get() 
    {

        Random rnd = new();
        return new F() 
        { 
            I1 = rnd.Next(1000), 
            I2 = rnd.Next(1000),
            I3 = rnd.Next(1000),
            I4 = rnd.Next(1000),
            I5 = rnd.Next(1000)
        };
    }

    public string GetJson()
    {
        return $"{{\"I1\":{I1},\"I2\":{I2},\"I3\":{I3},\"I4\":{I4},\"I5\":{I5}}}";
    }
}
```

## Метод сериализации

В проекте используется самописный класс [JsonSerializer](https://github.com/PiterPoker/SerializeReflection/edit/master/JsonSerializer.cs) для сериализации и десериализации, который реализован следующим образом:

```csharp
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

... Приватные методы для сериавлизации ...

        /// <summary>
        /// Десериализация строки в формате Json в объект 
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="jsonString">Строка в формате Json</param>
        /// <returns>Возвращает сериализованный объект</returns>
        public static T Deserialize<T>(string jsonString) where T : new()
        {
            var obj = new T();
            var jsonSpan = jsonString.AsSpan();
            var dictionaryJson = Parse(jsonSpan);
            var objType = obj.GetType();
            var properties = objType.GetProperties();
            var count = properties.Length;
            for (int index = 0; index < count; index++)
            {
                if(dictionaryJson.GetValueOrDefault(properties[index].Name) is object propertyValue)
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

... Приватные методы для десериавлизации ...

}
```

## Измерение производительности

Производительность сериализации измеряется путем замера времени выполнения до и после вызова функции. Сериализация выполняется в цикле для большей точности.

```csharp
using System;
using System.Diagnostics;
using SerializeReflection.CustomSerializer;

class Program
{
    public static void Main()
    {
        Stopwatch stopwatch = new();
        while (true)
        {
            try
            {
                var iterations = EnterIterations();
                RunTest(stopwatch, iterations, CustomDeserializeFClass);
                RunTest(stopwatch, iterations, NewtonsoftJsonDeserializeFClass);
                RunTest(stopwatch, iterations, CustomSerializeFClass);
                RunTest(stopwatch, iterations, NewtonsoftJsonSerializeFClass);
                
                RunTest(stopwatch, iterations, CustomDeserializeUserClass);
                RunTest(stopwatch, iterations, NewtonsoftJsonDeserializeUserClass);
                RunTest(stopwatch, iterations, CustomSerializeUserClass);
                RunTest(stopwatch, iterations, NewtonsoftJsonSerializeUserClass);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    /// <summary>
    /// Ввод количества итераций с клавиатуры 
    /// </summary>
    /// <returns>Возвращает количество итераций, введенных с клавиатуры</returns>
    /// <exception cref="Exception"></exception>
    public static int EnterIterations()
    {
        Console.Write("Enter the iterations count:");
        var stringIterations = Console.ReadLine();
        if (int.TryParse(stringIterations, out int iterations))
        {
            return iterations;
        }
        throw new Exception("Failed to parse the number of iterations");
    }

    /// <summary>
    /// Запуск теста для функции на замер производительности
    /// </summary>
    /// <param name="stopwatch">Объект Timer</param>
    /// <param name="iterations">Количество повторений</param>
    /// <param name="action">Функция для выполнения тестирования</param>
    public static void RunTest(Stopwatch stopwatch, int iterations, Action action)
    {
        Console.WriteLine($"Test function {action?.Method.Name} running...");
        stopwatch.Start();
        for (int i = 0; i < iterations; i++)
        {
            action?.Invoke();
        }
        stopwatch.Stop();
        Console.WriteLine($"Test function {action?.Method.Name} finished.");
        ShowResult(stopwatch, action?.Method.Name, iterations);
        stopwatch.Reset();
    }

    /// <summary>
    /// Отображение результатов тестирования
    /// </summary>
    /// <param name="stopwatch">Объект Timer</param>
    /// <param name="functionName">Наименование функции</param>
    /// <param name="iterations">Количество повторений</param>
    private static void ShowResult(Stopwatch stopwatch, string? functionName, int iterations)
    {
        Console.WriteLine(string.Format("Iteration count {0} for function {1} elapsed time (ms): {2}", iterations, functionName, stopwatch.ElapsedMilliseconds));
    }
}
```

## Среда разработки

- **IDE:** Visual Studio Code
- **Версия .NET:** .NET 6

## Использование

1. Клонируйте репозиторий.
2. Откройте проект в Visual Studio Code.
3. Убедитесь, что у вас установлен .NET 6 SDK.
4. Откройте терминал в Visual Studio Code и выполните следующие команды:
   ```sh
   dotnet build
   dotnet run
   ```
5. Проверьте вывод консоли для сериализованной строки и времени выполнения процесса сериализации.
