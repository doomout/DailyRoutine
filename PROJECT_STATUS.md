# DailyRoutine 프로젝트 현황

분석 기준일: 2026-09-17 (Asia/Seoul)  
기준 커밋: `c3ff204` — ApplicationDbContext 정의 완료  
분석 방법: 저장소의 소스·설정·뷰·정적 자산·설계 및 학습 기록 정적 분석

## 1. 현재 상태와 분석 범위

**현재는 ASP.NET Core MVC 기본 화면에 Routine 엔티티와 EF Core DbContext 정의를 추가한 초기 개발 단계다. 실제 루틴을 등록·조회·저장하는 사용자 기능은 아직 없다.**

요청 항목에 Spring Security/JWT가 포함되어 있지만, 이 저장소는 Spring/Java 프로젝트가 아니다. 백엔드는 C#/.NET 10, 프론트엔드는 같은 프로젝트의 Razor View다. 인증 관련 항목은 실제 ASP.NET Core 구현을 기준으로 아래에 정리했다.

- 확인 범위: 솔루션/프로젝트, 전체 애플리케이션 C# 코드, Razor View, 자체 CSS/JavaScript, 설정 파일, 프론트 라이브러리 참조, README.md, DESIGN.md, gpt.md, 테스트·Migration·배포 파일 존재 여부.
- `bin/`, `obj/`, `.vs/`는 빌드·IDE 산출물로 보며 기능 구현의 근거로 사용하지 않았다. 외부 라이브러리 전체에 대한 보안·품질 감사는 수행하지 않았다.
- 이번 작업은 분석 및 본 문서 작성만 수행했다. 기존 코드·설정·문서를 수정하지 않았고 파일 이동, 리팩터링, 기능 추가도 하지 않았다.
- 빌드, 패키지 복원/설치, 서버 실행, 브라우저 조작, 자동화 테스트 실행, DB 접속 및 Migration 적용은 수행하지 않았다.
- 따라서 아래의 **화면 연결 완료**는 소스상 요청부터 View까지 연결되었다는 뜻이다. 현재 환경에서 직접 실행해 사용 가능성을 검증했다는 뜻은 아니다.
- 실제 MySQL 서버의 존재·버전·테이블·데이터는 미확인이다. 코드의 Entity 정의를 실존 DB 테이블로 간주하지 않는다.

상태 구분:

| 상태 | 판단 기준 |
|---|---|
| 완료 | 해당 범위의 코드와 연결이 존재함. 런타임 검증 여부는 별도 표기 |
| 부분 구현 | 일부 클래스·설정·화면만 있고 사용자 흐름이 완성되지 않음 |
| 미구현 | 해당 기능의 실행 코드가 없음. 설계 문서에만 있는 경우도 포함 |
| 미확인 | 외부 환경 또는 실행 검증이 필요하여 소스만으로 확정할 수 없음 |

## 2. 기술 스택과 전체 구조

| 구분 | 현재 확인 내용 | 근거 |
|---|---|---|
| 솔루션 | 웹 프로젝트 1개 | `DailyRoutine.slnx` |
| 백엔드 | ASP.NET Core MVC, C#, `net10.0` | `DailyRoutine.csproj`, `Program.cs` |
| 언어 설정 | nullable, implicit usings 활성화 | `DailyRoutine.csproj` |
| DB 접근 준비 | `Microsoft.EntityFrameworkCore.Design` 10.0.12, `MySql.EntityFrameworkCore` 10.0.9 직접 참조 | `DailyRoutine.csproj` |
| DB 연결 | MySQL 사용 예정. Provider 설정·DI 등록·연결 문자열 없음 | `Program.cs`, `appsettings*.json`, `ApplicationDbContext.cs` |
| 프론트엔드 | 서버 렌더링 Razor, HTML/CSS/JavaScript | `Views/`, `wwwroot/` |
| UI 라이브러리 | Bootstrap 5.3.3, jQuery 3.7.1 | 로컬 배포 파일 헤더 및 `_Layout.cshtml` |
| 입력 검증 라이브러리 | jQuery Validation 1.21.0, Unobtrusive Validation 4.0.0 파일 보유 | `wwwroot/lib/`, `_ValidationScriptsPartial.cshtml` |
| 프론트 빌드 | 별도 React/Vue/SPA 프로젝트나 package.json 없음 | 파일 목록 |
| 개발 실행 주소 | HTTP `http://localhost:5280`, HTTPS `https://localhost:7214` | `Properties/launchSettings.json`; 실제 가동 여부 미확인 |
| 배포 | Dockerfile, Compose, CI 워크플로, 별도 운영 배포 구성 미구현 | 저장소 파일 목록 |

