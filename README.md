# Identifier Interface

```csharp
public interface IEntity
    {
    }
```
# Model
```csharp
 public class Student : IEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
```
```csharp
 public class CgpaHistory : IEntity
    {
        public string StudentName { get; set; }
	    public double Cgpa {get;set;}
    }
```
Note: You Can Add Entity Framework data annotation inside model class if required.

#Add Connection String to AppSettings 

```json
 "ConnectionStrings": {
    "DevConnection": "Server=BS-483;Database=KhatiExtendedEFTest;Trusted_Connection=True;MultipleActiveResultSets=True;"
  }
```

# DBContext

```csharp
 public class PersonDBContext : DatabaseContext<IEntity>
    {
        private readonly IConfiguration _configuration;
        public PersonDBContext(DbContextOptions options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        public override string connectionString() => _configuration.GetConnectionString("DevConnection");
        public override void EntityBinder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CgpaHistory>().ToTable("CgpaHistory").HasNoKey();
        }
    }
```
# Add Middleware

```csharp
builder.Services.ExtendedEF<PersonDBContext>();
```

#DBContext with Identity

```csharp
 public class PersonDBContext : DatabaseContextIdentityUser<IEntity,IdentityUser>
    {
        private readonly IConfiguration _configuration;
        public PersonDBContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public override string connectionString() => _configuration.GetConnectionString("DevConnection");
        public override void EntityBinder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CgpaHistory>().ToTable("CgpaHistory").HasNoKey();
        }
    }
```

Note: You can use Type of IdentityUser if required.

#Add Middleware If you're using Identity Context

```csharp
builder.Services.ExtendedEF<PersonDBContext>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
          .AddEntityFrameworkStores<PersonDBContext>()
          .AddDefaultTokenProviders();
```

# Add Migration

### Install Package

Microsoft.EntityFrameworkCore.Tools

```csharp
Add-Migration -Context PersonDBContext
```
# Update Migration

```csharp
update-database -Context PersonDBContext
```

#Remove Migration

```csharp
remove-migration -Context PersonDBContext
```

# Controller

```csharp
public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepository<Student> _studentRepo;
        private readonly IUnitOfWork<IEntity> _unitOfWork;
        public HomeController(ILogger<HomeController> logger, IRepository<Student> studentRepo,
            IUnitOfWork<IEntity> unitOfWork)
        {
            _studentRepo = studentRepo; 
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new Student()
            {
                Name = "Washiq"
            };
            var result = await _unitOfWork.Commit(async () =>
            {
                var res = await _studentRepo.InsertAsync(model);
                return res;
            });

            var model1 = new Student()
            {
                Name = "A"
            };

            var model2 = new Student()
            {
                Name = "B"
            };

            var model3 = new Student()
            {
                Name = "C"
            };

            List<Student> students = new List<Student> { model1, model2, model3 };

            var result1 = await _unitOfWork.Commit(async () =>
            {
                await _studentRepo.InsertRangeAsync(students);
            });

            var get = await _studentRepo.GetListAsync();
            var paginate = await _studentRepo.GetPagination(x => true, 1, 1);
            return Ok(new
            {
                all = get,
                paginate = paginate
            });
        }
    }
```