// <copyright file="ImportMarketService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services
{
    using TheIsland.Core.Services.SQL;
    using TheIsland.Core.Services.SQL.Entities;
    using static TheIsland.Core.Helpers.MarketTransactionHelpers;

    public interface IImportMarketService
    {
        Task<bool> ImportAsync();
    }

    public class ImportMarketService : IImportMarketService
    {
        private readonly DualMarketTransactionRepository _dualMarketTransactionRepository;
        private readonly DualWalletRepository _dualWalletRepository;
        private readonly LastReadRepository _lastReadRepository;
        private readonly MarketTransactionRepository _transactionRepository;

        public ImportMarketService(
            DualMarketTransactionRepository dualMarketTransactionRepository,
            DualWalletRepository dualWalletRepository,
            LastReadRepository lastReadRepository,
            MarketTransactionRepository transactionRepository)
        {
            this._dualMarketTransactionRepository = dualMarketTransactionRepository;
            this._dualWalletRepository = dualWalletRepository;
            this._lastReadRepository = lastReadRepository;
            this._transactionRepository = transactionRepository;
        }

        public async Task<bool> ImportAsync()
        {
            // Check LastRead records exist
            await this.CheckLastReadRecordsAsync().ConfigureAwait(false);

            LastRead lastReadWallet = await this._lastReadRepository.GetAsync(nameof(DualWalletTransaction)).ConfigureAwait(false);

            IEnumerable<DualWalletTransaction> walletTransactions = await this._dualWalletRepository.GetAllAfterIdAsync(lastReadWallet.table_id).ConfigureAwait(false);

            if (walletTransactions.Count() == 0)
            {
                return true;
            }

            // map market to our object
            MarketTransaction[] internalMarketTransactionWallet = walletTransactions.Select(transaction => transaction.ToMarketTransaction()).ToArray();

            if (internalMarketTransactionWallet.Count() == 0)
            {
                return true;
            }

            try
            {
                await this._transactionRepository.AddAsync(internalMarketTransactionWallet).ConfigureAwait(false);
                lastReadWallet.table_id = walletTransactions.Max(item => item.id);
                await this._lastReadRepository.UpdateAsync(lastReadWallet).ConfigureAwait(false);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private async Task CheckLastReadRecordsAsync()
        {
            LastRead insertRecord;

            if (await this._lastReadRepository.GetAsync(nameof(DualWalletTransaction)).ConfigureAwait(false) == null)
            {
                insertRecord = new LastRead();
                insertRecord.table_name = nameof(DualWalletTransaction);
                insertRecord.table_id = 0;
                await this._lastReadRepository.AddAsync(insertRecord).ConfigureAwait(false);
            }
        }
    }
}
