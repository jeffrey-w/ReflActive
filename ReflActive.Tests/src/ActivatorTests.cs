namespace ReflActive.Tests;

public class ActivatorTests
{
    private static readonly IActivationContext Context = ActivationContext.Init(development: true);
    
    private static readonly List<string> ExpectedIds = ["X", "Z"];

    private static readonly Activation ValidActivation = new()
    {
        Name = "Test",
        Arguments =
        [
            new BooleanArgument { Name = "B", Value = false },
            new IntegerArgument { Name = "I", Value = int.MaxValue },
            new DoubleArgument { Name = "D", Value = double.MaxValue },
            new StringArgument { Name = "S", Value = string.Empty },
            new StringArgument { Name = "E", Value = "Y" },
            new StringsArgument { Name = "Es", Value = ExpectedIds },
        ],
    };

    [Test]
    public void ActivateProvidesExpectedValueWhenValidActivationIsSpecified()
    {
        var target = Activator.Activate<TestActivationTarget>(ValidActivation, Context);
        Assert.Multiple(() =>
        {
            Assert.That(target.B, Is.False);
            Assert.That(target.I, Is.EqualTo(int.MaxValue));
            Assert.That(target.D, Is.EqualTo(double.MaxValue));
            Assert.That(target.S, Is.Empty);
            Assert.That(target.E.Id, Is.EqualTo("Y"));
            Assert.That(target.Es.Select(e => e.Id), Is.EquivalentTo(ExpectedIds));
        });
    }

    [Test]
    public void ActivateThrowsWhenActivationTargetDeclaresDuplicateParameters()
    {
        Assert.Throws<ArgumentException>(() => Activator.Activate<DuplicateParametersActivationTarget>(
                                             new Activation
                                             {
                                                 Name = "Duplicate Parameters",
                                                 Arguments = [new BooleanArgument { Name = "B", Value = false }],
                                             },
                                             Context));
    }

    [Test]
    public void ActivateThrowsWhenActivationTargetsParameterWithIncorrectArgumentType()
    {
        Assert.Throws<ArgumentException>(() => Activator.Activate<TestActivationTarget>(
                                             new Activation
                                             {
                                                 Name = "Test",
                                                 Arguments =
                                                 [
                                                     new BooleanArgument { Name = "B", Value = false },
                                                     new IntegerArgument { Name = "I", Value = int.MaxValue },
                                                     new DoubleArgument { Name = "D", Value = double.MaxValue },
                                                     new StringArgument { Name = "S", Value = string.Empty },
                                                     new StringArgument { Name = "E", Value = "Y" },
                                                     new StringArgument { Name = "Es", Value = string.Empty },
                                                 ],
                                             },
                                             Context));
    }

    [Test]
    public void ActivateThrowsWhenSpecifiedTypeAndActivationNameAreIncompatible()
    {
        Assert.Throws<InvalidOperationException>(() => Activator.Activate<OtherTestActivationTarget>(
                                                     ValidActivation,
                                                     Context));
    }

    [Test]
    public void ActivateThrowsWhenTargetHasNonDistinctName()
    {
        Assert.Throws<InvalidOperationException>(() => Activator.Activate<IDuplicateNameActivationTarget>(
                                                     new Activation { Name = "Duplicate Name", Arguments = [] },
                                                     Context));
    }

    [Test]
    public void ActivateThrowsWhenTargetDeclaresNoConstructor()
    {
        Assert.Throws<InvalidOperationException>(() => Activator.Activate<NoConstructorActivationTarget>(
                                                     new Activation { Name = "No Constructor", Arguments = [] },
                                                     Context));
    }

    [Test]
    public void ActivateThrowsWhenTargetDeclaresMoreThanOneConstructor()
    {
        Assert.Throws<InvalidOperationException>(() => Activator.Activate<MultipleConstructors>(
                                                     new Activation { Name = "Multiple Constructors", Arguments = []},
                                                     Context));
    }

    [Test]
    public void ActivateThrowsWhenActivationTargetConstructorDeclaresParameterWithoutParameterOfDependencyAttribute()
    {
        Assert.Throws<InvalidOperationException>(() => Activator.Activate<MissingAttributeActivationTarget>(
                                                     new Activation { Name = "Missing Attribute", Arguments = [] },
                                                     Context));
    }

    [Test]
    public void ActivateThrowsWhenConstructorDeclaresEntityConverterIsNotParameterizedByActivationContext()
    {
        Assert.Throws<InvalidOperationException>(() => Activator.Activate<InvalidConverterActivationTarget>(
                                                     new Activation
                                                     {
                                                         Name = "Invalid Converter",
                                                         Arguments =
                                                         [
                                                             new StringArgument { Name = "E", Value = "Y" }
                                                         ],
                                                     },
                                                     Context));
    }
}
