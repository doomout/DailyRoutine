# DailyRoutine 학습 노트

> DailyRoutine 프로젝트를 개발하면서 학습한 C# / ASP.NET Core / EF Core 핵심 개념을 정리한다.
>
> 날짜별 작업 이력은 Git History로 관리하고, 이 문서에는 **다시 복습할 가치가 있는 개념**만 기록한다.
>
> 프로젝트의 현재 구현 상태와 다음 작업은 `PROJECT_STATUS.md`를 참고한다.

---

# 1. 프로젝트 개발 구조

DailyRoutine은 ASP.NET Core MVC 기반으로 개발한다.

현재 기본 구조는 다음과 같다.

```text
Browser
   ↓
Controller
   ↓
Service
   ↓
ApplicationDbContext
   ↓
EF Core
   ↓
MySQL
```

각 계층의 기본 역할:

| 계층 | 역할 |
|---|---|
| Controller | HTTP 요청을 받고 응답을 결정 |
| Service | 애플리케이션 로직 및 데이터 작업 담당 |
| DbContext | EF Core를 통한 DB 작업 관리 |
| Entity | DB 데이터와 대응되는 C# 객체 |
| View | 사용자에게 보여줄 HTML 생성 |

현재는 EF Core 자체를 익히기 위해 별도의 Repository 계층을 만들지 않는다.

---

# 2. 현재 개발 환경

- ASP.NET Core MVC
- .NET 10
- EF Core 10.0.12
- Oracle `MySql.EntityFrameworkCore` 10.0.9
- MySQL 8.4.11
- Docker Desktop
- Visual Studio 2026
- Git / GitHub

DailyRoutine 전용 MySQL은 Docker Container로 실행한다.

```text
DailyRoutine
    ↓
EF Core
    ↓
MySql.EntityFrameworkCore
    ↓
MySQL 8.4
```

실제 Connection String과 비밀번호는 소스 코드나 Git에 저장하지 않는다.

Connection String은 ASP.NET Core **User Secrets**의 다음 키에 저장한다.

```text
ConnectionStrings:DefaultConnection
```

---

# 3. Entity

현재 핵심 Entity는 `Routine`이다.

```csharp
public class Routine
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public required DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}
```

## 주요 개념

### Id

EF Core Convention에 의해 `Id`는 기본적으로 Primary Key로 인식된다.

현재 MySQL에서는 다음과 같이 생성된다.

```text
PRIMARY KEY
AUTO_INCREMENT
```

### required

```csharp
public required string Name { get; set; }
```

객체를 생성할 때 해당 속성을 초기화하도록 요구하는 C# 기능이다.

`required`가 DB의 입력값 검증까지 담당하는 것은 아니다.

예를 들어 빈 문자열이나 공백 입력을 막으려면 별도의 Validation이 필요하다.

### nullable

```csharp
string?
DateOnly?
DateTime?
```

`?`가 붙은 형식은 `null`을 허용한다.

예:

```csharp
public DateOnly? EndDate { get; set; }
```

루틴의 종료일이 정해지지 않았다면 `null`을 사용할 수 있다.

### DateOnly

날짜만 필요한 값에는 `DateTime` 대신 `DateOnly`를 사용한다.

```text
StartDate → DateOnly
EndDate   → DateOnly?
```

DB에서는 MySQL `date` 타입으로 매핑된다.

### UTC 시간

```csharp
DateTime.UtcNow
```

생성 시각 등 시스템 Timestamp는 UTC를 기준으로 저장한다.

현재 `CreatedAtUtc = DateTime.UtcNow`는 C# 객체 생성 시 값이 설정되는 것이며 MySQL의 DEFAULT 값이 아니다.

---

# 4. ApplicationDbContext

