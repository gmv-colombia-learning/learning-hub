using VirtualBuddy.Application.AI.UseCases;

namespace VirtualBuddy.Application.AI
{
    public class AIFacade
    {
        public ChatWithProject Chat { get; }
        public IndexDocument Index { get; }

        public AIFacade(ChatWithProject chat, IndexDocument index)
        {
            Chat = chat;
            Index = index;
        }
    }
}
