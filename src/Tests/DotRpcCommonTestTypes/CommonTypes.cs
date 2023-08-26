using DotRpcCommonTestTypes;

namespace DotRpcCommonTestTypes
{
    public interface TEntity<TKey>
    {
        TKey Id { get; set; }
    }
    public interface IFilter { }
    public interface ICrudServiceBase<TEntity, TKey>
         where TEntity : TEntity<TKey>
    {
        TEntity Add(TKey id, string name);
        TEntity AddRange(ICollection<TEntity> entity);
        TEntity QueryFirst(TKey id);
        TEntity QueryFirstOrDefult(TKey id);
        IEnumerable<TEntity> Query(IFilter filter);
        TEntity Update(TEntity entity);
        bool Delete(TKey id);
    }


    public interface TestInterfaceBase<TEntity> : ICrudServiceBase<TEntity, int>
        where TEntity : TEntity<int>
    {

    }

    public class MyModel1KeyInt : TEntity<int>
    {
        public int Id { get; set; }
    }
    public class MyModel2KeyInt: TEntity<int>
    {
        public int Id { get; set; }
    }
    public class MyModel3KeyInt : TEntity<int>
    {
        public int Id { get; set; }
    }

    public class MyModel1KeyGuid : TEntity<Guid>
    {
        public Guid Id { get; set; }
    }
    public class MyModel2KeyGuid : TEntity<Guid>
    {
        public Guid Id { get; set; }
    }
    public class MyModel3KeyGuid : TEntity<Guid>
    {
        public Guid Id { get; set; }
    }



    public abstract class ModelCrudServiceBase<TEntity, TKey>
        : ICrudServiceBase<TEntity, TKey>
        where TEntity: TEntity<TKey>

    {
        public abstract TEntity Add(TKey id, string name);
        public abstract TEntity AddRange(ICollection<TEntity> entity);
        public abstract bool Delete(TKey id);
        public abstract IEnumerable<TEntity> Query(IFilter filter);
        public abstract TEntity QueryFirst(TKey id);
        public abstract TEntity QueryFirstOrDefult(TKey id);
        public abstract TEntity Update(TEntity entity);
    }

    public abstract class ModelCrudServiceBaseOfInt<TEntity>
        : ModelCrudServiceBase<TEntity, int>
        where TEntity: TEntity<int>
    { 
          
    }

    public abstract class ModelCrudServiceBaseOfGuid<TEntity>
       : ModelCrudServiceBase<TEntity, Guid>
       where TEntity : TEntity<Guid>
    {

    }

    public class MyModelIntCrudServiceBase<TEntity> : ModelCrudServiceBaseOfInt<TEntity>
        where TEntity : TEntity<int>
    {
        public override TEntity Add(int id, string name)
        {
            throw new NotImplementedException();
        }

        public override TEntity AddRange(ICollection<TEntity> entity)
        {
            throw new NotImplementedException();
        }

        public override bool Delete(int id)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<TEntity> Query(IFilter filter)
        {
            throw new NotImplementedException();
        }

        public override TEntity QueryFirst(int id)
        {
            throw new NotImplementedException();
        }

        public override TEntity QueryFirstOrDefult(int id)
        {
            throw new NotImplementedException();
        }

        public override TEntity Update(TEntity entity)
        {
            throw new NotImplementedException();
        }
    }

    public class MyModelGuidCrudServiceBase<TEntity> : ModelCrudServiceBaseOfGuid<TEntity>
        where TEntity : TEntity<Guid>
    {
        public override TEntity Add(Guid id, string name)
        {
            throw new NotImplementedException();
        }

        public override TEntity AddRange(ICollection<TEntity> entity)
        {
            throw new NotImplementedException();
        }

        public override bool Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<TEntity> Query(IFilter filter)
        {
            throw new NotImplementedException();
        }

        public override TEntity QueryFirst(Guid id)
        {
            throw new NotImplementedException();
        }

        public override TEntity QueryFirstOrDefult(Guid id)
        {
            throw new NotImplementedException();
        }

        public override TEntity Update(TEntity entity)
        {
            throw new NotImplementedException();
        }
    }



    namespace Namespace1
    {
        public class MyModel1KeyInt : DotRpcCommonTestTypes.MyModel1KeyInt { }
        public class MyModel2KeyInt : DotRpcCommonTestTypes.MyModel2KeyInt { }
        public class MyModel3KeyInt : DotRpcCommonTestTypes.MyModel3KeyInt { }

        public class MyModel1IntCrudService : MyModelIntCrudServiceBase<MyModel3KeyInt> { }
        public class MyModel2IntCrudService : MyModelIntCrudServiceBase<MyModel2KeyInt> { }
        public class MyModel3IntCrudService :MyModelIntCrudServiceBase<MyModel3KeyInt> { }

        public class MyModel1IntCrudServiceInherited : MyModelIntCrudServiceBase<DotRpcCommonTestTypes.MyModel3KeyInt> { }
        public class MyModel2IntCrudServiceInherited : MyModelIntCrudServiceBase<DotRpcCommonTestTypes.MyModel2KeyInt> { }
        public class MyModel3IntCrudServiceInherited : MyModelIntCrudServiceBase<DotRpcCommonTestTypes.MyModel3KeyInt> { }

        public class MyModel1KeyGuid : DotRpcCommonTestTypes.MyModel1KeyGuid { }
        public class MyModel2KeyGuid : DotRpcCommonTestTypes.MyModel2KeyGuid { }
        public class MyModel3KeyGuid : DotRpcCommonTestTypes.MyModel3KeyGuid { }

