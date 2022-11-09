using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBot
{
    class ModeratorsPool
    {
        private List<Moderator> _moderators = new List<Moderator>();
        private List<Moderator> _preModerators = new List<Moderator>();

        public ModeratorsPool(Moderator firstModerator)
        {
            _moderators.Add(firstModerator);
        }

        public bool TryAddModerator(string query)
        {
            bool isModeratorId = TryGetModeratorID(query, out int moderatorId);
            bool isPreModerator = TryFindPreModerator(moderatorId, out Moderator moderator);
            bool isModerator = _moderators.Contains(moderator);

            if (!isModeratorId || !isPreModerator || isModerator)
                return false;

            _moderators.Add(moderator);
            _preModerators.Remove(moderator);
            return true;
        }

        public async Task SendAddModeratorQuery(Moderator newModerator)
        {
            if (_moderators.Contains(newModerator) || _preModerators.Contains(newModerator))
                return;

            foreach (var moderator in _moderators)
                await moderator.ReceiveAddModeratorQuery(newModerator);

            _preModerators.Add(newModerator);
        }

        public async Task SendMassage(Message message)
        {
            foreach (var moderator in _moderators)
                await moderator.RecieveUserMessage(message);
        }

        public async Task SendApplyingAddModerator(Update query)
        {
            foreach (var moderator in _moderators)
                await moderator.SendApplyingAddModerator(query, _moderators[_moderators.Count - 1]);
        }

        private bool TryGetModeratorID(string query, out int moderatorId)
        {
            string[] words = query.Split(' ');
            return int.TryParse(words[words.Length - 1], out moderatorId);
        }

        private bool TryFindPreModerator(int moderatorId, out Moderator moderator)
        {
            moderator = null;
            bool isFound = false;

            foreach (var preModerater in _preModerators)
            {
                if (preModerater.Chat.Id == moderatorId)
                {
                    moderator = preModerater;
                    isFound = true;
                    break;
                }
            }

            return isFound;
        }
    }
}
