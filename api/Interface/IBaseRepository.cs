namespace api.Interface;

public interface IBaseRepository<T, TKey>
{
    Task<TKey> CreateAsync<TInput>(TInput? createInput) where TInput : class;
    Task<T> CreateAndReturnAsync<TInput>(TInput? createInput) where TInput : class;
    Task<bool> DeleteAsync(TKey Id);
    Task<int> DeleteManyAsync<TIncludeParams, TExcludeParams>(TIncludeParams includeParams, TExcludeParams excludeParams);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(TKey Id);
    Task<IEnumerable<T>> SearchAsync<TIncludeParams,TExcludeParams>(TIncludeParams? includeParams,TExcludeParams? excludeParams);
    Task<T> UpdateAndReturnAsync<TInput>(TKey id, TInput updateInput) where TInput : class;
    Task<bool> UpdateAsync<TInput>(TKey id, TInput updateInput) where TInput : class;
    Task<bool> UpdateWhereAsync<TUpdateParams, TIncludeParams, TExcludeParams>(
        TUpdateParams updateParams,
        TIncludeParams? includeParams,
        TExcludeParams? excludeParams
    )
        where TUpdateParams : class
        where TIncludeParams : class?
        where TExcludeParams : class?;
}