using HT.Template.BackEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace HT.Template.BackEnd
{
    public class AppRepository
    {
        /// <summary>
        /// 逻辑删除
        /// </summary>
        private static readonly bool EnableSotfDelete = false;

        public AppRepository(AppDbContext context) => Context = context ?? new AppDbContext();

        public AppDbContext Context { get; }

        public Guid? CurrentUserId { get; set; }

        public bool Save() => Context.SaveChanges() > 0;

        public async Task<bool> SaveAsync(CancellationToken cancellationToken = default) => await Context.SaveChangesAsync(cancellationToken) > 0;

        private TEntity SetCreateValue<TEntity>(TEntity entity) where TEntity : Entity
        {
            entity.CreatorId = CurrentUserId;
            entity.CreationTime = DateTime.Now;
            entity.LastModifierId = null;
            entity.LastModificationTime = null;
            entity.IsDeleted = false;
            entity.DeleterId = null;
            entity.DeletionTime = null;
            return entity;
        }

        public bool Insert<TEntity>(TEntity entity) where TEntity : Entity
        {
            Context.Set<TEntity>().Add(SetCreateValue(entity));
            return Save();
        }

        public bool InsertRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : Entity
        {
            var list = entities.Select(e => SetCreateValue(e));
            Context.Set<TEntity>().AddRange(list);
            return Save();
        }

        public bool InsertParams<TEntity>(params TEntity[] entities) where TEntity : Entity => InsertRange(entities.AsEnumerable());

        public async Task<bool> InsertAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : Entity
        {
            await Context.Set<TEntity>().AddAsync(SetCreateValue(entity), cancellationToken);
            return await SaveAsync(cancellationToken);
        }

        public async Task<bool> InsertRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : Entity
        {
            var list = entities.Select(entity => SetCreateValue(entity));
            await Context.Set<TEntity>().AddRangeAsync(list, cancellationToken);
            return await SaveAsync(cancellationToken);
        }

        public async Task<bool> InsertParamsAsync<TEntity>(params TEntity[] entities) where TEntity : Entity => await InsertRangeAsync(entities.AsEnumerable());

        private TEntity SetModifyValue<TEntity>(TEntity entity) where TEntity : Entity
        {
            entity.LastModifierId = CurrentUserId;
            entity.LastModificationTime = DateTime.Now;
            return entity;
        }

        public bool Update<TEntity>(TEntity entity) where TEntity : Entity
        {
            Context.Set<TEntity>().Update(SetModifyValue(entity));
            return Save();
        }

        public bool UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : Entity
        {
            Context.Set<TEntity>().UpdateRange(entities.Select(e => SetModifyValue(e)));
            return Save();
        }

        public bool UpdateParams<TEntity>(params TEntity[] entities) where TEntity : Entity => UpdateRange(entities.AsEnumerable());

        private TEntity SetDeleteValue<TEntity>(TEntity entity) where TEntity : Entity
        {
            entity.IsDeleted = true;
            entity.DeleterId = CurrentUserId;
            entity.DeletionTime = DateTime.Now;
            return entity;
        }

        public bool Delete<TEntity>(TEntity entity) where TEntity : Entity
        {
            if (EnableSotfDelete)
            {
                Context.Set<TEntity>().Update(SetDeleteValue(entity));
            }
            else
            {
                Context.Remove(entity);
            }
            return Save();
        }

        public bool DeleteRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : Entity
        {
            if (EnableSotfDelete)
            {
                Context.Set<TEntity>().UpdateRange(entities.Select(e => SetDeleteValue(e)));
            }
            else
            {
                Context.RemoveRange(entities);
            }
            return Save();
        }

        public bool DeleteParams<TEntity>(params TEntity[] entities) where TEntity : Entity => DeleteRange(entities.AsEnumerable());

        public bool Delete<TEntity>(Guid id) where TEntity : Entity => Delete<TEntity>(p => p.Id == id);

        public bool Delete<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : Entity
        {
            if (EnableSotfDelete)
            {
                Context.Set<TEntity>().UpdateRange(Context.Set<TEntity>().Where(where).Select(e => SetDeleteValue(e)));
            }
            else
            {
                Context.Set<TEntity>().RemoveRange(Context.Set<TEntity>().Where(where));
            }
            return Save();
        }

        public bool EmptyData<TEntity>() where TEntity : Entity
        {
            Context.Set<TEntity>().RemoveRange(Context.Set<TEntity>());
            return Save();
        }

        public int Count<TEntity>(string where = null) where TEntity : Entity => Get<TEntity>(where).Count;

        public int Count<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : Entity => Get(where).Count;

        public long LongCount<TEntity>(string where = null) where TEntity : Entity => Get<TEntity>(where).LongCount();

        public long LongCount<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : Entity => Get(where).LongCount();

        public bool Exist<TEntity>(Guid id) where TEntity : Entity => Exist<TEntity>(p => p.Id == id);

        public bool Exist<TEntity>(string where = null) where TEntity : Entity => LongCount<TEntity>(where) > 0;

        public bool Exist<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : Entity => LongCount(where) > 0;

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="where">条件中不可以使用导航属性</param>
        /// <param name="orderBy"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ICollection<TEntity> Get<TEntity>(
            string where = null,
            string orderBy = null,
            int pageSize = 0,
            int pageIndex = 1) where TEntity : Entity
        {
            IQueryable<TEntity> set = Context.Set<TEntity>().Where(p => !p.IsDeleted);
            if (!string.IsNullOrWhiteSpace(where))
            {
                set = set.Where(where);
            }
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                set = set.OrderBy(orderBy);
            }
            if (pageSize != 0)
            {
                set = set.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }
            return set.ToList();
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="where">条件中不可以使用导航属性</param>
        /// <param name="orderBy"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ICollection<TEntity> Get<TEntity>(
            Expression<Func<TEntity, bool>> where,
            string orderBy = null,
            int pageSize = 0,
            int pageIndex = 1) where TEntity : Entity
        {
            IQueryable<TEntity> set = Context.Set<TEntity>().Where(p => !p.IsDeleted);
            if (where != null)
            {
                set = set.Where(where);
            }
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                set = set.OrderBy(orderBy);
            }
            if (pageSize != 0)
            {
                set = set.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }
            return set.ToList();
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where">条件中不可以使用导航属性</param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ICollection<TEntity> Get<TEntity, TKey>(
            Expression<Func<TEntity, bool>> where = null,
            Expression<Func<TEntity, TKey>> orderBy = null,
            bool isAsc = true,
            int pageSize = 0,
            int pageIndex = 1) where TEntity : Entity
        {
            IQueryable<TEntity> set = Context.Set<TEntity>().Where(p => !p.IsDeleted);
            if (where != null)
            {
                set = set.Where(where);
            }
            if (orderBy != null)
            {
                set = isAsc ? set.OrderBy(orderBy) : set.OrderByDescending(orderBy);
            }
            if (pageSize != 0)
            {
                set = set.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }
            return set.ToList();
        }

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity GetSingle<TEntity>(Guid id) where TEntity : Entity => GetSingle<TEntity>(p => p.Id == id);

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="predicate">条件中可以使用导航属性</param>
        /// <returns></returns>
        public TEntity GetSingle<TEntity>(Func<TEntity, bool> predicate = null) where TEntity : Entity => predicate == null ? Get<TEntity>().FirstOrDefault() : Get<TEntity>().FirstOrDefault(predicate);

    }
}