using System;
using System.Collections.Generic;
using SlotHero.CurrentBuild;
using SlotHero.Sanctum;
using SlotHero.Save;

namespace SlotHero.Flow
{
    /// <summary>
    /// 진행 중인 런 하나를 열고 닫는다. 런의 저장과 복구를 맡는다.
    ///
    /// 새로 시작하면 시작 아이템과 유물 순서를 넣은 뒤 처음 쓰고,
    /// 이어 하면 늘 파일에서 다시 읽어 유물 순서를 저장해 둔 자리에서 되살린다.
    /// 끝내면 완료 기록을 쓰고 런을 놓는다.
    ///
    /// 2026년 10월 8일에 `GameFlowController` 에서 떼어 냈다. 외부 검토가
    /// "저장·복구, 이벤트 선택, 보상 처리 책임을 조금씩 분리하라" 고 권했고 원재가 하라고 했다.
    /// 화면을 여닫는 일은 하지 않는다. 그것은 흐름 조종기에 남는다.
    /// </summary>
    public class RunSession
    {
        private readonly SaveService _save;
        private readonly RunCatalog _catalog;
        private readonly Action _changed;

        private RunContext _context;
        private RunRelicPool _relicPool;

        /// <summary>
        /// 저장 창구와 물건 목록을 물려 만든다.
        /// changed 는 골드나 체력이 바뀔 때마다 부른다. 흐름이 상단 표시줄을 고친다.
        /// </summary>
        public RunSession(SaveService save, RunCatalog catalog, Action changed)
        {
            _save = save;
            _catalog = catalog;
            _changed = changed;
        }

        /// <summary>진행 중인 런. 없으면 null 이다.</summary>
        public RunContext Context
        {
            get { return _context; }
        }

        /// <summary>이 런의 유물 순서. 성소 진열이 쓴다.</summary>
        public RunRelicPool RelicPool
        {
            get { return _relicPool; }
        }

        /// <summary>
        /// 런을 새로 시작한다. 08 저장 시점 의 "런 시작" 이다.
        /// 시작 아이템과 유물 순서를 넣은 뒤에 처음 쓴다. 자동 저장 ① 런 시작은 한 번이다.
        /// 빈 런을 먼저 쓰면 그 사이에 꺼졌을 때 시작 아이템이 없는 런이 남는다.
        /// 시작하지 못하면 null 이다.
        /// </summary>
        public RunSaveData StartNew(int seed)
        {
            if (_save == null || _save.ProfileIndex < 0)
            {
                return null;
            }

            return _save.StartRun(seed, Prepare);
        }

        /// <summary>새 런을 처음 쓰기 전에 채운다. 유물 순서는 쓰는 순간 `StoreRelicPool` 이 런 데이터에 옮겨 담는다.</summary>
        private void Prepare(RunSaveData run)
        {
            Attach(run);
            _catalog.GiveStartingItems(_context);
            _relicPool = RunRelicPool.Build(run.Seed, _catalog);
        }

        /// <summary>
        /// 저장된 런을 다시 연다. 이어 할 런이 없으면 null 이다.
        ///
        /// **이어하기는 늘 파일에서 다시 읽은 런으로 한다.**
        /// 방 안에서 "저장하고 나가기" 를 하고 끄지 않은 채 타이틀에서 바로 이어 하면
        /// 메모리에는 방 안에서 쓴 골드, 채운 체력, 산 물건이 남아 있다. 그것으로 이어 하면
        /// 방에 처음부터 다시 들어가 야영과 구매를 한 번 더 할 수 있고, 껐다 켜서 이어 할 때와 결과가 달라진다.
        /// 2026년 10월 6일 원재가 "저장 후 이어하기를 해도 모든 상황이 같게 나와야 한다" 고 정했다.
        /// </summary>
        public RunSaveData Resume()
        {
            return Resume(true);
        }