```text
DailyRoutine/
├─ Controllers/
│  └─ HomeController.cs
├─ Data/
│  └─ ApplicationDbContext.cs
├─ Models/
│  ├─ Entities/Routine.cs
│  └─ ErrorViewModel.cs
├─ Views/
│  ├─ Home/Index.cshtml
│  ├─ Home/Privacy.cshtml
│  ├─ Shared/_Layout.cshtml
│  ├─ Shared/_Layout.cshtml.css
│  ├─ Shared/_ValidationScriptsPartial.cshtml
│  ├─ Shared/Error.cshtml
│  ├─ _ViewImports.cshtml
│  └─ _ViewStart.cshtml
├─ wwwroot/
│  ├─ css/site.css
│  ├─ js/site.js
│  ├─ lib/                  # Bootstrap, jQuery, 검증 라이브러리
│  └─ favicon.ico
├─ Properties/launchSettings.json
├─ Program.cs
├─ appsettings.json
├─ appsettings.Development.json
├─ DailyRoutine.csproj
├─ DailyRoutine.slnx
├─ README.md
├─ DESIGN.md
├─ gpt.md
└─ PROJECT_STATUS.md
```

현재 요청 흐름은 `브라우저 → MVC 라우팅 → HomeController → Razor View + 공통 Layout → HTML 응답`이다. `ApplicationDbContext`와 `Routine`은 이 흐름에 연결되지 않았다. 설계상의 `Controller → Service → DbContext → MySQL` 흐름은 아직 구현되지 않았다.

## 3. 현재 구현되어 이용 경로가 있는 기능

| 기능 | 현재 동작 | 구현 상태 / 제한 |
|---|---|---|
| 홈 화면 | Welcome 문구, ASP.NET Core 문서 링크 표시 | 기본 화면 연결 완료. 루틴 대시보드는 아님 |
| 공통 내비게이션 | DailyRoutine 브랜드/Home으로 홈 이동, Privacy로 개인정보 화면 이동 | 링크 연결 완료 |
| 작은 화면 메뉴 | Bootstrap collapse를 사용하는 메뉴 버튼 | 마크업·스크립트 연결됨. 접근성 참조 오류는 11절 참고 |
| 개인정보 화면 | 제목과 기본 템플릿 안내 문구 표시 | 라우팅 완료. 실제 개인정보 처리방침 콘텐츠는 미작성 |
| 오류 화면 | 오류 안내 및 Request ID 표시 | Controller/View 연결 완료. 실제 예외 발생 흐름은 실행 검증 안 함 |
| 공통 화면 자산 | Bootstrap, jQuery, 공통 CSS/JS, favicon 제공 구성 | `MapStaticAssets()`와 Layout 참조 존재 |
| 오류 캐시 방지 | Error 액션에 캐시 비활성화 속성 적용 | `ResponseCache(Duration=0, Location=None, NoStore=true)` |
| 환경별 기본 처리 | 비개발 환경에서 예외 처리 경로와 HSTS, 공통 HTTPS 리다이렉션 | 구성 존재. 인증 기능과는 별개 |

사용자가 자신의 루틴 데이터를 입력·조회·변경할 수 있는 업무 기능은 현재 0개다. 기본 페이지 표시 기능과 루틴 관리 기능의 완료 상태를 구분해야 한다.

## 4. 화면별 기능과 연결된 API/서버 액션

별도의 JSON REST API는 없다. 프론트는 MVC 액션이 반환한 HTML을 이용하며 `fetch`, Ajax 등의 자체 API 호출 코드도 없다.