        public class MyModel1GuidCrudService : MyModelGuidCrudServiceBase<MyModel3KeyGuid> { }
        public class MyModel2GuidCrudService : MyModelGuidCrudServiceBase<MyModel2KeyGuid> { }
        public class MyModel3GuidCrudService : MyModelGuidCrudServiceBase<MyModel3KeyGuid> { }

        public class MyModel1GuidCrudServiceInherited : MyModelGuidCrudServiceBase<DotRpcCommonTestTypes.MyModel3KeyGuid> { }
        public class MyModel2GuidCrudServiceInherited : MyModelGuidCrudServiceBase<DotRpcCommonTestTypes.MyModel2KeyGuid> { }
        public class MyModel3GuidCrudServiceInherited : MyModelGuidCrudServiceBase<DotRpcCommonTestTypes.MyModel3KeyGuid> { }


    }
    namespace Namespace2
    {
        public class MyModel1KeyInt : DotRpcCommonTestTypes.MyModel1KeyInt { }
        public class MyModel2KeyInt : DotRpcCommonTestTypes.MyModel2KeyInt { }
        public class MyModel3KeyInt : DotRpcCommonTestTypes.MyModel3KeyInt { }

        public class MyModel1IntCrudService : MyModelIntCrudServiceBase<MyModel3KeyInt> { }
        public class MyModel2IntCrudService : MyModelIntCrudServiceBase<MyModel2KeyInt> { }
        public class MyModel3IntCrudService : MyModelIntCrudServiceBase<MyModel3KeyInt> { }

        public class MyModel1IntCrudServiceInherited : MyModelIntCrudServiceBase<DotRpcCommonTestTypes.MyModel3KeyInt> { }
        public class MyModel2IntCrudServiceInherited : MyModelIntCrudServiceBase<DotRpcCommonTestTypes.MyModel2KeyInt> { }
        public class MyModel3IntCrudServiceInherited : MyModelIntCrudServiceBase<DotRpcCommonTestTypes.MyModel3KeyInt> { }

        public class MyModel1KeyGuid : DotRpcCommonTestTypes.MyModel1KeyGuid { }
        public class MyModel2KeyGuid : DotRpcCommonTestTypes.MyModel2KeyGuid { }
        public class MyModel3KeyGuid : DotRpcCommonTestTypes.MyModel3KeyGuid { }

        public class MyModel1GuidCrudService : MyModelGuidCrudServiceBase<MyModel3KeyGuid> { }
        public class MyModel2GuidCrudService : MyModelGuidCrudServiceBase<MyModel2KeyGuid> { }
        public class MyModel3GuidCrudService : MyModelGuidCrudServiceBase<MyModel3KeyGuid> { }

        public class MyModel1GuidCrudServiceInherited : MyModelGuidCrudServiceBase<DotRpcCommonTestTypes.MyModel3KeyGuid> { }
        public class MyModel2GuidCrudServiceInherited : MyModelGuidCrudServiceBase<DotRpcCommonTestTypes.MyModel2KeyGuid> { }
        public class MyModel3GuidCrudServiceInherited : MyModelGuidCrudServiceBase<DotRpcCommonTestTypes.MyModel3KeyGuid> { }
    }
    namespace Namespace3
    {
        public class MyModel1KeyInt : DotRpcCommonTestTypes.MyModel1KeyInt { }
        public class MyModel2KeyInt : DotRpcCommonTestTypes.MyModel2KeyInt { }
        public class MyModel3KeyInt : DotRpcCommonTestTypes.MyModel3KeyInt { }

        public class MyModel1IntCrudService : MyModelIntCrudServiceBase<MyModel3KeyInt> { }
        public class MyModel2IntCrudService : MyModelIntCrudServiceBase<MyModel2KeyInt> { }
        public class MyModel3IntCrudService : MyModelIntCrudServiceBase<MyModel3KeyInt> { }

        public class MyModel1IntCrudServiceInherited : MyModelIntCrudServiceBase<DotRpcCommonTestTypes.MyModel3KeyInt> { }
        public class MyModel2IntCrudServiceInherited : MyModelIntCrudServiceBase<DotRpcCommonTestTypes.MyModel2KeyInt> { }
        public class MyModel3IntCrudServiceInherited : MyModelIntCrudServiceBase<DotRpcCommonTestTypes.MyModel3KeyInt> { }

        public class MyModel1KeyGuid : DotRpcCommonTestTypes.MyModel1KeyGuid { }
        public class MyModel2KeyGuid : DotRpcCommonTestTypes.MyModel2KeyGuid { }
        public class MyModel3KeyGuid : DotRpcCommonTestTypes.MyModel3KeyGuid { }

        public class MyModel1GuidCrudService : MyModelGuidCrudServiceBase<MyModel3KeyGuid> { }
        public class MyModel2GuidCrudService : MyModelGuidCrudServiceBase<MyModel2KeyGuid> { }
        public class MyModel3GuidCrudService : MyModelGuidCrudServiceBase<MyModel3KeyGuid> { }

        public class MyModel1GuidCrudServiceInherited : MyModelGuidCrudServiceBase<DotRpcCommonTestTypes.MyModel3KeyGuid> { }
        public class MyModel2GuidCrudServiceInherited : MyModelGuidCrudServiceBase<DotRpcCommonTestTypes.MyModel2KeyGuid> { }
        public class MyModel3GuidCrudServiceInherited : MyModelGuidCrudServiceBase<DotRpcCommonTestTypes.MyModel3KeyGuid> { }
    }
}