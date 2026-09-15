# 분석 요약

현재 프로젝트는 **.NET 10 기반 ASP.NET Core MVC 기본 템플릿 상태**입니다. 루틴 관련 기능, EF Core 설정, MySQL 연결은 아직 없습니다.

이 프로젝트에는 **하나의 MVC 프로젝트 안에서 Controller → Service → DbContext로 역할을 나누는 구조**가 적합합니다. 처음에는 루틴 등록부터 작게 구현하고, 수행 기록과 반복 주기, 통계 순으로 확장하는 것을 권장합니다.

**파일 생성·수정, 패키지 설치, 빌드, DB 변경은 수행하지 않았습니다.** 아래는 현재 파일을 확인한 결과와 설계 제안입니다.

---

## 1. 현재 ASP.NET Core MVC 프로젝트 구조

```text
DailyRoutine/
├─ DailyRoutine.slnx
├─ DailyRoutine.csproj
├─ Program.cs
├─ appsettings.json
├─ appsettings.Development.json
├─ Controllers/
│  └─ HomeController.cs
├─ Models/
│  └─ ErrorViewModel.cs
├─ Views/
│  ├─ Home/
│  │  ├─ Index.cshtml
│  │  └─ Privacy.cshtml
│  ├─ Shared/
│  │  ├─ _Layout.cshtml
│  │  ├─ _Layout.cshtml.css
│  │  ├─ _ValidationScriptsPartial.cshtml
│  │  └─ Error.cshtml
│  ├─ _ViewImports.cshtml
│  └─ _ViewStart.cshtml
├─ wwwroot/
│  ├─ css/
│  ├─ js/
│  ├─ lib/
│  └─ favicon.ico
├─ Properties/
│  └─ launchSettings.json
├─ bin/
└─ obj/
```

### 주요 파일의 역할

| 파일·폴더 | 현재 역할 | Spring Boot와 비교 |
|---|---|---|
| `DailyRoutine.csproj` | 대상 프레임워크와 패키지 참조 관리 | `build.gradle` / `pom.xml` |
| `Program.cs` | 앱 시작, DI 등록, HTTP 처리 순서, 라우팅 설정 | 시작 클래스 + 각종 설정 클래스 |
| `Controllers/` | 요청을 받아 View 반환 | Spring MVC의 `@Controller` |
| `Models/` | 현재는 오류 화면용 데이터만 존재 | 현재 파일은 Entity보다 화면 DTO에 해당 |
| `Views/` | Razor를 이용한 서버 측 HTML 생성 | Thymeleaf의 `templates/` |
| `wwwroot/` | CSS, JavaScript 등 공개 정적 파일 | `static/` |
| `appsettings*.json` | 기본 및 환경별 설정 | `application.yml`, `application-dev.yml` |
| `launchSettings.json` | 개발 실행 주소와 환경 설정 | IDE 실행 설정에 가까움 |
| `bin/`, `obj/` | 빌드 결과와 중간 생성물 | `build/`, `target/` |

확인한 주요 소스는 [프로젝트 설정](D:/khg_EX/DailyRoutine/DailyRoutine.csproj), [Program.cs](D:/khg_EX/DailyRoutine/Program.cs), [HomeController.cs](D:/khg_EX/DailyRoutine/Controllers/HomeController.cs)입니다.

### 현재 동작

`Program.cs`에서 다음을 설정하고 있습니다.

- `AddControllersWithViews()`로 MVC 서비스 등록
- 개발 환경이 아닐 때 오류 처리와 HSTS 적용
- HTTPS 리다이렉션
- 라우팅과 권한 검사 미들웨어
- 정적 파일 제공
- 기본 경로 `{controller=Home}/{action=Index}/{id?}`

따라서 `/` 요청은 기본적으로 `HomeController.Index()`로 전달되고, `Views/Home/Index.cshtml`을 렌더링합니다.

```text
브라우저 요청
    → 라우팅
    → HomeController.Index()
    → Views/Home/Index.cshtml + 공통 Layout
    → HTML 응답
```

Bootstrap과 jQuery를 참조하는 기본 레이아웃도 준비되어 있습니다.

