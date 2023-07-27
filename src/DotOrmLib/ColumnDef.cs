using System.ComponentModel.DataAnnotations.Schema;
using System.Data;


public class ColumnDef : ColumnAttribute
{

    public ColumnDef(string name) : base(name) { }
    public SqlDbType SqlDbType { get; set; }
    public bool IsIdentity { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsUnique { get; set; }
    public bool IsNullable { get; set; }
    public int MaxLength { get; set; }
    public string? DefaultDbValue { get; set; }
}
public class ForeignKeyDef : ForeignKeyAttribute
{
    public ForeignKeyDef(string name) : base(name) { }
    public string Table { get; set; }
    public string Column { get; set; }
}