기본 라우트는 `Program.cs`의 `{controller=Home}/{action=Index}/{id?}`다.

| 화면/자원 | 브라우저 조회 경로 | 서버 연결 | 데이터 및 응답 | 화면 접근 |
|---|---|---|---|---|
| 홈 | `/`, `/Home`, `/Home/Index` | `HomeController.Index()` | `Views/Home/Index.cshtml`, HTML | 브랜드·Home 메뉴 |
| 개인정보 | `/Home/Privacy` | `HomeController.Privacy()` | `Views/Home/Privacy.cshtml`, HTML | 메뉴·푸터 |
| 오류 | `/Home/Error` | `HomeController.Error()` | `ErrorViewModel` → `Views/Shared/Error.cshtml`, HTML | 직접 URL 또는 비개발 환경 예외 처리. 일반 메뉴 없음 |
| 정적 자원 | `/css/site.css`, `/js/site.js`, `/lib/...` 등 | `MapStaticAssets()` | CSS/JS 등 | Layout에서 로드 |

위 경로는 일반적인 GET 조회 경로다. 세 Controller 액션에는 `[HttpGet]`, `[HttpPost]` 등의 HTTP 메서드 제한이 선언되어 있지 않다. 따라서 소스상 GET 전용 API로 명시된 것은 아니다. POST/PUT/DELETE로 업무 데이터를 변경하는 액션은 없다.

공통 라우트에 `{id?}`가 있어도 개별 루틴 상세 API가 생기는 것은 아니다. 현재 Home 액션들은 `id`를 받거나 DB를 조회하지 않는다.

## 5. 기능별 Controller / Service / Repository / Entity / DTO

| 기능/범위 | Controller | Service | Repository / 데이터 접근 | Entity | DTO / ViewModel |
|---|---|---|---|---|---|
| 홈 | `HomeController.Index` | 없음 | 없음 | 없음 | 전용 모델 없음. View에서 제목 설정 |
| 개인정보 | `HomeController.Privacy` | 없음 | 없음 | 없음 | 전용 모델 없음. View에서 제목 설정 |
| 오류 | `HomeController.Error` | 없음 | 없음 | 없음 | `ErrorViewModel` |
| 루틴 데이터 구조 준비 | 없음 | 없음 | `ApplicationDbContext.Routines` 정의만 있음 | `Routine` | 없음 |
| 루틴 CRUD | 없음 | 없음 | 호출 코드 없음 | `Routine`만 준비됨 | 없음 |
| 오늘의 루틴·완료 기록·날짜별 기록 | 없음 | 없음 | 없음 | 없음 | 없음 |
| 반복 주기·통계 | 없음 | 없음 | 없음 | 없음 | 없음 |
| 로그인·회원가입·관리자 | 없음 | 없음 | 없음 | 없음 | 없음 |

실제 애플리케이션 클래스는 Controller 1개, DbContext 1개, Entity 1개, 오류 ViewModel 1개다. 별도의 Service, Repository, DTO 클래스 및 업무용 ViewModel은 없다.

- `ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)`는 설정을 받아 부모 DbContext에 전달한다.
- `DbSet<Routine> Routines => Set<Routine>()`는 엔티티 접근 지점 정의다. 속성 선언만으로 조회·테이블 생성·데이터 저장이 수행되지 않는다.
- `OnModelCreating`, 별도 Entity Configuration, Provider를 지정하는 `OnConfiguring`, DI 등록, 실제 LINQ 조회 및 `SaveChanges/SaveChangesAsync` 호출은 없다.
- `ErrorViewModel.RequestId`는 `string?`, `ShowRequestId`는 RequestId가 비어 있지 않은지 판단하는 읽기 전용 속성이다. DB Entity나 REST API DTO가 아니라 오류 화면 출력 모델이다.
- `HomeController.Error()`는 `Activity.Current?.Id ?? HttpContext.TraceIdentifier`로 요청 식별자를 전달한다.
- 별도 Repository가 없다는 사실 자체는 오류가 아니다. `DESIGN.md`도 초기에는 Service에서 DbContext를 사용하는 구조를 제안한다.

