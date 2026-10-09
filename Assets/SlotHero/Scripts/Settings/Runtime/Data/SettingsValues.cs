using System;
using System.Collections.Generic;

namespace SlotHero.Settings
{
    /// <summary>값 하나. 식별자와 값만 담아 저장에 그대로 실린다.</summary>
    [Serializable]
    public struct SettingEntry
    {
        public string Id;
        public float Value;

        public SettingEntry(string id, float value)
        {
            Id = id;
            Value = value;
        }
    }

    /// <summary>
    /// 지금 설정 값.
    /// 저장 시스템 기획서의 "설정은 기기당 1"에 해당하므로 프로필과 따로 저장된다.
    ///
    /// 값을 실수 하나로만 담는다.
    /// 고르는 항목은 몇 번째를 골랐는지, 수치 항목은 그 값 자체다.
    /// 사전 대신 목록을 쓰는 것은 JsonUtility 가 사전을 직렬화하지 못하기 때문이다.
    /// </summary>
    [Serializable]
    public class SettingsValues
    {
        public List<SettingEntry> Entries = new List<SettingEntry>();

        /// <summary>그 항목의 값. 아직 없으면 기본값을 돌려준다.</summary>
        public float Get(SettingDefinition definition)
        {
            if (definition == null)
            {
                return 0f;
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].Id == definition.Id)
                {
                    return definition.Clamp(Entries[i].Value);
                }
            }

            return definition.Clamp(definition.DefaultValue);
        }

        /// <summary>식별자로 값을 찾는다. 없으면 fallback 을 돌려준다.</summary>
        public float GetRaw(string id, float fallback)
        {
            if (string.IsNullOrEmpty(id))
            {
                return fallback;
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].Id == id)
                {
                    return Entries[i].Value;
                }
            }

            return fallback;
        }

        /// <summary>켜고 끄는 항목을 참과 거짓으로 읽는다.</summary>
        public bool GetBool(string id, bool fallback)
        {
            return GetRaw(id, fallback ? 1f : 0f) >= 0.5f;
        }

        /// <summary>고른 자리를 정수로 읽는다.</summary>
        public int GetIndex(string id, int fallback)
        {
            return (int)GetRaw(id, fallback);
        }

        /// <summary>
        /// 값을 적는다. 값이 실제로 바뀌었으면 true 를 돌려준다.
        /// 범위를 넘으면 잘라서 담는다.
        /// </summary>
        public bool Set(SettingDefinition definition, float value)
        {
            if (definition == null || string.IsNullOrEmpty(definition.Id))
            {
                return false;
            }

            float clamped = definition.Clamp(value);

            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].Id != definition.Id)
                {
                    continue;
                }

                if (Entries[i].Value == clamped)
                {
                    return false;
                }

                Entries[i] = new SettingEntry(definition.Id, clamped);
                return true;
            }

            Entries.Add(new SettingEntry(definition.Id, clamped));
            return true;
        }

        /// <summary>
        /// 식별자로 값을 그대로 적는다. 범위는 자르지 않는다. 없으면 더한다.
        /// 저장 쪽이 다시 읽은 설정 위에 바꾼 항목만 옮겨 얹을 때 쓴다. 정의가 없어도 쓸 수 있다.
        /// </summary>
        public void SetRaw(string id, float value)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].Id == id)
                {
                    Entries[i] = new SettingEntry(id, value);
                    return;
                }
            }

            Entries.Add(new SettingEntry(id, value));
        }

        /// <summary>그 항목을 기본값으로 되돌린다.</summary>
        public bool RestoreDefault(SettingDefinition definition)
        {
            return definition != null && Set(definition, definition.DefaultValue);
        }

        /// <summary>
        /// 여러 항목을 기본값으로 되돌린다. 기본값 복원 버튼이 쓴다.
        /// `KeepOnRestore` 가 켜진 항목은 건너뛴다. 해상도와 창 모드가 그렇다.
        /// 값이 실제로 바뀐 항목만 changed 에 담는다. 비워 두면 담지 않는다.
        /// </summary>
        public void RestoreDefaults(List<SettingDefinition> definitions, List<SettingDefinition> changed)
        {
            if (definitions == null)
            {
                return;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                SettingDefinition definition = definitions[i];
                if (definition == null || definition.KeepOnRestore)
                {
                    continue;
                }

                if (RestoreDefault(definition) && changed != null)
                {
                    changed.Add(definition);
                }
            }
        }

        /// <summary>담긴 값이 없는 항목을 기본값으로 채운다.</summary>
        public void FillMissing(List<SettingDefinition> definitions)
        {
            if (definitions == null)
            {
                return;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                SettingDefinition definition = definitions[i];
                if (definition == null || string.IsNullOrEmpty(definition.Id))
                {
                    continue;
                }

                if (!Has(definition.Id))
                {
                    Entries.Add(new SettingEntry(definition.Id, definition.Clamp(definition.DefaultValue)));
                }
            }
        }

        /// <summary>그 항목의 값이 담겨 있는지.</summary>
        public bool Has(string id)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>정의에 없는 값을 버린다. 항목이 사라진 뒤 저장이 남아 있을 때 쓴다.</summary>
        public void DropUnknown(List<SettingDefinition> definitions)
        {
            if (definitions == null)
            {
                return;
            }

            for (int i = Entries.Count - 1; i >= 0; i--)
            {
                bool known = false;
                for (int j = 0; j < definitions.Count; j++)
                {
                    if (definitions[j] != null && definitions[j].Id == Entries[i].Id)
                    {
                        known = true;
                        break;
                    }
                }

                if (!known)
                {
                    Entries.RemoveAt(i);
                }
            }
        }
    }
}
