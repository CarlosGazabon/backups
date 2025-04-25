namespace inventio.Repositories.EntityFramework
{
    public interface IEFRepository<T>
    {
        /// <summary>
        /// Execute stores procedure and load a list of entities at the end
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="commandText">Command text</param>
        /// <param name="parameters">The Parameters</param>
        /// <returns>return Entities</returns>
        IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : class;
        IList<U> ExecuteStoredProcedure<U>(string storedProcName, params object[] parameters);
    }
}