using ASP_32.Data.Entities;
using ASP_32.Services.Kdf;
using Microsoft.EntityFrameworkCore;

namespace ASP_32.Data
{
    public class DataAccessor
    {
        private readonly IKdfService _kdfService;

        public DataAccessor(IKdfService kdfService)
        {
            _kdfService = kdfService;
        }

        private DataContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlServer("Server=.;Database=ASP_32;Trusted_Connection=True;TrustServerCertificate=True;")
                .Options;

            return new DataContext(options);
        }

        public UserAccess? Authenticate(string login, string password)
        {
            using var context = CreateContext();

            var userAccess = context
                .UserAccesses
                .AsNoTracking()
                .Include(ua => ua.User)
                .Include(ua => ua.Role)
                .FirstOrDefault(ua => ua.Login == login);

            if (userAccess == null)
                return null;

            string dk = _kdfService.Dk(password, userAccess.Salt);
            if (dk != userAccess.Dk)
                return null;

            return userAccess;
        }

        public IEnumerable<ProductGroup> GetProductGroups()
        {
            using var context = CreateContext();

            return context.ProductGroups
                .Where(g => g.DeletedAt == null)
                .AsEnumerable();
        }
    }

}
