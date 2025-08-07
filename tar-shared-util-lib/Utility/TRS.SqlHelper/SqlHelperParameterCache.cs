using System.Collections;
using System.Data;
using Microsoft.Data.SqlClient;


namespace TRS.SqlHelper
{
    public sealed class SqlHelperParameterCache
    {
        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        private SqlHelperParameterCache()
        {
        }

        private static SqlParameter[] DiscoverSpParameterSet(SqlConnection connection,string spName,bool includeReturnValueParameter,params object[] parameterValues)
        {
            ArgumentNullException.ThrowIfNull(connection);

            SqlCommand command = spName != null && spName.Length != 0 ? new SqlCommand(spName, connection) : throw new ArgumentNullException(nameof(spName));
            command.CommandType = CommandType.StoredProcedure;
            connection.Open();
            SqlCommandBuilder.DeriveParameters(command);
            connection.Close();
            if (!includeReturnValueParameter)
            {
                command.Parameters.RemoveAt(0);
            }

            SqlParameter[] sqlParameterArray1 = new SqlParameter[checked(command.Parameters.Count - 1 + 1)];
            command.Parameters.CopyTo((Array)sqlParameterArray1, 0);
            SqlParameter[] sqlParameterArray2 = sqlParameterArray1;
            int index = 0;
            while (index < sqlParameterArray2.Length)
            {
                sqlParameterArray2[index].Value = DBNull.Value;
                checked { ++index; }
            }
            return sqlParameterArray1;
        }

        private static SqlParameter[] CloneParameters(SqlParameter[] originalParameters)
        {
            int num1 = checked(originalParameters.Length - 1);
            SqlParameter[] sqlParameterArray = new SqlParameter[checked(num1 + 1)];
            int num2 = num1;
            int index = 0;
            while (index <= num2)
            {
                sqlParameterArray[index] = (SqlParameter)((ICloneable)originalParameters[index]).Clone();
                checked { ++index; }
            }
            return sqlParameterArray;
        }

        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, false);
        }

        public static SqlParameter[] GetSpParameterSet(
          string connectionString,
          string spName,
          bool includeReturnValueParameter)
        {
            SqlConnection connection = null;
            if (connectionString != null)
            {
                if (connectionString.Length != 0)
                {
                    try
                    {
                        connection = new SqlConnection(connectionString);
                        return GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
                    }
                    finally
                    {
                        connection?.Dispose();
                    }
                }
            }
            throw new ArgumentNullException(nameof(connectionString));
        }

        public static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName)
        {
            return GetSpParameterSet(connection, spName, false);
        }

        public static SqlParameter[] GetSpParameterSet(
          SqlConnection connection,
          string spName,
          bool includeReturnValueParameter)
        {
            ArgumentNullException.ThrowIfNull(connection);
            SqlConnection connection1 = null;
            try
            {
                connection1 = (SqlConnection)((ICloneable)connection).Clone();
                return GetSpParameterSetInternal(connection1, spName, includeReturnValueParameter);
            }
            finally
            {
                connection1?.Dispose();
            }
        }

        private static SqlParameter[] GetSpParameterSetInternal(
            SqlConnection connection,
            string spName,
            bool includeReturnValueParameter)
        {
            ArgumentNullException.ThrowIfNull(connection);
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException(nameof(spName));
            }

            string key = $"{connection.ConnectionString}:{spName}{(includeReturnValueParameter ? ":include ReturnValue Parameter" : "")}";
            SqlParameter[] originalParameters = (SqlParameter[])paramCache[key];
            if (originalParameters == null)
            {
                SqlParameter[] sqlParameterArray = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                paramCache[key] = sqlParameterArray;
                originalParameters = sqlParameterArray;
            }
            return CloneParameters(originalParameters);
        }
    }
}