## 6. DB 테이블, 주요 컬럼 및 관계

### 6.1 실제 DB 확인 상태

**저장소에서 확정할 수 있는 것은 Routine 모델과 DbSet 정의뿐이다. 생성된 실제 DB 테이블은 확인하지 않았다.**

- 두 appsettings 파일에 ConnectionStrings가 없다.
- `Program.cs`에 `AddDbContext<ApplicationDbContext>` 및 MySQL Provider 설정이 없다.
- Migration, ModelSnapshot, 스키마 SQL, Seed 코드가 없다.
- 앱 시작 시 `Migrate`, `EnsureCreated`를 호출하는 코드도 없다.
- 외부에서 만든 DB/테이블이나 별도 환경 설정 존재 여부는 이번 범위에서 확인하지 않았다.

### 6.2 현재 모델이 표현하는 테이블 후보

`ApplicationDbContext.Routines` 기준으로 향후 관례 매핑에서 예상되는 테이블명은 `Routines`다. 명시적 테이블명, SQL 자료형, 길이, 인덱스, DB 기본값은 아직 확정된 스키마가 아니다.

| 속성/컬럼 후보 | C# 형식 | null 의도 | 현재 정의와 제약 상태 |
|---|---|---|---|
| `Id` | `int` | 불가 | 관례상 기본 키 후보. 실제 AUTO_INCREMENT DDL은 없음 |
| `Name` | `required string` | 비-null 의도 | 생성 시 지정 요구. 빈 문자열·공백 및 최대 길이 검증 없음 |
| `Description` | `string?` | 허용 | 선택 설명. 최대 길이/SQL 자료형 미지정 |
| `StartDate` | `required DateOnly` | 불가 | 생성 시 지정 요구. 업무상 유효 날짜 검증 없음 |
| `EndDate` | `DateOnly?` | 허용 | 종료 없음은 null로 표현하려는 설계. 시작일 이후인지 검증 없음 |
| `CreatedAtUtc` | `DateTime` | 불가 | 객체 생성 시 `DateTime.UtcNow`. DB 기본값이나 실제 INSERT 시각은 아님 |
| `UpdatedAtUtc` | `DateTime?` | 허용 | 초기 null. 수정 시 자동 갱신하는 코드 없음 |

`required` 및 nullable 선언을 입력값 검증 전체로 보아서는 안 된다. 날짜 범위·공백·길이·UTC 보존 등은 향후 업무 처리와 DB 매핑에서 검증해야 한다. `DateOnly` 및 날짜/시각 컬럼의 실제 MySQL 매핑은 연결 후 검증이 필요하다.

### 6.3 현재 관계와 예정 관계

현재 Entity는 `Routine` 하나이며 외래 키, 탐색 속성, 엔티티 간 관계가 없다. 사용자 소유자를 나타내는 `UserId`도 없다.

아래는 **DESIGN.md에만 있는 미구현 설계**다.

| 예정 Entity | 예정 주요 속성 | 예정 관계/제약 |
|---|---|---|
| `RoutineRecord` | Id, RoutineId, Date, IsCompleted, CompletedAtUtc | Routine 1:N RoutineRecord. `(RoutineId, Date)` 유일 제약 제안 |
| `RoutineSchedule` | Id, RoutineId, RepeatType, EffectiveFrom, EffectiveTo | Routine 1:N RoutineSchedule. 적용 기간 중복 방지 제안 |
| `RoutineScheduleDay` | RoutineScheduleId, DayOfWeek | RoutineSchedule 1:N RoutineScheduleDay. 두 속성의 복합 키 제안 |

실제 FK, 삭제 정책, 인덱스 및 제약으로 구현된 것은 아니다. 기록이 있는 루틴은 종료하여 보존하는 정책도 아직 제안 단계다.

## 7. 사용자 기능 및 인증·권한 처리

