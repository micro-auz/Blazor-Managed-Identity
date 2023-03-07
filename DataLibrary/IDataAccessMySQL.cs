namespace DataLibrary
{
    public interface IDataAccessMySQL
    {
        Task<List<T>> LoadData<T, U>(string sql, U parameters, string connectionstring);
        Task SaveData<T>(string sql, T parameters, string connectionstring);
    }
}