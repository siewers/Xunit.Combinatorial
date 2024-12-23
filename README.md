# Xunit.Combinatorial

This project allows for parameterizing your Xunit test methods such that
they run multiple times, once for each combination of possible arguments
for your test method. You can also limit the number of test cases by using
a pairwise strategy, which generally provides good coverage for testing
but significantly reduces the test case explosion you might have when
you have more than two parameters.

[![NuGet package](https://img.shields.io/nuget/v/xunit.combinatorial.svg)][NuPkg]
[![🏭 Build](https://github.com/AArnott/Xunit.Combinatorial/actions/workflows/build.yml/badge.svg)](https://github.com/AArnott/Xunit.Combinatorial/actions/workflows/build.yml)

## Installation

This project is available as a [NuGet package][NuPkg].

The v1.x versions of this package support xunit 2.
The v2.x versions of this package support xunit 3.

## Examples

### Auto-generated values

Suppose you have this test method:

```cs
[Fact]
public void CheckFileSystem(bool recursive)
{
    // verifications here
}
```

To arrange for your test method to be invoked twice, once for each value
of its bool parameter, change the attributes to:

```cs
[Theory, CombinatorialData]
public void CheckFileSystem(bool recursive)
{
    // verifications here
}
```

The `CombinatorialDataAttribute` will supply Xunit with both `true` and `false`
arguments to run the test method with, resulting in two invocations of your
test method with individual results reported for each invocation.

### Custom-supplied values

To supply your own values to pass in for each parameter, use the
`CombinatorialValuesAttribute`:

```cs
[Theory, CombinatorialData]
public void CheckValidAge([CombinatorialValues(5, 18, 21, 25)] int age)
{
    // verifications here
}
```

This will run your test method four times with each of the prescribed values.

### Combinatorial effects

Of course it wouldn't be combinatorial without multiple parameters:

```cs
[Theory, CombinatorialData]
public void CheckValidAge(
    [CombinatorialValues(5, 18, 21, 25)] int age,
    bool friendlyOfficer)
{
    // This will run with all combinations:
    // 5  true
    // 18 true
    // 21 true
    // 25 true
    // 5  false
    // 18 false
    // 21 false
    // 25 false
}
```

Once you have more that two parameters, the number of test cases can grow
dramatically in order to cover every possible combination.
Consider this test with 3 parameters, each taking just two values:

```cs
[Theory, CombinatorialData]
public void CheckValidAge(bool p1, bool p2, bool p3)
{
    // Combinatorial generates these 8 test cases:
    // false false false
    // false false true
    // false true  false
    // false true  true
    // true  false false
    // true  false true
    // true  true  false
    // true  true  true
}
```

We already have 8 test cases. With more parameters or more values per parameter
the test cases can quickly grow to a very large number.
This can cause your test runs to take too long. This level of
exhaustive testing is often not necessary as many bugs will show up whenever
two parameters are specific values. This is called "pairwise testing" and
it generates far fewer test cases than combinatorial testing because
it only ensures there is a test case covering every combination of two
parameters, and thus can "compress" the test cases by making each test case
significantly test more than one pair.

To use pairwise testing, use the `PairwiseDataAttribute` instead of the
`CombinatorialDataAttribute`:

```cs
[Theory, PairwiseData]
public void CheckValidAge(bool p1, bool p2, bool p3)
{
    // Pairwise generates these 4 test cases:
    // false false false
    // false true  true
    // true  false true
    // true  true  false
}
```

We have cut the number of test cases in half by using pairwise instead of
combinatorial. In many cases the test reduction can be much greater.
Notice that although the test cases are fewer, you can still find a test
case that covers any *two* parameter values (thus *pair*wise).

### Values over a range

To run a test with a parameter over a range of values, we have
`CombinatorialRangeAttribute` to generate tests over intervals of integers.

```cs
[Theory, CombinatorialData]
public void CombinatorialCustomRange(
    [CombinatorialRange(0, 5)] int p1,
    [CombinatorialRange(0, 3, 2)] int p2)
{
    // Combinatorial generates these test cases:
    // 0 0
    // 1 0
    // 2 0
    // 3 0
    // 4 0
    // 0 2
    // 1 2
    // 2 2
    // 3 2
    // 4 2
}
```

`CombinatorialRangeAttribute` has two distinct constructors.
When supplied with two integers `from` and `count`, Xunit
will create a test case where the parameter equals `from`, and
it will increment the parameter by 1 for `count` number of cases.

In the second constructor, `CombinatorialRangeAttribute`
accepts three integer parameters. In the generated cases, the
parameter value will step up from the first integer to the
second integer, and the third integer specifies the interval of
which to increment.

### Value generated by a member

The `CombinatorialMemberDataAttribute` may be used to generate values for an individual Theory parameter
using a static member on the test class. The static member may be a field, property or method.

A value-generating method is used here:

```cs
public static IEnumerable<int> GetRange(int start, int count)
{
    return Enumerable.Range(start, count);
}

[Theory, CombinatorialData]
public void CombinatorialMemberDataFromParameterizedMethods(
    [CombinatorialMemberData(nameof(GetRange), 0, 5)] int p1)
{
    Assert.True(true);
}
```

A value-generating property is used here:

```cs
public static IEnumerable<int> IntPropertyValues => GetIntMethodValues();

public static IEnumerable<int> GetIntMethodValues()
{
    for (int i = 0; i < 5; i++)
    {
        yield return Random.Next();
    }
}

[Theory, CombinatorialData]
public void CombinatorialMemberDataFromProperties(
    [CombinatorialMemberData(nameof(IntPropertyValues))] int p1)
{
    Assert.True(true);
}
```

A value-generating field also works:

```cs
public static readonly IEnumerable<int> IntFieldValues = Enumerable.Range(0, 5).Select(_ => Random.Next());

[Theory, CombinatorialData]
public void CombinatorialMemberDataFromFields(
    [CombinatorialMemberData(nameof(IntFieldValues))] int p2)
{
    Assert.True(true);
}
```

### Randomly generated values

The `CombinatorialRandomDataAttribute` can be applied to theory parameters to generate random integer values.
The min, max, and number of values can all be set via named parameters.

```cs
[Theory, CombinatorialData]
public void CombinatorialRandomValuesCount(
    [CombinatorialRandomData(Count = 10)] int p1)
{
    Assert.InRange(p1, 0, int.MaxValue);
}

[Theory, CombinatorialData]
public void CombinatorialRandomValuesCountMinMaxValues(
    [CombinatorialRandomData(Count = 10, Minimum = -20, Maximum = -5)] int p1)
{
    Assert.InRange(p1, -20, -5);
}
```

 [NuPkg]: https://www.nuget.org/packages/Xunit.Combinatorial
