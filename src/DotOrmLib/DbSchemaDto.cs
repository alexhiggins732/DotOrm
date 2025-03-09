using DotOrmLib.Sql;
using System.Data;

namespace DotOrmLib
{
    public class DbSchemaDto
    {
        public List<SqlTableDto> Tables { get; set; } = new List<SqlTableDto>();
        public string DbName { get; set; } = null!;
        public string ClassName { get; set; } = null!;
        public static DbSchemaDto Create(SqlDbSchema def)
        {
            var result = new DbSchemaDto();
            result.DbName = def.DbName;
            result.ClassName = def.ClassName;
            foreach (var table in def.Tables)
            {
                result.Tables.Add(SqlTableDto.Create(table));
            }
            return result;
        }
    }

    public class SqlTableDto
    {
        public string TableName { get; set; } = null!;


        public string ClassName { get; set; } = null!;

        public ColumnCollectionDto Columns { get; set; } = new ColumnCollectionDto();
        public List<SqlForeignKeyDto> ForeignKeys { get; set; } = new List<SqlForeignKeyDto>();
        public List<SqlIndexDto> Indexes { get; set; } = new List<SqlIndexDto>();

        public static SqlTableDto Create(SqlTableDef table)
        {
            var result = new SqlTableDto()
            {
                TableName = table.TableName,
                ClassName = table.ClassName
            };

            foreach (var column in table.Columns)
            {
                result.Columns.Columns.Add(new SqlColumnDto
                {
                    ColumnName = column.Name, //column.ColumnName,
                    PropertyName = column.PropertyName,
                    PropertyType = SqlTypeMapper.MapSqlTypeToClrAlias(column.SqlDbType), //column.CSharpType.ToString(),// column.PropertyType,
                    SqlDataType = column.SqlDbType,
                    IsPrimaryKey = column.IsPrimaryKey,
                    IsForeignKey = column.IsForeignKey,
                    IsNullable = column.IsNullable,
                    IsIdentity = column.IsIdentity,
                    //IsComputed = column.IsComputed,
                    //IsRowVersion = column.IsRowVersion,
                    IsUnique = column.IsUnique,
                    //IsIndexed = column.IsIndexed,
                    //IsFullTextIndexed = column.IsFullTextIndexed,
                    //IsSpatial = column.IsSpatial,
                    //IsEncrypted = column.IsEncrypted,
                    //IsHidden = column.IsHidden,
                    //IsRowGuid = column.IsRowGuid,
                    //IsTimestamp = column.IsTimestamp
                    MaxLength = column.MaxLength,
                    /* this needs rework to support multiple foreign keys */
                    ForeignKeyConstraintName = column.ForeignKeyConstraintName,
                    ForeignKeyTableName = column.ForeignKeyTableName,
                    ForeignKeyColumnName = column.ForeignKeyColumnName,
                    DefaultDbValue = column.DefaultDbValue,
                });
            }

            foreach (var fk in table.ForeignKeys)
            {
                result.ForeignKeys.Add(new SqlForeignKeyDto
                {
                    //ForeignKeyName = fk.ForeignKeyName,
                    //TableName = fk.TableName,
                    //ColumnName = fk.ColumnName,
                    //RefTableName = fk.RefTableName,
                    //RefColumnName = fk.RefColumnName

                    ForeignKeyName = fk.ConstraintName,
                    TableName = fk.FromTableName,
                    ColumnName = fk.FromColumnName,
                    RefTableName = fk.ToTableName,
                    RefColumnName = fk.ToColumnName,
                });
            }

            foreach (var index in table.Indexes)
            {
             
                result.Indexes.Add(new SqlIndexDto
                {
                    IndexName = index.IndexName,
                    //TableName = index.TableName,
                    //ColumnName = index.ColumnName,
                    IsUnique = index.IsUnique,
                    //IsClustered = index.IsClustered,
                    //IsPrimaryKey = index.IsPrimaryKey,
                    //IsUniqueConstraint = index.IsUniqueConstraint,
                    //IsFullText = index.IsFullText,
                    //IsSpatial = index.IsSpatial,
                    //IsFiltered = index.IsFiltered,
                    //IsIgnored = index.IsIgnored,
                    Columns = index.Columns.ToList()
                });
            }


            return result;
        }
    }
    public class ColumnCollectionDto
    {
        public List<SqlColumnDto> Columns { get; set; } = new List<SqlColumnDto>();
    }

    public class SqlColumnDto
    {
        public string ColumnName { get; set; } = null!;
        public string PropertyName { get; set; } = null!;
        public string PropertyType { get; set; } = null!;
        public SqlDbType SqlDataType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsComputed { get; set; }
        public bool IsRowVersion { get; set; }
        public bool IsUnique { get; set; }
        public bool IsIndexed { get; set; }
        public bool IsFullTextIndexed { get; set; }
        public bool IsSpatial { get; set; }
        public bool IsEncrypted { get; set; }
        public bool IsHidden { get; set; }
        public bool IsRowGuid { get; set; }
        public bool IsTimestamp { get; set; }

       
        public int MaxLength { get; set; }
    
        public string ForeignKeyConstraintName { get; set; }
        public string ForeignKeyTableName { get; set; }
        public string ForeignKeyColumnName { get; set; }
        public string DefaultDbValue { get; internal set; }
        public int ColumnIndex { get; internal set; }

    }

    public class SqlForeignKeyDto
    {
        public string ForeignKeyName { get; set; } = null!;
        public string TableName { get; set; } = null!;
        public string ColumnName { get; set; } = null!;
        public string RefTableName { get; set; } = null!;
        public string RefColumnName { get; set; } = null!;
    }

    public class SqlIndexDto
    {
        public string IndexName { get; set; } = null!;
        public string TableName { get; set; } = null!;
        public string ColumnName { get; set; } = null!;
        public bool IsUnique { get; set; }
        public bool IsClustered { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUniqueConstraint { get; set; }
        public bool IsFullText { get; set; }
        public bool IsSpatial { get; set; }
        public bool IsFiltered { get; set; }
        public bool IsIgnored { get; set; }
        public List<string> Columns { get;  set; }
    }

}
