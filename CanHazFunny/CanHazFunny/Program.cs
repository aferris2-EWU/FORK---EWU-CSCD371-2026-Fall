namespace CanHazFunny;

class Program
{
    static void Main(string[] args)
    {
        new Jester(new ConsoleJokeOutput(), new JokeService()).TellJoke();
    }
}
