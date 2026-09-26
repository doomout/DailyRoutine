using DailyRoutine.Services;
using Microsoft.AspNetCore.Mvc;

namespace DailyRoutine.Controllers
{
    public class RoutinesController : Controller
    {
        // DB 조회는 주입받은 Service에 맡기고 Controller는 요청과 응답을 처리한다.
        private readonly RoutineService _routineService;

        // MVC가 Controller를 생성할 때 DI 컨테이너에서 RoutineService를 받아 전달한다.
        public RoutinesController(RoutineService routineService)
        {
            _routineService = routineService;
        }

        public async Task<IActionResult> Index()
        {
            // 조회가 끝날 때까지 스레드를 차단하지 않고 기다린 뒤 전체 목록을 받는다.
            var routines = await _routineService.GetAllAsync();

            // 조회한 List<Routine>을 Index View에서 사용할 모델로 전달한다.
            return View(routines);
        }
    }
}
