﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DotOrmLib.Sql;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.SqlClient.DataClassification;
using static System.Formats.Asn1.AsnWriter;
using System.Linq.Expressions;
using Newtonsoft.Json;
using DotMpi;

namespace DotOrmLib
{
    //defines a common set of operations
    public interface IService
    {

    }
    //a service that provides persistance for a common set of operations
    public interface IRepo : IService { }
    public interface IService<T> : IService
    {
        Task<T> Add(T entity);
        Task<T> GetById(int id);
    }
    public interface IRepo<T> : IService<T>, IRepo
    {

    }


    public interface IDotOrmRepoBase
    {
        Task ExecuteAsync(string query, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<TResult> ExecuteScalarASync<TResult>(string query, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<IEnumerable<TResult>> QueryAsync<TResult>(string query, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<TResult> QueryFirstAsync<TResult>(string query, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<TResult> QuerySingleAsync<TResult>(string query, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<TResult> QuerySingleOrDefaultAsync<TResult>(string query, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    }


    public static class IDbConnectionExensions
    {
        public static async Task
            ExecuteAsync1(
                this IDbConnection connection,
                string query, object? param = null,
                IDbTransaction? transaction = null,
                int? commandTimeout = null,
                CommandType? commandType = null
            )
        {
            await Task.CompletedTask;
        }

        public static async Task<TResult>
            ExecuteScalarAsync1<TResult>(
                  this IDbConnection connection,
                  string query, object? param = null,
                  IDbTransaction? transaction = null,
                  int? commandTimeout = null,
                  CommandType? commandType = null
            )
        {
            return await Task.FromResult<TResult>(default);
        }

        public static async Task<IEnumerable<TResult>>
            QueryAsync1<TResult>(
                  this IDbConnection connection,
                  string query, object? param = null,
                  IDbTransaction? transaction = null,
                  int? commandTimeout = null,
                  CommandType? commandType = null
            )
        {
            var result = await connection.QueryAsync<TResult>(query, param, transaction, commandTimeout, commandType);
            return await Task.FromResult(result);
        }

        public static async Task<TResult>
            QueryFirstAsync1<TResult>(
                this IDbConnection connection,
                string query, object? param = null,
                IDbTransaction? transaction = null,
                int? commandTimeout = null,
                CommandType? commandType = null
          )
        {
            return await Task.FromResult<TResult>(default);
        }

        public static async Task<TResult>
            QuerySingleAsync1<TResult>(
                this IDbConnection connection,
                string query, object? param = null,
                IDbTransaction? transaction = null,
                int? commandTimeout = null,
                CommandType? commandType = null
          )
        {
            return await Task.FromResult<TResult>(default);
        }

        public static async Task<TResult>
            QuerySingleOrDefaultAsync1<TResult>(
                this IDbConnection connection,
                string query, object? param = null,
                IDbTransaction? transaction = null,
                int? commandTimeout = null,
                CommandType? commandType = null
          )
        {
            return await Task.FromResult<TResult>(default);
        }
    }
    public class DotOrmRepoBase : IDotOrmRepoBase
    {


        protected string? connectionString;
        public DotOrmRepoBase(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task ExecuteAsync(string query, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.ExecuteAsync(query, param);

            }
        }
        public async Task<TResult> ExecuteScalarASync<TResult>(string query, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return await conn.ExecuteScalarAsync<TResult>(query, param);
            }
        }

        public async Task<IEnumerable<TResult>> QueryAsync<TResult>(string query, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return await conn.QueryAsync<TResult>(query, param);
            }
        }

        public async Task<TResult> QueryFirstAsync<TResult>(string query, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return await conn.QueryFirstAsync<TResult>(query, param);
            }
        }

        public async Task<TResult> QuerySingleAsync<TResult>(string query, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return await conn.QuerySingleAsync<TResult>(query, param);
            }
        }

        public async Task<TResult> QuerySingleOrDefaultAsync<TResult>(string query, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return await conn.QuerySingleOrDefaultAsync<TResult>(query, param);
            }
        }

    }
    public class DotOrmRepo<T> : DotOrmRepoBase, IRepo<T>, IDotOrmRepoBase
        where T : class
    {
        public SqlTableDef Model;
        private SqlColumnDef? identityColumn;
        private readonly string tableName;
        private string? idWhereClause;
        Func<T, dynamic>? idGetter = null;
        private string selectClause;
        string defaultSort;
        string idNotFoundErrorMessage;

        public DotOrmRepo(string connectionString)
            : base(connectionString)
        {
            Model = ModelFactory.GetSchema<T>();
            tableName = Model.TableName;
            identityColumn = Model.Columns.FirstOrDefault(x => x.IsIdentity);

            if (identityColumn is not null)
            {
                idWhereClause = $"[{identityColumn.Name}]=@id";
                var prop = typeof(T).GetProperty(identityColumn.PropertyName);
                idGetter = (entity) => new { Id = prop.GetValue(entity) };
                defaultSort = $"[{identityColumn.Name}]";
            }
            else
            {
                var keys = Model.Columns.Where(x => x.IsPrimaryKey);
                if (keys.Any())
                {
                    if (keys.Count() == 1 && identityColumn is null)
                    {
                        identityColumn = keys.First();
                        var prop = typeof(T).GetProperty(identityColumn.PropertyName);
                        idGetter = (entity) => new { Id = prop.GetValue(entity) };
                        defaultSort = $"[{identityColumn.Name}]";
                    }
                    else
                    {

                        defaultSort = string.Join(", ", keys.Select(key => $"[{key.Name}]"));
                    }
                    idWhereClause = string.Join(" and ", keys.Select(key => $"[{key.Name}]=@{key.PropertyName}"));
                    //idGetter = (entity) => new { }
                }
                else
                {
                    //idNotFoundErrorMessage = $"Failed to find id or key columns for model {Model.TableName}";
                    //throw new Exception($"Failed to find id or key columns for model {Model.TableName}");
                    defaultSort= string.Join(", ", keys.Select(key => $"[{key.Name}]"));
                }
            }
            var selectClauses = Model.Columns.Select(x => $"[{x.Name}] as [{x.PropertyName}]");
            this.selectClause = $"select {string.Join(", ", selectClauses)} from [{Model.TableName}]";
        }


        private string? insertQuery = null;
        private string GetInsertQuery()
        {
            if (insertQuery is null)
            {
                var sb = new StringBuilder();
                var insertedColumnDefs = Model.Columns.Select(x => x.SqlColumnDefintionString());
                sb.AppendLine($"declare @inserted table({string.Join(",", insertedColumnDefs)})");

                var insertColumns = Model.Columns.Where(x => !x.IsIdentity);

                var insertColumnClause = string.Join(", ", insertColumns.Select(x => $"[{x.Name}]"));
                var valuesClause = string.Join(",", insertColumns.Select(x => $"@{x.PropertyName}"));
                var insertedColumns = Model.Columns.Select(x => $"inserted.[{x.Name}]");
                var outputClause = $"OUTPUT {string.Join(", ", insertedColumns)} INTO @inserted";
                sb.AppendLine($"INSERT INTO [{Model.TableName}]({insertColumnClause})\r\n{outputClause}\r\nVALUES ({valuesClause})");
                var selectClauses = Model.Columns.Select(x => $"[{x.Name}] as [{x.PropertyName}]");
                var selectClause = $"select {string.Join(", ", selectClauses)} from @inserted";
                sb.AppendLine(selectClause);
                insertQuery = sb.ToString();
            }
            return insertQuery;

        }

        public async Task<T> Add(T entity)
        {
            //return await Transient.RetryWithTimeout(async () =>
            {

                /* 
                OPTIONS: Insert entity and select based on
                    1) scope_identity()
                    2) primary keys
                    3) unique
                    4) row scan using insert table
                
                NOTES:
                    Cache query based on model. Should not be calculated at runtime for each method call as here.
                        var result = Model.Add(entity)
                            var result = model.Add(Func<T> adder)
                                return adder(entity)
                TESTS:
                    All 4 option scenarios should have a test.
                    
                 */
                var insertQuery = GetInsertQuery();

                if (false) 
                {
                    if (idNotFoundErrorMessage is not null)
                        throw new Exception($"Identity or single primary key not defined on type: {typeof(T).Name}");
                    var insertColumns = Model.Columns.Where(x => !x.IsIdentity);

                    var insertColumnClause = string.Join(", ", insertColumns.Select(x => $"[{x.Name}]"));
                    var valuesClause = string.Join(",", insertColumns.Select(x => $"@{x.PropertyName}"));
                    var scopeIdWhereClause = $"[{identityColumn.Name}]=@scopeId";
                    var query = $@"
                    INSERT INTO [{Model.TableName}]({insertColumnClause})
                    VALUES ({valuesClause})

                    declare @scopeId {identityColumn.SqlDbType} = SCOPE_IDENTITY();

                    {selectClause} where {scopeIdWhereClause}
                ";
                }
                //.Select(x => $"[{model.TryGetColumnNameByProperty(x.Name)}] = @{x.PropertyName}");
                using (var conn = new SqlConnection(connectionString))
                {

                    await conn.OpenAsync();
                    var trans = await conn.BeginTransactionAsync();

                    try
                    {
                        var result = await Transient.RetryWithTimeout(() => conn.QueryFirstAsync<T>(insertQuery, entity, transaction: trans));
                        await trans.CommitAsync();
                        return result;
                    }
                    catch
                    {
                        try { await trans.RollbackAsync(); } catch { }
                        throw;
                    }
                }
            }
            //);
        }

        public async Task<List<T>> Get(IEnumerable<KeyValuePair<string, object>> whereParam)
        {
            //return await Transient.RetryWithTimeout(async () =>
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    var whereClause = whereParam.Select(x => $"[{Model.TryGetColumnNameByProperty(x.Key)}] = @{x.Key}");
                    var query = $"{selectClause} where {string.Join(", ", whereClause)}";
                    var result = await conn.QueryAsync<T>(query, whereParam);
                    return result.ToList();
                }
            }
            //);
        }

