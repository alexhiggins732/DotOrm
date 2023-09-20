namespace DotRpc.Tests.ProxyGeneratorTestModels
{
    public interface IPlainPocoService
    {
        int Add(int id, string name);
        PlainPoco Add(PlainPoco poco);
        bool Delete(int id);
        bool Delete(PlainPoco poco);
        PlainPoco Get(int id);
        IEnumerable<PlainPoco> GetByIds(IEnumerable<int> ids);
    }
}