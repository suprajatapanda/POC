using System.Collections;
using System.Data;
using System.Runtime.CompilerServices;
using System.Xml;
using Microsoft.Data.SqlClient;


namespace TRS.SqlHelper
{
    public sealed class TRSSqlHelper
    {
        private TRSSqlHelper()
        {
        }

        private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {
            ArgumentNullException.ThrowIfNull(command);
            if (commandParameters == null)
            {
                return;
            }

            SqlParameter[] sqlParameterArray = commandParameters;
            int index = 0;
            while (index < sqlParameterArray.Length)
            {
                SqlParameter sqlParameter = sqlParameterArray[index];
                if (sqlParameter != null)
                {
                    if ((sqlParameter.Direction == ParameterDirection.InputOutput || sqlParameter.Direction == ParameterDirection.Input) && sqlParameter.Value == null)
                    {
                        sqlParameter.Value = DBNull.Value;
                    }

                    command.Parameters.Add(sqlParameter);
                }
                checked { ++index; }
            }
        }
        private static void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
        {
            if (commandParameters == null && parameterValues == null)
            {
                return;
            }

            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            int num = checked(commandParameters.Length - 1);
            int index = 0;
            while (index <= num)
            {
                if (parameterValues[index] is IDbDataParameter)
                {
                    IDbDataParameter parameterValue = (IDbDataParameter)parameterValues[index];
                    commandParameters[index].Value = parameterValue.Value != null ? RuntimeHelpers.GetObjectValue(parameterValue.Value) : DBNull.Value;
                }
                else
                {
                    commandParameters[index].Value = parameterValues[index] != null ? RuntimeHelpers.GetObjectValue(parameterValues[index]) : DBNull.Value;
                }

                checked { ++index; }
            }
        }
        private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, ref bool mustCloseConnection)
        {
            ArgumentNullException.ThrowIfNull(command);
            if (commandText == null || commandText.Length == 0)
            {
                throw new ArgumentNullException(nameof(commandText));
            }

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                mustCloseConnection = true;
            }
            else
            {
                mustCloseConnection = false;
            }

            command.Connection = connection;
            command.CommandText = commandText;
            if (transaction != null)
            {
                command.Transaction = transaction.Connection != null ? transaction : throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", nameof(transaction));
            }

            command.CommandType = commandType;
            if (commandParameters == null)
            {
                return;
            }

