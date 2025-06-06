using System.Reflection;
using api.Enumerable;
using Dapper;

namespace api.Helper;

public static class FormatParams
{
    public static DynamicParameters BuildDynamicParameters(object? parameters)
    {
        DynamicParameters dynamicParameters = new();

        if (parameters == null)
        {
            return dynamicParameters;
        }

        foreach (var property in parameters.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var name = property.Name;
            var value = property.GetValue(parameters);
            if (value != null)
            {
                dynamicParameters.Add(name, value);
            }
        }

        return dynamicParameters;
    }
    public static string BuildWhereClauses(DynamicParameters dynamicParameters, WhereClauseFilters filterType)
    {
        var whereClauses = new List<string>();
        string whereClauseFilterSyntax = GetWhereClauseSyntax(filterType);
        foreach (var paramName in dynamicParameters.ParameterNames)
        {
            whereClauses.Add($"{paramName} {whereClauseFilterSyntax} @{paramName}");
        }
        if (whereClauses.Count > 0)
        {
            return string.Join(" AND ", whereClauses);
        }
        else
        {
            return string.Empty;
        }
    }
    public static string BuildSetClauses(DynamicParameters dynamicParameters)
    {
        //update if params exist
        if (dynamicParameters.ParameterNames.Any())
        {
            return string.Join(", ", dynamicParameters.ParameterNames.Select(p => $"{p} = @{p}"));
        }
        return string.Empty;
    }
    private static string GetWhereClauseSyntax(WhereClauseFilters filterType)
    {
        if (filterType == WhereClauseFilters.Include)
        {
            return "=";
        }
        if (filterType == WhereClauseFilters.Exclude)
        {
            return "<>";
        }
        return "";
    }
    public static DynamicParameters MergeDynamicParameters(DynamicParameters first, DynamicParameters second)
    {
        DynamicParameters mergedResult = new();
        CopyDynamicParameters(first, mergedResult, throwOnConflict: false);
        CopyDynamicParameters(second, mergedResult, throwOnConflict: false);
        return mergedResult;
    }
    private static void CopyDynamicParameters(DynamicParameters source, DynamicParameters destination, bool throwOnConflict)
    {
        foreach (var paramName in source.ParameterNames.Distinct())
    {
        var value = ((dynamic)source).Get<object>(paramName);

        if (destination.ParameterNames.Contains(paramName))
        {
            if (throwOnConflict)
                throw new ArgumentException($"Parameter conflict: '{paramName}' already exists in destination.");
        }
        else
        {
            destination.Add(paramName, value);
        }
    }
    }
}