using System.Data;
using System.Linq;
using api.Interface;
using Dapper;
using api.Helper;
using Microsoft.SqlServer.Server;
using api.Enumerable;
using Microsoft.AspNetCore.Http.HttpResults;

namespace api.Repository;

public class BaseRepository<T, TKey>(IDbConnection db, string tableName) : IBaseRepository<T, TKey>
{
    //==================
    // CREATE AND RETURN
    //==================
    public virtual async Task<T> CreateAndReturnAsync<TInput>(TInput? createInput) where TInput : class
    {
        TKey createdId = await CreateAsync(createInput);
        return await GetByIdAsync(createdId);
    }
    //=================
    // CREATE
    //=================
    public virtual async Task<TKey> CreateAsync<TInput>(TInput? createInput) where TInput : class
    {
        DynamicParameters createParams = FormatParams.BuildDynamicParameters(createInput);

        Guid? Id = null;
        if (typeof(TKey) == typeof(Guid))
        {
            Id = Guid.NewGuid();
        }

        if (Id != null)
        {
            createParams.Add("Id", Id);
        }

        createParams.Add("CreatedAt", DateTime.UtcNow);

        //insert with params if they exist
        if (createParams.ParameterNames.Any())
        {
            var columnNames = string.Join(", ", createParams.ParameterNames);
            var paramNames = string.Join(", ", createParams.ParameterNames.Select(p => "@" + p));

            var sql = $@"
            INSERT INTO {tableName} ({columnNames})
            VALUES ({paramNames})
            RETURNING Id
        ";
            return await db.QuerySingleAsync<TKey>(sql, createParams);
        }
        //insert with default values
        else
        {
            return await db.QuerySingleAsync($"INSERT INTO {tableName} DEFAULT VALUES RETURNING ID");
        }
    }
    //=================
    // DELETE
    //=================
    public virtual async Task<bool> DeleteAsync(TKey id)
    {
        var sql = $@"
            DELETE FROM {tableName}
            WHERE Id = @Id
        ";
        var rows = await db.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
    //=================
    // DELETE MANY
    //=================
    public virtual async Task<int> DeleteManyAsync<TIncludeParams,TExcludeParams>(TIncludeParams includeParams,TExcludeParams excludeParams)
    {
        var sql = $@"
            DELETE FROM {tableName}
        ";
        DynamicParameters dynamicParamsInclude = FormatParams.BuildDynamicParameters(includeParams);
        DynamicParameters dynamicParamsExclude = FormatParams.BuildDynamicParameters(excludeParams);
        DynamicParameters dynamicParamsAll = FormatParams.MergeDynamicParameters(dynamicParamsInclude, dynamicParamsExclude);

        //run sql as is with no additional filtering
        if (!dynamicParamsAll.ParameterNames.Any())
        {
            return await db.ExecuteAsync(sql);
        }

        var whereClauses = new List<string>();

        if (dynamicParamsInclude.ParameterNames.Any())
        {
            string whereClausesInclude = FormatParams.BuildWhereClauses(dynamicParamsInclude, WhereClauseFilters.Include);
            whereClauses.Add(whereClausesInclude);
        }
        if (dynamicParamsExclude.ParameterNames.Any())
        {
            string whereClausesExclude = FormatParams.BuildWhereClauses(dynamicParamsExclude, WhereClauseFilters.Exclude);
            whereClauses.Add(whereClausesExclude);
        }

        if (whereClauses.Count != 0)
        {
            sql += "WHERE " + string.Join(" AND ", whereClauses);
        }

        var deletedRowCount = await db.ExecuteAsync(sql, dynamicParamsAll);
        return deletedRowCount;
    }

    //=================
    // GET ALL
    //=================
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var sql = $@"
            SELECT * FROM {tableName}
        ";
        return await db.QueryAsync<T>(sql, tableName);
    }
    //=================
    // GET BY ID
    //=================
    public virtual async Task<T> GetByIdAsync(TKey id)
    {
        var sql = $@"
            SELECT * FROM {tableName}
            WHERE Id = @Id
        ";
        return await db.QuerySingleAsync<T>(sql, new { Id = id });
    }
    //=================
    // SEARCH
    //=================
    public virtual async Task<IEnumerable<T>> SearchAsync<TIncludeParams, TExcludeParams>(TIncludeParams? includeParams, TExcludeParams? excludeParams)
    {
        var sql = $@"
            SELECT * FROM {tableName}
        ";

        DynamicParameters dynamicParamsInclude = FormatParams.BuildDynamicParameters(includeParams);
        DynamicParameters dynamicParamsExclude = FormatParams.BuildDynamicParameters(excludeParams);
        DynamicParameters dynamicParamsAll = FormatParams.MergeDynamicParameters(dynamicParamsInclude, dynamicParamsExclude);

        //run sql as is with no additional filtering
        if (!dynamicParamsAll.ParameterNames.Any())
        {
            return await db.QueryAsync<T>(sql);
        }

        var whereClauses = new List<string>();

        if (dynamicParamsInclude.ParameterNames.Any())
        {
            string whereClausesInclude = FormatParams.BuildWhereClauses(dynamicParamsInclude, WhereClauseFilters.Include);
            whereClauses.Add(whereClausesInclude);
        }
        if (dynamicParamsExclude.ParameterNames.Any())
        {
            string whereClausesExclude = FormatParams.BuildWhereClauses(dynamicParamsExclude, WhereClauseFilters.Exclude);
            whereClauses.Add(whereClausesExclude);
        }

        if (whereClauses.Count != 0)
        {
            sql += "WHERE " + string.Join(" AND ", whereClauses);
        }

        return await db.QueryAsync<T>(sql, dynamicParamsAll);
    }
    //==================
    // UPDATE AND RETURN
    //==================
    public virtual async Task<T> UpdateAndReturnAsync<TInput>(TKey id, TInput updateInput) where TInput : class
    {
        await UpdateAsync(id, updateInput);
        return await GetByIdAsync(id);
    }

