namespace PCL_CE.Neo.Core.Abstractions;

public interface IDatabaseAdapter
{
    Task<T?> GetAsync<T>(string collection, string id) where T : class;
    Task<IEnumerable<T>> GetAllAsync<T>(string collection) where T : class;
    Task<bool> InsertAsync<T>(string collection, string id, T item) where T : class;
    Task<bool> UpdateAsync<T>(string collection, string id, T item) where T : class;
    Task<bool> DeleteAsync(string collection, string id);
    Task<long> CountAsync(string collection);
    Task<IEnumerable<T>> QueryAsync<T>(string collection, Func<T, bool> predicate) where T : class;
}
