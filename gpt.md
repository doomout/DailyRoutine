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
