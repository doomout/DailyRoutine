# DailyRoutine

매일 반복하는 루틴을 기록하고 관리하기 위한 개인용 웹 애플리케이션입니다.

아침/저녁 루틴, 운동, 독서, 공부 등 반복적으로 수행하는 일을 기록하고,
날짜별 수행 여부와 통계를 확인할 수 있는 서비스를 목표로 개발하고 있습니다.

현재는 개인 PC에서 사용하는 로컬 웹 애플리케이션으로 개발하며,
향후 Docker 및 웹 서버 환경으로 확장할 예정입니다.

---

## 프로젝트 목적

이 프로젝트는 단순히 애플리케이션을 완성하는 것뿐만 아니라
C#과 ASP.NET Core를 다시 학습하는 것을 목적으로 합니다.

AI 도구인 Codex를 개발 보조 도구로 활용하되,
기능을 한 번에 생성하지 않고 다음 과정을 반복하며 단계적으로 개발합니다.

1. 기능 및 구조 설계
2. 코드 구현
3. 코드와 동작 원리 이해
4. 빌드 및 테스트
5. Git Commit
6. 다음 기능 구현

개발 과정에서 조사하고 학습한 내용은 `gpt.md`에 기록합니다.

---

## 기술 스택

### Backend
- C#
- .NET 10
- ASP.NET Core MVC
- Entity Framework Core 10

### Database
- MySQL
- MySql.EntityFrameworkCore

현재는 Windows에 설치된 로컬 MySQL을 사용할 예정이며,
향후 Docker MySQL로 이전할 계획입니다.

### Frontend
- Razor View
- HTML / CSS
- Bootstrap

### Development
- Visual Studio 2026
- OpenAI Codex CLI
- Git / GitHub

---

## 주요 기능

향후 다음 기능을 구현할 예정입니다.

- 루틴 등록 / 수정 / 삭제
- 오늘의 루틴 조회
- 완료 / 미완료 기록
- 날짜별 수행 기록
- 반복 주기 설정
- 주간 / 월간 달성률
- 연속 달성 일수
- 통계 화면

---

## 데이터 모델

### Routine

반복적으로 수행할 루틴의 기본 정보를 관리합니다.

예:

- 아침 루틴
- 저녁 루틴
- 운동
- 독서
- 공부

현재 구현된 주요 속성:

| 속성 | 설명 |
|---|---|
| Id | 루틴 식별자 |
| Name | 루틴 이름 |
| Description | 루틴 설명 |
| StartDate | 루틴 시작일 |
| EndDate | 루틴 종료일 |
| CreatedAtUtc | 생성 시각 |
| UpdatedAtUtc | 수정 시각 |

날짜별 완료 여부는 `Routine` 자체에 저장하지 않고,
향후 별도의 수행 기록 Entity로 분리할 예정입니다.

---

## 프로젝트 구조

현재 주요 구조는 다음과 같습니다.

```text
DailyRoutine/
├─ Controllers/
├─ Models/
│  └─ Entities/
│     └─ Routine.cs
├─ Views/
├─ wwwroot/
├─ Program.cs
├─ appsettings.json
├─ DailyRoutine.csproj
├─ gpt.md
└─ README.md