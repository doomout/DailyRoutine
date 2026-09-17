> 이 문서는 날짜별 학습 이력이다. 아래 초기 조사 내용은 당시 상태이며, 최신 상태는 마지막의 2026-09-18 InitialCreate 적용 기록과 PROJECT_STATUS.md를 따른다.

# EF Core와 로컬 MySQL 연결 사전 조사

조사일: 2026-09-15

이 문서는 프로젝트 환경을 확인하고 공식 문서 및 배포 패키지를 조사한 결과다. 패키지 설치, `ApplicationDbContext` 생성, `appsettings.json` 수정은 진행하지 않았다. 실제 DB 연결 구현은 사용자 승인 이후 별도 단계로 진행한다.

## 현재 프로젝트 환경

`DailyRoutine.csproj`, `Program.cs`, `dotnet --info`에서 확인했다.

| 항목 | 현재 상태 |
|---|---|
| 프로젝트 | ASP.NET Core MVC |
| 대상 프레임워크 | `net10.0` — .NET 10 |
| 사용 SDK | `10.0.401` |
| .NET / ASP.NET Core 런타임 | `10.0.12` |
| 운영체제 | Windows 10, x64 |
| NuGet 패키지 참조 | 아직 없음 |
| EF Core 설정 | 아직 없음 |

로컬 MySQL 서버 버전은 아직 확인되지 않았다. 실제 연결 단계에서 서버 버전과 Provider의 지원 범위를 확인해야 한다.

**추천 조합: .NET 10 + EF Core 10 + Oracle의 `MySql.EntityFrameworkCore`.**

## 1. 필요한 NuGet 패키지

| 패키지 | 필요 시점 |
|---|---|
| `MySql.EntityFrameworkCore` | MySQL과 EF Core를 연결할 때 |
| `Microsoft.EntityFrameworkCore` | EF Core 기본 기능에 필요. Provider의 의존성으로도 포함됨 |
| `Microsoft.EntityFrameworkCore.Design` | 이후 마이그레이션 도구를 사용할 때 |
| `Microsoft.EntityFrameworkCore.Tools` | Visual Studio 패키지 관리자 콘솔을 사용할 경우에만 선택 |

모든 패키지를 직접 추가할 필요는 없다. Provider를 참조하면 EF Core 본체와 필요한 하위 패키지가 의존성으로 포함된다.

