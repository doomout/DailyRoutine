using DailyRoutine.Data;
using DailyRoutine.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyRoutine.Services
{
    /// <summary>
    /// Routine Entity와 관련된 비즈니스 로직 및
    /// DB 접근을 담당하는 Service 클래스.
    ///
    /// Controller가 DbContext를 직접 사용하지 않고
    /// 이 Service를 통해 Routine 데이터를 조회/저장하도록 구성한다.
    /// </summary>
    public class RoutineService
    {
        /// <summary>
        /// EF Core에서 DB 접근을 담당하는 DbContext.
        ///
        /// RoutineService에서 직접 new로 생성하지 않고
        /// ASP.NET Core의 DI(Dependency Injection)를 통해 전달받는다.
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// 생성자 주입(Constructor Injection).
        ///
        /// ASP.NET Core DI Container가 RoutineService를 생성할 때
        /// 등록되어 있는 ApplicationDbContext를 찾아 전달한다.
        /// </summary>
        public RoutineService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// DB의 Routines 테이블에 저장된 모든 Routine을 조회한다.
        ///
        /// _context.Routines
        ///     → ApplicationDbContext의 DbSet&lt;Routine&gt;
        ///
        /// ToListAsync()
        ///     → EF Core가 DB 조회를 수행하고
        ///       결과를 List&lt;Routine&gt;으로 반환한다.
        /// </summary>
        public async Task<List<Routine>> GetAllAsync()
        {
            return await _context.Routines.ToListAsync();
        }

        /// <summary>
        /// 새로운 Routine을 DB에 저장한다.
        ///
        /// Add()
        ///     → 바로 INSERT하는 것이 아니라
        ///       EF Core가 새 Entity로 추적하도록 등록한다.
        ///
        /// SaveChangesAsync()
        ///     → EF Core가 추적 중인 변경사항을 확인하여
        ///       실제 DB에 INSERT 등의 SQL을 실행한다.
        /// </summary>
        public async Task AddAsync(Routine routine)
        {
            _context.Routines.Add(routine);

            // 실제 DB에 변경사항을 반영한다.
            await _context.SaveChangesAsync();
        }
    }
}