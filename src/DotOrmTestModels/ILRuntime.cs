using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data;
using System.Runtime.Serialization;
using System.ServiceModel;


namespace DotOrmLib.Proxy;



[DataContract]
[Table("FlowControl")]
public class FlowControl
{
    [DataMember(Order = 1)]
    [ColumnDef("Id", Order = 1, IsIdentity = true, IsPrimaryKey = true, SqlDbType = SqlDbType.Int)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    [ColumnDef("Name", Order = 2, SqlDbType = SqlDbType.VarChar, MaxLength = 100, IsUnique = true)]
    public string Name { get; set; } = null!;

    [DataMember(Order = 3)]
    [ColumnDef("Value", Order = 3, SqlDbType = SqlDbType.Int)]
    public int Value { get; set; }

    [DataMember(Order = 4)]
    [ColumnDef("Description", Order = 4, SqlDbType = SqlDbType.VarChar, MaxLength = 500)]
    public string Description { get; set; } = null!;

}



[DataContract]
[Table("MetaSourceCode")]
public class MetaSourceCode
{
    [DataMember(Order = 1)]
    [ColumnDef("Id", Order = 1, IsIdentity = true, IsPrimaryKey = true, SqlDbType = SqlDbType.Int)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    [ColumnDef("Name", Order = 2, SqlDbType = SqlDbType.VarChar, MaxLength = 100)]
    public string Name { get; set; } = null!;

    [DataMember(Order = 3)]
    [ColumnDef("AssemblyQualifiedName", Order = 3, SqlDbType = SqlDbType.VarChar, MaxLength = 300)]
    public string AssemblyQualifiedName { get; set; } = null!;

    [DataMember(Order = 4)]
    [ColumnDef("CodeBase", Order = 4, SqlDbType = SqlDbType.VarChar, MaxLength = 500)]
    public string CodeBase { get; set; } = null!;

    [DataMember(Order = 5)]
    [ColumnDef("SourceCode", Order = 5, SqlDbType = SqlDbType.VarChar, MaxLength = -1)]
    public string SourceCode { get; set; } = null!;

}



[DataContract]
[Table("OpCode")]
public class OpCode
{
    [DataMember(Order = 1)]
    [ColumnDef("Id", Order = 1, IsIdentity = true, IsPrimaryKey = true, SqlDbType = SqlDbType.Int)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    [ColumnDef("ClrName", Order = 2, SqlDbType = SqlDbType.VarChar, MaxLength = 100, IsUnique = true)]
    public string ClrName { get; set; } = null!;

    [DataMember(Order = 3)]
    [ColumnDef("Name", Order = 3, SqlDbType = SqlDbType.VarChar, MaxLength = 100, IsUnique = true)]
    public string Name { get; set; } = null!;

    [DataMember(Order = 4)]
    [ColumnDef("Value", Order = 4, SqlDbType = SqlDbType.Int, IsUnique = true)]
    public int Value { get; set; }

    [DataMember(Order = 5)]
    [ColumnDef("Size", Order = 5, SqlDbType = SqlDbType.Int)]
    public int Size { get; set; }

    [DataMember(Order = 6)]
    [ColumnDef("FlowControlId", Order = 6, SqlDbType = SqlDbType.Int)]
    [ForeignKeyDef("FK_OpCode_FlowControlId", Table = "FlowControl", Column = "Id")]
    public int FlowControlId { get; set; }

    [DataMember(Order = 7)]
    [ColumnDef("OpCodeTypeId", Order = 7, SqlDbType = SqlDbType.Int)]
    [ForeignKeyDef("FK_OpCode_OpCodeTypeId", Table = "OpCodeType", Column = "Id")]
    public int OpCodeTypeId { get; set; }

    [DataMember(Order = 8)]
    [ColumnDef("OperandTypeId", Order = 8, SqlDbType = SqlDbType.Int)]
    [ForeignKeyDef("FK_OpCode_OperandTypeId", Table = "OperandType", Column = "Id")]
    public int OperandTypeId { get; set; }

    [DataMember(Order = 9)]
    [ColumnDef("StackBehaviourPopId", Order = 9, SqlDbType = SqlDbType.Int)]
    [ForeignKeyDef("FK_OpCode_StackBehaviourPopId", Table = "StackBehaviour", Column = "Id")]
    public int StackBehaviourPopId { get; set; }

    [DataMember(Order = 10)]
    [ColumnDef("StackBehaviourPushId", Order = 10, SqlDbType = SqlDbType.Int)]
    [ForeignKeyDef("FK_OpCode_StackBehaviourPushId", Table = "StackBehaviour", Column = "Id")]
    public int StackBehaviourPushId { get; set; }

    [DataMember(Order = 11)]
    [ColumnDef("Description", Order = 11, SqlDbType = SqlDbType.VarChar, MaxLength = 500)]
    public string Description { get; set; } = null!;

}



[DataContract]
[Table("OpCodeType")]
public class OpCodeType
{
    [DataMember(Order = 1)]
    [ColumnDef("Id", Order = 1, IsIdentity = true, IsPrimaryKey = true, SqlDbType = SqlDbType.Int)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    [ColumnDef("Name", Order = 2, SqlDbType = SqlDbType.VarChar, MaxLength = 100, IsUnique = true)]
    public string Name { get; set; } = null!;

    [DataMember(Order = 3)]
    [ColumnDef("Value", Order = 3, SqlDbType = SqlDbType.Int)]
    public int Value { get; set; }

    [DataMember(Order = 4)]
    [ColumnDef("Description", Order = 4, SqlDbType = SqlDbType.VarChar, MaxLength = 500)]
    public string Description { get; set; } = null!;

}



[DataContract]
[Table("OperandType")]
public class OperandType
{
    [DataMember(Order = 1)]
    [ColumnDef("Id", Order = 1, IsIdentity = true, IsPrimaryKey = true, SqlDbType = SqlDbType.Int)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    [ColumnDef("Name", Order = 2, SqlDbType = SqlDbType.VarChar, MaxLength = 100, IsUnique = true)]
    public string Name { get; set; } = null!;

    [DataMember(Order = 3)]
    [ColumnDef("Value", Order = 3, SqlDbType = SqlDbType.Int)]
    public int Value { get; set; }

    [DataMember(Order = 4)]
    [ColumnDef("Description", Order = 4, SqlDbType = SqlDbType.VarChar, MaxLength = 500)]
    public string Description { get; set; } = null!;

    [DataMember(Order = 5)]
    [ColumnDef("ByteSize", Order = 5, SqlDbType = SqlDbType.Int)]
    public int ByteSize { get; set; }

    [DataMember(Order = 6)]
    [ColumnDef("BitSize", Order = 6, SqlDbType = SqlDbType.Int)]
    public int BitSize { get; set; }

    [DataMember(Order = 7)]
    [ColumnDef("IsFloatingPoint", Order = 7, SqlDbType = SqlDbType.Bit)]
    public bool IsFloatingPoint { get; set; }

    [DataMember(Order = 8)]
    [ColumnDef("SystemType", Order = 8, SqlDbType = SqlDbType.VarChar, MaxLength = 20)]
    public string SystemType { get; set; } = null!;

}



[DataContract]
[Table("StackBehaviour")]
public class StackBehaviour
{
    [DataMember(Order = 1)]
    [ColumnDef("Id", Order = 1, IsIdentity = true, IsPrimaryKey = true, SqlDbType = SqlDbType.Int)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    [ColumnDef("Name", Order = 2, SqlDbType = SqlDbType.VarChar, MaxLength = 100, IsUnique = true)]
    public string Name { get; set; } = null!;

    [DataMember(Order = 3)]
    [ColumnDef("Value", Order = 3, SqlDbType = SqlDbType.Int)]
    public int Value { get; set; }

    [DataMember(Order = 4)]
    [ColumnDef("Description", Order = 4, SqlDbType = SqlDbType.VarChar, MaxLength = 500)]
    public string Description { get; set; } = null!;

    [DataMember(Order = 5)]
    [ColumnDef("PopCount", Order = 5, SqlDbType = SqlDbType.Int)]
    public int PopCount { get; set; }

    [DataMember(Order = 6)]
    [ColumnDef("PopType0", Order = 6, SqlDbType = SqlDbType.VarChar, MaxLength = 3)]
    public string PopType0 { get; set; } = null!;

    [DataMember(Order = 7)]
    [ColumnDef("PopType1", Order = 7, SqlDbType = SqlDbType.VarChar, MaxLength = 3)]
    public string PopType1 { get; set; } = null!;

    [DataMember(Order = 8)]
    [ColumnDef("PopType2", Order = 8, SqlDbType = SqlDbType.VarChar, MaxLength = 3)]
    public string PopType2 { get; set; } = null!;

    [DataMember(Order = 9)]
    [ColumnDef("PushCount", Order = 9, SqlDbType = SqlDbType.Int)]
    public int PushCount { get; set; }

    [DataMember(Order = 10)]
    [ColumnDef("PushType0", Order = 10, SqlDbType = SqlDbType.VarChar, MaxLength = 3)]
    public string PushType0 { get; set; } = null!;

    [DataMember(Order = 11)]
    [ColumnDef("PushType1", Order = 11, SqlDbType = SqlDbType.VarChar, MaxLength = 3)]
    public string PushType1 { get; set; } = null!;

}



[DataContract]
[Table("__RefactorLog")]
public class RefactorLog
{
    [DataMember(Order = 1)]
    [ColumnDef("OperationKey", Order = 1, IsPrimaryKey = true, SqlDbType = SqlDbType.UniqueIdentifier)]
    public Guid OperationKey { get; set; }

}



[DataContract]
[Table("TestModel")]
public class TestModel
{
    [DataMember(Order = 1)]
    [ColumnDef("Id", Order = 1, IsIdentity = true, IsPrimaryKey = true, SqlDbType = SqlDbType.Int)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    [ColumnDef("Name", Order = 2, SqlDbType = SqlDbType.VarChar, MaxLength = 100)]
    public string Name { get; set; } = null!;

    [DataMember(Order = 3)]
    [ColumnDef("Value", Order = 3, SqlDbType = SqlDbType.Int)]
    public int Value { get; set; }

    [DataMember(Order = 4)]
    [ColumnDef("Description", Order = 4, SqlDbType = SqlDbType.VarChar, MaxLength = 500)]
    public string Description { get; set; } = null!;

}


[ServiceContract(Namespace = "http://DotOrmLib.Proxy")]
public interface IFlowControlController
    : IServiceController<FlowControl>
{
}

[ServiceContract(Namespace = "http://DotOrmLib.Proxy")]
public interface IMetaSourceCodeController
    : IServiceController<MetaSourceCode>
{
}

[ServiceContract(Namespace = "http://DotOrmLib.Proxy")]
public interface IOpCodeController
    : IServiceController<OpCode>
{
}

[ServiceContract(Namespace = "http://DotOrmLib.Proxy")]
public interface IOpCodeTypeController
    : IServiceController<OpCodeType>
{
}

[ServiceContract(Namespace = "http://DotOrmLib.Proxy")]
public interface IOperandTypeController
    : IServiceController<OperandType>
{
}

[ServiceContract(Namespace = "http://DotOrmLib.Proxy")]
public interface IStackBehaviourController
    : IServiceController<StackBehaviour>
{
}

[ServiceContract(Namespace = "http://DotOrmLib.Proxy")]
public interface IRefactorLogController
    : IServiceController<RefactorLog>
{
}

[ServiceContract(Namespace = "http://DotOrmLib.Proxy")]
public interface ITestModelController
    : IServiceController<TestModel>
{
}