근거: [Oracle 패키지 의존성](https://www.nuget.org/packages/MySql.EntityFrameworkCore/10.0.9)

## 2. 각 패키지의 역할

### MySql.EntityFrameworkCore

EF Core의 조회·저장 작업을 MySQL에 맞게 처리한다. MySQL SQL 문법과 데이터 형식 매핑 등을 담당하는 Provider다.

### Microsoft.EntityFrameworkCore

ORM의 핵심이다. `DbContext`, `DbSet<T>`, 객체 변경 추적, LINQ 기반 조회와 저장 기능을 제공한다. ORM은 C# 객체와 DB의 행을 연결하는 도구다.

### Microsoft.EntityFrameworkCore.Design

개발 도구가 프로젝트의 모델과 `DbContext`를 분석하도록 돕는다. 마이그레이션은 테이블 생성·변경 내용을 코드로 기록하고 적용하는 기능이다.

### Microsoft.EntityFrameworkCore.Tools

Visual Studio 패키지 관리자 콘솔의 `Add-Migration`, `Update-Database` 같은 명령을 제공한다.

PowerShell에서 `dotnet ef`를 사용할 경우에는 별도의 .NET 도구인 `dotnet-ef`와 `Design` 패키지를 사용한다. `Tools` 패키지는 이 방식에 필수가 아니다.

근거: [Microsoft EF Core CLI 도구 문서](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

실제 MySQL 서버 통신은 Oracle Provider가 의존하는 `MySql.Data`가 담당한다. 일반적인 EF Core 사용에서는 이를 따로 직접 추가할 필요가 없다.

## 3. 현재 .NET 버전과의 호환성

.NET 버전과 EF Core 버전은 구분해야 한다. .NET 10 프로젝트라고 EF Core 10만 사용할 수 있는 것은 아니다. 다만 Provider는 사용하는 EF Core 버전과 맞아야 한다.

근거: [Microsoft Provider 문서](https://learn.microsoft.com/en-us/ef/core/providers/)

| 조합 | 현재 프로젝트에서의 판단 |
|---|---|
| .NET 10 + EF Core 10 + Oracle Provider `10.0.9` | 추천. Provider에 `net10.0`용 구성과 EF Core `10.0.9 이상` 의존성이 명시됨 |
| .NET 10 + EF Core 9 + Pomelo `9.0.0` | 사용 가능한 대안. Pomelo가 .NET 8 이상과 EF Core 9 지원을 명시함 |
| .NET 10 + EF Core 10 + Pomelo `9.0.0` | 호환 조합이 아님. Pomelo 9는 EF Core 9용 |

근거: [Oracle NuGet 패키지](https://www.nuget.org/packages/MySql.EntityFrameworkCore/10.0.9), [Pomelo 공식 호환성 표](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql#compatibility)

조사 시점에 확인된 EF Core 최신 안정 패치는 `10.0.12`다. 이후 설치한다면 Microsoft의 EF Core 본체·Design·도구는 같은 패치 버전으로 맞추는 구성이 적절하다. Provider의 패치 번호까지 같을 필요는 없고, Provider가 요구하는 의존성 범위를 만족해야 한다.

근거: [EF Core 10.0.12 패키지](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/10.0.12)

## 4. MySQL용 EF Core Provider 선택 이유

이번 프로젝트에서는 Oracle의 `MySql.EntityFrameworkCore`를 추천한다.

1. 현재 프로젝트의 .NET 10과 EF Core 10을 그대로 사용할 수 있다.
2. MySQL 제작사인 Oracle이 배포하는 Provider다.
3. 조사 시점에 확인된 Pomelo 안정 버전은 EF Core 9용이다. 사용 가능한 대안이지만 새로 학습을 시작하면서 EF Core 9를 선택할 필요가 줄어든다.
4. EF Core 10의 지원 기간이 더 길다. Microsoft 문서상 EF Core 10은 2028년 11월 10일, EF Core 9는 2026년 11월 10일까지 지원된다.

근거: [Microsoft 지원 일정](https://learn.microsoft.com/en-us/ef/core/what-is-new/)

MySQL 개발자 가이드의 호환성 표에는 아직 EF Core 10이 preview로 표시되어 있지만, 현재 Oracle 배포 패키지에는 정식 EF Core 10 의존성이 명시되어 있다. 따라서 이번 판단은 실제 배포 패키지의 정보를 함께 확인한 결과다.

근거: [MySQL 개발자 가이드](https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html), [Oracle 배포 패키지 정보](https://www.nuget.org/packages/MySql.EntityFrameworkCore/10.0.9)

## 5. ApplicationDbContext의 역할

`ApplicationDbContext`는 이 애플리케이션의 DB 작업을 관리하도록 우리가 만드는 클래스다. EF Core가 제공하는 `DbContext`를 상속하며, 이름 자체가 필수 규칙은 아니다.

주요 역할은 다음과 같다.

- **모델 구성:** `Routine`을 어떤 테이블·열·기본 키에 연결할지 정의한다.
- **조회 진입점 제공:** `DbSet<Routine>`을 통해 루틴을 조회하고 추가·삭제 대상으로 등록한다.
- **변경 추적:** 조회한 루틴의 이름 등이 바뀌었는지 기억한다.
- **저장:** `SaveChangesAsync()`를 호출하면 추적한 변경 사항을 DB에 반영한다.
- **DB 설정 사용:** 주입받은 Provider와 연결 설정을 이용한다.

예를 들어 루틴을 추적 조회한 뒤 `Name`을 변경하면, Context가 그 변경을 추적하고 저장 호출 시 `UPDATE`를 실행하도록 처리한다.

일반적인 ASP.NET Core 구성에서는 HTTP 요청마다 Context를 하나씩 사용한다.

근거: [Microsoft DbContext 문서](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)

## 6. Spring Boot의 JPA / EntityManager / Repository와 비교

완전한 일대일 대응은 아니지만, 다음처럼 이해하면 좋다.

| Spring / Java | EF Core / 현재 프로젝트 | 비교 |
|---|---|---|
| JPA + Hibernate | EF Core | 객체와 DB를 연결하는 ORM 영역. JPA는 표준이고 Hibernate는 구현체이며, EF Core는 자체 API와 구현을 제공 |
| `@Entity` 클래스 | `Routine` 클래스 | DB에 매핑할 객체. EF Core에서는 관례·설정으로 등록할 수 있어 `@Entity` 같은 표시가 필수는 아님 |
| `EntityManager`와 영속성 컨텍스트 | `ApplicationDbContext`와 변경 추적 기능 | 객체 조회, 상태 관리, 변경 추적과 저장을 관리 |
| `JpaRepository<Routine, Integer>` | `DbSet<Routine>` + `DbContext`가 기능 일부에 대응 | 기본 조회·추가·삭제·저장을 제공하지만, 메서드 이름으로 쿼리를 자동 생성하는 Spring Data Repository와 같지는 않음 |
| JDBC Driver | `MySql.Data` | 실제 DB 서버 통신 |
| Hibernate의 MySQL Dialect | MySQL EF Core Provider의 역할 일부 | DB에 맞는 SQL·형식 처리. EF Core Provider는 이보다 넓은 통합 역할을 수행 |

근거: [JPA EntityManager 공식 문서](https://jakarta.ee/specifications/persistence/3.2/apidocs/jakarta.persistence/jakarta/persistence/entitymanager), [Spring Data JPA 공식 문서](https://docs.spring.io/spring-data/jpa/reference/jpa.html)

핵심은 **`ApplicationDbContext`가 `EntityManager`에 가장 가깝다**는 것이다. 별도의 `RoutineRepository`는 필요해질 때 설계할 수 있으며, EF Core 사용의 필수 조건은 아니다.

## 진행 상태

- 환경 확인 및 공식 문서 조사 완료.
- 패키지 설치, `ApplicationDbContext` 생성, `appsettings.json` 수정은 진행하지 않음.
- 다음 구현 단계는 사용자 승인 대기.

---

## 후속 작업: NuGet 패키지 설치 완료

작업일: 2026-09-15

사용자 승인에 따라 이번 단계에서는 두 NuGet 패키지 설치와 프로젝트 빌드만 진행했다. 아래 내용은 앞선 사전 조사 이후의 실제 작업 결과다.

### 직접 설치한 패키지

| 패키지 | 설치 버전 | 역할 |
|---|---|---|
| `MySql.EntityFrameworkCore` | `10.0.9` | Oracle의 MySQL EF Core Provider. MySQL SQL 생성과 데이터 형식 매핑 등을 담당 |
| `Microsoft.EntityFrameworkCore.Design` | `10.0.12` | 이후 마이그레이션 등 개발 도구가 모델과 Context를 분석하는 데 필요한 기능 제공 |

두 버전 모두 preview/RC가 아닌 안정 버전이다. Oracle Provider의 `net10.0` 의존성은 EF Core 및 Relational `10.0.9 이상`이며, 이번에 실제 복원된 EF Core `10.0.12`가 이를 충족한다. Design도 .NET 10용 `10.0.12`를 사용한다.

확인 자료: [Oracle Provider 10.0.9](https://www.nuget.org/packages/MySql.EntityFrameworkCore/10.0.9), [Microsoft Design 10.0.12](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Design/10.0.12)

### 변경된 DailyRoutine.csproj

기존 `net10.0`, nullable 및 implicit usings 설정은 유지하고 다음 `ItemGroup`만 추가했다.

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.12">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
  <PackageReference Include="MySql.EntityFrameworkCore" Version="10.0.9" />
</ItemGroup>
```

- `PackageReference`: 이 프로젝트가 직접 사용하는 패키지를 선언한다. 직접 참조는 위 두 개뿐이다.
- `Version`: 요청하는 패키지 버전을 명시한다. 이번 복원에서는 두 패키지 모두 명시한 버전으로 결정되었다.
- `PrivateAssets=all`: Design을 이 프로젝트 내부 개발 의존성으로 사용하고, 이 프로젝트를 참조하는 다른 프로젝트로 해당 의존성이 전파되지 않게 한다. 현재 프로젝트에서 다운로드하지 않거나 사용하지 않는다는 뜻은 아니다.
- `IncludeAssets`: Design에서 사용할 자산 종류를 지정한다. 런타임·빌드·분석기 등의 자산을 사용하고 `compile`은 제외하여 일반 애플리케이션 코드에서 Design API를 직접 참조하지 않는 기본 구성으로 설정했다.

### 의존성으로 자동 설치된 패키지

직접 설치는 `csproj`에 `PackageReference`로 선언하는 것이고, 의존성 설치는 그 패키지들이 필요로 하는 하위 패키지를 NuGet이 자동으로 복원하는 것이다. 아래 버전은 실제 `obj/project.assets.json`의 `net10.0` 결과로 확인했다.

| 패키지 | 실제 복원 버전 | 설치 구분과 역할 |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | `10.0.12` | 간접 참조. Provider 및 Relational이 요구하는 ORM 본체 |
| `Microsoft.EntityFrameworkCore.Relational` | `10.0.12` | 간접 참조. Provider와 Design이 요구하는 관계형 DB 공통 기능 |
| `Microsoft.EntityFrameworkCore.Abstractions` | `10.0.12` | 간접 참조. EF Core 본체가 요구하는 기본 추상화 |
| `Microsoft.EntityFrameworkCore.Analyzers` | `10.0.12` | 간접 참조. EF Core 본체가 요구하는 코드 분석기 |
| `MySql.Data` | `26.7.0` | 간접 참조. Oracle Provider가 요구하는 MySQL 통신 드라이버 |

이 외에도 Design이 사용하는 Roslyn 코드 분석·생성 관련 패키지와 Humanizer, MySql.Data가 사용하는 암호화·압축 관련 패키지 등이 자동 복원되었다. 이들 역시 별도의 직접 참조로 추가하지 않았다.

핵심 의존성 관계는 다음과 같다.

```text
DailyRoutine
├─ MySql.EntityFrameworkCore 10.0.9 [직접]
│  ├─ Microsoft.EntityFrameworkCore 10.0.12 [간접]
│  ├─ Microsoft.EntityFrameworkCore.Relational 10.0.12 [간접]
│  └─ MySql.Data 26.7.0 [간접]
└─ Microsoft.EntityFrameworkCore.Design 10.0.12 [직접]
   └─ Microsoft.EntityFrameworkCore.Relational 10.0.12 [간접]
      └─ Microsoft.EntityFrameworkCore 10.0.12 [간접]
```

같은 패키지가 여러 경로에 나와도 중복 버전으로 설치된다는 의미는 아니다. NuGet이 요구 조건을 종합하여 이번에는 EF Core와 Relational 모두 `10.0.12`로 결정했다. Provider의 `10.0.9`는 EF Core도 반드시 `10.0.9`여야 한다는 뜻이 아니다.

`Microsoft.EntityFrameworkCore.Tools`, `dotnet-ef`, Pomelo는 설치하지 않았다. 이후 CLI로 마이그레이션을 실행할 때에는 별도 단계에서 `dotnet-ef` 도구 준비 여부를 확인하면 된다. Design 설치만으로 CLI 도구가 설치되는 것은 아니다.

### 설치 및 검증 결과

두 패키지 참조를 버전을 지정해 추가한 뒤 `dotnet restore DailyRoutine.csproj --verbosity quiet`로 복원했다. Design의 개발 의존성 설정을 반영한 후 아래 명령으로 최종 빌드했다.

```text
dotnet build DailyRoutine.csproj --verbosity quiet --nologo

빌드 성공
경고 0개
오류 0개
```

`--no-restore`로 참조를 추가할 때 출력된 호환성 검사 생략 안내는 후속 restore와 최종 build 전에 나온 안내다. 최종 restore와 build는 정상 완료되었다.

초기 실행은 사용자 프로필의 .NET 초기화 파일 접근 제한으로 실패하여 권한 승인 후 다시 실행했다. 이 과정에서 .NET CLI의 최초 실행 초기화가 수행되었고, CLI는 ASP.NET Core HTTPS 개발 인증서를 설치했다고 출력했다. 별도의 인증서 신뢰 명령은 실행하지 않았다.

최종 빌드는 패키지 복원과 컴파일의 성공을 확인한 것이다. 아직 MySQL 서버 접속이나 SQL 실행을 검증한 것은 아니다.

### 이번 단계의 변경 범위와 다음 상태

- 소스 관리 대상 변경: `DailyRoutine.csproj`, 기존 조사 문서 `gpt.md`.
- 패키지 복원 및 빌드에 따른 `obj`와 `bin` 산출물 생성·갱신.
- `ApplicationDbContext`는 생성하지 않았다.
- `Program.cs`와 `appsettings.json`은 수정하지 않았다.
- Migration은 생성하지 않았고 DB 변경도 수행하지 않았다.
- 이번 패키지 설치 단계 완료. 다음 단계는 사용자 승인 대기.

문서는 기존 파일명 `gpt.md`에 이어서 기록했다. 현재 Windows 작업 환경에서는 요청한 `GPT.md`와 동일한 파일을 가리킨다.

---

## 후속 작업: ApplicationDbContext 정의 완료

작업일: 2026-09-15

사용자 승인에 따라 `Data/ApplicationDbContext.cs`를 생성했다. 이번 단계의 목표는 EF Core가 `Routine`을 관리할 Context 클래스를 정의하는 것까지다.

### 구현한 코드의 구조

실제 파일에는 각 구성 요소의 역할을 설명하는 한글 주석을 작성했다. 핵심 구조는 다음과 같다.

```csharp
using DailyRoutine.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyRoutine.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Routine> Routines => Set<Routine>();
    }
}
```

### 각 코드가 EF Core에서 하는 일

| 코드 | 역할 |
|---|---|
| `using DailyRoutine.Models.Entities;` | 기존 `Routine` 엔티티 형식을 사용한다. 이 using 자체가 엔티티를 등록하는 것은 아니다. |
| `using Microsoft.EntityFrameworkCore;` | `DbContext`, `DbContextOptions<TContext>`, `DbSet<TEntity>`를 사용한다. |
| `namespace DailyRoutine.Data` | 데이터 접근을 담당하는 형식을 프로젝트의 Data 네임스페이스로 묶는다. |
| `ApplicationDbContext : DbContext` | EF Core의 조회, 모델 관리, 변경 추적, 저장 기능을 상속한다. |
| `DbContextOptions<ApplicationDbContext> options` | 이 Context에 사용할 설정을 외부에서 받는다. 이후 Provider와 연결 정보 등을 설정할 수 있다. |
| `: base(options)` | 전달받은 설정을 부모 `DbContext` 생성자에 넘긴다. 실제 설정의 사용은 EF Core가 담당한다. |
| `DbSet<Routine>` | `Routine`을 모델에 포함시키고 해당 엔티티를 조회하거나 추가·삭제 대상으로 등록하는 진입점을 제공한다. |
| `Routines => Set<Routine>()` | 현재 Context가 관리하는 Routine용 DbSet을 반환하는 읽기 전용 속성이다. 직접 `new DbSet`을 하거나 null 초기값을 둘 필요가 없다. |

생성자 주입은 클래스 내부에서 설정을 만드는 대신 외부에서 설정을 받는 방식이다. 현재 생성자는 주입을 받을 수 있도록 준비되었지만, `Program.cs`에서 DI 컨테이너에 등록하는 작업은 아직 하지 않았다.

`Routines` 속성이 읽기 전용이라는 것은 DbSet 자체를 다른 값으로 대입할 수 없다는 뜻이다. 향후 `Routines.Add(...)`처럼 DbSet의 메서드를 사용하는 것은 가능하다.

### 모델 등록과 실제 DB 작업의 차이

- 공개된 `DbSet<Routine>` 속성을 통해 EF Core가 모델을 구성할 때 Routine을 발견한다.
- 기본 관례에서는 `Routine.Id`를 기본 키로 인식한다. 별도 테이블 매핑이 없다면 관계형 매핑의 테이블 이름은 DbSet 속성명인 `Routines`를 따른다.
- 이것은 모델의 정의다. Context 파일이나 DbSet을 선언한다고 MySQL에 테이블이 만들어지지는 않는다.
- `Routines` 속성을 읽는 것만으로 전체 루틴을 조회하지 않는다. 이후 Provider가 설정된 상태에서 쿼리를 실행할 때 실제 조회가 이루어진다.
- 추가·삭제 대상으로 등록하거나 추적 중인 엔티티의 속성을 변경한 뒤 `SaveChangesAsync()` 등을 호출해야 해당 변경을 저장한다.
- 이번 단계에서는 Provider 설정, Context 인스턴스의 런타임 모델 검증, SQL 실행을 하지 않았다.

### Spring Boot / JPA와 비교

| Spring / JPA 개념 | 이번 구현에서 가까운 개념 | 공통점과 차이 |
|---|---|---|
| `EntityManager` | `ApplicationDbContext` 인스턴스 | 조회와 엔티티 상태 관리, 변경 추적, 저장을 담당한다. Context가 EntityManager에 가장 가깝다. |
| 영속성 컨텍스트 | DbContext 내부의 변경 추적 기능 | 관리하는 엔티티와 그 상태를 기억한다. EF Core에서는 ChangeTracker를 통해 추적 상태를 다룬다. |
| `JpaRepository<Routine, Integer>` | `DbSet<Routine>`과 DbContext의 기본 기능 일부 | DbSet은 루틴 조회·추가·삭제의 진입점이고 저장은 Context가 담당한다. Spring Data처럼 메서드 이름을 해석해 Repository 구현을 자동 생성하지 않는다. |
| 생성자 주입 | `DbContextOptions<ApplicationDbContext>`를 받는 생성자 | 외부에서 의존성을 전달받는 원리는 같다. 여기서는 EntityManager 자체가 아니라 Context를 구성할 설정을 전달받는다. |

예를 들어 이후 루틴 이름을 수정할 때에는 Context로 엔티티를 추적 조회하고, `Name`을 바꾼 다음 `SaveChangesAsync()`로 저장할 수 있다. JPA에서 관리 중인 엔티티의 변경을 감지하는 흐름과 비슷하다.

다만 `SaveChangesAsync()`를 JPA의 `flush()`나 트랜잭션 commit과 완전히 같은 것으로 보면 안 된다. EF Core는 명시적인 저장 호출을 통해 변경을 반영하며, 트랜잭션의 경계는 별도로 고려한다. 요청이 끝났다는 이유만으로 자동 저장되는 것은 아니다.

따라서 이번 단계에서는 별도의 `RoutineRepository`를 만들 필요가 없다. 애플리케이션 구조상 필요해질 때 Context를 감싸는 Repository를 추가할 수 있다.

학습 참고 자료:

- [Microsoft DbContext 구성과 수명](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
- [Microsoft 엔티티 형식과 테이블 매핑](https://learn.microsoft.com/en-us/ef/core/modeling/entity-types)
- [JPA EntityManager API](https://jakarta.ee/specifications/persistence/3.2/apidocs/jakarta.persistence/jakarta/persistence/entitymanager)
- [Spring Data JPA](https://docs.spring.io/spring-data/jpa/reference/jpa.html)

### 빌드 확인

기존에 복원한 패키지를 사용하여 컴파일 오류 여부만 확인했다.

```text
dotnet build DailyRoutine.csproj --no-restore --verbosity quiet --nologo

빌드 성공
경고 0개
오류 0개
```

이 결과는 Context 코드가 컴파일된다는 의미이며, 실제 MySQL 접속이나 모델 매핑·SQL 실행의 성공을 검증한 것은 아니다.

### 변경 범위와 다음 상태

- 생성: `Data/ApplicationDbContext.cs`.
- 학습 기록 추가: 기존 `gpt.md`.
- `Program.cs`, `appsettings.json`, `DailyRoutine.csproj`는 이번 단계에서 수정하지 않았다.
- MySQL 연결 및 Migration 생성은 진행하지 않았다.
- ApplicationDbContext 정의 단계 완료. 다음 단계는 사용자 승인 대기.

---

## 로컬 MySQL 연결 재조사 — 구현 전 승인 대기

조사일: 2026-09-17

대상은 `D:\khg_EX\DailyRoutine`의 C# / ASP.NET Core MVC / .NET 10 프로젝트다. `PROJECT_STATUS.md`와 실제 소스를 대조했다. 이번에는 이 학습 기록만 추가했으며 애플리케이션 코드·설정·PROJECT_STATUS.md는 변경하지 않았다. 다른 Java 프로젝트나 Docker 환경은 조사·사용하지 않았다.

### 1. 현재 DB 연결 설정 재확인

| 확인 대상 | 실제 상태 |
|---|---|
| `Models/Entities/Routine.cs` | 7개 속성의 Entity 정의 존재 |
| `Data/ApplicationDbContext.cs` | options 생성자와 `DbSet<Routine>` 존재. Provider 설정/OnConfiguring 없음 |
| `Program.cs` | MVC 등록만 존재. AddDbContext 및 UseMySQL 호출 없음 |
| `appsettings.json`, `appsettings.Development.json` | ConnectionStrings 없음 |
| `Properties/launchSettings.json` | Development 환경 및 실행 URL만 존재. DB 연결 설정 없음 |
| `DailyRoutine.csproj` | MySql.EntityFrameworkCore 10.0.9, Microsoft.EntityFrameworkCore.Design 10.0.12 참조. UserSecretsId 없음 |
| `obj/project.assets.json` | 기존 복원 결과에 MySql.EntityFrameworkCore 10.0.9 및 Microsoft.EntityFrameworkCore 10.0.12 확인 |
| 기타 애플리케이션 소스 검색 | 별도 Provider 등록, 디자인 타임 Context 팩터리, 연결 설정을 찾지 못함 |

**현재 프로젝트 코드에는 DB 연결 설정이 없다. Context 클래스 정의만 완료된 상태다.** Windows에 별도로 설정된 환경 변수나 외부 비밀 저장소의 값은 읽지 않았다. 그러한 값이 있더라도 현재 코드에는 이를 Context의 Provider 설정으로 연결하는 부분이 없다.

기존 복원 산출물 및 설치 패키지 문서는 읽기만 했고 복원·설치·빌드는 수행하지 않았다. 기존 PROJECT_STATUS.md의 DB 미연결 판단은 그대로 유효하다.

### 2. Windows에서 MySQL 서버 실행 여부를 확인하는 방법

아래는 사용자가 확인할 절차이며 이번 조사에서 서비스 시작/중지, 서버 접속 또는 포트 검사는 실행하지 않았다. **실제 서버 가동 여부는 아직 미확인이다.**

1. Windows에서 `Win + R` → `services.msc` → MySQL 관련 서비스를 찾는다.
2. 상태가 실행 중인지 확인한다. 서비스 이름은 설치 시 정할 수 있으므로 MySQL80/MySQL84 등으로 단정하지 않는다.
3. 서비스 속성의 실행 파일 경로를 확인하여 DailyRoutine에 사용할 Windows 설치 MySQL인지 식별한다.

PowerShell의 읽기 전용 확인 예:

```powershell
# 이름 또는 표시 이름에 MySQL이 포함된 Windows 서비스의 상태만 확인한다.
Get-Service | Where-Object {
    $_.Name -like '*MySQL*' -or $_.DisplayName -like '*MySQL*'
} | Select-Object Name, DisplayName, Status
```

이름이 다르거나 서비스가 아닌 방식으로 실행 중이면 검색 결과가 없을 수 있다. 결과가 없다는 것만으로 미설치/미실행을 확정하지 않는다.

호스트와 포트를 확정한 뒤에만 사용할 보조 검사 예:

```powershell
# 3306은 예시다. 확인된 대상 포트로 바꿔 사용한다.
Test-NetConnection -ComputerName localhost -Port 3306 -InformationLevel Quiet
```

포트 응답은 해당 포트에 접근 가능하다는 뜻일 뿐, MySQL인지 또는 DailyRoutine용 서버인지, 인증 가능한지를 보장하지 않는다. 실행 파일 경로·서비스·접속 설정을 함께 확인해야 한다. 다른 프로젝트의 포트나 컨테이너에 임의로 연결하지 않는다.

근거: [MySQL Windows 서비스 문서](https://dev.mysql.com/doc/refman/8.4/en/windows-start-service.html).

### 3. MySQL 서버 버전 확인 방법

MySQL Workbench에서 **확인된 Windows 로컬 서버 연결**을 선택하고 SQL 편집기에서 실행한다.

```sql
-- 현재 접속한 서버의 버전이다. DB나 테이블을 만들지 않는다.
SELECT VERSION();
```

명령행 클라이언트로 접속하는 예:

```powershell
# 계정명/포트는 확인된 값으로 바꾼다. 비밀번호는 -p 뒤에 적지 않고 프롬프트에서 입력한다.
mysql --host=localhost --port=3306 --user=계정명 --password
```

접속 후 `SELECT VERSION();`을 실행한다. `mysql`이 PATH에 없으면 Workbench 또는 확인된 설치 경로의 mysql.exe를 사용할 수 있다. `mysql --version`은 클라이언트 실행 파일 버전이므로 실행 중인 서버 버전 확인을 대신하지 않는다.

근거: [MySQL 서버 버전 조회 예제](https://dev.mysql.com/doc/refman/8.4/en/entering-queries.html).

### 4. Connection String 위치와 구조 설계

설정 키는 `ConnectionStrings:DefaultConnection`으로 통일하는 안이다. 승인 후 `appsettings.Development.json`의 기존 Logging을 유지하면서 아래 키를 추가한다. 지금은 파일에 적용하지 않았다.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}
```

빈 문자열은 설정 항목을 알리는 자리이며 접속 가능한 값이 아니다. 실제 연결 문자열은 같은 키로 개발용 User Secrets에 저장한다. 아이디도 Git에 올리지 않도록 전체 문자열을 분리한다.

다음은 실제 값이 아닌 User Secrets 내용 예시다.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=호스트;Port=포트;Database=DB명;User ID=계정명;Password=비밀번호;"
  }
}
```

TLS/인증서 옵션은 서버 요구 사항을 확인한 후 결정한다. 임의로 SSL을 끄거나 인증서 검증을 완화하지 않는다. 비밀번호에 세미콜론·따옴표 등이 있으면 연결 문자열 규칙과 JSON 이스케이프도 고려해야 한다.

현재 사용하는 `WebApplication.CreateBuilder(args)`의 기본 구성에서 일반적인 우선순위는 다음과 같다. 오른쪽이 같은 키를 덮어쓴다.

```text
appsettings.json → appsettings.Development.json
                → User Secrets(Development) → 환경 변수 → 명령행 인수
```

연결 문자열은 하나의 문자열 값이므로 서로 다른 저장소에서 비밀번호 부분만 자동 결합되지 않는다. User Secrets에 완성된 연결 문자열을 두는 방식이 단순하다. 환경 변수 대안의 키는 `ConnectionStrings__DefaultConnection`이다.

근거: [.NET 10 ASP.NET Core 설정 우선순위](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0).

### 5. appsettings.Development.json과 User Secrets의 차이 / Git 노출 방지

| 구분 | appsettings.Development.json | User Secrets |
|---|---|---|
| 목적 | 개발 환경의 공통 설정 | 개발자 PC의 비밀 설정 |
| 위치 | 프로젝트 내부 | 사용자 프로필 아래, 프로젝트 외부 |
| Git | 현재 이미 추적 중 | 비밀 파일은 저장소에 포함하지 않음 |
| 넣을 내용 | 로그 수준, 빈 연결 설정 키 등 | DB 아이디·비밀번호를 포함한 실제 연결 문자열 |
| 적용 | Development 환경에서 로드 | UserSecretsId를 준비하면 기본 빌더가 Development에서 로드 |

승인 후 준비 절차는 프로젝트 폴더에서 `dotnet user-secrets init`을 실행하거나 Visual Studio의 프로젝트 우클릭 → **사용자 비밀 관리**를 사용하는 것이다. 두 방법 모두 csproj에 UserSecretsId를 추가하므로 이번에는 실행하지 않았다.

UserSecretsId는 비밀번호가 아닌 프로젝트별 저장소 식별자로 Git에 포함할 수 있다. 실제 secrets.json은 Windows 사용자 프로필의 `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`에 저장된다. User Secrets는 암호화 저장소가 아니며 개발 전용이다.

실제 값은 이후 사용자가 로컬 편집기에서 입력하도록 한다. 비밀번호를 대화, gpt.md, 커밋 메시지, 터미널 출력에 기록하지 않는다. 비밀번호가 담긴 명령행은 셸 기록에 남을 수 있어 편집기 입력을 우선한다. 환경 변수 대안을 택하더라도 현재 Git 추적 중인 launchSettings.json에 자격 증명을 넣지 않는다.

이미 추적 중인 appsettings.Development.json에 비밀을 적고 .gitignore만 추가하는 방식은 노출 방지책이 되지 않는다. 과거 커밋에 비밀번호가 포함되었다면 파일에서 지우는 것만으로 이력이 사라지지 않으므로 비밀번호 교체와 이력 정리가 별도로 필요하다. 이번 조사에서 실제 비밀번호 노출을 발견했다는 의미는 아니다.

근거: [Microsoft 개발용 비밀 관리](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0).

### 6. AddDbContext<ApplicationDbContext>()의 역할

DI는 필요한 객체를 사용하는 곳에서 직접 만들지 않고 컨테이너에서 전달받는 방식이다.

- `builder.Services`는 앱에서 제공할 서비스의 등록 목록이다.
- `AddDbContext<ApplicationDbContext>(...)`는 Context와 그 설정을 등록한다.
- 기본 수명은 Scoped다. 일반 MVC 요청에서는 요청별 scope 안에서 필요할 때 Context를 만들고, 같은 scope에서는 같은 인스턴스를 사용하며 scope 종료 시 정리한다.
- 설정 콜백에서 Provider와 연결 문자열을 지정한다. 컨테이너는 `DbContextOptions<ApplicationDbContext>`를 기존 Context 생성자에 전달한다.
- `base(options)`를 통해 부모 DbContext가 전달받은 설정을 사용한다. 기존 Entity/Context 구조를 바꿀 필요가 없다.
- Context는 스레드 안전하지 않으므로 동일 인스턴스로 병렬 DB 작업을 실행하지 않는다.

등록 자체는 DB 접속 성공 확인, 테이블 생성, Migration 실행 또는 데이터 저장이 아니다. 현재 HomeController는 Context를 사용하지 않으므로 등록 후 홈 화면이 표시되어도 실제 DB 연결이 검증된 것은 아니다. 후속 승인 단계에서는 schema 변경 없는 별도 연결 확인과 DI 등록을 구분해야 한다.

근거: [Microsoft DbContext 구성과 수명](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/).

### 7. 설치된 MySql.EntityFrameworkCore 10.0.9 등록 방법

설치된 패키지의 `lib/net10.0/MySql.EntityFrameworkCore.xml`에서 다음 API를 직접 확인했다.

```text
Microsoft.EntityFrameworkCore.MySQLDbContextOptionsExtensions
UseMySQL(DbContextOptionsBuilder, string, ...)
```

따라서 현재 버전에서는 `using Microsoft.EntityFrameworkCore;`와 `options.UseMySQL(connectionString)`을 사용한다. 대소문자를 포함해 `UseMySQL`이다. 현재 Provider는 Oracle의 MySql.EntityFrameworkCore이며 Pomelo를 추가하거나 그 Provider용 UseMySql/ServerVersion.AutoDetect 예제를 섞지 않는다.

아래는 **Program.cs에 향후 추가할 등록 구간의 학습용 예시**다. 전체 파일 교체 예제가 아니며 실제 코드에는 적용하지 않았다. 기존 MVC 등록과 요청 파이프라인은 유지한다.

```csharp
using DailyRoutine.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 여러 설정 공급자를 합친 결과에서 ConnectionStrings:DefaultConnection을 읽습니다.
// Development에서는 준비된 User Secrets의 값이 appsettings 값을 덮어씁니다.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 비어 있는 예시 설정으로 실행되지 않도록 초기에 확인합니다.
// 예외 메시지에는 실제 연결 문자열이나 비밀번호를 넣지 않습니다.
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection 설정이 필요합니다.");
}

// Context를 기본 Scoped 수명으로 등록합니다.
// 아래 옵션은 기존 Context의 DbContextOptions 생성자로 전달됩니다.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString)); // Oracle MySQL Provider와 연결 정보를 지정합니다.

// 이 아래의 기존 MVC 등록, builder.Build(), 미들웨어, 라우팅, app.Run()은 유지합니다.
```

DB명은 존재하는 DB인지 먼저 확인해야 한다. 존재하지 않는 DB를 연결 문자열에 적는다고 DB가 생성되지는 않는다. 이 예시에는 EnsureCreated, Migrate, SaveChanges 또는 SQL 실행이 없다. 빈 연결 문자열을 차단하므로 User Secrets를 먼저 준비해야 앱을 시작할 수 있다는 변화도 승인 시 확인한다.

Oracle 가이드에는 예전 버전의 using 문과 DB 생성/저장 예제도 포함되어 있다. 이번에는 Provider 메서드 확인에만 참고했고, 10.0.9 네임스페이스는 설치된 패키지 문서로 확인했다. DB 생성 및 CRUD 예제는 현재 범위에 적용하지 않는다.

근거: [Oracle EF Core Provider 예제](https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core-example.html), 로컬 설치 패키지 XML API 문서 및 nuspec, 현재 csproj/복원 결과.

### 8. 승인 전에 필요한 정보와 다음 범위

| 필요한 정보 | 확인할 내용 |
|---|---|
| 서버 대상 | DailyRoutine에 사용할 Windows 설치 MySQL의 서비스명/설치 경로. 다른 프로젝트 환경인지 구분 |
| 서버 실행 상태 | 실행 중인지. 미실행이어도 이번에는 임의로 시작하지 않음 |
| 서버 버전 | 해당 서버에서 SELECT VERSION() 결과 |
| 호스트/포트 | 실제 접속 대상. localhost/3306은 아직 예시 |
| DB명 및 존재 여부 | 이미 생성된 DailyRoutine용 DB인지. 없으면 아직 없다고 표시 |
| DB 계정 | 사용할 계정명과 해당 DB 접근 가능 여부 |
| 비밀번호 준비 여부 | 값은 보내지 않고, 알고 있으며 로컬 입력 가능한지만 확인 |
| TLS 설정 | 서버가 요구하는 SSL 모드/인증서가 있는지 |

DB가 없다면 **연결 설정 준비**와 **대상 DB에 실제 접속 성공**을 구분하여 범위를 다시 정한다. 사용자 승인 없이 DB·테이블·계정·컨테이너를 생성하거나 다른 프로젝트 DB를 대신 사용하지 않는다.

승인 후의 최소 변경 후보는 appsettings.Development.json의 비밀 없는 키, csproj의 UserSecretsId, Program.cs의 Context/Provider 등록이다. 실제 비밀은 사용자가 로컬에 입력한다. Routine/Context 구조 변경, Migration, 테이블 생성, Controller/Service/View 추가는 포함하지 않는다.

이번 수행 결과: **연결 방법 조사와 gpt.md 기록 완료. 애플리케이션 구현·DB 접속·서비스/컨테이너 조작 없음. 필요한 접속 정보와 다음 구현에 대한 사용자 승인 대기.**

---

## 2026-09-18 — InitialCreate 실제 DB 적용 및 스키마 검증

### 연결 준비에서 적용까지

DailyRoutine 전용 MySQL 8.4.11 컨테이너 dailyroutine-mysql과 볼륨 dailyroutine-mysql-data를 사용한다. 호스트 127.0.0.1:3308은 컨테이너 3306에 연결된다. DB는 dailyroutine, 일반 사용자는 dailyroutine_user다. 기존 study_info Docker 환경과 Windows MariaDB는 수정하지 않았다.

프로젝트에는 UserSecretsId가 있고 실제 연결 문자열은 User Secrets의 ConnectionStrings:DefaultConnection에만 저장되어 있다. 비밀번호는 .env에서 준비했으며 .env는 Git에서 제외된다. 이 문서에는 실제 비밀번호나 연결 문자열을 기록하지 않는다. Program.cs는 GetConnectionString("DefaultConnection"), AddDbContext<ApplicationDbContext>(), options.UseMySQL(connectionString)을 사용한다.

EF Core 및 Design 10.0.12, Oracle MySql.EntityFrameworkCore 10.0.9를 사용한다. 전역 dotnet-ef 6.0.0을 변경하지 않고 프로젝트의 dotnet-tools.json에 로컬 dotnet-ef 10.0.12를 설치했다. InitialCreate 생성 단계에서는 DB가 비어 있었고, 다음 파일만 생성했다.

- Migrations/20260917145307_InitialCreate.cs
- Migrations/20260917145307_InitialCreate.Designer.cs
- Migrations/ApplicationDbContextModelSnapshot.cs

이번에 사용자의 DB 적용 승인을 받아 다음 명령을 실행했다.

```powershell
dotnet ef database update InitialCreate --project DailyRoutine.csproj --startup-project DailyRoutine.csproj --context ApplicationDbContext -- --environment Development
```

User Secrets의 대상 호스트·포트·DB·일반 사용자 및 .env와의 비밀번호 일치를 값 출력 없이 확인했다. 사전 확인 스크립트는 PowerShell의 DbConnectionStringBuilder 프로퍼티 처리 문제를 set_ConnectionString 호출로 수정했다. 최초 샌드박스 실행에서는 Windows TLS 자격 증명 오류가 발생했으나, 승인된 권한으로 동일 연결 설정을 사용해 재실행하니 빌드와 적용이 성공했다. TLS를 끄거나 비밀 설정을 변경하지 않았다.

### 실제 DB 검증 결과

일반 사용자 dailyroutine_user@%로 조회했다. MySQL 버전은 8.4.11이다.

| 항목 | 확인 결과 |
|---|---|
| 테이블 목록 | Routines, __EFMigrationsHistory |
| 스토리지 엔진 | 두 테이블 모두 InnoDB |
| DB 및 테이블 문자셋 | utf8mb4 |
| DB 및 테이블 Collation | utf8mb4_unicode_ci |
| 적용된 MigrationId | 20260917145307_InitialCreate |
| 이력 ProductVersion | 10.0.12 |
| Routines 데이터 수 | 0건 |

| 실제 컬럼명 | MySQL 타입 | NULL 허용 | 제약 |
|---|---|---|---|
| Id | int | 아니요 | PRIMARY KEY, AUTO_INCREMENT |
| Name | longtext | 아니요 | utf8mb4_unicode_ci |
| Description | longtext | 예 | utf8mb4_unicode_ci |
| StartDate | date | 아니요 | 별도 기본값 없음 |
| EndDate | date | 예 | DEFAULT NULL |
| CreatedAtUtc | datetime(6) | 아니요 | DB 기본값 없음 |
| UpdatedAtUtc | datetime(6) | 예 | DEFAULT NULL |

다음은 실행한 생성 SQL이 아니라 SHOW CREATE TABLE Routines가 반환한 실제 정의다.

```sql
CREATE TABLE `Routines` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `Description` longtext COLLATE utf8mb4_unicode_ci,
  `StartDate` date NOT NULL,
  `EndDate` date DEFAULT NULL,
  `CreatedAtUtc` datetime(6) NOT NULL,
  `UpdatedAtUtc` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```
