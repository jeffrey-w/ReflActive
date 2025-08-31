# <img src="logo.svg" alt="Logo" width=50 height=50 /> ReflActive

ReflActive is a .NET library for introspecting on CLR types, and providing
language-agnostic descriptions of their
constructors. Clients may render this information so that user input may be used
to instantiate those types at runtime
in a transparent way.

Targeted types, their constructors, and parameters thereof exhibit attributes,
which are reflected upon at runtime to
automatically build representations of the type that may be used to create
richly detailed user interfaces. In addition
to providing descriptions of the data necessary to instantiate a type, other
metadata may be associated with it as well
to provide a more complete context for its use.

ReflActive also provides an interface for translating user input into
constructor invocations. Having provided
appropriate values for each parameter to a constructor, strongly-typed instances
of the targeted class may be obtained
using a standard, serializable representation of the specified arguments.

## Installation

ReflActive can be installed using
[NuGet](https://nuget.org/packages/ReflActive/).

## Usage

You can find a quick tutorial and API documentation for
ReflActive [here](https://jeffrey-w.github.io/ReflActive/).

## Contributing

ReflActive is privately maintained. Please open an issue to request a feature or
report a bug. If you would like to
contribute, you may request an invitation to collaborate on this repository.

## License

[MIT](LICENSE.md)