`ApplicationDbContext`는 EF Core를 통해 DB 작업을 관리하는 클래스다.

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Routine> Routines => Set<Routine>();
}
```

주요 역할:

- Entity 모델 관리
- DB 조회
- Entity 상태 관리
- 변경 추적(Change Tracking)
- INSERT / UPDATE / DELETE 저장

## DbSet

```csharp
public DbSet<Routine> Routines => Set<Routine>();
```

`Routine` Entity에 대한 DB 작업의 진입점이다.

예:

```csharp
_context.Routines
```

를 통해 Routine 데이터를 조회하거나 추가할 수 있다.

`DbSet`을 선언했다고 즉시 DB를 조회하는 것은 아니다.

---

# 5. EF Core와 JPA 비교

EF Core를 처음 이해할 때 기존 Java/Spring 경험과 다음처럼 비교할 수 있다.

완전히 동일한 기술은 아니며 학습을 위한 개념적 비교다.

| Spring / Java | ASP.NET Core / .NET |
|---|---|
| JPA / Hibernate | Entity Framework Core |
| EntityManager | DbContext |
| Persistence Context | Change Tracking |
| Entity | Entity |
| JPQL / Criteria | LINQ |
| Spring DI Container | ASP.NET Core DI Container |
| JDBC Driver | MySql.Data |
| application.yml / properties | appsettings.json 등 |

`ApplicationDbContext`는 학습 관점에서 JPA의 `EntityManager`와 가장 가까운 개념으로 이해할 수 있다.

EF Core에서는 `DbSet<T>`와 `DbContext`가 기본적인 Repository 역할의 일부를 이미 제공한다.

Spring Data JPA의 `JpaRepository`와 완전히 같은 것은 아니다.

---

# 6. Dependency Injection (DI)

DI는 필요한 객체를 클래스 내부에서 직접 생성하지 않고 외부에서 전달받는 방식이다.

예를 들어 `RoutineService`는 `ApplicationDbContext`를 직접 생성하지 않는다.

```csharp
private readonly ApplicationDbContext _context;

public RoutineService(ApplicationDbContext context)
{
    _context = context;
}
```

다음과 같이 작성하지 않는다.

```csharp
var context = new ApplicationDbContext(...);
```

ASP.NET Core DI Container가 필요한 객체를 생성해서 전달한다.

현재 의존 관계:

```text
RoutinesController
       ↓ DI
RoutineService
       ↓ DI
ApplicationDbContext
```

Controller 역시 Service를 직접 생성하지 않는다.

```csharp
private readonly RoutineService _routineService;

public RoutinesController(RoutineService routineService)
{
    _routineService = routineService;
}
```

DI를 사용하면 각 클래스는 자신이 필요한 객체가 **어떻게 만들어지는지**보다 **무엇이 필요한지**에 집중할 수 있다.

---

# 7. Program.cs와 DI 등록

객체를 DI로 전달받으려면 먼저 DI Container에 등록해야 한다.

DbContext 등록:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString));
```

RoutineService 등록:

```csharp
builder.Services.AddScoped<RoutineService>();
```

`AddDbContext`는 일반적인 ASP.NET Core MVC 환경에서 DbContext를 Scoped 수명으로 등록한다.

즉 일반적인 HTTP 요청에서는 요청 Scope 동안 Context를 사용하고 요청이 끝나면 정리된다.

등록했다고 즉시 DB에 접속하거나 SQL을 실행하는 것은 아니다.

---

# 8. RoutineService

Controller에서 직접 DbContext를 사용하는 대신 현재 프로젝트에서는 Service를 통해 DB 작업을 수행한다.

```csharp
public class RoutineService
{
    private readonly ApplicationDbContext _context;

    public RoutineService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Routine>> GetAllAsync()
    {
        return await _context.Routines.ToListAsync();
    }

    public async Task AddAsync(Routine routine)
    {
        _context.Routines.Add(routine);
        await _context.SaveChangesAsync();
    }
}
```

## 조회

```csharp
await _context.Routines.ToListAsync();
```

개념적인 흐름:

```text
LINQ / EF Core Query
       ↓
EF Core
       ↓
SQL
       ↓
MySQL
       ↓
List<Routine>
```

## 저장

