﻿using System;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using NSwag.Annotations;
using PetCityApi1.JWT;
using PetCityApi1.Models;

namespace PetCityApi1.Controllers
{



    [OpenApiTag("Users", Description = "使用者註冊登入CURD")]


    public class UserController : ApiController
    {


        /// <returns></returns>
        // GET: api/User
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}



        // GET: api/User/5
        //public string Get(int id)
        //{
        //    return "value";
        //}


        /// <summary>
        /// 會員註冊
        /// </summary>
        // POST: api/User
        [Route("user/signup/")]
        public IHttpActionResult Post(ViewModelCust.Signup signup) //複雜型別
        {
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            //判斷資料庫中有無此筆資料
            //如無則新增
            //自訂義參數名稱 => 自訂義參數名稱.物件屬性

            var customer = petCityDbContext.Customers.Where(c => c.UserAccount == signup.UserAccount).FirstOrDefault();

            if (customer != null) //有此帳號  //此帳號已被註冊過
            {

                return BadRequest("已註冊過");
                //result="此帳號已註冊過";
            }

            if (ModelState.IsValid && signup.UserPassWord == signup.ConfirmedPassword)  //1.格式正確 寫到資料庫  // 寄信到信箱 //註冊成功 
            {
                string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
                string toEmail = signup.UserAccount;
                string password = ConfigurationManager.AppSettings["EmailPassword"];
                string emailbody = "這邊是首頁的連結：/homepage";
                DateTime now = DateTime.Now;
                string Subject = $"{now.ToString("yyyy/MM/dd")}註冊信";
                string body = $"感謝您註冊寵物坊城市，您已經可以開始使用。{emailbody}";

                SendGmailMail(fromEmail, toEmail, Subject, body, password);

                Customer cust = new Customer();
                string guid = Guid.NewGuid().ToString();

                cust.UserAccount = signup.UserAccount;
                //cust.UserPassWord = signup.UserPassWord;
                cust.UserPassWord = BitConverter
                    .ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(signup.UserPassWord))).Replace("-", null);
                cust.UserName = signup.UserName;
                cust.Identity = signup.Identity;
                cust.UserGuid = guid;

                petCityDbContext.Customers.Add(cust);
                petCityDbContext.SaveChanges();


                var result = new
                {
                    status = "success",
                    message = "註冊成功",
                };
                
                return Ok(result);
            }
            else //格式錯誤
            {
                return BadRequest("格式錯誤");   
            }

        }
        public static void SendGmailMail(string fromAddress, string toAddress, string Subject, string MailBody, string password)
        {
            MailMessage mailMessage = new MailMessage(fromAddress, toAddress);
            mailMessage.Subject = Subject;
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = MailBody;
            // SMTP Server
            SmtpClient mailSender = new SmtpClient("smtp.gmail.com");
            System.Net.NetworkCredential basicAuthenticationInfo = new System.Net.NetworkCredential(fromAddress, password);
            mailSender.Credentials = basicAuthenticationInfo;
            mailSender.Port = 587;
            mailSender.EnableSsl = true;
            mailSender.Send(mailMessage);
            mailMessage.Dispose();
            mailSender = null;
        }



        /// <summary>
        /// 會員登入 
        /// </summary>
        //// POST: api/User
        [Route("user/login/")]
        public IHttpActionResult Post(ViewModelCust.Login login)
        {
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            //Customer cust = new Customer();  //用類別的兩種方式 1.new 物件

            //判斷資料庫中有無此筆資料
            //如有則登入

            Customer customer = petCityDbContext.Customers.Where(c => c.UserAccount == login.UserAccount).FirstOrDefault(); //2.直接給值

            if (customer != null) //有此帳號 
            {
                //這個=加密過
                login.UserPassWord= BitConverter
                    .ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(login.UserPassWord)))
                    .Replace("-", null);

                
                if (customer.UserPassWord == login.UserPassWord)
                {
                    // GenerateToken() 生成新 JwtToken 用法
                    JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
                    string jwtToken = jwtAuthUtil.GenerateToken(customer.Id,customer.UserAccount,customer.UserName,customer.UserThumbnail,customer.Identity);
                    // 登入成功時，回傳登入成功順便夾帶 JwtToken
                    return Ok(new { Status = true, JwtToken = jwtToken });

                    //return Ok("登入成功");
                }
                else
                {
                    return BadRequest("帳號密碼有誤");
                }
            }
            else
            {
                return BadRequest("無此帳號");
            }
        }



        /// <summary>
        /// 會員忘記密碼寄信
        /// </summary>
        //// POST: api/User
        [Route("user/forgetpassword/")]
        public IHttpActionResult Post(ViewModelCust.ForgetPassword forgetPwd)
        {
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            //判斷資料庫中有無此筆資料
            //如有則寄信

            Customer customer = petCityDbContext.Customers.Where(c => c.UserAccount == forgetPwd.UserAccount).FirstOrDefault();

            //string sql= select UserGuid from Customers where UserAccount = '111@hotmail.com'

            if (customer != null) //有此帳號 
            {
                // 網站網址
                string path = /*Request.Url.Scheme + "://" + Request.Url.Authority +*/ Url.Content("https://petcity-booking.netlify.app/");

                // 從信件連結回到重設密碼頁面
                string receivePage = "#/modifypassword/";
                // 信件內容範本
                string body = "請點擊以下連結，返回網站重新設定密碼。<br><br>" + "<a href='" + path + receivePage  + customer.UserGuid + "'  target='_blank'>點此連結</a>";

                // 信件主題
                DateTime now = DateTime.Now;
                string Subject = $"{now.ToString("yyyy/MM/dd")} 重設密碼申請信";

                string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
                string toEmail = forgetPwd.UserAccount;
                string password = ConfigurationManager.AppSettings["EmailPassword"];

                SendGmailMail(fromEmail, toEmail, Subject, body, password);


                var result = new
                {
                    status = "success",
                    message = "已寄出重設密碼申請信",
                };


                return Ok(result);

            }
            else
            {
                return BadRequest("查無此帳號");
            }
        }



        /// <summary>
        /// 會員重設密碼 
        /// </summary>
         ////PUT: api/User/5
        [Route("user/resetpassword/")]
        public IHttpActionResult Put(string guid, ViewModelCust.ResetPassword resetPwd)
        {
            if (resetPwd.NewPassword == resetPwd.ConfirmedPassword)
            {
                // 修改個人資料至資料庫
                //string sql = @"UPDATE Customers SET UserPassWord = 123456 WHERE UserGuid = '503e8b10-eff9-463e-9f7d-7681313100f8'";

                PetCityNewcontext petCityDbContext = new PetCityNewcontext();

                Customer customer = petCityDbContext.Customers.Where(c => c.UserGuid == guid).FirstOrDefault();

                if (customer != null)
                {
                    //customer.UserPassWord = resetPwd.NewPassword;

                    customer.UserPassWord = BitConverter
                        .ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(resetPwd.NewPassword)))
                        .Replace("-", null);



                    petCityDbContext.SaveChanges();

                    var result = new
                    {
                        status= "success",
                        message= "密碼修改成功",
                    };
                    return Ok(result);   //配合200去處理  //還是status code
                }
                else
                {
                    return BadRequest ("無此帳號");
                }
            }
            else
            {
                return BadRequest("新密碼與確認新密碼不相同或格式錯誤");
            }

        }



        // DELETE: api/User/5
        //public void Delete(int id)
        //{
        //}

    }
}