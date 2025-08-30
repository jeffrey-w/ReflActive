# Introduction

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

## Motivation

ReflActive evolved from a desire to standardize how user interfaces for
selecting and tuning operations over data are
rendered. It was conceived as a means for sending information about the classes
performing those operations over the
wire, and using it to automate the construction of HTML forms that could capture
the input necessary to instantiate
those classes. As such, you will notice a correspondence between the properties
exposed by the data structures provided
by ReflActive, and the attributes on HTML form inputs that control how they are
displayed, accept data, and validate it.
However, those same properties ought to be adaptable to any UI toolkit in
principle.