| 항목 | 현재 상태 |
|---|---|
| 회원가입 / 로그인 / 로그아웃 | 화면, 액션, 서비스 모두 없음 |
| 사용자 계정 / 프로필 / 비밀번호 변경·재설정 | Entity, 저장소, 처리 코드 없음 |
| 관리자 / 역할 / 권한 관리 | 관리자 화면·컨트롤러·역할 모델·정책 없음 |
| 사용자별 루틴 분리 | UserId, 사용자 조건 조회, 소유권 검증 없음 |
| Spring Security | 사용하지 않음. ASP.NET Core 프로젝트임 |
| JWT | 발급, 검증, 서명 키, issuer/audience, 만료, refresh token 모두 없음 |
| 쿠키 로그인 / ASP.NET Core Identity | 인증 등록 및 관련 업무 코드 없음 |
| 인증 서비스/미들웨어 | `AddAuthentication`, `UseAuthentication` 없음 |
| 권한 미들웨어 | `UseAuthorization()` 호출만 존재 |
| 접근 제한 | `[Authorize]`, 역할 조건, 사용자 정의 권한 정책/전역 보호 정책 없음 |

현재 Home 액션들은 익명 접근 가능한 구조다. `UseAuthorization()`만으로 로그인이나 관리자 보호가 완성되는 것은 아니다. HTTPS 리다이렉션과 HSTS 역시 사용자 인증을 대체하지 않는다.

현재 설계는 개인용 로컬 앱을 우선하며 사용자 Entity는 보류 상태다. 인증/JWT는 요청된 점검 항목으로서 미구현이라고 분류하되, 현재 로컬 MVP의 필수 기능으로 확정된 것으로 해석하지 않는다. 외부 공개나 다중 사용자 지원 단계에서 인증 방식과 데이터 소유권 정책을 결정해야 한다.

상태 변경 폼/API가 없으므로 CSRF 방어가 적용된 업무 흐름도 없다. 향후 변경 요청 구현 시 서버 입력 검증 및 위조 방지 검증을 함께 확인해야 한다.

## 8. 프론트엔드 페이지와 주요 구성요소

| 파일 | 역할 | 연결 상태 |
|---|---|---|
| `Views/_ViewStart.cshtml` | 공통 Layout을 `_Layout`으로 지정 | 기존 페이지 공통 적용 |
| `Views/_ViewImports.cshtml` | 네임스페이스 및 MVC Tag Helpers 선언 | Controller/Action 링크 생성 등에 사용 |
| `Views/Shared/_Layout.cshtml` | HTML 틀, 제목, 메뉴, 본문, 푸터, CSS/JS 참조 | Home/Privacy/Error 공통 틀 |
| `Views/Home/Index.cshtml` | Welcome 및 외부 문서 링크 | HomeController.Index 연결 |
| `Views/Home/Privacy.cshtml` | Privacy Policy 제목, 작성 안내 | HomeController.Privacy 연결. 정책 본문 미작성 |
| `Views/Shared/Error.cshtml` | 오류 안내, Request ID 조건부 표시 | HomeController.Error 연결 |
| `Views/Shared/_ValidationScriptsPartial.cshtml` | jQuery 검증 플러그인 로드 | 기존 페이지에서 호출하지 않음. 입력 폼도 없음 |
| `Views/Shared/_Layout.cshtml.css` | 브랜드/링크/버튼/푸터 등 기본 스타일 | Layout에서 `DailyRoutine.styles.css` 참조 |
| `wwwroot/css/site.css` | 폰트 크기, 포커스, 여백, placeholder 등 기본 스타일 | Layout에서 로드 |
| `wwwroot/js/site.js` | 자체 JS 추가 위치 | 주석만 있으며 업무 로직 없음 |
| `wwwroot/lib/` | Bootstrap, jQuery 및 검증 플러그인 | 로컬 파일로 보유 |

`RenderBody()`로 페이지 본문을 넣고 `RenderSectionAsync("Scripts", required: false)`로 페이지별 스크립트 확장을 허용한다. 현재 페이지에는 업무용 스크립트가 없다.

별도 프론트 라우터, 상태 관리, API 클라이언트, 로그인 상태 표시, 루틴 카드/입력 폼/체크박스/달력/통계 차트 컴포넌트는 없다.

