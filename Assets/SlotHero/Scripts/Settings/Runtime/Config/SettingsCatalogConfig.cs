using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Settings
{
    /// <summary>
    /// 분류 탭 하나와 그 안의 항목.
    /// 인게임 화면 기획서 v0.2 / 07 설정 화면 의 "설정을 여러 탭으로 구분하여 배치한다"에 해당한다.
    /// </summary>
    [Serializable]
    public class SettingsTab
    {
        [Tooltip("탭 식별자.")]
        public string Id;

        [Tooltip("탭에 적는 이름.")]
        public string DisplayName;

        [Tooltip("이 탭에 놓을 항목.")]
        public List<SettingDefinition> Items = new List<SettingDefinition>();

        public SettingsTab()
        {
        }

        public SettingsTab(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        public int ItemCount
        {
            get { return Items != null ? Items.Count : 0; }
        }
    }

    /// <summary>
    /// 설정 화면에 무엇이 들어가는지 적어 둔 것.
    /// 기본값은 인게임 화면 기획서 v0.2 / 07 설정 화면 이 적은
    /// "일반, 그래픽, 사운드, 조작"과 와이어프레임 일반 탭의 네 항목이다.
    ///
    /// 설정 기획서가 따로 없으므로 항목은 와이어프레임에 나온 것만 채워 두었다.
    /// 실제 항목이 정해지면 이 에셋에서 더하고 뺀다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SettingsCatalogConfig",
        menuName = "Slot Hero/설정/설정 목록",
        order = 0)]
    public class SettingsCatalogConfig : ScriptableObject
    {
        [Tooltip("분류 탭. 위에서 아래로 그대로 놓인다.")]
        public List<SettingsTab> Tabs = new List<SettingsTab>
        {
            MakeGeneralTab(),
            new SettingsTab("graphics", "그래픽"),
            MakeSoundTab(),
            new SettingsTab("control", "조작"),
        };

        /// <summary>
        /// 일반 탭. 와이어프레임의 네 항목에 2026년 10월 5일 원재의 결정을 반영했다.
        ///   언어는 드롭다운으로 고른다
        ///   창 모드는 창, 전체 화면, 테두리 없는 창 셋이다
        ///   연출 속도는 보통, 빠름, 스킵 셋이다
        ///
        /// 창 모드는 앞의 두 선택지 차례가 그대로라 식별자를 두고 셋째만 붙였다.
        /// 연출 속도는 선택지 차례가 바뀌어 **식별자를 새로 지었다.** (`effectSpeed` → `presentationSpeed`)
        /// 예전 식별자를 그대로 두면 저장된 옛 값이 다른 선택지로 읽힌다.
        /// 예전 값은 `SettingsValues.DropUnknown` 이 버리고 새 기본값으로 시작한다.
        ///
        /// **언어와 창 모드는 기본값 복원으로 되돌리지 않는다.** 2026년 10월 8일 원재가 정했다.
        /// 창 모드는 해상도와 같이 화면에 맞춘 값이고, 언어는 되돌리면 읽던 글이 갑자기 바뀐다.
        /// 해상도 항목이 생기면 그것도 `KeptOnRestore` 로 적는다.
        /// </summary>
        private static SettingsTab MakeGeneralTab()
        {
            SettingsTab tab = new SettingsTab("general", "일반");
            tab.Items.Add(SettingDefinition.Dropdown("language", "언어", 0, "한국어", "English")
                .KeptOnRestore());
            tab.Items.Add(SettingDefinition.Choice("screenMode", "창 모드", 1, "창", "전체 화면", "테두리 없는 창")
                .KeptOnRestore());
            tab.Items.Add(SettingDefinition.Toggle("autoSaveNotice", "자동 저장", true, "꺼짐", "켜짐"));
            tab.Items.Add(SettingDefinition.Choice("presentationSpeed", "연출 속도", 0, "보통", "빠름", "스킵"));
            return tab;
        }

        /// <summary>
        /// 사운드 탭. 전체, 배경음, 환경음, 효과음을 0 ~ 100 막대로 고르고 오른쪽에 음소거 버튼을 둔다.
        /// 2026년 10월 5일에 원재가 정했다. 기본값은 기획서에 없어 모두 100 으로 두었다.
        /// </summary>
        private static SettingsTab MakeSoundTab()
        {
            SettingsTab tab = new SettingsTab("sound", "사운드");
            tab.Items.Add(SettingDefinition.Volume("masterVolume", "전체", 100f));
            tab.Items.Add(SettingDefinition.Volume("bgmVolume", "배경음", 100f));
            tab.Items.Add(SettingDefinition.Volume("ambienceVolume", "환경음", 100f));
            tab.Items.Add(SettingDefinition.Volume("sfxVolume", "효과음", 100f));
            return tab;
        }

        /// <summary>탭 수.</summary>
        public int TabCount
        {
            get { return Tabs != null ? Tabs.Count : 0; }
        }

        /// <summary>그 자리의 탭.</summary>
        public SettingsTab GetTab(int index)
        {
            if (Tabs == null || index < 0 || index >= Tabs.Count)
            {
                return null;
            }

            return Tabs[index];
        }

        /// <summary>식별자로 탭을 찾는다.</summary>
        public SettingsTab FindTab(string tabId)
        {
            if (Tabs == null || string.IsNullOrEmpty(tabId))
            {
                return null;
            }

            for (int i = 0; i < Tabs.Count; i++)
            {
                if (Tabs[i] != null && Tabs[i].Id == tabId)
                {
                    return Tabs[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 모든 탭의 항목을 한 줄로 모은다. 기본값 복원과 빠진 값 채우기에 쓴다.
        /// 소리 크기 항목의 음소거 값도 함께 넣는다. 줄로 나오지는 않지만 저장되고 되돌려져야 한다.
        /// </summary>
        public List<SettingDefinition> CollectAllItems()
        {
            List<SettingDefinition> all = new List<SettingDefinition>();

            if (Tabs == null)
            {
                return all;
            }

            for (int i = 0; i < Tabs.Count; i++)
            {
                SettingsTab tab = Tabs[i];
                if (tab == null || tab.Items == null)
                {
                    continue;
                }

                for (int j = 0; j < tab.Items.Count; j++)
                {
                    SettingDefinition item = tab.Items[j];
                    if (item == null)
                    {
                        continue;
                    }

                    all.Add(item);

                    if (item.HasMute)
                    {
                        all.Add(item.GetMuteDefinition());
                    }
                }
            }

            return all;
        }

        /// <summary>기본값 복원을 눌러도 그대로 두는 항목의 이름. 확인 팝업에 적는다.</summary>
        public List<string> CollectKeptOnRestoreNames()
        {
            List<string> names = new List<string>();
            List<SettingDefinition> all = CollectAllItems();

            for (int i = 0; i < all.Count; i++)
            {
                // 음소거는 줄로 나오지 않으므로 이름을 따로 적지 않는다.
                if (all[i].KeepOnRestore && !IsMuteOfAnother(all[i], all))
                {
                    names.Add(all[i].DisplayName);
                }
            }

            return names;
        }

        private static bool IsMuteOfAnother(SettingDefinition item, List<SettingDefinition> all)
        {
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].HasMute && all[i].MuteId == item.Id)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>식별자로 항목을 찾는다. 음소거 값도 찾는다.</summary>
        public SettingDefinition FindItem(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            List<SettingDefinition> all = CollectAllItems();
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].Id == id)
                {
                    return all[i];
                }
            }

            return null;
        }

        /// <summary>같은 식별자를 두 번 쓴 항목이 없는지. 있으면 저장이 엉킨다.</summary>
        public bool HasUniqueIds()
        {
            List<SettingDefinition> all = CollectAllItems();

            for (int i = 0; i < all.Count; i++)
            {
                if (string.IsNullOrEmpty(all[i].Id))
                {
                    return false;
                }

                for (int j = i + 1; j < all.Count; j++)
                {
                    if (all[i].Id == all[j].Id)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
