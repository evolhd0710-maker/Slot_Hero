using System;
using System.Globalization;

namespace SlotHero.Settings
{
    /// <summary>
    /// 설정 항목 하나가 값을 다루는 방식.
    /// 인게임 화면 기획서 v0.2 / 07 설정 화면 은 항목 이름과 현재 값만 정했으므로
    /// 값을 어떻게 고르는지는 이 두 가지로 나눠 두었다.
    /// </summary>
    public enum SettingKind
    {
        /// <summary>정해진 선택지 중 하나를 고른다. 켜고 끄는 항목도 선택지 두 개로 둔다.</summary>
        Choice = 0,

        /// <summary>
        /// 일정한 폭으로 오르내리는 수치. 소리 크기 같은 것이다.
        /// 칸 가운데에 끌어 움직이는 막대가 놓인다.
        /// </summary>
        Slider = 1,

        /// <summary>
        /// 정해진 선택지 중 하나를 펼친 목록에서 고른다. 언어가 이것이다.
        /// 원재가 2026년 10월 5일에 "누를 때마다 다음으로 넘어가지 말고 드롭다운으로" 라고 했다.
        /// 값은 <see cref="Choice"/> 와 같이 몇 번째를 골랐는지다.
        /// </summary>
        Dropdown = 2,
    }

    /// <summary>
    /// 설정 항목 하나의 정의.
    /// 무엇을 보여 주고 어떤 값을 가질 수 있는지를 담는다.
    /// 지금 값은 <see cref="SettingsValues"/>가 따로 들고 있다.
    ///
    /// 값은 모두 실수 하나로 다룬다.
    /// 선택은 몇 번째를 골랐는지, 수치는 그 값 자체다.
    /// 저장에 그대로 실으려면 형태가 하나여야 하기 때문이다.
    /// </summary>
    [Serializable]
    public class SettingDefinition
    {
        /// <summary>저장과 조회에 쓰는 식별자.</summary>
        public string Id;

        /// <summary>항목 이름. 칸 왼쪽에 적는다.</summary>
        public string DisplayName;

        /// <summary>값을 다루는 방식.</summary>
        public SettingKind Kind;

        /// <summary>선택지. 고르는 항목에만 쓴다. 켜고 끄는 항목은 둘만 넣는다.</summary>
        public string[] Options;

        /// <summary>처음 값과 기본값 복원으로 돌아갈 값.</summary>
        public float DefaultValue;

        /// <summary>수치 항목의 가장 작은 값.</summary>
        public float Min;

        /// <summary>수치 항목의 가장 큰 값.</summary>
        public float Max = 1f;

        /// <summary>수치 항목이 한 번에 오르내리는 폭.</summary>
        public float Step = 0.1f;

        /// <summary>수치 항목을 적는 꼴. {0} 자리에 값이 들어간다.</summary>
        public string ValueFormat = "{0:0}";

        /// <summary>
        /// 음소거 값의 식별자. 비어 있으면 음소거 버튼이 없다.
        /// 소리 크기 항목이 쓴다. 크기와 따로 저장해 음소거를 풀면 앞의 크기로 돌아간다.
        /// </summary>
        public string MuteId = string.Empty;

        /// <summary>
        /// 기본값 복원을 눌러도 그대로 두는 항목인지.
        ///
        /// 해상도와 창 모드처럼 기기와 화면에 맞춰 고른 값, 그리고 언어가 그렇다.
        /// 되돌리면 화면이나 글이 갑자기 바뀌어 플레이어가 다시 맞춰야 한다.
        /// 2026년 10월 8일 원재가 "해상도 같은 일부는 변경되지 않아야 한다" 고 정했고, 같은 날 언어도 뺐다.
        /// 처음 값은 이 항목도 <see cref="DefaultValue"/> 다.
        /// </summary>
        public bool KeepOnRestore;

        // 음소거 값의 정의. 화면이 같은 항목인지 견줄 때 같은 객체여야 하므로 한 번 만들어 둔다.
        [NonSerialized] private SettingDefinition _mute;

        public SettingDefinition()
        {
        }

        /// <summary>음소거 버튼이 있는 항목인지.</summary>
        public bool HasMute
        {
            get { return !string.IsNullOrEmpty(MuteId); }
        }

        /// <summary>
        /// 음소거 값의 정의. 켜고 끄는 항목이다. 음소거 버튼이 없으면 null 이다.
        /// 목록 화면에 줄로 나오지 않고 그 소리 크기 줄의 버튼으로만 나온다.
        /// </summary>
        public SettingDefinition GetMuteDefinition()
        {
            if (!HasMute)
            {
                return null;
            }

            if (_mute == null || _mute.Id != MuteId)
            {
                _mute = Toggle(MuteId, DisplayName + " 음소거", false, "꺼짐", "켜짐");
            }

            // 음소거는 제 소리 크기 항목을 따른다. 크기만 되돌리고 음소거는 남기면 어색하다.
            _mute.KeepOnRestore = KeepOnRestore;

            return _mute;
        }

