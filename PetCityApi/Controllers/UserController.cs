﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using Microsoft.Owin.Security.Provider;
using NSwag.Annotations;
using PetCityApi1.JWT;
using PetCityApi1.Models;

namespace PetCityApi1.Controllers
{

    [OpenApiTag("Users", Description = "使用者登入註冊CURD")]

    public class UserController : ApiController
    {
        /// <summary>
        /// 會員註冊，不用帶TOKEN
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

            if (ModelState.IsValid && signup.UserPassWord == signup.ConfirmedPassword) //1.格式正確 寫到資料庫  // 寄信到信箱 //註冊成功 
            {
                string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
                string toEmail = signup.UserAccount;
                string password = ConfigurationManager.AppSettings["EmailPassword"];

                // 網站網址
                string path = /*Request.Url.Scheme + "://" + Request.Url.Authority +*/
                    Url.Content("https://petcity-booking.netlify.app/");

                // 從信件連結回到重設密碼頁面
                string receivePage = "#/login";

                // 信件內容範本
                string body = "感謝您註冊寵物坊城市，您已經可以開始使用。。<br><br>" + "<a href='" + path + receivePage +
                              "'  target='_blank'>點此連結</a>";

                string emailbody = "這邊是首頁的連結：/homepage";
                DateTime now = DateTime.Now;
                string Subject = $"{now.ToString("yyyy/MM/dd")}註冊信";

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

        public static void SendGmailMail(string fromAddress, string toAddress, string Subject, string MailBody,
            string password)
        {
            MailMessage mailMessage = new MailMessage(fromAddress, toAddress);
            mailMessage.Subject = Subject;
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = MailBody;
            // SMTP Server
            SmtpClient mailSender = new SmtpClient("smtp.gmail.com");
            System.Net.NetworkCredential basicAuthenticationInfo =
                new System.Net.NetworkCredential(fromAddress, password);
            mailSender.Credentials = basicAuthenticationInfo;
            mailSender.Port = 587;
            mailSender.EnableSsl = true;
            mailSender.Send(mailMessage);
            mailMessage.Dispose();
            mailSender = null;
        }

        /// <summary>
        /// 會員登入 ，不用帶TOKEN
        /// </summary>
        //// POST: api/User
        [Route("user/login/")]
        public IHttpActionResult Post(ViewModelCust.Login login)
        {
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            //Customer cust = new Customer();  //用類別的兩種方式 1.new 物件

            //判斷資料庫中有無此筆資料//如有則登入

            Customer customer = petCityDbContext.Customers.Where(c => c.UserAccount == login.UserAccount)
                .FirstOrDefault(); //2.直接給值

            if (customer != null) //有此帳號 
            {
                //這個=加密過
                login.UserPassWord = BitConverter
                    .ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(login.UserPassWord)))
                    .Replace("-", null);

                if (customer.UserPassWord == login.UserPassWord)
                {
                    // GenerateToken() 生成新 JwtToken 用法
                    JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
                    string jwtToken = jwtAuthUtil.GenerateToken(customer.Id, customer.UserAccount, customer.UserName,
                        customer.UserThumbnail, customer.Identity);

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
        /// 登入後取得大頭貼，帶TOKEN
        /// </summary>
        [Route("user/banner/")]
        [JwtAuthFilter]
        public IHttpActionResult GetBanner()
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string image = (string)userToken["Image"];

            string thumbNail = "";
            if (thumbNail != null)
            {
                thumbNail = "https://petcity.rocket-coding.com/upload/profile/" + image;
            }
            return Ok(thumbNail);
        }

        /// <summary>
        /// 會員忘記密碼寄信，不用帶TOKEN
        /// </summary>
        //// POST: api/User
        [Route("user/forgetpassword/")]
        public IHttpActionResult Post(ViewModelCust.ForgetPassword forgetPwd)
        {
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            //判斷資料庫中有無此筆資料//如有則寄信

            Customer customer = petCityDbContext.Customers.Where(c => c.UserAccount == forgetPwd.UserAccount)
                .FirstOrDefault();

            if (customer != null) //有此帳號 
            {
                // 網站網址
                string path = /*Request.Url.Scheme + "://" + Request.Url.Authority +*/
                    Url.Content("https://petcity-booking.netlify.app/");

                // 從信件連結回到重設密碼頁面
                string receivePage = "#/modifypassword/";

                // 信件內容範本
                string body = "請點擊以下連結，返回網站重新設定密碼。<br><br>" + "<a href='" + path + receivePage + customer.UserGuid +
                              "'  target='_blank'>點此連結</a>";

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
        /// 會員重設密碼 ，不用帶TOKEN
        /// </summary>
        ////PUT: api/User/5
        [Route("user/resetpassword/")]
        public IHttpActionResult Put(string guid, ViewModelCust.ResetPassword resetPwd)
        {
            if (resetPwd.NewPassword == resetPwd.ConfirmedPassword)
            {
                // 修改個人資料至資料庫

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
                        status = "success",
                        message = "密碼修改成功",
                    };
                    return Ok(result); //配合200去處理  //還是status code
                }
                else
                {
                    return BadRequest("無此帳號");
                }
            }
            else
            {
                return BadRequest("新密碼與確認新密碼不相同或格式錯誤");
            }
        }

        /// <summary>
        /// 使用者送出訂單請求-取得飼主資訊,需要帶TOKEN
        ///</summary>
        [Route("user/book/")]
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult Get()
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var user = petCityDbContext.Customers.Where(c => c.Id == customerId).FirstOrDefault();
            if (user == null)
            {
                return BadRequest("無此帳號");
            }

            var result = new
            {
                UserAccount = user.UserAccount,
                UserName = user.UserName,
                UserPhone = user.UserPhone,
            };
            return Ok(new { Status = true, result });
        }

