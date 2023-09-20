namespace DotRpc.Tests.ProxyGeneratorTestModels
{
    public class PlainPocoService : IPlainPocoService
    {
        public int Add(int id, string name)
        {
            return id;
        }
        public PlainPoco Add(PlainPoco poco)
        {
            return poco;
        }
        public bool Delete(int id)
        {
            return true;
        }
        public bool Delete(PlainPoco poco)
        {
            return true;
        }
        public PlainPoco Get(int id)
        {
            return new PlainPoco();
        }
        public IEnumerable<PlainPoco> GetByIds(IEnumerable<int> ids)
        {
            return (new[] { new PlainPoco() });
        }
    }

}
