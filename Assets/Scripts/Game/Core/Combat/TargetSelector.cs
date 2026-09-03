using System.Collections.Generic;

namespace Game.Core.Combat
{
    // "가장 가까운 적 하나"를 고르는 눈 — 이 게임의 유일한 타겟 규칙 (CLAUDE.md 축).
    // 누가 후보 목록에 드는가(정의역, 스펙 7-1)는 호출자(CombatManager) 책임 — 여기는 받은 목록에서 고르기만 한다.
    public static class TargetSelector
    {
        // 반환 = 고른 타겟의 SpawnIndex, 후보가 없으면 -1.
        // 동률이면 SpawnIndex 작은 쪽 — 목록 순서와 무관하게 항상 같은 답이어야 재현성(고정 스텝·멀티)이 선다.
        public static int SelectNearest(float myX, float myZ, IReadOnlyList<TargetInfo> candidates)
        {
            if (candidates == null || candidates.Count == 0)
                return -1;

            int bestSpawn = -1;
            float bestDistSq = float.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                float dx = candidates[i].X - myX;
                float dz = candidates[i].Z - myZ;
                float distSq = dx * dx + dz * dz;   // 대소 비교만 하므로 √ 생략 — 약 80기가 매 틱 도는 계산

                // 더 가까우면 교체, 똑같이 가까우면 스폰 순번 작은 쪽으로 교체
                if (distSq < bestDistSq ||
                    (distSq == bestDistSq && candidates[i].SpawnIndex < bestSpawn))
                {
                    bestDistSq = distSq;
                    bestSpawn = candidates[i].SpawnIndex;
                }
            }

            return bestSpawn;
        }
    }
}