```csharp
_context.Routines.Add(routine);

await _context.SaveChangesAsync();
```

`Add()`를 호출하는 순간 바로 INSERT가 실행되는 것은 아니다.

```text
Add()
 ↓
Change Tracker에 Added 상태로 등록
 ↓
SaveChangesAsync()
 ↓
SQL INSERT 실행
 ↓
MySQL 저장
```

이 개념은 JPA의 Persistence Context / Entity 상태 관리와 비교해서 이해할 수 있다.

---

# 9. async / await

DB 조회처럼 시간이 걸리는 I/O 작업에는 비동기 API를 사용한다.

예:

```csharp
public async Task<List<Routine>> GetAllAsync()
{
    return await _context.Routines.ToListAsync();
}
```

Controller에서도:

```csharp
public async Task<IActionResult> Index()
{
    var routines = await _routineService.GetAllAsync();

    return View(routines);
}
```

현재 단계에서는 다음 정도로 이해한다.

```text
DB 작업 요청
   ↓
await
   ↓
DB 작업이 끝날 때까지 비동기적으로 기다림
   ↓
결과를 받아 다음 코드 실행
```

async/await의 내부 동작은 추후 별도로 학습한다.

---

# 10. Controller

Controller는 HTTP 요청을 받아 필요한 Service를 호출하고 어떤 응답을 반환할지 결정한다.

현재 `RoutinesController`의 Index 흐름:

```csharp
public async Task<IActionResult> Index()
{
    var routines = await _routineService.GetAllAsync();

    return View(routines);
}
```

역할을 나누면:

```text
Controller
   ↓
"전체 Routine이 필요하다"

Service
   ↓
"DB에서 Routine을 조회한다"

DbContext / EF Core
   ↓
MySQL 조회

Service
   ↓
List<Routine>

Controller
   ↓
View에 전달
```

Controller가 직접 SQL이나 DB 처리 세부사항을 담당하지 않도록 한다.

---

# 11. ASP.NET Core MVC 요청 흐름

현재 `/Routines` 요청의 전체 흐름:

```text
Browser
   │
   │ GET /Routines
   ▼
RoutinesController.Index()
   │
   ▼
RoutineService.GetAllAsync()
   │
   ▼
ApplicationDbContext
   │
   ▼
EF Core
   │
   ▼
MySQL
   │
   │ SELECT
   ▼
List<Routine>
   │
   ▼
Controller
   │
   │ View(routines)
   ▼
Views/Routines/Index.cshtml
   │
   │ Razor Rendering
   ▼
HTML
   │
   ▼
Browser
```

이 흐름은 DailyRoutine의 기본적인 조회 구조다.

---

# 12. Razor View

Razor는 HTML과 C#을 함께 사용해서 서버에서 HTML을 생성하는 ASP.NET Core의 View Template 방식이다.

기존 JSP 경험과 비교하면 이해하기 쉽다.

```text
JSP
HTML + Java

Razor
HTML + C#

React JSX
UI Markup + JavaScript
```

JSP와 Razor는 전통적으로 서버에서 HTML을 만들어 브라우저에 전달한다는 점에서 비슷하다.

React는 일반적인 SPA 구성에서는 API로 데이터를 받아 브라우저에서 UI를 구성한다.

---

# 13. Razor Model

Controller에서:

```csharp
return View(routines);
```

라고 하면 `routines` 객체가 View의 Model로 전달된다.

View에서는:

```cshtml
@model List<DailyRoutine.Models.Entities.Routine>
```

로 받을 수 있다.

관계를 표현하면:

```text
Controller

List<Routine> routines
        │
        │ return View(routines)
        ▼

Razor View

@model List<Routine>

Model
```

View에서는 이후 `Model`을 통해 Controller가 전달한 데이터를 사용할 수 있다.

---

# 14. Razor의 @ 문법

Razor에서는 `@`를 이용하여 HTML 안에서 C#을 사용한다.

조건문:

```cshtml
@if (Model.Count == 0)
{
    <p>등록된 루틴이 없습니다.</p>
}
```

