using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Resources;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Security;
using LinqKit;
using NSwag.Annotations;
using PagedList;
using PetCityApi1.JWT;
using PetCityApi1.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PetCityApi1.Controllers
{
    [OpenApiTag("Hotels", Description = "旅館註冊登入CURD")]
    public class HotelController : ApiController
    {
        /// <summary>
        /// 測試用API
        /// </summary>
        [HttpGet]
        [Route("test/")]
        public IHttpActionResult Test()
        {
            return Ok(new { Message = "OK" });
        }

        /// <summary>
        /// 旅館註冊
        /// </summary>
        [Route("hotel/signup/")]
        public IHttpActionResult Post(ViewModelHotel.Signup signup) //複雜型別
        {
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            //判斷資料庫中有無此筆資料
            //如無則新增
            //自訂義參數名稱 => 自訂義參數名稱.物件屬性

            var hotel = petCityDbContext.Hotels.Where(h => h.HotelAccount == signup.HotelAccount).FirstOrDefault();

            if (hotel != null) //有此帳號  //此帳號已被註冊過
            {
                return BadRequest("已註冊過");
                //result="此帳號已註冊過";
            }

            if (ModelState.IsValid && signup.HotelPassWord == signup.ConfirmedPassword)
            //1.格式正確 寫到資料庫  // 寄信到信箱 //註冊成功 
            {
                string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
                string toEmail = signup.HotelAccount;
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

                Hotel hotell = new Hotel();
                string guid = Guid.NewGuid().ToString();

                hotell.HotelAccount = signup.HotelAccount;
                //hotell.HotelPassWord = signup.HotelPassWord;
                hotell.HotelPassWord = BitConverter
                    .ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(signup.HotelPassWord)))
                    .Replace("-", null);
                hotell.HotelName = signup.HotelName;
                hotell.Identity = signup.Identity;
                hotell.HotelGuid = guid;

                petCityDbContext.Hotels.Add(hotell);
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
        /// 旅館登入 
        /// </summary>
        [Route("hotel/login/")]
        public IHttpActionResult Post(ViewModelHotel.Login login)
        {
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            //hotel cust = new hotel();  //使用類別的兩種方式 1.new 物件

            //判斷資料庫中有無此筆資料
            //如有則登入

            Hotel hotel = petCityDbContext.Hotels.Where(h => h.HotelAccount == login.HotelAccount)
                .FirstOrDefault(); //2.直接給值

            if (hotel != null) //有此帳號 
            {
                login.HotelPassWord = BitConverter
                    .ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(login.HotelPassWord)))
                    .Replace("-", null);//這個=加密過

                if (hotel.HotelPassWord == login.HotelPassWord && hotel.Identity == login.Identity)
                {
                    // GenerateToken() 生成新 JwtToken 用法
                    JwtAuthUtil_Hotel jwtAuthUtil_Hotel = new JwtAuthUtil_Hotel();
                    string jwtToken = jwtAuthUtil_Hotel.GenerateToken(hotel.Id, hotel.HotelAccount, hotel.HotelName,
                        hotel.HotelThumbnail, hotel.Identity);
                    // 登入成功時，回傳登入成功順便夾帶 JwtToken
                    return Ok(new { Status = true, JwtToken = jwtToken });
                    //return Ok("登入成功");
                }
                else
                {
                    return BadRequest("密碼有誤或身分不符");
                }
            }
            else
            {
                return BadRequest("無此帳號");
            }
        }

        /// <summary>
        /// 旅館忘記密碼寄信
        /// </summary>
        [Route("hotel/forgetpassword/")]
        public IHttpActionResult Post(ViewModelHotel.ForgetPassword forgetPwd)
        {
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            //判斷資料庫中有無此筆資料
            //如有則寄信

            Hotel hotel = petCityDbContext.Hotels.Where(h => h.HotelAccount == forgetPwd.HotelAccount).FirstOrDefault();

            if (hotel != null) //有此帳號 
            {
                // 網站網址
                string path = /*Request.Url.Scheme + "://" + Request.Url.Authority +*/
                    Url.Content("https://petcity-booking.netlify.app/");

                // 從信件連結回到重設密碼頁面
                string receivePage = "#/modifypassword/";

                // 信件內容範本
                string body = "請點擊以下連結，返回網站重新設定密碼。<br><br>" + "<a href='" + path + receivePage + hotel.HotelGuid +
                              "'  target='_blank'>點此連結</a>";

                // 信件主題
                DateTime now = DateTime.Now;
                string Subject = $"{now.ToString("yyyy/MM/dd")} 重設密碼申請信";

                string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
                string toEmail = forgetPwd.HotelAccount;
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
        /// 旅館重設密碼 
        /// </summary>
        [Route("hotel/resetpassword/")]
        public IHttpActionResult Put(string guid, ViewModelHotel.ResetPassword resetPwd)
        {
            if (resetPwd.NewPassword == resetPwd.ConfirmedPassword)
            {
                // 修改個人資料至資料庫

                PetCityNewcontext petCityDbContext = new PetCityNewcontext();
                Hotel hotel = petCityDbContext.Hotels.Where(h => h.HotelGuid == guid).FirstOrDefault();

                if (hotel != null)
                {
                    //hotel.HotelPassWord = resetPwd.NewPassword;
                    hotel.HotelPassWord = BitConverter
                        .ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(resetPwd.NewPassword)))
                        .Replace("-", null);

                    petCityDbContext.SaveChanges();

                    var result = new
                    {
                        status = "success",
                        message = "密碼修改成功",
                    };
                    return Ok(result); //問前端配合200去處理  //還是status code
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
        /// 取得地區
        /// </summary>
        [HttpGet]
        [Route("area/")]
        public IHttpActionResult GetArea()
        {
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            List<Area> areas = petCityDbContext.Areas.ToList();

            return Ok(areas);
        }

        /// <summary>
        /// 寵物旅館後台管理-業者讀取旅館資訊-後台
        /// </summary>
        [JwtAuthFilter_Hotel] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        [Route("hotel/")]
        public IHttpActionResult Get()
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hotelId = (int)userToken["Id"];

            //1.有取到 JwtToken 後，判斷授權格式不存在且不正確時，請重新登入  記得加上Bearer
            //2.檢查有效期限是否過期，如 JwtToken 過期，需導引重新登入
            //3.解密失敗，請重新登入

            // Do Something ~
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            Hotel hotel = petCityDbContext.Hotels.Where(h => h.Id == hotelId).FirstOrDefault();
            if (hotel == null)
            {
                return BadRequest("無此帳號");
            }

            //從資料庫"取資料" //轉成陣列傳給前端
            var foodTypes = petCityDbContext.Hotels.FirstOrDefault(h => h.Id == hotelId).FoodTypes;
            List<string> foodTypeArr = foodTypes?.Split(',').ToList();
            var serviceTypes = petCityDbContext.Hotels.FirstOrDefault(h => h.Id == hotelId).ServiceTypes;
            List<string> serviceTypeArr = serviceTypes?.Split(',').ToList();

            List<string> hotelPhotoList = new List<string>();

            //因為沒有照片 System.NullReferenceException
            //讀資料庫數量 如果總數量小於5 就取總數量  剛好或是超過5就是5張

            int photoCount = 5;
            int actTotalCount = petCityDbContext.HotelPhotos.Count();
            if (actTotalCount < 5)
            {
                photoCount = actTotalCount;
            }
            var hotelPhoto = petCityDbContext.HotelPhotos.Where(r => r.HotelId == hotelId)
                .OrderByDescending(r => r.Id).Take(photoCount);

            if (hotelPhoto != null)
            {
                foreach (var photo in hotelPhoto)
                {
                    hotelPhotoList.Add("https://petcity.rocket-coding.com/upload/profile/" + photo.Photo);
                }
            }

            //string url = "https://petcity.rocket-coding.com/upload/profile/" + hotel.HotelThumbnail;
            //if (hotel.HotelThumbnail != null)
            //{
            //    hotel.HotelThumbnail = url;               //這樣值會被蓋過去
            //}
            string url = null;
            if (hotel.HotelThumbnail != null)
            {
                url = "https://petcity.rocket-coding.com/upload/profile/" + hotel.HotelThumbnail;
            }

            var result = new //這樣會用兩份記憶體
            {
                HotelName = hotel.HotelName,
                HotelPhone = hotel.HotelPhone,
                HotelArea = hotel.Area?.Areas,              //? 如果是null回傳null  如果有值就回傳值
                HotelAddress = hotel.HotelAddress,
                HotelStartTime = hotel.HotelStartTime,
                HotelEndTime = hotel.HotelEndTime,
                HotelInfo = hotel.HotelInfo,

                //FoodTypes = hotel.FoodTypes,
                FoodTypes = foodTypeArr,
                //ServiceTypes = hotel.ServiceTypes,
                ServiceTypes = serviceTypeArr,

                HotelPhotos = hotelPhotoList,
                HotelThumbnail = url,
            };

            // 處理完請求內容
            return Ok(new { Status = true, result });
        }

        /// <summary>
        ///  寵物旅館後台管理-業者修改旅館資訊-後台
        /// </summary>
        [JwtAuthFilter_Hotel] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        [Route("hotel/")]
        public IHttpActionResult Put(ViewModelHotel.ModifyHotel modifyHotel)
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去put資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hotelId = (int)userToken["Id"];

            // Do Something ~
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            Hotel hotel = petCityDbContext.Hotels.Where(h => h.Id == hotelId).FirstOrDefault();

            //前端傳陣列來  //組字串存進資料 不用再modifyHotel.FoodTypes.Split(',');
            string resultStartTime = modifyHotel.HotelBusinessTime[0];
            string resultEndtTime = modifyHotel.HotelBusinessTime[1];

            string resultFoodType = "";
            foreach (var type in modifyHotel.FoodTypes)
            {
                resultFoodType += type + ",";
            }
            resultFoodType = resultFoodType.TrimEnd(',');

            string resultServiceType = "";
            foreach (var type in modifyHotel.ServiceTypes)
            {
                resultServiceType += type + ",";
            }
            resultServiceType = resultServiceType.TrimEnd(',');

            if (hotel != null)
            {
                hotel.HotelName = modifyHotel.HotelName;
                hotel.HotelPhone = modifyHotel.HotelPhone;

                hotel.AreaId = modifyHotel.AreaId;

                hotel.HotelAddress = modifyHotel.HotelAddress;
                hotel.HotelStartTime = resultStartTime;
                hotel.HotelEndTime = resultEndtTime;
                hotel.HotelInfo = modifyHotel.HotelInfo;

                //hotel.FoodTypes = modifyHotel.FoodTypes;
                hotel.FoodTypes = resultFoodType;
                //hotel.ServiceTypes = modifyHotel.ServiceTypes;
                hotel.ServiceTypes = resultServiceType;

                //hotel.HotelPhoto = modifyHotel.HotelPhoto;
                //hotel.HotelThumbnail = modifyHotel.HotelThumbnail;

                petCityDbContext.SaveChanges();

                // 處理完請求內容
                var result = new
                {
                    status = "success",
                    message = "已修改成功",
                };

                return Ok(result);
            }
            else
            {
                return BadRequest("無此帳號");
            }
        }

        /// <summary>
        /// 上傳大頭貼圖片用
        /// </summary>
        [Route("hotel/uploadprofile/")]
        [HttpPost]
        [JwtAuthFilter_Hotel] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public async Task<IHttpActionResult> UploadProfile() //Async Task 非同步
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hoteld = (int)userToken["Id"];


            // 檢查請求是否包含 multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            // 檢查資料夾是否存在，若無則建立
            string root = HttpContext.Current.Server.MapPath(@"~/upload/profile");
            //if (!Directory.Exists(root))
            //{
            //    Directory.CreateDirectory(@"~/upload/profile");
            //}

            try
            {
                // 讀取 MIME(媒體類別) 資料
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // 取得檔案副檔名，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                string fileNameData = provider.Contents.FirstOrDefault().Headers.ContentDisposition.FileName.Trim('\"');
                string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // .jpg

                // 定義檔案名稱
                string fileName = "Hotel" + "Profile" + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;

                // 儲存圖片，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                var fileBytes = await provider.Contents.FirstOrDefault().ReadAsByteArrayAsync();
                var outputPath = Path.Combine(root, fileName);
                using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                }

                // 使用 SixLabors.ImageSharp 調整圖片尺寸 (正方形大頭貼)
                var image = SixLabors.ImageSharp.Image.Load<Rgba32>(outputPath);
                image.Mutate(x => x.Resize(120, 120)); // 輸入(120, 0)會保持比例出現黑邊
                image.Save(outputPath);

                PetCityNewcontext petCityDbContext = new PetCityNewcontext();
                Hotel hotel = petCityDbContext.Hotels.Where(h => h.Id == hoteld).FirstOrDefault();
                hotel.HotelThumbnail = fileName;

                petCityDbContext.SaveChanges();

                return Ok(new
                {
                    Status = true,
                    Data = new
                    {
                        FileName = "https://petcity.rocket-coding.com/upload/profile/" + fileName
                        //https://localhost:44385/upload/profile/
                    }
                });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message); // 400
            }
        }

        /// <summary>
        /// 上傳旅館圖片用
        /// </summary>
        [Route("hotel/uploadhotelphotos/")]
        [HttpPost]
        [JwtAuthFilter_Hotel] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public async Task<IHttpActionResult> UploadHotelPhotos() //Async Task 非同步
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hotelId = (int)userToken["Id"];

            // 檢查請求是否包含 multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            // 檢查資料夾是否存在，若無則建立
            string root = HttpContext.Current.Server.MapPath(@"~/upload/profile");
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(@"~/upload/profile");
            }

            //移除前一次上傳的東西
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var hotelPhoto = petCityDbContext.HotelPhotos.Where(r => r.HotelId == hotelId).ToList();
            if (hotelPhoto.Count > 0)
            {
                foreach (var item in hotelPhoto)
                {
                    petCityDbContext.HotelPhotos.Remove(item);
                }
                petCityDbContext.SaveChanges();
            }

            try
            {
                // 讀取 MIME(媒體類別) 資料
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                List<string> photoList = new List<string>();
                int photonum = 1;

                foreach (var file in provider.Contents)
                {
                    // 取得檔案副檔名，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                    string fileNameData = file.Headers.ContentDisposition.FileName.Trim('\"');
                    string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // .jpg

                    // 定義檔案名稱
                    //Task.Delay(1000);
                    string fileName = "Hotel" + "Photo" + DateTime.Now.ToString("yyyyMMddHHmmss") + photonum +
                                      fileType; //延遲delay
                    photonum++;

                    // 儲存圖片，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                    var fileBytes = await file.ReadAsByteArrayAsync();
                    var outputPath = Path.Combine(root, fileName);
                    using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                    }

                    // 使用 SixLabors.ImageSharp 調整圖片尺寸 (正方形大頭貼)
                    var image = SixLabors.ImageSharp.Image.Load<Rgba32>(outputPath);
                    //image.Mutate(x => x.Resize(120, 120)); // 輸入(120, 0)會保持比例出現黑邊
                    image.Save(outputPath);

                    //將檔名存進資料庫
                    //PetCityNewcontext petCityDbContext = new PetCityNewcontext();
                    HotelPhoto hotelPhotos = new HotelPhoto();

                    hotelPhotos.Photo = fileName;
                    hotelPhotos.HotelId = hotelId;
                    petCityDbContext.HotelPhotos.Add(hotelPhotos);
                    petCityDbContext.SaveChanges();

                    photoList.Add("https://petcity.rocket-coding.com/upload/profile/" + fileName);
                    //https://localhost:44385/upload/profile/
                }

                return Ok(new { Status = true, photoList, });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message); // 400
            }
        }

        /// <summary>
        /// 上傳房型照片用
        /// </summary>
        [Route("hotel/uploadroomphoto/")]
        [HttpPost]
        [JwtAuthFilter_Hotel] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public async Task<IHttpActionResult> UploadRoomPhoto([FromUri] int roomId) //Async Task 非同步
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hotelId = (int)userToken["Id"];


            // 檢查請求是否包含 multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            // 檢查資料夾是否存在，若無則建立
            string root = HttpContext.Current.Server.MapPath(@"~/upload/profile");
            //if (!Directory.Exists(root))
            //{
            //    Directory.CreateDirectory(@"~/upload/profile");
            //}

            try
            {
                // 讀取 MIME(媒體類別) 資料
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // 取得檔案副檔名，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                string fileNameData = provider.Contents.FirstOrDefault().Headers.ContentDisposition.FileName.Trim('\"');
                string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // .jpg

                // 定義檔案名稱
                string fileName = "Room" + "Photo" + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;

                // 儲存圖片，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                var fileBytes = await provider.Contents.FirstOrDefault().ReadAsByteArrayAsync();
                var outputPath = Path.Combine(root, fileName);
                using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                }

                // 使用 SixLabors.ImageSharp 調整圖片尺寸 (正方形大頭貼)
                var image = SixLabors.ImageSharp.Image.Load<Rgba32>(outputPath);

                //image.Mutate(x => x.Resize(120, 120)); // 輸入(120, 0)會保持比例出現黑邊
                image.Save(outputPath);


                PetCityNewcontext petCityDbContext = new PetCityNewcontext();
                var room = petCityDbContext.Rooms.Where(r => r.HotelId == hotelId).Where(r => r.Id == roomId).ToList();
                room[0].RoomPhoto = fileName;

                petCityDbContext.SaveChanges();

                return Ok(new
                {
                    Status = true,
                    Data = new
                    {
                        FileName = "https://petcity.rocket-coding.com/upload/profile/" + fileName
                    }
                });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message); // 400
            }
        }

        /// <summary>
        /// 寵物旅館後台管理-讀取房型列表 (後台)
        /// </summary>
        [HttpGet]
        [Route("hotel/room/list")]
        [JwtAuthFilter_Hotel] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult GetRoomList()
        {
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hotelId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            var rooms = petCityDbContext.Rooms.Where(r => r.HotelId == hotelId)
                .ToList();

            List<Room> roomList = new List<Room>();

            foreach (var room in rooms)
            {
                var result = new Room() //這樣會用兩份記憶體
                {
                    Id = room.Id,
                    RoomPhoto = "https://petcity.rocket-coding.com/upload/profile/" + room.RoomPhoto,
                    RoomName = room.RoomName,
                    PetType = room.PetType,
                    RoomPrice = room.RoomPrice,
                    RoomInfo = room.RoomInfo,
                };

                roomList.Add(result);
            }

            return Ok(new { Status = true, roomList });
        }

        /// <summary>
        ///  寵物旅館後台管理-房型列表-新增房型 (後台)
        /// </summary>
        [JwtAuthFilter_Hotel] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        [Route("hotel/room/")]
        public IHttpActionResult Post(ViewModelHotel.Room addRoom)
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去put資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hotelId = (int)userToken["Id"];

            // Do Something ~
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            var room = petCityDbContext.Rooms.Where(r => r.HotelId == hotelId).Select(r => r.Id).ToList();//由下往上找

            Room rooms = new Room();
            rooms.HotelId = hotelId;
            //rooms.RoomPhoto = addRoom.RoomPhoto;
            rooms.RoomName = addRoom.RoomName;
            rooms.PetType = addRoom.PetType;
            rooms.RoomPrice = addRoom.RoomPrice;
            rooms.RoomInfo = addRoom.RoomInfo;

            petCityDbContext.Rooms.Add(rooms);
            petCityDbContext.SaveChanges();

            var result = new
            {
                status = "success",
                message = "新增房型成功",
                roomid = rooms.Id,
            };
            return Ok(new { Status = true, result }); //問前端配合200去處理  //還是status code
        }

        /// <summary>
        ///  寵物旅館後台管理-房型列表-讀取房型 (後台)
        /// </summary>
        [JwtAuthFilter_Hotel] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        [Route("hotel/room/")]
        public IHttpActionResult Get(int roomId)
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hoteld = (int)userToken["Id"];

            // Do Something ~
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            //var room = petCityDbContext.Rooms.Where(r => r.HotelId == hoteld && r.Id == roomid).Select(r => new
            //{
            //    r.Id,
            //    r.RoomPhoto,
            //    r.RoomName,
            //    r.PetType,
            //    r.RoomPrice,
            //    r.RoomInfo,

            //}).ToList().FirstOrDefault();

            var room = petCityDbContext.Rooms.Where(r => r.HotelId == hoteld && r.Id == roomId).ToList()
                .FirstOrDefault();

            var result = new //這樣會用兩份記憶體
            {
                Id = room.Id,
                RoomPhoto = "https://petcity.rocket-coding.com/upload/profile/" + room.RoomPhoto,
                RoomName = room.RoomName,
                RoomPrice = room.RoomPrice,
                RoomInfo = room.RoomInfo,
            };

            if (room != null)
            {
                // 處理完請求內容
                return Ok(new { Status = true, result });
            }
            return BadRequest("無此房型");
        }

        /// <summary>
        ///  寵物旅館後台管理-房型列表-修改房型 (後台)
        /// </summary>
        [JwtAuthFilter_Hotel] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        [Route("hotel/room/")]
        public IHttpActionResult Put(int roomId, ViewModelHotel.Room addRoom)
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去put資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hoteld = (int)userToken["Id"];

            // Do Something ~
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            Room room = petCityDbContext.Rooms.Where(r => r.HotelId == hoteld).Where(r => r.Id == roomId)
                .FirstOrDefault();

            if (room != null)
            {
                //room.RoomPhoto = addRoom.RoomPhoto;
                room.RoomName = addRoom.RoomName;
                room.PetType = addRoom.PetType;
                room.RoomPrice = addRoom.RoomPrice;
                room.RoomInfo = addRoom.RoomInfo;

                petCityDbContext.SaveChanges();

                var result = new
                {
                    status = "success",
                    message = "修改房型成功",
                };
                return Ok(result); //問前端配合200去處理  //還是status code
            }

            return BadRequest("無此房型");
        }

        /// <summary>
        ///  寵物旅館後台管理-房型列表-刪除房型 (後台)
        /// </summary>
        [JwtAuthFilter_Hotel] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        [Route("hotel/room/")]
        public IHttpActionResult Delete(int roomId)
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去put資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hoteld = (int)userToken["Id"];

            // Do Something ~
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            Room room = petCityDbContext.Rooms.Where(r => r.HotelId == hoteld).Where(r => r.Id == roomId)
                .FirstOrDefault();

            if (room != null)
            {
                petCityDbContext.Rooms.Remove(room);
                petCityDbContext.SaveChanges();

                var result = new
                {
                    status = "success",
                    message = "刪除房型成功",
                };
                return Ok(result); //問前端配合200去處理  //還是status code
            }
            return BadRequest("無此房型");
        }

        /// <summary>
        /// Hotel資料
        /// </summary>
        class HotelData //Function大 類別大 屬性大
        {
            public string RoomLowPrice { get; set; }
            public int HotelId { get; set; }
            public string HotelName { get; set; }
            public string HotelPhoto { get; set; }
            public double HotelScore { get; set; }
            public string HotelInfo { get; set; }
        }

        /// <summary>
        /// 類別舉例
        /// </summary>
        public class FoodTypeList
        {
            /// <summary>
            /// 食物偏好
            /// </summary>
            public string FoodType { get; set; }
            public List<string> Data { get; set; } = new List<string>();
        }

        /// <summary>
        /// 畫面資料
        /// </summary>
        class HotelViewModle
        {
            /// <summary>
            /// Hotel資料清單
            /// </summary>
            public List<HotelData> Data { get; set; } //List 裡面放什麼型別都可以

            /// <summary>
            /// 總頁數
            /// </summary>
            public int Totalpage { get; set; }

            /// <summary>
            /// 目前第幾頁
            /// </summary>
            public int Nowpage { get; set; }
        }

        /// <summary>
        ///  使用者讀取旅館列表與分頁功能 (homepage) (前台)
        /// </summary>
        [Route("hotel/hotelList")]
        public IHttpActionResult GetHotelList(int page = 1, int pageSize = 5)
        {
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var hotels = petCityDbContext.Hotels.AsQueryable(); //宣告好指令 但不執行

            int total = 0;
            if (hotels.Count() % pageSize == 0)
            {
                total = hotels.Count() / pageSize;
            }
            else
            {
                total = hotels.Count() / pageSize + 1;
            }

            var hotel = new HotelViewModle
            {
                //集合列表                     //選取             //物件
                Data = petCityDbContext.Hotels.Select(h => new HotelData
                {
                    RoomLowPrice = h.Rooms.OrderBy(r => r.RoomPrice).FirstOrDefault().RoomPrice,
                    HotelId = h.Id,
                    HotelName = h.HotelName,
                    HotelPhoto = "https://petcity.rocket-coding.com/upload/profile/" +
                                 h.HotelPhotos.OrderBy(p => p.Id).FirstOrDefault().Photo,
                    HotelScore =
                        Math.Round(
                            (double)h.Rooms.Sum(r => r.Orders.Sum(o => o.Score)) / h.Rooms.Sum(c => c.Orders.Count), 1),
                    //Room是一個集合物件  所以select會以id分類丟出結果
                    /*   Count= h.Rooms.Select(c => c.Orders.Count)*/ //除錯用
                    HotelInfo = h.HotelInfo,
                }).ToList(), //選取完東西後 轉換成一個列表 ToList不一定要寫

                Totalpage = total,
                Nowpage = 1
            };

            return Ok(hotel);
        }

        /// <summary>
        /// 4-1.單一旅館資訊頁-前台旅館 (前台)
        /// </summary>
        [Route("hotel/hotelInfo")]
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

            //var rooms = petCityDbContext.Rooms.Where(r => r.HotelId == hotelId)
            //    .ToList();
            //List<Room> roomList = new List<Room>();
            //foreach (var room in rooms)
            //{
            //    var resultroom = new Room() //這樣會用兩份記憶體
            //    {
            //        Id = room.Id,
            //        RoomPhoto = "https://petcity.rocket-coding.com/upload/profile/" + room.RoomPhoto,
            //        RoomName = room.RoomName,
            //        PetType = room.PetType,
            //        RoomPrice = room.RoomPrice,
            //        RoomInfo = room.RoomInfo,
            //    };
            //    roomList.Add(resultroom);
            //}

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

        ///// <summary>
        /////  篩選 DEMO
        ///// </summary>
        //[Route("hotel/hotelFilterDemo")]
        //public IHttpActionResult GetHotelFilterDemo()
        //{
        //    //判斷有沒有值 再去做後續動作
        //    //判斷陣列長度 有沒有大於0

        //    int areaId = 11;
        //    string[] foodTypes = { "wetFood", "freshFood", "dryFood" };
        //    string[] serviceTypes = { "pickup", "bath", "123" };

        //    PetCityNewcontext petCityDbContext = new PetCityNewcontext();

        //    var hotels = petCityDbContext.Hotels.AsQueryable();
        //    if (areaId != 0)  //單一物件可以用Where
        //    {
        //        hotels = hotels.Where(h => h.AreaId == areaId);
        //    }

        //    var predicate = PredicateBuilder.New<Hotel>();

        //    if (foodTypes.Length > 0)
        //    {
        //        for (int i = 0; i < foodTypes.Length; i++)
        //        {
        //            var type = foodTypes[i];
        //            predicate = predicate.Or(h => h.FoodTypes.Contains(type));
        //            //hotels = hotels.Where(h => h.FoodTypes.Contains(type));
        //        }
        //    }

        //    if (serviceTypes.Length > 0)
        //    {
        //        for (int j = 0; j < serviceTypes.Length; j++)
        //        {
        //            var type = serviceTypes[j];
        //            predicate = predicate.Or(h => h.FoodTypes.Contains(type));
        //            //hotels = hotels.Where(h => h.FoodTypes.Contains(type));
        //        }
        //    }

        //    //hotels = hotels.Where(predicate);
        //    return Ok(hotels);
        //}


        /// <summary>
        ///  正式篩選  (先勿用 尚未完成)
        /// </summary>
        [Route("hotel/hotelFilter")]
        public IHttpActionResult PostHotelFilter(ViewModelHotel.Filter filter)
        {
            //判斷有沒有值 再去做後續動作
            //判斷陣列長度 有沒有大於0

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            var predicateHotel = PredicateBuilder.New<Hotel>();  //起手式  //使用套件下的
            var hotels = petCityDbContext.Hotels.AsQueryable(); //跟資料庫有關  只是先放著不執行

            if (filter.AreaId != 0)  //單一物件可以用Where
            {
                hotels = hotels.Where(h => h.AreaId == filter.AreaId);
            }

            if (filter.FoodTypes.Length > 0)
            {
                for (int i = 0; i < filter.FoodTypes.Length; i++)
                {
                    var type = filter.FoodTypes[i];
                    predicateHotel = predicateHotel.Or(h => h.FoodTypes.Contains(type));
                    //hotels = hotels.Where(h => h.FoodTypes.Contains(type));
                }
            }

            if (filter.ServiceTypes.Length > 0)
            {
                for (int j = 0; j < filter.ServiceTypes.Length; j++)
                {
                    var type = filter.ServiceTypes[j];
                    predicateHotel = predicateHotel.Or(h => h.FoodTypes.Contains(type));
                    //hotels = hotels.Where(h => h.FoodTypes.Contains(type));
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.PetType))  //單一物件可以用Where
            {                    //Where只能使用一次    //任何跟他相符的
                hotels = hotels.Where(h => h.Rooms.Any(r=>r.PetType == filter.PetType) );
            }

            //用時間判斷 找出那些房間不行，扣掉不行的就是要的 //這段區間內可以被訂的旅館
            if (!string.IsNullOrWhiteSpace(filter.CheckInDate) && !string.IsNullOrWhiteSpace(filter.CheckOutDate))
            {
                string checkInDateString = filter.CheckInDate;
                DateTime checkinDate = DateTime.Parse(checkInDateString);

                string checkOutDateString = filter.CheckOutDate;
                DateTime checkoutDate = DateTime.Parse(checkOutDateString);

                //hotels = hotels.Where(h => h.Rooms.Any(r => r.Orders.Any(o => o.CheckInDate >= checkinDate && o.CheckOutDate < checkoutDate)));

                //先使用Where方法在petCityDbContext.Hotels集合中篩選出符合條件的旅店。條件是：每個旅店都必須滿足以下條件：
                //旅店有房間，並且這些房間都有預訂單，而且每個預訂單的入住日期都大於checkinDate，以及退房日期都小於checkoutDate。這樣，就可以選擇退出符合條件的旅店。

                hotels = petCityDbContext.Hotels.Where(h => h.Rooms.Any(r => r.Orders.Any(o => o.CheckInDate >= checkinDate && o.CheckOutDate < checkoutDate)));
            }

            //hotels = hotels.Where(predicate);

            return Ok(hotels);
        }





    }
}





