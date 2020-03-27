using HT.Template.BackEnd.Hubs;
using HT.Template.BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HT.Template.BackEnd.Controllers
{
    [Route("[controller]")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(nameof(PermissionRequirement))]
    public abstract class AppControllerBase : Controller
    {
        /// <summary>
        /// AppControllerBase
        /// </summary>
        protected AppControllerBase(AppDbContext context, IHubContext<ApplicationHub, IApplicationHubClient> hubContext)
        {
            Context = context;
            HubContext = hubContext;
            Repository = new AppRepository(Context);
        }

        /// <summary>
        /// 数据库
        /// </summary>
        public AppDbContext Context { get; set; }

        /// <summary>
        /// SignalR
        /// </summary>
        public IHubContext<ApplicationHub, IApplicationHubClient> HubContext { get; set; }

        /// <summary>
        /// 数据库
        /// </summary>
        public AppRepository Repository { get; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            var data = context.HttpContext.GetHttpRequestData();
            Serilog.Log.Debug($"开始请求| {data.Method} | {data.Path}");
            Repository.CurrentUserId = data.UserId;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            var data = context.HttpContext.GetHttpRequestData();
            Serilog.Log.Debug($"结束请求| {data.Method} | {data.Path}");
        }


    }

    /// <summary>
    /// AppControllerBase
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class AppControllerBase<TEntity> : AppControllerBase where TEntity : Entity
    {
        /// <summary>
        /// AppControllerBase
        /// </summary>
        protected AppControllerBase(AppDbContext context, IHubContext<ApplicationHub, IApplicationHubClient> hubContext) : base(context, hubContext)
        {
        }

        /// <summary>
        /// 批量接口路由
        /// </summary>
        public const string BatchRoute = "Batch";

        /// <summary>
        /// 批量接口路由
        /// </summary>
        public const string BatchRouteW = "/Batch";

        /// <summary>
        /// 获取数据条数
        /// </summary>
        /// <returns>数据条数</returns>
        [HttpGet(nameof(Count))]
        public IActionResult Count(string where = null) => Ok(Repository.LongCount<TEntity>(where));

        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <returns>所有数据</returns>
        [HttpGet]
        public IActionResult Get() => Ok(Repository.Get<TEntity>());

        /// <summary>
        /// 获取一批数据
        /// </summary>
        /// <param name="where">where条件（例：[id="123"]）</param>
        /// <param name="orderBy">orderBy条件（例：[id asc]）</param>
        /// <param name="pageSize">每页数量（0为所有）</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>筛选数据</returns>
        [HttpGet(nameof(Filter))]
        public IActionResult Filter(string where = null, string orderBy = null, int pageSize = 10, int pageIndex = 1) => Ok(new { Data = Repository.Get<TEntity>(where, orderBy, pageSize, pageIndex), Count = Repository.LongCount<TEntity>(where) });

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>一条数据</returns>
        [HttpGet(nameof(Entity.Id))]
        public IActionResult Get(Guid id) => Ok(Repository.GetSingle<TEntity>(id));

        /// <summary>
        /// 新增一条数据
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>Api结果</returns>
        [HttpPost]
        public async Task<IActionResult> Insert(TEntity entity) => Ok(new APIResult(await Repository.InsertAsync(entity), "", entity));

        /// <summary>
        /// 新增一批数据
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <returns>Api结果</returns>
        [HttpPost(BatchRoute)]
        public async Task<IActionResult> Insert(IEnumerable<TEntity> entities) => Ok(new APIResult(await Repository.InsertRangeAsync(entities), "", entities));

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>Api结果</returns>
        [HttpPut]
        public IActionResult Update(TEntity entity) => Ok(new APIResult(Repository.Update(entity), "", entity));

        /// <summary>
        /// 更新一批数据
        /// </summary>
        /// <param name="entities">实体</param>
        /// <returns>Api结果</returns>
        [HttpPut(BatchRoute)]
        public IActionResult Update(IEnumerable<TEntity> entities) => Ok(new APIResult(Repository.UpdateRange(entities), "", entities));

        /// <summary>
        /// 删除一条数据
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>Api结果</returns>
        [HttpDelete]
        public IActionResult Delete(TEntity entity) => Ok(new APIResult(Repository.Delete(entity)));

        /// <summary>
        /// 删除一批数据
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <returns>Api结果</returns>
        [HttpDelete(BatchRoute)]
        public IActionResult Delete(IEnumerable<TEntity> entities) => Ok(new APIResult(Repository.DeleteRange(entities)));

        /// <summary>
        /// 删除一条数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Api结果</returns>
        [HttpDelete(nameof(Entity.Id))]
        public IActionResult Delete(Guid id) => Ok(new APIResult(Repository.Delete<TEntity>(id)));
    }
}