다만 **`UseAuthorization()`이 있다는 것만으로 로그인 기능이 구현된 것은 아닙니다.** 현재 인증 설정이나 사용자 Entity는 없습니다.

또한 `.csproj`에는 별도 패키지 참조가 없으며, `DbContext`, Entity, Migration, DB 연결 문자열도 없습니다. 로컬 MySQL의 설치 버전과 실제 접속 상태는 이번 소스 분석으로 확인하지 않았습니다.

---

## 2. 이 프로젝트에 적합한 폴더·레이어 구조

학습 초기에는 프로젝트를 여러 개로 분리하기보다, **현재 프로젝트 안에서 폴더로 책임을 구분**하는 편이 이해하기 쉽습니다.

다음은 기능이 확장되었을 때의 제안 구조입니다. 지금 전부 생성한다는 의미는 아닙니다.

```text
DailyRoutine/
├─ Controllers/
│  ├─ HomeController.cs
│  ├─ RoutinesController.cs
│  ├─ TodayController.cs
│  ├─ HistoryController.cs
│  └─ StatisticsController.cs
├─ Models/
│  └─ Entities/
│     ├─ Routine.cs
│     ├─ RoutineRecord.cs
│     ├─ RoutineSchedule.cs          # 반복 주기 단계
│     └─ RoutineScheduleDay.cs       # 요일 지정 단계
├─ ViewModels/
│  ├─ Routines/
│  ├─ Today/
│  ├─ History/
│  └─ Statistics/
├─ Services/
│  ├─ RoutineService.cs
│  ├─ RoutineRecordService.cs
│  ├─ RoutineScheduleService.cs
│  └─ StatisticsService.cs
├─ Data/
│  ├─ ApplicationDbContext.cs
│  └─ Configurations/
├─ Migrations/
├─ Views/
│  ├─ Routines/
│  ├─ Today/
│  ├─ History/
│  ├─ Statistics/
│  └─ Shared/
├─ wwwroot/
└─ Program.cs
```

### 각 레이어의 책임

```text
Controller → Service → DbContext → MySQL
                 ↓
             조회·처리 결과
                 ↓
Controller → ViewModel → Razor View
```

- **Controller**: 요청 입력, 입력 검증 결과 확인, Service 호출, View 또는 리다이렉트 반환
- **Service**: 루틴 시작일 판단, 완료 처리, 반복 일정 계산 등 업무 규칙
- **DbContext**: Entity 조회·변경 추적·저장, DB 연결
- **Entity**: DB에 저장할 데이터와 관계
- **ViewModel**: 특정 화면의 입력 또는 출력에 필요한 데이터
- **Razor View**: 화면 표현

예를 들어 루틴 등록 화면은 사용자가 수정해도 되는 이름과 설명만 입력받으면 됩니다. DB의 삭제 상태나 생성 시각까지 입력받을 이유가 없으므로, Entity와 등록용 ViewModel을 분리합니다.

### Repository는 필요한가?

**초기에는 별도 Repository 없이 Service에서 DbContext를 사용하는 구조를 권장합니다.**

Spring Data JPA와 완전히 일대일로 대응하지는 않습니다.

| Spring/JPA | ASP.NET Core/EF Core |
|---|---|
| JPA Entity | EF Core Entity |
| `EntityManager`와 영속성 컨텍스트 | `DbContext`의 변경 추적·저장 기능과 유사 |
| `JpaRepository`의 조회·저장 역할 | `DbSet<T>`와 `DbContext`가 상당 부분 제공 |
| JPQL, Criteria 등의 쿼리 | LINQ 쿼리 |
| `@Service`와 생성자 주입 | Service 클래스를 DI에 등록하고 생성자 주입 |
| Thymeleaf 화면 DTO | Razor ViewModel |

`DbSet<Routine>`이 `JpaRepository`처럼 사용자 정의 인터페이스의 구현체를 자동 생성하는 것은 아닙니다.

우선 EF Core 자체를 익히고, 조회 코드의 중복이나 복잡성이 실제로 생길 때 Repository 도입을 검토하면 됩니다. Service 인터페이스도 교체나 테스트 대역이 필요한 시점에 추가할 수 있습니다.

