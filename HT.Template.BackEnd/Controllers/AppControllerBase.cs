using HT.Template.BackEnd.Hubs;
using HT.Template.BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status201Created)]
    //[ProducesResponseType(StatusCodes.Status204NoContent)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //[ProducesResponseType(StatusCodes.Status403Forbidden)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
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
        public long Count(string where = null) => Repository.LongCount<TEntity>(where);

        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <returns>所有数据</returns>
        [HttpGet]
        public ICollection<TEntity> Get() => Repository.Get<TEntity>();

        /// <summary>
        /// 获取一批数据
        /// </summary>
        /// <param name="where">where条件（例：[id="123"]）</param>
        /// <param name="orderBy">orderBy条件（例：[id asc]）</param>
        /// <param name="pageSize">每页数量（0为所有）</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>筛选数据</returns>
        [HttpGet(nameof(Filter))]
        public ICollection<TEntity> Filter(string where = null, string orderBy = null, int pageSize = 10, int pageIndex = 1) =>
            Repository.Get<TEntity>(where, orderBy, pageSize, pageIndex);

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>一条数据</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet(nameof(Entity.Id))]
        public ActionResult<TEntity> Get(Guid id)
        {
            var entity = Repository.GetSingle<TEntity>(id);
            return entity == null ? NotFound() : (ActionResult<TEntity>)entity;
        }

        /// <summary>
        /// 新增一条数据
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Insert(TEntity entity) =>
            await Repository.InsertAsync(entity) ? CreatedAtAction(nameof(Get), new { entity.Id }, entity) : (IActionResult)BadRequest();

        /// <summary>
        /// 新增一批数据
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost(BatchRoute)]
        public async Task<IActionResult> Insert(IEnumerable<TEntity> entities) =>
            await Repository.InsertRangeAsync(entities) ? CreatedAtAction(nameof(Get), entities) : (IActionResult)BadRequest();

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut]
        public IActionResult Update(TEntity entity) => Repository.Update(entity) ? NoContent() : (IActionResult)BadRequest();

        /// <summary>
        /// 更新一批数据
        /// </summary>
        /// <param name="entities">实体</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut(BatchRoute)]
        public IActionResult Update(IEnumerable<TEntity> entities) => Repository.UpdateRange(entities) ? NoContent() : (IActionResult)BadRequest();

        /// <summary>
        /// 删除一条数据
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete]
        public IActionResult Delete(TEntity entity) => Repository.Delete(entity) ? NoContent() : (IActionResult)BadRequest();

        /// <summary>
        /// 删除一批数据
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete(BatchRoute)]
        public IActionResult Delete(IEnumerable<TEntity> entities) => Repository.DeleteRange(entities) ? NoContent() : (IActionResult)BadRequest();

        /// <summary>
        /// 删除一条数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete(nameof(Entity.Id))]
        public IActionResult Delete(Guid id) => Repository.Delete<TEntity>(id) ? NoContent() : (IActionResult)BadRequest();
    }
}
