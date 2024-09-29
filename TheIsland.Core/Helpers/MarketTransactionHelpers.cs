// <copyright file="MarketTransactionHelpers.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Helpers
{
    using Newtonsoft.Json.Linq;
    using NQ;
    using TheIsland.Core.Entities;

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
            JObject json = JObject.Parse(input.payload);
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