---

## 3. 필요한 Entity와 각 역할

### 3-1. `Routine` — 반복해서 수행할 일

“독서”, “아침약”처럼 **무엇을 해야 하는지**를 저장합니다.

| 속성 후보 | 역할 |
|---|---|
| `Id` | 루틴 식별자 |
| `Name` | 루틴 이름 |
| `Description` | 선택적 설명 |
| `StartDate` | 수행을 시작하는 날짜 |
| `EndDate` | 마지막 수행 대상 날짜, 계속한다면 null |
| `CreatedAtUtc` | 생성 시각 |
| `UpdatedAtUtc` | 마지막 수정 시각 |

초기에는 하루에 한 번 수행하는 루틴을 기준으로 합니다. 제시한 “아침약”과 “저녁약”은 각각 별도 루틴으로 표현할 수 있습니다.

**오늘의 완료 여부를 Routine 자체에 저장하면 안 됩니다.** 날짜가 바뀔 때 이전 상태가 사라지므로, 날짜별 기록이 별도로 필요합니다.

### 3-2. `RoutineRecord` — 특정 날짜의 수행 상태

“9월 15일의 독서를 완료했다”를 저장합니다.

| 속성 후보 | 역할 |
|---|---|
| `Id` | 기록 식별자 |
| `RoutineId` | 어떤 루틴의 기록인지 나타내는 외래 키 |
| `Date` | 수행 대상 날짜 |
| `IsCompleted` | 완료 여부 |
| `CompletedAtUtc` | 실제 완료 처리 시각, 미완료라면 null |

중요한 제약은 다음과 같습니다.

> **`RoutineId + Date` 조합은 유일해야 합니다.**

같은 날짜의 독서 기록이 여러 개 생기지 않도록 DB에서도 보장해야 합니다.

초기에는 다음 방식이 단순합니다.

- 수행 대상 목록은 루틴과 일정으로 계산
- 처음 상태를 저장할 때 기록 생성
- 이후 같은 기록을 수정
- 수행 대상인데 기록이 없으면 미완료로 해석

따라서 앱을 열지 않은 날도 미완료를 계산할 수 있고, 매일 자정에 기록을 생성하는 작업이 필수는 아닙니다.

### 3-3. `RoutineSchedule` — 반복 규칙과 적용 기간

반복 주기 기능을 구현할 때 추가합니다.

| 속성 후보 | 역할 |
|---|---|
| `Id` | 일정 식별자 |
| `RoutineId` | 대상 루틴 |
| `RepeatType` | 매일, 지정 요일 등 |
| `EffectiveFrom` | 규칙 적용 시작일 |
| `EffectiveTo` | 규칙 적용 종료일, 종료 전이면 null |

**규칙이 바뀌면 기존 규칙을 덮어쓰기보다 새 적용 기간을 추가하는 방식을 권장합니다.**

예를 들어 독서를 “매일”에서 “월·수·금”으로 바꿔도 지난달 수행 대상은 여전히 매일이어야 합니다. 현재 규칙만 보관하면 지난달 달성률까지 달라질 수 있습니다.

초기에는 매일 반복만 지원하고, 반복 주기 단계에서 기존 시작일부터의 “매일” 일정을 보존하도록 확장하면 됩니다.

### 3-4. `RoutineScheduleDay` — 지정 요일

월·수·금처럼 여러 요일을 선택할 때 사용합니다.

| 속성 후보 | 역할 |
|---|---|
| `RoutineScheduleId` | 대상 반복 규칙 |
| `DayOfWeek` | 선택한 요일 |

`RoutineScheduleId + DayOfWeek`를 복합 키로 두면 같은 요일의 중복 저장을 막을 수 있습니다. 매일 반복 규칙에는 요일 행이 필요하지 않습니다.

요일 문자열을 `"월,수,금"`으로 저장하는 것보다 조회와 검증이 명확하며, Entity 관계 학습에도 적합합니다.

### 초기에는 별도 Entity가 필요 없는 것

