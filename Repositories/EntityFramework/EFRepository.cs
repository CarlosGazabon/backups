using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Inventio.Data;

namespace inventio.Repositories.EntityFramework
{
    public class EFRepository<T> : IEFRepository<T> where T : class
    {
        private readonly ApplicationDBContext context;

        public EFRepository(ApplicationDBContext context)
        {
            this.context = context;
        }


        private IList<U> MapToList<U>(DbDataReader dr)
        {
            var objList = new List<U>();
            var props = typeof(U).GetRuntimeProperties();

            var colMapping = dr.GetColumnSchema()
                .Where(x => props.Any(y => y.Name.ToLower() == x.ColumnName.ToLower()))
                .ToDictionary(key => key.ColumnName.ToLower());

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    U obj = Activator.CreateInstance<U>();
                    foreach (var prop in props)
                    {
                        if (colMapping.ContainsKey(prop.Name.ToLower()))
                        {
                            var val = dr.GetValue(colMapping[prop.Name.ToLower()].ColumnOrdinal!.Value);
                            prop.SetValue(obj, val == DBNull.Value ? null : val);
                        }
                    }
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : class
        {
            ////add parameters to command
            if (parameters != null && parameters.Length > 0)
            {
                for (int i = 0; i <= parameters.Length - 1; i++)
                {
                    var p = parameters[i] as DbParameter;
                    if (p == null)
                    {
                        throw new Exception("Not support parameter type");
                    }

                    commandText += i == 0 ? " " : ", ";

                    commandText += "@" + p.ParameterName;
                    if (p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output)
                    {
                        ////output parameter
                        commandText += " output";
                    }
                }
            }
            if (parameters == null)
                return context.Set<TEntity>().FromSqlRaw(commandText).ToList();
            else
                return context.Set<TEntity>().FromSqlRaw(commandText, parameters).ToList();
        }

        public IList<U> ExecuteStoredProcedure<U>(string storedProcName, params object[] parameters)
        {
            var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = storedProcName;
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddRange(parameters);

            if (command.Connection!.State == System.Data.ConnectionState.Closed)
                command.Connection.Open();
            try
            {
                using (var reader = command.ExecuteReader())
                {
                    return MapToList<U>(reader);
                }
            }
            finally
            {
                command.Connection.Close();
            }
        }

    }
}