        public async Task<List<T>> Get(object whereParam)
        {
            //return await Transient.RetryWithTimeout(async () =>
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    var whereClause = whereParam.GetType().GetProperties().Select(x => $"[{Model.TryGetColumnNameByProperty(x.Name)}] = @{x.Name}");
                    var query = $"{selectClause} where {string.Join(" and ", whereClause)}";
                    var result = await conn.QueryAsync<T>(query, whereParam);
                    return result.ToList();

                }
            }
            //);
        }

        public async Task<List<T>> Get(WhereClauseBuilder<T> where)
        {
            //return await Transient.RetryWithTimeout(async () =>
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    var whereParams = where.Build();
                    var query = $"{selectClause} where {whereParams.WhereClause}";
                    var param = new DynamicParameters();
                    whereParams.Parameters.ToList().ForEach(x =>
                    {
                        if (x.Value is not null)
                            param.Add(x.Key, x.Value);
                        else
                            param.Add(x.Key, DBNull.Value, DbType.Object);
                    });
                    var result = await conn.QueryAsync<T>(query, param);
                    return result.ToList();

                }
            }
            //);
        }

        public WhereClauseBuilder<T> Where(Expression<Func<T, bool>> expression)
        {
            //return Transient.RetryWithTimeout(() =>
            {
                var builder = new WhereClauseBuilder<T>(expression, this);
                return builder;
            }
            //);
        }



        public async Task<int> Count()
        {
            //return await Transient.RetryWithTimeout(async () =>
            {
                var query = $"select count(0) from [{Model.TableName}]";
                using (var conn = new SqlConnection(connectionString))
                {
                    return await conn.ExecuteScalarAsync<int>(query);
                }
            }
            //);
        }

        public async Task<int> Count(string whereClause, string parameterJson)
        {
            //return await Transient.RetryWithTimeout(async () =>
            {
                if (string.IsNullOrEmpty(whereClause)) return await Count();
                DynamicParameters? param = GetDynamicParameters(parameterJson);
                var query = $"select count(0) from [{Model.TableName}] where {whereClause}";
                using (var conn = new SqlConnection(connectionString))
                {
                    return await conn.ExecuteScalarAsync<int>(query, param);
                }
            }
            //);
        }


        public async Task<List<T>> GetList(int skip, int take, string? whereClause = null, string? parameterJson = null)
        {
            //return await Transient.RetryWithTimeout(async () =>
            {
                if (idNotFoundErrorMessage is not null && defaultSort is null)
                    throw new Exception($"Identity or key sort not defined on type: {typeof(T).Name}");
                var delim = " from ";
                var idx = selectClause.IndexOf(delim);
                var preamble = selectClause.Substring(0, idx);
                var tail = selectClause.Substring(idx);
                preamble = @$"{preamble}, ROW_NUMBER() OVER (ORDER BY {defaultSort}) AS DotOrmRowNumber {tail}";
                if (whereClause != null)
                    preamble = $"{preamble} where {whereClause}";

                var query = $@"select * from 
                ({preamble})
                t  WHERE DotOrmRowNumber BETWEEN {skip} AND {skip + take}";

                DynamicParameters? param = GetDynamicParameters(parameterJson);

                using (var conn = new SqlConnection(connectionString))
                {
                    var result = await conn.QueryAsync<T>(query, param);
                    return result.ToList();
                }

            }
            //);
        }

        private DynamicParameters? GetDynamicParameters(string? parameterJson)
        {
            //return Transient.RetryWithTimeout(() =>
            {
                DynamicParameters? param = null;
                if (parameterJson != null)
                {
                    param = new();
                    var whereParams =
                        JsonConvert.DeserializeObject<Dictionary<string, SerializableValue>>(parameterJson);
                    whereParams.ToList().ForEach(x =>
                    {
                        if (x.Value is not null)
                            param.Add(x.Key, x.Value.ObjectValue);
                        else
                            param.Add(x.Key, DBNull.Value, DbType.Object);
                    });
                }
                return param;
            }
            //);
        }

        public async Task<T> GetById(int id)

        {
            //return await Transient.RetryWithTimeout(async () =>
            {
                if (idNotFoundErrorMessage is not null)
                    throw new Exception($"Identity or single primary key not defined on type: {typeof(T).Name}");

                if (string.IsNullOrEmpty(idWhereClause))
                    throw new Exception($"Could not find primary or identity column for type: {typeof(T).Name}");

                using (var conn = new SqlConnection(connectionString))
                {
                    var query = $"{selectClause} where {idWhereClause}";
                    return await conn.QueryFirstOrDefaultAsync<T>(query, new { id });
                }
            }
            //);
        }


        public async Task<int> DeleteById(int id)
        {
            //return await Transient.RetryWithTimeout(async () =>
            {
                if (idNotFoundErrorMessage is not null)
                    throw new Exception($"Identity or single primary key not defined on type: {typeof(T).Name}");
                using (var conn = new SqlConnection(connectionString))
                {
                    var query = $"delete from [{Model.TableName}] where {idWhereClause}";
                    return await conn.ExecuteScalarAsync<int>(query, new { id });
                }
            }
            //);
        }

        public async Task<int> Delete(T entity)
        {
            //return await Transient.RetryWithTimeout(async () =>
            {
                //TODO: should be able to delete by matching all columns.
                if (idGetter is null)
                    throw new Exception($"Identity or primary key not defined on type: {typeof(T).Name}");
                using (var conn = new SqlConnection(connectionString))
                {
                    var query = $"delete from [{Model.TableName}] where {idWhereClause}";
                    var idWhere = idGetter(entity);
                    return await conn.ExecuteScalarAsync<int>(query, (object)idWhere);
                }
            }
            //);
        }

        public async Task<int> Update(object updateParam, object? whereParam = null)
        {
            //return await Transient.RetryWithTimeout(async () =>
            {
                if (updateParam is null)
                    return 0;

                var whereClauses = whereParam?.GetType().GetProperties().Select(x => $"[{Model.TryGetColumnNameByProperty(x.Name)}] = @{x.Name}_0");
                var whereDict = whereParam?.GetType().GetProperties().ToDictionary(x => $"{x.Name}_0", x => x.GetValue(whereParam));
                // var whereClause = whereDict?.Select(x => $"{x.Key} = {x.Value}");

                var updateClauses = updateParam.GetType().GetProperties().Select(x => $"[{Model.TryGetColumnNameByProperty(x.Name)}] = @{x.Name}");
                var updateDict = updateParam.GetType().GetProperties().ToDictionary(x => $"@{x.Name}", x => x.GetValue(updateParam));

                var query = @$"
                -- DECLARE @updatedIds TABLE ([{identityColumn.Name}] {identityColumn.SqlDbType});
                UPDATE [{Model.TableName}] 
                    SET {string.Join(", ", updateClauses)}
                --OUTPUT inserted.[{identityColumn.Name}] INTO @updatedIds";

                if (whereClauses is not null && whereClauses.Any())
                    query = $@"{query}
                WHERE {string.Join(" and ", whereClauses)}
                ";

                query = $@"{query} 
                   /* Options: 
                        return updated count:
                            select count(0) as updated from @updatedIds or select @@rowcount
                        return updatedIds
                            select id from @updatedIds
                        return all updated entties:
                            {selectClause} WHERE [{identityColumn.Name}] in (select [{identityColumn.Name}] from @updatedIds)
                        for performance just select the row count:
                    */
                    SELECT @@ROWCOUNT as UpdateCount";



                var combinedParam = new DynamicParameters();
                updateDict.ToList().ForEach(x => combinedParam.Add(x.Key, x.Value, Model.GetDbTypeForProperty(x.Key)));
                whereDict?.ToList().ForEach(x => combinedParam.Add(x.Key, x.Value, Model.GetDbTypeForProperty(x.Key)));



                using (var conn = new SqlConnection(connectionString))
                {
                    var result = await conn.QueryFirstAsync<int>(query, combinedParam);
                    return result;
                }
            }
            //);
        }


        public async Task<int> Update(T entity)
        {
            //return await Transient.RetryWithTimeout(async () =>
            {
                if (entity is null)
                    return 0;

                var updateClauses = Model.Columns.Where(x => !x.IsIdentity).Select(x => $"[{Model.TryGetColumnNameByProperty(x.Name)}] = @{x.PropertyName}");

                using (var conn = new SqlConnection(connectionString))
                {
                    var id = ((dynamic)entity).Id;
                    var query = $@"
                    UPDATE [{Model.TableName}]
                        SET {string.Join(", ", updateClauses)}
                    WHERE {idWhereClause}
                        
                    SELECT @@ROWCOUNT as UpdateCount
                    ";
                    return await conn.QueryFirstAsync<int>(query, entity);
                }
            }
            //);
        }

    }
    public static class SqlConnectionExtensions
    {
        public static async Task<TKey?> InsertAsync<TEntity, TKey>(this DbConnection connection, TEntity entity, DbTransaction? transaction = null)
        {
            return await Transient.RetryWithTimeout(async () =>
            {
                var query = TypeHelper.GetInsertCommand<TKey>();
                var result = await connection.ExecuteScalarAsync<TKey>(query, entity, transaction: transaction);
                return result;
            });
        }
    }
    public class TypeHelper
    {
        public static string GetInsertCommand<T>()
        {
            return string.Empty;
        }
    }
    public class Transient
    {
        public static async Task<T> RetryWithTimeout<T>(Func<Task<T>> getter)
        {
            try
            {
                return await getter();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] Error: {ex}");
                return default;
            }
        }

        public static T RetryWithTimeout<T>(Func<T> getter)
        {
            try
            { return getter(); }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] Error: {ex}");
                return default;
            }
        }
    }
}
