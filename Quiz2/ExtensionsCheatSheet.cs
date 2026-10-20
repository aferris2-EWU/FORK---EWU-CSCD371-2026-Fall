public static class Extensions
{
    extension(Person person)          // instance: named receiver
    {
        public string GetFullName()
            => $"{person.FirstName} {person.LastName}";
    }

    extension(Person)                 // static: type only
    {
        public static Person CreateKevin()
            => new Person { FirstName = "Kevin", LastName = "Bost" };
    }
}

// if you see (Person person) it must *not* say static below
// if you see (Person) it must say static below

// link to practice quiz: https://claude.ai/artifact/5cXdM5bon2hbjLhmBjwzh3