반복문:

```cshtml
@foreach (var routine in Model)
{
    <td>@routine.Name</td>
}
```

HTML 내부 값 출력:

```cshtml
<td>@routine.Name</td>
```

서버에서는 Razor를 처리한 후 최종 HTML을 생성한다.

브라우저는 C#이나 Razor 코드를 실행하지 않는다.

```text
Index.cshtml
C# + HTML
    ↓
ASP.NET Core Razor
    ↓
HTML
    ↓
Browser
```

---

# 15. Null 관련 C# 문법

Razor View에서 다음 코드를 사용했다.

```csharp
routine.EndDate?.ToString("yyyy-MM-dd")
    ?? "종료일 없음"
```

## ?. 연산자

Null Conditional Operator.

```csharp
routine.EndDate?.ToString()
```

`EndDate`가 null이 아니면 `ToString()`을 호출한다.

null이면 메서드를 호출하지 않고 null을 반환한다.

## ?? 연산자

Null Coalescing Operator.

```csharp
value ?? "기본값"
```

왼쪽 값이 null이면 오른쪽 값을 사용한다.

따라서:

```csharp
routine.EndDate?.ToString("yyyy-MM-dd")
    ?? "종료일 없음"
```

의 결과는:

```text
EndDate 있음
→ 2026-12-31

EndDate 없음
→ 종료일 없음
```

---

# 16. EF Core Migration

Migration은 Entity 모델의 변경을 DB Schema 변경으로 관리하는 기능이다.

현재 최초 Migration:

```text
20260917145307_InitialCreate
```

앞의 숫자는 Migration 생성 시각을 기반으로 한 Timestamp다.

```text
YYYYMMDDHHMMSS
```

## Up()

Migration을 적용할 때 수행할 Schema 변경을 정의한다.

예:

```text
CreateTable
AddColumn
CreateIndex
```

## Down()

Migration을 되돌릴 때 수행할 작업을 정의한다.

예:

```text
DropTable
DropColumn
```

## ModelSnapshot

마지막 Migration 기준의 EF Core 모델 상태를 저장한다.

다음 Migration을 생성할 때 현재 Entity 모델과 비교하여 변경 사항을 찾는 데 사용한다.

DB 데이터 백업 파일이 아니다.

---

# 17. __EFMigrationsHistory

EF Core Migration을 DB에 적용하면 MySQL에 다음 테이블이 만들어진다.

```text
__EFMigrationsHistory
```

이 테이블에는 DB에 어떤 Migration이 적용되었는지가 기록된다.

현재 적용된 Migration:

```text
20260917145307_InitialCreate
```

따라서 EF Core는 이미 적용된 Migration을 구분할 수 있다.

개념적으로는:

```text
Migration 파일
→ 어떤 Schema 변경을 할 것인가

__EFMigrationsHistory
→ 어떤 Migration이 이미 DB에 적용되었는가
```

로 이해할 수 있다.

---

# 18. 현재 MySQL Schema

InitialCreate Migration 적용 후 생성된 테이블:

```text
Routines
__EFMigrationsHistory
```

`Routines`의 현재 주요 컬럼:

| 컬럼 | MySQL 타입 | NULL |
|---|---|---|
| Id | int | NOT NULL |
| Name | longtext | NOT NULL |
| Description | longtext | NULL |
| StartDate | date | NOT NULL |
| EndDate | date | NULL |
| CreatedAtUtc | datetime(6) | NOT NULL |
| UpdatedAtUtc | datetime(6) | NULL |

기본 설정:

```text
Storage Engine : InnoDB
Charset        : utf8mb4
Collation      : utf8mb4_unicode_ci
```

현재 `Name`에 길이 제한을 지정하지 않았기 때문에 MySQL에서는 `longtext`로 생성되었다.

향후 필요하면 Data Annotation 또는 Fluent API로 길이를 제한할 수 있다.

---

# 19. EF Core Provider