## 9. 미구현/TODO 및 프론트·백엔드 연결 차이

자체 애플리케이션 코드에서 `TODO`, `FIXME`, `NotImplementedException` 표시를 찾지 못했다. 그러나 미구현 범위는 엔티티 주석과 README/DESIGN의 계획에 명확히 남아 있다. TODO 문자열이 없다고 개발 완료 상태인 것은 아니다.

### 9.1 기능별 진행 상태

| 기능 | 백엔드 상태 | 프론트 상태 | 종합 |
|---|---|---|---|
| Routine 모델 | 7개 속성 정의 | 표시/입력 없음 | 모델 정의 완료 |
| EF Core/MySQL 연결 | 패키지와 Context만 존재 | 관련 업무 화면 없음 | 부분 구현 |
| 루틴 목록·등록·수정·삭제/종료 | Entity 외 처리 없음 | 화면 없음 | 미구현 |
| 오늘의 수행 대상 조회 | 날짜 계산·조회 없음 | 화면 없음 | 미구현 |
| 완료·미완료 저장 | 기록 Entity/처리 없음 | 체크 UI 없음 | 미구현 |
| 날짜별 수행 기록 | 기록 조회 없음 | 날짜 선택 화면 없음 | 미구현 |
| 반복 주기·요일 설정 | 일정 Entity/규칙 없음 | 설정 화면 없음 | 미구현 |
| 주간·월간 달성률 | 집계 없음 | 통계 화면 없음 | 미구현 |
| 연속 달성 일수 | 계산 없음 | 표시 없음 | 미구현 |
| 로그인·회원가입·관리자 | 관련 코드 없음 | 관련 화면 없음 | 미구현, 개인용 초기 범위에서는 보류 |
| 개인정보 정책 | 정적 View 반환 | 템플릿 문구만 표시 | 내용 부분 구현 |
| Docker/운영 배포 | 구성 없음 | 해당 없음 | 미구현 |

### 9.2 코드가 있으나 프론트에서 사용할 수 없는 것

- `Routine`과 `ApplicationDbContext.Routines`: 데이터 구조와 접근 지점만 있다. Controller/Service/DB 설정/화면이 없어 사용자가 조회·저장할 수 없다. 완성된 숨은 CRUD API가 있는 것은 아니다.
- `HomeController.Error`: 일반 메뉴는 없지만 직접 URL과 오류 처리에서 사용하므로 연결 누락으로 분류하지 않는다.
- 프론트에서 호출하지 않는 **완성된 업무 API는 없다**.

### 9.3 화면은 있으나 백엔드 처리가 완성되지 않은 것

- 루틴/사용자 업무 화면 자체가 없으므로, 입력 화면만 완성되고 저장 API만 빠진 사례는 없다.
- Home/Privacy/Error는 모두 Controller와 연결되어 있다.
- Privacy의 부족한 부분은 정책 콘텐츠이며, 연결되지 않은 DB 처리나 API 문제가 아니다.
- `_ValidationScriptsPartial`은 재사용 준비 파일일 뿐 입력 화면/서버 검증 기능이 아니다. 현재 사용처가 없다.

## 10. 테스트 구현 및 검증 상태

| 대상 | 현재 자동화 테스트 | 이번 분석에서 확인한 범위 |
|---|---|---|
| HomeController 및 페이지 | 없음 | 소스상 액션/View/내비게이션 연결 |
| ErrorViewModel / 오류 처리 | 없음 | 식별자 전달 및 캐시 속성 정의 |
| Routine 기본값·검증 | 없음 | 속성 선언 및 초기값 |
| DbContext·MySQL 매핑·CRUD | 없음 | Context 정의만 확인 |
| 사용자·권한 | 없음 | 기능 자체 미구현 |
| 프론트/E2E | 없음 | 자체 JavaScript 로직 및 테스트 구성 없음 |
| CI 테스트 | 없음 | 워크플로/테스트 프로젝트 없음 |

솔루션에는 웹 프로젝트 하나만 있고 테스트 프로젝트, 테스트 파일, 테스트 프레임워크 직접 참조가 없다. 테스트 커버리지 수치는 측정하지 않았다.

