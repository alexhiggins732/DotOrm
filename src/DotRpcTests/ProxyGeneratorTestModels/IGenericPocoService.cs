namespace DotRpc.Tests.ProxyGeneratorTestModels
{
    public interface IGenericPocoService<T1, T2>
    {
        GenericPoco<T1, T2> Add(GenericPoco<T1, T2> poco);
        T1 Add(T1 id, T2 name);
        bool Delete(GenericPoco<T1, T2> poco);
        bool Delete(T1 id, T2 name);
        GenericPoco<T1, T2> Get(T1 id);
        IEnumerable<GenericPoco<T1, T2>> GetByIds(IEnumerable<T1> ids);
    }
}