- **주간·월간 달성률**: 일정과 수행 기록을 조회해 계산
- **연속 달성 일수**: 수행 대상 날짜와 기록으로 계산
- **통계 화면 결과**: ViewModel로 구성
- **사용자**: 현재 개인용 범위에서는 보류

---

## 4. Entity 간 관계

```text
Routine 1 ───── N RoutineRecord

Routine 1 ───── N RoutineSchedule
RoutineSchedule 1 ───── N RoutineScheduleDay
```

예를 들면 다음과 같습니다.

```text
독서
├─ 수행 기록
│  ├─ 9월 15일: 완료
│  └─ 9월 16일: 미완료
└─ 반복 규칙
   ├─ 9월 1일 ~ 9월 30일: 매일
   └─ 10월 1일부터: 월·수·금
```

EF Core에서는 외래 키와 함께 `Routine.Records` 같은 **탐색 속성**으로 관계를 표현합니다. JPA의 `@OneToMany`, `@ManyToOne`과 유사한 관계를 EF Core 설정으로 지정한다고 이해하면 됩니다.

### 함께 정해야 할 데이터 규칙

- 같은 루틴의 반복 규칙 적용 기간은 겹치지 않아야 합니다.
- 수행 대상이 아닌 날짜에는 완료 기록을 만들지 않도록 검증합니다.
- 루틴 삭제로 과거 수행 기록이 함께 사라지지 않도록 해야 합니다.

삭제는 **기록이 없는 루틴은 실제 삭제하고, 기록이 있는 루틴은 종료 처리하여 보존**하는 방식을 제안합니다. 이는 구현 전 확정할 업무 정책입니다.

초기에는 과거 화면에 현재 루틴 이름을 표시해도 충분합니다. 나중에 “당시 이름까지 보존”할 필요가 생기면 별도 이력 설계를 추가하면 됩니다.

---

## 5. 필요한 주요 화면

| 화면 | 주요 내용 | 구현 시점 |
|---|---|---|
| 루틴 목록 | 등록된 루틴, 수정·삭제 이동 | 초기 |
| 루틴 등록 | 이름, 설명, 시작일 입력 | 초기 |
| 루틴 수정 | 기본 정보 변경 | 초기 |
| 루틴 삭제 확인 | 대상과 기록 보존 여부 안내 | 초기 |
| 오늘의 루틴 | 오늘 수행 대상, 완료·미완료 체크 | 기록 기능 단계 |
| 날짜별 기록 | 날짜 선택, 해당 날짜의 대상과 결과 | 기록 기능 다음 |
| 반복 주기 설정 | 매일·요일 선택, 적용 시작일 | 반복 기능 단계 |
| 통계 | 주간·월간 달성률, 루틴별 결과, 연속 달성 | 마지막 기능 단계 |

오늘 화면은 다음 정도로 시작할 수 있습니다.

```text
오늘의 루틴 — 2026년 9월 15일
완료 2 / 전체 6

[✓] 아침약
[ ] 저녁약
[ ] 탈모약 바르기
[✓] 헬스장
[ ] 독서
[ ] 공부
```

초기에는 Razor와 Bootstrap, 일반 폼 제출로 충분합니다. 화면 새로고침 없이 체크하는 처리는 기본 동작을 이해한 다음 개선할 수 있습니다.

---

## 6. 필요한 Controller와 Service

### Controller

| Controller | 책임 |
|---|---|
| `HomeController` | 초기 홈, 공통 오류 처리 |
| `RoutinesController` | 루틴 목록·등록·수정·삭제 |
| `TodayController` | 오늘의 목록, 완료 상태 변경 |
| `HistoryController` | 날짜별 기록 조회 |
| `StatisticsController` | 기간별 통계 조회 |

반복 주기 화면은 처음에는 `RoutinesController`에 포함해도 됩니다.

Spring MVC와 마찬가지로 조회는 GET, 데이터 변경은 POST로 구분합니다. 저장 후에는 리다이렉트하는 패턴을 사용하면 새로고침으로 폼을 다시 제출하는 문제를 줄일 수 있습니다.

