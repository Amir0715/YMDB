using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
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
            this._tcs = new TaskCompletionSource<bool>();
            this._ct = new CancellationTokenSource(timeout);
            this._ct.Token.Register((Action) (() => this._tcs.TrySetResult(true)));
            this._timeout = timeout;
            this._message = message;
            this._user = user;
            this._deletion = deletion;
            this._behaviour = behaviour;
            this._emojis = emojis;
            this._pages = new List<Page>();
            foreach (Page page in pages)
                this._pages.Add(page);
        }

        public void SetEnumerator(IEnumerator<Page> enumerator)
        {
            this._enumerator ??= enumerator;
        }

        public async Task<Page> GetPageAsync()
        {
            await Task.Yield();
            return this._pages[this.index];
        }

        public async Task SkipLeftAsync()
        {
            await Task.Yield();
            this.index = 0;
        }

        public async Task SkipRightAsync()
        {
            await Task.Yield();
            this.index = this._pages.Count - 1;
        }

        public async Task NextPageAsync()
        {
            if (this._enumerator.MoveNext())
                this._pages.Add(this._enumerator.Current);
            await Task.Yield();
            switch (this._behaviour)
            {
                case PaginationBehaviour.WrapAround:
                    if (this.index == this._pages.Count - 1)
                    {
                        this.index = 0;
                        break;
                    }

                    ++this.index;
                    break;
                case PaginationBehaviour.Ignore:
                    if (this.index == this._pages.Count - 1)
                        break;
                    ++this.index;
                    break;
            }
        }

        public async Task PreviousPageAsync()
        {
            await Task.Yield();
            switch (this._behaviour)
            {
                case PaginationBehaviour.WrapAround:
                    if (this.index == 0)
                    {
                        this.index = this._pages.Count - 1;
                        break;
                    }

                    --this.index;
                    break;
                case PaginationBehaviour.Ignore:
                    if (this.index == 0)
                        break;
                    --this.index;
                    break;
            }
        }

        public async Task<PaginationEmojis> GetEmojisAsync()
        {
            await Task.Yield();
            return this._emojis;
        }

        public async Task<DiscordMessage> GetMessageAsync()
        {
            await Task.Yield();
            return this._message;
        }

        public async Task<DiscordUser> GetUserAsync()
        {
            await Task.Yield();
            return this._user;
        }

        public async Task DoCleanupAsync()
        {
            switch (this._deletion)
            {
                case PaginationDeletion.DeleteEmojis:
                    await this._message.DeleteAllReactionsAsync().ConfigureAwait(false);
                    break;
                case PaginationDeletion.DeleteMessage:
                    await this._message.DeleteAsync().ConfigureAwait(false);
                    break;
            }
        }

        public async Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync()
        {
            await Task.Yield();
            return this._tcs;
        }


        ~CustomPaginationRequest() => this.Dispose();

        /// <summary>Disposes this PaginationRequest.</summary>
        public void Dispose()
        {
            this._ct.Dispose();
            this._tcs = (TaskCompletionSource<bool>) null;
            this._enumerator.Dispose();
        }
    }
}