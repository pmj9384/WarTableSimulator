namespace Game.Core.Combat
{
    // 타겟이 될 수 있는 개체의 종류 — 정의역 규칙(스펙 7-1)은 호출자가 이 값으로 거른다
    public enum TargetKind { Unit, Turret, Decoy, Barricade }

    // 타겟 후보 1개의 최소 정보 — 선정에 필요한 것만 담는다 (HP·팀·소속은 선정과 무관하므로 없음)
    public struct TargetInfo
    {
        public int SpawnIndex;   // 자체 부여 스폰 순번 — 동률 타이브레이크 기준 (GetInstanceID는 실행마다 달라 금지, 스펙 정정)
        public float X;
        public float Z;          // 전장은 평면 40×24m — 높이(y)는 판정에 안 쓴다
        public TargetKind Kind;

        public TargetInfo(int spawnIndex, float x, float z, TargetKind kind)
        {
            SpawnIndex = spawnIndex;
            X = x;
            Z = z;
            Kind = kind;
        }
    }
}
