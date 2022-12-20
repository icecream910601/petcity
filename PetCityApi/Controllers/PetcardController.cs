using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using NSwag.Annotations;
using PetCityApi1.JWT;
using PetCityApi1.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PetCityApi1.Controllers
{
    [OpenApiTag("PetCards", Description = "寵物名片CURD")]
    public class PetcardController : ApiController
    {
        /// <summary>
        /// 使用者送出訂單請求 新增寵物名片 取得寵物名片ID,需要帶TOKEN
        ///</summary>
        [Route("petcard/")]
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult Post(ViewModelPet postPet)
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var petCardList = petCityDbContext.PetCards.Where(p => p.CustomerId == customerId).Select(p => p.Id).ToList();

            PetCard petCard = new PetCard();
            petCard.CustomerId = customerId;
            petCard.PetName = postPet.PetName;
            petCard.PetType = postPet.PetType;
            petCard.PetAge = postPet.PetAge;
            petCard.PetSex = postPet.PetSex;

            string resultFoodType = "";
            foreach (var type in postPet.FoodTypes)
            {
                resultFoodType += type + ",";
            }
            resultFoodType = resultFoodType.TrimEnd(',');
            petCard.FoodTypes = resultFoodType;

            petCard.PetPersonality = postPet.PetPersonality;
            petCard.PetMedicine = postPet.PetMedicine;
            petCard.PetNote = postPet.PetNote;

            string resultServiceType = "";
            foreach (var type in postPet.ServiceTypes)
            {
                resultServiceType += type + ",";
            }
            resultServiceType = resultServiceType.TrimEnd(',');
            petCard.ServiceTypes = resultServiceType;

            petCityDbContext.PetCards.Add(petCard);
            petCityDbContext.SaveChanges();

            var result = new
            {
                status = "success",
                message = "新增寵物名片成功",
                petid = petCard.Id,
            };
            return Ok(new { Status = true, result });
        }

        /// <summary>
        /// 使用者上傳寵物照片用,需要帶TOKEN
        /// </summary>
        [Route("petcard/uploadpetphoto/")]
        [HttpPost]
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public async Task<IHttpActionResult> UploadPetPhoto([FromUri] int petCardId) //Async Task 非同步
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

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
                string fileName = "Pet" + "Photo" + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;

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
                var petphoto = petCityDbContext.PetCards.Where(p => p.CustomerId == customerId).Where(p => p.Id == petCardId).ToList();
                petphoto[0].PetPhoto = fileName;

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
        /// 使用者編輯寵物名片,需要帶TOKEN
        ///</summary>
        [Route("petcard/")]
        [JwtAuthFilter] // [JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult Put(int petCardId, ViewModelPet postPet)
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            PetCard petCardList = petCityDbContext.PetCards.Where(p => p.CustomerId == customerId).Where(p => p.Id == petCardId).FirstOrDefault();

            if (petCardList != null)
            {
                petCardList.CustomerId = customerId;
                petCardList.PetName = postPet.PetName;
                petCardList.PetType = postPet.PetType;
                petCardList.PetAge = postPet.PetAge;
                petCardList.PetSex = postPet.PetSex;

                string resultFoodType = "";
                foreach (var type in postPet.FoodTypes)
                {
                    resultFoodType += type + ",";
                }
                resultFoodType = resultFoodType.TrimEnd(',');
                petCardList.FoodTypes = resultFoodType;

                petCardList.PetPersonality = postPet.PetPersonality;
                petCardList.PetMedicine = postPet.PetMedicine;
                petCardList.PetNote = postPet.PetNote;

                string resultServiceType = "";
                foreach (var type in postPet.ServiceTypes)
                {
                    resultServiceType += type + ",";
                }
                resultServiceType = resultServiceType.TrimEnd(',');
                petCardList.ServiceTypes = resultServiceType;

                petCityDbContext.SaveChanges();

                var result = new
                {
                    status = "success",
                    message = "修改寵物名片成功",
                };
                return Ok(result); //問前端配合200去處理  //還是status code
            }
            return BadRequest("無此寵物名片");
        }

        /// <summary>
        ///  使用者讀取寵物名片,需要帶TOKEN
        /// </summary>
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        [Route("petcard/")]
        public IHttpActionResult Get(int petCardId)
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去get資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            // Do Something ~
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var petCardList = petCityDbContext.PetCards.Where(p => p.CustomerId == customerId && p.Id == petCardId).FirstOrDefault();
            if (petCardList == null)
            {
                return BadRequest("無此寵物名片");
            }

            //從資料庫"取資料" //轉成陣列傳給前端
            string foodTypes = petCityDbContext.PetCards.FirstOrDefault(p => p.Id == petCardId)?.FoodTypes;
            List<string> foodTypeArr = foodTypes?.Split(',').ToList();
            string serviceTypes = petCityDbContext.PetCards.FirstOrDefault(p => p.Id == petCardId)?.ServiceTypes;
            List<string> serviceTypeArr = serviceTypes?.Split(',').ToList();

            string url = null;
            if (petCardList.PetPhoto != null)
            {
                url = "https://petcity.rocket-coding.com/upload/profile/" + petCardList.PetPhoto;
            }

            var result = new //這樣會用兩份記憶體
            {
                Id = petCardList.Id,
                PetPhoto = url,
                PetName = petCardList.PetName,
                PetType = petCardList.PetType,
                PetAge = petCardList.PetAge,
                PetSex = petCardList.PetSex,
                FoodTypes = foodTypeArr,
                PetPersonality = petCardList.PetPersonality,
                PetMedicine = petCardList.PetMedicine,
                PetNote = petCardList.PetNote,
                ServiceTypes = serviceTypeArr,
            };
            // 處理完請求內容
            return Ok(new { Status = true, result });
        }

        /// <summary>
        ///  使用者刪除寵物名片,需要帶TOKEN
        /// </summary>
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        [Route("petcard/")]
        public IHttpActionResult Delete(int petCardId)
        {
            //解密token 取出裡面的例如id   然後再判斷有沒有這個id 有這個id  再去put資料
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            // Do Something ~
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            PetCard petCardList = petCityDbContext.PetCards.Where(p => p.CustomerId == customerId).Where(p => p.Id == petCardId)
                .FirstOrDefault();

            var petCardOrderList = petCityDbContext.Orders.Where(o => o.PetCardId == petCardId).Select(o => o.PetCardId).ToList();


            if (petCardList != null && petCardOrderList.Count == 0)
            {
                petCityDbContext.PetCards.Remove(petCardList);
                petCityDbContext.SaveChanges();

                var result = new
                {
                    status = "success",
                    message = "刪除寵物名片成功",
                };
                return Ok(result); //問前端配合200去處理  //還是status code
            }
            else
            {
                if (petCardList == null)
                {
                    return BadRequest("無此寵物名片");
                }
                else
                {
                    return Ok(new { status = "此名片有存在歷史訂單", });
                }
            }

        }


        /// <summary>
        /// 使用者讀取寵物名片列表 ,需要帶TOKEN
        /// </summary>
        [HttpGet]
        [Route("petcard/petcardlist")]
        [JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult GetPetCardList()
        {
            // 取出請求內容，解密 JwtToken 取出資料
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            //var petCard = petCityDbContext.PetCards.Where(p => p.CustomerId == customerId).ToList();
            //if (petCard.Count == 0)
            //{
            //    return Ok("此帳號無卡片");
            //}

            //從資料庫"取資料" //轉成陣列傳給前端
            //三元運算子 ?? 來檢查 serviceTypes 是否為 null。如果是，則會將空字串賦值給 serviceTypeArr，否則會將 serviceTypes 的值賦值給 serviceTypeArr。
            ////錯誤var foodTypes = petCityDbContext.PetCards.FirstOrDefault(p => p.CustomerId == customerId).FoodTypes;
            //var foodTypes = petCityDbContext.PetCards.Where(p => p.CustomerId == customerId).Select(p => p.FoodTypes).ToList();
            ////string[] foodTypeArr = foodTypes?.Split(',');
            //string[] foodTypeArr = (foodTypes ?? "").Split(',');
            
            ////錯誤var serviceTypes = petCityDbContext.PetCards.FirstOrDefault(p => p.CustomerId == customerId).ServiceTypes;
            //var serviceTypes = petCityDbContext.PetCards.Where(p => p.CustomerId == customerId).Select(p => p.ServiceTypes).ToList();
            ////string[] serviceTypeArr = serviceTypes?.Split(',');
            //string[] serviceTypeArr = (serviceTypes ?? "").Split(',');
            

            var order = petCityDbContext.Orders.AsQueryable();
            var petCardList = petCityDbContext.PetCards.Where(p => p.CustomerId == customerId).Select(p => new
            {
                PetCardId = p.Id,
                //三元運算值
                PetPhoto = p.PetPhoto == null ? "" : "https://petcity.rocket-coding.com/upload/profile/" + p.PetPhoto,
                p.PetName,
                p.PetType,
                p.PetAge,
                p.PetSex,
                p.FoodTypes,
                p.PetPersonality,
                p.PetMedicine,
                p.PetNote,
                p.ServiceTypes,
                IsOrders = order.Where(o => o.PetCardId == p.Id).Count() > 0 ? "有訂單" : "沒訂單"
                //null 是判斷這個資料撈出來是不是空的 
                //count 是有沒有資料
                //把撈出來的資料作成集合的東西
            }).AsEnumerable().Select(a => new
            {
                a.PetCardId,
                a.PetPhoto,
                a.PetName,
                a.PetType,
                a.PetAge,
                a.PetSex,
                FoodTypes =  a.FoodTypes?.ToString().Split(',') ,
                a.PetPersonality,
                a.PetMedicine,
                a.PetNote,
                ServiceTypes = a.ServiceTypes?.ToString().Split(','),
                a.IsOrders
            }).ToList();
            return Ok(new { Status = true, petCardList });
        }

        /// <summary>
        ///  讀取寵物名片,不需TOKEN
        /// </summary>
        [Route("petcard/order")]
        public IHttpActionResult GetPetCard(int petCardId)
        {
            // Do Something ~
            PetCityNewcontext petCityDbContext = new PetCityNewcontext();
            var petCardList = petCityDbContext.PetCards.Where(p => p.Id == petCardId).FirstOrDefault();
            if (petCardList == null)
            {
                return BadRequest("無此寵物名片");
            }

            //從資料庫"取資料" //轉成陣列傳給前端
            string foodTypes = petCityDbContext.PetCards.FirstOrDefault(p => p.Id == petCardId)?.FoodTypes;
            List<string> foodTypeArr = foodTypes?.Split(',').ToList();
            string serviceTypes = petCityDbContext.PetCards.FirstOrDefault(p => p.Id == petCardId)?.ServiceTypes;
            List<string> serviceTypeArr = serviceTypes?.Split(',').ToList();

            string url = null;
            if (petCardList.PetPhoto != null)
            {
                url = "https://petcity.rocket-coding.com/upload/profile/" + petCardList.PetPhoto;
            }

            var result = new //這樣會用兩份記憶體
            {
                Id = petCardList.Id,
                PetPhoto = url,
                PetName = petCardList.PetName,
                PetType = petCardList.PetType,
                PetAge = petCardList.PetAge,
                PetSex = petCardList.PetSex,
                FoodTypes = foodTypeArr,
                PetPersonality = petCardList.PetPersonality,
                PetMedicine = petCardList.PetMedicine,
                PetNote = petCardList.PetNote,
                ServiceTypes = serviceTypeArr,
            };
            // 處理完請求內容
            return Ok(new { Status = true, result });
        }




    }



}
