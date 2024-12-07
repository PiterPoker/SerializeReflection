using SerializeReflection.CustomSerializer;
using SerializeReflection.Models;
using System.Diagnostics;

public class Program
{
    public static void Main()
    {
        Stopwatch stopwatch = new();
        while (true)
        {
            try
            {
                var inerations = EnterInerations();
                RunTest(stopwatch, inerations, CustomDeserializeFClass);
                RunTest(stopwatch, inerations, NewtonsoftJsonDeserializeFClass);
                RunTest(stopwatch, inerations, CustomSerializeFClass);
                RunTest(stopwatch, inerations, NewtonsoftJsonSerializeFClass);
                
                RunTest(stopwatch, inerations, CustomDeserializeUserClass);
                RunTest(stopwatch, inerations, NewtonsoftJsonDeserializeUserClass);
                RunTest(stopwatch, inerations, CustomSerializeUserClass);
                RunTest(stopwatch, inerations, NewtonsoftJsonSerializeUserClass);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    /// <summary>
    /// Ввод количество итераций с клавиатуры 
    /// </summary>
    /// <returns>Возвращает количество итераций введенных с клавиатуры</returns>
    /// <exception cref="Exception"></exception>
    public static int EnterInerations()
    {
        Console.Write("Enter the iterations count:");
        var stringInerations = Console.ReadLine();
        if (int.TryParse(stringInerations, out int inerations))
        {
            return inerations;
        }
        throw new Exception("Failed to parse the number of iterations");
    }

    /// <summary>
    /// Запуск теста для функции на замер производительности
    /// </summary>
    /// <param name="stopwatch">Объект Timer</param>
    /// <param name="iterations">Количество повторений</param>
    /// <param name="action">Функция для выполнения тестироования</param>
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
    /// Отображения результатов тестирования
    /// </summary>
    /// <param name="stopwatch">Объект Timer</param>
    /// <param name="functionName">Нименование функции</param>
    /// <param name="iterations">Количество повторений</param>
    private static void ShowResult(Stopwatch stopwatch, string? functionName, int iterations) => Console.WriteLine(string.Format("Iteration count {0} for function {1} elapsed time (ms): {2}", iterations, functionName, stopwatch.ElapsedMilliseconds));
    
    #region Class F

    public static void CustomSerializeFClass()
    {
        string jsonString = JsonSerializer.Serialize(F.Get());
        File.WriteAllText($"jsonFiles/{nameof(F)}.json", jsonString);
    }

    public static void CustomDeserializeFClass()
    {
        string jsonString = File.ReadAllText($"jsonFiles/{nameof(F)}.json");
        JsonSerializer.Deserialize<F>(F.Get().GetJson());
    }

    public static void NewtonsoftJsonSerializeFClass()
    {
        string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(F.Get());
        File.WriteAllText($"jsonFiles/{nameof(F)}.json", jsonString);
    }

    public static void NewtonsoftJsonDeserializeFClass()
    {
        string jsonString = File.ReadAllText($"jsonFiles/{nameof(F)}.json");
        Newtonsoft.Json.JsonConvert.DeserializeObject<F>(jsonString);
    }

    #endregion
    
    #region Class User

    public static void CustomSerializeUserClass()
    {
        string jsonString = JsonSerializer.Serialize(User.GetUser());
        File.WriteAllText($"jsonFiles/{nameof(User)}.json", jsonString);
    }

    public static void CustomDeserializeUserClass()
    {
        string jsonString = File.ReadAllText($"jsonFiles/{nameof(User)}.json");
        JsonSerializer.Deserialize<User>(jsonString);
    }

    public static void NewtonsoftJsonSerializeUserClass()
    {
        string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(User.GetUser());
        File.WriteAllText($"jsonFiles/{nameof(User)}.json", jsonString);
    }

    public static void NewtonsoftJsonDeserializeUserClass()
    {
        string jsonString = File.ReadAllText($"jsonFiles/{nameof(User)}.json");
        Newtonsoft.Json.JsonConvert.DeserializeObject<User>(jsonString);
    }

    #endregion
}
