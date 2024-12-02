// <copyright file="MarketTransactionHelpers.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Framework.Helpers
{
    using NQ;
    using TheIsland.Data.Entities;

    public static class MarketTransactionHelpers
    {
        public static MarketTransaction ToMarketTransaction(this MarketOrder input)
        {
            MarketTransaction output = new MarketTransaction();
            output.price = input.unitPrice;
            output.type = input.buyQuantity > 0 ? TransactionType.BuyOrder : TransactionType.SellOrder;
            output.quantity = input.buyQuantity;
            output.market_id = input.marketId;
            output.item_id = input.itemType;
            return output;
        }

        public static MarketTransaction ToMarketTransaction(this DualMarketTransaction input)
        {
            MarketTransaction output = new MarketTransaction();
            output.price = input.price;
            output.type = input.original_buy_quantity > 0 ? TransactionType.BuyOrder : TransactionType.SellOrder;
            output.quantity = input.original_buy_quantity;
            output.creation_date = input.creation_date;
            output.market_id = input.market_id;
            output.item_id = input.item_type_id;
            return output;
        }

        public static MarketTransaction ToMarketTransaction(this DualWalletTransaction input)
        {
            MarketTransaction output = new MarketTransaction();

            output.price = input.amount / 100;
            output.type = output.price > 0 ? TransactionType.Sell : TransactionType.Buy;
            output.quantity = input.quantity;
            output.creation_date = input.time;
            output.market_id = input.market_id;
            output.item_id = input.item_id;

            return output;
        }
    }
}
