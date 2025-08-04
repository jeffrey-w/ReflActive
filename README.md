# <p align="center"><img src="./icon.png" alt="ReflActive logo" width="256px" height="256px" /></p>

ReflActive is a .NET library for introspecting on CLR types, and providing language-agnostic descriptions of their
constructors. Clients may render this information so that user input may be used to instantiate those types at runtime
in a transparent way.

Targeted types, their constructors, and parameters thereof exhibit attributes, which are reflected upon at runtime to
automatically build representations of the type that may be used to create richly detailed user interfaces. In addition
to providing descriptions of the data necessary to instantiate a type, other metadata may be associated with it as well
to provide a more complete context for its use.

ReflActive also provides an interface for translating user input into constructor invocations. Having provided
appropriate values for each parameter to a constructor, strongly-typed instances of the targeted class may be obtained
using a standard, serializable representation of the specified arguments.

## Installation

ReflActive can be installed using NuGet.

## Usage

Suppose we have a type that performs some important, parameterized computation, and we would like to expose those
parameters to our clients.

```csharp
public class ImportantComputation
{
    private readonly int _param1;
    private readonly string _param2;
    private readonly Entity _param3;
    private readonly Config _config;
    
    public ImportantComputation(int param1, string param2, Entity param3, Config config)
    {
        _param1 = param1;
        _param2 = param2;
        _param3 = param3;
        _config = config;
    }

    public void Execute()
    {
        Console.WriteLine(
            $"Executing with: param1={_param1}, param2={_param2}, param3={_param3.Id}, config={_config.Name}.");
    }
}
```

Providing users with fields to specify values for `param1` and `param2` to the constructor declared by
`ImportantComputation` is not especially difficult. Fields for `param3` and `config` might be slightly more complicated,
but certainly can be implemented. However, the effort becomes more tedious as parameters are added or modified, and as
the number of types to expose are added. Furthermore, it would be ideal for there to be a more standard way to marshall
input requirements to and from the process in which the type will be instantiated. The listing below replicates the
definition above, but this time using the declarative constructs provided by ReflActive to enable information pertinent
to the instantiation of the class to be collected and compiled in a way that can be acted upon by clients and their
users.

```csharp
[ActivationTarget(
    Name = "Important Computation",
    Description = "An example that demonstrates the ReflActive library.")]
public class ImportantComputation
{
    private readonly int _param1;
    private readonly string _param2;
    private readonly Entity _param3;
    private readonly Config _config;

    [ActivationTargetConstructor]
    public ImportantComputation(
        [Parameter(Name = "Param One")]
        [StaticDefault(Value = 123)]
        [DiscreteNumber(0, int.MaxValue)]
        int param1,
        [Parameter(Name = "Param Two")]
        [DynamicDefault(Name = "Param Two Default")]
        [Text(Min = 1, Max = 1024)]
        string param2,
        [Parameter(Name = "Param Three")]
        Entity param3,
        Config config)
    {
        _param1 = param1;
        _param2 = param2;
        _param3 = param3;
        _config = config;
    }
    
    // ...
}
```

There's quite a lot going on here, so let's go through this example thoroughly. First, notice the attribute on the class
itself: `ActivationTargetAttribute`. This identifies the class as one that may be instantiated using the facilities
provided by ReflActive. It also allows helpful information to be associated with the class, which may be rendered to
user interfaces as necessary.

Next, consider the attribute on the constructor for `ImportantComputation`. The `ActivationTargetConstructorAttribute`
identifies the constructor that ReflActive will call when attempting to instantiate instances of the targeted class. It
is important that only one constructor for a class exhibit this attribute.

