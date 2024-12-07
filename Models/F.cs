
namespace SerializeReflection.Models;

[Serializable]
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