        /// <summary>
        /// 使用者送出訂單請求-檢查區間這間房間有沒有被訂過 返回可否入住訊息- status=reserved  帶TOKEN  
        /// </summary>
        //// POST: api/User
        [Route("user/book/")]
        [JwtAuthFilter]
        public IHttpActionResult Post(ViewModelCust.Book book)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var user = petCityDbContext.Orders.Where(o => o.PetCards.Customer.Id == customerId);

            string checkInDateString = book.CheckInDate;
            DateTime checkinDate = DateTime.Parse(checkInDateString);
            string checkOutDateString = book.CheckOutDate;
            DateTime checkoutDate = DateTime.Parse(checkOutDateString);

            var overlappingOrders = petCityDbContext.Orders
                .Where(o => o.RoomId == book.RoomId)
                .Where(o => o.CheckInDate < checkoutDate && o.CheckOutDate > checkinDate);

            if (overlappingOrders.Any())
            {
                return BadRequest("預約失敗");
            }
            else
            {
                Order order = new Order();

                order.OrderedDate = DateTime.Now;
                order.PetCardId = book.PetCardId;
                order.RoomId = book.RoomId;
                order.CheckInDate = checkinDate;
                order.CheckOutDate = checkoutDate;
                order.TotalNight = book.TotalNight;
                order.TotalPrice = book.TotalPrice;
                order.UserName = book.UserName;
                order.UserPhone = book.UserPhone;
                order.Status = book.Status;

                petCityDbContext.Orders.Add(order);
                petCityDbContext.SaveChanges();

                var result = new
                {
                    status = "success",
                    message = "預約成功",
                    orderId = order.Id,
                };
                return Ok(result);
            }
        }

        /// <summary>
        /// 使用者後台管理-使用者讀取個人資訊-(後台),需要帶TOKEN
        ///</summary>
        [Route("user/")]
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult GetUerInfo()
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var user = petCityDbContext.Customers.Where(c => c.Id == customerId).FirstOrDefault();

            if (user == null)
            {
                return BadRequest("無此帳號");
            }

            string url = null;
            if (user.UserThumbnail != null)
            {
                url = "https://petcity.rocket-coding.com/upload/profile/" + user.UserThumbnail;
            }