Finally, there are the attributes on the constructor parameters. Every parameter that is to be exposed to clients must
exhibit the `ParameterAttribute`. Similar to the `ActivationTargetAttribute`, it allows you to provide useful
information to users that are specifying input to the parameter. Parameters may optionally be associated with a default
value. The `StaticDefaultAttribute` lets you provide a typical constant value to associate with a parameter. The
`DynamicDefaultAttribute` lets you specify the name of a variable, declared in the [activation context](#activation-contexts) in which a
class is being instantiated, that holds the typical value to associate with a parameter. In addition, depending on the
type to which a parameter belongs, it may exhibit one of many  attributes that limit its domain. For example the
`DiscreteNumber` attribute allows you to specify the least and greatest values that an argument to an integer-valued
parameter may take. The `TextAttribute` allows you to specify the least and greatest number of characters that an
argument to a string-valued parameter may exhibit, and also an optional regular expression for further limiting what
constitutes a valid argument to the parameter. The following attributes are available for restricting the domains of
parameters.

- `DiscreteNumberAttribute`
- `ContinuousNumberAttribute`
- `TextAttribute`
- `SingletonEntityAttribute`
- `CompositeEntityAttribute`
- `DependencyAttribute`

### Singleton and Composite Entities

Consider the parameter `param3` to the constructor for `ImportantComputation`, which belongs to the type `Entity`. How
exactly can we encode information about the domain of this parameter? If each member of the type is uniquely identified
by a value that can be represented as a string, we can simply provide the subset of the string representations of those
identifiers that correspond to the entities that we wish to allow users to pass as arguments to the parameter. This is
what the `SingletonEntityAttribute` and `CompositeEntityAttribute` classes are for. For each parameter that exhibits one
of those attributes, an associated implementation of the `ISingletonEntityCoverter` or `ICompositeEntityConverter` must
be defined respectively. Then, the implementation is specified at the declaration of the attribute, which allows the
subset of allowed entities and optionally those that are typically selected to be associated with the parameter. Let's
illustrate using our previous example. Assume that `Entity` is defined as follows.

```csharp
public class Entity
{
    public required string Id { get; init; }
}
```

Let's further assume that the valid values for `Entity.Id` are `"X"`, `"Y"`, and `"Z"`. Finally, we will let the
`Entity` identified by `"X"` be our default value. We may now define our entity converter.

```csharp
public class SingletonEntityConverter(IActivationContext _) : ISingletonEntityConverter
{
    private readonly Dictionary<string, Entity> _entities = new()
    {
        { "X", new Entity { Id = "X" } }, { "Y", new Entity { Id = "Y" } }, { "Z", new Entity { Id = "Z" } },
    };

    public IEnumerable<string> Ids => _entities.Keys;
    public string Default => "X";

    public object Entity(string id)
    {
        return _entities[id];
    }
}
```

Notice the (primary) [constructor parameter](#activation-contexts). This is required, even if it is not used by an
entity converter.

At our constructor declaration, we add the `SingletonEntityAttribute` to our parameter.

```csharp
[ActivationTargetConstructor]
public ImportantComputation(
    [Parameter(Name = "Param One")]
    [StaticDefault(Value = 123)]
    [DiscreteNumber(0, int.MaxValue)]
    int param1,
    [Parameter(Name = "Param Two")]
    [DynamicDefault(Name = "Param Two Default")]
    [Text(Min = 1, Max = 1024)]
    string param2,
    [Parameter(Name = "Param Three")]
    [SingletonEntity(Type = typeof(SingletonEntityConverter))]
    Entity param3,
    Config config)
{
    // ...
}
```

Now, valid identifiers and default bindings for `param3` will be automatically inferred by ReflActive, and may be
presented to users. The `CompositeEntityAttribute` works in a similar manner except that it is meant for set-valued
parameters.

### Dependencies

What about parameters that take values from complex types, which do not declare a property by which instances are
uniquely identified? The parameter `config` to the constructor for `ImportantComputation` belongs to the type `Config`,
which represents user-supplied configuration values. In this case, the `Config` class is not likely to exhibit a unique
identifier (it's simply a bag of values that define a computation environment). Let's suppose it's defined as such.

```csharp
public class Config
{
    public string Name => "Value";
}
```

Additionally, the value to bind to `config` is probably a singleton: the unique member of `Config` that specifies the
preferences of the user on whose behalf the process in which `ImportantComputation` is being instantiated was initiated.
For now, assume that this value has already been instantiated, and that ReflActive is able to access the instance when
it needs to. To indicate that the instance ought to be bound to the parameter, we use the `DependencyAttribute`.

```csharp
[ActivationTargetConstructor]
public ImportantComputation(
    [Parameter(Name = "Param One")]
    [StaticDefault(Value = 123)]
    [DiscreteNumber(0, int.MaxValue)]
    int param1,
    [Parameter(Name = "Param Two")]
    [DynamicDefault(Name = "Param Two Default")]
    [Text(Min = 1, Max = 1024)]
    string param2,
    [Parameter(Name = "Param Three")]
    [SingletonEntity(Type = typeof(SingletonEntityConverter))]
    Entity param3,
    [Dependency]
    Config config)
{
    // ...
}
```

That is all that is required to ensure the appropriate `Config` value is passed as an argument to the constructor.
Notice that `config` does not exhibit the `ParameterAttribute`. That's because we would not want to expose information
about this parameter to the user: there is only one choice of value anyway, and it has already been defined by the time
the user is specifying input to the constructor for `ImportantComputation`.

Another use for the `[DependencyAttribute]` is to mark collections that are typically obtained from databases. These are
in effect singleton instances of the underlying relation that prevailed at the time queries to the database were
executed. The collections may indeed contain objects that admit identity semantics, but the collection itself does not,
and so is better cast as a dependency.

Up until now, we have avoided explaining how ReflActive accesses values that are not defined at compile time. To achieve
this, we rely on the `IActivationContext` interface.

### Activation Contexts

The `IActivationContext` provides a facility for defining complex values at runtime that may be bound to parameters
declared at compile time. You may think of it as a mechanism similar to what operating system environment variables
achieve. Broadly speaking, an `IActivationContext` maintains references to two types of values: variables and
dependencies. The difference between these storage classes is only the way by which they are identified. Variables are
associated with a name, represented by a string, that must be unique to an `IActivationContext`. Dependencies, on the
other hand, are identified by any of the types to which they are assignable. Naturally, the subset of types to which a
dependency is assignable must be pairwise disjoint with the set associated with every other dependency (excluding
`object`, of course, which belongs to every set and may not be used to identify a dependency). As shown below, the
`IActivationContext` interface provides a means for defining and accessing variables and dependencies, and for updating
variables that are not declared constant.

```csharp
var context = ActivationContext.Init();

context.DefineVariable("Param Two Default", "test");
context.AddDependency(new Config());

var defaultValue = context.Get<string>("Param Two Default"); // defaultValue == "test"
var config = context.GetDependency<Config>(); // config.Value == "Value"

context.Set("Param Two Default", "TEST")
```

You may also define variables by declaring the `ActivationConfigurationAttribute` on public static classes, or the
`ActivationVariableAttribute` on public static fields of a class.

The names and types that identify variables and dependencies may be referenced by the attributes declared on constructor
parameters, and a lookup is performed at runtime to resolve the desired value. For example, recall the attribute
declaration `[DynamicDefault("Param Two Default")]` on the parameter `param2` to the constructor for
`ImportantComputation`. This will associate the value `"TEST"` by default to `param2`. Similarly, the `[Dependency]`
attribute declared on the parameter `config` will cause it to be bound to the dependency in the current
`IActivationContext` that belongs to the `Config` type.

### Metadata

We have now declared the appropriate attributes on our class and its constructor, and initialized an
`IActivationContext`. Now what? The `IActivationTargetMetadata` interface provides a descriptive summary of the
information compiled by ReflActive for a given type. To obtain an `IActivationTargetMetadata` for
`ImportantComputation`, we may use the `Singleton` factory method, as shown below.

```csharp
var metadata = ActivationTargetMetadata.Singleton<ImportantComputation>(context);
```

That's it! Other factory methods exist for different use cases, including the following.

- `Composite` &mdash; groups `IActivationTargetMetadata` instances that are logically associated by a common root name
- `For` &mdash; provides a collection of `IActivationTargetMetadata` for a specified type hierarchy

The `IActivationTargetMetadata` provides the following important properties (among others).

- `Name` &mdash; the identifier assigned to the targeted class (N.B., this is the value bound to the `Name` property of
  the `ActivationTargetAttribute` declared on the targeted class, not the name of the class itself)
- `Description` &mdash; a natural language characterization of the targeted class
- `Toggles` &mdash; the Boolean-valued parameters to the constructor for the targeted class
- `Counts` &mdash; the integer-valued parameters to the constructor for the targeted class
- `Quantities` &mdash; the real-valued parameters to the constructor for the targeted class
- `Labels` &mdash; the string-valued parameters to the constructor for the targeted class
- `SingleSelections` &mdash; the parameters to the constructor for the targeted class that may be bound to a single value
  from a finite subset of the strings that identify a complex type
- `CompositeSelections` &mdash; the parameters to the constructor for the targeted class that may be bound to many values
  from a finite subset of the strings that identify a complex type

Other properties are discussed below.

__Development and Experimental Metadata__

It may be the case that you would only like to expose the existence of some types (and a means for instantiating them)
to users that are building a system. Orthogonally, you may like to indicate that an exposed class may be used by
everyone, but caution them to be more vigilant about the possibility for errors. That is what the `IsDevelopment` and
`IsExperimental` properties are for respectively. Depending on the configuration of the similarly named properties of an
instance of the `IActivationContext` interface, the `ActivationTargetMetadata.Singleton` factory  method will throw an
exception if you attempt to instantiate unsupported metadata, and the `ActivationTargetMetadata.For` factory method will
filter unsupported metadata from the results it provides. For example, suppose our `ImportantComputation` class was
declared `IsDevelopment`. Then, since our activation `context` is not for development, our previous call to `Singleton`
would fail. Note that the converse is not necessarily true. If `context` were `IsDevelopment`, then
`ImportantComputation` could be instantiated in it regardless of whether it was declared for development only or not.

__Additional Information__

Suppose that you would like to associate information with a type that does not nicely correspond to one of the metadata
properties that we've already discussed. How might you convey that information? You can do so by declaring the
`PropertiesAttribute` on a targeted class.

```csharp
[ActivationTarget(
    Name = "Important Computation",
    Description = "An example that demonstrates the ReflActive library.")]
[Properties(typeof(ImportantComputationProperties))]
public class ImportantComputation
{
    // ...
}
```

This allows for arbitrary name-value pairs of strings to be associated with the class. The pairs are derived from the
public properties of the specified type, which must be public and either static or declare a constructor that takes any
number of string parameters. If the constructor does declare parameters, they may be specified at the attribute
declaration, in the order defined by the constructor.

Here is the definition of `ImportantComputationProperties`.

```csharp
public static class ImportantComputationProperties
{
    public static string Name => "Value";
}
```

Now, the properties defined on the `ImportantComputationProperties` class will be available through the `Properties`
attribute of the `IActivationTargetMetadata` instance instantiated for it. It ought to be noted that properties that do
not belong to `string` will be converted to that type by calling `ToString` on the value bound to them.

Let's inspect what our metadata instance looks like, serializing it using `System.Text.Json`.

```json
{
  "name": "Important Computation",
  "discriminator": "",
  "description": "An example that demonstrates the ReflActive library.",
  "isDevelopment": false,
  "isExperimental": false,
  "isComposite": false,
  "isParameterized": true,
  "properties": {
    "name": "Value"
  },
  "toggles": [],
  "counts": [
    {
      "step": 1,
      "min": 0,
      "max": 2147483647,
      "name": "Param One",
      "description": "",
      "isRequired": true,
      "default": 123
    }
  ],
  "quantities": [],
  "labels": [
    {
      "min": 1,
      "max": 1024,
      "pattern": null,
      "name": "Param Two",
      "description": "",
      "isRequired": true,
      "default": "TEST"
    }
  ],
  "singleSelections": [
    {
      "values": [
        "X",
        "Y",
        "Z"
      ],
      "name": "Param Three",
      "description": "",
      "isRequired": true,
      "default": "X"
    }
  ],
  "compositeSelections": []
}
```

__Composite Metadata__

Sometimes, you may want to indicate that a collection of types are logically similar. Perhaps they each represent
different modes of operation for a single category of computation. It would be awkward to have to differentiate them
using only the `Name` property of the `IActivationTargetMetadata` interface. That is why the interface also declares a
`Discriminator` property, with a corresponding property declared on the `AnalysisTargetAttribute`. When types belong to
the same group, they share the same `Name`, but exhibit a different `Discriminator`. To capture this association, you
may create an instance of the `ICompositeActivationMetadata` interface, which implements the `IActivationTargetMetadata`
interface, and also declares a `Children` property that contains associated metadata instances. You may use the
`ActivationTargetMetadata.Composite` factory method to explicitly specify the `Children` of an
`ICompositeActivationTargetMetadata` instance, or rely on `ActivationTargetMetadata.For` to infer groupings
automatically within a hierarchy of types. The `Properties` of an `ICompositeActivationTargetMetadata` are computed as
the intersection of properties incident on its `Children`. Those properties available on the composite are no longer
available on its `Children`.

### Activations

Let's say that we've gone through everything above, have rendered a stunning UI to capture the necessary inputs to
instantiate our `ImportantComputation`, and have received input from our user. What's next? Now, we must build an
`Activation` from that input and pass it to the `Activator.Activate` method to obtain an instance of our
`ImportantComputation`. Note, that `Activation` instances are converted to and from JSON easily using
`System.Text.Json`.

```csharp
var activation = new Activation
{
    Name = "Important Computation",
    Arguments =
    [
        new IntegerArgument { Name = "Param One", Value = 42 },
        new StringArgument { Name = "Param Two", Value = "test" },
        new StringArgument { Name = "Param Three", Value = "Z" },
    ],
};

var computation = Activator.Activate<ImportantComputation>(activation, context);
```

That's all there is to it. We have successfully encapsulated application functionality into a class, rendered
information about the input required from users to invoke that functionality, and have transformed the input provided
into an object that can execute the functionality.

Let's make sure everything is working as expected.

```csharp
computation.Execute();
```

Finally, observe the following output at the terminal.

```
Executing with: param1=42, param2=test, param3=Z, config=Value.
```

## Contributing

ReflActive is privately maintained. Please open an issue to request a feature or report a bug. If you would like to
contribute, you may request an invitation to collaborate on this repository.

## License

[MIT](LICENSE.md)