`gpt.md`의 2026-09-15 후속 작업에는 Context 작성 후 `dotnet build DailyRoutine.csproj --no-restore --verbosity quiet --nologo`가 경고 0개/오류 0개로 성공했다고 기록되어 있다. **이는 과거 기록이며 이번 분석에서 재실행한 결과가 아니다.** 컴파일 성공 기록은 MySQL 접속, 실제 테이블 생성, CRUD 및 브라우저 동작 검증을 의미하지 않는다.

기능 추가 시 필요한 검증 후보:

1. DB 연결 및 Migration 적용 후 실제 MySQL에서 Routine 저장·조회, DateOnly/null/한글 데이터 매핑 확인.
2. 이름 공백/길이, 시작일/종료일 역전, 존재하지 않는 ID 등 입력·경계 조건.
3. 등록→목록→수정→삭제/종료의 화면부터 DB까지 왕복 확인.
4. 수행 기록 구현 후 동일 루틴·날짜 중복 방지와 완료/미완료 재요청의 일관성.
5. 한국 시간의 날짜 경계, 일정 변경 전후 이력 보존, 대상 0건 통계 및 연속 달성 규칙.
6. 인증 도입 시 익명/로그인/권한별 접근과 타 사용자 데이터 접근 차단.

## 11. 명백한 오류 및 개선 필요 사항

소스 검토만으로 현재 컴파일 실패나 실행 중 예외를 확정할 근거는 발견하지 못했다. 아래는 확인된 문제와 구현 공백을 구분한 목록이다.

| 구분 | 발견 내용과 근거 | 영향 / 후속 처리 후보 |
|---|---|---|
| 확인된 접근성 오류 | `_Layout.cshtml:17`의 `aria-controls="navbarSupportedContent"`와 일치하는 id가 메뉴 컨테이너에 없음 | 보조 기술의 제어 대상 참조 불일치. Bootstrap의 class 선택자 기반 collapse 동작과는 별개로 연결 수정 필요 |
| 확인된 문서 오류 | README.md의 프로젝트 구조용 코드 펜스가 닫히지 않음 | 현재 문서의 형식 불완전. 후속 내용 추가 시 코드 블록으로 오인될 수 있음 |
| 문서 최신화 필요 | DESIGN.md의 현재 상태 설명은 패키지/Entity/DbContext가 없다고 서술 | 지금 코드와 불일치. 계획/과거 분석과 현재 상태를 구분해야 함 |
| 문서 최신화 필요 | README 구조에 Data가 빠져 있고 gpt.md 앞부분은 설치 전 상태, 뒤에는 완료 기록이 누적됨 | 진행 상황은 최신 후속 기록과 본 문서를 기준으로 판단해야 함 |
| 주석 최신화 필요 | Routine.cs:5는 이후 DbContext 등록이라고 설명하지만 현재 DbSet 선언은 존재 | 모델 포함과 실제 DB 연결/테이블 생성을 구분하여 주석 갱신 후보 |
| 핵심 구현 공백 | Context DI 등록·Provider·연결 정보·매핑·Migration·호출 코드 없음 | 영속화 불가. 현재 Home 화면은 Context를 사용하지 않아 이것만으로 즉시 고장난 것은 아님 |
| 검증 구현 공백 | Name의 공백/길이 및 날짜 범위 검증 없음 | CRUD 도입 전에 입력 ViewModel/업무 검증 및 DB 제약 정의 필요 |
| 시각 처리 공백 | CreatedAtUtc는 객체 생성 시각, UpdatedAtUtc는 자동 갱신되지 않음 | 저장·수정 시각의 의미와 UTC/사용자 날짜 처리 정책 확정 필요 |
| 사용자 보호 미구현 | 인증·정책·소유자 모델 없음 | 개인용 초기 범위에서는 미구현 상태. 외부 공개/다중 사용자 단계에 접근 보호 설계 필요 |
| 화면 콘텐츠 미완성 | Welcome, Privacy, Error에 기본 템플릿 문구 사용 | 루틴 서비스 화면 및 실제 안내 문구로 교체할 후속 작업 필요 |
| 품질 검증 공백 | 자동화 테스트 및 이번 런타임/DB 검증 없음 | 연결 단계부터 핵심 흐름 및 날짜·중복 경계 검증 필요 |

