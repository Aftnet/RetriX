using LibRetriX;

namespace RetriX.Shared.Services
{
    public interface IInputService : IInitializable
    {
        void InjectInputPlayer1(InputTypes inputType);
        void PollInput();
        short GetInputState(uint port, InputTypes inputType);
    }
}