using System.Reflection;
using Combinatorial.Core;

namespace TUnit;

public sealed class CombinatorialDataAttribute : NonTypedDataSourceGeneratorAttribute
{
    /// <inheritdoc />
    public override IEnumerable<Func<object?[]?>> GenerateDataSources(DataGeneratorMetadata dataGeneratorMetadata)
    {
        Requires.NotNull(dataGeneratorMetadata, nameof(dataGeneratorMetadata));

        ParameterInfo[]? parameters = dataGeneratorMetadata.ParameterInfos;

        if (parameters is null or { Length: 0 })
        {
            yield return () => [];
            yield break;
        }

        var values = new List<object?>[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            values[i] = ValuesUtilities.GetValuesFor(parameters[i]).ToList();
        }


        object[] currentValues = new object[parameters.Length];
        foreach (object?[] parameterValues in FillCombinations(parameters, values, currentValues, 0))
        {
            yield return () => parameterValues;
        }
    }

    /// <summary>
    /// Produces a sequence of argument arrays that capture every possible
    /// combination of values.
    /// </summary>
    /// <param name="parameters">The parameters taken by the test method.</param>
    /// <param name="candidateValues">An array of each argument's list of possible values.</param>
    /// <param name="currentValues">An array that is being recursively initialized with a set of arguments to pass to the test method.</param>
    /// <param name="index">The index into <paramref name="currentValues"/> that this particular invocation should rotate through <paramref name="candidateValues"/> for.</param>
    /// <returns>A sequence of all combinations of arguments from <paramref name="candidateValues"/>, starting at <paramref name="index"/>.</returns>
    private static IEnumerable<object?[]> FillCombinations(ParameterInfo[] parameters, List<object?>[] candidateValues, object?[] currentValues, int index)
    {
        Requires.NotNull(parameters, nameof(parameters));
        Requires.NotNull(candidateValues, nameof(candidateValues));
        Requires.NotNull(currentValues, nameof(currentValues));
        Requires.Argument(parameters.Length == candidateValues.Length, nameof(candidateValues), $"Expected to have same array length as {nameof(parameters)}");
        Requires.Argument(parameters.Length == currentValues.Length, nameof(currentValues), $"Expected to have same array length as {nameof(parameters)}");
        Requires.Range(index >= 0 && index < parameters.Length, nameof(index));

        foreach (object? value in candidateValues[index])
        {
            currentValues[index] = value;

            if (index + 1 < parameters.Length)
            {
                foreach (object?[] result in FillCombinations(parameters, candidateValues, currentValues, index + 1))
                {
                    yield return result;
                }
            }
            else
            {
                // We're the tail end, so just produce the value.
                // Copy the array before returning since we're about to mutate currentValues
                object[] finalSet = new object[currentValues.Length];
                Array.Copy(currentValues, finalSet, currentValues.Length);
                yield return finalSet;
            }
        }
    }
}
