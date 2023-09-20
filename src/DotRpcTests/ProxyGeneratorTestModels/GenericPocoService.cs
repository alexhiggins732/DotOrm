namespace DotRpc.Tests.ProxyGeneratorTestModels
{
    public class GenericPocoService<T1, T2> : IGenericPocoService<T1, T2>
    {
        public T1 Add(T1 id, T2 name)
        {
            return id;
        }
        public GenericPoco<T1, T2> Add(GenericPoco<T1, T2> poco)
        {
            return poco;
        }
        public bool Delete(T1 id, T2 name)
        {
            return true;
        }
        public bool Delete(GenericPoco<T1, T2> poco)
        {
            return true;
        }
        public GenericPoco<T1, T2> Get(T1 id)
        {
            return new GenericPoco<T1, T2>() { Id = id };
        }
        public IEnumerable<GenericPoco<T1, T2>> GetByIds(IEnumerable<T1> ids)
        {
            return ids.Select(x => new GenericPoco<T1, T2>() { Id = x });
        }
    }

}
