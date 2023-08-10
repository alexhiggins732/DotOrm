using DotOrmLib.Sql;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DotOrmLib
{
    public class DbModel
    {
        public List<DatabaseModel> Databases { get; set; } = new();
    }
    public class DatabaseModel
    {
        public string Name { get; set; }
        public List<EntityModel> entityModels = new();
    }
    public class EntityModel
    {
        public string Name { get; set; }
        public IncludeModels Includes { get; set; } = new();
    }
    public class IncludeModels
    {
        public List<EntityModel> Composites { get; set; } = new();
        public Dictionary<string, EntityModel> Collections = new();
    }

    namespace Sql
    {
        using Dapper;
        using Humanizer;
        using Microsoft.Data.SqlClient;
        using System;
        using System.CodeDom.Compiler;
        using System.Collections;
        using System.Collections.Concurrent;
        using System.Collections.Generic;
        using System.ComponentModel;
        using System.ComponentModel.DataAnnotations;
        using System.ComponentModel.DataAnnotations.Schema;
        using System.Data;
        using System.Reflection;
        using System.Runtime.InteropServices;
        using System.Text;
        using System.Text.RegularExpressions;

        public class ModelFactory
        {
            public static ConcurrentDictionary<string, SqlTableDef> schemas;
            public static ConcurrentDictionary<Type, string> schemaMap;
            public static ConcurrentDictionary<PropertyInfo, ColumnDef?> columnMap;
            public static ConcurrentDictionary<PropertyInfo, ForeignKeyDef?> fkMap;
            static ModelFactory()
            {
                schemas = new();
                schemaMap = new();
                columnMap = new();
                fkMap = new();
            }

            public static bool ContainsTableName<T>()
                => schemaMap.ContainsKey(typeof(T));

            public static string GetTableName<T>()
            {
                var tableName = schemaMap.GetOrAdd(typeof(T), t => typeof(T).GetCustomAttributes(true)
                    .OfType<TableAttribute>()
                    .Select(attr => attr.Name)
                    .FirstOrDefault() ?? typeof(T).Name);
                return tableName;
            }
            public static List<TAtrribute> GetCustomAttributes<T, TAtrribute>()
            {
                return typeof(T).GetCustomAttributes(true)
                   .OfType<TAtrribute>()
                   .ToList();
            }

            public static ColumnDef? GetColumnDef(PropertyInfo prop)
            {
                return columnMap.GetOrAdd(prop, (x) => GetCustomAttribute<ColumnDef>(prop));
            }
            public static ForeignKeyDef? GetFkDef(PropertyInfo prop)
            {
                return fkMap.GetOrAdd(prop, (x) => GetCustomAttribute<ForeignKeyDef>(prop));
            }

            public static TAttribute? GetCustomAttribute<TAttribute>(PropertyInfo prop)
            {
                return prop.GetCustomAttributes(true)
                           .OfType<TAttribute>()
                           .FirstOrDefault();
            }

            public static bool ContainsModel<T>()
              => ContainsTableName<T>() && ContainsModel(GetTableName<T>());

            public static bool ContainsModel(string tableName)
                => schemas.ContainsKey(tableName);

            public static SqlTableDef GetSchema<T>()
            {
                return schemas.GetOrAdd(GetTableName<T>(), (t) => GetModelDefinition<T>());
            }

            private static SqlTableDef GetModelDefinition<T>()
            {
                var tableAtt = GetCustomAttributes<T, TableAttribute>().FirstOrDefault();
                if (tableAtt != null)
                {
                    // todo implement type cache
                    var props = typeof(T).GetProperties().ToList();
                    var colDict = props.ToDictionary(x => x, x => GetColumnDef(x));
                    var fkDict = props.ToDictionary(x => x, x => GetFkDef(x));
                    var result = new SqlTableDef()
                    {
                        TableName = tableAtt.Name
                    };
                    int idx = 0;
                    foreach (var kvp in colDict)
                    {
                        idx++;
                        var def = kvp.Value;
                        var prop = kvp.Key;
                        var propType = kvp.Key.PropertyType;
                        var fk = fkDict[kvp.Key];
                        SqlColumnDef columnDef = new()
                        {
                            Name = def?.Name ?? prop.Name,
                            IsNullable = def?.IsNullable ?? propType.IsNullable(),
                            IsIdentity = def?.IsIdentity ?? false,
                            IsPrimaryKey = def?.IsPrimaryKey ?? false,
                            IsForeignKey = fk?.Table is not null,
                            IsUnique = def?.IsUnique ?? false,
                            SqlDbType = def?.SqlDbType ?? ModelBuilder.GetSqlDbType(propType),
                            MaxLength = def?.MaxLength ?? 0,
                            ForeignKeyTableName = fk?.Table ?? string.Empty,
                            ForeignKeyColumnName = fk?.Column ?? string.Empty,
                            ForeignKeyConstraintName = fk?.Name ?? string.Empty,
                            DefaultDbValue = def?.DefaultDbValue,
                            ColumnIndex = def?.Order ?? idx,
                        };
                        columnDef.CSharpType = ModelBuilder.GetClrType(columnDef);
                    }
                }
                return
                    ModelBuilder.GetTableDefinition<T>();
            }

            public static void Add(string tableName, SqlTableDef table)
                => schemas[tableName] = table;



        }
        public class ModelBuilder
        {
            public static SqlDbSchema GetDbSchema(string dbName)
            {
                return GetDbSchema(ConnectionStringProvider.Create().ConnectionString, dbName);
            }

            private static SqlDbSchema GetDbSchema(string connectionString, string dbName)
            {
                var def = new SqlDbSchema(dbName);
                List<DbSchemaModel> columnModels = null!;
                using (var conn = new SqlConnection(connectionString))
                {
                    var query = queryGetAllTableSchemasQuery();
                    var start = DateTime.Now;
                    var message = $"[{start}] Getting DbSchema [{dbName}]";
                    Console.Title = message;
                    Console.WriteLine(message);
                    columnModels = conn.Query<DbSchemaModel>(query, commandTimeout: 60).ToList();
                    message = $"[{DateTime.Now}] Retrieved DbSchema [{dbName}] in {DateTime.Now.Subtract(start)}";
                    Console.Title = message;
                    Console.WriteLine(message);

                }
                var lookup = columnModels.ToLookup(x => x.TableName);
                foreach (var grp in lookup)
                {
                    var tableDef = new SqlTableDef();
                    tableDef.TableName = grp.Key;

                    var columnDefs = grp.Select(x => x.ToSqlColumnDef());
                    tableDef.Columns.AddRange(columnDefs.OrderBy(x => x.ColumnIndex));
                    def.Tables.Add(tableDef);
                }
                return def;
            }


            static string queryGetAllTableSchemasQuery()
            {
                var result = $@"
IF OBJECT_ID('tempdb..#dbModel') IS NOT NULL
    DROP TABLE #dbModel;


create table #DbModel
(
	Id int identity(1,1) not null Primary Key,
	TableName nvarchar(128) not null,
	ColumnName nvarchar(128) not null,
	IsNullable bit not null,
	IsIdentity bit not null,
	IsPrimaryKey bit not null,
	IsForeignKey bit not null,
	IsUnique bit not null,
	DataType nvarchar(128) not null,
	MaxLength int not null,
	ForeignKeyTableName nvarchar(128) not null,
	ForeignKeyColumnName nvarchar(128) not null,
	ForeignKeyName nvarchar(128) not null,
	ColumnDefault nvarchar(max),
	ColumnIndex int not null
);


WITH keys AS 
(
	select * from (
		SELECT
			ku.CONSTRAINT_NAME,
			ku.TABLE_NAME,
			ku.COLUMN_NAME,
			CONSTRAINT_TYPE,
			refTable.name as FK_TABLE_NAME,
			refCol.name as FK_COLUMN_NAME,
			ROW_NUMBER() OVER (PARTITION BY ku.TABLE_NAME, ku.COLUMN_NAME, CONSTRAINT_TYPE ORDER BY ku.CONSTRAINT_NAME) AS RowNum
		FROM
			[INFORMATION_SCHEMA].[KEY_COLUMN_USAGE] ku
		JOIN
			[INFORMATION_SCHEMA].[TABLE_CONSTRAINTS] tc ON ku.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
		left join sys.tables srctable on ku.table_name=srctable.Name
		left join sys.columns srccol on srccol.object_id=srctable.object_id and ku.COLUMN_NAME = srccol.name
		left join sys.foreign_key_columns k on k.parent_object_id=srctable.object_id and k.parent_column_id=srccol.column_id 
		left join sys.columns refCol on k.referenced_object_id=refCol.object_id and k.referenced_column_id = refCol.column_id
		left join sys.tables refTable on refCol.object_id= refTable.object_id
		) a where rownum=1
)
insert into #DbModel(TableName,
	ColumnName,
	IsNullable,
	IsIdentity,
	IsPrimaryKey,
	IsForeignKey,
	IsUnique,
	DataType,
	MaxLength,
	ForeignKeyTableName,
	ForeignKeyColumnName,
	ForeignKeyName,
	ColumnDefault,
	ColumnIndex)
select 
								Isnull(t.name,'') as TableName,
                                IsNull(c.Name,'') as ColumnName,
                                cast(IsNull(c.IS_NULLABLE, 0) as Bit) as IsNullable,
                                cast(IsNull(c.is_identity, 0) as Bit) as IsIdentity,
								cast((case when pk.CONSTRAINT_NAME is null then 0 else 1 end) as bit) as IsPrimaryKey,
								cast((case when fk.CONSTRAINT_NAME is null then 0 else 1 end) as bit) as IsForeignKey,
								cast((case when un.CONSTRAINT_NAME is null then 0 else 1 end) as bit) as IsUnique,
                                ISNULL(TYPE_NAME(c.system_type_id), t.name)  AS DataType,
                                IsNull(COLUMNPROPERTY(t.object_id, c.name, 'charmaxlen'), 0) as MaxLength,
                                IsNull(fk.FK_TABLE_NAME,'') AS ForeignKeyTableName,
                                IsNull(fk.FK_COLUMN_NAME,'') AS ForeignKeyColumnName,
								ISNULL(fk.CONSTRAINT_NAME, '') as ForeignKeyName,
                                OBJECT_DEFINITION(c.default_object_id) AS ColumnDefault,
                                C.column_id as ColumnIndex			
from
	sys.columns c join sys.tables t on c.object_id=t.object_id
	left join keys pk on t.name= pk.table_name and c.name= pk.COLUMN_NAME and pk.CONSTRAINT_TYPE='PRIMARY KEY'
	left join keys un on t.name= un.table_name and c.name= un.COLUMN_NAME and un.CONSTRAINT_TYPE='UNIQUE'
	left join keys fk on t.name= fk.table_name and c.name= fk.COLUMN_NAME and fk.CONSTRAINT_TYPE='FOREIGN KEY'
-- order by t.name, c.column_id

select * from #DbModel order by TableName, ColumnName, ColumnIndex
";
                return result;
            }
            public static SqlDbSchema GetDbSchemaOld(string connectionString, string dbName)
            {
                var tables = GetDbTables(connectionString, dbName);
                var def = new SqlDbSchema(dbName);
                int count = 0;
                foreach (var table in tables)
                {
                    count++;
                    Console.WriteLine($"[{DateTime.Now}] Generating model {count} of {tables.Count}: {table}");
                    var tableDef = ModelBuilder.GetTableDefinition(connectionString, table);
                    def.Tables.Add(tableDef);
                }
                return def;

            }


            private static List<string> GetDbTables(string connectionString, string dbName)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    var tables = connection.Query<string>($"select name from [{dbName}].sys.tables").ToList();
                    return tables;
                }
            }
            public static SqlTableDef GetTableDefinition(string tableName)
                => GetTableDefinition(ConnectionStringProvider.Create().ConnectionString, tableName);

            private static SqlTableDef GetTableDefinition(string connectionString, string tableName)
            {

                SqlTableDef tableDef = new SqlTableDef() { TableName = tableName };

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Get the table columns information from the database
                    var query = $@"select 
                                c.Name	as COLUMN_NAME,
                                c.IS_NULLABLE,
                                c.is_identity as IsIdentity,
                                --IsNull(COLUMNPROPERTY(t.object_id,c.name, 'IsPrimaryKey'), 0)  AS IsPrimaryKey,
								case 
									when exists (select CONSTRAINT_TYPE from
										[INFORMATION_SCHEMA].[KEY_COLUMN_USAGE] ku
											join [INFORMATION_SCHEMA].[TABLE_CONSTRAINTS] tc on ku.CONSTRAINT_NAME=tc.CONSTRAINT_NAME
										where CONSTRAINT_TYPE ='PRIMARY KEY' and ku.TABLE_NAME= t.name and ku.COLUMN_NAME = c.name)  then 1 
									else 
										0
									end 
								as IsPrimaryKey,
								case when exists (select CONSTRAINT_TYPE from
										[INFORMATION_SCHEMA].[KEY_COLUMN_USAGE] ku
											join [INFORMATION_SCHEMA].[TABLE_CONSTRAINTS] tc on ku.CONSTRAINT_NAME=tc.CONSTRAINT_NAME
										where CONSTRAINT_TYPE ='FOREIGN KEY' and ku.TABLE_NAME= t.name and ku.COLUMN_NAME = c.name)  then 1 
										else 
											0
									end 
								AS IsForeignKey,

                                case when exists (select CONSTRAINT_TYPE from
										[INFORMATION_SCHEMA].[KEY_COLUMN_USAGE] ku
											join [INFORMATION_SCHEMA].[TABLE_CONSTRAINTS] tc on ku.CONSTRAINT_NAME=tc.CONSTRAINT_NAME
										where CONSTRAINT_TYPE ='UNIQUE' and ku.TABLE_NAME= t.name and ku.COLUMN_NAME = c.name)  then 1 
										else 
											0
									end 
								AS IsUnique,
                                 ISNULL(TYPE_NAME(c.system_type_id), t.name)  AS DATA_TYPE,
                                IsNull(COLUMNPROPERTY(t.object_id, c.name, 'charmaxlen'), 0) as CHARACTER_MAXIMUM_LENGTH,
                                IsNull(refTable.name,'') AS ForeignKeyTableName,
                                IsNull(refCol.name,'') AS ForeignKeyColumnName,
                                OBJECT_DEFINITION(c.default_object_id) AS COLUMN_DEFAULT,
								IsNull((select top 1 ku.CONSTRAINT_NAME from
										[INFORMATION_SCHEMA].[KEY_COLUMN_USAGE] ku
											join [INFORMATION_SCHEMA].[TABLE_CONSTRAINTS] tc on ku.CONSTRAINT_NAME=tc.CONSTRAINT_NAME
										where CONSTRAINT_TYPE ='FOREIGN KEY' and ku.TABLE_NAME= t.name and ku.COLUMN_NAME = c.name
										), '') as ForeignKeyName,
                                C.column_id as ColumnIndex

from


sys.columns c 
join sys.tables t on c.object_id=t.object_id
--join INFORMATION_SCHEMA.COLUMNS cs on c.name= cs.COLUMN_NAME and cs.TABLE_NAME= t.name
left join sys.foreign_key_columns k on k.parent_object_id=t.object_id and k.parent_column_id=c.column_id 
left join sys.columns refCol on k.referenced_object_id=refCol.object_id and k.referenced_column_id = refCol.column_id
left join sys.tables refTable on refCol.object_id= refTable.object_id
--left join [INFORMATION_SCHEMA].[KEY_COLUMN_USAGE] ku on t.name= ku.TABLE_NAME and c.name= ku.COLUMN_NAME 
--left join [INFORMATION_SCHEMA].[TABLE_CONSTRAINTS] tc on ku.CONSTRAINT_NAME=tc.CONSTRAINT_NAME
   WHERE
                                t.name = @TableName";

                    SqlCommand command = new SqlCommand(
                        query,
                        connection
                    );
                    command.Parameters.AddWithValue("@TableName", tableName);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SqlColumnDef columnDef = new SqlColumnDef();
                            columnDef.Name = reader.GetString(0);
                            columnDef.IsNullable = reader.GetBoolean(1);
                            columnDef.IsIdentity = reader.GetBoolean(2);
                            columnDef.IsPrimaryKey = reader.GetInt32(3) > 0;
                            columnDef.IsForeignKey = reader.GetInt32(4) > 0;
                            columnDef.IsUnique = reader.GetInt32(5) > 0;
                            columnDef.SqlDbType = GetSqlDbType(reader.GetString(6));
                            columnDef.MaxLength = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
                            columnDef.ForeignKeyTableName = reader.GetString(8);
                            columnDef.ForeignKeyColumnName = reader.GetString(9);
                            columnDef.DefaultDbValue = reader.IsDBNull(10) ? null : reader.GetString(10);
                            columnDef.ForeignKeyConstraintName = reader.GetString(11);
                            columnDef.CSharpType = GetClrType(columnDef);
                            columnDef.IsForeignKey = !string.IsNullOrEmpty(columnDef.ForeignKeyTableName);
                            columnDef.ColumnIndex = reader.GetInt32(12);
                            tableDef.Columns.Add(columnDef);
                        }
                    }
                }

                ModelFactory.Add(tableName, tableDef);
                return tableDef;
            }

            public static Type GetClrType(SqlColumnDef columnDef)
            {
                return SqlTypeMapper.MapSqlTypeToClrType(columnDef.SqlDbType);
            }

            public static SqlDbType GetSqlDbType(string dbTypeName)
            {
                var parsed = Enum.TryParse<SqlDbType>(dbTypeName, true, out SqlDbType result);
                if (!parsed)
                {
                    switch (dbTypeName)
                    {
                        case "numeric":
                            result = SqlDbType.Decimal;
                            break;
                        default: throw new System.ArgumentException($"Requested value '{dbTypeName}' could not be parsed as {nameof(SqlDbType)} ");
                    }
                }
                return result;
            }

            public static SqlDbType GetSqlDbType(Type type)
            {
                return SqlTypeMapper.MapClrTypeToDbType(type);
            }


            public static SqlTableDef GetTableDefinition<T>()
                => GetTableDefinition<T>(ConnectionStringProvider.Create().ConnectionString);

            public static SqlTableDef GetTableDefinition<T>(string connectionString)
            {

                if (ModelFactory.ContainsModel<T>())
                    return ModelFactory.GetSchema<T>();

                return GetTableDefinition(connectionString, ModelFactory.GetTableName<T>());
            }
        }

        public class SqlDbSchema
        {
            public SqlDbSchema(string dbName)
            {
                DbName = dbName;
                this.ClassName = dbName.ToPropertyName();
            }

            public List<SqlTableDef> Tables { get; set; } = new List<SqlTableDef>();
            public string DbName { get; }
            public string ClassName { get; set; }

            public string BuildCsharpModels(string? @namespace = null)
            {
                @namespace = @namespace ?? $"DotOrmLib.Proxy.{ClassName}";
                var b = new StringBuilder();
                b.AppendLine("using System;");
                b.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
                b.AppendLine("using System.Data.Common;");
                b.AppendLine("using System.Data;");
                b.AppendLine("using System.Runtime.Serialization;");
                b.AppendLine("using System.ServiceModel;");
                b.AppendLine($"using {@namespace}.Models;");
                b.AppendLine($"using {@namespace}.Interfaces;");
                b.AppendLine();
                b.AppendLine();

                b.AppendLine($"namespace {@namespace}.Models");
                b.AppendLine($"{{");
                b.AppendLine();
                string indent = $"    ";
                var serviceBuilder = new StringBuilder();
                serviceBuilder.AppendLine($"namespace {@namespace}.Interfaces");
                serviceBuilder.AppendLine($"{{");
                foreach (var table in Tables)
                {
                    var def = table.GenerateCSharpModel();
                    b.AppendLine($"{def}\n");
                    serviceBuilder.AppendLine($"\n{indent}[ServiceContract(Namespace = \"http://{@namespace}\")]");
                    serviceBuilder.AppendLine($"{indent}public interface I{table.ClassName}Controller\n{indent}{indent}: IServiceController<{table.ClassName}>");
                    serviceBuilder.AppendLine($"{indent}{{\n{indent}}}");
                }
                b.AppendLine($"}}");
                b.AppendLine();
                b.AppendLine();
                serviceBuilder.AppendLine($"}}");
                b.AppendLine(serviceBuilder.ToString());
                return b.ToString();
            }
        }

        public class ColumnCollection : IEnumerable<SqlColumnDef>
        {
            private Dictionary<string, SqlColumnDef> columns;
            private ILookup<string, KeyValuePair<string, SqlColumnDef>> propertyLookup;
            private ILookup<string, KeyValuePair<string, SqlColumnDef>> nameLookup;
            private ILookup<bool, KeyValuePair<string, SqlColumnDef>> identityLookup;
            private ILookup<bool, KeyValuePair<string, SqlColumnDef>> keyLookup;

            public ColumnCollection()
            {
                columns = new(StringComparer.InvariantCultureIgnoreCase);
                propertyLookup = columns.ToLookup(x => x.Value.PropertyName);
                nameLookup = columns.ToLookup(x => x.Value.Name);
                identityLookup = columns.ToLookup(x => x.Value.IsIdentity);
                keyLookup = columns.ToLookup(x => x.Value.IsPrimaryKey);
            }
            void updateLookup()
            {
                propertyLookup = columns.ToLookup(x => x.Value.PropertyName);
                nameLookup = columns.ToLookup(x => x.Value.Name);
                identityLookup = columns.ToLookup(x => x.Value.IsIdentity);
                keyLookup = columns.ToLookup(x => x.Value.IsPrimaryKey);
            }
            public IEnumerator<SqlColumnDef> GetEnumerator()
            {
                return columns.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public SqlColumnDef? GetByPropertyName(string propertyName)
            {
                var props = propertyLookup[propertyName];
                return props.FirstOrDefault().Value;
            }
            public SqlColumnDef? GetByColumnName(string columnName)
            {
                var props = nameLookup[columnName];
                return props.FirstOrDefault().Value;
            }
            public SqlColumnDef? GetIdentityColumn()
            {
                var props = identityLookup[true];
                return props.FirstOrDefault().Value;
            }
            public IEnumerable<SqlColumnDef>? GetKeyColumns()
            {
                var props = keyLookup[true];
                return props.Select(x => x.Value);
            }

            public void Add(SqlColumnDef columnDef)
            {
                columns.Add(columnDef.Name, columnDef);
                updateLookup();
            }

            internal void AddRange(IEnumerable<SqlColumnDef> columnDefs)
            {
                foreach (var def in columnDefs)
                {
                    columns.Add(def.Name, def);
                }
                updateLookup();
            }
        }
        public class SqlTableDef
        {
            public string TableName { get; set; } = null!;

            private string className;
            public string ClassName
            {
                get
                {
                    return className ?? (className = TableName.ToPropertyName());
                }
                set { className = value; }
            }

            public ColumnCollection Columns { get; set; } = new ColumnCollection();
            public List<SqlForeignKey> ForeignKeys { get; set; } = new List<SqlForeignKey>();
            public List<SqlIndex> Indexes { get; set; } = new List<SqlIndex>();

            public string GenerateCSharpModel(string? @namespace = null, string? className = null)
            {
                @namespace = @namespace ?? "DotOrmLib.Proxy";
                className = className ?? ClassName;
                var b = new StringBuilder();
                string indent = $"    ";
                b.AppendLine($"{indent}[DataContract]");
                b.AppendLine($"{indent}[Table(\"{TableName}\")]");
                b.AppendLine($"{indent}public class {className}");
                b.AppendLine($"{indent}{{");


                foreach (var columnDef in Columns)
                {
                    b.AppendLine($"{indent}{indent}{columnDef.DataMemberAttributeString()}");
                    b.AppendLine($"{indent}{indent}{columnDef.ColumnAttributeString()}");
                    if (columnDef.IsForeignKey)
                        b.AppendLine($"{indent}{indent}{columnDef.ForeignKeyAttributeString()}");
                    b.Append($"{indent}{indent}public {columnDef.CSharpType.Alias()}");
                    if (columnDef.IsNullable) b.Append("?");
                    b.Append($" {columnDef.PropertyName} {{ get; set; }}");

                    if (!columnDef.CSharpType.IsValueType && !columnDef.IsNullable)
                        b.Append(" = null!;");
                    b.AppendLine();
                    b.AppendLine();
                }
                b.Length--;
                b.Length--;
                b.AppendLine($"{indent}}}");
                return b.ToString();
            }


            public SqlColumnDef? TryGetByProperty(string propertyName)
            {
                var column = Columns.GetByPropertyName(propertyName);
                return column;
            }
            public string? TryGetColumnNameByProperty(string propertyName)
            {
                var column = TryGetByProperty(propertyName);
                return column?.Name;
            }

            public SqlColumnDef? TryGetByColumnName(string columnName)
            {
                var column = Columns.GetByColumnName(columnName);
                return column;
            }
            public SqlColumnDef? TryGetIdentityColumn()
            {
                var column = Columns.GetIdentityColumn();
                return column;
            }
            public IEnumerable<SqlColumnDef>? TryGetKeyColumns()
            {
                var column = Columns.GetKeyColumns();
                return column;
            }

            public DbType? GetDbTypeForColumn(string columnName)
            {
                var column = Columns.GetByColumnName(columnName);
                return column?.SqlDbType.ToDbType();
            }

            public DbType? GetDbTypeForProperty(string propertyName)
            {
                var column = Columns.GetByPropertyName(propertyName);
                return column?.SqlDbType.ToDbType();
            }
        }

        public class SqlColumnDef
        {
            public string Name { get; set; }
            public string PropertyName => Name.ToPropertyName();
            public bool IsNullable { get; set; }
            public bool IsIdentity { get; set; }
            public bool IsPrimaryKey { get; set; }
            public bool IsForeignKey { get; set; }
            public bool IsUnique { get; set; }
            public SqlDbType SqlDbType { get; set; }
            public int MaxLength { get; set; }
            public Type CSharpType { get; set; }
            public string ForeignKeyConstraintName { get; set; }
            public string ForeignKeyTableName { get; set; }
            public string ForeignKeyColumnName { get; set; }
            public string DefaultDbValue { get; internal set; }
            public int ColumnIndex { get; internal set; }


            public string SqlColumnDefintionString()
            {
                var b = new StringBuilder();
                b.Append($"[{Name}] {SqlDbType}");
                if (b.Length != 0)
                    b.Append($" ({MaxLength})");
                if (IsNullable)
                    b.Append(" NULL");
                else
                    b.Append(" NOT NULL");
                return b.ToString();
            }

            public string ColumnAttributeString()
            {
                var b = new StringBuilder();
                b.Append($"[ColumnDef(\"{Name}\", Order = {ColumnIndex}");
                if (IsIdentity)
                    b.Append($", {nameof(IsIdentity)} = {IsIdentity.ToString().ToLower()}");
                if (IsPrimaryKey)
                    b.Append($", {nameof(IsPrimaryKey)} = {IsPrimaryKey.ToString().ToLower()}");
                b.Append($", {nameof(SqlDbType)} = {nameof(SqlDbType)}.{SqlDbType}");
                if (IsNullable)
                    b.Append($", {nameof(IsNullable)} = {IsNullable.ToString().ToLower()}");
                if (MaxLength != 0)
                    b.Append($", {nameof(MaxLength)} = {MaxLength}");
                if (IsUnique)
                    b.Append($", {nameof(IsUnique)} = {IsUnique.ToString().ToLower()}");
                if (DefaultDbValue is not null)
                    b.Append($", {nameof(DefaultDbValue)} = \"{DefaultDbValue}\"");
                b.Append(")]");


                return b.ToString();
            }

            public string DataMemberAttributeString()
            {
                var b = new StringBuilder();
                b.Append($"[DataMember(Order = {ColumnIndex})]");
                return b.ToString();
            }


            public string ForeignKeyAttributeString()
            {
                var b = new StringBuilder();
                if (IsForeignKey)
                    b.Append($"[ForeignKeyDef(\"{ForeignKeyConstraintName}\", Table = \"{ForeignKeyTableName}\", Column = \"{ForeignKeyColumnName}\")]");
                return b.ToString();
            }
        }

        public class SqlForeignKey
        {
            public string ConstraintName { get; set; }
            public string FromTableName { get; set; }
            public string FromColumnName { get; set; }
            public string ToTableName { get; set; }
            public string ToColumnName { get; set; }
        }

        public class SqlIndex
        {
            public string IndexName { get; set; }
            public bool IsUnique { get; set; }
            public List<string> Columns { get; set; } = new List<string>();
        }


        public class SqlTypeMapper
        {
            public static SqlDbType MapClrTypeToDbType(Type type)
            {
                var alias = type.Alias().TrimEnd('?');
                switch (alias)
                {
                    case "bool":
                        return SqlDbType.Bit;
                    case "byte":
                        return SqlDbType.TinyInt;
                    case "short":

                        return SqlDbType.SmallInt;
                    case "sbyte":
                    case "ushort":
                    case "int":
                        return SqlDbType.Int;
                    case "uint":
                    case "ulong":
                    case "long":
                        return SqlDbType.BigInt;
                    case "float":
                        return SqlDbType.Real;
                    case "double":
                        return SqlDbType.Float;
                    case "decimal":
                        return SqlDbType.Decimal;
                    case "byte[]":
                        return SqlDbType.VarBinary;
                    case "DateTime":
                        return SqlDbType.DateTime;
                    case "DateTimeOffset":
                        return SqlDbType.DateTimeOffset;
                    case "Guid":
                        return SqlDbType.UniqueIdentifier;
                    case "TimeSpan":
                    case "Time":
                        return SqlDbType.Time;
                    case "Date":
                        return SqlDbType.Date;
                    default:
                        return SqlDbType.VarChar;

                }
            }
            public static Type MapSqlTypeToClrType(SqlDbType sqlDbType, bool isNullable = false)
            {
                switch (sqlDbType)
                {
                    case SqlDbType.BigInt:
                        return isNullable ? typeof(long?) : typeof(long);
                    case SqlDbType.Binary:
                    case SqlDbType.Image:
                    case SqlDbType.Timestamp:
                    case SqlDbType.VarBinary:
                        return isNullable ? typeof(byte?[]) : typeof(byte[]);
                    case SqlDbType.Bit:
                        return isNullable ? typeof(bool?) : typeof(bool);
                    case SqlDbType.Char:
                    case SqlDbType.NChar:
                    case SqlDbType.NText:
                    case SqlDbType.NVarChar:
                    case SqlDbType.Text:
                    case SqlDbType.VarChar:
                    case SqlDbType.Xml:
                        return typeof(string);
                    case SqlDbType.Date:
                    case SqlDbType.DateTime:
                    case SqlDbType.DateTime2:
                    case SqlDbType.SmallDateTime:
                        return isNullable ? typeof(DateTime?) : typeof(DateTime);
                    case SqlDbType.DateTimeOffset:
                        return isNullable ? typeof(DateTimeOffset?) : typeof(DateTimeOffset);
                    case SqlDbType.Decimal:
                    case SqlDbType.Money:
                    case SqlDbType.SmallMoney:
                        return isNullable ? typeof(decimal?) : typeof(decimal);
                    case SqlDbType.Float:
                        return isNullable ? typeof(double?) : typeof(double);
                    case SqlDbType.Int:
                        return isNullable ? typeof(int?) : typeof(int);
                    case SqlDbType.Real:
                        return isNullable ? typeof(float?) : typeof(float);
                    case SqlDbType.SmallInt:
                        return isNullable ? typeof(short?) : typeof(short);
                    case SqlDbType.TinyInt:
                        return isNullable ? typeof(byte?) : typeof(byte);
                    case SqlDbType.UniqueIdentifier:
                        return isNullable ? typeof(Guid?) : typeof(Guid);
                    case SqlDbType.Time:
                        return isNullable ? typeof(TimeSpan?) : typeof(TimeSpan);

                    case SqlDbType.Structured:
                        return typeof(DataTable); // Custom Structured Data Type
                    case SqlDbType.Variant:

                        return typeof(object);
                    case SqlDbType.Udt:
                        return typeof(object); // Custom User-Defined Type
                    default:
                        return typeof(object); // Default to object type if not recognized
                }
            }
        }
        public static class Extensions
        {
            public static string ToPropertyName(this string dbColumnName)
            {
                var humanized = dbColumnName.Humanize();
                var result = humanized.Dehumanize().ToPascalCaseWithAcronyms();
                return result;
            }

            public static string ToPascalCaseWithAcronyms(this string value)
            {
                if (string.IsNullOrEmpty(value) || value.Length < 2)
                    return value;

                var b = new System.Text.StringBuilder();
                var last = value[0];
                b.Append(last);
                for (var i = 1; i < value.Length; i++)
                {

                    var c = value[i];
                    if (char.IsLower(c))
                        b.Append(c);
                    else // (char.IsUpper(c))
                    {
                        if (!char.IsUpper(last))
                            b.Append(c);
                        else //(char.IsUpper(last))
                        {
                            if (i + 1 >= value.Length || char.IsUpper(value[i + 1]))
                                b.Append(char.ToLower(c));
                            else
                                b.Append(c);
                        }
                    }
                    last = c;
                }



                return b.ToString();
            }
            public static bool IsNullableField(this PropertyInfo prop)
                => prop.PropertyType.IsNullable();


            public static bool IsNullable(this Type type)
            {

                // Check if the property's type is a nullable value type
                if (Nullable.GetUnderlyingType(type) != null)
                {
                    return true;
                }

                // Check if the property's type is a reference type
                return !type.IsValueType;

            }
            public static string Alias(this Type type)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // For nullable types, return the alias of the underlying type with "?"
                    var underlyingType = Nullable.GetUnderlyingType(type);
                    return $"{Alias(underlyingType)}?";
                }
                switch (type.FullName)
                {
                    case "System.Int32": return "int";
                    case "System.Double": return "double";
                    case "System.Decimal": return "decimal";
                    case "System.String": return "string";
                    case "System.Boolean": return "bool";
                    case "System.DateTime": return "DateTime";
                    case "System.Guid": return "Guid";
                    case "System.Byte": return "byte";
                    case "System.Char": return "char";
                    case "System.Single": return "float";
                    case "System.Int64": return "long";
                    case "System.Int16": return "short";
                    case "System.UInt32": return "uint";
                    case "System.UInt64": return "ulong";
                    case "System.UInt16": return "ushort";
                    case "System.SByte": return "sbyte";
                    // Add more aliases for other types as needed
                    default:
                        return type.Name;
                }
            }

            public static DbType ToDbType(this SqlDbType sqlDbType)
            {
                switch (sqlDbType)
                {

                    case SqlDbType.BigInt:
                        return DbType.Int64;

                    case SqlDbType.Binary:
                    case SqlDbType.VarBinary:
                    case SqlDbType.Image:
                    case SqlDbType.Timestamp:
                        return DbType.Binary;

                    case SqlDbType.Bit:
                        return DbType.Boolean;

                    case SqlDbType.Char:
                        return DbType.AnsiStringFixedLength;
                    case SqlDbType.NChar:
                        return DbType.StringFixedLength;

                    case SqlDbType.Date:
                    case SqlDbType.DateTime:
                    case SqlDbType.DateTime2:
                    case SqlDbType.SmallDateTime:
                        return DbType.DateTime;


                    case SqlDbType.DateTimeOffset:
                        return DbType.DateTimeOffset;

                    case SqlDbType.Decimal:
                        return DbType.Decimal;
                    case SqlDbType.Float:
                        return DbType.Double;

                    case SqlDbType.Int:
                        return DbType.Int32;

                    case SqlDbType.Money:
                    case SqlDbType.SmallMoney:
                        return DbType.Currency;

                    case SqlDbType.NText:
                    case SqlDbType.NVarChar:
                        return DbType.String;

                    case SqlDbType.Real:
                        return DbType.Single;

                    case SqlDbType.UniqueIdentifier:
                        return DbType.Guid;


                    case SqlDbType.SmallInt:
                        return DbType.Int16;


                    case SqlDbType.Text:
                    case SqlDbType.VarChar:
                        return DbType.AnsiString;

                    case SqlDbType.Time:
                        return DbType.Time;

                    case SqlDbType.TinyInt:
                        return DbType.Byte;

                    case SqlDbType.Structured:
                    case SqlDbType.Variant:
                    case SqlDbType.Udt:

                        return DbType.Object;

                    case SqlDbType.Xml:
                        return DbType.Xml;


                    default:
                        throw new ArgumentOutOfRangeException(nameof(sqlDbType), sqlDbType, "Unsupported SqlDbType");
                }
            }

        }
    }

    [DataContract]
    public class DbSchemaModel
    {
        public int Id { get; set; }
        public string TableName { get; set; } = null!;
        public string ColumnName { get; set; } = null!;
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsUnique { get; set; }
        public string DataType { get; set; } = null!;
        public int MaxLength { get; set; }
        public string ForeignKeyTableName { get; set; } = null!;
        public string ForeignKeyColumnName { get; set; } = null!;
        public string ForeignKeyName { get; set; } = null!;
        public string? ColumnDefault { get; set; }
        public int ColumnIndex { get; set; }

        public SqlColumnDef ToSqlColumnDef()
        {
            SqlColumnDef columnDef = new()
            {
                Name = ColumnName,
                IsNullable = IsNullable,
                IsIdentity = IsIdentity,
                IsPrimaryKey = IsPrimaryKey,
                IsForeignKey = IsForeignKey,
                IsUnique = IsUnique,
                SqlDbType = ModelBuilder.GetSqlDbType(DataType),
                MaxLength = MaxLength,
                ForeignKeyTableName = ForeignKeyTableName,
                ForeignKeyColumnName = ForeignKeyColumnName,
                ForeignKeyConstraintName = ForeignKeyName,
                DefaultDbValue = ColumnDefault,
                ColumnIndex = ColumnIndex,
            };
            columnDef.CSharpType = ModelBuilder.GetClrType(columnDef);
            return columnDef;
        }
    }

}