패키지 버전 숫자가 서로 다르다는 사실만으로 호환성 오류라고 판정하지 않는다. 이번 분석에서는 패키지를 복원하거나 Provider를 구동하지 않았으며 새 호환성·취약점 감사를 수행하지 않았다.

## 12. 다음 개발 후보와 문서 관리 기준

현재 코드와 기존 설계의 진행 순서를 고려하면 **다음 최소 개발 단위는 ApplicationDbContext를 실제 MySQL에 연결하는 작업**이다. 아래는 분석 결과에 따른 후보이며 이번에 구현한 내용이 아니다.

| 순서 | 개발 후보 | 완료 판단 기준 |
|---|---|---|
| 1 | MySQL 연결 설정 및 Context DI 등록 | 실행 환경의 Provider·연결 설정으로 DB 접근이 확인됨 |
| 2 | Routine 매핑과 첫 Migration | 테이블/자료형/키/길이/null 제약을 검토하고 실제 DB 적용 확인 |
| 3 | 루틴 목록·등록 | Controller→Service→DbContext→MySQL→View 흐름으로 등록 후 조회 가능 |
| 4 | 루틴 수정·삭제/종료 | 입력 검증, 없는 ID 처리, 기록 보존 정책을 포함해 화면에서 사용 가능 |
| 5 | RoutineRecord 및 오늘의 루틴·완료 기록 | 날짜별 유일 제약과 완료/미완료 저장, 오늘 대상 계산 가능 |
| 6 | 날짜별 조회·반복 일정 | 과거 규칙 보존과 날짜별 수행 대상 일관성 확인 |
| 7 | 통계·연속 달성 | 분모/기간/오늘/대상 없는 날의 규칙과 화면 검증 완료 |
| 8 | 공개 범위에 맞춘 인증·배포 | 로컬/외부 공개 범위를 결정하고 필요한 접근 보호·설정·백업 확인 |

새 기능 추가 후 이 문서를 갱신할 때에는 다음을 함께 기록한다.

- 기준일과 기준 커밋, 바뀐 기능 상태 및 관련 파일.
- 화면의 진입 경로, 서버 액션/API, Controller/Service/데이터 접근/Entity/ViewModel 연결.
- 실제 생성·변경된 테이블 및 Migration, 아직 적용하지 않은 스키마 구분.
- 검증한 빌드·테스트·브라우저·DB 결과와 확인하지 못한 범위.
- 설계 제안과 실제 구현을 분리하고, 엔티티 정의만으로 사용자 기능을 완료 처리하지 않음.

## 13. 최종 요약

| 구분 | 현재 상태 |
|---|---|
| **완료된 기능** | MVC 기본 라우팅, Home/Privacy/Error의 액션·View 연결, 공통 메뉴·레이아웃·정적 자산 구성, 기본 오류 처리 구성. 개발 기반으로 Routine 7개 속성과 ApplicationDbContext 정의, EF Core/MySQL 패키지 참조 완료. 실행 검증은 이번에 하지 않음 |
| **부분 구현된 기능** | 루틴 영속화 기반(모델/Context/패키지만 존재), 개인정보 화면 콘텐츠(템플릿만 존재). 사용자 관점의 루틴 CRUD는 미구현 |
| **미구현 기능** | MySQL 런타임 연결·명시적 매핑·Migration, 루틴 CRUD, 오늘 목록, 완료/미완료 기록, 날짜별 이력, 반복 일정, 통계·연속 달성, 로그인·회원가입·관리자·JWT, 자동화 테스트, Docker/운영 배포. 계정 기능은 초기 개인용 범위에서 보류 |
| **다음 개발 후보** | MySQL 연결/DI → Routine 매핑·첫 Migration → 목록·등록 → 수정·삭제/종료 → 수행 기록·오늘 화면 → 반복 일정·통계. 각 단계에서 필요한 검증 추가 |
