# Study Guide: Interfaces, Inheritance & SOLID (CanHazFunny / Assignment 3)

This guide covers the **subject matter** behind CanHazFunny, not the assignment itself:

- Essential C# **Chapter 7 (Inheritance)** and **Chapter 8 (Interfaces)**
- **SOLID** principles, especially Dependency Inversion
- **Dependency injection**, null checking, and **unit testing with Moq**

The practice questions follow the same format as Quiz 1: *"What does this code print? Or does it fail to compile / throw?"*
Every snippet was run with `dotnet run file.cs` on .NET 10, so the answers below are the actual output.

> **Quiz 1 pattern to remember:** most of the questions were trick questions about **when code is allowed to change something**
> (`init`, `private set`, value parameters, reassigning a reference parameter). Expect Quiz 2 to do the same with
> **which method actually gets called** (virtual/override/new, explicit interface implementations, default interface methods).

---

## Part 1: Core Concepts

### 1.1 Interfaces: what they are

An interface is a **contract**: a list of members a type promises to provide. CanHazFunny defines two:

```csharp
public interface IJokeService { string GetJoke(); }
public interface IJokeOutput  { void WriteJoke(string joke); }
```

| Rule | Detail |
|---|---|
| Cannot be instantiated | `new IJokeService()` → **CS0144** compile error |
| No instance fields | `private int _count;` inside an interface → **CS0525** |
| Members are `public` by default | Implementing members on the class must be `public` (for implicit implementation) |
| A class can implement **many** interfaces | `class Robot : IWalker, IRunner` |
| A class can inherit only **one** base class | C# has single inheritance for classes |
| Naming convention | Prefix with `I` (`IJokeService`) |
| Interfaces can inherit other interfaces | `interface IFileLogger : ILogger { }` |
| C# 8+ allows **default implementations** | Method bodies in the interface (see 1.4) |
| C# 8+ allows **static** members | `static int Count { get; set; }`, called as `IShape.Count` |

### 1.2 Implicit vs. explicit implementation

```csharp
public class Greeter : IGreeter
{
    public string Greet() => "Hi";            // IMPLICIT: callable on Greeter or IGreeter
    string IGreeter.Greet() => "Hi";          // EXPLICIT: callable ONLY through an IGreeter reference
}
```

- An **explicit** implementation has **no access modifier** and is prefixed with the interface name.
- You **cannot** call it through a variable of the class type; you must cast to the interface first.
- Use it to (a) hide interface plumbing from the class's public API, or (b) give **different** behavior when two interfaces have the same signature.

### 1.3 Interface vs. abstract class

| | Interface | Abstract class |
|---|---|---|
| Multiple inheritance | ✅ implement many | ❌ inherit only one |
| Instance fields / state | ❌ | ✅ |
| Constructors | ❌ | ✅ |
| Can be instantiated | ❌ | ❌ |
| Access modifiers on members | public by default | any |
| Relationship | "**can do**" (capability) | "**is a**" (shared base identity + code) |

### 1.4 Default interface methods (C# 8)

```csharp
public interface ILogger
{
    void Log(string message);
    void LogError(string message) => Log($"ERROR: {message}");  // default implementation
}
public class ConsoleLogger : ILogger { public void Log(string m) => Console.WriteLine(m); }
```

- The class doesn't have to implement `LogError`.
- **Gotcha:** the default method is **not inherited by the class**. `new ConsoleLogger().LogError("x")` → **CS1061** compile error. You can only call it through an `ILogger` reference.
- If the class *does* define its own `LogError`, that version wins even when called through the interface.

### 1.5 Inheritance keywords (Chapter 7)