    //==================
    // UPDATE
    //==================
    public virtual async Task<bool> UpdateAsync<TInput>(TKey id, TInput updateInput) where TInput : class
    {
        DynamicParameters updateParams = FormatParams.BuildDynamicParameters(updateInput);

        updateParams.Add("Id", id);

        //update if params exist
        if (updateParams.ParameterNames.Any())
        {
            var setClauses = FormatParams.BuildSetClauses(updateParams);

            var sql = $@"
            UPDATE {tableName}
            SET {setClauses}
            WHERE Id = @Id
        ";
            var rowsAffected = await db.ExecuteAsync(sql, updateParams);
            return rowsAffected > 0;
        }
        //no update was done
        return false;
    }

    public async Task<bool> UpdateManyAsync<TUpdateParams, TIncludeParams, TExcludeParams>(
        TUpdateParams updateInput,
        TIncludeParams? includeParams = null,
        TExcludeParams? excludeParams = null
    )
        where TUpdateParams : class
        where TIncludeParams : class?
        where TExcludeParams : class?
    {
        DynamicParameters dynamicParamsUpdate = FormatParams.BuildDynamicParameters(updateInput);
        DynamicParameters dynamicParamsInclude = FormatParams.BuildDynamicParameters(includeParams);
        DynamicParameters dynamicParamsExclude = FormatParams.BuildDynamicParameters(excludeParams);
        DynamicParameters dynamicParamsFilter = FormatParams.MergeDynamicParameters(dynamicParamsInclude, dynamicParamsExclude);
        DynamicParameters dynamicParamsAll = FormatParams.MergeDynamicParameters(dynamicParamsUpdate, dynamicParamsFilter);

        var setClauses = FormatParams.BuildSetClauses(dynamicParamsUpdate);
        var sql = $@"
            UPDATE {tableName}
            SET {setClauses}
        ";

        if (!dynamicParamsFilter.ParameterNames.Any())
        {
            return (await db.ExecuteAsync(sql, dynamicParamsUpdate)) > 0;
        }

        var whereClauses = new List<string>();

        if (dynamicParamsInclude.ParameterNames.Any())
        {
            string whereClausesInclude = FormatParams.BuildWhereClauses(dynamicParamsInclude, WhereClauseFilters.Include);
            whereClauses.Add(whereClausesInclude);
        }
        if (dynamicParamsExclude.ParameterNames.Any())
        {
            string whereClausesExclude = FormatParams.BuildWhereClauses(dynamicParamsExclude, WhereClauseFilters.Exclude);
            whereClauses.Add(whereClausesExclude);
        }

        if (whereClauses.Count != 0)
        {
            sql += "WHERE " + string.Join(" AND ", whereClauses);
        }
        var rowsAffected = await db.ExecuteAsync(sql, dynamicParamsAll);
        return rowsAffected > 0;
    }
}