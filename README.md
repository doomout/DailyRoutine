## 핵심 학습 개념

이 프로젝트에서는 ASP.NET Core와 Entity Framework Core를 사용하면서
기존의 직접적인 DB 연결 방식과 다른 현대적인 .NET 애플리케이션 구조를 학습합니다.

### Entity Framework Core

Entity Framework Core(EF Core)는 .NET에서 사용하는 ORM(Object-Relational Mapper)입니다.

기존 ADO.NET 방식에서는 개발자가 직접 Connection을 생성하고,
SQL을 작성한 뒤 실행 결과를 객체에 매핑하고 Connection을 관리해야 했습니다.

```text
Application
    ↓
Connection 생성
    ↓
SQL 직접 작성
    ↓
DB 실행
    ↓
결과를 객체로 변환
```

EF Core를 사용하면 C# Entity와 LINQ를 중심으로 데이터를 다룰 수 있으며,
EF Core와 Database Provider가 SQL 생성과 데이터 매핑 등의 작업을 담당합니다.

DailyRoutine의 DB 접근 구조는 다음과 같습니다.

```text
Routine / Service
        ↓
ApplicationDbContext
        ↓
Entity Framework Core
        ↓
MySql.EntityFrameworkCore
        ↓
MySQL
```

`ApplicationDbContext`는 EF Core의 `DbContext`를 상속하며
애플리케이션에서 Entity 조회, 변경 추적 및 저장 작업의 중심 역할을 합니다.

예를 들어 다음과 같이 `Routine` Entity에 접근할 수 있습니다.

```csharp
var routines = await _context.Routines.ToListAsync();
```

EF Core는 이러한 LINQ 표현식을 Database Provider가 처리할 수 있는 SQL로 변환합니다.

---

### Dependency Injection

DI(Dependency Injection, 의존성 주입)는 객체가 필요한 의존 객체를
직접 생성하지 않고 외부에서 전달받도록 구성하는 방식입니다.

예를 들어 Service에서 `ApplicationDbContext`가 필요할 때 직접 생성하지 않습니다.

```csharp
public class RoutineService
{
    private readonly ApplicationDbContext _context;

    public RoutineService(ApplicationDbContext context)
    {
        _context = context;
    }
}
```

대신 `Program.cs`에서 `ApplicationDbContext`를 ASP.NET Core의
DI Container에 등록합니다.

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString));
```

이후 `ApplicationDbContext`가 필요한 Controller나 Service의 생성자에 선언하면
ASP.NET Core가 등록된 설정에 따라 객체를 생성하여 전달합니다.

```text
Program.cs
    ↓
DI Container에 ApplicationDbContext 등록
    ↓
Controller / Service가 ApplicationDbContext 요청
    ↓
ASP.NET Core가 필요한 객체를 생성하여 주입
```

이를 통해 객체 생성 및 수명 관리와 비즈니스 로직을 분리할 수 있습니다.

---

### ASP.NET Core와 MySQL 연결 구조

DailyRoutine에서는 DB 연결 정보를 코드에 직접 작성하지 않고
ASP.NET Core Configuration과 User Secrets를 사용합니다.

```text
User Secrets
    ↓
ConnectionStrings:DefaultConnection
    ↓
Program.cs
    ↓
AddDbContext<ApplicationDbContext>
    ↓
UseMySQL
    ↓
ApplicationDbContext
    ↓
MySql.EntityFrameworkCore
    ↓
Docker MySQL
```

실제 DB 계정과 비밀번호가 포함된 Connection String은
Git 저장소에 커밋하지 않습니다.

`Program.cs`에서는 설정된 Connection String을 다음과 같이 가져옵니다.

```csharp
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");
```

그리고 EF Core가 MySQL을 사용하도록 Provider와 `ApplicationDbContext`를 등록합니다.

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString));
```

`AddDbContext` 등록 자체가 DB 테이블을 생성하는 것은 아닙니다.
DB 스키마 생성 및 변경은 이후 EF Core Migration을 통해 관리합니다.

## ASP.NET Core와 Spring Boot 개념 비교

DailyRoutine 프로젝트에서 사용하는 ASP.NET Core와 EF Core의 여러 개념은
Java의 Spring Boot 및 JPA/Hibernate와 유사한 역할을 합니다.

완전히 동일한 기술은 아니지만, 새로운 .NET 개념을 이해하기 위한
학습 관점에서는 다음과 같이 비교할 수 있습니다.

| Java / Spring | C# / .NET | 역할 |
| --- | --- | --- |
| Spring Boot | ASP.NET Core | 웹 애플리케이션 프레임워크 |
| Spring MVC | ASP.NET Core MVC | MVC 기반 웹 개발 |
| Spring DI Container | ASP.NET Core DI Container | 객체 생성 및 의존성 관리 |
| JPA / Hibernate | Entity Framework Core | ORM 및 Entity 관리 |
| EntityManager | DbContext | Entity 조회, 변경 추적 및 저장 |
| Entity | Entity | DB 테이블과 연결되는 객체 |
| Repository | DbSet 및 LINQ | Entity 조회 및 데이터 작업 |
| JPQL / Criteria API | LINQ | 객체 중심의 데이터 조회 |
| application.yml / properties | appsettings.json | 애플리케이션 설정 |
| Flyway / Liquibase | EF Core Migration | DB 스키마 변경 관리 |