입력 검증에는 Data Annotations와 `ModelState`를 사용합니다. Spring의 Bean Validation과 `BindingResult`를 떠올리면 이해하기 쉽습니다.

### Service

| Service | 주요 책임 |
|---|---|
| `RoutineService` | 루틴 조회·등록·수정·삭제 또는 종료 |
| `RoutineRecordService` | 날짜별 상태 조회, 완료·미완료 저장 |
| `RoutineScheduleService` | 특정 날짜의 수행 대상 판단, 반복 규칙 관리 |
| `StatisticsService` | 달성률과 연속 달성 계산 |

오늘 목록, 과거 기록, 통계가 **같은 수행 대상 판단 로직을 공유**해야 합니다. 그렇지 않으면 오늘 화면의 전체 개수와 통계의 전체 개수가 달라질 수 있습니다.

완료 요청도 “현재 상태를 반대로 바꾸기”보다 **완료인지 미완료인지 목표 상태를 전달**하는 방식이 좋습니다. 같은 요청이 두 번 들어와도 의도한 상태를 유지하기 쉽습니다.

---

## 7. 전체 구현 순서

**각 단계마다 코드 설명 → 작은 구현 → 동작 확인 → 다음 단계 승인** 순으로 진행하는 것이 학습 목적에 맞습니다.

| 단계 | 구현 범위 | 학습 포인트 |
|---|---|---|
| 1 | `Routine` Entity 정의 | C# 속성, nullable, Entity와 ViewModel 차이 |
| 2 | EF Core·MySQL 패키지 선정 및 연결 | 공급자, 설정, DI, `DbContext` |
| 3 | Routine 매핑과 첫 Migration | 테이블 생성, 키, 길이 제한, 스키마 변경 |
| 4 | 루틴 목록·등록 | Controller → Service → DB → View 흐름 |
| 5 | 수정·삭제 또는 종료 | 입력 검증, 변경 추적, 기록 보존 정책 |
| 6 | `RoutineRecord`와 Migration | 외래 키, 1:N, 복합 유일 제약 |
| 7 | 오늘 목록과 완료 체크 | 날짜 처리, 상태 저장 |
| 8 | 날짜별 기록 | 날짜 조건 조회, 기록 없는 날짜 해석 |
| 9 | 반복 주기와 규칙 이력 | 수행 대상 계산, 과거 규칙 보존 |
| 10 | 주간·월간 달성률 | 집계와 통계 기준 |
| 11 | 연속 달성과 통계 화면 | 경계 조건, Bootstrap 표현 |
| 12 | Docker MySQL 이전 | 설정 변경, 데이터 이전, 연결 확인 |
| 13 | 웹 배포 | 인증, 운영 설정, HTTPS, 백업 |

통계 구현 전에 다음 기준을 확정하는 것이 좋습니다.

- 달성률은 **완료 건수 ÷ 수행 대상 건수 × 100**
- 수행 대상이 0건이면 “대상 없음”으로 표시
- 진행 중인 주·월은 기본적으로 오늘까지 집계하고 범위를 명시
- 월·수·금 루틴에서 화요일은 연속 달성을 끊지 않음
- 아직 끝나지 않은 오늘의 미완료는 전날까지의 연속 기록을 즉시 끊지 않도록 처리

이 기준들은 구현 시 경계 사례를 확인할 핵심 항목입니다.

---

## 8. 로컬 MySQL → Docker MySQL → 웹 서비스 이전 설계

### 8-1. 연결 정보를 코드에서 분리

DB 주소·포트·계정은 Controller나 Service에 넣지 않고 설정으로 주입합니다.

- 개발 환경: User Secrets 등에 비밀번호 보관
- 운영 환경: 환경 변수 또는 배포 환경의 비밀 설정 사용
- 공통 설정: `appsettings.json`
- 연결 문자열 키 예: `ConnectionStrings:DefaultConnection`

ASP.NET Core는 환경 변수로 JSON 설정을 덮어쓸 수 있습니다. 환경 변수 이름은 `ConnectionStrings__DefaultConnection`처럼 표현합니다. [공식 설정 문서](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0)

