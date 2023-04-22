namespace SignalR.Example.Model
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime Date { get; set; }

        public Person() { }

        public Person(string name, int age, DateTime date)
        {
            Name = name;
            Age = age;
            Date = date;
        }
    }
}
