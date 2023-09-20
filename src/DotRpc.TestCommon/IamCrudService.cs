using DotRpc.TestCommon;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.ComponentModel.DataAnnotations;

namespace DotRpc.TestCommon
{
    namespace IamCrudService
    {
        public interface IEntity { }
        public interface IEntity<TKey> : IEntity
            where TKey : notnull, IEquatable<TKey>
        {
            [Key] TKey Id { get; set; }
        }
        public interface IEntityOfInt : IEntity<int> { }

        public interface ICrudService<T>
        {

        }
        public interface IEntityCrudService<TEntity, TKey>
            where TEntity : IEntity<TKey>
            where TKey : notnull, IEquatable<TKey>

        {

        }

        [RpcService(Name = "CrudService")]
        public interface IIntEntityCrudService<TEntityOfInt>
            : IEntityCrudService<TEntityOfInt, int>
            where TEntityOfInt : IEntityOfInt
        {
            int Add(TEntityOfInt entity);
            bool Delete(int key);
            IEnumerable<TEntityOfInt> GetAll();
            //IEnumerable<TEntityOfInt> GetByIds(IEnumerable<int> keys, int skip = 0, int take = 100);
            IEnumerable<TEntityOfInt> GetByPage(int skip = 0, int take = 100);
            TEntityOfInt? GetFirst(int key);
            IEnumerable<TEntityOfInt> Query(string filter, int skip = 0, int take = 100);
            bool Update(TEntityOfInt entity);

        }
        public interface IFilter<TEntity> where TEntity : IEntity
        {
        }
        public interface IIntFilter<TEntity> where TEntity : IEntityOfInt
        {
        }

        #region services

        public abstract class CrudService<TEntity> : ICrudService<TEntity>
            where TEntity : IEntity
        {

        }
        public abstract class EntityCrudService<TEntity, TKey> :
            CrudService<TEntity>,
            ICrudService<TEntity>
            where TEntity : IEntity<TKey>
            where TKey : notnull, IEquatable<TKey>
        {
            //public abstract TKey Add(TEntity entity);
            //public abstract bool Delete(TKey key);
            //public abstract TEntity GetFirst(TKey key);
            //public abstract IEnumerable<TEntity> GetAll();
            //public abstract IEnumerable<TEntity> GetByPage(int skip = 0, int take = 100);
            //public abstract IEnumerable<TEntity> GetByIds(IEnumerable<TKey> keys, int skip = 0, int take = 100);
            //public abstract IEnumerable<TEntity> Query(IFilter<TEntity> filter, int skip = 0, int take = 100);
            //public abstract bool Update(TEntity entity);

            public virtual TKey Add(TEntity entity)
            {
                throw new NotImplementedException();
            }

            public virtual bool Delete(TKey key)
            {
                throw new NotImplementedException();
            }

            public virtual IEnumerable<TEntity> GetAll()
            {
                throw new NotImplementedException();
            }

            public virtual IEnumerable<TEntity> GetByIds(IEnumerable<TKey> keys, int skip = 0, int take = 100)
            {
                throw new NotImplementedException();
            }

            public virtual IEnumerable<TEntity> GetByPage(int skip = 0, int take = 100)
            {
                throw new NotImplementedException();
            }

            public virtual TEntity? GetFirst(TKey key)
            {
                throw new NotImplementedException();
            }

            public virtual IEnumerable<TEntity> Query(string filter, int skip = 0, int take = 100)
            {
                throw new NotImplementedException();
            }
            public virtual bool Update(TEntity entity)
            {
                throw new NotImplementedException();
            }
        }

        public interface IKeyProvider<TKey>
        {
            TKey GetNextKey();
        }
        public class KeyProviderFactory
        {
            internal static IKeyProvider<TKey> Create<TKey>()
            {
                var t = typeof(TKey);
                if (t == typeof(int)) return (IKeyProvider<TKey>)(new IntKeyProvider());
                else if (t == typeof(Guid)) return (IKeyProvider<TKey>)(new GuidKeyProvider());
                else throw new NotImplementedException();
            }

