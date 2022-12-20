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
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Security;
using System.Web.UI.WebControls;
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
    /// <summary>
    /// Hotel資料
    /// </summary>
    class HotelData //Function大 類別大 屬性大
    {
        public int? RoomLowPrice { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public string HotelPhoto { get; set; }
        public double? HotelScore { get; set; }
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
    class HotelViewModel
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
    /// 圖片回傳模型
    /// </summary>
    public class ImageResponse
    {
        /// <summary>
        /// 圖片識別碼
        /// </summary>
        public int ImageId { get; set; }
        /// <summary>
        /// 圖片base64
        /// </summary>
        public string Base64 { get; set; }
        /// <summary>
        /// 副檔名
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// 圖片Url
        /// </summary>
        public string ImageUrl { get; set; }
    }
    /// <summary>
    /// 圖片異動模型
    /// </summary>
    public class ModifyImageRequest
    {
        /// <summary>
        /// 本次要新增的圖片Base64 [{Base64="",Extension=".jpg"},{Base64="",Extension=".jpg"}]
        /// </summary>
        public List<AddImageModel> AddImage { get; set; }

        /// <summary>
        /// 本次要刪除的圖片的資料庫ID [1,2,3]
        /// </summary>
        public List<int> DelImage { get; set; }
    }

    public class AddImageModel    //檔案內容  副檔名
    {
        public string Base64 { get; set; }

        public string Extension { get; set; }
    }



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
        /// 旅館註冊,不用帶TOKEN identity=hotel
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

            //1.格式正確 寫到資料庫  // 寄信到信箱 //註冊成功 
            if (ModelState.IsValid && signup.HotelPassWord == signup.ConfirmedPassword)
            {
                string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
                string toEmail = signup.HotelAccount;
                string password = ConfigurationManager.AppSettings["EmailPassword"];

                // 網站網址
                string path = /*Request.Url.Scheme + "://" + Request.Url.Authority +*/
                    Url.Content("https://petcity-booking.netlify.app/");

                // 從信件連結回到登入頁面
                string receivePage = "#/login";

                // 信件內容範本
                string body = "感謝您註冊寵物坊城市，您已經可以開始使用。<br><br>" + "<a href='" + path + receivePage +
                              "'  target='_blank'>點此連結</a>";

                //string emailbody = "這邊是首頁的連結：/homepage";
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
        /// 旅館登入，不用帶TOKEN
        /// </summary>
        [Route("hotel/login/")]
        public IHttpActionResult Post(ViewModelHotel.Login login)
        {
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            //hotel cust = new hotel();  //使用類別的兩種方式 1.new 物件

            //判斷資料庫中有無此筆資料//如有則登入

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

                    string thumbNail = "";
                    if (hotel.HotelThumbnail != null)
                    {
                        thumbNail = "https://petcity.rocket-coding.com/upload/profile/" + hotel.HotelThumbnail;
                    }

                    // 登入成功時，回傳登入成功順便夾帶 JwtToken
                    return Ok(new { Status = true, JwtToken = jwtToken, Image = thumbNail });
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

        ///// <summary>
        ///// 登入後取得大頭貼，帶TOKEN
        ///// </summary>
        //[Route("hotel/banner/")]
        //[JwtAuthFilter_Hotel]
        //public IHttpActionResult GetBanner()
        //{
        //    var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
        //    string image = (string)userToken["Image"];

        //    string thumbNail = "";
        //    if (thumbNail != null)
        //    {
        //        thumbNail = "https://petcity.rocket-coding.com/upload/profile/" + image;
        //    }
        //    return Ok(thumbNail);
        //}

        /// <summary>
        /// 旅館忘記密碼寄信，不用帶TOKEN
        /// </summary>
        [Route("hotel/forgetpassword/")]
        public IHttpActionResult Post(ViewModelHotel.ForgetPassword forgetPwd)
        {
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            //判斷資料庫中有無此筆資料//如有則寄信

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
        /// 旅館重設密碼，不用帶TOKEN
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
        /// 取得地區，不用帶TOKEN
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
        /// 寵物旅館後台管理-業者讀取旅館資訊-(後台),需要帶TOKEN
        /// </summary>
        [JwtAuthFilter_Hotel] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        [Route("hotel/")]
        public IHttpActionResult Get()
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hotelId = (int)userToken["Id"];

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

            //List<string> hotelPhotoList = new List<string>();
            List<ImageResponse> hotelPhotoList = new List<ImageResponse>();

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
                    //前端要get圖片  //取出來的時候是一個資料流/文字... 放在byte //轉成base64給前端
                    string base64 = "";
                    string filePath = HttpContext.Current.Server.MapPath(@"~/upload/profile") + "/" + photo.Photo;
                    //讀取
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        byte[] buffer = new byte[fileStream.Length];
                        fileStream.Read(buffer, 0, buffer.Length);
                        base64 = Convert.ToBase64String(buffer);
                        fileStream.Close();
                    }
                    hotelPhotoList.Add(new ImageResponse()
                    {
                        ImageId = photo.Id,
                        Base64 = base64,
                        //拿到小數點後第一個字a.b
                        Extension = photo.Photo.Split('.').LastOrDefault(),
                        ImageUrl = "https://petcity.rocket-coding.com/upload/profile/" + photo.Photo,
                    });
                }
            }
            //這樣值會被蓋過去不建議
            //string url = "https://petcity.rocket-coding.com/upload/profile/" + hotel.HotelThumbnail;
            //if (hotel.HotelThumbnail != null)
            //{
            //    hotel.HotelThumbnail = url;             
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

                HotelArea = hotel.Area?.Id,
                /*  HotelArea = hotel.Area?.Areas,     */         //? 如果是null回傳null  如果有值就回傳值
                HotelAddress = hotel.HotelAddress,
                HotelStartTime = hotel.HotelStartTime,
                HotelEndTime = hotel.HotelEndTime,
                HotelInfo = hotel.HotelInfo,

                //FoodTypes = hotel.FoodTypes, //要給前端陣列
                FoodTypes = foodTypeArr,
                //ServiceTypes = hotel.ServiceTypes, //要給前端陣列
                ServiceTypes = serviceTypeArr,

                HotelPhotos = hotelPhotoList,
                HotelThumbnail = url,
            };

            // 處理完請求內容
            return Ok(new { Status = true, result });
        }

        /// <summary>
        ///  寵物旅館後台管理-業者修改旅館資訊-(後台),需要帶TOKEN
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

                //hotel.FoodTypes = modifyHotel.FoodTypes; //前端給我轉字串
                hotel.FoodTypes = resultFoodType;
                //hotel.ServiceTypes = modifyHotel.ServiceTypes; //前端給我轉字串
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
        /// 上傳旅館大頭貼圖片用,需要帶TOKEN
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
            string hotelName = userToken["Name"].ToString();

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

            try
            {
                // 讀取 MIME(媒體類別) 資料
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // 取得檔案副檔名，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                string fileNameData = provider.Contents.FirstOrDefault().Headers.ContentDisposition.FileName.Trim('\"');
                string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // .jpg

                // 定義檔案名稱
                string fileName = hotelName + "Profile" + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;

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
                    }
                });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message); // 400
            }
        }

        /// <summary>
        /// 上傳旅館圖片用,需要帶TOKEN
        /// </summary>
        [Route("hotel/uploadhotelphotos/")]
        [HttpPost]
        [JwtAuthFilter_Hotel] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public async Task<IHttpActionResult> UploadHotelPhotos(ModifyImageRequest request) //Async Task 非同步
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hotelId = (int)userToken["Id"];
            string hotelName = userToken["Name"].ToString();

            // 檢查資料夾是否存在，若無則建立
            string root = HttpContext.Current.Server.MapPath(@"~/upload/profile");
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(@"~/upload/profile");
            }

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var hotelPhoto = petCityDbContext.HotelPhotos.Where(h => h.HotelId == hotelId).ToList();
            int sqlCount = hotelPhoto.Count;
            int addCount = request.AddImage.Count();
            int delCount = request.DelImage.Count();
            if (sqlCount + addCount - delCount > 5)
            {
                return BadRequest("超過數量上限");
            }

            try
            {
                //刪除檔案
                foreach (var imageId in request.DelImage)
                {
                    HotelPhoto hotelPhotodel = petCityDbContext.HotelPhotos
                        .Where(h => h.HotelId == hotelId && h.Id == imageId).FirstOrDefault();
                    // 如果圖片存在，就加入刪除清單
                    if (hotelPhotodel != null)
                    {
                        petCityDbContext.HotelPhotos.Remove(hotelPhotodel);
                    }
                }
                // 執行資料庫的存檔動作
                petCityDbContext.SaveChanges();

                int index = 1;

                //新增檔案
                foreach (var imageAdd in request.AddImage)
                {
                    //data:image/png;base64
                    //3.3前端給我base64 我要轉成byte //寫檔案  讓他變成真實檔案 存到資料夾裡面
                    //string fileName = "HotelPhoto" + Guid.NewGuid() + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + imageAdd.Extension;
                    string fileName = hotelName + "Photo" + index + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + imageAdd.Extension;

                    index++;

                    string base64 = imageAdd.Base64;
                    byte[] arr = Convert.FromBase64String(base64);
                    string filePath = HttpContext.Current.Server.MapPath(@"~/upload/profile") + "/" + fileName;
                    //創立
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        fileStream.Write(arr, 0, arr.Length);
                        fileStream.Close();
                    }

                    // 使用 SixLabors.ImageSharp 調整圖片尺寸 (正方形大頭貼)
                    var image = SixLabors.ImageSharp.Image.Load<Rgba32>(filePath);
                    image.Mutate(x => x.Resize(120, 120)); // 輸入(120, 0)會保持比例出現黑邊
                    image.Save(filePath);

                    //將檔名存進資料庫
                    HotelPhoto hotelPhotos = new HotelPhoto();
                    hotelPhotos.Photo = fileName;
                    hotelPhotos.HotelId = hotelId;
                    petCityDbContext.HotelPhotos.Add(hotelPhotos);
                    petCityDbContext.SaveChanges();
                }
                return Ok(new { Status = true, Messege = "上傳成功" });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message); // 400
            }
        }

        /// <summary>
        /// 上傳房型照片用,需要帶TOKEN
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
            string hotelName = userToken["Name"].ToString();

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

            try
            {
                // 讀取 MIME(媒體類別) 資料
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // 取得檔案副檔名，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                string fileNameData = provider.Contents.FirstOrDefault().Headers.ContentDisposition.FileName.Trim('\"');
                string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // .jpg

                // 定義檔案名稱
                string fileName = hotelName + "Room" + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;

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
        /// 寵物旅館後台管理-讀取房型列表 (後台),需要帶TOKEN
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
        ///  寵物旅館後台管理-房型列表-新增房型 (後台),需要帶TOKEN
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
        ///  寵物旅館後台管理-房型列表-讀取房型 (後台),需要帶TOKEN
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
                PetType = room.PetType,
            };

            if (room != null)
            {
                // 處理完請求內容
                return Ok(new { Status = true, result });
            }
            return BadRequest("無此房型");
        }

        /// <summary>
        ///  寵物旅館後台管理-房型列表-修改房型 (後台),需要帶TOKEN
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
        ///  寵物旅館後台管理-房型列表-刪除房型 (後台),需要帶TOKEN
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

        ///// <summary>
        ///// 分頁DEMO
        ///// </summary>
        //[HttpGet]
        //[Route("area/")]
        //public IHttpActionResult GetArea() //pageNumber
        //{
        //    PetCityNewcontext petCityDbContext = new PetCityNewcontext();

        //    //List<Area> areas = petCityDbContext.Areas.ToList();
        //    var areas = petCityDbContext.Areas.AsQueryable();
        //    var aa = areas.OrderBy(x => x.Id).ToPagedList(1, 2);  //目前頁碼 //一頁幾個
        //    var result = new
        //    {
        //        total = areas.Count() / 2, //總筆數  //分頁算式要再查 //用參數控制
        //        nowpage = 1, //目前頁數//初始值
        //        data = aa,
        //    };

        //    return Ok(result);
        //}


        /// <summary>
        /// 4-1.單一旅館資訊頁-前台旅館 (前台)，不用帶TOKEN
        /// </summary>
        [Route("hotel/hotelInfo")]
        public IHttpActionResult GetHotelInfo(int hotelId, DateTime? startDate, DateTime? endDate) //2022-12-16
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

            List<string> serviceTypeArr = new List<string>();
            //從資料庫"取資料" //轉成陣列傳給前端
            var serviceTypes = petCityDbContext.Hotels.FirstOrDefault(h => h.Id == hotelId).ServiceTypes;
            if (!string.IsNullOrWhiteSpace(serviceTypes))
            {
                serviceTypeArr = serviceTypes.Split(',').ToList();
            }

            var orders = petCityDbContext.Orders.Where(o => o.Rooms.Hotel.Id == hotelId).OrderByDescending(o => o.Score).Take(5);
            //var hotelComments = orders.Select(o => new
            //{
            //    UserName = o.PetCards.Customer.UserName,
            //    UserPhoto = o.PetCards.Customer.UserThumbnail,
            //    Score = o.Score,
            //    Comment = o.Comment,
            //});
            var hotelComments = orders.Select(o => orders.Count() == 0 ? null : new
            {
                UserName = o.PetCards.Customer.UserName,
                UserPhoto = o.PetCards.Customer.UserThumbnail,
                Score = o.Score,
                Comment = o.Comment,
            });

            var db = petCityDbContext.Hotels.Where(h => h.Id == hotelId).AsQueryable();
            var hotel = db.Select(h => new
            {
                HotelId = h.Id,
                HotelPhoto = hotelPhotoList,

                //HotelScore =
                //    Math.Round(
                //        (double)h.Rooms.Sum(r => r.Orders.Sum(o => o.Score)) / h.Rooms.Sum(c => c.Orders.Count), 1),
                //找status=checkOutComment
                //如果訂單裡面沒有這個旅店(房間)的資料  則回傳NULL?
                //如果有算總分/平均
                // HotelScore =   Math.Round((double) h.Rooms.Where(o=>o.Orders.Any()).Sum(r => r.Orders.Where(o => o.Status == "checkOutComment").Sum(o => o.Score)) / h.Rooms.Where(o=>o.Orders.Any()).Sum(c => c.Orders.Where(o => o.Status == "checkOutComment").Count()),1),
                //aa = h.Rooms.Where(o => o.Orders.Any()).Sum(r => r.Orders.Where(o => o.Status == "checkOutComment").Sum(o => o.Score)),

                HotelScore = h.Rooms.Where(o => o.Orders.Any()).Sum(r => r.Orders.Where(o => o.Status == "checkOutComment").Sum(o => o.Score)) == null ? 0 :
                 Math.Round((double)h.Rooms.Sum(r => r.Orders.Where(o => o.Status == "checkOutComment").Sum(o => o.Score)) / h.Rooms.Where(r => r.Orders.Where(o => o.Status == "checkOutComment").Count() > 0).Sum(r => r.Orders.Where(o => o.Status == "checkOutComment").Count()), 1),

                HotelName = h.HotelName,
                HotelAddress = h.HotelAddress,
                HotelPhone = h.HotelPhone,
                HotelStartTime = h.HotelStartTime,
                HotelEndTime = h.HotelEndTime,
                HotelInfo = h.HotelInfo,
                HotelService = serviceTypeArr,

                HotelComment = hotelComments,

                //Room = h.Rooms.Select(r => new
                //{
                //    Id = r.Id,
                //    RoomPhoto = "https://petcity.rocket-coding.com/upload/profile/" + r.RoomPhoto,
                //    RoomName = r.RoomName,
                //    PetType = r.PetType,
                //    RoomPrice = r.RoomPrice,
                //    RoomInfo = r.RoomInfo,
                //}),
                Room = h.Rooms.Where(r => !r.Orders.Any(o =>
                    (startDate <= o.CheckInDate && o.CheckInDate < endDate) ||
                    (startDate < o.CheckOutDate && o.CheckOutDate <= endDate))).Select(r => new
                    {
                        Id = r.Id,
                        RoomPhoto = r.RoomPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + r.RoomPhoto,
                        RoomName = r.RoomName,
                        PetType = r.PetType,
                        RoomPrice = r.RoomPrice,
                        RoomInfo = r.RoomInfo,
                    })
            });
            return Ok(new
            {
                Hotel = hotel
            });
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
        ///  使用者讀取旅館列表+分頁+篩選 (homepage) (前台)，不用帶TOKEN 
        /// </summary>
        [Route("hotel/hotelFilter")]
        public IHttpActionResult PostHotelFilter(ViewModelHotel.Filter filter)
        {
            //判斷有沒有值 再去做後續動作
            //判斷陣列長度 有沒有大於0

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            ////起手式  //使用套件下的
            //var predicateHotel = PredicateBuilder.New<Hotel>(true);
            //跟資料庫有關  只是先放著不執行

            var hotels = petCityDbContext.Hotels.AsQueryable();

            //單一物件可以用Where
            if (filter.AreaId != 0)
            {
                hotels = hotels.Where(h => h.AreaId == filter.AreaId);
            }

            if (filter.FoodTypes.Length > 0)
            { //起手式  //使用套件下的
                var predicateHotel = PredicateBuilder.New<Hotel>();
                for (int i = 0; i < filter.FoodTypes.Length; i++)
                {
                    var type = filter.FoodTypes[i];
                    predicateHotel = predicateHotel.Or(h => h.FoodTypes.Contains(type));
                    //hotels = hotels.Where(h => h.FoodTypes.Contains(type));
                }
                hotels = hotels.Where(predicateHotel);
            }

            //沒有重疊到的話可以這樣子做
            if (filter.PriceRange.Length > 0)
            {
                int RoomPriceStart;
                int RoomPriceEnd;
                int min = Int32.MaxValue;
                int max = 0;
                //起手式  //使用套件下的
                var predicateHotel = PredicateBuilder.New<Hotel>();
                for (int k = 0; k < filter.PriceRange.Length; k++)
                {
                    //0=0-499; 1=500-999 ; 2=1000-1499 ; 3=1500-1999 ; 4=2000-9999
                    var priceIndex = filter.PriceRange[k];
                    //filter.PriceRange[k]= filter.RoomPriceStart - filter.RoomPriceEnd;

                    switch (priceIndex)
                    {
                        case 0:
                            RoomPriceStart = 0;
                            RoomPriceEnd = 499;
                            break;

                        case 1:
                            RoomPriceStart = 500;
                            RoomPriceEnd = 999;
                            break;

                        case 2:
                            RoomPriceStart = 1000;
                            RoomPriceEnd = 1499;
                            break;

                        case 3:
                            RoomPriceStart = 1500;
                            RoomPriceEnd = 1999;
                            break;

                        default:
                            RoomPriceStart = 2000;
                            RoomPriceEnd = 99999;
                            break;
                    }

                    if (min > RoomPriceStart)
                    {
                        min = RoomPriceStart;
                    }
                    if (max < RoomPriceEnd)
                    {
                        max = RoomPriceEnd;
                    }
                    //錯誤hotels = hotels.Where(h => h.Rooms.Any(r => filter.RoomPriceStart <= r.RoomPrice && r.RoomPrice < filter.RoomPriceEnd));
                    //錯誤var aa = hotels.Where(h=> h.Rooms.Any(r => RoomPriceStart <= r.RoomPrice && r.RoomPrice < RoomPriceEnd)).ToList();
                    //錯誤predicateHotel = predicateHotel.Or(h => h.Rooms.Any(r => RoomPriceStart <= r.RoomPrice && r.RoomPrice < RoomPriceEnd));
                }
                hotels = hotels.Where(h => h.Rooms.Any(r => min <= r.RoomPrice && r.RoomPrice < max));
            }

            if (filter.ServiceTypes.Length > 0)
            {//起手式  //使用套件下的
                var predicateHotel = PredicateBuilder.New<Hotel>();
                for (int j = 0; j < filter.ServiceTypes.Length; j++)
                {
                    var type = filter.ServiceTypes[j];
                    predicateHotel = predicateHotel.Or(h => h.ServiceTypes.Contains(type));
                    //hotels = hotels.Where(h => h.FoodTypes.Contains(type));
                }
                hotels = hotels.Where(predicateHotel);
            }

            if (!string.IsNullOrWhiteSpace(filter.PetType))  //單一物件可以用Where
            {                    //Where只能使用一次    //任何跟他相符的
                hotels = hotels.Where(h => h.Rooms.Any(r => r.PetType == filter.PetType));
            }

            //用時間判斷 找出那些房間不行，扣掉不行的就是要的 //這段區間內可以被訂的旅館
            if (!string.IsNullOrWhiteSpace(filter.CheckInDate) && !string.IsNullOrWhiteSpace(filter.CheckOutDate))
            {
                string checkInDateString = filter.CheckInDate;
                DateTime checkinDate = DateTime.Parse(checkInDateString);
                string checkOutDateString = filter.CheckOutDate;
                DateTime checkoutDate = DateTime.Parse(checkOutDateString);

                //先使用Where方法在petCityDbContext.Hotels集合中篩選出符合條件的旅店。
                //旅店有房間，並且這些房間都有預訂單，而且每個預訂單的入住日期都大於checkinDate，以及退房日期都小於checkoutDate。
                //再排除符合條件的旅店。
                hotels = hotels.Where(h =>
                    h.Rooms.Any(r =>
                        !r.Orders.Any(o =>
                            //訂單中的入住時間 介於input的入跟退
                            (checkinDate <= o.CheckInDate && o.CheckInDate < checkoutDate) ||
                            //訂單中的退房時間 介於input的入跟退
                            (checkinDate < o.CheckOutDate && o.CheckOutDate <= checkoutDate))));
            }
            //hotels = hotels.Where(predicateHotel);
            //var aa = hotels.ToList();

            //分頁
            int total = 0;
            if (hotels.Count() % filter.PageSize == 0)
            {
                total = hotels.Count() / filter.PageSize;
            }
            else
            {
                total = hotels.Count() / filter.PageSize + 1;
            }
            var hotelSplit = hotels.OrderBy(h => h.Id).ToPagedList(filter.Page, filter.PageSize);
            //var skip = (filter.Page - 1) * filter.PageSize;
            //var hotelSplit = hotels.OrderBy(h => h.Id).Skip(skip).Take(filter.PageSize);
            //var bb = hotelSplit.FirstOrDefault(h => h.Id == 10);

            //格式
            var hotelList = new HotelViewModel
            {
                //集合列表                     //選取             //物件
                Data = hotelSplit.ToList().Select(h => new HotelData
                {
                    //RoomLowPrice = h.Rooms.OrderBy(r => r.RoomPrice).FirstOrDefault().RoomPrice,
                    RoomLowPrice = h.Rooms.OrderBy(r => r.RoomPrice).FirstOrDefault()?.RoomPrice,
                    HotelId = h.Id,
                    HotelName = h.HotelName,

                    //三元運算值
                    HotelPhoto = h.HotelPhotos.Count == 0 ? "" : "https://petcity.rocket-coding.com/upload/profile/" + h.HotelPhotos.OrderBy(p => p.Id).FirstOrDefault().Photo,

                    HotelScore =
                        Math.Round(
                            (double)h.Rooms.Sum(r => r.Orders
                                .Where(o => o.Score != null) // only sum scores that are not null
                                .Sum(o => o.Score)) / h.Rooms.Sum(c => c.Orders
                                .Where(o => o.Score != null) // only count orders with non-null scores
                                .Count()), 1),
                    ////Room是一個集合物件  所以select會以id分類丟出結果
                    ///*   Count= h.Rooms.Select(c => c.Orders.Count)*/ //除錯用

                    HotelInfo = h.HotelInfo,
                }).Where(h => h.RoomLowPrice != null && h.HotelName != null && h.HotelPhoto != null && h.HotelInfo != null).ToList(), //選取完東西後 轉換成一個列表 ToList不一定要寫

                Totalpage = total,
                Nowpage = filter.Page,
            };
            return Ok(hotelList);
        }

        /// <summary>
        ///  寵物旅館後台管理-送出訂單管理 -status=checkIn 要TOKEN
        /// </summary>
        [Route("hotel/checkIn")]
        [JwtAuthFilter_Hotel]
        public IHttpActionResult Put(int orderId, ViewModelHotel.CheckInStatus checkIn)
        {
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hoteld = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.Rooms.Hotel.Id == hoteld).Where(o => o.Id == orderId).ToList();

            orders[0].Status = checkIn.CheckIn;
            petCityDbContext.SaveChanges();

            var result = new
            {
                status = "success",
                message = "入住成功",
            };
            return Ok(result);
        }

        /// <summary>
        ///  寵物旅館後台管理-送出訂單管理 -status=checkOut 要TOKEN
        /// </summary>
        [Route("hotel/checkOut")]
        [JwtAuthFilter_Hotel]
        public IHttpActionResult Put(int orderId, ViewModelHotel.CheckOutStatus checkOut)
        {
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hoteld = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.Rooms.Hotel.Id == hoteld).Where(o => o.Id == orderId).ToList();

            orders[0].Status = checkOut.CheckOut;
            petCityDbContext.SaveChanges();

            string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
            string toEmail = orders[0].PetCards.Customer.UserAccount;
            string password = ConfigurationManager.AppSettings["EmailPassword"];

            // 網站網址
            string path = /*Request.Url.Scheme + "://" + Request.Url.Authority +*/
                Url.Content("https://petcity-booking.netlify.app/");

            // 從信件連結回到評論頁面
            string receivePage = "#/customer/comment";
            //string receivePage = "#/customer/comment?token=";


            //var customerInfo = petCityDbContext.Orders.Where(o => o.Rooms.Hotel.Id == hoteld)
            //    .Where(o => o.Id == orderId).Select(p => new
            //    {
            //        p.PetCards.Customer.Id,
            //        p.PetCards.Customer.UserAccount,
            //        p.PetCards.Customer.UserName,
            //        p.PetCards.Customer.UserThumbnail,
            //        p.PetCards.Customer.Identity
            //    }).ToList();
            //int customerId = customerInfo[0].Id;
            //string customerAccount = customerInfo[0].UserAccount;
            //string customerName = customerInfo[0].UserName;
            //string customerIdentity = customerInfo[0].Identity;
            //string customerImage = customerInfo[0].UserThumbnail;
            //JwtAuthUtil JWTtoken = new JwtAuthUtil();
            //string token = JWTtoken.GenerateToken(customerId, customerAccount, customerName, customerImage, customerIdentity);


            // 信件內容範本
            string body = "感謝您本次使用寵物坊城市，讓毛小孩交給我們照顧是我們的榮幸。現在您可以到我的評價中給予評價。<br><br>";
            //+ "<a href='" + path + receivePage +token+"'  target='_blank'>點此連結</a>";

            DateTime now = DateTime.Now;
            string Subject = $"{now.ToString("yyyy/MM/dd")}邀請評價";

            SendGmailMail(fromEmail, toEmail, Subject, body, password);

            var result = new
            {
                status = "success",
                message = "退房成功",
            };
            return Ok(result);
        }

        /// <summary>
        ///  寵物旅館後台管理-送出訂單管理 -status=cancel 要TOKEN
        /// </summary>
        [Route("hotel/cancel")]
        [JwtAuthFilter_Hotel]
        public IHttpActionResult Put(int orderId, ViewModelHotel.CancelStatus cancel)
        {
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hoteld = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.Rooms.Hotel.Id == hoteld).Where(o => o.Id == orderId).ToList();

            orders[0].Status = cancel.Cancel;
            petCityDbContext.SaveChanges();

            var result = new
            {
                status = "success",
                message = "取消成功",
            };
            return Ok(result);
        }

        /// <summary>
        ///  寵物旅館後台管理-取得訂單管理列表 -待入住 要TOKEN
        /// </summary>
        [Route("hotel/reservedList")]
        [JwtAuthFilter_Hotel]
        public IHttpActionResult GetOrderList()
        {
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hoteld = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.Rooms.Hotel.Id == hoteld).Where(o => o.Status == "reserved").OrderBy(o => o.RoomId).OrderByDescending(o => o.CheckInDate).Select(o => new
            {
                o.Id,
                o.PetCards.Customer.UserName,
                o.PetCardId,
                o.PetCards.PetName,
                PetPhoto = o.PetCards.PetPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + o.PetCards.PetPhoto,
                o.Rooms.RoomName,
                o.CheckInDate,
                o.CheckOutDate,
                o.Status,
            }).AsEnumerable().Select(a => new
            {
                a.Id,
                a.UserName,
                a.PetCardId,
                a.PetName,
                a.PetPhoto,
                a.RoomName,
                checkInDateOnly = a.CheckInDate?.ToString("yyyy-MM-dd"),
                checkOutDateOnly = a.CheckOutDate?.ToString("yyyy-MM-dd"),
                a.Status
            }).ToList();

            //var checkInDate = orders.Select(o => o.CheckInDate).AsEnumerable().Select(a => a.);
            //string checkInDateOnly = checkInDate?.ToString("yyyy-MM-dd");
            //var checkOutDate = orders.Select(o => o.CheckOutDate);
            //string checkOutDateOnly = checkOutDate?.ToString("yyyy-MM-dd");

            ////創建一個 List，用來存儲所有訂單
            //var resultList = new List<object>();

            ////遍歷所有訂單
            //foreach (var order in orders)
            //{
            //    //將每份訂單資訊存儲到 List 中
            //    resultList.Add(new
            //    {
            //        order.Id,
            //        order.PetCards.Customer.UserName,
            //        order.PetCardId,
            //        order.PetCards.PetName,
            //        PetPhoto = order.PetCards.PetPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + order.PetCards.PetPhoto,
            //        order.Rooms.RoomName,
            //        checkInDateOnly,
            //        checkOutDateOnly,
            //        order.Status,
            //    });
            //}
            //返回訂單結果
            return Ok(new { Status = true, result = orders });
        }

        /// <summary>
        ///  寵物旅館後台管理-訂單管理列表 -已入住 要TOKEN
        /// </summary>
        [Route("hotel/checkInList")]
        [JwtAuthFilter_Hotel]
        public IHttpActionResult GetCheckInList()
        {
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hoteld = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.Rooms.Hotel.Id == hoteld).Where(o => o.Status == "checkIn").OrderBy(o => o.RoomId).OrderByDescending(o => o.CheckInDate).Select(o => new
            {
                o.Id,
                o.PetCards.Customer.UserName,
                o.PetCardId,
                o.PetCards.PetName,
                PetPhoto = o.PetCards.PetPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + o.PetCards.PetPhoto,
                o.Rooms.RoomName,
                o.CheckInDate,
                o.CheckOutDate,
                o.Status,
            }).AsEnumerable().Select(a => new
            {
                a.Id,
                a.UserName,
                a.PetCardId,
                a.PetName,
                a.PetPhoto,
                a.RoomName,
                checkInDateOnly = a.CheckInDate?.ToString("yyyy-MM-dd"),
                checkOutDateOnly = a.CheckOutDate?.ToString("yyyy-MM-dd"),
                a.Status
            }).ToList();

            //var checkInDate = orders.Select(o => o.CheckInDate).FirstOrDefault();
            //string checkInDateOnly = checkInDate?.ToString("yyyy-MM-dd");
            //var checkOutDate = orders.Select(o => o.CheckOutDate).FirstOrDefault();
            //string checkOutDateOnly = checkOutDate?.ToString("yyyy-MM-dd");

            ////創建一個 List，用來存儲所有訂單
            //var resultList = new List<object>();

            ////遍歷所有訂單
            //foreach (var order in orders)
            //{
            //    //將每份訂單資訊存儲到 List 中
            //    resultList.Add(new
            //    {
            //        order.Id,
            //        order.PetCards.Customer.UserName,
            //        order.PetCardId,
            //        order.PetCards.PetName,
            //        PetPhoto = order.PetCards.PetPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + order.PetCards.PetPhoto,
            //        order.Rooms.RoomName,
            //        checkInDateOnly,
            //        checkOutDateOnly,
            //        order.Status,
            //    });
            //}
            //返回訂單結果
            return Ok(new { Status = true, result = orders });
        }

        /// <summary>
        ///  寵物旅館後台管理-訂單管理列表 -已完成 要TOKEN
        /// </summary>
        [Route("hotel/checkOutList")]
        [JwtAuthFilter_Hotel]
        public IHttpActionResult GetcheckOutList()
        {
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hoteld = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.Rooms.Hotel.Id == hoteld).Where(o => o.Status == "checkOut").OrderBy(o => o.RoomId).OrderByDescending(o => o.CheckInDate).Select(o => new
            {
                o.Id,
                o.PetCards.Customer.UserName,
                o.PetCardId,
                o.PetCards.PetName,
                PetPhoto = o.PetCards.PetPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + o.PetCards.PetPhoto,
                o.Rooms.RoomName,
                o.CheckInDate,
                o.CheckOutDate,
                o.Status,
            }).AsEnumerable().Select(a => new
            {
                a.Id,
                a.UserName,
                a.PetCardId,
                a.PetName,
                a.PetPhoto,
                a.RoomName,
                checkInDateOnly = a.CheckInDate?.ToString("yyyy-MM-dd"),
                checkOutDateOnly = a.CheckOutDate?.ToString("yyyy-MM-dd"),
                a.Status
            }).ToList();

            //var checkInDate = orders.Select(o => o.CheckInDate).FirstOrDefault();
            //string checkInDateOnly = checkInDate?.ToString("yyyy-MM-dd");
            //var checkOutDate = orders.Select(o => o.CheckOutDate).FirstOrDefault();
            //string checkOutDateOnly = checkOutDate?.ToString("yyyy-MM-dd");

            ////創建一個 List，用來存儲所有訂單
            //var resultList = new List<object>();

            ////遍歷所有訂單
            //foreach (var order in orders)
            //{
            //    //將每份訂單資訊存儲到 List 中
            //    resultList.Add(new
            //    {
            //        order.Id,
            //        order.PetCards.Customer.UserName,
            //        order.PetCardId,
            //        order.PetCards.PetName,
            //        PetPhoto = order.PetCards.PetPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + order.PetCards.PetPhoto,
            //        order.Rooms.RoomName,
            //        checkInDateOnly,
            //        checkOutDateOnly,
            //        order.Status,
            //    });
            //}
            //返回訂單結果
            return Ok(new { Status = true, result = orders });
        }

        /// <summary>
        ///  寵物旅館後台管理-訂單管理列表 -已取消 要TOKEN
        /// </summary>
        [Route("hotel/cancelList")]
        [JwtAuthFilter_Hotel]
        public IHttpActionResult GetCancelList()
        {
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hoteld = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.Rooms.Hotel.Id == hoteld).Where(o => o.Status == "cancel").OrderBy(o => o.RoomId).OrderByDescending(o => o.CheckInDate).Select(o => new
            {
                o.Id,
                o.PetCards.Customer.UserName,
                o.PetCardId,
                o.PetCards.PetName,
                PetPhoto = o.PetCards.PetPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + o.PetCards.PetPhoto,
                o.Rooms.RoomName,
                o.CheckInDate,
                o.CheckOutDate,
                o.Status,
            }).AsEnumerable().Select(a => new
            {
                a.Id,
                a.UserName,
                a.PetCardId,
                a.PetName,
                a.PetPhoto,
                a.RoomName,
                checkInDateOnly = a.CheckInDate?.ToString("yyyy-MM-dd"),
                checkOutDateOnly = a.CheckOutDate?.ToString("yyyy-MM-dd"),
                a.Status
            }).ToList();

            //var checkInDate = orders.Select(o => o.CheckInDate).FirstOrDefault();
            //string checkInDateOnly = checkInDate?.ToString("yyyy-MM-dd");
            //var checkOutDate = orders.Select(o => o.CheckOutDate).FirstOrDefault();
            //string checkOutDateOnly = checkOutDate?.ToString("yyyy-MM-dd");

            ////var checkInDate = orders.Select(o => o.CheckInDate).FirstOrDefault().ToString();
            ////string checkInDateOnly = checkInDate.Substring(0, 9);
            ////var checkOutDate = orders.Select(o => o.CheckOutDate).FirstOrDefault().ToString();
            ////string checkOutDateOnly = checkOutDate.Substring(0, 9);

            ////創建一個 List，用來存儲所有訂單
            //var resultList = new List<object>();
            ////遍歷所有訂單
            //foreach (var order in orders)
            //{
            //    //將每份訂單資訊存儲到 List 中
            //    resultList.Add(new
            //    {
            //        order.Id,
            //        order.PetCards.Customer.UserName,
            //        order.PetCardId,
            //        order.PetCards.PetName,
            //        PetPhoto = order.PetCards.PetPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + order.PetCards.PetPhoto,
            //        order.Rooms.RoomName,
            //        checkInDateOnly,
            //        checkOutDateOnly,
            //        order.Status,
            //    });
            //}
            //返回訂單結果
            return Ok(new { Status = true, result = orders });
        }

        /// <summary>
        ///  寵物旅館後台管理-查看評價列表 - 要TOKEN
        /// </summary>
        [Route("hotel/commentList")]
        [JwtAuthFilter_Hotel]
        public IHttpActionResult GetCommentList()
        {
            var userToken = JwtAuthFilter_Hotel.GetToken(Request.Headers.Authorization.Parameter);
            int hoteld = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var orders = petCityDbContext.Orders.Where(o => o.Rooms.Hotel.Id == hoteld).Where(o => o.Status == "checkOutComment").OrderBy(o => o.RoomId).OrderByDescending(o => o.CheckInDate).Select(o => new
            {
                o.Id,
                o.Rooms.RoomName,
                o.CheckInDate,
                o.CheckOutDate,
                o.PetCards.Customer.UserName,
                UserThumbnail = o.PetCards.Customer.UserThumbnail == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + o.PetCards.Customer.UserThumbnail,
                o.Comment,
                o.Score,
                o.Status,
            }).AsEnumerable().Select(a => new
            {
                a.Id,
                a.RoomName,
                checkInDateOnly = a.CheckInDate?.ToString("yyyy-MM-dd"),
                checkOutDateOnly = a.CheckOutDate?.ToString("yyyy-MM-dd"),
                a.UserName,
                a.UserThumbnail,
                a.Comment,
                a.Score,
                a.Status
            }).ToList();


            //var checkInDate = orders.Select(o => o.CheckInDate).FirstOrDefault();
            //string checkInDateOnly = checkInDate?.ToString("yyyy-MM-dd");
            //var checkOutDate = orders.Select(o => o.CheckOutDate).FirstOrDefault();
            //string checkOutDateOnly = checkOutDate?.ToString("yyyy-MM-dd");

            ////創建一個 List，用來存儲所有訂單
            //var resultList = new List<object>();
            ////遍歷所有訂單
            //foreach (var order in orders)
            //{
            //    //將每份訂單資訊存儲到 List 中
            //    resultList.Add(new
            //    {
            //        order.Id,
            //        order.Rooms.RoomName,
            //        checkInDateOnly,
            //        checkOutDateOnly,
            //        order.PetCards.Customer.UserName,
            //        UserThumbnail = order.PetCards.Customer.UserThumbnail == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + order.PetCards.Customer.UserThumbnail,
            //        order.Comment,
            //        order.Score,
            //    });
            //}
            //返回訂單結果
            return Ok(new { Status = true, result = orders });
        }


    }
}




