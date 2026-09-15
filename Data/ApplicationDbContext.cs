using DailyRoutine.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyRoutine.Data
{
    /// <summary>
    /// DailyRoutine에서 EF Core의 조회, 엔티티 변경 추적, 저장을 담당하는 작업 단위입니다.
    /// JPA의 EntityManager처럼 관리 중인 엔티티의 상태를 추적합니다.
    /// 이 클래스를 정의하는 것만으로 DB 연결이나 테이블 생성이 이루어지지는 않습니다.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// 이 Context에 사용할 설정을 외부에서 생성자를 통해 전달받습니다.
        /// options에는 이후 사용할 DB Provider와 연결 정보 등의 설정이 담깁니다.
        /// 제네릭 형식은 이 설정이 ApplicationDbContext용임을 나타냅니다.
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            // 전달받은 설정은 부모 DbContext가 사용하도록 base(options)로 넘깁니다.
            // 실제 의존성 주입 등록과 MySQL 설정은 이후 단계에서 진행합니다.
        }

        /// <summary>
        /// Routine을 EF Core 모델에 포함하고 루틴 조회 및 추가·삭제의 진입점을 제공합니다.
        /// Set<Routine>()은 현재 Context가 관리하는 DbSet을 반환하며,
        /// 이 속성을 읽는 것만으로 DB의 모든 루틴을 조회하지는 않습니다.
        /// 추가·삭제 등록이나 추적 중인 객체의 변경은 SaveChangesAsync() 등으로 저장합니다.
        /// Spring Data JPA Repository의 기본 기능 일부와 비슷하지만,
        /// 메서드 이름으로 쿼리를 자동 생성하는 Repository 인터페이스는 아닙니다.
        /// </summary>
        public DbSet<Routine> Routines => Set<Routine>();
    }
}
