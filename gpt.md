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