            var result = new
            {
                UserPhoto = user.UserThumbnail,
                UserAccount = user.UserAccount,
                UserName = user.UserName,
                UserPhone = user.UserPhone,
                UserAddress = user.UserAddress,
            };
            return Ok(new { Status = true, result });
        }

        /// <summary>
        /// 使用者後台管理-使用者修改個人資訊-(後台),需要帶TOKEN
        ///</summary>
        [Route("user/")]
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult PutUerInfo(ViewModelCust.UserInfo userInfo)
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var user = petCityDbContext.Customers.Where(c => c.Id == customerId).FirstOrDefault();

            if (user != null)
            {
                user.UserName = userInfo.UserName;
                user.UserPhone = userInfo.UserPhone;
                user.UserAddress = userInfo.UserAddress;

                petCityDbContext.SaveChanges();

                var result = new
                {
                    status = "success",
                    message = "修改個人資料成功",
                };
                return Ok(result); //問前端配合200去處理  //還是status code
            }
            return BadRequest("無此帳號");
        }

        /// <summary>
        /// 使用者後台管理-使用者取得"未完成"評價清單-(後台),需要帶TOKEN
        ///</summary>
        [Route("user/notCommentList")]
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult GetNotCommentList()
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.PetCards.Customer.Id == customerId).Where(o => o.Status == "checkOut").ToList();

            //創建一個 List，用來存儲所有訂單
            List<ViewModelCust.CommentList> commentsList = new List<ViewModelCust.CommentList>();

            //遍歷所有訂單
            foreach (var order in orders)
            {
                //將每份訂單資訊存儲到 List 中
                commentsList.Add(new ViewModelCust.CommentList()
                {
                    Id = order.Id,
                    RoomPhoto = order.Rooms.RoomPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + order.Rooms.RoomPhoto,
                    HotelName = order.Rooms.Hotel.HotelName,
                    RoomName = order.Rooms.RoomName,
                    CheckInDate = order.CheckInDate,
                    CheckOutDate = order.CheckOutDate,
                    Status = order.Status,
                });
            }
            //返回訂單結果
            return Ok(new { Status = true, result = commentsList });
        }

        /// <summary>
        ///  使用者後台管理-使用者撰寫並送出我的安心評價-(後台) -status=checkOutComment 要TOKEN
        /// </summary>
        [Route("user/comment")]
        [JwtAuthFilter]
        public IHttpActionResult Put(int orderId, ViewModelCust.SendCommentModel comment)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.PetCards.Customer.Id == customerId).Where(o => o.Id == orderId).Where(o => o.Status == "checkOut").ToList();

            orders[0].Status = comment.Status;
            orders[0].Score = comment.Score;
            orders[0].Comment = comment.Comment;

            petCityDbContext.SaveChanges();

            var result = new
            {
                status = "success",
                message = "送出評價成功",
            };
            return Ok(result);
        }

        /// <summary>
        /// 使用者後台管理-使用者取得"已完成"評價清單-(後台),需要帶TOKEN
        ///</summary>
        [Route("user/commentList")]
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult GetCommentList()
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.PetCards.Customer.Id == customerId).Where(o => o.Status == "checkOutComment").ToList();

            //創建一個 List，用來存儲所有訂單
            List<ViewModelCust.CommentList> commentsList = new List<ViewModelCust.CommentList>();

            //遍歷所有訂單
            foreach (var order in orders)
            {
                //將每份訂單資訊存儲到 List 中
                commentsList.Add(new ViewModelCust.CommentList()
                {
                    Id = order.Id,
                    RoomPhoto = order.Rooms.RoomPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + order.Rooms.RoomPhoto,
                    HotelName = order.Rooms.Hotel.HotelName,
                    RoomName = order.Rooms.RoomName,
                    CheckInDate = order.CheckInDate,
                    CheckOutDate = order.CheckOutDate,
                    Status = order.Status,
                });
            }
            //返回訂單結果
            return Ok(new { Status = true, result = commentsList });
        }

        /// <summary>
        /// 使用者後台管理-使用者查看單一安心評價 -(後台),需要帶TOKEN
        ///</summary>
        [Route("user/comment")]
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult GetComment(int orderId)
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.PetCards.Customer.Id == customerId).Where(o => o.Id == orderId).Where(o => o.Status == "checkOutComment").FirstOrDefault();

            var result = new
            {
                Score = orders.Score,
                Comment = orders.Comment,
            };
            return Ok(new { Status = true, result });
        }

        /// <summary>
        /// 使用者後台管理-使用者查看"完成預約"清單 需要帶TOKEN
        ///</summary>
        [Route("user/reservedList")]
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult GetReservedList()
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.PetCards.Customer.Id == customerId).Where(o => o.Status == "reserved").ToList();

            //創建一個 List，用來存儲所有訂單
            List<ViewModelCust.CommentList> reservedList = new List<ViewModelCust.CommentList>();

            //遍歷所有訂單
            foreach (var order in orders)
            {
                //將每份訂單資訊存儲到 List 中
                reservedList.Add(new ViewModelCust.CommentList()
                {
                    Id = order.Id,
                    RoomPhoto = order.Rooms.RoomPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + order.Rooms.RoomPhoto,
                    HotelName = order.Rooms.Hotel.HotelName,
                    RoomName = order.Rooms.RoomName,
                    CheckInDate = order.CheckInDate,
                    CheckOutDate = order.CheckOutDate,
                    Status = order.Status,
                });
            }
            //返回訂單結果
            return Ok(new { Status = true, result = reservedList });
        }

        /// <summary>
        /// 使用者後台管理-使用者查看"取消預約"清單 需要帶TOKEN
        ///</summary>
        [Route("user/cancelList")]
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult GetCancelList()
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.PetCards.Customer.Id == customerId).Where(o => o.Status == "cancel").ToList();

            //創建一個 List，用來存儲所有訂單
            List<ViewModelCust.CommentList> cancelList = new List<ViewModelCust.CommentList>();

            //遍歷所有訂單
            foreach (var order in orders)
            {
                //將每份訂單資訊存儲到 List 中
                cancelList.Add(new ViewModelCust.CommentList()
                {
                    Id = order.Id,
                    RoomPhoto = order.Rooms.RoomPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + order.Rooms.RoomPhoto,
                    HotelName = order.Rooms.Hotel.HotelName,
                    RoomName = order.Rooms.RoomName,
                    CheckInDate = order.CheckInDate,
                    CheckOutDate = order.CheckOutDate,
                    Status = order.Status,
                });
            }
            //返回訂單結果
            return Ok(new { Status = true, result = cancelList });
        }

        /// <summary>
        /// 使用者後台管理-使用者查看"歷史預約"清單 需要帶TOKEN
        ///</summary>
        [Route("user/completeList")]
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult GetCompleteList()
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.PetCards.Customer.Id == customerId).Where(o => o.Status == "checkOut" || o.Status == "checkOutComment").ToList();

            //創建一個 List，用來存儲所有訂單
            List<ViewModelCust.CommentList> completeList = new List<ViewModelCust.CommentList>();

            //遍歷所有訂單
            foreach (var order in orders)
            {
                //將每份訂單資訊存儲到 List 中
                completeList.Add(new ViewModelCust.CommentList()
                {
                    Id = order.Id,
                    RoomPhoto = order.Rooms.RoomPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + order.Rooms.RoomPhoto,
                    HotelName = order.Rooms.Hotel.HotelName,
                    RoomName = order.Rooms.RoomName,
                    CheckInDate = order.CheckInDate,
                    CheckOutDate = order.CheckOutDate,
                    Status = order.Status,
                });
            }
            //返回訂單結果
            return Ok(new { Status = true, result = completeList });
        }

        /// <summary>
        /// 一般會員後台頁-收藏店家 ，要帶TOKEN (應該是放在homepage)
        /// </summary>
        ////PUT: api/User/5
        [Route("user/keepList/")]
        [JwtAuthFilter]
        public IHttpActionResult Post(int hotelId)
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            KeepList keepList = new KeepList();

            keepList.CustomerId = customerId;
            keepList.HotelId = hotelId;

            petCityDbContext.KeepLists.Add(keepList);
            petCityDbContext.SaveChanges();

            var result = new
            {
                status = "success",
                message = "收藏成功",
            };
            return Ok(result);
        }

        /// <summary>
        /// 一般會員後台頁-取消收藏店家 ，要帶TOKEN
        /// </summary>
        ////PUT: api/User/5
        [Route("user/keepList/")]
        [JwtAuthFilter]
        public IHttpActionResult Delete(int hotelId)
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            var keepList = petCityDbContext.KeepLists.Where(k => k.CustomerId == customerId).Where(k => k.HotelId == hotelId)
                .FirstOrDefault();

            if (keepList != null)
            {
                petCityDbContext.KeepLists.Remove(keepList);
                petCityDbContext.SaveChanges();

                var result = new
                {
                    status = "success",
                    message = "移除收藏成功",
                };
                return Ok(result); //問前端配合200去處理  //還是status code
            }
            return BadRequest("無此收藏資料");
        }

        /// <summary>
        /// 一般會員後台頁-取得收藏店家列表 ，要帶TOKEN
        /// </summary>
        /// <returns></returns>
        [Route("user/keepList/")]
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult GetPetCardList()
        {
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            var keepList = petCityDbContext.KeepLists.Where(k => k.CustomerId == customerId).Select(k => new
            {
                HotelId = k.Id,
                HotelName = k.Hotel.HotelName,

                //三元運算值
                HotelPhoto = k.Hotel.HotelPhotos.Count == 0 ? "" : "https://petcity.rocket-coding.com/upload/profile/" + k.Hotel.HotelPhotos.OrderBy(p => p.Id).FirstOrDefault().Photo,

                HotelScore =
                    Math.Round(
                        (double)k.Hotel.Rooms.Sum(r => r.Orders
                            .Where(o => o.Score != null) // only sum scores that are not null
                            .Sum(o => o.Score)) / k.Hotel.Rooms.Sum(c => c.Orders
                            .Where(o => o.Score != null) // only count orders with non-null scores
                            .Count()), 1),
                ////Room是一個集合物件  所以select會以id分類丟出結果
                ///*   Count= h.Rooms.Select(c => c.Orders.Count)*/ //除錯用

                HotelInfo = k.Hotel.HotelInfo,
            });
            return Ok(new { Status = true, keepList });
        }

        /// <summary>
        /// 4-1.單一旅館資訊頁-前台旅館 (前台) (給收藏訂單的察看旅館業面用) 不用TOKEN
        /// </summary>
        [Route("user/hotelInfo")]
        public IHttpActionResult GetHotelInfo(int hotelId)
        {
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            int photoCount = 5;
            int actTotalCount = petCityDbContext.HotelPhotos.Count();
            if (actTotalCount < 5)
            {
                photoCount = actTotalCount;
            }
            var hotelPhoto = petCityDbContext.HotelPhotos.Where(r => r.HotelId == hotelId)
                .OrderByDescending(r => r.Id).Take(photoCount);
            List<string> hotelPhotoList = new List<string>();
            if (hotelPhoto != null)
            {
                foreach (var photo in hotelPhoto)
                {
                    hotelPhotoList.Add("https://petcity.rocket-coding.com/upload/profile/" + photo.Photo);
                }
            }

            //從資料庫"取資料" //轉成陣列傳給前端
            var serviceTypes = petCityDbContext.Hotels.FirstOrDefault(h => h.Id == hotelId).ServiceTypes;
            List<string> serviceTypeArr = serviceTypes?.Split(',').ToList();

            var orders = petCityDbContext.Orders.Where(o => o.Rooms.Hotel.Id == hotelId).OrderByDescending(o => o.Score).Take(5);

            var hotelComments = orders.Select(o => new
            {
                UserName = o.PetCards.Customer.UserName,
                UserPhoto = o.PetCards.Customer.UserThumbnail,
                Score = o.Score,
                Comment = o.Comment,
            });

            var hotel = petCityDbContext.Hotels.Where(h => h.Id == hotelId).Select(h => new
            {
                HotelId = h.Id,
                HotelPhoto = hotelPhotoList,

                HotelScore =
                    Math.Round(
                        (double)h.Rooms.Sum(r => r.Orders.Sum(o => o.Score)) / h.Rooms.Sum(c => c.Orders.Count), 1),

                HotelName = h.HotelName,
                HotelInfo = h.HotelInfo,
                HotelService = serviceTypeArr,

                HotelComment = hotelComments,

                Room = h.Rooms.Select(r => new
                {
                    Id = r.Id,
                    RoomPhoto = "https://petcity.rocket-coding.com/upload/profile/" + r.RoomPhoto,
                    RoomName = r.RoomName,
                    PetType = r.PetType,
                    RoomPrice = r.RoomPrice,
                    RoomInfo = r.RoomInfo,
                }),
            });
            return Ok(new { Hotel = hotel });
        }

    }


}


