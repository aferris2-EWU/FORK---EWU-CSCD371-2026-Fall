using System;
using System.IO;
using Xunit;

namespace CanHazFunny.Tests;

public class ConsoleJokeOutputTests
{
    [Fact]
    public void WriteJoke_WritesJokeToConsole()
    {
        TextWriter originalOut = Console.Out;
        using StringWriter writer = new();
        Console.SetOut(writer);

        try
        {
            new ConsoleJokeOutput().WriteJoke("Why do programmers prefer dark mode? Because light attracts bugs.");
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        Assert.Equal("Why do programmers prefer dark mode? Because light attracts bugs." + Environment.NewLine, writer.ToString());
    }
}
