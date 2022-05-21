using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;

namespace YMDB.Bot.CustomPaginationRequests
{
    public class CustomPaginationRequest : IPaginationRequest
    {
        private TaskCompletionSource<bool> _tcs;
        private CancellationTokenSource _ct;
        private TimeSpan _timeout;
        private List<Page> _pages;
        private PaginationBehaviour _behaviour;
        private PaginationDeletion _deletion;
        private DiscordMessage _message;
        private PaginationEmojis _emojis;
        private DiscordUser _user;
        private int index;

        private IEnumerator<Page> _enumerator;

        internal CustomPaginationRequest(
            DiscordMessage message,
            DiscordUser user,
            PaginationBehaviour behaviour,
            PaginationDeletion deletion,
            PaginationEmojis emojis,
            TimeSpan timeout,
            params Page[] pages)
        {
            _tcs = new TaskCompletionSource<bool>();
            _ct = new CancellationTokenSource(timeout);
            _ct.Token.Register((Action) (() => _tcs.TrySetResult(true)));
            _timeout = timeout;
            _message = message;
            _user = user;
            _deletion = deletion;
            _behaviour = behaviour;
            _emojis = emojis;
            _pages = new List<Page>();
            foreach (Page page in pages)
                _pages.Add(page);
        }

        public void SetEnumerator(IEnumerator<Page> enumerator)
        {
            _enumerator ??= enumerator;
        }

        public async Task<Page> GetPageAsync()
        {
            await Task.Yield();
            return _pages[index];
        }

        public async Task SkipLeftAsync()
        {
            await Task.Yield();
            index = 0;
        }

        public async Task SkipRightAsync()
        {
            await Task.Yield();
            index = _pages.Count - 1;
        }

        public async Task NextPageAsync()
        {
            if (_enumerator.MoveNext())
                _pages.Add(_enumerator.Current);
            await Task.Yield();
            switch (_behaviour)
            {
                case PaginationBehaviour.WrapAround:
                    if (index == _pages.Count - 1)
                    {
                        index = 0;
                        break;
                    }

                    ++index;
                    break;
                case PaginationBehaviour.Ignore:
                    if (index == _pages.Count - 1)
                        break;
                    ++index;
                    break;
            }
        }

        public async Task PreviousPageAsync()
        {
            await Task.Yield();
            switch (_behaviour)
            {
                case PaginationBehaviour.WrapAround:
                    if (index == 0)
                    {
                        index = _pages.Count - 1;
                        break;
                    }

                    --index;
                    break;
                case PaginationBehaviour.Ignore:
                    if (index == 0)
                        break;
                    --index;
                    break;
            }
        }

        public async Task<PaginationEmojis> GetEmojisAsync()
        {
            await Task.Yield();
            return _emojis;
        }

        public async Task<DiscordMessage> GetMessageAsync()
        {
            await Task.Yield();
            return _message;
        }

        public async Task<DiscordUser> GetUserAsync()
        {
            await Task.Yield();
            return _user;
        }

        public async Task DoCleanupAsync()
        {
            switch (_deletion)
            {
                case PaginationDeletion.DeleteEmojis:
                    await _message.DeleteAllReactionsAsync().ConfigureAwait(false);
                    break;
                case PaginationDeletion.DeleteMessage:
                    await _message.DeleteAsync().ConfigureAwait(false);
                    break;
            }
        }

        public async Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync()
        {
            await Task.Yield();
            return _tcs;
        }


        ~CustomPaginationRequest() => Dispose();

        /// <summary>Disposes this PaginationRequest.</summary>
        public void Dispose()
        {
            _ct.Dispose();
            _tcs = null;
            _enumerator.Dispose();
        }
    }
}
