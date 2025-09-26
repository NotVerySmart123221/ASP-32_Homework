using ASP_32.Data;
using ASP_32.Data.Entities;
using ASP_32.Models.Rest;
using ASP_32.Services.Auth;
using ASP_32.Services.Kdf;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;

namespace ASP_32.Controllers.Api
{
    [Route("api/user")]
    [ApiController]
    public class UserController(
            DataContext dataContext,
            DataAccessor dataAccessor,
            IKdfService kdfService,
            IConfiguration configuration,
            IAuthService authService) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly IKdfService _kdfService = kdfService;
        private readonly IAuthService _authService = authService;
        private readonly IConfiguration _configuration = configuration;

        [HttpGet("jwt")]
        public RestResponse AuthenticateJwt()
        {
            RestResponse response = new();
            UserAccess userAccess;
            try
            {
                var (login, password) = GetBasicCredentials();
                userAccess = _dataAccessor.Authenticate(login, password)
                    ?? throw new Exception("Credentials rejected");
            }
            catch (Exception ex)
            {
                response.Status = RestStatus.Status401;
                response.Data = ex.Message;
                return response;
            }
            // Трансформуємо userAccess до JWT
            var headerObject = new
            {
                alg = "HS256",
                typ = "JWT"
            };
            String headerJson = JsonSerializer.Serialize(headerObject);
            String header64 = Base64UrlTextEncoder.Encode(
                System.Text.Encoding.UTF8.GetBytes(headerJson));

            var payloadObject = new
            {
                // стандартні поля
                iss = "ASP-32",	                    // Issuer	Identifies principal that issued the JWT.
                sub = userAccess.UserId,	        // Subject	Identifies the subject of the JWT.
                aud = userAccess.RoleId,	        // Audience	Identifies the recipients that the JWT is intended for. Each principal intended to process the JWT must identify itself with a value in the audience claim. If the principal processing the claim does not identify itself with a value in the aud claim when this claim is present, then the JWT must be rejected.
                exp = DateTime.Now.AddMinutes(10),	// Expiration Time	Identifies the expiration time on and after which the JWT must not be accepted for processing. The value must be a NumericDate:[9] either an integer or decimal, representing seconds past 1970-01-01 00:00:00Z.
                nbf = DateTime.Now,	                // Not Before	Identifies the time on which the JWT will start to be accepted for processing. The value must be a NumericDate.
                iat = DateTime.Now,	                // Issued at	Identifies the time at which the JWT was issued. The value must be a NumericDate.
                jti = Guid.NewGuid(),	            // JWT ID	Case-sensitive unique identifier of the token even among different issuers.iss	Issuer	Identifies principal that issued the JWT.
                // додаткові поля поза стандартом
                name  = userAccess.User.Name,
                email = userAccess.User.Email,
            };
            String payloadJson = JsonSerializer.Serialize(payloadObject);
            String payload64 = Base64UrlTextEncoder.Encode(
                System.Text.Encoding.UTF8.GetBytes(payloadJson));

            String secret = _configuration.GetSection("Jwt").GetSection("Secret").Value
                ?? throw new KeyNotFoundException("Not found configuration 'Jwt.Secret'");
            
            String tokenBody = header64 + '.' + payload64;

            String signature = Base64UrlTextEncoder.Encode(
                System.Security.Cryptography.HMACSHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(secret),
                    System.Text.Encoding.UTF8.GetBytes(tokenBody)
            ));

            response.Data = tokenBody + '.' + signature;

            return response;
        }

        private (String, String) GetBasicCredentials()
        {
            String? header = HttpContext.Request.Headers.Authorization;
            if (header == null)      // Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            {
                throw new Exception("Authorization Header Required");
            }
            String credentials =    // 'Basic ' - length = 6
                header[6..];        // QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            String userPass =       // Aladdin:open sesame
                System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(credentials));

            String[] parts = userPass.Split(':', 2);
            String login = parts[0];
            String password = parts[1];
            return (login, password);
        }

        /* Д.З. Переробити методи контролера UserController на роботу з DataAccessor
         * (за потреби створивши відповідні методи у ньому)
         * Наприкінці роботи прибрати інжекцію DataContext
         */
        [HttpGet]
        public object Authenticate()
        {
            String? header = HttpContext.Request.Headers.Authorization;
            if (header == null)      // Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            {
                HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return new { Status = "Authorization Header Required" };
            }
            /* Д.З. Реалізувати повний цикл перевірок даних, що передаються
             * для автентифікації
             * - заголовок починається з 'Basic '
             * - credentials успішно декодуються з Base64
             * - userPass ділиться на дві частини (може не містити ":")
             */
            String credentials =    // 'Basic ' - length = 6
                header[6..];        // QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            String userPass =       // Aladdin:open sesame
                System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(credentials));

            String[] parts = userPass.Split(':', 2);
            String login = parts[0];
            String password = parts[1];

            var userAccess = _dataContext
                .UserAccesses
                .AsNoTracking()
                .Include(ua => ua.User)
                .Include(ua => ua.Role)
                .FirstOrDefault(ua => ua.Login == login);

            if (userAccess == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return new { Status = "Credentials rejected" };
            }
            String dk = _kdfService.Dk(password, userAccess.Salt);
            if(dk != userAccess.Dk)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return new { Status = "Credentials rejected." };
            }
            // зберігаємо у сесії факт успішної автентифікації
            // HttpContext.Session.SetString(
            //     "UserController::Authenticate",
            //     JsonSerializer.Serialize(userAccess)
            // );
            _authService.SetAuth(userAccess);
            return userAccess;
        }

        [HttpPost]
        public object SignUp()
        {
            return new { Status = "SignUp Works" };
        }

        [HttpPost("admin")]  // POST /api/user/admin
        public object SignUpAdmin()
        {
            return new { Status = "SignUpAdmin Works" };
        }
    }
}
/* Відмінності АРІ та MVC контролерів
 * MVC:
 *  адресація за назвою дії (Action) - різні дії -- різні адреси
 *  GET  /Home/Index     --> HomeController.Index()
 *  POST /Home/Index     --> HomeController.Index()
 *  GET  /Home/Privacy   --> HomeController.Privacy()
 *  повернення - IActionResult частіше за все View
 *  
 * API:
 *  адресація за анотацією [Route("api/user")], різниця
 *  у методах запиту
 *  GET  api/user  ---> [HttpGet] Authenticate()
 *  POST api/user  ---> [HttpPost] SignUp()
 *  PUT  api/user  ---> 
 *  
 *  C   POST
 *  R   GET
 *  U   PUT(replace) PATCH(partially update)
 *  D   DELETE
 */
/* Авторизація. Схеми.
 * 0) Кукі (Cookie) - заголовки НТТР-пакету, які зберігаються у клієнта
 *      та автоматично включаються ним до всіх наступних запитів до сервера
 *      "+" простота використання
 *      "-" автоматизовано тільки у браузерах, в інших програмах це справа
 *           програміста. 
 *      "-" відкритість, легкість перехоплення даних
 *      
 * 1) Сесії (серверні): базуються на Кукі, проте всі дані зберігаються
 *     на сервері, у куках передається тільки ідентифікатор сесії
 *     "+" покращена безпека
 *     "-" велике навантаження на сховище сервера
 *     
 * 2) Токени (клієнтські): клієнт зберігає токен, який лише перевіряється
 *     сервером.
 *     "+" відмова від кукі та сесій
 *     "-" більше навантаження на роботу сервера
 *  2а) Токени-ідентифікатори
 *  2б) Токени з даними (JWT)
 */