using ASP_32.Data;
using ASP_32.Data.Entities;
using ASP_32.Models.Rest;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASP_32.Controllers.Api
{
    [Route("api/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly DataAccessor _dataAccessor;

        public CartController(DataAccessor dataAccessor)
        {
            _dataAccessor = dataAccessor;
        }

        [HttpPut("{id}")]
        public RestResponse UpdateCartItem(string id, [FromBody] int cnt)
        {
            var response = new RestResponse
            {
                Meta = new()
                {
                    Service = $"Shop API 'User cart'. Update Cart Item {id} cnt={cnt}",
                    ServerTime = DateTime.Now.Ticks
                }
            };

            ExecuteAuthorized(userId =>
            {
                var result = _dataAccessor.UpdateCartItem(userId, id, cnt);
                response.Status = RestStatus.Status200;
                response.Data = result;
            }, response);

            return response;
        }

        [HttpPost("{id}")]
        public RestResponse AddProduct(string id)
        {
            var restResponse = new RestResponse
            {
                Meta = new() { Service = "Shop API 'User cart'. Add product to cart", ServerTime = DateTime.Now.Ticks }
            };

            ExecuteAuthorized(userId =>
            {
                _dataAccessor.AddToCart(userId, id);
                var activeCart = _dataAccessor.GetActiveCart(userId);
                string imagePath = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/Storage/Item/";

                var cartDto = activeCart with
                {
                    CartItems = activeCart.CartItems
                        .Select(ci => ci with
                        {
                            Product = ci.Product with
                            {
                                ImageUrl = imagePath + (ci.Product.ImageUrl ?? "no-image.jpg")
                            }
                        }).ToList()
                };

                restResponse.Status = RestStatus.Status200;
                restResponse.Data = cartDto;
            }, restResponse);

            return restResponse;
        }

        [HttpGet]
        public RestResponse GetCart()
        {
            var restResponse = new RestResponse
            {
                Meta = new() { Service = "Shop API 'User cart'. Get cart", ServerTime = DateTime.Now.Ticks }
            };

            ExecuteAuthorized(userId =>
            {
                var activeCart = _dataAccessor.GetActiveCart(userId);
                string imagePath = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/Storage/Item/";

                var cartDto = activeCart with
                {
                    CartItems = activeCart.CartItems
                        .Select(ci => ci with
                        {
                            Product = ci.Product with
                            {
                                ImageUrl = imagePath + (ci.Product.ImageUrl ?? "no-image.jpg")
                            }
                        }).ToList()
                };

                restResponse.Status = RestStatus.Status200;
                restResponse.Data = cartDto;
            }, restResponse);

            return restResponse;
        }

        [HttpDelete("{id}")]
        public RestResponse DeleteCartItem(string id)
        {
            var response = new RestResponse
            {
                Meta = new() { Service = "Shop API 'User cart'. Delete Cart Item", ServerTime = DateTime.Now.Ticks }
            };

            ExecuteAuthorized(userId =>
            {
                response.Data = _dataAccessor.DeleteCartItem(userId, id);
                response.Status = RestStatus.Status200;
            }, response);

            return response;
        }

        private void ExecuteAuthorized(Action<string> action, RestResponse response)
        {
            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                try
                {
                    string userId = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.PrimarySid).Value;
                    action(userId);
                }
                catch (ArgumentNullException ex)
                {
                    response.Status = RestStatus.Status400;
                    response.Data = ex.Message;
                }
                catch (InvalidOperationException)
                {
                    response.Status = RestStatus.Status401;
                    response.Data = "Error user identification. Check JWT";
                }
                catch (FormatException ex)
                {
                    response.Status = RestStatus.Status400;
                    response.Data = ex.Message;
                }
                catch (Exception ex)
                {
                    response.Status = RestStatus.Status500;
                    response.Data = ex.Message;
                }
            }
            else
            {
                response.Status = RestStatus.Status401;
                response.Data = "User not authenticated";
            }
        }
    }
}
