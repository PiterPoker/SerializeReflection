namespace SerializeReflection.Models
{
    public class User
    {
        public List<Project>? Projects { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public DateTime Birthday { get; set; }
        public int Rank { get; set; }
        public string? Email { get; set; }
        public Gender Gender { get; set; }

        public static User GetUser()
        {  
            // Массив имен 
            string[] firstNames = { "Anna", "John", "Maria", "Sergey", "Catherine", "Alex", "Olga", "Dmitry", "Helen", "Andrew", "Tanya", "Max", "Svetlana", "Roman", "Natalie", "Vladimir", "Ludmila", "Michael", "Alena", "Victor" };
            // Массив фамилий 
            string[] lastNames = { "Smith", "Johnson", "Brown", "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia", "Martinez", "Robinson", "Clark", "Lewis", "Walker", "Hall", "Allen", "Young" };
            Random rnd = new();
            return new()
            {
                Name = firstNames[rnd.Next(20)],
                LastName = lastNames[rnd.Next(20)],
                Birthday = DateTime.Now.AddYears(-26),
                Email = "uset@mail.com",
                Gender = Gender.male,
                Rank = rnd.Next(3),
                Projects = new List<Project>
                {
                    new() {
                        Name = $"Projsect {rnd.Next(5)}",
                        Type = "Game",
                        Dates = new List<DateTime>
                        {
                            DateTime.Now.AddDays(-rnd.Next(1000)),
                            DateTime.Now.AddDays(-rnd.Next(1000)),
                            DateTime.Now.AddDays(-rnd.Next(1000))
                        }
                    },
                    new() {
                        Name = $"Projsect {rnd.Next(5)}",
                        Type = "Info",
                        Dates = new List<DateTime>
                        {
                            DateTime.Now.AddDays(-rnd.Next(1000)),
                            DateTime.Now.AddDays(-rnd.Next(1000)),
                            DateTime.Now.AddDays(-rnd.Next(1000))
                        }
                    },
                    new() {
                        Name = $"Projsect {rnd.Next(5)}",
                        Type = "Media",
                        Dates = new List<DateTime>
                        {
                            DateTime.Now.AddDays(-rnd.Next(1000)),
                            DateTime.Now.AddDays(-rnd.Next(1000)),
                            DateTime.Now.AddDays(-rnd.Next(1000))
                        }
                    }
                }
            };
        }
    }

    public enum Gender
    {
        male,
        female
    }
}