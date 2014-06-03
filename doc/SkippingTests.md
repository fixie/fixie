## How do I skip tests?

Don't skip tests.

*No, really, how do I skip tests?*

The default convention does not support skipped tests. In a custom convention, use the `Skip(...)` method to define what makes a test skipped. `Skip(...)` accepts a delegate of type `Func<Case, bool>`. In other words, for any given test case, your delegate must return true when the test case should be skipped.

You may want to skip based on an attribute, an attribute with an expiration date, a naming convention, or some other rule. Here we define a `[Skip]` attribute and a custom convention which looks for it on test methods:

```cs
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class SkipAttribute : Attribute { }

public class CustomConvention : Convention
{
    public CustomConvention()
    {
        Classes
            .NameEndsWith("Tests");
     
        Methods
            .Where(method => method.IsVoid());
     
        CaseExecution
            .Skip(@case => @case.Method.HasOrInherits<SkipAttribute>());
    }
}
```

Now, any test method marked with `[Skip]` will be skipped by the test runner.

The `Skip(...)` method has an overload which accepts a skip reason provider, a delegate of type `Func<Case, string>`.  When provided, this delegate is called in order to generate a skip reason string to include in the output. For instance, we could modify the above example by including a `Reason` property on the `SkipAttribute` class, and then declare that this property is the source of skip reasons:

```cs
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class SkipAttribute : Attribute
{
    public string Reasons { get; set; }
}

public class CustomConvention : Convention
{
    public CustomConvention()
    {
        Classes
            .NameEndsWith("Tests");
     
        Methods
            .Where(method => method.IsVoid());
     
        CaseExecution
            .Skip(@case => @case.Method.HasOrInherits<SkipAttribute>(),
                  @case => @case.Method.GetCustomAttribute<SkipAttribute>(true).Reason);
    }
}
```