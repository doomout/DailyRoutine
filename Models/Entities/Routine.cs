namespace DailyRoutine.Models.Entities
{
    /// <summary>
    /// 독서, 아침약처럼 반복해서 수행할 루틴의 기본 정보를 나타냅니다.
    /// 현재는 일반 C# 클래스이며, 이후 DbContext의 모델에 등록하여 DB 테이블과 연결합니다.
    /// 날짜별 완료 여부는 루틴 자체의 정보가 아니므로 이 클래스에 저장하지 않습니다.
    /// </summary>
    public class Routine
    {
        /// <summary>
        /// 루틴을 구분하는 식별자입니다. 이름이 같더라도 Id로 서로 다른 루틴을 구분합니다.
        /// EF Core는 관례상 Id 속성을 기본 키로 인식합니다.
        /// 이후 MySQL 매핑에서는 정수 키의 자동 증가를 사용하고, 등록할 때 직접 지정하지 않습니다.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 화면에 표시할 루틴 이름입니다. 예: 아침약, 독서.
        /// required는 C# 코드에서 객체를 생성할 때 이 속성을 지정하도록 요구합니다.
        /// string은 null을 허용하지 않는다는 의도이며, 빈 문자열이나 공백까지 막지는 않습니다.
        /// 이름의 길이 제한과 실제 입력값 검증은 이후 매핑 및 등록 기능 단계에서 추가합니다.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 루틴에 대한 선택적 설명입니다. 예: 잠들기 전에 20분 읽기.
        /// string?의 물음표는 설명을 입력하지 않았을 때 null을 허용한다는 뜻입니다.
        /// EF Core도 nullable 참조 형식 설정을 바탕으로 선택적 속성으로 해석합니다.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 이 루틴을 수행하기 시작하는 날짜이며, 해당 날짜도 수행 대상에 포함합니다.
        /// 시각이 필요하지 않으므로 DateTime 대신 날짜만 표현하는 DateOnly를 사용합니다.
        /// 서버의 로컬 날짜를 기본값으로 넣지 않고, 생성하는 쪽에서 명시적으로 지정합니다.
        /// required는 값의 지정을 요구할 뿐, 유효한 업무 날짜인지 검사하지는 않습니다.
        /// </summary>
        public required DateOnly StartDate { get; set; }

        /// <summary>
        /// 마지막으로 수행할 날짜이며, 해당 날짜까지 수행 대상에 포함합니다.
        /// DateOnly?는 null을 허용하는 값 형식으로, null이면 종료일 없이 계속하는 루틴입니다.
        /// 종료일이 시작일보다 빠르지 않은지 확인하는 규칙은 이후 처리 로직에서 검증합니다.
        /// </summary>
        public DateOnly? EndDate { get; set; }

        /// <summary>
        /// 루틴 객체를 생성한 시각입니다. 서버 위치에 영향을 받지 않도록 UTC를 사용합니다.
        /// 이 초기값은 new Routine으로 객체를 만들 때 C#에서 설정하며, DB 기본값이 아닙니다.
        /// DB 저장 시각과 정확히 같다는 의미는 아니며, 저장 시각을 기준으로 삼으려면 이후 별도로 설정합니다.
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 마지막으로 수정한 시각입니다. 아직 수정하지 않은 루틴은 null로 구분합니다.
        /// EF Core가 이름을 보고 자동 갱신하지 않으므로, 이후 수정 처리에서 UTC 시각을 대입합니다.
        /// </summary>
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
