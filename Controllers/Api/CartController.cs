using ASP_32.Data;
using ASP_32.Models.Rest;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASP_32.Controllers.Api
{
    [Route("api/cart")]
    [ApiController]
    public class CartController : Controller
    {
        private readonly DataAccessor _dataAccessor;

        public CartController(DataAccessor dataAccessor)
        {
            _dataAccessor = dataAccessor;
        }


        [HttpPost("{id}")]
        public RestResponse AddProduct(string id) // Product id
        {
            RestResponse restResponse = new()
            {
                Meta = new()
                {
                    Service = "Shop API 'User cart'. Add product to cart",
                    ServerTime = DateTime.Now.Ticks,
                }
            };

            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                try
                {
                    string userId = HttpContext.User.Claims
    .First(c => c.Type == ClaimTypes.PrimarySid).Value;

                    _dataAccessor.AddToCart(userId, id);
                }
                catch (Exception ex) when (ex is ArgumentNullException)
                {
                    restResponse.Status = RestStatus.Status400;
                restResponse.Data = ex.Message;
                }
                catch (Exception ex) when (ex is InvalidOperationException)
                {
                restResponse.Status = RestStatus.Status401;
                restResponse.Data = ex.Message;
                }
            }
            else
            {
                restResponse.Status = RestStatus.Status401;
            }

            return restResponse;
        }
    }
}