> 위 비교는 개념을 이해하기 위한 학습용 대응 관계이며,
> 각 기술의 구조와 동작 방식이 완전히 동일하다는 의미는 아닙니다.

### 전체 구조 비교

Spring Boot에서 JPA/Hibernate를 사용하는 경우 대략 다음과 같은 구조를 가집니다.

```text
Spring Boot
    ↓
Controller
    ↓
Service
    ↓
Spring Data JPA / Repository
    ↓
JPA / Hibernate
    ↓
Database
```

DailyRoutine에서는 다음과 같은 구조를 사용합니다.

```text
ASP.NET Core
    ↓
Controller
    ↓
Service
    ↓
ApplicationDbContext / DbSet
    ↓
Entity Framework Core
    ↓
MySQL Provider
    ↓
MySQL
```

DailyRoutine 초기 단계에서는 별도의 Repository 계층을 만들지 않고
EF Core의 `DbContext`와 `DbSet`을 이용하여 데이터를 관리할 예정입니다.

---

## 기존 C# 개발 방식과 현재 .NET 개발 방식

과거 ADO.NET을 직접 사용하는 방식에서는 개발자가 DB 연결과 SQL 실행을
상대적으로 직접 관리했습니다.

```text
Application
    ↓
Connection 생성
    ↓
Connection Open
    ↓
SQL 작성
    ↓
Command 실행
    ↓
결과 처리
    ↓
Connection Close
```

DailyRoutine에서는 ASP.NET Core의 DI와 EF Core를 사용하여
이러한 작업의 상당 부분을 프레임워크에 맡깁니다.

```text
Program.cs
    ↓
DI Container
    ↓
Controller
    ↓
Service
    ↓
ApplicationDbContext
    ↓
EF Core
    ↓
MySQL Provider
    ↓
MySQL
```

따라서 개발자는 Connection을 필요한 곳마다 직접 생성하기보다
애플리케이션 시작 시 필요한 구성 요소를 DI Container에 등록하고,
Controller나 Service에서 필요한 객체를 생성자를 통해 전달받습니다.

DB 작업 역시 SQL을 직접 작성하는 방식뿐만 아니라
Entity와 LINQ를 이용하여 처리할 수 있습니다.

예:

```csharp
var routines = await _context.Routines.ToListAsync();
```

EF Core는 이러한 LINQ 표현식을 DB Provider가 처리할 수 있는 SQL로 변환하고,
조회 결과를 다시 C# Entity 객체로 매핑합니다.

---

## 현재 학습 방향

DailyRoutine은 애플리케이션 구현과 함께
현대적인 C# 및 .NET 개발 방식을 학습하는 것을 목표로 합니다.

특히 다음 개념은 프로젝트 구현 과정에서 실제 코드와 함께 학습합니다.

- Modern C# 문법
  - nullable reference type
  - `required`
  - `DateOnly`
  - `async` / `await`
  - LINQ
- ASP.NET Core
  - MVC
  - Dependency Injection
  - Configuration
  - User Secrets
  - Middleware
- Entity Framework Core
  - Entity
  - DbContext
  - DbSet
  - Change Tracking
  - Migration
  - LINQ 기반 DB 조회
- Database
  - MySQL
  - Docker 기반 DB 환경
  - DB Provider
  - Schema Migration

새로운 개념을 학습할 때는 단순히 사용법만 익히지 않고
다음 관점에서 비교하며 이해합니다.

```text
새로운 C# / .NET 개념
        ↓
과거 C# / ADO.NET에서는 어떻게 처리했는가?
        ↓
Spring Boot / JPA에서는 어떤 개념에 해당하는가?
        ↓
DailyRoutine에서는 실제로 어디에 사용되는가?
```

예를 들어 EF Core를 처음 학습할 때는 다음과 같이 이해합니다.

```text
Java / Spring                    C# / .NET
────────────────────────────────────────────

JPA / Hibernate        ← 비교 →   EF Core

EntityManager          ← 비교 →   DbContext

Entity                 ← 비교 →   Entity

JPQL / Criteria        ← 비교 →   LINQ

Spring DI              ← 비교 →   ASP.NET Core DI
```

이러한 비교는 각 기술이 완전히 동일하다는 의미가 아니라,
기존 개발 경험을 기반으로 새로운 .NET 기술의 역할을 빠르게 이해하기 위한
학습 방법으로 사용합니다.

프로젝트를 진행하면서 새롭게 조사하거나 학습한 세부 내용은
`gpt.md`에 지속적으로 기록합니다.