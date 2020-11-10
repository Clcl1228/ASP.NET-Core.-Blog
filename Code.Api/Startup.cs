using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.Swagger;

namespace Code.Api
{
    public class Startup
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region 参数
            //读取配置文件
            var audienceConfig = Configuration.GetSection("Audience");
            var symmetricKeyAsBase64 = audienceConfig["Secret"];
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            #endregion
            services.AddControllers();
            services.AddMvc();

            #region Swagger  接口说明文档配置
            services.AddSwaggerGen(a =>
            {
                a.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = "v1.0.0",
                    Title = "BaByYou.API",
                    Description = "斗油大陆说明文档"
                });

                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "Code.Api.xml");
                a.IncludeXmlComments(xmlPath, true);


                var xmlModelPath = Path.Combine(basePath, "You.Core.Model.xml");//这个就是Model层的xml文件名
                a.IncludeXmlComments(xmlModelPath);

                //开启加权小说
                a.OperationFilter<AddResponseHeadersFilter>();
                a.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                //在header中添加token,传递到后台
                a.OperationFilter<SecurityRequirementsOperationFilter>();
                a.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT授权（数据将在请求头中进行传输）直接在下矿输入Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

            });
            #endregion

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Client", policy => policy.RequireRole("Client").Build());//单独角色
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin").Build());
                options.AddPolicy("SystemOrAdmin", policy => policy.RequireRole("Admin", "System"));//或的关系
                options.AddPolicy("SystemAndAdmin", policy => policy.RequireRole("Admin").RequireRole("System"));//且的关系
            });

            services.AddAuthentication(x =>
            {
                //看这个单词熟悉么？没错，就是上边错误里的那个。
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })// 也可以直接写字符串，AddAuthentication("Bearer")
              .AddJwtBearer(o =>
              {
                  o.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidateIssuerSigningKey = true,
                      IssuerSigningKey = signingKey,//参数配置在下边
                      ValidateIssuer = true,
                      ValidIssuer = audienceConfig["Issuer"],//发行人
                      ValidateAudience = true,
                      ValidAudience = audienceConfig["Audience"],//订阅人
                      ValidateLifetime = true,
                      ClockSkew = TimeSpan.Zero,//这个是缓冲过期时间，也就是说，即使我们配置了过期时间，这里也要考虑进去，过期时间+缓冲，默认好像是7分钟，你可以直接设置为0
                      RequireExpirationTime = true,
                  };

              });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())  //开发环境
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseMvc();

            #region Swagger
            app.UseSwagger();
            app.UseSwaggerUI(a =>
            {
                a.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiHelper V1");
                a.RoutePrefix = "";
            });

            #endregion
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}"
                    ) ;
                //endpoints.MapHub<ChatHub>("/api2/chartHub");
            });

        }
    }
}