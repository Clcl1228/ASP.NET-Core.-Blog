using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using You.Core.Model;

namespace Code.Api.Controllers
{
    /// <summary>
    /// 油魂主殿
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "SystemOrAdmin")]
    public class BaByYouController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<BaByYouController> _logger;
        
        public BaByYouController(ILogger<BaByYouController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// 获取秘籍:油满天下
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<BaByYou> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new BaByYou
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
        /// <summary>
        /// 获取秘籍:比比油
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{Name}")]
        public string Get(string name)
        {
            return "values";
        }
        /// <summary>
        ///  获取油力等级
        /// </summary>
        /// <param name="you">实体类：油参数</param>
        /// <returns></returns>
        [HttpPost("{id}")]
        public ActionResult Post(You.Core.Model.You you)
        {
            return Ok();
        }
        [HttpPost]
        /// <summary>
        /// 登陆油宝琉璃宗
        /// </summary>
        /// <param name="usname"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public async Task<object> GetJwtStr(string usname,string pwd)
        {
            TokenModelJwt tokenModel = new TokenModelJwt { Uid = 1, Role = "Admin" };
            var jwtStr = JwtHelper.IssueJwt(tokenModel);//登录，获取到一定规则的 Token 令牌
            var suc = true;
            return Ok(new
            {
                success = suc,
                token = jwtStr
            });
        }
    }
}
