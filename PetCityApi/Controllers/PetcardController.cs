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
        /// 使用者送出訂單請求 新增寵物名片 取得寵物名片ID
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
        /// 上傳寵物照片用
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
        /// 編輯寵物名片
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
        ///  讀取寵物名片
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
            var petCardList = petCityDbContext.PetCards.Where(p => p.CustomerId == customerId && p.Id == petCardId).ToList()
                .FirstOrDefault();
            if (petCardList == null)
            {
                return BadRequest("無此寵物名片");
            }

            //從資料庫"取資料" //轉成陣列傳給前端
            var foodTypes = petCityDbContext.PetCards.FirstOrDefault(p => p.Id == 2).FoodTypes;
            List<string> foodTypeArr = foodTypes?.Split(',').ToList();
            var serviceTypes = petCityDbContext.PetCards.FirstOrDefault(p => p.Id == 2).ServiceTypes;
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
                PetSex = petCardList.PetSex,
                FoodTypes = foodTypeArr,
                PetPersonality = petCardList.PetPersonality,
                PetMedicine = petCardList.PetMedicine,
                PetNote = petCardList.PetNote,
                ServiceTypes = serviceTypeArr,
            };

            if (petCardList != null)
            {
                // 處理完請求內容
                return Ok(new { Status = true, result });
            }
            return BadRequest("無此寵物名片");
        }

        /// <summary>
        ///  刪除寵物名片
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

            if (petCardList != null)
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
            return BadRequest("無此寵物名片");
        }


        /// <summary>
        /// 讀取寵物名片列表 
        /// </summary>
        [HttpGet]
        [Route("petcard/petcardlist")]
        //[JwtAuthFilter] //[JwtAuthFilter] 標籤，可放於需登入的 API 上，用來檢核 JWT-Token 是否正確
        public IHttpActionResult GetPetCardList()
        {
            // 取出請求內容，解密 JwtToken 取出資料
            //var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            //int customerId = (int)userToken["Id"];

            PetCityNewcontext petCityDbContext = new PetCityNewcontext();

            //從資料庫"取資料" //轉成陣列傳給前端
            var foodTypes = petCityDbContext.PetCards.FirstOrDefault(p => p.CustomerId == 24).FoodTypes;
            string[] foodTypeArr = foodTypes?.Split(',');
            var serviceTypes = petCityDbContext.PetCards.FirstOrDefault(p => p.CustomerId == 24).FoodTypes;
            string[] serviceTypeArr = serviceTypes?.Split(',');


            var petPhoto = petCityDbContext.PetCards.Where(p => p.CustomerId == 24).ToList();
            if (petPhoto.Count == 0)
            {
                return BadRequest("無此帳號");
            }

            var petCardList = petCityDbContext.PetCards.Where(p => p.CustomerId == 24).Select(p => new
            {
                PetCardId=p.Id,
                PetPhoto = p.PetPhoto==null?"": "https://petcity.rocket-coding.com/upload/profile/" + p.PetPhoto,  //三元運算值
                p.PetName,
                PetTrpe=p.FoodTypes,
                p.PetAge,
                p.PetSex,
                FoodTypes= foodTypeArr.ToList(),
                p.PetPersonality,
                p.PetMedicine,
                p.PetNote,
                ServiceType= serviceTypeArr.ToList(),
            });
            return Ok(new { Status = true, petCardList });
        }





    }



}