EF Core 자체가 모든 DB의 세부 SQL을 직접 처리하는 것은 아니다.

현재 구조:

```text
Application
     ↓
EF Core
     ↓
MySql.EntityFrameworkCore
     ↓
MySql.Data
     ↓
MySQL
```

`MySql.EntityFrameworkCore`는 EF Core와 MySQL 사이의 Provider 역할을 한다.

현재 사용하는 Oracle Provider에서는:

```csharp
options.UseMySQL(connectionString);
```

을 사용한다.

`UseMySQL`의 대소문자에 주의한다.

Pomelo Provider의 예제와 API를 섞지 않는다.

---

# 20. User Secrets

DB 비밀번호 같은 민감한 정보는 Git에 저장하지 않는다.

현재 실제 Connection String은 User Secrets에 저장한다.

코드에서는:

```csharp
builder.Configuration
    .GetConnectionString("DefaultConnection");
```

으로 설정을 읽는다.

개발 환경에서 설정은 여러 공급자로부터 구성될 수 있다.

개념적으로:

```text
appsettings.json
        ↓
appsettings.Development.json
        ↓
User Secrets
        ↓
Environment Variables
        ↓
Command Line
```

뒤에서 읽은 같은 키의 값이 앞의 값을 덮어쓸 수 있다.

UserSecretsId 자체는 비밀번호가 아니므로 프로젝트 파일에 포함할 수 있다.

---

# 21. 기존 ADO.NET 방식과 비교

과거 ADO.NET에서는 개발자가 직접 다음 작업을 많이 처리했다.

```text
Connection 생성
↓
Connection Open
↓
Command 생성
↓
SQL 작성
↓
Execute
↓
Result 읽기
↓
Connection Close / Dispose
```

EF Core에서는 상당 부분을 추상화한다.

```text
DbContext
↓
DbSet / LINQ
↓
EF Core
↓
Provider
↓
DB
```

하지만 내부적으로 DB Connection과 SQL 실행이 사라진 것은 아니다.

EF Core와 Provider가 해당 작업을 관리해주는 것이다.

---

# 22. 학습 원칙

DailyRoutine은 단순히 프로그램을 완성하는 것이 아니라 현대 C# / ASP.NET Core를 다시 익히는 학습 프로젝트다.

따라서 다음 원칙으로 진행한다.

1. 한 번에 전체 CRUD를 구현하지 않는다.
2. 기능을 작은 단계로 나눈다.
3. 새 코드가 나오면 역할을 이해한다.
4. 직접 실행하여 결과를 확인한다.
5. 확인된 단위로 Git Commit한다.
6. AI가 생성한 코드도 동작 원리를 확인한다.
7. 새로운 개념이 등장하면 이 문서에 정리한다.

---

# 23. 문서 관리 규칙

각 문서의 역할을 구분한다.

```text
README.md
└─ 프로젝트 소개
   기술 스택
   주요 기능
   전체 구조

PROJECT_STATUS.md
└─ 현재 구현 상태
   완료된 기능
   현재 DB 상태
   다음 작업

gpt.md
└─ 개발하면서 학습한 개념
   C# 문법
   ASP.NET Core
   EF Core
   Razor
   DI 등

Git History
└─ 언제 무엇을 변경했는지
```

## gpt.md 기록 기준

앞으로 단순 작업 이력은 추가하지 않는다.

예:

```text
2026-09-27
Create.cshtml 생성
빌드 성공
```

이런 내용은 Git History와 PROJECT_STATUS에서 관리한다.

대신 새로운 개념을 배웠을 때 기록한다.

예:

```text
Model Binding
Validation
POST
RedirectToAction
LINQ
Change Tracking
ViewModel
```

즉,

> **"오늘 무엇을 했는가?"가 아니라  
> "이번 개발에서 무엇을 배웠는가?"를 기록한다.**

이 원칙을 사용하면 프로젝트가 커져도 `gpt.md`가 단순 작업 로그로 무한히 길어지는 것을 방지할 수 있다.