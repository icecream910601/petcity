using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using Jose;
using PetCityApi1.Models;

namespace PetCityApi1.JWT
{
    public class JwtAuthUtil
    {
        private readonly PetCityNewcontext petCityDbContext = new PetCityNewcontext(); // DB 連線

        /// <summary>
        /// 生成 JwtToken 
        /// </summary>
        /// <param name="id">會員id</param>
        /// <returns>JwtToken</returns>
        public string GenerateToken(int id, string userAccount, string userName, string userThumbnail, string identity)
        {
            // 自訂字串，驗證用，用來加密送出的 key (放在 Web.config 的 appSettings)
            string secretKey = WebConfigurationManager.AppSettings["TokenKey"]; // 從 appSettings 取出  //私自的鑰匙//為了更嚴謹
            
            var customer = petCityDbContext.Customers.Find(id); // 進 DB 取出想要夾帶的基本資料

            // payload 需透過 token 傳遞的資料 (可夾帶常用且不重要的資料)
            var payload = new Dictionary<string, object>    
            {
                 { "Id", customer.Id },
                { "Account",customer.UserAccount },
                { "Name", customer.UserName },
                { "Image", customer.UserThumbnail },
                { "Identity", customer.Identity },
                { "Exp", DateTime.Now.AddHours(1).ToString() } // JwtToken 時效設定 30 分
            };

            // 產生 JwtToken
            var token = Jose.JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }


        /// <summary>
        /// 生成只刷新效期的 JwtToken
        /// </summary>
        /// <returns>JwtToken</returns>
        public string ExpRefreshToken(Dictionary<string, object> tokenData)
        {
            string secretKey = WebConfigurationManager.AppSettings["TokenKey"];
            // payload 從原本 token 傳遞的資料沿用，並刷新效期
            var payload = new Dictionary<string, object>
            {
                { "Id", (int)tokenData["Id"] },
                { "Account", tokenData["Account"].ToString() },
                { "Name", tokenData["Name"].ToString() },
                { "Image", tokenData["Image"] },
                { "Identity", tokenData["Identity"].ToString() },
                { "Exp", DateTime.Now.AddHours(1).ToString() } // JwtToken 時效刷新設定 30 分
            };

            //產生刷新時效的 JwtToken
            var token = Jose.JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        /// <summary>
        /// 生成無效 JwtToken
        /// </summary>
        /// <returns>JwtToken</returns>
        public string RevokeToken()
        {
            string secretKey = "RevokeToken"; // 故意用不同的 key 生成
            var payload = new Dictionary<string, object>
            {
                { "Id", 0 },
                { "Account", "None" },
                { "Name", "None" },
                { "Image", "None" },
                { "Identity", "None" },
                { "Exp", DateTime.Now.AddDays(-15).ToString() } // 使 JwtToken 過期 失效
            };

            // 產生失效的 JwtToken
            var token = Jose.JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }


    }

}
