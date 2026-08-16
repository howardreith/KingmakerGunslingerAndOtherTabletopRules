using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;

namespace KingmakerGunslinger.UrbanBarbarian
{
    public sealed class UnitPartControlledRageSelection : UnitPart
    {
        [JsonProperty]
        private string _state;

        internal ControlledRageAllocation SelectionFor(ControlledRageTier tier)
        {
            return Read().SelectionFor(tier);
        }

        internal void Unlock(ControlledRageTier tier)
        {
            ControlledRageSelectionState state = Read();
            state.Unlock(tier);
            Write(state);
        }

        internal bool TrySelect(ControlledRageTier tier,
            ControlledRageAllocation allocation, bool rageActive)
        {
            ControlledRageSelectionState state = Read();
            bool selected = state.TrySelectExact(tier, allocation, rageActive);
            if (selected) Write(state);
            return selected;
        }

        public override void PreSave()
        {
            Write(Read());
            base.PreSave();
        }

        public override void PostLoad()
        {
            base.PostLoad();
            Write(Read());
        }

        private ControlledRageSelectionState Read()
        {
            return string.IsNullOrWhiteSpace(_state)
                ? new ControlledRageSelectionState()
                : ControlledRageSelectionState.Parse(_state);
        }

        private void Write(ControlledRageSelectionState state)
        {
            _state = state.Serialize();
        }
    }

    public sealed class ControlledRageSelectionController :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public int Tier;

        public override void OnTurnOn()
        {
            ControlledRageRuntime.UnlockTier(Owner, (ControlledRageTier)Tier);
        }

        public override void OnTurnOff()
        {
            ControlledRageRuntime.RemoveTier(Owner,
                (ControlledRageTier)Tier);
        }
    }
}