        /// <summary>
        /// 저장된 런을 다시 연다. reload 를 끄면 파일에서 다시 읽지 않고 저장 창구가 막 읽어 둔 런을 쓴다.
        /// 흐름이 저장 파일을 기다렸다 막 다시 읽었을 때 끈다. 메타를 백업으로 되살린 뒤에 또 읽으면
        /// 본 파일이 아직 잠겨 있어 다시 실패하고, 기다리기가 끝없이 되풀이된다.
        /// </summary>
        public RunSaveData Resume(bool reload)
        {
            if (_save == null)
            {
                return null;
            }

            if (reload)
            {
                _save.ReloadRun();
            }

            RunSaveData run = _save.Run;
            if (run == null)
            {
                return null;
            }

            Attach(run);

            // 유물 순서는 저장해 둔 자리에서 이어 간다. 처음부터 꺼내면 이미 본 유물이 또 나온다.
            // 이 칸이 생기기 전의 저장 파일이면 비어 있어 처음부터 만든다.
            if (run.RelicPool == null)
            {
                run.RelicPool = new RelicPoolSaveData();
            }

            _relicPool = RunRelicPool.Restore(
                run.Seed, run.RelicPool.TableIndex, run.RelicPool.NextIndex, run.RelicPool.Order, _catalog);

            return run;
        }

        /// <summary>
        /// 런 데이터를 쓰기 직전에 유물 순서를 어디까지 꺼냈는지 옮겨 담는다. 저장 창구의 `RunSaving` 이 부른다.
        ///
        /// 지금 런의 것일 때만 담는다. 런을 새로 시작하는 순간에는 앞 런의 순서가 남아 있을 수 있다.
        /// 성소 안에서 꺼낸 것은 방을 끝낼 때 함께 저장된다. 방 안에서 나가면 방에 들어갈 때의 자리가 남는다.
        /// </summary>
        public void StoreRelicPool(RunSaveData run)
        {
            if (run == null || _relicPool == null || _context == null || _context.Run != run)
            {
                return;
            }

            run.RelicPool.TableIndex = _relicPool.TableIndex;
            run.RelicPool.NextIndex = _relicPool.NextIndex;
            run.RelicPool.Order = new List<string>(_relicPool.Order);
        }

        /// <summary>
        /// 런을 끝낸다. 08 저장 시점 의 "런 종료" 다. 끝난 런을 돌려주고 런을 놓는다.
        ///
        /// 완료 기록을 쓰지 못하면 저장 쪽이 런을 지우지 않고 쓰기 실패 팝업을 띄운다(saved 가 false).
        /// 결과 화면은 그대로 보여 준다. 런 파일이 남아 타이틀에서 그 방부터 이어 할 수 있다.
        /// </summary>
        public RunSaveData End(bool cleared, string today, out CurrentBuildSnapshot finalBuild, out bool saved)
        {
            finalBuild = null;
            saved = false;

            if (_save == null || _context == null)
            {
                return null;
            }

            finalBuild = _context.BuildSnapshot();
            RunSaveData finished = _save.EndRun(cleared, today, out saved);
            Detach();
            return finished;
        }

        /// <summary>런을 놓는다. 저장하고 나가거나 런을 끝냈을 때 부른다. 파일은 건드리지 않는다.</summary>
        public void Detach()
        {
            if (_context != null)
            {
                _context.Changed -= HandleChanged;
                _context = null;
            }

            _relicPool = null;
            _catalog.Attach(null);
        }

        /// <summary>
        /// 런 시드를 만든다.
        /// 시계에서 뽑는다. `UnityEngine.Random` 을 쓰지 않는 것은 같은 시드가 어느 기기에서든
        /// 같은 결과를 내야 하기 때문이고, 시드 자체는 매번 달라야 하므로 시계를 쓴다.
        /// </summary>
        public static int MakeSeed()
        {
            long ticks = DateTime.UtcNow.Ticks;
            return (int)(ticks ^ (ticks >> 32));
        }

        /// <summary>메타에 적는 오늘 날짜. 저장 창구와 같은 꼴(`SaveService.FormatToday`)을 쓴다.</summary>
        public static string Today()
        {
            return SaveService.FormatToday();
        }

        /// <summary>그 런을 붙들어 화면들이 쓰는 꼴(`RunContext`)로 만든다. 앞 런은 놓는다.</summary>
        private void Attach(RunSaveData run)
        {
            Detach();

            _catalog.Attach(run);
            _context = new RunContext(run, _catalog);
            _context.Changed += HandleChanged;
        }

        private void HandleChanged()
        {
            if (_changed != null)
            {
                _changed();
            }
        }
    }
}