Spring Boot의 외부 설정과 같은 방향입니다.

### 8-2. 앱 실행 위치에 따른 DB 주소 구분

| 앱 위치 | MySQL 위치 | 앱에서 사용하는 주소 예 |
|---|---|---|
| Windows | Windows 로컬 | `localhost:3306` |
| Windows | Docker | `localhost:공개한포트` |
| Docker | 같은 Docker 네트워크 | `mysql:3306` 같은 서비스 이름 |
| 웹 서버 | 별도 DB 서버 | DB 서버의 내부 주소 |

앱도 컨테이너로 옮기면 `localhost`는 앱 컨테이너 자신을 가리킵니다. 이 차이를 설정으로 처리하면 업무 코드를 바꿀 필요가 없습니다.

### 8-3. .NET·EF Core·MySQL 공급자 호환성 확인

현재 프로젝트 대상은 `net10.0`입니다. 그렇다고 EF Core와 MySQL 공급자를 버전 확인 없이 설치할 수 있는 것은 아닙니다.

Microsoft도 EF Core 공급자가 일반적으로 다른 메이저 버전 사이에서 호환되지 않는다고 안내합니다. **설치 단계에서 실제 공급자의 지원표와 패키지 의존성을 기준으로 조합을 확정해야 합니다.** [EF Core 공급자 문서](https://learn.microsoft.com/en-us/ef/core/providers/), [MySQL EF Core 지원 문서](https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html)

이번에는 특정 패키지 버전을 확정하거나 변경하지 않았습니다.

### 8-4. 스키마 변경과 데이터 이전 구분

- **Migration**: 테이블과 컬럼 등 스키마 변경 관리
- **백업·복원**: 실제 루틴과 수행 기록 이전

로컬 MySQL을 Docker로 옮길 때는 기존 데이터를 백업·복원하고, Migration 적용 상태를 확인해야 합니다. Migration만 실행한다고 기존 데이터까지 복사되지는 않습니다.

운영에서는 DB 변경 내용을 확인한 뒤 배포 절차에서 적용하는 방식이 적합합니다.

### 8-5. 날짜와 시각을 분리

이 앱에서 특히 중요한 부분입니다.

- 수행 대상 날짜: `DateOnly` → MySQL `DATE`
- 생성·완료 시각: UTC 기준 저장
- “오늘” 판단: 설정한 사용자 시간대 기준, 초기에는 한국 시간
- 시간 판단을 한곳에 모아 서버의 로컬 시간에 직접 의존하지 않도록 구성

한국 PC에서 실행하던 앱을 UTC 서버로 옮겨도 같은 “오늘”을 보여줘야 합니다. 선택한 공급자의 날짜 형식 매핑도 연결 단계에서 확인합니다.

### 8-6. DB 환경 차이 줄이기

로컬과 Docker의 MySQL 버전, 문자셋, 정렬 규칙을 맞추는 것이 좋습니다.

- 한글과 이모지를 고려한 `utf8mb4`
- 테이블 이름의 대소문자 표기 일관성
- Docker 데이터 볼륨과 별도 백업
- 앱 전용 DB 계정
- 컨테이너 재시작 후 연결과 데이터 보존 확인

### 8-7. 웹 공개 전 개인 데이터 접근 보호

현재는 개인용 로컬 학습 범위로 진행하되, 인터넷에서 접근할 수 있게 배포하기 전에는 로그인 또는 접근 제한이 필요합니다.

그 단계에서 인증, HTTPS, 운영 오류 처리, 로그와 백업을 함께 점검하면 됩니다. 현재의 `launchSettings.json`은 개발 실행 설정이므로 운영 배포 설정은 별도로 구성합니다.

---

## 권장하는 첫 단계

승인 후 첫 구현 범위는 **`Routine` Entity 하나를 정의하고, 각 속성의 의미를 한글 주석과 함께 이해하는 단계**가 적절합니다. DB 연결과 화면 작성은 그다음 단계로 나누겠습니다.

**현재는 분석만 완료했으며, 요청하신 대로 승인을 기다리겠습니다.**
