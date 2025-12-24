namespace ReflActive.Tests;

public class ActivationTargetMetadataTests
{
    private static readonly IEnumerable<string> ExpectedValues = ["X", "Y", "Z"];

    [Test]
    public void MetadataExhibitsExpectedProperties()
    {
        var metadata =
            ActivationTargetMetadata.Singleton<TestActivationTarget>(ActivationContext.Init(development: true));
        Assert.Multiple(() =>
        {
            Assert.That(metadata.Name, Is.EqualTo("Test"));
            Assert.That(metadata.Discriminator, Is.Empty);
            Assert.That(metadata.Description, Is.EqualTo("Test"));
            Assert.That(metadata.IsParameterized, Is.True);
            Assert.That(metadata.IsDevelopment, Is.True);
            Assert.That(metadata.IsExperimental, Is.False);
            Assert.That(metadata.IsComposite, Is.False);
            Assert.That(metadata.Properties, Has.Count.EqualTo(2));
            Assert.That(metadata.Properties, Has.ItemAt("One").EqualTo(1));
            Assert.That(metadata.Properties, Has.ItemAt("Two").EqualTo(2));
            Assert.That(metadata.Toggles.ToList(), Has.Count.EqualTo(1));
            Assert.That(metadata.Toggles, Has.All.Property("Name").EqualTo("B"));
            Assert.That(metadata.Toggles, Has.All.Property("Description").EqualTo("Toggle"));
            Assert.That(metadata.Toggles, Has.All.Property("Default").True);
            Assert.That(metadata.Toggles, Has.All.Property("IsRequired").False);
            Assert.That(metadata.Counts.ToList(), Has.Count.EqualTo(1));
            Assert.That(metadata.Counts, Has.All.Property("Name").EqualTo("I"));
            Assert.That(metadata.Counts, Has.All.Property("Description").EqualTo("Count"));
            Assert.That(metadata.Counts, Has.All.Property("Default").EqualTo(TestActivationTarget.DefaultCount));
            Assert.That(metadata.Counts, Has.All.Property("Step").EqualTo(2));
            Assert.That(metadata.Counts, Has.All.Property("IsRequired").True);
            Assert.That(metadata.Quantities.ToList(), Has.Count.EqualTo(1));
            Assert.That(metadata.Quantities, Has.All.Property("Name").EqualTo("D"));
            Assert.That(metadata.Quantities, Has.All.Property("Description").EqualTo("Quantity"));
            Assert.That(metadata.Quantities, Has.All.Property("Default").Null);
            Assert.That(metadata.Quantities, Has.All.Property("Min").EqualTo(double.MinValue));
            Assert.That(metadata.Quantities, Has.All.Property("Max").EqualTo(TestActivationTarget.QuantityMax));
            Assert.That(metadata.Quantities, Has.All.Property("Precision").EqualTo(3));
            Assert.That(metadata.Quantities, Has.All.Property("IsRequired").True);
            Assert.That(metadata.Labels.ToList(), Has.Count.EqualTo(1));
            Assert.That(metadata.Labels, Has.All.Property("Name").EqualTo("S"));
            Assert.That(metadata.Labels, Has.All.Property("Description").EqualTo("Label"));
            Assert.That(metadata.Labels, Has.All.Property("Default").Null);
            Assert.That(metadata.Labels, Has.All.Property("Min").EqualTo(1));
            Assert.That(metadata.Labels, Has.All.Property("Max").EqualTo(10));
            Assert.That(metadata.Labels, Has.All.Property("Pattern").EqualTo(@"[A-Za-z_]\w"));
            Assert.That(metadata.Labels, Has.All.Property("IsRequired").True);
            Assert.That(metadata.SingleSelections.ToList(), Has.Count.EqualTo(1));
            Assert.That(metadata.SingleSelections, Has.All.Property("Name").EqualTo("E"));
            Assert.That(metadata.SingleSelections, Has.All.Property("Description").EqualTo("Single Selection"));
            Assert.That(metadata.SingleSelections, Has.All.Property("Default").EqualTo("X"));
            Assert.That(metadata.SingleSelections, Has.All.Property("Values").EquivalentTo(ExpectedValues));
            Assert.That(metadata.SingleSelections, Has.All.Property("IsRequired").True);
            Assert.That(metadata.CompositeSelections.ToList(), Has.Count.EqualTo(1));
            Assert.That(metadata.CompositeSelections, Has.All.Property("Name").EqualTo("Es"));
            Assert.That(metadata.CompositeSelections, Has.All.Property("Description").EqualTo("Composite Selection"));
            Assert.That(metadata.CompositeSelections, Has.All.Property("Default").EquivalentTo(ExpectedValues.Except(["X"])));
            Assert.That(metadata.CompositeSelections, Has.All.Property("Values").EquivalentTo(ExpectedValues));
            Assert.That(metadata.CompositeSelections, Has.All.Property("IsRequired").True);
        });
    }

    [Test]
    public void SingletonThrowsWhenSpecifiedTypeIsInvalidInSpecifiedActivationContext()
    {
        Assert.Throws<ArgumentException>(
            () => ActivationTargetMetadata.Singleton<TestActivationTarget>(ActivationContext.Init()));
    }

    [Test]
    public void FallbackValueIsUsedWhenDynamicDefaultNameIsUndefinedInSpecifiedActivationContext()
    {
        var metadata =
            ActivationTargetMetadata.Singleton<UndefinedDynamicDefaultWithFallbackValueActivationTarget>(ActivationContext.Init());
        Assert.Multiple(() =>
        {
            Assert.That(metadata.Counts.ToList(), Has.Count.EqualTo(1));
            Assert.That(metadata.Counts, Has.All.Property("Name").EqualTo("I"));
            Assert.That(metadata.Counts, Has.All.Property("Default").EqualTo(int.MaxValue));
        });
    }

    [Test]
    public void QueryingDefaultValueForParameterWithUndefinedDynamicDefaultThrowsWhenNoFallbackValueIsProvided()
    {
        var metadata =
            ActivationTargetMetadata.Singleton<UndefinedDynamicDefaultWithoutFallbackValueActivationTarget>(
                ActivationContext.Init());
        Assert.Throws<KeyNotFoundException>(() =>
        {
            _ = metadata.Counts
                        .Single()
                        .Default;
        });
    }
}
