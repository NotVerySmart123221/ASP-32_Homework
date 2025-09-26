using ASP_32.Data;
using ASP_32.Data.Entities;
using ASP_32.Models.Home;
using ASP_32.Models.Rest;
using ASP_32.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_32.Controllers.Api
{
    [Route("api/group")]
    [ApiController]
    public class GroupController(
            IStorageService storageService,
            DataAccessor dataAccessor,
            DataContext dataContext) : ControllerBase
    {
        private readonly IStorageService _storageService = storageService;
        private readonly DataContext _dataContext = dataContext;
        private readonly DataAccessor _dataAccessor = dataAccessor;

        [HttpGet]
        public RestResponse AllGroups()
        {
            return new()
            {
                // Status = RestStatus.Status400,
                Meta = new() { 
                    Manipulations = ["GET", "POST"],
                    Cache = 24 * 60 * 60,
                    Service = "Shop API: product groups",
                    DataType = "json/array"
                },
                Data = _dataAccessor.GetProductGroups()
            };
        }

        [HttpGet("{id}")]
        public RestResponse GroupBySlug(String id)
        {
            var pg = _dataAccessor.GetProductGroupBySlug(id);
            return new()
            {
                Status = pg == null ? RestStatus.Status404 : RestStatus.Status200,
                Meta = new()
                {
                    Manipulations = ["GET"],
                    Cache = 60 * 60,
                    Service = "Shop API: products of group by slug",
                    DataType = pg == null ? "null" : "json/object"
                },
                Data = pg
            };
        }

        [HttpPost]
        public object AddGroup(AdminGroupFormModel model)
        {
            // Валідація - на ДЗ
            _dataContext.ProductGroups.Add(new Data.Entities.ProductGroup
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Description = model.Description,
                Slug = model.Slug,
                ImageUrl = _storageService.Save(model.Image)
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