            class GuidKeyProvider : IKeyProvider<Guid>
            {
                public Guid GetNextKey() => Guid.NewGuid();
            }

            class IntKeyProvider : IKeyProvider<int>
            {
                private int Id;
                public IntKeyProvider() { this.Id = 0; }
                public int GetNextKey() => ++Id;
            }
        }

        public class InMemoryEntityCrudService<TEntity, TKey>
            : EntityCrudService<TEntity, TKey>
                where TEntity : IEntity<TKey>
                where TKey : notnull, IEquatable<TKey>
        {
            public static Dictionary<TKey, TEntity> DataStore = new();
            static IKeyProvider<TKey> KeyProvider = KeyProviderFactory.Create<TKey>();
            public InMemoryEntityCrudService()
            {

            }
            public override TKey Add(TEntity entity)
            {
                if (!entity.Id.Equals(default(TKey)))
                {
                    if (GetFirst(entity.Id) is not null)
                        return default(TKey);
                }
                entity.Id = KeyProvider.GetNextKey();
                DataStore.TryAdd(entity.Id, entity);
                return entity.Id;
            }

            public override bool Delete(TKey key)
            {
                if (DataStore.ContainsKey(key))
                    return DataStore.Remove(key);
                return false;
            }

            public override IEnumerable<TEntity> GetAll()
            {
                return DataStore.Values.ToList();
            }

            public override IEnumerable<TEntity> GetByIds(IEnumerable<TKey> keys, int skip = 0, int take = 100)
            {
                return DataStore.Values.Skip(skip).Take(take).Where(x => keys.Contains(x.Id));
            }

            public override IEnumerable<TEntity> GetByPage(int skip = 0, int take = 100)
            {
                return DataStore.Values.Skip(skip).Take(take);
            }

            public override TEntity? GetFirst(TKey key)
            {
                DataStore.TryGetValue(key, out var entity);
                return entity;
            }

            public override IEnumerable<TEntity> Query(string filter, int skip = 0, int take = 100)
            {
                return base.Query(filter, skip, take);
            }

            public override bool Update(TEntity entity)
            {
                if (entity.Id.Equals(default(TKey)) || !DataStore.ContainsKey(entity.Id))
                    return false;

                DataStore[entity.Id] = entity;
                return true;
            }
        }
        
        public class IntEntityCrudService<TEntityOfInt> :
            InMemoryEntityCrudService<TEntityOfInt, int>,
            IIntEntityCrudService<TEntityOfInt>
            where TEntityOfInt : IEntityOfInt
        {
        }

        [RpcService(Name = nameof(AppCrudService))]
        public interface IAppCrudService : IIntEntityCrudService<App> { }
        public class AppCrudService: IntEntityCrudService<App>, IAppCrudService
        {

        }

     
        #endregion

        #region models
        public class EntityBase<TKey> : IEntity<TKey>
            where TKey : notnull, IEquatable<TKey>, new()
        {
            public EntityBase()
            {
                Id = new();
            }
            public TKey Id { get; set; }
        }
        public class EntityBaseOfInt : EntityBase<int>, IEntityOfInt { }
        public class App : EntityBaseOfInt
        {
            public string Name { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public int ServiceAccountUserId { get; set; }
        }
        public class Module
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int AppId { get; set; }
        }
        public class Feature
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int ModuleId { get; set; }
        }
        public class Scope
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int FeatureId { get; set; }
        }
        public class RoleScopes
        {
            public string Id { get; set; }
            public int ScopeId { get; set; }
            public int RoleId { get; set; }
        }
        public class User
        {
            public int Id { get; set; }
            public string UUID { get; set; }
        }
        public class Role : EntityBaseOfInt
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public class UserRoles
        {
            public int id { get; set; }
            public int UserId { get; set; }
            public int RoleId { get; set; }
        }
        public class Group
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public class UserGroups
        {
            public int id { get; set; }
            public int UserId { get; set; }
            public int GroupId { get; set; }
        }
        public class RoleGroups
        {
            public int id { get; set; }
            public int RoleId { get; set; }
            public int GroupId { get; set; }
        }
        #endregion
    }
}