        /// <summary>선택지 중 하나를 고르는 항목을 만든다.</summary>
        public static SettingDefinition Choice(
            string id,
            string displayName,
            int defaultIndex,
            params string[] options)
        {
            SettingDefinition definition = new SettingDefinition();
            definition.Id = id;
            definition.DisplayName = displayName;
            definition.Kind = SettingKind.Choice;
            definition.Options = options;
            definition.DefaultValue = defaultIndex;
            definition.Min = 0f;
            definition.Max = options != null && options.Length > 0 ? options.Length - 1 : 0f;
            definition.Step = 1f;
            return definition;
        }

        /// <summary>켜고 끄는 항목을 만든다. 선택지가 둘인 고르는 항목이다.</summary>
        public static SettingDefinition Toggle(
            string id,
            string displayName,
            bool defaultOn,
            string offLabel,
            string onLabel)
        {
            return Choice(id, displayName, defaultOn ? 1 : 0, offLabel, onLabel);
        }

        /// <summary>선택지 중 하나를 펼친 목록에서 고르는 항목을 만든다.</summary>
        public static SettingDefinition Dropdown(
            string id,
            string displayName,
            int defaultIndex,
            params string[] options)
        {
            SettingDefinition definition = Choice(id, displayName, defaultIndex, options);
            definition.Kind = SettingKind.Dropdown;
            return definition;
        }

        /// <summary>
        /// 소리 크기 항목을 만든다. 0 ~ 100 을 1 씩 움직이고 오른쪽에 음소거 버튼이 붙는다.
        /// 음소거 값은 `id` 뒤에 `Mute` 를 붙인 식별자로 따로 저장한다.
        /// </summary>
        public static SettingDefinition Volume(string id, string displayName, float defaultValue)
        {
            SettingDefinition definition = Slider(id, displayName, defaultValue, 0f, 100f, 1f, "{0:0}");
            definition.MuteId = id + "Mute";
            return definition;
        }

        /// <summary>오르내리는 수치 항목을 만든다.</summary>
        public static SettingDefinition Slider(
            string id,
            string displayName,
            float defaultValue,
            float min,
            float max,
            float step,
            string valueFormat)
        {
            SettingDefinition definition = new SettingDefinition();
            definition.Id = id;
            definition.DisplayName = displayName;
            definition.Kind = SettingKind.Slider;
            definition.DefaultValue = defaultValue;
            definition.Min = min;
            definition.Max = max;
            definition.Step = step;
            definition.ValueFormat = valueFormat;
            return definition;
        }

        /// <summary>기본값 복원을 눌러도 그대로 두게 표시하고 자신을 돌려준다. 목록을 적을 때 이어 쓰려는 것이다.</summary>
        public SettingDefinition KeptOnRestore()
        {
            KeepOnRestore = true;
            return this;
        }

        /// <summary>선택지 중 하나를 고르는 항목인지. 드롭다운도 값은 고르는 항목과 같다.</summary>
        public bool IsChoice
        {
            get { return Kind == SettingKind.Choice || Kind == SettingKind.Dropdown; }
        }

        /// <summary>선택지 수.</summary>
        public int OptionCount
        {
            get { return Options != null ? Options.Length : 0; }
        }

        /// <summary>값을 가질 수 있는 범위 안으로 자른다.</summary>
        public float Clamp(float value)
        {
            if (IsChoice)
            {
                int count = OptionCount;
                if (count <= 0)
                {
                    return 0f;
                }

                int index = (int)value;
                if (index < 0)
                {
                    return 0f;
                }

                return index > count - 1 ? count - 1 : index;
            }

            if (value < Min)
            {
                return Min;
            }

            return value > Max ? Max : value;
        }

        /// <summary>
        /// 다음 값으로 넘긴다.
        /// 고르는 항목은 마지막 다음에 처음으로 돌아가고,
        /// 수치 항목은 한 폭만큼 오르되 가장 큰 값에서 멈춘다.
        /// </summary>
        public float Next(float value)
        {
            if (IsChoice)
            {
                int count = OptionCount;
                if (count <= 1)
                {
                    return Clamp(value);
                }

                int index = (int)Clamp(value);
                return (index + 1) % count;
            }

            return Clamp(value + Step);
        }

        /// <summary>앞 값으로 되돌린다. 고르는 항목은 처음에서 마지막으로 돌아간다.</summary>
        public float Previous(float value)
        {
            if (IsChoice)
            {
                int count = OptionCount;
                if (count <= 1)
                {
                    return Clamp(value);
                }

                int index = (int)Clamp(value);
                return index <= 0 ? count - 1 : index - 1;
            }

            return Clamp(value - Step);
        }

        /// <summary>칸 오른쪽에 적을 글.</summary>
        public string FormatValue(float value)
        {
            float clamped = Clamp(value);

            if (IsChoice)
            {
                int index = (int)clamped;
                if (Options == null || index < 0 || index >= Options.Length)
                {
                    return string.Empty;
                }

                return Options[index];
            }

            string format = string.IsNullOrEmpty(ValueFormat) ? "{0:0}" : ValueFormat;
            return string.Format(CultureInfo.InvariantCulture, format, clamped);
        }
    }
}