| Keyword | Meaning |
|---|---|
| `virtual` | Base method that derived classes are *allowed* to override |
| `override` | Replaces a virtual/abstract base method. Dispatch uses the **runtime** type |
| `new` (modifier) | **Hides** the base method. Dispatch uses the **compile-time (variable)** type |
| `abstract` | No body. Derived (non-abstract) classes **must** override it (**CS0534** if they don't). Class must be abstract |
| `sealed` class | Cannot be inherited (**CS0509**) |
| `sealed override` | Further-derived classes cannot override this member again |
| `base.Method()` | Calls the parent's version |
| `: base(args)` | Calls a parent constructor. **Required** if the parent has no parameterless constructor (**CS7036**) |
| `protected` | Visible to this class and derived classes only |

**Constructor order:** the base constructor runs **before** the derived constructor body.

**The key rule:**
> `override` → look at the **object** (runtime type).
> `new` / non-virtual → look at the **variable** (compile-time type).

### 1.6 Casting & type checks

| Syntax | On failure |
|---|---|
| `(JokeService)obj` | throws **`InvalidCastException`** at runtime |
| `obj as JokeService` | returns **`null`** (reference/nullable types only) |
| `obj is IJokeOutput` | returns `false` |
| `obj is Cat c` | pattern: tests the type **and** declares `c` |
| `obj switch { Cat { Lives: > 5 } c => ..., _ => ... }` | switch expression with property patterns |

- **Upcasting** (Dog → Animal, class → interface) is implicit and always safe.
- **Downcasting** (Animal → Dog) needs an explicit cast and can fail at runtime.

### 1.7 Structs + interfaces = boxing (connects to Quiz 1 Q5)

Assigning a **struct** to an interface variable **boxes a copy**. Changes made through the interface don't affect the original struct, and vice versa. This is the same "you're modifying a copy" trap as `ShiftAge(int age, ...)` in Quiz 1.

### 1.8 Extension methods

```csharp
public static class AnimalExtensions
{
    public static string Shout(this Animal a) => a.Describe().ToUpper();
}
```

- They must be in a **static class** and be **static methods** whose first parameter is marked with `this`.
- **An instance method with the same signature always wins** over an extension method.
- They are a common way to add behavior to an interface without changing it (for example `BaseLoggerExtensions` in Assignment 2).

### 1.9 SOLID

| Letter | Principle | One-liner | CanHazFunny example |
|---|---|---|---|
| **S** | Single Responsibility | A class should have one reason to change | `JokeService` only fetches, `ConsoleJokeOutput` only prints, `Jester` only coordinates |
| **O** | Open/Closed | Open for extension, closed for modification | Add `FileJokeOutput : IJokeOutput` without editing `Jester` |
| **L** | Liskov Substitution | A subtype must be usable anywhere its base type is, without breaking behavior | Any `IJokeService` (real, mock, fake) works in `Jester` |
| **I** | Interface Segregation | Many small, focused interfaces beat one fat one | Two interfaces (`IJokeService`, `IJokeOutput`) instead of one `IJokeEverything` |
| **D** | Dependency Inversion | Depend on **abstractions**, not concretions. High-level code shouldn't `new` up low-level code | `Jester` takes `IJokeService`/`IJokeOutput` in its constructor |

**Classic LSP violation:** `Square : Rectangle`, where setting `Width` also changes `Height`. Code that expects a `Rectangle` breaks.

### 1.10 Dependency injection & guard clauses

```csharp
public Jester(IJokeOutput jokeOutput, IJokeService jokeService)
{
    JokeOutput  = jokeOutput  ?? throw new ArgumentNullException(nameof(jokeOutput));
    JokeService = jokeService ?? throw new ArgumentNullException(nameof(jokeService));
    // Equivalent modern form: ArgumentNullException.ThrowIfNull(jokeOutput);
}
```

- **Constructor injection** means dependencies are passed in rather than created inside the class.
- `??` uses the right side only when the left is `null`. `throw` is allowed as an expression there.
- `nameof(jokeOutput)` produces the string `"jokeOutput"`, which becomes `ex.ParamName`. Tests can assert on it.
- `null!` (null-forgiving operator) silences the nullable **warning** but still passes `null` at runtime.
- **Why do this?** It lets tests swap the real web service for a fake or mock (testability, DIP).

### 1.11 Unit testing with Moq & xUnit

```csharp
Mock<IJokeService> service = new();
service.Setup(s => s.GetJoke()).Returns("A funny joke");
service.SetupSequence(s => s.GetJoke()).Returns("Chuck Norris...").Returns("Good joke");

new Jester(output.Object, service.Object).TellJoke();

output.Verify(o => o.WriteJoke("Good joke"), Times.Once);
service.Verify(s => s.GetJoke(), Times.Exactly(2));
output.Verify(o => o.WriteJoke(It.IsAny<string>()), Times.Once);
```

| Term | Meaning |
|---|---|
| `mock.Object` | The fake instance that implements the interface |
| `Setup(...).Returns(x)` | Always return `x` |
| `SetupSequence(...)` | Return different values on successive calls |
| `Verify(..., Times.X)` | Assert how many times a call happened (`Once`, `Never`, `Exactly(n)`, `AtLeastOnce`) |
| `It.IsAny<T>()` | Matches any argument |
| `[Fact]` | Test with no parameters |
| `[Theory]` + `[InlineData(...)]` | Same test run with several inputs |
| `Assert.Throws<T>(() => ...)` | Passes only if exactly `T` is thrown; returns the exception |
| **Mock vs. stub** | Stub = supplies canned data. Mock = also **verifies interactions** |

Moq can mock **interfaces** easily. It can only mock **virtual/abstract** members of classes, which is another reason to code against interfaces.

**Testing `Console.WriteLine`:** redirect with `Console.SetOut(new StringWriter())`, and restore the original writer in `finally`.

### 1.12 Strings: a CanHazFunny gotcha

`string.Contains(string)` is **ordinal and case-sensitive**. `"CHUCK NORRIS".Contains("Chuck Norris")` is `false`.
Use `Contains("Chuck Norris", StringComparison.OrdinalIgnoreCase)` to ignore case.

---

## Part 2: Practice Questions (Quiz Style)

For each one: **What is the output? If it doesn't compile or it throws, say why.** Answers are in the collapsible sections.

### P1: Explicit implementation

```csharp
IGreeter g = new Greeter();
Greeter c = new Greeter();
Console.WriteLine(g.Greet());
Console.WriteLine(c.Greet());

public interface IGreeter { string Greet(); }
public class Greeter : IGreeter { string IGreeter.Greet() => "Hello"; }
```

<details><summary>Answer</summary>

**Compile error CS1061** on `c.Greet()`. An explicitly implemented member is only reachable through the interface type. `g.Greet()` alone would print `Hello`.
</details>

### P2: `override` vs. `new`

```csharp
Animal a = new Dog();
Dog d = (Dog)a;
Console.WriteLine(a.Speak());
Console.WriteLine(a.Name());
Console.WriteLine(d.Name());

public class Animal { public virtual string Speak() => "..."; public string Name() => "Animal"; }
public class Dog : Animal { public override string Speak() => "Woof"; public new string Name() => "Dog"; }
```

<details><summary>Answer</summary>

```
Woof
Animal
Dog
```
`Speak` is overridden, so it uses the runtime type (Dog). `Name` is hidden with `new`, so it uses the variable type: `a` is `Animal` and `d` is `Dog`.
</details>

### P3: Default interface method

```csharp
ILogger logger = new ConsoleLogger();
logger.Log("Starting");
logger.LogError("Disk full");

public interface ILogger
{
    void Log(string message);
    void LogError(string message) => Log($"ERROR: {message}");
}
public class ConsoleLogger : ILogger
{
    public void Log(string message) => Console.WriteLine(message);
}
```

**Follow-up:** What if the first line were `ConsoleLogger logger = new ConsoleLogger();`?

<details><summary>Answer</summary>

```
Starting
ERROR: Disk full
```
**Follow-up:** **Compile error CS1061** on `logger.LogError`. Default interface members are not inherited by the class, so they can only be called through the interface.
</details>

### P4: Constructor chaining

```csharp
Child child = new("Kevin");
Console.WriteLine();
Console.WriteLine(child.Name);

public class Parent
{
    public string Name { get; }
    public Parent(string name) { Name = name; Console.Write("Parent "); }
}
public class Child : Parent
{
    public Child(string name) : base(name.ToUpper()) { Console.Write("Child"); }
}
```

**Follow-up:** What if `Child`'s constructor were `public Child() { }` with no `: base(...)`?

<details><summary>Answer</summary>

```
Parent Child
KEVIN
```
The base constructor runs first. **Follow-up:** **Compile error CS7036**. `Parent` has no parameterless constructor, so `Child` must call `base(...)` explicitly.
</details>

### P5: Abstract classes

```csharp
Shape s = new Square(3);
Console.WriteLine($"{s.Name}: {s.Area()}");

public abstract class Shape
{
    public abstract double Area();
    public virtual string Name => "Shape";
}
public class Square : Shape
{
    private double Side { get; }
    public Square(double side) => Side = side;
    public override double Area() => Side * Side;
}
```

**Follow-ups:** (a) `Shape s = new Shape();` (b) `public class Circle : Shape { }`

<details><summary>Answer</summary>

`Shape: 9`. `Name` was not overridden, so the base version is used.
(a) **CS0144**: cannot create an instance of an abstract type.
(b) **CS0534**: `Circle` does not implement the inherited abstract member `Area()`.
</details>

### P6: `as`, `is`, and casts

```csharp
object o = new ConsoleJokeOutput();
IJokeOutput? output = o as IJokeOutput;
JokeService? service = o as JokeService;
Console.WriteLine(output is null);
Console.WriteLine(service is null);
Console.WriteLine(o is IJokeOutput);
JokeService forced = (JokeService)o;
Console.WriteLine("Done");
// (IJokeOutput, IJokeService, ConsoleJokeOutput, and JokeService are defined as in CanHazFunny)
```

<details><summary>Answer</summary>

```
False
True
True
```
Then **`InvalidCastException`** is thrown at runtime, so `Done` never prints. `as` returns `null` on failure, while a direct cast throws.
</details>

### P7: The Chuck Norris filter

```csharp
string[] jokes = ["CHUCK NORRIS counted to infinity", "Why do Java devs wear glasses?"];
foreach (string joke in jokes)
{
    Console.WriteLine(joke.Contains("Chuck Norris"));
    Console.WriteLine(joke.Contains("Chuck Norris", StringComparison.OrdinalIgnoreCase));
}
```

<details><summary>Answer</summary>

```
False
True
False
False
```
The default `Contains` is case-sensitive, so the uppercase Chuck Norris joke would slip through the filter.
</details>

### P8: What's illegal in an interface?

```csharp
// (a)
IJokeService service = new IJokeService();

// (b)
public interface IJokeService
{
    private int _count;
    string GetJoke();
}
```

<details><summary>Answer</summary>

(a) **CS0144**: you cannot instantiate an interface.
(b) **CS0525**: interfaces cannot contain instance fields. (Static fields *are* allowed in C# 8+.)
</details>

### P9: Structs through an interface (boxing)

```csharp
Counter counter = new();
ICounter boxed = counter;
boxed.Increment();
counter.Increment();
counter.Increment();
Console.WriteLine($"{counter.Count} {boxed.Count}");

public interface ICounter { int Count { get; } void Increment(); }
public struct Counter : ICounter
{
    public int Count { get; private set; }
    public void Increment() => Count++;
}
```

<details><summary>Answer</summary>

`2 1`. Assigning the struct to `ICounter` boxed a **copy**. From then on the two values are independent. (If `Counter` were a `class`, the output would be `3 3`.)
</details>

### P10: Extension method vs. instance method

```csharp
Animal a = new Animal();
Console.WriteLine(a.Describe());
Console.WriteLine(a.Shout());

public class Animal { public string Describe() => "instance method"; }
public static class AnimalExtensions
{
    public static string Describe(this Animal animal) => "extension method";
    public static string Shout(this Animal animal) => animal.Describe().ToUpper();
}
```

<details><summary>Answer</summary>

```
instance method
INSTANCE METHOD
```
The instance method always wins. The extension `Describe` is never chosen through method-call syntax.
</details>

### P11: Sealed

```csharp
public sealed class JokeService { }
public class FunnierJokeService : JokeService { }
```

<details><summary>Answer</summary>

**CS0509**: you cannot derive from a sealed type. (This is why `JokeResponse` in CanHazFunny is declared `sealed record`. The analyzers like sealed private types.)
</details>

### P12: Two interfaces, same method

```csharp
Robot r = new();
IWalker w = r;
IRunner run = r;
Console.WriteLine(r.Move());
Console.WriteLine(w.Move());
Console.WriteLine(run.Move());

public interface IWalker { string Move(); }
public interface IRunner { string Move(); }
public class Robot : IWalker, IRunner
{
    public string Move() => "Rolling";
    string IRunner.Move() => "Sprinting";
}
```

<details><summary>Answer</summary>

```
Rolling
Rolling
Sprinting
```
The public `Move` implicitly satisfies `IWalker`. `IRunner` uses its explicit implementation.
</details>

### P13: `protected virtual` and `base`

```csharp
Base b = new Derived();
b.Print();

public class Base
{
    public void Print() => Console.WriteLine(Describe());
    protected virtual string Describe() => "Base";
}
public class Derived : Base
{
    protected override string Describe() => "Derived + " + base.Describe();
}
```

<details><summary>Answer</summary>

`Derived + Base`. The non-virtual `Print` in `Base` still calls the **overridden** `Describe`, because virtual dispatch works even from inside the base class.
</details>

### P14: Null guard

```csharp
Jester jester = new(null!, new FakeService());

public class Jester
{
    private IJokeOutput Output { get; }
    private IJokeService Service { get; }
    public Jester(IJokeOutput output, IJokeService service)
    {
        try
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"Caught: {ex.ParamName}");
            throw;
        }
    }
}
```

<details><summary>Answer</summary>

It prints `Caught: output`, then the program crashes with an unhandled `ArgumentNullException (Parameter 'output')`. `null!` only silences the compiler. It still passes `null`. `throw;` rethrows and keeps the original stack trace.
</details>

### P15: Static members & default method overridden by the class

```csharp
IShape shape = new Circle();
Console.WriteLine(shape.Describe());
Console.WriteLine(IShape.Count);

public interface IShape
{
    static int Count { get; set; } = 7;
    string Name { get; }
    string Describe() => $"I am a {Name}";
}
public class Circle : IShape
{
    public string Name => "circle";
    public string Describe() => "Round!";
}
```

<details><summary>Answer</summary>

```
Round!
7
```
The class's own `Describe` replaces the default implementation, even when called through the interface. Static interface members are accessed through the interface name.
</details>

### P16: Pattern matching

```csharp
Animal a = new Cat();
Console.WriteLine(a switch
{
    Cat { Lives: > 5 } c => $"Healthy cat with {c.Lives}",
    Cat => "Cat",
    _ => "Unknown"
});
Console.WriteLine(a.ToString());

public class Animal { }
public class Cat : Animal { public int Lives { get; set; } = 9; public override string ToString() => "Meow"; }
```

<details><summary>Answer</summary>

```
Healthy cat with 9
Meow
```
`ToString` is `virtual` on `System.Object`, so the override is used. Every class implicitly derives from `object`.
</details>

---

## Part 3: Short-Answer / Conceptual Questions

<details><summary>1. Why does <code>Jester</code> take interfaces instead of creating <code>new JokeService()</code> itself?</summary>

Dependency Inversion: high-level code depends on abstractions. This makes `Jester` **testable**, because a mock can replace the real web service, which keeps tests fast, deterministic, and free of network calls. It is also **extensible** (Open/Closed), because a new output can be added without editing `Jester`.
</details>

<details><summary>2. Why are there two interfaces instead of one <code>IJoke</code> interface with both methods?</summary>

Interface Segregation and Single Responsibility: getting a joke and displaying a joke are separate concerns that change for different reasons. Clients shouldn't depend on methods they don't use.
</details>

<details><summary>3. When would you pick an abstract class over an interface?</summary>

Pick an abstract class when the types share real **state or implementation** (fields, constructors, protected helpers) and have a true "is-a" relationship. Pick an interface when you need a capability that unrelated types can share, or when a type needs multiple contracts.
</details>

<details><summary>4. What's the difference between a mock and a stub?</summary>

A stub only returns canned data so the code under test can run. A mock also records calls, so you can **verify** interactions (`Verify(..., Times.Once)`). In Moq the same `Mock<T>` object can act as either one.
</details>

<details><summary>5. Why is <code>JokeService.GetJoke()</code> hard to unit test, and why doesn't it matter much?</summary>

It makes a real HTTP call (non-deterministic, slow, depends on someone else's server). Because everything else depends on `IJokeService`, the logic worth testing (`Jester`) can be tested with a mock. Keep the hard-to-test class as thin as possible.
</details>

<details><summary>6. What does <code>Times.Exactly(2)</code> prove in the Chuck Norris test?</summary>

It proves that the Jester retried: the first (Chuck Norris) joke was rejected and a second one was fetched. Together with `Verify(o => o.WriteJoke(badJoke), Times.Never)`, it shows the bad joke was never printed.
</details>

<details><summary>7. Name the build settings from <code>Directory.Build.props</code> and what each does.</summary>

- `Nullable=enable`: turns on nullable reference type warnings
- `TreatWarningsAsErrors`: compiler warnings fail the build
- `EnableNETAnalyzers`: runs the built-in code-quality analyzers (CAxxxx)
- `CodeAnalysisTreatWarningsAsErrors`: analyzer warnings fail the build
- `EnforceCodeStyleInBuild`: `.editorconfig` style rules (IDExxxx) are enforced when building
- `LangVersion=latest` / `TargetFramework=net10.0`: latest C# and .NET

Putting them in `Directory.Build.props` applies them to **every project** below that folder.
</details>

---

## Part 4: Git/GitHub Refresher (Quiz 1 Q1 Might Come Back)

Fork workflow order: **Fork → Clone (your fork) → Branch → Edit → Commit → Push → Pull Request**

| Concept | Remember |
|---|---|
| `origin` vs `upstream` | `origin` = your fork. `upstream` = the original repo you forked from |
| Sync your fork | `git fetch upstream` then `git merge upstream/main` (or rebase), then `git push` |
| Fork vs. clone | A fork is a server-side copy on GitHub. A clone is a local copy on your machine |
| Why branch? | Keeps `main` clean and gives one PR per feature/fix |
| Commit vs. push | A commit is local history. A push uploads commits to the remote |

---

## Appendix: Quiz 1 Answer Key (Verified)

| Q | Answer | Why |
|---|---|---|
| Q1 | Fork → Clone → Branch → Fix → Commit → Push → PR | No direct access to the repo, so you must fork |
| Q2 | `Hello  (42)` (two spaces) | `Name` is never set, so it's `null`, and `null` interpolates as an empty string |
| Q3 | `Hello Kevin, Age: 42` | `init` is allowed in an object initializer. `nameof(Person.Age)` is `"Age"` |
| Q4 | **Compile error CS8852** | `kevin.Name += " Bost"` assigns an `init`-only property outside an initializer. (Without that line it prints `Hello Kevin (42)`: reassigning the `person` parameter doesn't affect the caller's `kevin`.) |
| Q5 | `You are 45 years old` | `ShiftAge` modifies a **copy** of the `int`, while `AddAge` modifies the property. 40 + 5 = 45 |
