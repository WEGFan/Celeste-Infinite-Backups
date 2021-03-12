using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.InfiniteBackups.Utils {
    public class BetterIntSlider : TextMenu.Item {
        public int LastDir { get; private set; }

        public int Min { get; private set; }

        public int Max { get; private set; }

        public string Label;
        public int Index;
        public Action<int> OnValueChange;
        public int PreviousIndex;
        private float sine;
        private float fastMoveTimer;
        public Func<int, string> ValuesFunc;
        public Func<float> ValueWidthFunc;
        private float? valueWidth;

        public BetterIntSlider(string label, Func<int, string> valuesFunc, int min, int max, int value = 0) {
            Label = label;
            Selectable = true;
            Min = min;
            Max = max;
            Index = value < min ? min : value > max ? max : value;
            ValuesFunc = valuesFunc;
            ValueWidthFunc = GetDefaultValueWidth;
        }

        public BetterIntSlider Change(Action<int> action) {
            OnValueChange = action;
            return this;
        }

        public override void Added() {
            Container.InnerContent = TextMenu.InnerContentMode.TwoColumn;
        }

        private int GetIndexDelta() {
            if (fastMoveTimer < 1) {
                return 1;
            }
            if (fastMoveTimer < 3) {
                return 5;
            }
            if (fastMoveTimer < 5) {
                return 10;
            }
            return 25;
        }

        public override void LeftPressed() {
            if (Input.MenuLeft.Repeating) {
                fastMoveTimer += Engine.DeltaTime * 8;
            } else {
                fastMoveTimer = 0;
            }

            if (Index > Min) {
                Audio.Play("event:/ui/main/button_toggle_off");
                PreviousIndex = Index;
                Index -= GetIndexDelta();
                Index = Math.Max(Min, Index); // ensure we stay within bounds
                LastDir = -1;
                ValueWiggler.Start();
                OnValueChange?.Invoke(Index);
            }
        }

        public override void RightPressed() {
            if (Input.MenuRight.Repeating) {
                fastMoveTimer += Engine.DeltaTime * 8;
            } else {
                fastMoveTimer = 0;
            }

            if (Index < Max) {
                Audio.Play("event:/ui/main/button_toggle_on");
                PreviousIndex = Index;
                Index += GetIndexDelta();
                Index = Math.Min(Max, Index); // ensure we stay within bounds
                LastDir = 1;
                ValueWiggler.Start();
                OnValueChange?.Invoke(Index);
            }
        }

        public override void ConfirmPressed() {
            if (Max - Min == 1) {
                if (Index == Min) {
                    Audio.Play("event:/ui/main/button_toggle_on");
                } else {
                    Audio.Play("event:/ui/main/button_toggle_off");
                }
                PreviousIndex = Index;
                LastDir = Index == Min ? 1 : -1;
                Index = Index == Min ? Max : Min;
                ValueWiggler.Start();
                OnValueChange?.Invoke(Index);
            }
        }

        public override void Update() {
            sine += Engine.RawDeltaTime;
        }

        public override float LeftWidth() {
            return ActiveFont.Measure(Label).X + 32f;
        }

        public override float RightWidth() {
            return ValueWidth() + 120f;
        }

        public float ValueWidth() {
            if (!valueWidth.HasValue) {
                valueWidth = ValueWidthFunc();
            }
            return valueWidth.Value;
        }

        private float GetDefaultValueWidth() {
            float width = 0f;
            for (int i = Min; i <= Max; i++) {
                width = Math.Max(width, ActiveFont.Measure(ValuesFunc(i)).X);
            }
            return width;
        }

        public override float Height() {
            return ActiveFont.LineHeight;
        }

        public override void Render(Vector2 position, bool highlighted) {
            float alpha = Container.Alpha;
            Color strokeColor = Color.Black * (alpha * alpha * alpha);
            Color color = Disabled ? Color.DarkSlateGray : (highlighted ? Container.HighlightColor : Color.White) * alpha;
            ActiveFont.DrawOutline(Label, position, new Vector2(0f, 0.5f), Vector2.One, color, 2f, strokeColor);
            if (Max - Min > 0) {
                float rWidth = RightWidth();
                ActiveFont.DrawOutline(ValuesFunc(Index), position + new Vector2(Container.Width - rWidth * 0.5f + LastDir * ValueWiggler.Value * 8f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, color, 2f, strokeColor);

                Vector2 vector = Vector2.UnitX * (float)(highlighted ? Math.Sin(sine * 4f) * 4f : 0f);

                Vector2 position2 = position + new Vector2(Container.Width - rWidth + 40f + (LastDir < 0 ? -ValueWiggler.Value * 8f : 0f), 0f) - (Index > Min ? vector : Vector2.Zero);
                ActiveFont.DrawOutline("<", position2, new Vector2(0.5f, 0.5f), Vector2.One, Index > Min ? color : Color.DarkSlateGray * alpha, 2f, strokeColor);

                position2 = position + new Vector2(Container.Width - 40f + (LastDir > 0 ? ValueWiggler.Value * 8f : 0f), 0f) + (Index < Max ? vector : Vector2.Zero);
                ActiveFont.DrawOutline(">", position2, new Vector2(0.5f, 0.5f), Vector2.One, Index < Max ? color : Color.DarkSlateGray * alpha, 2f, strokeColor);
            }
        }
    }
}