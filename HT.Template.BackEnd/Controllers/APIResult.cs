namespace HT.Template.BackEnd.Controllers
{
    /// <summary>
    /// API结果
    /// </summary>
    public class APIResult
    {
        /// <summary>
        /// 成功
        /// </summary>
        public bool Succeeded { get; set; } = false;

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public object Data { get; set; }

        public APIResult(bool success = false, string message = "", object data = null)
        {
            Succeeded = success;
            Message = message;
            Data = data;
        }

        public static APIResult OK { get; } = new APIResult(true);
    }
}