            AttachParameters(command, commandParameters);
        }
        private static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connectionString, commandType, commandText, null);
        }
        private static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            ArgumentNullException.ThrowIfNull(connection);
            SqlCommand command = new();
            bool mustCloseConnection = false;
            PrepareCommand(command, connection, null, commandType, commandText, commandParameters, ref mustCloseConnection);
            int num = command.ExecuteNonQuery();
            command.Parameters.Clear();
            if (mustCloseConnection)
            {
                connection.Close();
            }

            return num;
        }
        private static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            ArgumentNullException.ThrowIfNull(connection);
            SqlCommand command = new();
            bool mustCloseConnection = false;
            PrepareCommand(command, connection, null, commandType, commandText, commandParameters, ref mustCloseConnection);
            object objectValue = RuntimeHelpers.GetObjectValue(command.ExecuteScalar());
            command.Parameters.Clear();
            if (mustCloseConnection)
            {
                connection.Close();
            }

            return objectValue;
        }
        private static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteXmlReader(connection, commandType, commandText, null);
        }
        private static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            ArgumentNullException.ThrowIfNull(connection);
            SqlCommand command = new();
            bool mustCloseConnection = false;
            try
            {
                PrepareCommand(command, connection, null, commandType, commandText, commandParameters, ref mustCloseConnection);
                XmlReader xmlReader = command.ExecuteXmlReader();
                command.Parameters.Clear();
                return xmlReader;
            }
            catch (Exception)
            {
                if (mustCloseConnection)
                {
                    connection.Close();
                }

                throw;
            }
        }
        private static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            ArgumentNullException.ThrowIfNull(connection);
            SqlCommand sqlCommand = new();
            DataSet dataSet = new();
            bool mustCloseConnection = false;
            PrepareCommand(sqlCommand, connection, null, commandType, commandText, commandParameters, ref mustCloseConnection);
            SqlDataAdapter sqlDataAdapter = null;
            try
            {
                sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                sqlDataAdapter.Fill(dataSet);
                sqlCommand.Parameters.Clear();
            }
            finally
            {
                sqlDataAdapter?.Dispose();
            }
            if (mustCloseConnection)
            {
                connection.Close();
            }

            return dataSet;
        }
        private static SqlDataReader ExecuteReader(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, SqlConnectionOwnership connectionOwnership)
        {
            ArgumentNullException.ThrowIfNull(connection);
            bool mustCloseConnection = false;
            SqlCommand command = new();
            try
            {
                PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters, ref mustCloseConnection);
                SqlDataReader sqlDataReader = connectionOwnership != SqlConnectionOwnership.External ? command.ExecuteReader(CommandBehavior.CloseConnection) : command.ExecuteReader();
                bool flag = true;
                try
                {
                    foreach (SqlParameter parameter in command.Parameters)
                    {
                        if (parameter.Direction != ParameterDirection.Input)
                        {
                            flag = false;
                        }
                    }
                }
                finally
                {
                    IEnumerator enumerator = null;
                    if (enumerator is IDisposable)
                    {
                        ((IDisposable)enumerator).Dispose();
                    }
                }
                if (flag)
                {
                    command.Parameters.Clear();
                }

                return sqlDataReader;
            }
            catch (Exception)
            {
                if (mustCloseConnection)
                {
                    connection.Close();
                }

                throw;
            }
        }
        private static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteReader(connectionString, commandType, commandText, null);
        }
        private static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteScalar(connectionString, commandType, commandText, null);
        }
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            SqlConnection connection = null;
            if (connectionString != null)
            {
                if (connectionString.Length != 0)
                {
                    try
                    {
                        connection = new SqlConnection(connectionString);
                        connection.Open();
                        return ExecuteNonQuery(connection, commandType, commandText, commandParameters);
                    }
                    finally
                    {
                        connection?.Dispose();
                    }
                }
            }
            throw new ArgumentNullException(nameof(connectionString));
        }
        public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException(nameof(spName));
            }

            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }

            SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
            AssignParameterValues(spParameterSet, parameterValues);
            return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
        }
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            SqlConnection connection = null;
            if (connectionString != null)
            {
                if (connectionString.Length != 0)
                {
                    try
                    {
                        connection = new SqlConnection(connectionString);
                        connection.Open();
                        return ExecuteDataset(connection, commandType, commandText, commandParameters);
                    }
                    finally
                    {
                        connection?.Dispose();
                    }
                }
            }
            throw new ArgumentNullException(nameof(connectionString));
        }
        public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException(nameof(spName));
            }

            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }

            SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
            AssignParameterValues(spParameterSet, parameterValues);
            return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
        }
        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            SqlConnection connection = null;
            if (connectionString != null)
            {
                if (connectionString.Length != 0)
                {
                    try
                    {
                        connection = new SqlConnection(connectionString);
                        connection.Open();
                        return ExecuteReader(connection, null, commandType, commandText, commandParameters, SqlConnectionOwnership.Internal);
                    }
                    catch (Exception)
                    {
                        connection?.Dispose();
                        throw;
                    }
                }
            }
            throw new ArgumentNullException(nameof(connectionString));
        }
        public static SqlDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException(nameof(spName));
            }

            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }

            SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
            AssignParameterValues(spParameterSet, parameterValues);
            return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
        }
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            SqlConnection connection = null;
            if (connectionString != null)
            {
                if (connectionString.Length != 0)
                {
                    try
                    {
                        connection = new SqlConnection(connectionString);
                        connection.Open();
                        return ExecuteScalar(connection, commandType, commandText, commandParameters);
                    }
                    finally
                    {
                        connection?.Dispose();
                    }
                }
            }
            throw new ArgumentNullException(nameof(connectionString));
        }
        public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException(nameof(spName));
            }

            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }

            SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
            AssignParameterValues(spParameterSet, parameterValues);
            return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
        }
        public static XmlReader ExecuteXmlReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            ArgumentNullException.ThrowIfNull(connection);
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException(nameof(spName));
            }

            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
            }

            SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
            AssignParameterValues(spParameterSet, parameterValues);
            return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
        }
        private enum SqlConnectionOwnership
        {
            Internal,
            External,
        }
    }
}
