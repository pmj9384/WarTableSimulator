using UnityEngine;

// 판의 사회자 — 배치→전투→결과 진행의 손잡이. 국면 상태 자체는 템플릿 GameManager.GameState를 재사용한다
// (상태 기계를 두 벌 두지 않기 — 09-03 결정. 코스모 관례와의 차이는 템플릿 경계 때문: 게임 고유 흐름은 템플릿 파일 밖에).
public class MatchManager : InGameManager
{
    public void StartBattle()
    {
        GameManager.SetGameState(GameManager.GameState.GamePlay);
        GameManager.Battle.StartBattle();
    }

    // BattleManager가 전멸을 알리면 호출된다 — 결과 UI는 3주차 아웃게임 연결 때
    public void EndBattle(int winnerTeam)
    {
        GameManager.Battle.StopBattle();
        GameManager.SetGameState(winnerTeam == 0 ? GameManager.GameState.GameClear
                                                 : GameManager.GameState.GameOver);
        Debug.Log($"[Match] 전투 종료 — 승리 팀: {winnerTeam}");
    }
}
