using ASP_32.Data;
using ASP_32.Data.Entities;
using ASP_32.Models.Api;
using ASP_32.Models.Rest;
using ASP_32.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASP_32.Controllers.Api
{
    [Route("api/product")]
    [ApiController]
    public class ProductController(
            IStorageService storageService,
            DataAccessor dataAccessor,
            DataContext dataContext) : ControllerBase
    {
        private readonly IStorageService _storageService = storageService;
        private readonly DataContext _dataContext = dataContext;
        private readonly DataAccessor _dataAccessor = dataAccessor;

        [HttpPost("feedback/{id}")]
        public RestResponse AddFeedback(String id, int? rate, String? comment)
        {
            RestResponse restResponse = new(){
                Meta = new()
                {
                    Manipulations = ["POST"],
                    Cache = 0,
                    Service = "Shop API: product feedback",
                    DataType = "null",
                    Opt = {
                         { "id", id },
                         { "rate", rate ?? -1 },
                         { "comment", comment ?? "" },
                    },
                },
                Data = null
            };
            Guid? userId = null;
            // Визначаємо, чи є авторизований користувач
            if(HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                userId = Guid.Parse(HttpContext.User.Claims
                    .First(c => c.Type == ClaimTypes.PrimarySid)
                    .Value);
            }
            // Перевіряємо чи є товар з переданим id (id OR slug)
            var product = _dataAccessor.GetProductBySlug(id);
            if (product == null)
            {
                restResponse.Status = RestStatus.Status404;
                return restResponse;
            }
            _dataContext.Feedbacks.Add(new()
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                UserId = userId,
                Comment = comment,
                Rate = rate,
                CreatedAt = DateTime.Now,
            });
            _dataContext.SaveChanges();
            return restResponse;
        }
        /* Д.З. Переробити ProductController::AddFeedback на роботу з 
         * DataAccessor замість DataContext (створивши відповідні методи)
         */

        [HttpPost]
        public object AddProduct(ApiProductFormModel model)
        {
            #region Валідація моделі
            Guid groupGuid;
            try { groupGuid = Guid.Parse(model.GroupId); }
            catch { return new { status = "Invalid GroupId", code = 400 }; }
            #endregion

            _dataContext
                .Products
                .Add(new()
                {
                    Id = Guid.NewGuid(),
                    GroupId = groupGuid,
                    Name = model.Name,
                    Description = model.Description,
                    Slug = model.Slug,
                    Price = model.Price,
                    Stock = model.Stock,
                    ImageUrl = model.Image == null ? null :
                        _storageService.Save(model.Image),
                });

            try
            {
                _dataContext.SaveChanges();
                return new { status = "OK", code = 200 };
            }
            catch (Exception ex)
            {
                return new { status = ex.Message, code = 500 };
            }
        }
    }
}
/* Д.З. Реалізувати валідацію моделі Product (ApiProductFormModel)
 * - засобами фронтенду (JS перед надсиланням)
 * - засобами ASP при прийомі даних
 */
